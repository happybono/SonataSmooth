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

namespace NoiseReductionSample
{
    public partial class FrmMain : Form
    {
        private double[] _sgCoeffs;
        private int _lastWindow = -1, _lastPoly = -1;

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


        public FrmMain()
        {
            InitializeComponent();
        }

        private async void btnCalibrate_Click(object sender, EventArgs e)
        {
            // 1) ProgressBar 초기화 (UI 스레드)
            progressBar2.Style = ProgressBarStyle.Continuous;
            progressBar2.Minimum = 0;
            progressBar2.Maximum = 100;
            progressBar2.Value = 0;

            try
            {
                // 2) UI 스레드에서 한 번만 값 읽기
                int n = listBox1.Items.Count;
                var input = new double[n];
                for (int i = 0; i < n; i++)
                    input[i] = Convert.ToDouble(listBox1.Items[i], CultureInfo.InvariantCulture);

                int w = int.Parse(cbxKernelWidth.Text, CultureInfo.InvariantCulture);
                int polyOrder = int.Parse(cbxPolyOrder.Text, CultureInfo.InvariantCulture);
                bool useRect = rbtnRect.Checked;
                bool useMed = rbtnMed.Checked;
                bool useAvg = rbtnAvg.Checked;
                bool useSG = rbtnSG.Checked;

                int[] binom = CalcBinomialCoefficients(2 * w + 1);

                var progressReporter = new Progress<int>(pct =>
                {
                    progressBar2.Value = Math.Max(0, Math.Min(100, pct));
                });

                // 3) 백그라운드에서 연산 (UI 컨트롤 접근 없음)
                double[] results = await Task.Run(() =>
                {
                    double[] sgCoeffs = null;
                    if (useSG)
                        sgCoeffs = ComputeSavitzkyGolayCoefficients(2 * w + 1, polyOrder);

                    return ParallelEnumerable
                        .Range(0, n)
                        .AsOrdered()
                        .WithDegreeOfParallelism(Environment.ProcessorCount)
                        .Select(i =>
                        {
                            if (useRect)
                            {
                                // 직사각형 평균
                                double sum = 0; int cnt = 0;
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
                                // 이항 중앙값
                                var weighted = new List<double>();
                                for (int k = -w; k <= w; k++)
                                {
                                    int idx = i + k;
                                    if (idx < 0 || idx >= n) continue;
                                    double v = input[idx];
                                    int wt = binom[k + w];
                                    for (int z = 0; z < wt; z++)
                                        weighted.Add(v);
                                }
                                if (weighted.Count == 0) return 0;
                                weighted.Sort();
                                int m = weighted.Count / 2;
                                return (weighted.Count % 2 == 0)
                                     ? (weighted[m - 1] + weighted[m]) / 2.0
                                     : weighted[m];
                            }
                            else if (useAvg)
                            {
                                // 이항 평균
                                double sum = 0; int cs = 0;
                                for (int k = -w; k <= w; k++)
                                {
                                    int idx = i + k;
                                    if (idx < 0 || idx >= n) continue;
                                    double v = input[idx];
                                    int c = binom[k + w];
                                    sum += v * c;
                                    cs += c;
                                }
                                return cs > 0 ? sum / cs : 0;
                            }
                            else if (useSG)
                            {
                                // Savitzky–Golay 필터
                                double sum = 0;
                                for (int k = -w; k <= w; k++)
                                {
                                    int idx = i + k;
                                    // 미러링 경계 처리
                                    double v = (idx < 0) ? input[-idx]
                                             : (idx >= n) ? input[2 * n - idx - 2]
                                             : input[idx];
                                    sum += sgCoeffs[k + w] * v;
                                }
                                return sum;
                            }

                            return 0;
                        })
                        .ToArray();
                });

                // 4) UI 스레드에서 결과 바인딩
                await AddItemsInBatches(listBox2, results, progressReporter);
                lblCnt2.Text = "Count : " + listBox2.Items.Count;
                btnCopy2.Enabled = btnSelClear2.Enabled = false;
            }
            finally
            {
                progressBar2.Value = 0;
            }
        }

