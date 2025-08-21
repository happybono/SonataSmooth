using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using Excel = Microsoft.Office.Interop.Excel;
using File = System.IO.File;

namespace SonataSmooth
{
    public partial class FrmMain : Form
    {
        private const string ExcelTitlePlaceholder = "Click here to enter a title for your dataset.";

        private static readonly Regex numberRegex = new Regex(
            @"[+-]?\d+(?:,\d{3})*(?:\.\d+)?(?:[eE][+-]?\d+)?",
            RegexOptions.Compiled | RegexOptions.CultureInvariant
            );

        private static readonly Regex htmlTagRegex = new Regex(
            @"<.*?>",
            RegexOptions.Compiled | RegexOptions.Singleline
            );

        private static readonly Regex clipboardRegex = new Regex(
            @"[+-]?(\d+(,\d{3})*|(?=\.\d))((\.\d+([eE][+-]\d+)?)|)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant
            );

        private CancellationTokenSource _ctsSelectAll1;
        private CancellationTokenSource _ctsSelectAll2;

        private readonly FrmExportSettings settingsForm;
        private FrmAbout aboutForm;
        private int dataCount;
        private int r;
        private int polyOrder;

        public FrmMain()
        {
            InitializeComponent();

            this.KeyPreview = true;

            UpdateListBox1BtnsState(null, EventArgs.Empty);
            UpdateListBox2BtnsState(null, EventArgs.Empty);
            settingsForm = new FrmExportSettings(this);
            aboutForm = null;
        }

