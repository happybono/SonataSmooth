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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using Excel = Microsoft.Office.Interop.Excel;

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
        private int w;
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

        private async void btnCalibrate_Click(object sender, EventArgs e)
        {
            settingsForm.ApplyParameters(cbxKernelWidth.Text, cbxPolyOrder.Text);

            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            if (listBox1.Items.Count == 0)
                return;

            try
            {
                btnCalibrate.Enabled = false;

                double[] input;
                int n;
                try
                {
                    n = listBox1.Items.Count;
                    input = new double[n];
                    for (int i = 0; i < n; i++)
                    {
                        var item = listBox1.Items[i];
                        if (item == null)
                            throw new InvalidOperationException($"The item at index {i} is null.");
                        string s = item.ToString();
                        if (!double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out input[i]))
                            throw new FormatException($"Failed to convert item at index {i} : \"{s}\"");
                    }
                }
                catch (Exception ex) when (ex is FormatException || ex is OverflowException || ex is InvalidOperationException)
                {
                    MessageBox.Show($"Data input error : {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int w, polyOrder;
                double sigma;
                try
                {
                    if (!int.TryParse(cbxKernelWidth.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out w))
                        throw new FormatException($"Failed to parse kernel width : \"{cbxKernelWidth.Text}\".");
                    if (!int.TryParse(cbxPolyOrder.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out polyOrder))
                        throw new FormatException($"Failed to parse polynomial order : \"{cbxPolyOrder.Text}\".");
                    // sigma 변수를 comboBox 나 textBox 에서 받아도 되지만, 여기서는 width 에 기반해 기본값 계산
                    sigma = (2.0 * w + 1) / 6.0;
                }
                catch (Exception ex) when (ex is FormatException || ex is OverflowException)
                {
                    MessageBox.Show($"Parameter error : {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!ValidateSmoothingParameters(listBox1.Items.Count, w, polyOrder))
                    return;

                bool useRect = rbtnRect.Checked;
                bool useMed = rbtnMed.Checked;
                bool useAvg = rbtnAvg.Checked;
                bool useSG = rbtnSG.Checked;
                bool useGauss = rbtnGauss.Checked;

                int[] binom;
                try
                {
                    binom = CalcBinomialCoefficients(2 * w + 1);
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
                        gaussCoeffs = ComputeGaussianCoefficients(2 * w + 1, sigma);
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
                                sgCoeffs = ComputeSavitzkyGolayCoefficients(2 * w + 1, polyOrder);
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
                                        for (int k = -w; k <= w; k++)
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
                                        return WeightedMedianAt(input, i, w, binom);
                                    }
                                    else if (useAvg)
                                    {
                                        double sum = 0;
                                        int cs = 0;
                                        for (int k = -w; k <= w; k++)
                                        {
                                            int idx = i + k;
                                            if (idx < 0 || idx >= n) continue;
                                            sum += input[idx] * binom[k + w];
                                            cs += binom[k + w];
                                        }
                                        return cs > 0 ? sum / cs : 0;
                                    }
                                    else if (useSG)
                                    {
                                        double sum = 0;
                                        for (int k = -w; k <= w; k++)
                                        {
                                            int mi = Mirror(i + k);
                                            sum += sgCoeffs[k + w] * input[mi];
                                        }
                                        return sum;
                                    }
                                    else if (useGauss)
                                    {
                                        double sum = 0;
                                        for (int k = -w; k <= w; k++)
                                        {
                                            int mi = Mirror(i + k);
                                            sum += gaussCoeffs[k + w] * input[mi];
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

                    slblKernelWidth.Text = w.ToString();

                    bool showPoly = useSG;
                    toolStripStatusLabel5.Visible =
                    toolStripStatusLabel6.Visible =
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
                    $"Kernel width is too large.\nThe window size ({windowSize}) must not exceed the number of data points ({dataCount}).",
                    "Parameter Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            if (useSG && polyOrder >= windowSize)
            {
                MessageBox.Show(
                    $"Polynomial order must be smaller than the window size ({windowSize}).",
                    "Parameter Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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

            txtExcelTitle.Text = ExcelTitlePlaceholder;
            txtExcelTitle.TextAlign = HorizontalAlignment.Center;
            txtExcelTitle.ForeColor = Color.Gray;
            txtVariable.Select();
        }

        public void SetComboValues(string kernelWidth, string polyOrder)
        {
            cbxKernelWidth.Text = kernelWidth;
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

        private async Task DeleteSelectedItemsPreserveSelection(ListBox lb, System.Windows.Forms.ProgressBar progressBar, Label lblCount)
        {
            var indices = lb.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToArray();
            int total = indices.Length;
            if (total == 0) return;

            progressBar.Minimum = 0;
            progressBar.Maximum = total;
            progressBar.Value = 0;

            var newSelections = new List<int>();
            var shiftMap = new Dictionary<int, bool>();

            foreach (var idx in indices)
            {
                shiftMap[idx] = true;
                if (idx < lb.Items.Count - 1)
                {
                    newSelections.Add(idx);
                }
            }

            lb.SuspendLayout();
            lb.BeginUpdate();

            // 삭제
            int updateInterval = Math.Max(1, total / 100);
            for (int i = 0; i < total; i++)
            {
                lb.Items.RemoveAt(indices[i]);
                if (((i + 1) % updateInterval == 0) || i == total - 1)
                {
                    progressBar.Value = i + 1;
                    Application.DoEvents();
                }
            }

            lb.EndUpdate();
            lb.ResumeLayout();

            lb.SelectedIndices.Clear();
            foreach (var idx in newSelections)
            {
                if (idx < lb.Items.Count)
                    lb.SelectedIndices.Add(idx);
            }

            lblCount.Text = $"Count : {lb.Items.Count}";
            await Task.Delay(200);
            progressBar.Value = 0;
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            int selectedCount = listBox1.SelectedIndices.Count;

            string message = selectedCount == listBox1.Items.Count
                ? $"You are about to delete all {selectedCount} items from the list.\nThis will also delete all items from the Refined Dataset listbox.\n\nAre you sure you want to proceed?"
                : $"You are about to delete {selectedCount} selected item{(selectedCount > 1 ? "s" : "")} from the list.\n\nAre you sure you want to proceed?";

            var result = MessageBox.Show(message,
                                         "Delete Confirmation",
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {
                return;
            }


            if (selectedCount == listBox1.Items.Count)
            {
                listBox1.Items.Clear();
                listBox2.Items.Clear();
                lblCnt1.Text = $"Count : {listBox1.Items.Count}";
                lblCnt2.Text = $"Count : {listBox2.Items.Count}";
                listBox1.Select();
                progressBar1.Value = 0;

                txtExcelTitle.Text = ExcelTitlePlaceholder;
                txtExcelTitle.TextAlign = HorizontalAlignment.Center;
                txtExcelTitle.ForeColor = Color.Gray;
                UpdateListBox1BtnsState(null, EventArgs.Empty);
                txtVariable.Select();
                return;
            }

            await DeleteSelectedItemsPreserveSelection(listBox1, progressBar1, lblCnt1);
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
                && !string.IsNullOrWhiteSpace(txtExcelTitle.Text)
                && txtExcelTitle.Text != ExcelTitlePlaceholder;
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
            slblKernelWidth.Text = "--";
            toolStripStatusLabel6.Visible = false;
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
            cbxKernelWidth.SelectedIndex = 3;
            cbxPolyOrder.SelectedIndex = 1;

            txtExcelTitle.Text = ExcelTitlePlaceholder;
            txtExcelTitle.ForeColor = Color.Gray;
            txtExcelTitle.Enter += txtExcelTitle_Enter;
            txtExcelTitle.Leave += txtExcelTitle_Leave;
            txtExcelTitle.TextChanged += txtExcelTitle_TextChanged;
            UpdateExportExcelButtonState();

            settingsForm.chbRect.Checked = true;
            settingsForm.chbAvg.Checked = true;
            settingsForm.chbMed.Checked = true;
            settingsForm.chbGauss.Checked = true;
            settingsForm.chbSG.Checked = true;

            settingsForm.rbtnCSV.Checked = true;
            settingsForm.DoAutoSave = true;

            // ComboBox 등 동기화
            settingsForm.cbxKernelWidth.Text = cbxKernelWidth.Text;
            settingsForm.cbxPolyOrder.Text = cbxPolyOrder.Text;

            UpdateListBox1BtnsState(null, EventArgs.Empty);
            UpdateListBox2BtnsState(null, EventArgs.Empty);

            dataCount = listBox1.Items.Count;

            // ComboBox 값으로부터 파싱
            int.TryParse(cbxKernelWidth.Text, out w);
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
            int w = 2, polyOrder = 2, n = 0;
            bool doRect = false, doAvg = false, doMed = false, doGauss = false, doSG = false;
            string excelTitle = "";
            double[] initialData = null;

            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    w = int.TryParse(settingsForm.cbxKernelWidth.Text, out var tmpW) ? tmpW : 2;
                    polyOrder = int.TryParse(settingsForm.cbxPolyOrder.Text, out var tmpP) ? tmpP : 2;
                    doRect = settingsForm.chbRect.Checked;
                    doAvg = settingsForm.chbAvg.Checked;
                    doMed = settingsForm.chbMed.Checked;
                    doGauss = settingsForm.chbGauss.Checked;
                    doSG = settingsForm.chbSG.Checked;
                    excelTitle = txtExcelTitle.Text;
                    initialData = listBox1.Items
                        .Cast<object>()
                        .Select(x => double.TryParse(
                                 x?.ToString(),
                                 NumberStyles.Any,
                                 CultureInfo.InvariantCulture,
                                 out var d)
                               ? d
                               : 0.0)
                        .ToArray();
                    n = initialData.Length;
                }));
            }
            else
            {
                w = int.TryParse(settingsForm.cbxKernelWidth.Text, out var tmpW) ? tmpW : 2;
                polyOrder = int.TryParse(settingsForm.cbxPolyOrder.Text, out var tmpP) ? tmpP : 2;
                doRect = settingsForm.chbRect.Checked;
                doAvg = settingsForm.chbAvg.Checked;
                doMed = settingsForm.chbMed.Checked;
                doGauss = settingsForm.chbGauss.Checked;
                doSG = settingsForm.chbSG.Checked;
                excelTitle = txtExcelTitle.Text;
                initialData = listBox1.Items
                    .Cast<object>()
                    .Select(x => double.TryParse(
                             x?.ToString(),
                             NumberStyles.Any,
                             CultureInfo.InvariantCulture,
                             out var d)
                           ? d
                           : 0.0)
                    .ToArray();
                n = initialData.Length;
            }

            if (!ValidateSmoothingParameters(n, w, polyOrder))
                return;

            if (n == 0)
            {
                MessageBox.Show(
                    "No data to export.",
                    "Export CSV",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            double sigma = (2.0 * w + 1) / 6.0;
            int[] binom = CalcBinomialCoefficients(2 * w + 1);

            var rectAvg = new double[n];
            var binomAvg = new double[n];
            var binomMed = new double[n];
            var gaussFilt = new double[n];
            var sgFilt = new double[n];

            await Task.Run(() =>
            {
                Parallel.For(0, n, i =>
                {
                    // Rectangular Average
                    double sum = 0; int cnt = 0;
                    for (int k = -w; k <= w; k++)
                    {
                        int idx = i + k;
                        if (idx >= 0 && idx < n)
                        {
                            sum += initialData[idx];
                            cnt++;
                        }
                    }
                    rectAvg[i] = cnt > 0 ? sum / cnt : 0.0;

                    // Binomial Average
                    sum = 0; cnt = 0;
                    for (int k = -w; k <= w; k++)
                    {
                        int idx = i + k;
                        if (idx >= 0 && idx < n)
                        {
                            sum += initialData[idx] * binom[k + w];
                            cnt += binom[k + w];
                        }
                    }
                    binomAvg[i] = cnt > 0 ? sum / cnt : 0.0;

                    // Weighted Median
                    binomMed[i] = WeightedMedianAt(initialData, i, w, binom);

                    // Gaussian Filter (Mirror)
                    var gaussCoeffs = ComputeGaussianCoefficients(2 * w + 1, sigma);
                    sum = 0;
                    int Mirror(int idx) =>
                        idx < 0 ? -idx - 1 :
                        idx >= n ? 2 * n - idx - 1 :
                        idx;
                    for (int k = -w; k <= w; k++)
                        sum += gaussCoeffs[k + w] * initialData[Mirror(i + k)];
                    gaussFilt[i] = sum;

                    // Savitzky–Golay
                    var sgCoeffs = ComputeSavitzkyGolayCoefficients(2 * w + 1, polyOrder);
                    sum = 0;
                    for (int k = -w; k <= w; k++)
                        sum += sgCoeffs[k + w] * initialData[Mirror(i + k)];
                    sgFilt[i] = sum;
                });
            });

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

                        if (dlg.ShowDialog() == DialogResult.OK)
                            basePath = dlg.FileName;
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

                    if (dlg.ShowDialog() == DialogResult.OK)
                        basePath = dlg.FileName;
                }
            }
            if (string.IsNullOrEmpty(basePath))
                return;

            var columns = new List<(string Header, double[] Data)> {
        ("Initial Dataset", initialData)
    };
            if (doRect) columns.Add(("Rectangular Averaging", rectAvg));
            if (doAvg) columns.Add(("Binomial Averaging", binomAvg));
            if (doMed) columns.Add(("Binomial Median Filtering", binomMed));
            if (doGauss) columns.Add(("Gaussian Filtering", gaussFilt));
            if (doSG) columns.Add(("Savitzky–Golay Filtering", sgFilt));

            const int ExcelMaxRows = 1_048_576;
            int headerLines =
                1   // 제목
              + 1   // 전체 중 'n' 번째 부분 (분할 저장 시)
              + 1   // 빈 줄
              + 1   // Smoothing Parameters
              + 1   // Kernel Width
              + (doSG ? 1 : 0) // 다항식의 차수
              + 1   // 빈 줄
              + 1   // Generated : 생성 일시
              + 1   // 빈 줄
              + 1;  // 행 Header

            int maxDataRows = ExcelMaxRows - headerLines;
            int partCount = (n + maxDataRows - 1) / maxDataRows;

            IProgress<int> progress = new Progress<int>(percent =>
            {
                if (progressBar1.InvokeRequired)
                    progressBar1.Invoke(new Action(() => progressBar1.Value = percent));
                else
                    progressBar1.Value = percent;
            });

            string dir = Path.GetDirectoryName(basePath);
            string nameOnly = Path.GetFileNameWithoutExtension(basePath);
            string ext = Path.GetExtension(basePath);

            for (int part = 0; part < partCount; part++)
            {
                int startRow = part * maxDataRows;
                int rowCount = Math.Min(maxDataRows, n - startRow);

                string path = partCount == 1
                    ? basePath
                    : Path.Combine(dir, $"{nameOnly}_Part{part + 1}{ext}");

                const int bufSize = 81920;
                var encoding = Encoding.UTF8;

                using (var fs = new FileStream(
                           path,
                           FileMode.Create,
                           FileAccess.Write,
                           FileShare.None,
                           bufSize,
                           useAsync: true))
                using (var sw = new StreamWriter(fs, encoding, bufSize))
                {
                    await sw.WriteLineAsync(excelTitle);
                    await sw.WriteLineAsync($"Part {part + 1} of {partCount}");
                    await sw.WriteLineAsync(string.Empty);
                    await sw.WriteLineAsync("Smoothing Parameters");
                    await sw.WriteLineAsync($"Kernel Width : {w}");
                    if (doSG)
                        await sw.WriteLineAsync($"Polynomial Order : {polyOrder}");
                    await sw.WriteLineAsync(string.Empty);
                    await sw.WriteLineAsync($"Generated : {DateTime.Now.ToString("G", CultureInfo.CurrentCulture)}");
                    await sw.WriteLineAsync(string.Empty);

                    // 행 Header 쓰기
                    await sw.WriteLineAsync(string.Join(
                        ",", columns.Select(c => c.Header)));

                    // 데이터 쓰기
                    for (int i = startRow; i < startRow + rowCount; i++)
                    {
                        string line = string.Join(
                            ",",
                            columns.Select(c =>
                                c.Data[i].ToString(
                                    CultureInfo.InvariantCulture)));
                        await sw.WriteLineAsync(line);

                        // Progress
                        int percent = (int)(((double)(i + 1) / n) * 100);
                        progress.Report(percent);
                    }
                }
            }

            if (progressBar1.InvokeRequired)
                progressBar1.Invoke(new Action(() => progressBar1.Value = 0));
            else
                progressBar1.Value = 0;

            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show(
                        "CSV export completed.",
                        "Export CSV",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }));
            }
            else
            {
                MessageBox.Show(
                    "CSV export completed.",
                    "Export CSV",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

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
                        string openPath = partCount == 1
                            ? basePath
                            : Path.Combine(dir, $"{nameOnly}_Part{part + 1}{ext}");
                        if (File.Exists(openPath))
                            Process.Start(openPath);
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

        private Task WriteCsvOptimizedAsync(
            string path,
            string title,
            int kernelWidth,
            int? polyOrder,
            List<(string Header, double[] Data)> columns,
            int totalRows,
            IProgress<int> progress)
        {
            return Task.Run(async () =>
            {
                var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
                const int bufSize = 1 << 20;
                using (var fs = new FileStream(
                           path,
                           FileMode.Create,
                           FileAccess.Write,
                           FileShare.None,
                           bufSize,
                           useAsync: true))
                using (var sw = new StreamWriter(fs, encoding, bufSize))
                {
                    await sw.WriteLineAsync(title);
                    await sw.WriteLineAsync();
                    await sw.WriteLineAsync("Smoothing Parameters");
                    await sw.WriteLineAsync($"Kernel Width,{kernelWidth}");
                    await sw.WriteLineAsync($"Polynomial Order,{(polyOrder.HasValue ? polyOrder.Value.ToString() : "N/A")}");
                    await sw.WriteLineAsync();

                    var headerSb = new StringBuilder(256)
                        .Append("Index");
                    foreach (var col in columns)
                        headerSb.Append(',').Append(col.Header);
                    await sw.WriteLineAsync(headerSb.ToString());
                    headerSb.Clear();

                    var lineSb = new StringBuilder(512);
                    int reportInterval = Math.Max(1, totalRows / 200);
                    for (int i = 0; i < totalRows; i++)
                    {
                        lineSb.Append(i + 1);
                        foreach (var col in columns)
                        {
                            lineSb.Append(',');
                            lineSb.Append(col.Data[i].ToString("G17", CultureInfo.InvariantCulture));
                        }
                        await sw.WriteLineAsync(lineSb.ToString());
                        lineSb.Clear();

                        if (i % reportInterval == 0)
                            progress.Report((int)(100.0 * i / totalRows));
                    }
                    progress.Report(100);

                    await sw.FlushAsync();
                }
            });
        }

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
            int w = int.TryParse(settingsForm.cbxKernelWidth.Text, out var tmpW) ? tmpW : 2;
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


            if (!ValidateSmoothingParameters(n, w, polyOrder))
                return;

            const int maxRows = 1_048_573;
            double sigma = (2.0 * w + 1) / 6.0;
            int[] binom = CalcBinomialCoefficients(2 * w + 1);

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
                Parallel.For(0, n, i =>
                {
                    // Rectangular
                    double sum = 0; int cnt = 0;
                    for (int k = -w; k <= w; k++)
                    {
                        int idx = i + k;
                        if (idx >= 0 && idx < n) { sum += initialData[idx]; cnt++; }
                    }
                    rectAvg[i] = cnt > 0 ? sum / cnt : 0.0;

                    // Binomial average
                    sum = 0; cnt = 0;
                    for (int k = -w; k <= w; k++)
                    {
                        int idx = i + k;
                        if (idx >= 0 && idx < n)
                        {
                            sum += initialData[idx] * binom[k + w];
                            cnt += binom[k + w];
                        }
                    }
                    binomAvg[i] = cnt > 0 ? sum / cnt : 0.0;

                    // Weighted median
                    binomMed[i] = WeightedMedianAt(initialData, i, w, binom);

                    // Gaussian filter (mirror)
                    var gaussCoeffs = ComputeGaussianCoefficients(2 * w + 1, sigma);
                    sum = 0;
                    int Mirror(int idx) =>
                        idx < 0 ? -idx - 1 : (idx >= n ? 2 * n - idx - 1 : idx);
                    for (int k = -w; k <= w; k++)
                        sum += gaussCoeffs[k + w] * initialData[Mirror(i + k)];
                    gaussFilt[i] = sum;

                    // Savitzky–Golay filter
                    var sgCoeffs = ComputeSavitzkyGolayCoefficients(2 * w + 1, polyOrder);
                    sum = 0;
                    for (int k = -w; k <= w; k++)
                        sum += sgCoeffs[k + w] * initialData[Mirror(i + k)];
                    sgFilt[i] = sum;
                });
            });

            var excel = new Excel.Application();
            var wb = excel.Workbooks.Add();
            var ws = (Excel.Worksheet)wb.Worksheets[1];
            //excel.Visible = true;

            ws.Cells[1, 1] = txtExcelTitle.Text;
            ws.Cells[3, 1] = "Smoothing Parameters";
            ws.Cells[4, 1] = $"Kernel Width : {w}";
            ws.Cells[5, 1] = doSG
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

                    for (int r = 0; r < chunk; r++, idx++)
                        arr[r, 0] = data[idx];

                    var range = ws.Range[
                        ws.Cells[4, curCol],
                        ws.Cells[4 + chunk - 1, curCol]
                    ];
                    range.Value2 = arr;

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

            var chartObjects = (Excel.ChartObjects)ws.ChartObjects();
            var chartLeft = ws.Cells[chartRowBase, chartColBase].Left;
            var chartTop = ws.Cells[chartRowBase, chartColBase].Top;
            var chartObj = chartObjects.Add(chartLeft, chartTop, 900, 600);
            var chart = chartObj.Chart;

            chart.ChartType = Excel.XlChartType.xlLine;
            chart.HasTitle = true;
            chart.ChartTitle.Text = "Refining Raw Signals with SonataSmooth";

            chart.HasLegend = true;
            chart.Legend.Position = Excel.XlLegendPosition.xlLegendPositionRight;

            chart.Axes(Excel.XlAxisType.xlValue).HasTitle = true;
            chart.Axes(Excel.XlAxisType.xlValue).AxisTitle.Text = "Value";
            chart.Axes(Excel.XlAxisType.xlCategory).HasTitle = true;
            chart.Axes(Excel.XlAxisType.xlCategory).AxisTitle.Text = "Sequence Number";

            foreach (var (Title, StartCol, EndCol) in sections)
            {
                Excel.Range unionRange = null;

                for (int c = StartCol; c <= EndCol; c++)
                {
                    int fullCols = n / maxRows;
                    int rowsInCol = (c - StartCol < fullCols)
                        ? maxRows
                        : n - fullCols * maxRows;
                    if (rowsInCol <= 0) break;

                    var r = ws.Range[
                        ws.Cells[4, c],
                        ws.Cells[4 + rowsInCol - 1, c]
                    ];
                    unionRange = unionRange == null
                        ? r
                        : excel.Application.Union(unionRange, r);
                }

                var series = chart.SeriesCollection().NewSeries();
                series.Name = Title;
                series.Values = unionRange;
                excel.Visible = true;
            }

            progressBar1.Value = 0;
        }

        private void cbxKernelWidth_SelectedIndexChanged(object sender, EventArgs e)
        {
            settingsForm.KernelWidth = int.TryParse(cbxKernelWidth.Text, out var w) ? w : settingsForm.KernelWidth;
            settingsForm.cbxKernelWidth.Text = cbxKernelWidth.Text;
        }

        private void cbxPolyOrder_SelectedIndexChanged(object sender, EventArgs e)
        {
            settingsForm.PolyOrder = int.TryParse(cbxPolyOrder.Text, out var p) ? p : settingsForm.PolyOrder;
            settingsForm.cbxPolyOrder.Text = cbxPolyOrder.Text;
        }

        private void btnExportSettings_Click(object sender, EventArgs e)
        {
            settingsForm.ApplyParameters(cbxKernelWidth.Text, cbxPolyOrder.Text);
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
            if (txtExcelTitle.Text == ExcelTitlePlaceholder)
            {
                txtExcelTitle.Text = "";
                txtExcelTitle.ForeColor = SystemColors.WindowText;
            }
            txtExcelTitle.TextAlign = HorizontalAlignment.Left;
        }

        private void txtExcelTitle_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtExcelTitle.Text))
            {
                txtExcelTitle.Text = ExcelTitlePlaceholder;
                txtExcelTitle.ForeColor = Color.Gray;
                txtExcelTitle.TextAlign = HorizontalAlignment.Center;
            }
        }

        private void txtExcelTitle_TextChanged(object sender, EventArgs e)
        {
           UpdateExportExcelButtonState();

            // 텍스트가 placeholder 가 아니고 비어있지 않으면 우측 정렬
            if (txtExcelTitle.Text != ExcelTitlePlaceholder)
            {
                txtExcelTitle.TextAlign = HorizontalAlignment.Left;
            }
            else
            {
                // placeholder 또는 빈 값일 때는 가운데 정렬
                txtExcelTitle.TextAlign = HorizontalAlignment.Center;
            }
        }

        private void UpdateExportExcelButtonState()
        {
            bool hasItems = listBox1.Items.Count > 0;
            bool isValid = hasItems
                && !string.IsNullOrWhiteSpace(txtExcelTitle.Text)
                && txtExcelTitle.Text != ExcelTitlePlaceholder;
            btnExport.Enabled = isValid;
        }

        private void txtExcelTitle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
            {

            }
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