        /// <summary>
        /// windowSize = 2*w + 1, polyOrder ≤ windowSize-1
        /// </summary>
        private static double[] ComputeSavitzkyGolayCoefficients(int windowSize, int polyOrder)
        {
            int m = polyOrder;
            int half = windowSize / 2;
            // Vandermonde 행렬 A (windowSize x (m+1))
            double[,] A = new double[windowSize, m + 1];
            for (int i = -half; i <= half; i++)
            {
                for (int j = 0; j <= m; j++)
                    A[i + half, j] = Math.Pow(i, j);
            }

            // ATA = A^T * A ((m+1)x(m+1))
            double[,] ATA = new double[m + 1, m + 1];
            for (int i = 0; i <= m; i++)
                for (int j = 0; j <= m; j++)
                    for (int k = 0; k < windowSize; k++)
                        ATA[i, j] += A[k, i] * A[k, j];

            // ATA 역행렬(invATA)
            double[,] invATA = InvertMatrix(ATA);

            // AT = A^T ((m+1)xwindowSize)
            double[,] AT = new double[m + 1, windowSize];
            for (int i = 0; i <= m; i++)
                for (int k = 0; k < windowSize; k++)
                    AT[i, k] = A[k, i];

            // 필터 계수 h[k] = sum_j invATA[0,j] * AT[j,k]
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

        /// <summary>
        /// 간단한 가우스 소거법 기반 정방 행렬 역행렬 계산
        /// </summary>
        private static double[,] InvertMatrix(double[,] a)
        {
            int n = a.GetLength(0);
            var aug = new double[n, 2 * n];
            // [A | I]
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    aug[i, j] = a[i, j];
            for (int i = 0; i < n; i++)
                aug[i, n + i] = 1;

            // 가우스 소거
            for (int i = 0; i < n; i++)
            {
                // 피벗
                double pivot = aug[i, i];
                for (int j = 0; j < 2 * n; j++)
                    aug[i, j] /= pivot;

                // 다른 행 제거
                for (int r = 0; r < n; r++)
                {
                    if (r == i) continue;
                    double factor = aug[r, i];
                    for (int c = 0; c < 2 * n; c++)
                        aug[r, c] -= factor * aug[i, c];
                }
            }

            // 결과 I | A^{-1}
            var inv = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    inv[i, j] = aug[i, j + n];
            return inv;
        }

        private int[] CalcBinomialCoefficients(int length)
        {
            if (length < 1)
                throw new ArgumentException("length must be ≥ 1", nameof(length));

            var c = new int[length];
            c[0] = 1;
            for (int i = 1; i < length; i++)
                c[i] = c[i - 1] * (length - i) / i;
            return c;
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
            listBox1.Items.Add(txtVariable.Text);
            lblCnt1.Text = "Count : " + listBox1.Items.Count;
        }

        private void txtVariable_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                btnAdd.PerformClick();
                txtVariable.Text = String.Empty;
            }
        }

        private async void btnSelectAll_Click(object sender, EventArgs e)
        {
            int n = listBox1.Items.Count;
            if (n == 0) return;

            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            listBox1.BeginUpdate();
            listBox1.ClearSelected();

            int reportInterval = Math.Max(1, n / 100);
            for (int i = 0; i < n; i++)
            {
                listBox1.SetSelected(i, true);

                if ((i % reportInterval) == 0)
                {
                    progressBar1.Value = (int)((i / (double)(n - 1)) * 100);
                    await Task.Yield();
                }
            }
            listBox1.EndUpdate();
            progressBar1.Value = 100;
            await Task.Delay(200);
            progressBar1.Value = 0;
        }

        private async void btnClear_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            lblCnt1.Text = "Count : " + listBox1.Items.Count;