        private bool TryParseInputData(out double[] input, out int n)
        {
            n = listBox1.Items.Count;
            input = new double[n];
            for (int i = 0; i < n; i++)
            {
                var item = listBox1.Items[i];
                if (item == null)
                {
                    MessageBox.Show($"The item at index {i} is null.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                string s = item.ToString();
                if (!double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out input[i]))
                {
                    MessageBox.Show($"Failed to convert item at index {i} : \"{s}\"", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return true;
        }

        private bool TryParseParameters(out int r, out int polyOrder, out double sigma)
        {
            r = 0;
            polyOrder = 0;
            sigma = 0;
            if (!int.TryParse(cbxKernelRadius.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out r))
            {
                MessageBox.Show($"Failed to parse kernel radius : \"{cbxKernelRadius.Text}\".", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(cbxPolyOrder.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out polyOrder))
            {
                MessageBox.Show($"Failed to parse polynomial order : \"{cbxPolyOrder.Text}\".", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            sigma = (2.0 * r + 1) / 6.0;
            return true;
        }

        private async void btnCalibrate_Click(object sender, EventArgs e)
        {
            settingsForm.ApplyParameters(cbxKernelRadius.Text, cbxPolyOrder.Text);

            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            if (listBox1.Items.Count == 0)
                return;

            try
            {
                btnCalibrate.Enabled = false;

                if (!TryParseInputData(out double[] input, out int n))
                    return;

                if (!TryParseParameters(out int r, out int polyOrder, out double sigma))
                    return;

                if (!ValidateSmoothingParameters(listBox1.Items.Count, r, polyOrder))
                    return;

                bool useRect = rbtnRect.Checked;
                bool useMed = rbtnMed.Checked;
                bool useAvg = rbtnAvg.Checked;
                bool useSG = rbtnSG.Checked;
                bool useGauss = rbtnGauss.Checked;

                int[] binom;
                try
                {
                    binom = CalcBinomialCoefficients(2 * r + 1);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Binomial coefficient calculation error : {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                double[] gaussCoeffs = null;
                if (useGauss)
                {
                    try
                    {
                        gaussCoeffs = ComputeGaussianCoefficients(2 * r + 1, sigma);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Gaussian coefficient computation error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                var progressReporter = new Progress<int>(pct =>
                {
                    progressBar1.Value = Math.Max(progressBar1.Minimum, Math.Min(progressBar1.Maximum, pct));
                });

                double[] results;
                try
                {
                    results = await Task.Run(() =>
                    {
                        double[] sgCoeffs = null;
                        if (useSG)
                        {
                            try
                            {
                                sgCoeffs = ComputeSavitzkyGolayCoefficients(2 * r + 1, polyOrder);
                            }
                            catch (Exception ex)
                            {
                                throw new InvalidOperationException($"SG coefficient computation failed : {ex.Message}", ex);
                            }
                        }

                        return ParallelEnumerable
                            .Range(0, n)
                            .AsOrdered()
                            .WithDegreeOfParallelism(Environment.ProcessorCount)
                            .Select(i =>
                            {
                                try
                                {
                                    int Mirror(int idx)
                                    {
                                        if (idx < 0) return -idx - 1;
                                        if (idx >= n) return 2 * n - idx - 1;
                                        return idx;
                                    }

                                    if (useRect)
                                    {
                                        double sum = 0;
                                        int cnt = 0;
                                        for (int k = -r; k <= r; k++)
                                        {
                                            int idx = i + k;
                                            if (idx >= 0 && idx < n)
                                            {
                                                sum += input[idx];
                                                cnt++;
                                            }
                                        }
                                        return cnt > 0 ? sum / cnt : 0;
                                    }
                                    else if (useMed)
                                    {
                                        return WeightedMedianAt(input, i, r, binom);
                                    }
                                    else if (useAvg)
                                    {
                                        double sum = 0;
                                        int cs = 0;
                                        for (int k = -r; k <= r; k++)
                                        {
                                            int idx = i + k;
                                            if (idx < 0 || idx >= n) continue;
                                            sum += input[idx] * binom[k + r];
                                            cs += binom[k + r];
                                        }
                                        return cs > 0 ? sum / cs : 0;
                                    }
                                    else if (useSG)
                                    {
                                        double sum = 0;
                                        for (int k = -r; k <= r; k++)
                                        {
                                            int mi = Mirror(i + k);
                                            sum += sgCoeffs[k + r] * input[mi];
                                        }
                                        return sum;
                                    }
                                    else if (useGauss)
                                    {
                                        double sum = 0;
                                        for (int k = -r; k <= r; k++)
                                        {
                                            int mi = Mirror(i + k);
                                            sum += gaussCoeffs[k + r] * input[mi];
                                        }
                                        return sum;
                                    }

                                    return 0.0;
                                }
                                catch
                                {
                                    return 0.0;
                                }
                            })
                            .ToArray();
                    });
                }
                catch (AggregateException aex)
                {
                    MessageBox.Show(
                        $"An error occurred during parallel computation : {aex.Flatten().InnerException?.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error
                    );
                    UpdateListBox2BtnsState(null, EventArgs.Empty);
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An unexpected error occurred during computation : {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    UpdateListBox2BtnsState(null, EventArgs.Empty);
                    return;
                }

                try
                {
                    listBox2.BeginUpdate();
                    listBox2.Items.Clear();

                    await AddItemsInBatches(listBox2, results, progressReporter, 60);
                    listBox2.EndUpdate();
                    lblCnt2.Text = $"Count : {listBox2.Items.Count}";

                    slblCalibratedType.Text = useRect ? "Rectangular Average"
                                         : useMed ? "Weighted Median"
                                         : useAvg ? "Binomial Average"
                                         : useSG ? "Savitzky-Golay Filter"
                                         : useGauss ? "Gaussian Filter"
                                                    : "Unknown";

                    slblKernelRadius.Text = r.ToString();

                    bool showPoly = useSG;
                    toolStripStatusLabel5.Visible =
                    toolStripSeparator2.Visible =
                    slblPolynomialOrder.Visible = showPoly;
                    if (showPoly) slblPolynomialOrder.Text = polyOrder.ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error binding results : {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                btnCalibrate.Enabled = true;
                UpdateListBox1BtnsState(null, EventArgs.Empty);
                UpdateListBox2BtnsState(null, EventArgs.Empty);
            }
            progressBar1.Value = 0;
        }


        private (double[] Rect, double[] Binom, double[] Median, double[] Gauss, double[] SG)
            ApplySmoothing(double[] input, int r, int polyOrder, bool doRect, bool doAvg, bool doMed, bool doGauss, bool doSG)
        {
            int n = input.Length;
            int[] binom = CalcBinomialCoefficients(2 * r + 1);
            double sigma = (2.0 * r + 1) / 6.0;

            // 필터 계수 한 번만 계산
            double[] gaussCoeffs = doGauss ? ComputeGaussianCoefficients(2 * r + 1, sigma) : null;
            double[] sgCoeffs = doSG ? ComputeSavitzkyGolayCoefficients(2 * r + 1, polyOrder) : null;

            var rect = new double[n];
            var binomAvg = new double[n];
            var median = new double[n];
            var gauss = new double[n];
            var sg = new double[n];

            int Mirror(int idx) => idx < 0 ? -idx - 1 : idx >= n ? 2 * n - idx - 1 : idx;

            Parallel.For(0, n, i => {
                double sum; int cnt;

                if (doRect)
                {
                    sum = 0; cnt = 0;
                    for (int k = -r; k <= r; k++)
                    {
                        int idx = i + k;
                        if (idx >= 0 && idx < n) { sum += input[idx]; cnt++; }
                    }
                    rect[i] = cnt > 0 ? sum / cnt : 0.0;
                }

                if (doAvg)
                {
                    sum = 0; cnt = 0;
                    for (int k = -r; k <= r; k++)
                    {
                        int idx = i + k;
                        if (idx >= 0 && idx < n) { sum += input[idx] * binom[k + r]; cnt += binom[k + r]; }
                    }
                    binomAvg[i] = cnt > 0 ? sum / cnt : 0.0;
                }

                if (doMed) median[i] = WeightedMedianAt(input, i, r, binom);

                if (doGauss && gaussCoeffs != null)
                {
                    sum = 0;
                    for (int k = -r; k <= r; k++)
                        sum += gaussCoeffs[k + r] * input[Mirror(i + k)];
                    gauss[i] = sum;
                }

                if (doSG && sgCoeffs != null)
                {
                    sum = 0;
                    for (int k = -r; k <= r; k++)
                        sum += sgCoeffs[k + r] * input[Mirror(i + k)];
                    sg[i] = sum;
                }
            });

            return (rect, binomAvg, median, gauss, sg);
        }



        private static double[] ComputeGaussianCoefficients(int length, double sigma)
        {
            if (length < 1)
                throw new ArgumentException("length must be ≥ 1", nameof(length));
            if (sigma <= 0)
                throw new ArgumentException("sigma must be > 0", nameof(sigma));

            var coeffs = new double[length];
            int w = (length - 1) / 2;
            double twoSigmaSq = 2 * sigma * sigma;
            double sum = 0.0;

            for (int i = 0; i < length; i++)
            {
                int x = i - w;
                coeffs[i] = Math.Exp(-(x * x) / twoSigmaSq);
                sum += coeffs[i];
            }
            if (sum <= 0)
                throw new InvalidOperationException("Gaussian kernel sum is zero or negative.");

            // 정규화
            for (int i = 0; i < length; i++)
                coeffs[i] /= sum;

            return coeffs;
        }


        private static double WeightedMedianAt(double[] data, int center, int w, int[] binom)
        {
            var pairs = new List<(double Value, int Weight)>(2 * w + 1);
            for (int k = -w; k <= w; k++)
            {
                int idx = center + k;
                if (idx < 0 || idx >= data.Length) continue;
                pairs.Add((data[idx], binom[k + w]));
            }
            if (pairs.Count == 0)
                return 0;

            // 값 기준 정렬
            pairs.Sort((a, b) => a.Value.CompareTo(b.Value));

            long totalWeight = pairs.Sum(p => p.Weight);
            long half = totalWeight / 2;
            bool isEvenTotal = (totalWeight % 2 == 0);

            long accum = 0;
            for (int i = 0; i < pairs.Count; i++)
            {
                accum += pairs[i].Weight;

                // 누적 가중치 > 절반 : 기존보다 작은 쪽 넘어섰으면 current 반환
                if (accum > half)
                {
                    return pairs[i].Value;
                }

                // 누적 가중치 == 절반인 순간 : 다음 값과 평균 처리
                if (isEvenTotal && accum == half)
                {
                    // 안전하게 bounds 체크
                    double nextVal = (i + 1 < pairs.Count)
                                     ? pairs[i + 1].Value
                                     : pairs[i].Value;
                    return (pairs[i].Value + nextVal) / 2.0;
                }
            }

            // 모든 가중치 합산 후에도 못 찾았다면 최대값 반환
            return pairs[pairs.Count - 1].Value;
        }



        private static int[] CalcBinomialCoefficients(int length)
        {
            if (length < 1)
                throw new ArgumentException("length must be ≥ 1", nameof(length));

            var c = new int[length];
            c[0] = 1;
            for (int i = 1; i < length; i++)
                c[i] = c[i - 1] * (length - i) / i;
            return c;
        }

        private static double[] ComputeSavitzkyGolayCoefficients(int windowSize, int polyOrder)
        {
            int m = polyOrder;
            int half = windowSize / 2;
            double[,] A = new double[windowSize, m + 1];

            for (int i = -half; i <= half; i++)
                for (int j = 0; j <= m; j++)
                    A[i + half, j] = Math.Pow(i, j);

            var ATA = new double[m + 1, m + 1];
            for (int i = 0; i <= m; i++)
                for (int j = 0; j <= m; j++)
                    for (int k = 0; k < windowSize; k++)
                        ATA[i, j] += A[k, i] * A[k, j];

            var invATA = InvertMatrix(ATA);

            var AT = new double[m + 1, windowSize];
            for (int i = 0; i <= m; i++)
                for (int k = 0; k < windowSize; k++)
                    AT[i, k] = A[k, i];

            var h = new double[windowSize];
            for (int k = 0; k < windowSize; k++)
            {
                double sum = 0;
                for (int j = 0; j <= m; j++)
                    sum += invATA[0, j] * AT[j, k];
                h[k] = sum;
            }

            return h;
        }

        private static double[,] InvertMatrix(double[,] a)
        {
            int n = a.GetLength(0);
            var aug = new double[n, 2 * n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    aug[i, j] = a[i, j];
                aug[i, n + i] = 1;
            }

            for (int i = 0; i < n; i++)
            {
                double pivot = aug[i, i];
                for (int j = 0; j < 2 * n; j++)
                    aug[i, j] /= pivot;

                for (int r = 0; r < n; r++)
                {
                    if (r == i) continue;
                    double factor = aug[r, i];
                    for (int c = 0; c < 2 * n; c++)
                        aug[r, c] -= factor * aug[i, c];
                }
            }

            var inv = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    inv[i, j] = aug[i, j + n];

            return inv;
        }

        private bool ValidateSmoothingParameters(int dataCount, int w, int polyOrder)
        {
            int windowSize = 2 * w + 1;
            bool useSG = rbtnSG != null && rbtnSG.Checked;

            if (windowSize > dataCount)
            {
                MessageBox.Show(
                    $"Kernel radius is too large.\n\n" +
                    $"Window size formula : (2 × radius) + 1\n" +
                    $"Current : (2 × {w}) + 1 = {windowSize}\n" +
                    $"Data count : {dataCount}\n\n" +
                    $"Rule : windowSize ≤ dataCount\n" +
                    $"Result : {windowSize} ≤ {dataCount} → Violation",
                    "Parameter Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return false;
            }

            if (useSG && polyOrder >= windowSize)
            {
                MessageBox.Show(
                    $"Polynomial order must be smaller than the window size.\n\n" +
                    $"Rule : polyOrder < windowSize\n" +
                    $"Result : {polyOrder} ≤ {windowSize} → {(polyOrder < windowSize ? "OK" : "Violation")}\n\n" +
                    $"Tip : windowSize = (2 × radius) + 1",
                    "Parameter Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return false;
            }
            return true;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (double.TryParse(txtVariable.Text, out double value))
            {
                listBox1.Items.Add(value);
                lblCnt1.Text = "Count : " + listBox1.Items.Count;
            }
            else
            {
                txtVariable.Focus();
                txtVariable.SelectAll();
            }

            if (listBox1.Items.Count > 0)
            {
                UpdateListBox1BtnsState(null, EventArgs.Empty);
                UpdateListBox2BtnsState(null, EventArgs.Empty);
            }
            else
            {
                return;
            }
        }

        private void txtVariable_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter && btnAdd.Enabled)
            {
                btnAdd.PerformClick();
                txtVariable.Text = String.Empty;
                e.SuppressKeyPress = true;
            }
        }

        private async void btnSelectAll_Click(object sender, EventArgs e)
        {
            _ctsSelectAll1?.Cancel(); // 이전에 진행 중인 작업 중단
            _ctsSelectAll1 = new CancellationTokenSource();
            var token = _ctsSelectAll1.Token;

            int n = listBox1.Items.Count;
            if (n == 0) return;

            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            listBox1.BeginUpdate();
            listBox1.ClearSelected();

            if (n == 1)
            {
                listBox1.SetSelected(0, true);
                progressBar1.Value = 100;
                listBox1.EndUpdate();
                listBox1.Focus();
                UpdateListBox1BtnsState(null, EventArgs.Empty);
                await Task.Delay(200);
                progressBar1.Value = 0;
                return;
            }

            int reportInterval = Math.Max(1, n / 100);
            int yieldInterval = Math.Max(1, n / 1000);

            try
            {
                for (int i = 0; i < n; i++)
                {
                    if (token.IsCancellationRequested) break;

                    listBox1.SetSelected(i, true);

                    if (i % reportInterval == 0)
                    {
                        double ratio = (i + 1) / (double)n;
                        int pct = (int)Math.Round(ratio * 100.0);
                        progressBar1.Value = Math.Min(100, Math.Max(0, pct));
                    }

                    if (i % yieldInterval == 0)
                        await Task.Yield();
                }
            }
            finally
            {
                listBox1.EndUpdate();
            }

            progressBar1.Value = 100;
            listBox1.Focus();
            UpdateListBox1BtnsState(null, EventArgs.Empty);
            await Task.Delay(200);
            progressBar1.Value = 0;
        }

        private async void btnClear_Click(object sender, EventArgs e)
        {
            int itemCount = listBox1.Items.Count;

            var result = MessageBox.Show($"This will delete all {itemCount} item{(itemCount != 1 ? "s" : "")} from the Initial Dataset listbox.\nThis will also delete all items from the Refined Dataset listbox.\n\nAre you sure you want to proceed?",
                                         "Delete Confirmation",
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {
                return;
            }

            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            listBox1.BeginUpdate();
            listBox1.ClearSelected();
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox1.EndUpdate();

            await Task.Yield();

            lblCnt1.Text = $"Count : {listBox1.Items.Count}";
            lblCnt2.Text = $"Count : {listBox1.Items.Count}";

            progressBar1.Value = 100;
            progressBar1.Refresh();

            await Task.Yield();
            progressBar1.Value = 0;

            UpdateListBox1BtnsState(null, EventArgs.Empty);
            UpdateListBox2BtnsState(null, EventArgs.Empty);

            slblCalibratedType.Text = "--";
            slblKernelRadius.Text = "--";
            toolStripSeparator2.Visible = false;
            toolStripStatusLabel5.Visible = false;
            slblPolynomialOrder.Visible = false;
            slblPolynomialOrder.Text = "--";

            txtDatasetTitle.Text = ExcelTitlePlaceholder;
            txtDatasetTitle.TextAlign = HorizontalAlignment.Center;
            txtDatasetTitle.ForeColor = Color.Gray;
            txtVariable.Text = string.Empty;
            txtVariable.Select();
        }

        public void SetComboValues(string kernelRadius, string polyOrder)
        {
            cbxKernelRadius.Text = kernelRadius;
            cbxPolyOrder.Text = polyOrder;
        }


        private async void btnPaste_Click(object sender, EventArgs e)
        {
            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            btnCalibrate.Enabled = false;

            try
            {
                string text = Clipboard.GetText();
                progressBar1.Value = 10;

                var matches = clipboardRegex.Matches(text)
                                    .Cast<Match>()
                                    .Where(m => !string.IsNullOrEmpty(m.Value))
                                    .ToArray();
                progressBar1.Value = 30;

                double[] values = await Task.Run(() =>
                    matches
                        .AsParallel()
                        .WithDegreeOfParallelism(Environment.ProcessorCount)
                        .Select(m => double.Parse(
                            m.Value,
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture
                        ))
                        .ToArray()
                );
                progressBar1.Value = 70;

                if (values.Length == 0)
                    return;

                listBox1.BeginUpdate();
                listBox1.Items.AddRange(values.Cast<object>().ToArray());
                listBox1.EndUpdate();
                listBox1.TopIndex = listBox1.Items.Count - 1;

                progressBar1.Value = 100;
                lblCnt1.Text = $"Count : {listBox1.Items.Count}";
                await Task.Delay(200);
            }
            finally
            {
                UpdateListBox1BtnsState(null, EventArgs.Empty);
                UpdateListBox2BtnsState(null, EventArgs.Empty);

                progressBar1.Value = 0;
                btnCalibrate.Enabled = true;
            }
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            string[] availableFormats = e.Data.GetFormats();

            if (e.Data.GetDataPresent("Text"))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private async void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            UpdateListBox1BtnsState(null, EventArgs.Empty);
            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            try
            {
                string raw = getDropText(e);
                progressBar1.Value = 10;

                if (string.IsNullOrWhiteSpace(raw))
                    return;

                if (raw.IndexOf("<html", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    raw = await Task.Run(() =>
                        htmlTagRegex.Replace(raw, " ")
                    );
                    progressBar1.Value = 25;

                    if (string.IsNullOrWhiteSpace(raw))
                        return;
                }

                double[] parsed = await Task.Run(() =>
                {
                    var ci = CultureInfo.InvariantCulture;
                    return numberRegex.Matches(raw)
                        .Cast<Match>()
                        .AsParallel()
                        .AsOrdered()
                        .WithDegreeOfParallelism(Environment.ProcessorCount)
                        .Select(m =>
                        {
                            string tok = m.Value.Replace(",", "").Trim();
                            return double.TryParse(tok, NumberStyles.Any, ci, out double d)
                                ? d
                                : double.NaN;
                        })
                        .Where(d => !double.IsNaN(d))
                        .ToArray();
                });
                progressBar1.Value = 60;

                if (parsed.Length == 0)
                    return;

                // 진행률 전달자 : 60 ~ 100 사이 값만 처리하도록 설정
                int baseProgress = 60;
                var progressReporter = new Progress<int>(pct =>
                {
                    int adjustedPct = Math.Max(baseProgress, Math.Min(100, pct));
                    progressBar1.Value = adjustedPct;
                    progressBar1.Refresh();
                });

                await AddItemsInBatches(listBox1, parsed, progressReporter, baseProgress);

                progressBar1.Value = 100;
                lblCnt1.Text = "Count : " + listBox1.Items.Count;
                await Task.Delay(200);
            }
            finally
            {
                bool hasItems = listBox1.Items.Count > 0;
                btnCopy.Enabled = hasItems;
                btnDelete.Enabled = hasItems;

                progressBar1.Value = 0;
                UpdateListBox1BtnsState(null, EventArgs.Empty);
            }
        }

        private async Task AddItemsInBatches(
            ListBox box, double[] items, IProgress<int> progress, int baseProgress)
        {
            const int BatchSize = 1000;
            int total = items.Length;
            int done = 0;

            box.BeginUpdate();

            while (done < total)
            {
                int cnt = Math.Min(BatchSize, total - done);
                object[] chunk = items
                    .Skip(done)
                    .Take(cnt)
                    .Cast<object>()
                    .ToArray();

                box.Items.AddRange(chunk);
                done += cnt;

                // 60 ~ 100% 범위로 진행률 환산
                int pct = baseProgress + (int)(done * (100L - baseProgress) / total);
                progress.Report(pct);

                await Task.Delay(1);
            }

            box.EndUpdate();
            box.TopIndex = box.Items.Count - 1;
        }

        private string getDropText(DragEventArgs e)
        {
            var fmts = e.Data.GetFormats().Cast<string>();
            if (fmts.Contains(DataFormats.UnicodeText))
                return e.Data.GetData(DataFormats.UnicodeText)?.ToString();

            if (fmts.Contains(DataFormats.Text))
            {
                var txt = e.Data.GetData(DataFormats.Text)?.ToString();
                if (!string.IsNullOrWhiteSpace(txt) &&
                    !txt.TrimStart().StartsWith("<html", StringComparison.OrdinalIgnoreCase))
                    return txt;
            }

            if (fmts.Contains("HTML Format"))
                return e.Data.GetData("HTML Format")?.ToString();

            return null;
        }

        private void btnSelClear_Click(object sender, EventArgs e)
        {
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            listBox1.BeginUpdate();
            listBox1.ClearSelected();
            listBox1.EndUpdate();

            progressBar1.Value = 100;
            listBox1.Focus();
            lblCnt1.Text = "Count : " + listBox1.Items.Count;

            Task.Delay(200).ContinueWith(_ => progressBar1.Value = 0, TaskScheduler.FromCurrentSynchronizationContext());
            UpdateListBox2BtnsState(null, EventArgs.Empty);
        }

        private async Task DeleteSelectedItemsPreserveSelection(
            ListBox listBox,
            System.Windows.Forms.ProgressBar progressBar,
            Label countLabel)
        {
            var indicesToRemove = listBox
                .SelectedIndices
                .Cast<int>()
                .OrderByDescending(i => i)
                .ToList();

            int total = indicesToRemove.Count;

            progressBar.Minimum = 0;
            progressBar.Maximum = total;
            progressBar.Value = 0;
            progressBar.Style = ProgressBarStyle.Continuous;

            listBox.BeginUpdate();
            try
            {
                for (int i = 0; i < total; i++)
                {
                    int idx = indicesToRemove[i];
                    listBox.Items.RemoveAt(idx);

                    progressBar.Value = i + 1;

                    // UI 반영 시간 확보
                    await Task.Delay(30);
                }
            }
            finally
            {
                listBox.EndUpdate();
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            int selectedCount = listBox1.SelectedIndices.Count;

            string message = selectedCount == listBox1.Items.Count
                ? $"You are about to delete all {selectedCount} items from the list.\nThis will also delete all items from the Refined Dataset listbox.\n\nAre you sure you want to proceed?"
                : $"You are about to delete {selectedCount} selected item{(selectedCount > 1 ? "s" : "")} from the list.\n\nAre you sure you want to proceed?";

            var result = MessageBox.Show(
                message,
                "Delete Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.No)
                return;

            // 전체 삭제 시
            if (selectedCount == listBox1.Items.Count)
            {
                listBox1.Items.Clear();
                listBox2.Items.Clear();
                lblCnt1.Text = $"Count : {listBox1.Items.Count}";
                lblCnt2.Text = $"Count : {listBox2.Items.Count}";
                progressBar1.Value = 0;
                listBox1.Select();

                txtDatasetTitle.Text = ExcelTitlePlaceholder;
                txtDatasetTitle.TextAlign = HorizontalAlignment.Center;
                txtDatasetTitle.ForeColor = Color.Gray;

                UpdateListBox1BtnsState(null, EventArgs.Empty);
                txtVariable.Select();
                return;
            }

            // 선택 항목 삭제
            await DeleteSelectedItemsPreserveSelection(listBox1, progressBar1, lblCnt1);

            // 삭제 완료 후 남은 항목 개수, 포커스, 버튼 상태 갱신
            lblCnt1.Text = $"Count : {listBox1.Items.Count}";
            listBox1.Select();
            UpdateListBox1BtnsState(null, EventArgs.Empty);
            UpdateListBox2BtnsState(null, EventArgs.Empty);
        }

        private void UpdateListBox1BtnsState(object s, EventArgs e)
        {
            bool hasItems = listBox1.Items.Count > 0;
            bool hasSelection = listBox1.SelectedItems.Count > 0;
            bool canSync = hasSelection && listBox1.Items.Count == listBox2.Items.Count && listBox1.Items.Count > 0;

            btnAdd.Enabled = txtVariable.Text.Length > 0 && double.TryParse(txtVariable.Text, out _);
            btnCopy.Enabled = hasItems;
            btnEdit.Enabled = hasSelection;
            btnCalibrate.Enabled = hasItems;
            btnExport.Enabled = hasItems
                && !string.IsNullOrWhiteSpace(txtDatasetTitle.Text)
                && txtDatasetTitle.Text != ExcelTitlePlaceholder;
            btnDelete.Enabled = hasSelection;
            btnClear.Enabled = hasItems;
            btnSelClear.Enabled = hasSelection;
            btnSelectAll.Enabled = hasItems;
            btnSync1.Enabled = canSync;

            if (!hasItems)
            {
                btnCalibrate.Enabled = false;
                btnDelete.Enabled = false;
                btnClear.Enabled = false;
                btnSelClear.Enabled = false;
                btnSync1.Enabled = false;
                listBox1.ClearSelected();
            }
            else
            {
                btnCalibrate.Enabled = true;
                btnDelete.Enabled = hasSelection;
                btnClear.Enabled = hasItems;
                btnSelClear.Enabled = hasSelection;
                btnSync1.Enabled = canSync;
            }
        }

        private void UpdateListBox2BtnsState(object s, EventArgs e)
        {
            bool hasItems = listBox2.Items.Count > 0;
            bool hasSelection = listBox2.SelectedItems.Count > 0;
            bool canSync = hasSelection && listBox2.Items.Count == listBox1.Items.Count && listBox2.Items.Count > 0;

            btnCopy2.Enabled = hasItems;
            btnClear2.Enabled = hasItems;
            btnSelClear2.Enabled = hasSelection;
            btnSelectAll2.Enabled = hasItems;
            btnSync2.Enabled = canSync;

            if (!hasItems)
            {
                btnCopy2.Enabled = false;
                btnClear2.Enabled = false;
                btnSelClear2.Enabled = false;
                btnSync2.Enabled = false;
                listBox2.ClearSelected();
            }
            else
            {
                btnCopy2.Enabled = true;
                btnClear2.Enabled = true;
                btnSelClear2.Enabled = hasSelection;
                btnSync2.Enabled = canSync;
            }
        }

        private async void btnClear2_Click(object sender, EventArgs e)
        {
            int itemCount = listBox2.Items.Count;

            var result = MessageBox.Show($"This will delete all {itemCount} item{(itemCount != 1 ? "s" : "")} from the Refined Dataset listbox.\n\nAre you sure you want to proceed?",
                                         "Delete Confirmation",
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {
                return;
            }

            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            listBox2.BeginUpdate();
            listBox2.Items.Clear();
            listBox2.ClearSelected();
            listBox2.EndUpdate();

            await Task.Yield();

            progressBar1.Value = 100;
            progressBar1.Refresh();

            lblCnt2.Text = "Count : " + listBox2.Items.Count;
            slblCalibratedType.Text = "--";
            slblKernelRadius.Text = "--";
            toolStripSeparator2.Visible = false;
            toolStripStatusLabel5.Visible = false;
            slblPolynomialOrder.Visible = false;
            slblPolynomialOrder.Text = "--";

            await Task.Yield();
            progressBar1.Value = 0;

            UpdateListBox1BtnsState(null, EventArgs.Empty);
            UpdateListBox2BtnsState(null, EventArgs.Empty);

            listBox2.Focus();
        }

        private async void btnSelectAll2_Click(object sender, EventArgs e)
        {
            _ctsSelectAll2?.Cancel();
            _ctsSelectAll2 = new CancellationTokenSource();
            var token = _ctsSelectAll2.Token;

            int n2 = listBox2.Items.Count;
            if (n2 == 0)
                return;

            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            listBox2.BeginUpdate();
            listBox2.ClearSelected();

            int reportInterval = Math.Max(1, n2 / 100);
            int yieldInterval = Math.Max(1, n2 / 1000);

            try
            {
                for (int i = 0; i < n2; i++)
                {
                    if (token.IsCancellationRequested)
                        break;

                    listBox2.SetSelected(i, true);

                    if (i % reportInterval == 0)
                    {
                        double ratio = (i + 1) / (double)n2;
                        int pct = (int)Math.Round(ratio * 100.0);
                        progressBar1.Value = Math.Min(100, Math.Max(0, pct));
                    }

                    if (i % yieldInterval == 0)
                        await Task.Yield();
                }
            }
            finally
            {
                listBox2.EndUpdate();
            }

            progressBar1.Value = 100;
            await Task.Delay(200).ContinueWith(_ => { });
            UpdateListBox2BtnsState(null, EventArgs.Empty);
            progressBar1.Value = 0;
        }

        private async void btnSelectClear2_Click(object sender, EventArgs e)
        {
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            listBox2.BeginUpdate();
            listBox2.ClearSelected();
            listBox2.EndUpdate();

            listBox2.Focus();
            lblCnt2.Text = "Count : " + listBox2.Items.Count;

            progressBar1.Value = 100;
            await Task.Delay(200);
            UpdateListBox2BtnsState(null, EventArgs.Empty);
            progressBar1.Value = 0;
        }


        private void txtVariable_TextChanged(object sender, EventArgs e)
        {
            btnAdd.Enabled = txtVariable.Text.Length > 0 && double.TryParse(txtVariable.Text, out _);
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateListBox2BtnsState(null, EventArgs.Empty);
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete)
            {
                btnDelete.PerformClick();
                e.SuppressKeyPress = true;
            }

            if (e.KeyData == (Keys.Control | Keys.Delete))
            {
                btnClear.PerformClick();
                e.SuppressKeyPress = true;
            }

            if (e.KeyData == Keys.F2)
            {
                btnEdit.PerformClick();
                e.SuppressKeyPress = true;
            }

            if (e.KeyData == (Keys.Control | Keys.C))
            {
                btnCopy.PerformClick();
                e.SuppressKeyPress = true;
            }

            if (e.KeyData == (Keys.Control | Keys.V))
            {
                btnPaste.PerformClick();
                e.SuppressKeyPress = true;
            }

            if (e.KeyData == (Keys.Control | Keys.A))
            {
                btnSelectAll.PerformClick();
                e.SuppressKeyPress = true;
            }

            if (e.KeyData == Keys.Escape)
            {
                btnSelClear.PerformClick();
                e.SuppressKeyPress = true;
            }

            lblCnt1.Text = "Count : " + listBox1.Items.Count;
        }

        private void listBox2_KeyDown(object sender, KeyEventArgs e)
        {
            {
                if (e.KeyData == (Keys.Control | Keys.Delete))
                {
                    btnClear2.PerformClick();
                    e.SuppressKeyPress = true;
                }

                if (e.KeyData == (Keys.Control | Keys.C))
                {
                    btnCopy2.PerformClick();
                    e.SuppressKeyPress = true;
                }

                if (e.KeyData == (Keys.Control | Keys.A))
                {
                    btnSelectAll2.PerformClick();
                    e.SuppressKeyPress = true;
                }

                if (e.KeyData == Keys.Escape)
                {
                    btnSelClear2.PerformClick();
                    e.SuppressKeyPress = true;
                }

                lblCnt2.Text = "Count : " + listBox2.Items.Count;
            }
        }


        private void btnCopy_Click(object sender, EventArgs e)
        {
            IEnumerable<object> source = listBox1.SelectedItems.Count > 0
                ? listBox1.SelectedItems.Cast<object>()
                : listBox1.Items.Cast<object>();

            var doubles = source
                .Select(item =>
                {
                    if (item is double d)
                        return d;
                    if (item != null && double.TryParse(item.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed))
                        return parsed;
                    return (double?)null;
                })
                .Where(x => x.HasValue)
                .Select(x => x.Value);

            Clipboard.SetText(string.Join(Environment.NewLine, doubles));
        }

        private void btnCopy2_Click(object sender, EventArgs e)
        {
            IEnumerable<object> source = listBox2.SelectedItems.Count > 0
                ? listBox2.SelectedItems.Cast<object>()
                : listBox2.Items.Cast<object>();

            var doubles = source
                .Select(item =>
                {
                    if (item is double d)
                        return d;
                    if (item != null && double.TryParse(item.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed))
                        return parsed;
                    return (double?)null;
                })
                .Where(x => x.HasValue)
                .Select(x => x.Value);

            Clipboard.SetText(string.Join(Environment.NewLine, doubles));
        }

        private void rbtnSG_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnSG.Checked)
            {
                lblPolyOrder.Enabled = true;
                cbxPolyOrder.Enabled = true;
            }
            else
            {
                lblPolyOrder.Enabled = false;
                cbxPolyOrder.Enabled = false;
            }
        }


        private void frmMain_Load(object sender, EventArgs e)
        {
            cbxKernelRadius.SelectedIndex = 3;
            cbxPolyOrder.SelectedIndex = 1;

            txtDatasetTitle.Text = ExcelTitlePlaceholder;
            txtDatasetTitle.ForeColor = Color.Gray;
            txtDatasetTitle.Enter += txtExcelTitle_Enter;
            txtDatasetTitle.Leave += txtExcelTitle_Leave;
            txtDatasetTitle.TextChanged += txtExcelTitle_TextChanged;
            UpdateExportExcelButtonState();

            settingsForm.chbRect.Checked = true;
            settingsForm.chbAvg.Checked = true;
            settingsForm.chbMed.Checked = true;
            settingsForm.chbGauss.Checked = true;
            settingsForm.chbSG.Checked = true;

            settingsForm.rbtnCSV.Checked = true;
            settingsForm.DoAutoSave = true;

            // ComboBox 등 동기화
            settingsForm.cbxKernelRadius.Text = cbxKernelRadius.Text;
            settingsForm.cbxPolyOrder.Text = cbxPolyOrder.Text;

            UpdateListBox1BtnsState(null, EventArgs.Empty);
            UpdateListBox2BtnsState(null, EventArgs.Empty);

            dataCount = listBox1.Items.Count;

            // ComboBox 값으로부터 파싱
            int.TryParse(cbxKernelRadius.Text, out r);
            int.TryParse(cbxPolyOrder.Text, out polyOrder);

            this.KeyPreview = true;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateListBox1BtnsState(null, EventArgs.Empty);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            var frm = new FrmModify();
            frm.ShowDialog();

            frm.textBox1.Select();
        }


        private async Task ExportCsvAsync()
        {
            // UI 초기화
            if (progressBar1.InvokeRequired)
            {
                progressBar1.Invoke(new Action(() =>
                {
                    progressBar1.Style = ProgressBarStyle.Continuous;
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = 100;
                    progressBar1.Value = 0;
                }));
            }
            else
            {
                progressBar1.Style = ProgressBarStyle.Continuous;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = 100;
                progressBar1.Value = 0;
            }

            int r = 2, polyOrder = 2, n = 0;
            bool doRect = false, doAvg = false, doMed = false, doGauss = false, doSG = false;
            string excelTitle = "";
            double[] initialData = null;

            // UI 에서 설정 값 및 데이터 읽기
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    r = int.TryParse(settingsForm.cbxKernelRadius.Text, out var tmpW) ? tmpW : 2;
                    polyOrder = int.TryParse(settingsForm.cbxPolyOrder.Text, out var tmpP) ? tmpP : 2;

                    doRect = settingsForm.chbRect.Checked;
                    doAvg = settingsForm.chbAvg.Checked;
                    doMed = settingsForm.chbMed.Checked;
                    doGauss = settingsForm.chbGauss.Checked;
                    doSG = settingsForm.chbSG.Checked;

                    excelTitle = txtDatasetTitle.Text;

                    initialData = listBox1.Items
                        .Cast<object>()
                        .Select(x => double.TryParse(x?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0.0)
                        .ToArray();

                    n = initialData.Length;
                }));
            }
            else
            {
                r = int.TryParse(settingsForm.cbxKernelRadius.Text, out var tmpW) ? tmpW : 2;
                polyOrder = int.TryParse(settingsForm.cbxPolyOrder.Text, out var tmpP) ? tmpP : 2;

                doRect = settingsForm.chbRect.Checked;
                doAvg = settingsForm.chbAvg.Checked;
                doMed = settingsForm.chbMed.Checked;
                doGauss = settingsForm.chbGauss.Checked;
                doSG = settingsForm.chbSG.Checked;

                excelTitle = txtDatasetTitle.Text;

                initialData = listBox1.Items
                    .Cast<object>()
                    .Select(x => double.TryParse(x?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0.0)
                    .ToArray();

                n = initialData.Length;
            }

            if (!ValidateSmoothingParameters(n, r, polyOrder)) return;

            if (n == 0)
            {
                MessageBox.Show("No data to export.", "Export CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            double sigma = (2.0 * r + 1) / 6.0;
            int[] binom = CalcBinomialCoefficients(2 * r + 1);

            var rectAvg = new double[n];
            var binomAvg = new double[n];
            var binomMed = new double[n];
            var gaussFilt = new double[n];
            var sgFilt = new double[n];

            // 데이터 계산
            await Task.Run(() =>
            {
                // 변수 shadowing 을 방지하기 위해, ApplySmoothing() 은 Loop 밖에서 한 번만 호출하세요.
                var (rectAvgResult, binomAvgResult, medianResult, gaussResult, sgResult) =
                    ApplySmoothing(initialData, r, polyOrder, doRect, doAvg, doMed, doGauss, doSG);

                // 결과를 내보내기 위해 배열에 값을 할당합니다.
                if (doRect) Array.Copy(rectAvgResult, rectAvg, n);
                if (doAvg) Array.Copy(binomAvgResult, binomAvg, n);
                if (doMed) Array.Copy(medianResult, binomMed, n);
                if (doGauss) Array.Copy(gaussResult, gaussFilt, n);
                if (doSG) Array.Copy(sgResult, sgFilt, n);
            });


            // 저장 경로 Dialog
            string basePath = null;
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    using (var dlg = new SaveFileDialog())
                    {
                        dlg.Filter = "CSV files (*.csv)|*.csv";
                        dlg.DefaultExt = "csv";
                        dlg.AddExtension = true;
                        if (dlg.ShowDialog(this) == DialogResult.OK) basePath = dlg.FileName;
                    }
                }));
            }
            else
            {
                using (var dlg = new SaveFileDialog())
                {
                    dlg.Filter = "CSV files (*.csv)|*.csv";
                    dlg.DefaultExt = "csv";
                    dlg.AddExtension = true;
                    if (dlg.ShowDialog(this) == DialogResult.OK) basePath = dlg.FileName;
                }
            }

            if (string.IsNullOrEmpty(basePath)) return;

            // 내보낼 Column 구성
            var columns = new List<(string Header, double[] Data)>
    {
        ("Initial Dataset", initialData)
    };
            if (doRect) columns.Add(("Rectangular Averaging", rectAvg));
            if (doAvg) columns.Add(("Binomial Averaging", binomAvg));
            if (doMed) columns.Add(("Binomial Median Filtering", binomMed));
            if (doGauss) columns.Add(("Gaussian Filtering", gaussFilt));
            if (doSG) columns.Add(("Savitzky–Golay Filtering", sgFilt));

            const int ExcelMaxRows = 1_048_576;
            int headerLines =
                1 + // Title
                1 + // Part info (when splitting)
                1 + // blank
                1 + // "Smoothing Parameters"
                1 + // Kernel Radius
                1 + // Kernel Width
                (doSG ? 1 : 0) + // Polynomial Order
                1 + // blank
                1 + // Generated
                1 + // blank
                1;  // header row

            int maxDataRows = ExcelMaxRows - headerLines;
            int partCount = (n + maxDataRows - 1) / maxDataRows;
            int kernelWidth = 2 * r + 1;

            // 완료 플래그 : 완료 후 늦게 도착하는 Report 를 무시
            bool completed = false;

            // 진행률 리포터 : 완료 후 도착 Callback 무시 + 값 클램프
            IProgress<int> progress = new Progress<int>(percent =>
            {
                if (completed) return;
                int v = Math.Max(0, Math.Min(100, percent));
                progressBar1.Value = v;
            });

            string dir = Path.GetDirectoryName(basePath);
            string nameOnly = Path.GetFileNameWithoutExtension(basePath);
            string ext = Path.GetExtension(basePath);

            try
            {
                for (int part = 0; part < partCount; part++)
                {
                    int startRow = part * maxDataRows;
                    int rowCount = Math.Min(maxDataRows, n - startRow);
                    string path = partCount == 1 ? basePath : Path.Combine(dir, $"{nameOnly}_Part{part + 1}{ext}");

                    const int bufSize = 81920;
                    var encoding = Encoding.UTF8;

                    using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufSize, useAsync: true))
                    using (var sw = new StreamWriter(fs, encoding, bufSize))
                    {
                        // Header
                        await sw.WriteLineAsync(excelTitle);
                        await sw.WriteLineAsync($"Part {part + 1} of {partCount}");
                        await sw.WriteLineAsync(string.Empty);
                        await sw.WriteLineAsync("Smoothing Parameters");
                        await sw.WriteLineAsync($"Kernel Radius : {r}");
                        await sw.WriteLineAsync($"Kernel Width : {kernelWidth}");
                        if (doSG) await sw.WriteLineAsync($"Polynomial Order : {polyOrder}");
                        await sw.WriteLineAsync(string.Empty);
                        await sw.WriteLineAsync($"Generated : {DateTime.Now.ToString("G", CultureInfo.CurrentCulture)}");
                        await sw.WriteLineAsync(string.Empty);

                        // Column headers
                        await sw.WriteLineAsync(string.Join(",", columns.Select(c => c.Header)));

                        // Data
                        int lastReported = -1;
                        for (int i = startRow; i < startRow + rowCount; i++)
                        {
                            string line = string.Join(",", columns.Select(c => c.Data[i].ToString(CultureInfo.InvariantCulture)));
                            await sw.WriteLineAsync(line);

                            int percent = (int)(((double)(i + 1) / n) * 100);
                            if (percent != lastReported)
                            {
                                lastReported = percent;
                                progress.Report(percent);
                            }
                        }
                    }
                }
            }
            finally
            {
                // 완료 표시: 이후 들어오는 Report 무시
                completed = true;

                if (progressBar1.InvokeRequired)
                    progressBar1.Invoke(new Action(() =>
                    {
                        progressBar1.Value = 0;
                        progressBar1.Refresh();
                    }));
                else
                {
                    progressBar1.Value = 0;
                    progressBar1.Refresh();
                }
            }

            // 완료 알림
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show("CSV export completed.", "Export CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }));
            }
            else
            {
                MessageBox.Show("CSV export completed.", "Export CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // 자동 열기
            bool openFile = false;
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    openFile = settingsForm.chbOpenFile != null && settingsForm.chbOpenFile.Checked;
                }));
            }
            else
            {
                openFile = settingsForm.chbOpenFile != null && settingsForm.chbOpenFile.Checked;
            }

            if (openFile)
            {
                try
                {
                    for (int part = 0; part < partCount; part++)
                    {
                        string openPath = partCount == 1 ? basePath : Path.Combine(dir, $"{nameOnly}_Part{part + 1}{ext}");
                        if (File.Exists(openPath))
                        {
                            try
                            {
                                Process.Start(new ProcessStartInfo(openPath) { UseShellExecute = true });
                            }
                            catch (System.ComponentModel.Win32Exception)
                            {
                                Process.Start(new ProcessStartInfo("rundll32.exe", $"shell32.dll,OpenAs_RunDLL \"{openPath}\"") { UseShellExecute = true });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() =>
                        {
                            MessageBox.Show($"Failed to open the file automatically.\n{ex.Message}", "Open File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }));
                    }
                    else
                    {
                        MessageBox.Show($"Failed to open the file automatically.\n{ex.Message}", "Open File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        //private Task WriteCsvOptimizedAsync(
        //    string path,
        //    string title,
        //    int kernelRadius,
        //    int? polyOrder,
        //    List<(string Header, double[] Data)> columns,
        //    int totalRows,
        //    IProgress<int> progress)
        //{
        //    return Task.Run(async () =>
        //    {
        //        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        //        const int bufSize = 1 << 20;
        //        using (var fs = new FileStream(
        //                   path,
        //                   FileMode.Create,
        //                   FileAccess.Write,
        //                   FileShare.None,
        //                   bufSize,
        //                   useAsync: true))
        //        using (var sw = new StreamWriter(fs, encoding, bufSize))
        //        {
        //            await sw.WriteLineAsync(title);
        //            await sw.WriteLineAsync();
        //            await sw.WriteLineAsync("Smoothing Parameters");
        //            await sw.WriteLineAsync($"Kernel Radius,{kernelRadius}");
        //            await sw.WriteLineAsync($"Polynomial Order,{(polyOrder.HasValue ? polyOrder.Value.ToString() : "N/A")}");
        //            await sw.WriteLineAsync();

        //            var headerSb = new StringBuilder(256)
        //                .Append("Index");
        //            foreach (var col in columns)
        //                headerSb.Append(',').Append(col.Header);
        //            await sw.WriteLineAsync(headerSb.ToString());
        //            headerSb.Clear();

        //            var lineSb = new StringBuilder(512);
        //            int reportInterval = Math.Max(1, totalRows / 200);
        //            for (int i = 0; i < totalRows; i++)
        //            {
        //                lineSb.Append(i + 1);
        //                foreach (var col in columns)
        //                {
        //                    lineSb.Append(',');
        //                    lineSb.Append(col.Data[i].ToString("G17", CultureInfo.InvariantCulture));
        //                }
        //                await sw.WriteLineAsync(lineSb.ToString());
        //                lineSb.Clear();

        //                if (i % reportInterval == 0)
        //                    progress.Report((int)(100.0 * i / totalRows));
        //            }
        //            progress.Report(100);

        //            await sw.FlushAsync();
        //        }
        //    });
        //}

        private async void btnExport_Click(object sender, EventArgs e)
        {
            bool doXLSX = settingsForm.rbtnXLSX.Checked;
            bool doCSV = settingsForm.rbtnCSV.Checked;

            if (doCSV)
            {
                await ExportCsvAsync();
                return;
            }

            if (doXLSX)
            {
                ExportExcelAsync();
                return;
            }
        }

        private async void ExportExcelAsync()
        {
            // COM 해제 유틸 (로컬 함수) 
            void FinalRelease(object com)
            {
                try
                {
                    if (com != null && System.Runtime.InteropServices.Marshal.IsComObject(com))
                        System.Runtime.InteropServices.Marshal.FinalReleaseComObject(com);
                }
                catch { /* 필요 시 Logging 기능 추가 */ }
            }

            void ReleaseAll(Stack<object> stack)
            {
                while (stack.Count > 0) FinalRelease(stack.Pop());
            }

            int r = int.TryParse(settingsForm.cbxKernelRadius.Text, out var tmpW) ? tmpW : 2;
            int polyOrder = int.TryParse(settingsForm.cbxPolyOrder.Text, out var tmpP) ? tmpP : 2;

            bool doRect = settingsForm.chbRect.Checked;
            bool doAvg = settingsForm.chbAvg.Checked;
            bool doMed = settingsForm.chbMed.Checked;
            bool doGauss = settingsForm.chbGauss.Checked;
            bool doSG = settingsForm.chbSG.Checked;

            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            var initialData = listBox1.Items
                .Cast<object>()
                .Select(item => double.TryParse(item?.ToString(), out var d) ? d : 0.0)
                .ToArray();

            int n = initialData.Length;

            if (!ValidateSmoothingParameters(n, r, polyOrder))
                return;

            const int maxRows = 1_048_573;
            double sigma = (2.0 * r + 1) / 6.0;
            int[] binom = CalcBinomialCoefficients(2 * r + 1);

            if (n == 0)
            {
                MessageBox.Show(
                    "No data to export.",
                    "Export Excel",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            double[] rectAvg = new double[n];
            double[] binomAvg = new double[n];
            double[] binomMed = new double[n];
            double[] gaussFilt = new double[n];
            double[] sgFilt = new double[n];

            await Task.Run(() =>
            {
                //  변수 shadowing 을 방지하기 위해, ApplySmoothing() 은 Loop 밖에서 한 번만 호출하세요.
                var (rectAvgResult, binomAvgResult, medianResult, gaussResult, sgResult) =
                    ApplySmoothing(initialData, r, polyOrder, doRect, doAvg, doMed, doGauss, doSG);

                // 결과를 내보내기 위해 배열에 값을 할당합니다.
                if (doRect) Array.Copy(rectAvgResult, rectAvg, n);
                if (doAvg) Array.Copy(binomAvgResult, binomAvg, n);
                if (doMed) Array.Copy(medianResult, binomMed, n);
                if (doGauss) Array.Copy(gaussResult, gaussFilt, n);
                if (doSG) Array.Copy(sgResult, sgFilt, n);
            });

            // COM 객체 추적 Stack
            var coms = new Stack<object>();

            Excel.Application excel = null;
            Excel.Workbooks workbooks = null;
            Excel.Workbook wb = null;
            Excel.Sheets sheets = null;
            Excel.Worksheet ws = null;

            Excel.ChartObjects chartObjects = null;
            Excel.ChartObject chartObj = null;
            Excel.Chart chart = null;
            Excel.SeriesCollection seriesCollection = null;

            try
            {
                excel = new Excel.Application();
                coms.Push(excel);

                workbooks = excel.Workbooks;
                coms.Push(workbooks);

                wb = workbooks.Add();
                coms.Push(wb);

                sheets = wb.Worksheets;
                coms.Push(sheets);

                ws = (Excel.Worksheet)sheets[1];
                coms.Push(ws);

                //excel.Visible = true;

                ws.Cells[1, 1] = txtDatasetTitle.Text;
                ws.Cells[3, 1] = "Smoothing Parameters";
                ws.Cells[4, 1] = $"Kernel Radius : {r}";
                ws.Cells[5, 1] = $"Kernel Width : {2 * r + 1}";
                ws.Cells[6, 1] = doSG
                    ? $"Polynomial Order : {polyOrder}"
                    : "Polynomial Order : N/A";

                async Task<int> FillData(double[] data, int startCol)
                {
                    int total = data.Length;
                    int curCol = startCol;
                    int idx = 0;

                    while (idx < total)
                    {
                        int chunk = Math.Min(maxRows, total - idx);
                        var arr = new double[chunk, 1];

                        for (int row = 0; row < chunk; row++, idx++)
                            arr[row, 0] = data[idx];

                        Excel.Range topCell = null, bottomCell = null, range = null;
                        try
                        {
                            topCell = (Excel.Range)ws.Cells[4, curCol];
                            bottomCell = (Excel.Range)ws.Cells[4 + chunk - 1, curCol];
                            range = ws.Range[topCell, bottomCell];
                            range.Value2 = arr;
                        }
                        finally
                        {
                            FinalRelease(range);
                            FinalRelease(bottomCell);
                            FinalRelease(topCell);
                        }

                        progressBar1.Value = Math.Min(100, (int)(100.0 * idx / total));
                        await Task.Yield();

                        curCol++;
                    }

                    progressBar1.Value = 100;
                    return curCol - 1;
                }

                async Task<int> FillSection(string title, double[] data, int startCol)
                {
                    ws.Cells[3, startCol] = title;
                    int lastCol = await FillData(data, startCol);
                    return lastCol + 2;
                }

                var sections = new List<(string Title, int StartCol, int EndCol)>();
                int col = 3;

                async Task AddSection(string title, double[] data, bool enabled)
                {
                    if (!enabled) return;
                    int start = col;
                    col = await FillSection(title, data, col);
                    sections.Add((title, start, col - 2));
                }

                await AddSection("Initial Dataset", initialData, true);
                await AddSection("Rectangular Averaging", rectAvg, doRect);
                await AddSection("Binomial Averaging", binomAvg, doAvg);
                await AddSection("Binomial Median Filtering", binomMed, doMed);
                await AddSection("Gaussian Filtering", gaussFilt, doGauss);
                await AddSection("Savitzky-Golay Filtering", sgFilt, doSG);

                int lastSectionEnd = sections.Last().EndCol;
                int chartColBase = lastSectionEnd + 4;
                int chartRowBase = 4;

                chartObjects = (Excel.ChartObjects)ws.ChartObjects();
                coms.Push(chartObjects);

                double chartLeft, chartTop;
                Excel.Range anchorCell = null;
                try
                {
                    anchorCell = (Excel.Range)ws.Cells[chartRowBase, chartColBase];
                    chartLeft = anchorCell.Left;
                    chartTop = anchorCell.Top;
                }
                finally
                {
                    FinalRelease(anchorCell);
                }

                chartObj = chartObjects.Add(chartLeft, chartTop, 900, 600);
                coms.Push(chartObj);

                chart = chartObj.Chart;
                coms.Push(chart);

                chart.ChartType = Excel.XlChartType.xlLine;
                chart.HasTitle = true;
                chart.ChartTitle.Text = txtDatasetTitle.Text;
                //chart.ChartTitle.Text = "Refining Raw Signals with SonataSmooth";

                chart.HasLegend = true;
                chart.Legend.Position = Excel.XlLegendPosition.xlLegendPositionRight;

                chart.Axes(Excel.XlAxisType.xlValue).HasTitle = true;
                chart.Axes(Excel.XlAxisType.xlValue).AxisTitle.Text = "Value";
                chart.Axes(Excel.XlAxisType.xlCategory).HasTitle = true;
                chart.Axes(Excel.XlAxisType.xlCategory).AxisTitle.Text = "Sequence Number";

                seriesCollection = chart.SeriesCollection();
                coms.Push(seriesCollection);

                foreach (var (Title, StartCol, EndCol) in sections)
                {
                    Excel.Range unionRange = null;
                    try
                    {
                        for (int c = StartCol; c <= EndCol; c++)
                        {
                            int fullCols = n / maxRows;
                            int rowsInCol = (c - StartCol < fullCols)
                                ? maxRows
                                : n - fullCols * maxRows;
                            if (rowsInCol <= 0) break;

                            Excel.Range top = null, bottom = null, dataRange = null;
                            try
                            {
                                top = (Excel.Range)ws.Cells[4, c];
                                bottom = (Excel.Range)ws.Cells[4 + rowsInCol - 1, c];
                                dataRange = ws.Range[top, bottom];

                                if (unionRange == null)
                                    unionRange = dataRange;
                                else
                                {
                                    // Union 이 새로운 Range 를 반환하므로 이전 unionRange 는 그대로 두고 누적
                                    Excel.Range merged = null;
                                    try
                                    {
                                        merged = excel.Union(unionRange, dataRange);
                                    }
                                    finally
                                    {
                                        // 기존 unionRange 와 dataRange 의 RCW 를 해제,
                                        // merged 를 새로운 unionRange 로 교체
                                        FinalRelease(unionRange);
                                        FinalRelease(dataRange);
                                    }
                                    unionRange = merged;
                                    // merged 는 unionRange 로 승격되었으므로 여기서 해제하지 않음
                                    top = null; bottom = null; dataRange = null;
                                }
                            }
                            finally
                            {
                                FinalRelease(bottom);
                                FinalRelease(top);
                                // dataRange 는 위에서 merged 교체 시 해제됨.
                                // (첫 블록일 경우 unionRange 가 참조)
                            }
                        }

                        var series = seriesCollection.NewSeries();
                        try
                        {
                            series.Name = Title;
                            series.Values = unionRange;
                        }
                        finally
                        {
                            FinalRelease(series);
                        }

                        excel.Visible = true;
                    }
                    finally
                    {
                        FinalRelease(unionRange);
                    }
                }

                progressBar1.Value = 0;

                // 창 유지 : 사용자에게 권한 이관
                wb.Saved = false; // 닫기 시 저장 대화상자 표시
                excel.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Excel export failed: " + ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 생성된 COM 참조 모두 해제 (Excel 창은 그대로 살아있음)
                ReleaseAll(coms);

                // RCW Finalizer 보장을 위해 2 회 GC
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private void cbxKernelRadius_SelectedIndexChanged(object sender, EventArgs e)
        {
            settingsForm.kernelRadius = int.TryParse(cbxKernelRadius.Text, out var w) ? w : settingsForm.kernelRadius;
            settingsForm.cbxKernelRadius.Text = cbxKernelRadius.Text;
        }

        private void cbxPolyOrder_SelectedIndexChanged(object sender, EventArgs e)
        {
            settingsForm.polyOrder = int.TryParse(cbxPolyOrder.Text, out var p) ? p : settingsForm.polyOrder;
            settingsForm.cbxPolyOrder.Text = cbxPolyOrder.Text;
        }

        private void btnExportSettings_Click(object sender, EventArgs e)
        {
            settingsForm.ApplyParameters(cbxKernelRadius.Text, cbxPolyOrder.Text);
            settingsForm.ShowDialog();
        }

        private void btnInfo_Click(object sender, EventArgs e)
        {
            if (aboutForm == null || aboutForm.IsDisposed)
                aboutForm = new FrmAbout();
            aboutForm.ShowDialog(this);
        }

        private void txtExcelTitle_Enter(object sender, EventArgs e)
        {
            if (txtDatasetTitle.Text == ExcelTitlePlaceholder)
            {
                txtDatasetTitle.Text = "";
                txtDatasetTitle.ForeColor = SystemColors.WindowText;
            }
            txtDatasetTitle.TextAlign = HorizontalAlignment.Left;
        }

        private void txtExcelTitle_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDatasetTitle.Text))
            {
                txtDatasetTitle.Text = ExcelTitlePlaceholder;
                txtDatasetTitle.ForeColor = Color.Gray;
                txtDatasetTitle.TextAlign = HorizontalAlignment.Center;
            }
        }

        private void txtExcelTitle_TextChanged(object sender, EventArgs e)
        {
            UpdateExportExcelButtonState();

            // 텍스트가 placeholder 가 아니고 비어있지 않으면 우측 정렬
            if (txtDatasetTitle.Text != ExcelTitlePlaceholder)
            {
                txtDatasetTitle.TextAlign = HorizontalAlignment.Left;
            }
            else
            {
                // placeholder 또는 빈 값일 때는 가운데 정렬
                txtDatasetTitle.TextAlign = HorizontalAlignment.Center;
            }
        }

        private void UpdateExportExcelButtonState()
        {
            bool hasItems = listBox1.Items.Count > 0;
            bool isValid = hasItems
                && !string.IsNullOrWhiteSpace(txtDatasetTitle.Text)
                && txtDatasetTitle.Text != ExcelTitlePlaceholder;
            btnExport.Enabled = isValid;
        }

        private void btnSync1_Click(object sender, EventArgs e)
        {
            int count = listBox1.Items.Count;
            if (count != listBox2.Items.Count || listBox1.SelectedIndices.Count == 0)
                return;

            listBox2.BeginUpdate();
            try
            {
                listBox2.ClearSelected();

                var indices = new int[listBox1.SelectedIndices.Count];
                listBox1.SelectedIndices.CopyTo(indices, 0);

                for (int i = 0; i < indices.Length; i++)
                    listBox2.SetSelected(indices[i], true);

                // 스크롤 위치 동기화
                listBox2.TopIndex = listBox1.TopIndex;
            }
            finally
            {
                listBox2.EndUpdate();
            }
        }

        private void btnSync2_Click(object sender, EventArgs e)
        {
            int count = listBox2.Items.Count;
            if (count != listBox1.Items.Count || listBox2.SelectedIndices.Count == 0)
                return;

            listBox1.BeginUpdate();
            try
            {
                listBox1.ClearSelected();

                var indices = new int[listBox2.SelectedIndices.Count];
                listBox2.SelectedIndices.CopyTo(indices, 0);

                for (int i = 0; i < indices.Length; i++)
                    listBox1.SetSelected(indices[i], true);

                // 스크롤 위치 동기화
                listBox1.TopIndex = listBox2.TopIndex;
            }
            finally
            {
                listBox1.EndUpdate();
            }
        }
    }
}

