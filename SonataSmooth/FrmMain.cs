﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace SonataSmooth
{
    public partial class FrmMain : Form
    {
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


        public FrmMain()
        {
            InitializeComponent();

            // 키보드 Delete → btnDelete 클릭 처리
            this.KeyPreview = true;

            // 선택 상태에 따라 버튼 활성 / 비활성 토글
            listBox1.SelectedIndexChanged += (s, e) =>
            {
                bool hasSelection = listBox1.SelectedIndex >= 0;
                btnDelete.Enabled = hasSelection;
                btnEdit.Enabled = hasSelection;
                btnCopy.Enabled = hasSelection;
                btnSelClear.Enabled = hasSelection;
            };
        }

        private async void btnCalibrate_Click(object sender, EventArgs e)
        {
            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            if (listBox1.Items.Count == 0)
                return;

            try
            {
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
                    MessageBox.Show($"Binomial coefficient calculation error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                                throw new InvalidOperationException($"SG coefficient computation failed: {ex.Message}", ex);
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
                        $"An error occurred during parallel computation: {aex.Flatten().InnerException?.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error
                    );
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An unexpected error occurred during computation: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                    btnSelClear2.Enabled = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error binding results: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                if (listBox2.Items.Count > 0)
                {
                    btnCopy2.Enabled = true;
                    btnSelClear2.Enabled = true;
                }
                else
                {
                    btnCopy2.Enabled = false;
                    btnSelClear2.Enabled = false;
                }

                progressBar1.Value = 0;
                btnCalibrate.Enabled = true;
            }
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

                // 누적 가중치 > 절반: 기존보다 작은 쪽 넘어섰으면 current 반환
                if (accum > half)
                {
                    return pairs[i].Value;
                }

                // 누적 가중치 == 절반인 순간: 다음 값과 평균 처리
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
                btnCopy.Enabled = true;
                btnDelete.Enabled = true;
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
            _ctsSelectAll1?.Cancel(); // 이전 작업 취소
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
                UpdateButtonStates();
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
            UpdateButtonStates();
            await Task.Delay(200);
            progressBar1.Value = 0;
        }

        private async void btnClear_Click(object sender, EventArgs e)
        {
            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            listBox1.BeginUpdate();
            listBox1.ClearSelected();
            listBox1.Items.Clear();
            listBox1.EndUpdate();

            await Task.Yield();

            lblCnt1.Text = $"Count : {listBox1.Items.Count}";
            btnCopy.Enabled = false;
            btnSelClear.Enabled = false;
            btnDelete.Enabled = false;
            btnEdit.Enabled = false;

            progressBar1.Value = 100;
            progressBar1.Refresh();

            await Task.Yield();
            progressBar1.Value = 0;

            listBox1.Focus();
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
                if (listBox1.Items.Count > 0)
                {
                    btnCopy.Enabled = true;
                    btnDelete.Enabled = true;
                }
                else
                {
                    btnCopy.Enabled = false;
                    btnDelete.Enabled = false;
                }

                progressBar1.Value = 0;
                btnCalibrate.Enabled = true;
            }
        }


        private async void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            btnCalibrate.Enabled = false;
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
                btnCalibrate.Enabled = true;
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

            // Deletion process
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
            if (listBox1.SelectedIndices.Count == listBox1.Items.Count)
            {
                listBox1.Items.Clear();
                btnCopy.Enabled = false;
                lblCnt1.Text = "Count : 0";
                listBox1.Select();
                progressBar1.Value = 0;
                return;
            }
            await DeleteSelectedItemsPreserveSelection(listBox1, progressBar1, lblCnt1);
            lblCnt1.Text = $"Count : {listBox1.Items.Count}";
            listBox1.Select();
        }

        private void UpdateButtonStates()
        {
            bool hasSel = listBox1.SelectedItems.Count > 0;
            btnDelete.Enabled = hasSel;
            btnEdit.Enabled = hasSel;
            btnCopy.Enabled = hasSel;
            btnSelClear.Enabled = hasSel;
        }

        private async void btnClear2_Click(object sender, EventArgs e)
        {
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
            progressBar1.Value = 0;
        }


        private void txtVariable_TextChanged(object sender, EventArgs e)
        {
            btnAdd.Enabled = txtVariable.Text.Length > 0 && double.TryParse(txtVariable.Text, out _);
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.Items.Count == 0)
            {
                btnCopy2.Enabled = false;
                btnSelClear2.Enabled = false;
                btnEdit.Enabled = false;
            }
            else
            {
                btnCopy2.Enabled = true;
            }

            if (listBox2.SelectedItems.Count == 0)
            {
                btnSelClear2.Enabled = false;
            }
            else
            {
                btnSelClear2.Enabled = true;
            }
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
            var items = listBox1.SelectedItems.Count > 0
                ? listBox1.SelectedItems.Cast<double>()
                : listBox1.Items.Cast<double>();

            Clipboard.SetText(string.Join(Environment.NewLine, items));
        }

        private void btnCopy2_Click(object sender, EventArgs e)
        {
            var items = listBox2.SelectedItems.Count > 0
                ? listBox2.SelectedItems.Cast<double>()
                : listBox2.Items.Cast<double>();

            Clipboard.SetText(string.Join(Environment.NewLine, items));
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
            btnAdd.Enabled = false;
            btnEdit.Enabled = false;
            btnCopy2.Enabled = false;
            btnSelClear2.Enabled = false;
            btnCopy.Enabled = false;
            btnSelClear.Enabled = false;
            btnDelete.Enabled = false;

            this.KeyPreview = true;

            listBox1.SelectedIndexChanged += (s, evt) =>
            {
                bool hasSel = listBox1.SelectedIndex >= 0;
                btnDelete.Enabled = hasSel;
                btnEdit.Enabled = hasSel;
                btnCopy.Enabled = hasSel;
                btnSelClear.Enabled = hasSel;
            };
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.Items.Count == 0)
            {
                btnCopy.Enabled = false;
                btnSelClear.Enabled = false;
                btnEdit.Enabled = false;
            }
            else
            {
                btnCopy.Enabled = true;
            }

            if (listBox1.SelectedItems.Count == 0)
            {
                btnEdit.Enabled = false;
                btnSelClear.Enabled = false;
            }
            else
            {
                btnSelClear.Enabled = true;
                btnEdit.Enabled = true;
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            var frm = new FrmModify();
            frm.ShowDialog();

            frm.textBox1.Select();
        }
    }
}