            btnCopy.Enabled = false;
            btnSelClear.Enabled = false;
            btnDelete.Enabled = false;

            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            listBox1.BeginUpdate();
            listBox1.ClearSelected();
            listBox1.EndUpdate();

            listBox1.Focus();
            lblCnt2.Text = "Count : " + listBox2.Items.Count;

            progressBar1.Value = 100;
            await Task.Delay(200);
            progressBar1.Value = 0;
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
                progressBar1.Value = 0;
                btnCalibrate.Enabled = true;
            }
        }


        private async void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            // Calibrate 버튼 비활성 + ProgressBar 초기화
            btnCalibrate.Enabled = false;
            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            try
            {
                // 1) 드래그된 원본 텍스트 추출
                string raw = getDropText(e);
                progressBar1.Value = 10;

                if (string.IsNullOrWhiteSpace(raw))
                    return;

                // 2) HTML 태그 제거가 필요하면 백그라운드에서 처리
                if (raw.IndexOf("<html", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    raw = await Task.Run(() =>
                        htmlTagRegex.Replace(raw, " ")
                    );
                    progressBar1.Value = 25;

                    if (string.IsNullOrWhiteSpace(raw))
                        return;
                }

                // 3) 숫자 파싱: 백그라운드 PLINQ
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
                            // 천 단위 구분자 제거 후 파싱
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

                // 4) UI에 배치 추가 + 진행률 리포터
                var progressReporter = new Progress<int>(pct =>
                {
                    pct = Math.Max(0, Math.Min(100, pct));
                    progressBar1.Value = pct;
                    progressBar1.Refresh();
                });

                await AddItemsInBatches(listBox1, parsed, progressReporter);

                // 5) 완료 표시
                progressBar1.Value = 100;
                lblCnt1.Text = "Count : " + listBox1.Items.Count;
                await Task.Delay(200);  // 100% 상태를 잠깐 보여주기
            }
            finally
            {
                // 최종 초기화
                progressBar1.Value = 0;
                btnCalibrate.Enabled = true;
            }
        }

        private void InitProgressBar()
        {
            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
        }

        private async Task AddItemsInBatches(
            ListBox box, double[] items, IProgress<int> progress)
        {
            const int BatchSize = 500;
            int total = items.Length, done = 0;

            box.BeginUpdate();
            box.Items.Clear();

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

                int pct = (int)(done * 100L / total);
                progress.Report(pct);

                Application.DoEvents();
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

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count == 0)
            {
                btnCopy.Enabled = false;
                return;
            }

            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            try
            {
                progressBar1.Value = 10;
                var allItems = listBox1.Items.Cast<object>().ToArray();
                var selIdxSet = listBox1.SelectedIndices.Cast<int>().ToHashSet();

                progressBar1.Value = 30;
                var remaining = await Task.Run(() =>
                    allItems
                        .AsParallel()
                        .WithDegreeOfParallelism(Environment.ProcessorCount)
                        .Where((item, idx) => !selIdxSet.Contains(idx))
                        .ToArray()
                );

                progressBar1.Value = 70;
                listBox1.BeginUpdate();
                listBox1.Items.Clear();
                listBox1.Items.AddRange(remaining);
                listBox1.EndUpdate();

                progressBar1.Value = 100;
                lblCnt1.Text = "Count : " + listBox1.Items.Count;
                await Task.Delay(200);
            }
            finally
            {
                progressBar1.Value = 0;
            }
        }

        private async void btnClear2_Click(object sender, EventArgs e)
        {
            progressBar2.Minimum = 0;
            progressBar2.Maximum = 100;
            progressBar2.Value = 0;

            listBox2.BeginUpdate();
            listBox2.Items.Clear();
            listBox2.EndUpdate();

            progressBar2.Value = 100;
            lblCnt2.Text = "Count : " + listBox2.Items.Count;
            btnCopy2.Enabled = btnSelClear2.Enabled = false;

            await Task.Delay(200);
            progressBar2.Value = 0;
        }

        private async void btnSelectAll2_Click(object sender, EventArgs e)
        {
            int n2 = listBox2.Items.Count;
            if (n2 == 0) return;

            progressBar2.Style = ProgressBarStyle.Continuous;
            progressBar2.Minimum = 0;
            progressBar2.Maximum = 100;
            progressBar2.Value = 0;

            listBox2.BeginUpdate();
            listBox2.ClearSelected();

            int reportInterval = Math.Max(1, n2 / 100);
            for (int i = 0; i < n2; i++)
            {
                listBox2.SetSelected(i, true);

                if ((i % reportInterval) == 0)
                {
                    progressBar2.Value = (int)((i / (double)(n2 - 1)) * 100);
                    await Task.Yield();
                }
            }
            listBox2.EndUpdate();
            progressBar2.Value = 100;
            await Task.Delay(200);
            progressBar2.Value = 0;
        }

        private async void btnSelectClear2_Click(object sender, EventArgs e)
        {
            progressBar2.Minimum = 0;
            progressBar2.Maximum = 100;
            progressBar2.Value = 0;

            listBox2.BeginUpdate();
            listBox2.ClearSelected();
            listBox2.EndUpdate();

            listBox2.Focus();
            lblCnt2.Text = "Count : " + listBox2.Items.Count;

            progressBar2.Value = 100;
            await Task.Delay(200);
            progressBar2.Value = 0;
        }


        private void txtVariable_TextChanged(object sender, EventArgs e)
        {
            if (txtVariable.Text.Length == 0)
            {
                btnAdd.Enabled = false;
            }
            else
            {
                btnAdd.Enabled = true;
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedItems.Count == 0)
            {
                btnCopy2.Enabled = false;
                btnSelClear2.Enabled = false;
            }
            else
            {
                btnCopy2.Enabled = true;
                btnSelClear2.Enabled = true;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count == 0)
            {
                btnCopy.Enabled = false;
                btnSelClear.Enabled = false;
                btnDelete.Enabled = false;
            }
            else
            {
                btnCopy.Enabled = true;
                btnSelClear.Enabled = true;
                btnDelete.Enabled = true;
            }
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete)
            {
                btnDelete.PerformClick();
            }

            if (e.KeyData == (Keys.Control | Keys.Delete))
            {
                btnClear.PerformClick();
            }

            if (e.KeyData == (Keys.Control | Keys.C))
            {
                btnCopy.PerformClick();
            }

            if (e.KeyData == (Keys.Control | Keys.V))
            {
                btnPaste.PerformClick();
            }

            if (e.KeyData == (Keys.Control | Keys.A))
            {
                btnSelectAll.PerformClick();
            }

            if (e.KeyData == Keys.Escape)
            {
                btnSelClear.PerformClick();
            }

            lblCnt1.Text = "Count : " + listBox1.Items.Count;
        }

        private void listBox2_KeyDown(object sender, KeyEventArgs e)
        {
            {

                if (e.KeyData == (Keys.Control | Keys.Delete))
                {
                    btnClear2.PerformClick();
                }

                if (e.KeyData == (Keys.Control | Keys.C))
                {
                    btnCopy2.PerformClick();
                }

                if (e.KeyData == (Keys.Control | Keys.A))
                {
                    btnSelectAll2.PerformClick();
                }

                if (e.KeyData == Keys.Escape)
                {
                    btnSelClear2.PerformClick();
                }

                lblCnt2.Text = "Count : " + listBox2.Items.Count;
            }
        }


        private void btnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(string.Join(Environment.NewLine, listBox1.SelectedItems.Cast<double>().ToArray()));
        }

        private void btnCopy2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(string.Join(Environment.NewLine, listBox2.SelectedItems.Cast<double>().ToArray()));
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
            btnCopy2.Enabled = false;
            btnSelClear2.Enabled = false;
            btnCopy.Enabled = false;
            btnSelClear.Enabled = false;
            btnDelete.Enabled = false;
        }
    }
}

