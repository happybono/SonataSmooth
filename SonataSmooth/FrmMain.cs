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
        // 보정 진행 중인 상태인지 여부에 대한 플래그
        private bool isRefinedLoading = false;

        // 엑셀 제목 입력 TextBox의 기본 안내 문구 (Placeholder)
        private const string ExcelTitlePlaceholder = "Click here to enter a title for your dataset.";

        // 숫자 추출용 정규식
        private static readonly Regex numberRegex = new Regex(
            @"[+-]?\d+(?:,\d{3})*(?:\.\d+)?(?:[eE][+-]?\d+)?",
            RegexOptions.Compiled | RegexOptions.CultureInvariant
            );

        // HTML 태그 제거용 정규식
        private static readonly Regex htmlTagRegex = new Regex(
            @"<.*?>",
            RegexOptions.Compiled | RegexOptions.Singleline
            );

        // 클립보드에서 숫자 추출용 정규식
        private static readonly Regex clipboardRegex = new Regex(
            @"[+-]?(\d+(,\d{3})*|(?=\.\d))((\.\d+([eE][+-]\d+)?)|)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant
            );

        private CancellationTokenSource _ctsInitSelectAll;
        private CancellationTokenSource _ctsRefSelectAll;

        private readonly FrmExportSettings settingsForm;
        private FrmAbout aboutForm;
        private int dataCount;
        private int r;
        private int polyOrder;

        private double dpiX;
        private double dpiY;

        private const int RecommendedMinRadius = 3;
        private const int RecommendedMaxRadius = 7;
        private const int RecommendedMinPolyOrder = 2;
        private const int RecommendedMaxPolyOrder = 6;

        private bool _isShowingTitleValidationMessage;
        private string _lastInvalidTitle;

        public FrmMain()
        {
            InitializeComponent();

            this.KeyPreview = true;

            UpdatelbInitDataBtnsState(null, EventArgs.Empty);
            UpdatelbRefinedDataBtnsState(null, EventArgs.Empty);
            settingsForm = new FrmExportSettings(this);
            aboutForm = null;
        }

        private struct SmoothingResult
        {
            public bool Success { get; }
            public string Error { get; }
            private SmoothingResult(bool success, string error)
            {
                Success = success;
                Error = error;
            }
            public static SmoothingResult Ok() => new SmoothingResult(true, null);
            public static SmoothingResult Fail(string error) => new SmoothingResult(false, error);
        }

        private void ShowError(string title, string message)
        {
            MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// lbInitData 의 모든 항목을 double 형식의 배열로 파싱합니다.
        /// 항목이 null 이거나 숫자 변환에 실패하면 MessageBox 로 오류를 표시하고 false 값을 반환합니다.
        /// </summary>
        private SmoothingResult TryParseInputData(out double[] input, out int n)
        {
            n = lbInitData.Items.Count;
            input = new double[n];
            for (int i = 0; i < n; i++)
            {
                var item = lbInitData.Items[i];
                if (item == null)
                {
                    return SmoothingResult.Fail($"The item at index {i} is null.");
                }
                string s = item.ToString();
                if (!double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out input[i]))
                {
                    return SmoothingResult.Fail($"Failed to convert item at index {i} : \"{s}\"");
                }
            }
            return SmoothingResult.Ok();
        }

        /// <summary>
        /// 커널 반경과 다항식 차수를 ComboBox 에서 파싱합니다.
        /// 파싱 실패 시 MessageBox 로 오류를 알리고 false 값을 반환합니다.
        /// Gaussian 보정 방식에 활용될 sigma 값도 계산합니다.
        /// </summary>
        private SmoothingResult TryParseParameters(out int r, out int polyOrder, out double sigma)
        {
            r = 0;
            polyOrder = 0;
            sigma = 0;
            if (!int.TryParse(cbxKernelRadius.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out r))
            {
                return SmoothingResult.Fail($"Failed to parse kernel radius : \"{cbxKernelRadius.Text}\".");
            }
            if (!int.TryParse(cbxPolyOrder.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out polyOrder))
            {
                return SmoothingResult.Fail($"Failed to parse polynomial order : \"{cbxPolyOrder.Text}\".");
            }
            sigma = (2.0 * r + 1) / 6.0;
            return SmoothingResult.Ok();
        }

        /// <summary>
        /// "Calibrate" 버튼 클릭 시 호출되는 이벤트 핸들러.
        /// 초기 데이터를 기준으로 선택한 보정 필터를 적용해 각각의 값들을 보정하고, 결과를 Refined Dataset에 표시합니다.
        /// 각 단계별로 상세한 예외 처리와 진행률 표시, UI 상태 갱신을 수행합니다.
        /// </summary>
        private async void btnCalibrate_Click(object sender, EventArgs e)
        {
            // Export 설정 폼에 현재 커널 반경 / 다항식 차수 값 동기화
            settingsForm.ApplyParameters(cbxKernelRadius.Text, cbxPolyOrder.Text);

            // ProgressBar 초기화
            pbMain.Style = ProgressBarStyle.Continuous;
            pbMain.Minimum = 0;
            pbMain.Maximum = 100;
            pbMain.Value = 0;

            // 데이터가 없으면 즉시 반환
            if (lbInitData.Items.Count == 0)
                return;

            try
            {
                //  ListBox의 데이터를 double[] 형식으로 파싱 (실패 시 반환)
                var parseInputResult = TryParseInputData(out double[] input, out int n);
                if (!parseInputResult.Success)
                {
                    ShowError("Input Parse Error", parseInputResult.Error);
                    return;
                }

                // 입력된 Parameters (커널 반경, 다항식 차수, Gaussian 표준편차) 파싱 (실패 시 반환)
                var parseParamResult = TryParseParameters(out int r, out int polyOrder, out double sigma);
                if (!parseParamResult.Success)
                {
                    ShowError("Parameter Parse Error", parseParamResult.Error);
                    return;
                }

                // Parameters 유효성 검사 (윈도우 크기 및 다항식 차수) (실패 시 반환)
                var validateResult = ValidateSmoothingParameters(lbInitData.Items.Count, r, polyOrder);
                if (!validateResult.Success)
                {
                    ShowError("Parameter Validation Error", validateResult.Error);
                    return;
                }

                // UI 상태 갱신 및 진행률 표시 준비 (각각의 보정 방식 CheckBox 체크 여부 확인)
                bool useRect = rbtnRect.Checked;
                bool useMed = rbtnMed.Checked;
                bool useAvg = rbtnAvg.Checked;
                bool useSG = rbtnSG.Checked;
                bool useGauss = rbtnGauss.Checked;

                // 이항 계수 커널 계산 (실패 시 반환)
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

                // Gaussian 커널 계산 (실패 시 반환)
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

                // 진행률 보고자 설정 (0 ~ 100% 값 클램프) 및 비동기 작업 시작
                var progressReporter = new Progress<int>(pct =>
                {
                    pbMain.Value = Math.Max(pbMain.Minimum, Math.Min(pbMain.Maximum, pct));
                });

                double[] results;
                try
                {
                    // 실제 스무딩 계산 (병렬 처리, 선택된 필터에 따라 분기)
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

                        // 각 데이터 포인트별로 선택된 필터 적용
                        return ParallelEnumerable
                            .Range(0, n)
                            .AsOrdered()
                            .WithDegreeOfParallelism(Environment.ProcessorCount)
                            .Select(i =>
                            {
                                try
                                {
                                    // 경계 값 처리 함수 (Mirroring)
                                    int Mirror(int idx)
                                    {
                                        if (idx < 0) return -idx - 1;
                                        if (idx >= n) return 2 * n - idx - 1;
                                        return idx;
                                    }

                                    // 각 필터별 계산 분기
                                    // 직사각형 평균 (단순 이동 평균)
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

                                    // 이항계수 가중 이동 중간 값 (이상치 제거에 가장 효과적인 방식)
                                    else if (useMed)
                                    {
                                        return WeightedMedianAt(input, i, r, binom);
                                    }

                                    // 이항계수 가중 이동 평균
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

                                    // Savitzky-Golay (사비츠키-골레이) 보정
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

                                    // Gaussian (가우시안) 보정
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

                                    // 어떠한 필터도 선택되지 않은 경우 0.0 반환
                                    return 0.0;
                                }
                                catch (Exception ex)
                                {
                                    // 예외 발생 시 (개별 Index 오류 발생) 해당 Index 에 대해 0.0 반환하고 로그 출력
                                    Debug.WriteLine($"[Parallel Select] Index {i} error: {ex}");
                                    return 0.0;
                                }
                            })
                            .ToArray();
                    });
                }
                catch (AggregateException aex)
                {
                    // 병렬 처리 중 예외 발생 시 오류 메시지
                    MessageBox.Show(
                        $"An error occurred during parallel computation : {aex.Flatten().InnerException?.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error
                    );
                    UpdatelbRefinedDataBtnsState(null, EventArgs.Empty);
                    return;
                }
                catch (Exception ex)
                {
                    // 기타 예외 발생 시 오류 메시지
                    MessageBox.Show($"An unexpected error occurred during computation : {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    UpdatelbRefinedDataBtnsState(null, EventArgs.Empty);
                    return;
                }

                try
                {
                    // 계산된 결과를 Refined Dataset ListBox에 비동기적으로 추가
                    // UI 상태 갱신 및 진행률 표시
                    lbRefinedData.BeginUpdate();
                    lbRefinedData.Items.Clear();

                    // 보정 진행 중 버튼 비활성화
                    btnCalibrate.Enabled = false;
                    btnInitClear.Enabled = false;
                    btnInitEdit.Enabled = false;
                    btnInitPaste.Enabled = false;
                    btnInitDelete.Enabled = false;
                    btnInitSelectAll.Enabled = false;
                    btnInitSelectSync.Enabled = false;
                    btnRefClear.Enabled = false;
                    btnRefSelectSync.Enabled = false;
                    btnRefSelectAll.Enabled = false;

                    isRefinedLoading = true;     // 로딩 시작
                    await AddItemsInBatches(lbRefinedData, results, progressReporter, 60);
                    isRefinedLoading = false;    // 로딩 종료

                    lbRefinedData.EndUpdate();
                    lblRefCnt.Text = $"Count : {lbRefinedData.Items.Count}";

                    // 보정 방식 및 Parameter 정보 표시
                    slblCalibratedType.Text = useRect ? "Rectangular Average"
                                         : useMed ? "Weighted Median"
                                         : useAvg ? "Binomial Average"
                                         : useSG ? "Savitzky-Golay Filter"
                                         : useGauss ? "Gaussian Filter"
                                                    : "Unknown";

                    // 커널 반경 표시
                    slblKernelRadius.Text = r.ToString();

                    // 다항식 차수 표시 (Savitzky-Golay 보정 방식을 선택한 경우에만 해당)
                    bool showPoly = useSG;
                    tlblPolyOrder.Visible = showPoly;
                    tlblSeparator2.Visible = showPoly;
                    slblPolyOrder.Visible = showPoly;
                    if (showPoly) slblPolyOrder.Text = polyOrder.ToString();
                }
                catch (Exception ex)
                {
                    // 결과 바인딩 중 예외 발생 시 오류 메시지
                    MessageBox.Show($"Error binding results : {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    isRefinedLoading = false;
                }
            }
            finally
            {
                // 최종적으로 비활성화 처리했던 모든 버튼 상태 활성화 (정상화) 및 UI 상태 복원
                slblDesc.Visible = false;
                btnCalibrate.Enabled = true;
                btnInitClear.Enabled = true;
                btnInitEdit.Enabled = true;
                btnInitSelectAll.Enabled = true;
                btnInitDelete.Enabled = true;
                btnInitSelectSync.Enabled = true;
                btnRefClear.Enabled = true;
                btnRefSelectAll.Enabled = true;
                btnRefSelectSync.Enabled = true;

                UpdatelbInitDataBtnsState(null, EventArgs.Empty);
                UpdatelbRefinedDataBtnsState(null, EventArgs.Empty);
            }

            // ProgressBar 초기화
            pbMain.Value = 0;
        }

        // 입력된 데이터에 대해 다양한 보정 방식 (스무딩 필터) 을 적용하는 Method
        private (double[] Rect, double[] Binom, double[] Median, double[] Gauss, double[] SG)
            ApplySmoothing(double[] input, int r, int polyOrder, bool doRect, bool doAvg, bool doMed, bool doGauss, bool doSG)
        {
            int n = input.Length; // 데이터 개수
            int[] binom = CalcBinomialCoefficients(2 * r + 1);  // 이항 계수 계산
            double sigma = (2.0 * r + 1) / 6.0;                 // 가우시안 보정 방식의 표준편차 계산

            // 각 필터의 계수 (Kernel) 계산
            double[] gaussCoeffs = doGauss ? ComputeGaussianCoefficients(2 * r + 1, sigma) : null;
            double[] sgCoeffs = doSG ? ComputeSavitzkyGolayCoefficients(2 * r + 1, polyOrder) : null;

            // 결과 저장 배열 초기화
            var rect = new double[n];
            var binomAvg = new double[n];
            var median = new double[n];
            var gauss = new double[n];
            var sg = new double[n];

            // 경계 값 처리를 위한 미러링 함수 (Index 가 범위를 벗어날 경우 대칭 반사)
            int Mirror(int idx) => idx < 0 ? -idx - 1 : idx >= n ? 2 * n - idx - 1 : idx;

            // 병렬 처리로 각각의 데이터 항목에 대해 보정 
            Parallel.For(0, n, i => {
                double sum; int cnt;

                // 직사각형 평균 (단순 이동 평균)
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

                // 이항계수 가중 이동 평균
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

                // 이항계수 가중 이동 중간 값 (이상치 제거에 가장 효과적인 방식)
                if (doMed) median[i] = WeightedMedianAt(input, i, r, binom);

                // Gaussian (가우시안) 보정
                if (doGauss && gaussCoeffs != null)
                {
                    sum = 0;
                    for (int k = -r; k <= r; k++)
                        sum += gaussCoeffs[k + r] * input[Mirror(i + k)];
                    gauss[i] = sum;
                }

                // Savitzky-Golay (사비츠키-골레이) 보정
                if (doSG && sgCoeffs != null)
                {
                    sum = 0;
                    for (int k = -r; k <= r; k++)
                        sum += sgCoeffs[k + r] * input[Mirror(i + k)];
                    sg[i] = sum;
                }
            });

            // 각 필터별 결과 배열 반환
            return (rect, binomAvg, median, gauss, sg);
        }

        // Gaussian 보정 방식 : 계수 계산 Method
        private static double[] ComputeGaussianCoefficients(int length, double sigma)
        {
            if (length < 1)
                throw new ArgumentException("length must be ≥ 1", nameof(length));
            if (sigma <= 0)
                throw new ArgumentException("sigma must be > 0", nameof(sigma));

            var coeffs = new double[length];        // 계수 배열
            int w = (length - 1) / 2;               // 커널 중심
            double twoSigmaSq = 2 * sigma * sigma;  // 분산 관련 값
            double sum = 0.0;                       // 정규화를 위한 합계

            // 각 위치별 Gaussian 함수 적용
            for (int i = 0; i < length; i++)
            {
                int x = i - w;
                coeffs[i] = Math.Exp(-(x * x) / twoSigmaSq);
                sum += coeffs[i];
            }
            if (sum <= 0)
                throw new InvalidOperationException("Gaussian kernel sum is zero or negative.");

            // 전체 합이 1 이 되도록 정규화
            for (int i = 0; i < length; i++)
                coeffs[i] /= sum;

            return coeffs;
        }

        // 이항계수 가중 이동 중간 값 계산 Method
        private static double WeightedMedianAt(double[] data, int center, int w, int[] binom)
        {
            var pairs = new List<(double Value, int Weight)>(2 * w + 1);
            for (int k = -w; k <= w; k++)
            {
                int idx = center + k;
                if (idx < 0 || idx >= data.Length) continue; // 경계 확인
                pairs.Add((data[idx], binom[k + w]));
            }
            if (pairs.Count == 0)
                return 0;                                    // 유효한 데이터가 없으면 0 반환

            // 값 기준 오름차순 정렬
            pairs.Sort((a, b) => a.Value.CompareTo(b.Value));

            long totalWeight = pairs.Sum(p => p.Weight);     // 전체 가중치 합산
            long half = totalWeight / 2;                     // 절반 가중치
            bool isEvenTotal = (totalWeight % 2 == 0);       // 전체 가중치가 짝수인지 여부

            long accum = 0;                                  // 누적 가중치
            for (int i = 0; i < pairs.Count; i++)
            {
                accum += pairs[i].Weight;

                // 누적 가중치 > 절반 : 누적 가중치가 절반을 넘으면 해당 값 반환
                if (accum > half)
                {
                    return pairs[i].Value;
                }

                // 누적 가중치 = 절반인 순간 : 누적 가중치가 정확하게 절만인 경우, 다음 값과 평균 반환 (짝수 개일 때의 중간값 처리)
                if (isEvenTotal && accum == half)
                {
                    // 안전하게 Bounds 체크
                    double nextVal = (i + 1 < pairs.Count)
                                     ? pairs[i + 1].Value
                                     : pairs[i].Value;
                    return (pairs[i].Value + nextVal) / 2.0;
                }
            }

            // 모든 가중치 합산 후에도 못 찾았다면 최대 값 반환
            return pairs[pairs.Count - 1].Value;
        }

        /// <summary>
        /// 주어진 길이의 이항계수(파스칼 삼각형의 한 행)를 계산하여 반환하는 메서드입니다.
        /// 예를 들어 length=5일 때, [1, 4, 6, 4, 1]을 반환합니다.
        /// 이 계수는 스무딩 필터(특히 Binomial Average, Weighted Median 등)에서 가중치로 사용됩니다.
        /// </summary>

        /// <param name="length">커널(윈도우)의 크기. 반드시 1 이상이어야 합니다.</param>
        /// <returns>이항계수 배열(길이 length)</returns>
       
        private static int[] CalcBinomialCoefficients(int length)
        {
            // 커널 길이가 1 미만이면 예외 발생 (유효성 검사)
            if (length < 1)
                throw new ArgumentException("length must be ≥ 1", nameof(length));

            var c = new int[length];                    // 결과 이항계수 배열
            c[0] = 1;                                   // 첫 번째 계수는 항상 1 (nC0)

            // 파스칼 삼각형의 재귀적 공식으로 각 계수 계산
            for (int i = 1; i < length; i++)            
                c[i] = c[i - 1] * (length - i) / i;     // c[i] = c[i - 1] × (n - i) / i;  (n = length - 1)
            return c;                                   // 계산된 이항계수 배열 반환
        }

        // Savitzky-Golay 보정 방식 : 계수 계산 Method
        private static double[] ComputeSavitzkyGolayCoefficients(int windowSize, int polyOrder)
        {
            int m = polyOrder;                              // 다항식 차수
            int half = windowSize / 2;                      // 커널 반경
            double[,] A = new double[windowSize, m + 1];    // Vandermonde 설계 행렬

            // 각 행 / 열에 대해 x^j 값 채우기
            for (int i = -half; i <= half; i++)
                for (int j = 0; j <= m; j++)
                    A[i + half, j] = Math.Pow(i, j);

            // ATA = A^T * A 계산
            var ATA = new double[m + 1, m + 1];
            for (int i = 0; i <= m; i++)
                for (int j = 0; j <= m; j++)
                    for (int k = 0; k < windowSize; k++)
                        ATA[i, j] += A[k, i] * A[k, j];

            // ATA 역행렬 계산
            var invATA = InvertMatrix(ATA);

            // A^T 계산
            // A^T 는 m + 1 x windowSize 크기
            var AT = new double[m + 1, windowSize];
            for (int i = 0; i <= m; i++)
                for (int k = 0; k < windowSize; k++)
                    AT[i, k] = A[k, i];

            // Savitzky-Golay 커널 계산
            // h = (A^T A)^(-1) A^T 의 첫 번째 행이 중앙 위치의 계수
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

        // 행렬 역행렬 계산 Method (가우스-조던 소거법 [Gauss-Jordan Elimination] + 피벗팅 [Pivoting] + 상대 임계치)
        private static double[,] InvertMatrix(double[,] a)
        {
            int n = a.GetLength(0);             // 행렬 크기
            var aug = new double[n, 2 * n];     // 확장 행렬 (a | I) : a 는 원본 행렬 | I 는 단위 행렬

            // 확장 행렬 초기화
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    aug[i, j] = a[i, j];
                aug[i, n + i] = 1;
            }

            // 가우스-조던 소거법 [Gauss-Jordan Elimination]
            for (int i = 0; i < n; i++)
            {
                // 현재 열 (i) 에서 절대값이 가장 큰 행 찾기 (수치 안정성)
                int maxRow = i;
                for (int k = i + 1; k < n; k++)
                {
                    if (Math.Abs(aug[k, i]) > Math.Abs(aug[maxRow, i]))
                        maxRow = k;
                }
                // maxRow 와 i 행 교환
                if (maxRow != i)
                {
                    for (int k = 0; k < 2 * n; k++)
                    {
                        double temp = aug[i, k];
                        aug[i, k] = aug[maxRow, k];
                        aug[maxRow, k] = temp;
                    }
                }

                double pivot = aug[i, i];     // Pivot 값

                // 상대 임계치 로직 (수치적 안정성 강화 및 특이 행렬 방지)
                // 현재 행의 최대 절대값 Scale 계산
                double rowScale = 0.0;
                for (int j = i; j < n; j++)
                    rowScale = Math.Max(rowScale, Math.Abs(aug[i, j]));

                // 상대 임계치 : Scale × 1e-12 와 Machine ε 를 비교했을 때 더 큰 값
                double tol = Math.Max(rowScale * 1e-12, Double.Epsilon);

                if (Math.Abs(pivot) < tol)
                {
                    // 특이 행렬 처리 : 빈 (0) 행렬 반환 또는 예외 출력
                    return new double[n, n];
                    // throw new InvalidOperationException("Matrix is singular; cannot invert.");
                }

                // Pivot 행 정규화
                for (int j = 0; j < 2 * n; j++)
                    aug[i, j] /= pivot;

                // 다른 모든 행에서 현재 Pivot 열 (i) 제거
                for (int r = 0; r < n; r++)
                {
                    if (r == i) continue;
                    double factor = aug[r, i];
                    for (int c = 0; c < 2 * n; c++)
                        aug[r, c] -= factor * aug[i, c];
                }
            }

            // 역행렬 추출
            var inv = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    inv[i, j] = aug[i, j + n];

            return inv;
        }

        // 입력된 파라미터의 유효성 검사 Method (윈도우 크기 및 다항식 차수)
        private SmoothingResult ValidateSmoothingParameters(int dataCount, int w, int polyOrder)
        {
            int windowSize = 2 * w + 1;
            bool useSG = rbtnSG != null && rbtnSG.Checked;

            // 윈도우 크기가 데이터 개수보다 큰 경우 오류 메시지 출력
            if (windowSize > dataCount)
            {
                var msg =
                    "Kernel radius is too large.\n\n" +
                    $"Window size formula : (2 × radius) + 1\n" +
                    $"Current : (2 × {w}) + 1 = {windowSize}\n" +
                    $"Data count : {dataCount}\n\n" +
                    "Rule : windowSize ≤ dataCount";
                return SmoothingResult.Fail(msg);
            }

            // 다항식 차수가 윈도우 크기보다 크거나 같은 경우 오류 메시지 출력
            if (useSG && polyOrder >= windowSize)
            {
                var msg =
                    "Polynomial order must be smaller than the window size.\n\n" +
                    $"Rule : polyOrder < windowSize\n" +
                    $"Current polyOrder : {polyOrder}\n" +
                    $"Window size      : {windowSize}\n\n" +
                    "Tip : windowSize = (2 × radius) + 1";
                return SmoothingResult.Fail(msg);
            }

            return SmoothingResult.Ok(); // 모든 조건 만족 후 통과 시 Ok 반환
        }

        private void UpdateStatusLabel(int beforeCount)
        {
            int added = lbInitData.Items.Count - beforeCount;

            if (added == 0)
            {
                slblDesc.Text = "No items have been added Initial Dataset.";
            }
            else if (added == 1)
            {
                slblDesc.Text = "1 item has been added Initial Dataset.";
            }
            else
            {
                slblDesc.Text = $"{added} items have been added to Initial Dataset.";
            }

            slblDesc.Visible = true;
        }



        private void btnInitAdd_Click(object sender, EventArgs e)
        {
            if (double.TryParse(txtInitAdd.Text, out double value))
            {
                lbInitData.Items.Add(value);
                lblInitCnt.Text = "Count : " + lbInitData.Items.Count;
                slblDesc.Visible = true;
                slblDesc.Text = $"Value '{value}' has been added to Initial Dataset.";
            }
            else
            {
                txtInitAdd.Focus();
                txtInitAdd.SelectAll();
            }

            if (lbInitData.Items.Count > 0)
            {
                UpdatelbInitDataBtnsState(null, EventArgs.Empty);
                UpdatelbRefinedDataBtnsState(null, EventArgs.Empty);
            }
            else
            {
                return;
            }

            txtInitAdd.Text = String.Empty;
        }

        private void txtInitAdd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter && btnInitAdd.Enabled)
            {
                btnInitAdd.PerformClick();
                txtInitAdd.Text = String.Empty;
                e.SuppressKeyPress = true;
            }
        }

        private async void btnInitSelectAll_Click(object sender, EventArgs e)
        {
            _ctsInitSelectAll?.Cancel(); // 이전에 진행 중인 작업 중단
            _ctsInitSelectAll = new CancellationTokenSource();
            var token = _ctsInitSelectAll.Token;

            int n = lbInitData.Items.Count;
            if (n == 0) return;

            pbMain.Style = ProgressBarStyle.Continuous;
            pbMain.Minimum = 0;
            pbMain.Maximum = 100;
            pbMain.Value = 0;

            lbInitData.BeginUpdate();
            lbInitData.ClearSelected();

            if (n == 1)
            {
                lbInitData.SetSelected(0, true);
                pbMain.Value = 100;
                lbInitData.EndUpdate();
                lbInitData.Focus();
                UpdatelbInitDataBtnsState(null, EventArgs.Empty);
                await Task.Delay(200);
                pbMain.Value = 0;
                return;
            }

            int reportInterval = Math.Max(1, n / 100);
            int yieldInterval = Math.Max(1, n / 1000);

            try
            {
                for (int i = 0; i < n; i++)
                {
                    if (token.IsCancellationRequested) break;

                    lbInitData.SetSelected(i, true);

                    if (i % reportInterval == 0)
                    {
                        double ratio = (i + 1) / (double)n;
                        int pct = (int)Math.Round(ratio * 100.0);
                        pbMain.Value = Math.Min(100, Math.Max(0, pct));
                    }

                    if (i % yieldInterval == 0)
                        await Task.Yield();
                }
            }
            finally
            {
                lbInitData.EndUpdate();
            }

            pbMain.Value = 100;
            lbInitData.Focus();
            UpdatelbInitDataBtnsState(null, EventArgs.Empty);
            await Task.Delay(200);
            pbMain.Value = 0;
        }

        private async void btnInitClear_Click(object sender, EventArgs e)
        {
            int itemCount = lbInitData.Items.Count;
            int refItemCount = lbRefinedData.Items.Count;

            string itemText = itemCount == 1 ? "item" : "items";
            string refItemText = refItemCount == 1 ? "item" : "items";

            string refMessage = refItemCount == 0
                ? string.Empty
                : $"This will also delete all {refItemCount} {refItemText} from the Refined Dataset.";

            string message;

            if (string.IsNullOrEmpty(refMessage))
            {
                message = $@"You are about to delete all {itemCount} {itemText} from the Initial Dataset.

Are you sure you want to proceed?";
            }
            else
            {
                message = $@"You are about to delete all {itemCount} {itemText} from the Initial Dataset.
{refMessage}

Are you sure you want to proceed?";
            }

            DialogResult result = MessageBox.Show(
                message,
                "Delete Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );
        
            if (result == DialogResult.No)
            {
                return;
            }

            pbMain.Style = ProgressBarStyle.Continuous;
            pbMain.Minimum = 0;
            pbMain.Maximum = 100;
            pbMain.Value = 0;

            lbInitData.BeginUpdate();
            lbInitData.ClearSelected();
            lbInitData.Items.Clear();
            lbRefinedData.Items.Clear();
            lbInitData.EndUpdate();

            await Task.Yield();

            lblInitCnt.Text = $"Count : {lbInitData.Items.Count}";
            lblRefCnt.Text = $"Count : {lbInitData.Items.Count}";

            pbMain.Value = 100;
            pbMain.Refresh();

            await Task.Yield();
            pbMain.Value = 0;

            UpdatelbInitDataBtnsState(null, EventArgs.Empty);
            UpdatelbRefinedDataBtnsState(null, EventArgs.Empty);

            slblCalibratedType.Text = "--";
            slblKernelRadius.Text = "--";
            tlblSeparator2.Visible = false;
            tlblPolyOrder.Visible = false;
            slblPolyOrder.Visible = false;
            slblPolyOrder.Text = "--";

            slblDesc.Visible = true;

            string initialMsg = $"Deleted {itemCount} item{(itemCount != 1 ? "s" : "")} from the initial dataset";

            string finalMsg = initialMsg;
            if (refItemCount > 0)
            {
                finalMsg += $" and Deleted {refItemCount} item{(refItemCount != 1 ? "s" : "")} from the Refined Dataset";
            }

            // 마침표 추가
            slblDesc.Text = finalMsg + ".";

            slblDesc.Visible = true;
            txtDatasetTitle.Text = ExcelTitlePlaceholder;
            txtDatasetTitle.TextAlign = HorizontalAlignment.Center;
            txtDatasetTitle.ForeColor = Color.Gray;
            txtInitAdd.Text = string.Empty;
            txtInitAdd.Select();
        }

        public void SetComboValues(string kernelRadius, string polyOrder)
        {
            cbxKernelRadius.Text = kernelRadius;
            cbxPolyOrder.Text = polyOrder;
        }


        private async void btnInitPaste_Click(object sender, EventArgs e)
        {
            pbMain.Style = ProgressBarStyle.Continuous;
            pbMain.Minimum = 0;
            pbMain.Maximum = 100;
            pbMain.Value = 0;
            btnCalibrate.Enabled = false;

            int beforeCount = lbInitData.Items.Count; // 붙여넣기 전 개수
            int addedCount = 0; // 추가된 개수 저장

            try
            {
                string text = Clipboard.GetText();
                pbMain.Value = 10;

                var matches = clipboardRegex.Matches(text)
                    .Cast<Match>()
                    .Where(m => !string.IsNullOrEmpty(m.Value))
                    .ToArray();

                pbMain.Value = 30;

                double[] values = await Task.Run(() =>
                    matches
                        .AsParallel()
                        .WithDegreeOfParallelism(Environment.ProcessorCount)
                        .Select(m => double.Parse(
                            m.Value,
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture))
                        .ToArray()
                );

                pbMain.Value = 70;

                if (values.Length == 0) return;

                addedCount = values.Length; // 여기서 추가 개수 확정

                lbInitData.BeginUpdate();
                lbInitData.Items.AddRange(values.Cast<object>().ToArray());
                lbInitData.EndUpdate();
                lbInitData.TopIndex = lbInitData.Items.Count - 1;

                pbMain.Value = 100;
                lblInitCnt.Text = $"Count : {lbInitData.Items.Count}";

                await Task.Delay(200);
            }
            finally
            {
                UpdatelbInitDataBtnsState(null, EventArgs.Empty);
                UpdatelbRefinedDataBtnsState(null, EventArgs.Empty);
                pbMain.Value = 0;
                btnCalibrate.Enabled = true;

                UpdateStatusLabel(beforeCount);
            }
        }

        private void lbInitData_DragEnter(object sender, DragEventArgs e)
        {
            string[] availableFormats = e.Data.GetFormats();
            int beforeCount = lbInitData.Items.Count;

            if (e.Data.GetDataPresent("Text"))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private async void lbInitData_DragDrop(object sender, DragEventArgs e)
        {
            int beforeCount = lbInitData.Items.Count;
            UpdatelbInitDataBtnsState(null, EventArgs.Empty);
            pbMain.Style = ProgressBarStyle.Continuous;
            pbMain.Minimum = 0;
            pbMain.Maximum = 100;
            pbMain.Value = 0;

            try
            {
                string raw = GetDropText(e);
                pbMain.Value = 10;

                if (string.IsNullOrWhiteSpace(raw))
                    return;

                if (raw.IndexOf("<html", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    raw = await Task.Run(() =>
                        htmlTagRegex.Replace(raw, " ")
                    );
                    pbMain.Value = 25;

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
                pbMain.Value = 60;

                if (parsed.Length == 0)
                    return;

                // 진행률 전달자 : 60 ~ 100 사이 값만 처리하도록 설정
                int baseProgress = 60;
                var progressReporter = new Progress<int>(pct =>
                {
                    int adjustedPct = Math.Max(baseProgress, Math.Min(100, pct));
                    pbMain.Value = adjustedPct;
                    pbMain.Refresh();
                });

                await AddItemsInBatches(lbInitData, parsed, progressReporter, baseProgress);

                pbMain.Value = 100;
                lblInitCnt.Text = "Count : " + lbInitData.Items.Count;
                await Task.Delay(200);
            }
            finally
            {
                bool hasItems = lbInitData.Items.Count > 0;
                btnInitCopy.Enabled = hasItems;
                btnInitDelete.Enabled = hasItems;

                pbMain.Value = 0;
                
                UpdatelbInitDataBtnsState(null, EventArgs.Empty);
                UpdateStatusLabel(beforeCount);
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

        private string GetDropText(DragEventArgs e)
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

        private async void btnInitSelectClr_Click(object sender, EventArgs e)
        {
            int deselectedCount = lbInitData.SelectedIndices.Count;
            await ClearSelectionWithProgress(lbInitData, pbMain, lblInitCnt);

            slblDesc.Text = $"Deselected {deselectedCount} selected item{(deselectedCount != 1 ? "s" : "")} from Initial Dataset.";
            slblDesc.Visible = true;

            UpdatelbInitDataBtnsState(null, EventArgs.Empty);
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
                progressBar.Value = 0;
                listBox.EndUpdate();
            }
        }

        private async void btnInitDelete_Click(object sender, EventArgs e)
        {
            int totalCount = lbInitData.Items.Count;
            int refinedCount = lbRefinedData.Items.Count;
            int selectedCount = lbInitData.SelectedItems.Count;

            string totalItemText = totalCount == 1 ? "item" : "items";
            string selectedItemText = selectedCount == 1 ? "item" : "items";
            string refItemText = refinedCount == 1 ? "item" : "items";

            string refMessage = refinedCount == 0
                ? string.Empty
                : $"This will also delete all {refinedCount} {refItemText} from the Refined Dataset.";

            string body;
            if (selectedCount == 0)
            {
                body = "No items selected to delete.";
            }
            else if (selectedCount == totalCount)
            {
                body = $"You are about to delete all {selectedCount} {totalItemText} from the Initial Dataset."
                     + (refMessage == string.Empty
                         ? ""
                         : $"\n{refMessage}");
            }
            else
            {
                body = $"You are about to delete {selectedCount} selected {selectedItemText} from the Initial Dataset.";
            }

            string message = body + "\n\nAre you sure you want to proceed?";

            var result = MessageBox.Show(
                message,
                "Delete Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.No)
                return;

            // 전체 삭제 시
            if (selectedCount == lbInitData.Items.Count)
            {
                lbInitData.Items.Clear();
                lbRefinedData.Items.Clear();
                lblInitCnt.Text = $"Count : {lbInitData.Items.Count}";
                lblRefCnt.Text = $"Count : {lbRefinedData.Items.Count}";
                pbMain.Value = 0;
                lbInitData.Select();

                txtDatasetTitle.Text = ExcelTitlePlaceholder;
                txtDatasetTitle.TextAlign = HorizontalAlignment.Center;
                txtDatasetTitle.ForeColor = Color.Gray;

                UpdatelbInitDataBtnsState(null, EventArgs.Empty);
                txtInitAdd.Select();
                return;
            }

            await DeleteSelectedItemsPreserveSelection(lbInitData, pbMain, lblInitCnt);

            slblDesc.Visible = true;

            string descMessage;

            if (selectedCount == totalCount)
            {
                string refPart = refinedCount == 0
                    ? string.Empty
                    : $" and all {refinedCount} {refItemText} from Refined Dataset";

                descMessage = $"Deleted all {totalCount} {totalItemText} from Initial Dataset{refPart}.";
            }
            else
            {
                descMessage = $"Deleted {selectedCount} selected {selectedItemText} from Initial Dataset.";
            }

            slblDesc.Visible = true;
            slblDesc.Text = descMessage;

            lblInitCnt.Text = $"Count : {lbInitData.Items.Count}";
            lbInitData.Select();
            UpdatelbInitDataBtnsState(null, EventArgs.Empty);
            UpdatelbRefinedDataBtnsState(null, EventArgs.Empty);
        }

        private void UpdatelbInitDataBtnsState(object s, EventArgs e)
        {
            bool hasItems = lbInitData.Items.Count > 0;
            bool hasSelection = lbInitData.SelectedItems.Count > 0;
            bool canSync = hasSelection && lbInitData.Items.Count == lbRefinedData.Items.Count && lbInitData.Items.Count > 0;

            btnInitAdd.Enabled = txtInitAdd.Text.Length > 0 && double.TryParse(txtInitAdd.Text, out _);
            btnInitCopy.Enabled = hasItems;
            btnInitEdit.Enabled = hasSelection;
            btnCalibrate.Enabled = hasItems;
            btnExport.Enabled = hasItems
                && !string.IsNullOrWhiteSpace(txtDatasetTitle.Text)
                && txtDatasetTitle.Text != ExcelTitlePlaceholder;
            btnInitDelete.Enabled = hasSelection;
            btnInitClear.Enabled = hasItems;
            btnInitSelectClr.Enabled = hasSelection;
            btnInitSelectAll.Enabled = hasItems;
            btnInitSelectSync.Enabled = canSync;

            if (!hasItems)
            {
                btnCalibrate.Enabled = false;
                btnInitDelete.Enabled = false;
                btnInitClear.Enabled = false;
                btnInitSelectClr.Enabled = false;
                btnInitSelectSync.Enabled = false;
                lbInitData.ClearSelected();
            }
            else
            {
                btnCalibrate.Enabled = true;
                btnInitDelete.Enabled = hasSelection;
                btnInitClear.Enabled = hasItems;
                btnInitSelectClr.Enabled = hasSelection;
                btnInitSelectSync.Enabled = canSync;
            }
        }

        private void UpdatelbRefinedDataBtnsState(object s, EventArgs e)
        {
            bool hasItems = lbRefinedData.Items.Count > 0;
            bool hasSelection = lbRefinedData.SelectedItems.Count > 0;
            bool canSync = hasSelection && lbRefinedData.Items.Count == lbInitData.Items.Count && lbRefinedData.Items.Count > 0;

            btnRefCopy.Enabled = hasItems;
            btnRefClear.Enabled = hasItems;
            btnRefSelectClr.Enabled = hasSelection;
            btnRefSelectAll.Enabled = hasItems;
            btnRefSelectSync.Enabled = canSync;

            if (!hasItems)
            {
                btnRefCopy.Enabled = false;
                btnRefClear.Enabled = false;
                btnRefSelectClr.Enabled = false;
                btnRefSelectSync.Enabled = false;
                lbRefinedData.ClearSelected();
            }
            else
            {
                btnRefCopy.Enabled = true;
                btnRefClear.Enabled = true;
                btnRefSelectClr.Enabled = hasSelection;
                btnRefSelectSync.Enabled = canSync;
            }
        }

        private async void btnRefClear_Click(object sender, EventArgs e)
        {
            int itemCount = lbRefinedData.Items.Count;

            var result = MessageBox.Show($"You are about to delete all {itemCount} item{(itemCount != 1 ? "s" : "")} from the Refined Dataset.\n\nAre you sure you want to proceed?",
                                         "Delete Confirmation",
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {
                return;
            }

            pbMain.Style = ProgressBarStyle.Continuous;
            pbMain.Minimum = 0;
            pbMain.Maximum = 100;
            pbMain.Value = 0;

            lbRefinedData.BeginUpdate();
            lbRefinedData.Items.Clear();
            lbRefinedData.ClearSelected();
            lbRefinedData.EndUpdate();

            await Task.Yield();

            pbMain.Value = 100;
            pbMain.Refresh();

            lblRefCnt.Text = "Count : " + lbRefinedData.Items.Count;
            slblCalibratedType.Text = "--";
            slblKernelRadius.Text = "--";
            tlblSeparator2.Visible = false;
            tlblPolyOrder.Visible = false;
            slblPolyOrder.Visible = false;
            slblPolyOrder.Text = "--";

            await Task.Yield();
            pbMain.Value = 0;

            UpdatelbInitDataBtnsState(null, EventArgs.Empty);
            UpdatelbRefinedDataBtnsState(null, EventArgs.Empty);

            slblDesc.Text = $"Deleted all {itemCount} item{(itemCount != 1 ? "s" : "")} from Refined Dataset.";
            slblDesc.Visible = true;

            lbRefinedData.Focus();
        }

        private async void btnRefSelectAll_Click(object sender, EventArgs e)
        {
            _ctsRefSelectAll?.Cancel();
            _ctsRefSelectAll = new CancellationTokenSource();
            var token = _ctsRefSelectAll.Token;

            int n2 = lbRefinedData.Items.Count;
            if (n2 == 0)
                return;

            pbMain.Style = ProgressBarStyle.Continuous;
            pbMain.Minimum = 0;
            pbMain.Maximum = 100;
            pbMain.Value = 0;

            lbRefinedData.BeginUpdate();
            lbRefinedData.ClearSelected();

            int reportInterval = Math.Max(1, n2 / 100);
            int yieldInterval = Math.Max(1, n2 / 1000);

            try
            {
                for (int i = 0; i < n2; i++)
                {
                    if (token.IsCancellationRequested)
                        break;

                    lbRefinedData.SetSelected(i, true);

                    if (i % reportInterval == 0)
                    {
                        double ratio = (i + 1) / (double)n2;
                        int pct = (int)Math.Round(ratio * 100.0);
                        pbMain.Value = Math.Min(100, Math.Max(0, pct));
                    }

                    if (i % yieldInterval == 0)
                        await Task.Yield();
                }
            }
            finally
            {
                lbRefinedData.EndUpdate();
            }

            pbMain.Value = 100;
            await Task.Delay(200).ContinueWith(_ => { });
            UpdatelbRefinedDataBtnsState(null, EventArgs.Empty);
            pbMain.Value = 0;
        }

        private async void btnRefSelectClr_Click(object sender, EventArgs e)
        {
            int deselectedCount = lbRefinedData.SelectedIndices.Count;
            await ClearSelectionWithProgress(lbRefinedData, pbMain, lblInitCnt);

            slblDesc.Text = $"Deselected {deselectedCount} selected item{(deselectedCount != 1 ? "s" : "")} from Refined Dataset.";
            slblDesc.Visible = true;

            UpdatelbRefinedDataBtnsState(null, EventArgs.Empty);
        }

        private async Task ClearSelectionWithProgress(ListBox lb, System.Windows.Forms.ProgressBar progressBar, Label lblCount)
        {
            int selectedCount = lb.SelectedIndices.Count;
            if (selectedCount == 0)
                return;

            progressBar.Minimum = 0;
            progressBar.Maximum = selectedCount;
            progressBar.Value = 0;
            progressBar.Style = ProgressBarStyle.Continuous;

            lb.BeginUpdate();
            try
            {
                while (lb.SelectedIndices.Count > 0)
                {
                    int index = lb.SelectedIndices[0];
                    lb.SetSelected(index, false);
                    progressBar.Value++;
                    await Task.Delay(10); // UI 반영 시간 확보
                }
            }
            finally
            {
                progressBar.Value = 0;
                lb.EndUpdate();
            }
        }

        private void txtInitAdd_TextChanged(object sender, EventArgs e)
        {
            btnInitAdd.Enabled = txtInitAdd.Text.Length > 0 && double.TryParse(txtInitAdd.Text, out _);
        }

        private void lbRefinedData_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatelbRefinedDataBtnsState(null, EventArgs.Empty);


            int count = lbRefinedData.SelectedItems.Count;

            if (count == 0)
                return;

            slblDesc.Visible = true;
            slblDesc.Text = $"{count} {(count == 1 ? "item has" : "items have")} been selected in Refined Dataset.";
        }

        private void lbInitData_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete)
            {
                btnInitDelete.PerformClick();
                e.SuppressKeyPress = true;
            }

            if (e.KeyData == (Keys.Control | Keys.Delete))
            {
                btnInitClear.PerformClick();
                e.SuppressKeyPress = true;
            }

            if (e.KeyData == Keys.F2)
            {
                btnInitEdit.PerformClick();
                e.SuppressKeyPress = true;
            }

            if (e.KeyData == (Keys.Control | Keys.C))
            {
                btnInitCopy.PerformClick();
                e.SuppressKeyPress = true;
            }

            if (e.KeyData == (Keys.Control | Keys.V))
            {
                btnInitPaste.PerformClick();
                e.SuppressKeyPress = true;
            }

            if (e.KeyData == (Keys.Control | Keys.A))
            {
                btnInitSelectAll.PerformClick();
                e.SuppressKeyPress = true;
            }

            if (e.KeyData == Keys.Escape)
            {
                btnInitSelectClr.PerformClick();
                e.SuppressKeyPress = true;
            }

            lblInitCnt.Text = "Count : " + lbInitData.Items.Count;
        }

        private void lbRefinedData_KeyDown(object sender, KeyEventArgs e)
        {
            {
                if (e.KeyData == (Keys.Control | Keys.Delete))
                {
                    btnRefClear.PerformClick();
                    e.SuppressKeyPress = true;
                }

                if (e.KeyData == (Keys.Control | Keys.C))
                {
                    btnRefCopy.PerformClick();
                    e.SuppressKeyPress = true;
                }

                if (e.KeyData == (Keys.Control | Keys.A))
                {
                    btnRefSelectAll.PerformClick();
                    e.SuppressKeyPress = true;
                }

                if (e.KeyData == Keys.Escape)
                {
                    btnRefSelectClr.PerformClick();
                    e.SuppressKeyPress = true;
                }

                lblRefCnt.Text = "Count : " + lbRefinedData.Items.Count;
            }
        }


        private void btnInitCopy_Click(object sender, EventArgs e)
        {
            IEnumerable<object> source = lbInitData.SelectedItems.Count > 0
                ? lbInitData.SelectedItems.Cast<object>()
                : lbInitData.Items.Cast<object>();

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
                .Select(x => x.Value)
                .ToList(); 

            Clipboard.SetText(string.Join(Environment.NewLine, doubles));

            int copiedCount = doubles.Count;
            slblDesc.Visible = true;
            slblDesc.Text = copiedCount == 1
                ? "Successfully copied 1 item."
                : $"Successfully copied {copiedCount} items.";
        }


        private void btnRefCopy_Click(object sender, EventArgs e)
        {
            IEnumerable<object> source = lbRefinedData.SelectedItems.Count > 0
                ? lbRefinedData.SelectedItems.Cast<object>()
                : lbRefinedData.Items.Cast<object>();

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
                .Select(x => x.Value)
                .ToList();

            Clipboard.SetText(string.Join(Environment.NewLine, doubles));

            int copiedCount = doubles.Count;
            slblDesc.Visible = true;
            slblDesc.Text = copiedCount == 1
                ? "Successfully copied 1 item."
                : $"Successfully copied {copiedCount} items.";
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

        private void FrmMain_Load(object sender, EventArgs e)
        {
            cbxKernelRadius.SelectedIndex = 3;
            cbxPolyOrder.SelectedIndex = 1;

            using (Graphics g = this.CreateGraphics())
            {
                dpiX = g.DpiX;
                dpiY = g.DpiY;
            }

            pbMain.Size = new Size(
                (int)(734 * dpiX / 96),
                (int)(5 * dpiY / 96)
            );

            slblDesc.Size = new Size(
                (int)(731 * dpiX / 96),
                (int)(19 * dpiY / 96)
            );

            txtDatasetTitle.Text = ExcelTitlePlaceholder;
            txtDatasetTitle.ForeColor = Color.Gray;
            txtDatasetTitle.Enter += txtDatasetTitle_Enter;
            txtDatasetTitle.Leave += txtDatasetTitle_Leave;
            txtDatasetTitle.TextChanged += txtDatasetTitle_TextChanged;
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

            UpdatelbInitDataBtnsState(null, EventArgs.Empty);
            UpdatelbRefinedDataBtnsState(null, EventArgs.Empty);

            dataCount = lbInitData.Items.Count;

            // ComboBox 값으로부터 파싱
            int.TryParse(cbxKernelRadius.Text, out r);
            int.TryParse(cbxPolyOrder.Text, out polyOrder);

            this.KeyPreview = true;
        }

        private void lbInitData_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatelbInitDataBtnsState(null, EventArgs.Empty);


            int count = lbInitData.SelectedItems.Count;

            if (count == 0)
                return;

            slblDesc.Visible = true;
            slblDesc.Text = $"{count} {(count == 1 ? "item has" : "items have")} been selected in Initial Dataset.";
        }

        private void btnInitEdit_Click(object sender, EventArgs e)
        {
            var frm = new FrmModify();
            frm.ShowDialog();

            frm.txtInitEdit.Select();
        }


        private async Task ExportCsvAsync()
        {
            // UI 초기화
            if (pbMain.InvokeRequired)
            {
                pbMain.Invoke(new Action(() =>
                {
                    pbMain.Style = ProgressBarStyle.Continuous;
                    pbMain.Minimum = 0;
                    pbMain.Maximum = 100;
                    pbMain.Value = 0;
                }));
            }
            else
            {
                pbMain.Style = ProgressBarStyle.Continuous;
                pbMain.Minimum = 0;
                pbMain.Maximum = 100;
                pbMain.Value = 0;
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

                    initialData = lbInitData.Items
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

                initialData = lbInitData.Items
                    .Cast<object>()
                    .Select(x => double.TryParse(x?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0.0)
                    .ToArray();

                n = initialData.Length;
            }

            var validateCsvParams = ValidateSmoothingParameters(n, r, polyOrder);
            if (!validateCsvParams.Success)
            {
                ShowError("Export Parameter Error", validateCsvParams.Error);
                return;
            }

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
                        dlg.FileName = $"{excelTitle}.csv";
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
                    dlg.FileName = $"{excelTitle}.csv";
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
                pbMain.Value = v;
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

                if (pbMain.InvokeRequired)
                    pbMain.Invoke(new Action(() =>
                    {
                        pbMain.Value = 0;
                        pbMain.Refresh();
                    }));
                else
                {
                    pbMain.Value = 0;
                    pbMain.Refresh();
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
                catch (Exception ex)
                {
                    Debug.WriteLine($"[FinalRelease] COM object release failed: {ex}");
                }
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

            pbMain.Style = ProgressBarStyle.Continuous;
            pbMain.Minimum = 0;
            pbMain.Maximum = 100;
            pbMain.Value = 0;

            var initialData = lbInitData.Items
                .Cast<object>()
                .Select(item => double.TryParse(item?.ToString(), out var d) ? d : 0.0)
                .ToArray();

            int n = initialData.Length;

            var validateXlsxParams = ValidateSmoothingParameters(n, r, polyOrder);
            if (!validateXlsxParams.Success)
            {
                ShowError("Export Parameter Error", validateXlsxParams.Error);
                return;
            }

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

                try
                {
                    var smoothingMethods = new List<string>();
                    if (doRect) smoothingMethods.Add("Rectangular");
                    if (doAvg) smoothingMethods.Add("Binomial");
                    if (doMed) smoothingMethods.Add("Median");
                    if (doGauss) smoothingMethods.Add("Gaussian");
                    if (doSG) smoothingMethods.Add("Savitzky-Golay");

                    //  Excel 파일의 Built-in 속성에 기록.
                    //  wb.BuiltinDocumentProperties["Title"].Value = txtDatasetTitle.Text;
                    //  wb.BuiltinDocumentProperties["Category"].Value = "SonataSmooth Export";
                    //  wb.BuiltinDocumentProperties["Last Author"].Value = Environment.UserName;
                    //  wb.BuiltinDocumentProperties["Keywords"].Value = "SonataSmooth, Smoothing, Export";
                    //  string subject = smoothingMethods.Count > 0
                    //     ? string.Join(", ", smoothingMethods) + " smoothing applied"
                    //     : "No smoothing applied";

                    //  wb.BuiltinDocumentProperties["Subject"].Value = subject;
                    //  wb.BuiltinDocumentProperties["Comments"].Value = "Exported from SonataSmooth application";

                    wb.BuiltinDocumentProperties["Title"].Value = $"SonataSmooth Overture : {txtDatasetTitle.Text}";
                    wb.BuiltinDocumentProperties["Category"].Value = "SonataSmooth Movement Score";
                    wb.BuiltinDocumentProperties["Author"].Value = "Maestro SonataSmooth";
                    wb.BuiltinDocumentProperties["Last Author"].Value = Environment.UserName;
                    wb.BuiltinDocumentProperties["Keywords"].Value = "SonataSmooth, Smoothing, Movements, Harmony, Export";

                    string subject = smoothingMethods.Count > 0
                        ? $"Concerto of {string.Join(" & ", smoothingMethods)} smoothing movements"
                        : "Cadenza of Silence : No smoothing applied";

                    wb.BuiltinDocumentProperties["Subject"].Value = subject;

                    string[] musicalPhrases = new[]
                    {
                        "Adagio in Data Minor",
                        "Presto Noise Reduction",
                        "Allegro of Algorithms",
                        "Nocturne for Numeric Streams",
                        "Fugue of Filters",
                        "Symphony of Smoothness",
                        "Etude in Error Suppression",
                        "Rhapsody of Regression",
                        "Minuet of Median Magic",
                        "Cantata of Clean Curves"
                    };

                    Random rnd = new Random();
                    string randomPhrase = musicalPhrases[rnd.Next(musicalPhrases.Length)];

                    string comments = $"Encore performed by the SonataSmooth Orchestra - \"{randomPhrase}\"";

                    if (smoothingMethods.Count == 4)
                    {
                        comments += Environment.NewLine + Environment.NewLine + "Hidden Movement Unlocked : The Quartet of Filters has performed in perfect harmony.";
                    }

                    wb.BuiltinDocumentProperties["Comments"].Value = comments;
                }
                catch (Exception ex)
                {
                    // Excel Interop 에서 발생할 수 있는 주요 예외를 구분하여 처리
                    if (ex is System.Runtime.InteropServices.COMException comEx)
                    {
                        // COM 예외 : Excel 이 설치되어 있지 않거나, 권한 문제, 속성 이름 오탈자 등
                        MessageBox.Show(
                            "Failed to set Excel document properties (COM error).\n\n" +
                            "Excel may not be installed, the property name may be incorrect, or there may be a permissions issue.\n\n" +
                            $"Details : {comEx.Message}",
                            "Excel Property Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                        // 필요 시 로그 : Debug.WriteLine(comEx);
                    }
                    else if (ex is ArgumentException argEx)
                    {
                        // 잘못된 속성 이름 등
                        MessageBox.Show(
                            "Failed to set Excel document properties (Argument error).\n\n" +
                            "The built-in property name is incorrect, or an unsupported value has been assigned.\n\n" +
                            $"Details : {argEx.Message}",
                            "Excel Property Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                    }
                    else if (ex is InvalidCastException castEx)
                    {
                        // 잘못된 타입으로 값 할당 시
                        MessageBox.Show(
                            "Failed to set Excel document properties (Type error).\n\n" +
                            "The type of value assigned to the property is invalid.\n\n" +
                            $"Details : {castEx.Message}",
                            "Excel Property Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                    }
                    else if (ex is System.UnauthorizedAccessException unauthEx)
                    {
                        // 파일 또는 속성에 대한 권한 부족
                        MessageBox.Show(
                            "Failed to set Excel document properties (Access denied).\n\n" +
                            "You do not have sufficient permissions for the Excel file or its properties.\n\n" +
                            $"Details : {unauthEx.Message}",
                            "Excel Property Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                    }
                    else
                    {
                        // 기타 예외 처리
                        MessageBox.Show(
                            "An unexpected error occurred while setting Excel document properties.\n\n" +
                            $"Details : {ex.Message}",
                            "Excel Property Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                    }

                    // 필요 시 상세 로그 표시
                    // System.Diagnostics.Debug.WriteLine(ex.ToString());
                }

                sheets = wb.Worksheets;
                coms.Push(sheets);

                ws = (Excel.Worksheet)sheets[1];
                ws.Name = txtDatasetTitle.Text;
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

                        pbMain.Value = Math.Min(100, (int)(100.0 * idx / total));
                        await Task.Yield();

                        curCol++;
                    }

                    pbMain.Value = 100;
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

                pbMain.Value = 0;

                // 창 유지 : 사용자에게 권한 이관
                wb.Saved = false; // 닫기 시 저장 대화상자 표시
                excel.Visible = true;
            }

            catch (System.Runtime.InteropServices.COMException ex)
            {
                string msg = "Excel interop error: " + ex.Message + Environment.NewLine + Environment.NewLine +
                    "Microsoft Excel does not appear to be installed, or there was a problem starting Excel." + Environment.NewLine +
                    "If Excel is not installed, you can visit the Microsoft Office website to purchase or install Office." + Environment.NewLine + Environment.NewLine +
                    "Would you like to open the Microsoft Office download page now?";

                var result = MessageBox.Show(
                    msg,
                    "Export Error",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button2
                    );

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo("https://www.microsoft.com/microsoft-365/buy/compare-all-microsoft-365-products") { UseShellExecute = true });
                    }
                    catch
                    {
                        return;
                    }
                }
            }
            catch (AggregateException ex)
            {
                var allMessages = string.Join(Environment.NewLine, ex.InnerExceptions.Select(inner => inner.Message));
                MessageBox.Show(
                    "One or more errors occurred during export : " + Environment.NewLine + allMessages,
                    "Export Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (System.IO.PathTooLongException)
            {
                MessageBox.Show("The file path is too long. Please choose a shorter path.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                MessageBox.Show("The specified directory was not found. Please check the save location.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (System.IO.IOException)
            {
                MessageBox.Show("An I/O error occurred while saving the file. Please check disk space and permissions.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (System.OutOfMemoryException)
            {
                MessageBox.Show("Not enough memory to complete the export. Try closing other applications.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (System.BadImageFormatException)
            {
                MessageBox.Show("Excel (Office) bitness (32-bit / 64-bit) or Interop DLL mismatch. Please check your Office installation.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (System.UnauthorizedAccessException)
            {
                MessageBox.Show("You do not have permission to save to this location. Please choose a different path.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred: " + ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (int.TryParse(cbxKernelRadius.Text, out var w))
            {
                settingsForm.kernelRadius = w;
                settingsForm.cbxKernelRadius.Text = cbxKernelRadius.Text;
            }
        }

        private void cbxPolyOrder_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (int.TryParse(cbxPolyOrder.Text, out var p))
            {
                settingsForm.polyOrder = p;
                settingsForm.cbxPolyOrder.Text = cbxPolyOrder.Text;
            }
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

        private void txtDatasetTitle_Enter(object sender, EventArgs e)
        {
            if (txtDatasetTitle.Text == ExcelTitlePlaceholder)
            {
                txtDatasetTitle.Text = "";
                txtDatasetTitle.ForeColor = SystemColors.WindowText;
            }
            txtDatasetTitle.TextAlign = HorizontalAlignment.Left;
        }

        private void txtDatasetTitle_Leave(object sender, EventArgs e)
        {
            string raw = txtDatasetTitle.Text;
            string title = raw?.Trim() ?? string.Empty;

            if (title == ExcelTitlePlaceholder || string.IsNullOrEmpty(raw))
            {
                txtDatasetTitle.Text = ExcelTitlePlaceholder;
                txtDatasetTitle.ForeColor = Color.Gray;
                txtDatasetTitle.TextAlign = HorizontalAlignment.Center;
                return;
            }

            const int MaxLength = 31;
            string invalidChars = ":\\/?*[";
            char[] winInvalidChars = System.IO.Path.GetInvalidFileNameChars();
            string[] reservedNames = {
        "CON","PRN","AUX","NUL",
        "COM1","COM2","COM3","COM4","COM5","COM6","COM7","COM8","COM9",
        "LPT1","LPT2","LPT3","LPT4","LPT5","LPT6","LPT7","LPT8","LPT9"
    };
            string titleUpper = title.ToUpperInvariant();

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(title))
                errors.Add("The name cannot be blank.");

            if (title.Length > MaxLength)
                errors.Add($"The name must not exceed {MaxLength} characters.");

            bool hasInvalidChar = title.IndexOfAny(invalidChars.ToCharArray()) >= 0 || title.Contains("]");
            bool hasWinInvalidChar = title.IndexOfAny(winInvalidChars) >= 0;
            if (hasInvalidChar || hasWinInvalidChar)
            {
                errors.Add(
                    "The name cannot contain any of the following characters: : \\ / ? * [ ]\n" +
                    "or any Windows file name invalid characters: " +
                    string.Join(" ", winInvalidChars.Select(c => c == ' ' ? "<space>" : c.ToString()))
                );
            }

            if (reservedNames.Any(rn => titleUpper.Equals(rn, StringComparison.OrdinalIgnoreCase)))
                errors.Add("The name cannot be a reserved Windows file name (e.g., CON, PRN, AUX, NUL, COM1, LPT1, etc.).");

            if (errors.Count > 0)
            {
                if (_isShowingTitleValidationMessage || string.Equals(_lastInvalidTitle, title, StringComparison.Ordinal))
                {
                    txtDatasetTitle.Text = ExcelTitlePlaceholder;
                    txtDatasetTitle.ForeColor = Color.Gray;
                    txtDatasetTitle.TextAlign = HorizontalAlignment.Center;
                    return;
                }

                _isShowingTitleValidationMessage = true;
                _lastInvalidTitle = title;
                try
                {
                    txtDatasetTitle.Text = ExcelTitlePlaceholder;
                    txtDatasetTitle.ForeColor = Color.Gray;
                    txtDatasetTitle.TextAlign = HorizontalAlignment.Center;
                    MessageBox.Show(string.Join("\n\n", errors), "Invalid Title", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                finally
                {
                    _isShowingTitleValidationMessage = false;
                }
                return;
            }

            _lastInvalidTitle = null;
            txtDatasetTitle.ForeColor = SystemColors.WindowText;
            txtDatasetTitle.TextAlign = HorizontalAlignment.Left;
        }

        private void txtDatasetTitle_TextChanged(object sender, EventArgs e)
        {
            UpdateExportExcelButtonState();

            if (txtDatasetTitle.Text != ExcelTitlePlaceholder)
            {
                txtDatasetTitle.TextAlign = HorizontalAlignment.Left;
            }
            else
            {
                txtDatasetTitle.TextAlign = HorizontalAlignment.Center;
            }
        }

        private void UpdateExportExcelButtonState()
        {
            bool hasItems = lbInitData.Items.Count > 0;
            bool isValid = hasItems
                && !string.IsNullOrWhiteSpace(txtDatasetTitle.Text)
                && txtDatasetTitle.Text != ExcelTitlePlaceholder;
            btnExport.Enabled = isValid;
        }

        private void btnInitSelectSync_Click(object sender, EventArgs e)
        {
            int count = lbInitData.Items.Count;
            if (count != lbRefinedData.Items.Count || lbInitData.SelectedIndices.Count == 0)
                return;

            lbRefinedData.BeginUpdate();
            try
            {
                lbRefinedData.ClearSelected();

                var indices = new int[lbInitData.SelectedIndices.Count];
                lbInitData.SelectedIndices.CopyTo(indices, 0);

                for (int i = 0; i < indices.Length; i++)
                    lbRefinedData.SetSelected(indices[i], true);

                // 스크롤 위치 동기화
                lbRefinedData.TopIndex = lbInitData.TopIndex;

                slblDesc.Text = $"Synchronized {indices.Length} selected item{(indices.Length > 1 ? "s" : "")} to Refined Dataset.";
                slblDesc.Visible = true;
            }
            finally
            {
                lbRefinedData.EndUpdate();
            }
        }

        private void btnRefSelectSync_Click(object sender, EventArgs e)
        {
            int count = lbRefinedData.Items.Count;
            if (count != lbInitData.Items.Count || lbRefinedData.SelectedIndices.Count == 0)
                return;

            lbInitData.BeginUpdate();
            try
            {
                lbInitData.ClearSelected();

                var indices = new int[lbRefinedData.SelectedIndices.Count];
                lbRefinedData.SelectedIndices.CopyTo(indices, 0);

                for (int i = 0; i < indices.Length; i++)
                    lbInitData.SetSelected(indices[i], true);

                // 스크롤 위치 동기화
                lbInitData.TopIndex = lbRefinedData.TopIndex;

                slblDesc.Text = $"Synchronized {indices.Length} selected item{(indices.Length > 1 ? "s" : "")} to Initial Dataset.";
                slblDesc.Visible = true;
            }
            finally
            {
                lbInitData.EndUpdate();
            }
        }

        public void ShowStatusMessage(string message)
        {
            slblDesc.Text = message;
            slblDesc.Visible = true;
        }


        #region Mouse Hover and Leave Events
        private void MouseLeaveHandler(object sender, EventArgs e)
        {
            if (isRefinedLoading || lbRefinedData.Items.Count == 0)
            {
                slblDesc.Text = "To calibrate, add data to the Initial Dataset, choose a Calibration Method, set Smoothing Parameters.";
                slblDesc.Visible = true;
            }
            else
            {
                slblDesc.Visible = false;
            }
        }


        private void cbxKernelRadius_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Visible = true;
            slblDesc.Text = $"Defines how many data points on each side of the target point are included in the smoothing window. (Recommended : {RecommendedMinRadius} - {RecommendedMaxRadius})";
        }


        private void cbxKernelRadius_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }


        private void lblPolyOrder_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Visible = true;
            slblDesc.Text = $"Specifies the degree of the polynomial used to fit the data within each smoothing window. (Recommended : {RecommendedMinPolyOrder} - {RecommendedMaxPolyOrder}).";
        }

        private void lblPolyOrder_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void cbxPolyOrder_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Visible = true;
            slblDesc.Text = $"Specifies the degree of the polynomial used to fit the data within each smoothing window. (Recommended : {RecommendedMinPolyOrder} - {RecommendedMaxPolyOrder}).";
        }
        private void cbxPolyOrder_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void lblKernelRadius_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Visible = true;
            slblDesc.Text = $"Defines how many data points on each side of the target point are included in the smoothing window. (Recommended : {RecommendedMinRadius} - {RecommendedMaxRadius})";
        }

        private void lblKernelRadius_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void txtInitAdd_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Visible = true;
            slblDesc.Text = "Enter a numeric value to add to the Initial Dataset. Press Enter key or click the Add button to submit.";
        }

        private void txtInitAdd_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void btnInitAdd_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Visible = true;
            slblDesc.Text = "Add the entered value to the Initial Dataset.";
        }

        private void btnInitAdd_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void rbtnRect_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Visible = true;
            slblDesc.Text = "Applies a simple moving average using equal weights within the kernel window.";
        }

        private void rbtnRect_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void rbtnAvg_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Visible = true;
            slblDesc.Text = "Smooths data using binomial coefficients for weighted averaging within the kernel window.";
        }

        private void rbtnAvg_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void rbtnMed_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Visible = true;
            slblDesc.Text = "Reduces noise by computing the weighted median using binomial coefficients within the kernel window.";
        }

        private void rbtnMed_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void rbtnGauss_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Visible = true;
            slblDesc.Text = "Smooths data using a Gaussian kernel for weighted averaging, emphasizing central values.";
        }

        private void rbtnGauss_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void rbtnSG_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Visible = true;
            slblDesc.Text = "Fits a polynomial to the data within the kernel window for advanced smoothing and trend preservation.";
        }

        private void rbtnSG_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }


        private void btnInitClear_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Visible = true;

            int initItemCount = lbInitData.Items.Count;
            int refItemCount = lbRefinedData.Items.Count;

            string initialMsg = initItemCount == 1
                ? "Delete the item from the Initial Dataset"
                : $"Delete all {initItemCount} items from the Initial Dataset";

            string refinedMsg = refItemCount == 0
                ? string.Empty
                : refItemCount == 1
                    ? "and Delete the item from the Refined Dataset"
                    : $"and Delete all {refItemCount} items from the Refined Dataset";

            slblDesc.Text = refinedMsg == string.Empty
                ? initialMsg + "."
                : $"{initialMsg} {refinedMsg}.";
        }


        private void btnInitClear_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void btnInitCopy_MouseHover(object sender, EventArgs e)
        {
            int selCount = lbInitData.SelectedItems.Count;
            int totalCount = lbInitData.Items.Count;
            slblDesc.Visible = true;

            slblDesc.Text = (selCount == 0 || selCount == lbInitData.Items.Count)
                ? $"Copy all {totalCount} items from the Refined Dataset to the clipboard."
                : selCount == 1
                    ? "Copy the selected item from the Refined Dataset to the clipboard."
                    : $"Copy {selCount} selected items from the Refined Dataset to the clipboard.";
        }


        private void btnInitCopy_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void btnInitPaste_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Visible = true;
            slblDesc.Text = "Paste numeric values from the clipboard into the Initial Dataset.";
        }

        private void btnInitPaste_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void btnInitEdit_MouseHover(object sender, EventArgs e)
        {
            int selCount = lbInitData.SelectedItems.Count;
            slblDesc.Visible = true;

            if (selCount == 1)
                slblDesc.Text = "Edit the selected item in the Initial Dataset.";
            else
                slblDesc.Text = $"Edit {selCount} selected items in the Initial Dataset.";
        }

        private void btnInitEdit_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void btnInitDelete_MouseHover(object sender, EventArgs e)
        {
            int selCount = lbInitData.SelectedItems.Count;
            int totalCount = lbInitData.Items.Count;
            int refCount = lbRefinedData.Items.Count;
            slblDesc.Visible = true;

            if (selCount == 1)
            {
                slblDesc.Text = "Delete the selected item from the Initial Dataset.";
            }
            else if (selCount == totalCount && totalCount > 0)
            {
                string initMsg = $"Delete all {selCount} items from the Initial Dataset";

                if (refCount > 0)
                {
                    string refMsg = refCount == 1
                        ? " and Delete the item from the Refined Dataset"
                        : $" and Delete all {refCount} items from the Refined Dataset";

                    slblDesc.Text = initMsg + refMsg + ".";
                }
                else
                {
                    slblDesc.Text = initMsg + ".";
                }
            }
            else
            {
                slblDesc.Text = $"Delete {selCount} selected item{(selCount != 1 ? "s" : "")} from the Initial Dataset.";
            }
        }

        private void btnInitDelete_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }
        
        private void btnInitSelectAll_MouseHover(object sender, EventArgs e)
        {
            int itemCount = lbInitData.Items.Count;
            slblDesc.Visible = true;

            if (itemCount == 1)
            {
                slblDesc.Text = "Select the item in the Initial Dataset.";
            }
            else if (itemCount > 1)
            {
                slblDesc.Text = "Select all items in the Initial Dataset.";
            }
        }

        private void btnInitSelectAll_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void btnInitSelectClr_MouseHover(object sender, EventArgs e)
        {
            int selCount = lbInitData.SelectedItems.Count;
            slblDesc.Visible = true;
            
            if (selCount == 1)
            {
                slblDesc.Text = "Deselect the selected item in the Initial Dataset.";
            }
            else
            {
                slblDesc.Text = $"Deselect all {selCount} selected items in the Initial Dataset.";
            }
        }

        private void btnInitSelectSync_MouseHover(object sender, EventArgs e)
        {
            int selCount = lbInitData.SelectedItems.Count;
            slblDesc.Visible = true;

            if (selCount == 1)
                slblDesc.Text = "Select the corresponding item in the Refined Dataset.";
            else
                slblDesc.Text = $"Select {selCount} corresponding items in the Refined Dataset.";
        }

        private void btnRefClear_MouseHover(object sender, EventArgs e)
        {
            int itemCount = lbRefinedData.Items.Count;
            slblDesc.Visible = true;

            if (itemCount == 1)
                slblDesc.Text = "Delete the item from the Refined Dataset.";
            else
                slblDesc.Text = $"Delete all {itemCount} items from the Refined Dataset.";
        }

        private void btnRefClear_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void btnRefCopy_MouseHover(object sender, EventArgs e)
        {
            int selCount = lbRefinedData.SelectedItems.Count;
            int totalCount = lbRefinedData.Items.Count;
            slblDesc.Visible = true;

            slblDesc.Text = (selCount == 0 || selCount == lbRefinedData.Items.Count)
                ? $"Copy all {totalCount} items from the Refined Dataset to the clipboard."
                : selCount == 1
                    ? "Copy the selected item from the Refined Dataset to the clipboard."
                    : $"Copy {selCount} selected items from the Refined Dataset to the clipboard.";
        }


        private void btnRefCopy_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void btnRefSelectAll_MouseHover(object sender, EventArgs e)
        {
            int itemCount = lbRefinedData.Items.Count;
            slblDesc.Visible = true;

            if (itemCount == 1)
            {
                slblDesc.Text = "Select the item in the Refined Dataset.";
            }
            else if (itemCount > 1)
            {
                slblDesc.Text = "Select all items in the Refined Dataset.";
            }
        }


        private void btnRefSelectAll_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void btnRefSelectClr_MouseHover(object sender, EventArgs e)
        {
            int selCount = lbRefinedData.SelectedItems.Count;
            slblDesc.Visible = true;

            if (selCount == 1)
                slblDesc.Text = "Deselect the selected item in the Refined Dataset.";
            else
                slblDesc.Text = $"Deselect all {selCount} selected items in the Refined Dataset.";
        }

        private void btnRefSelectSync_MouseHover(object sender, EventArgs e)
        {
            int selCount = lbRefinedData.SelectedItems.Count;

            slblDesc.Visible = true;
            if (selCount == 1)
                slblDesc.Text = "Select the corresponding item in the Initial Dataset.";
            else
                slblDesc.Text = $"Select {selCount} corresponding items in the Initial Dataset.";
        }

        private void txtDatasetTitle_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Visible = true;
            if (txtDatasetTitle.Text == ExcelTitlePlaceholder)
            {
                slblDesc.Text = "Enter a title for the Excel document. This will be used as the main title in the first cell.";
            }
            else
            {
                slblDesc.Text = "Edit the title for the Excel document. This will be used as the main title in the first cell.";
            }
        }

        private void txtDatasetTitle_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void btnInfo_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Visible = true;
            slblDesc.Text = "Click to view SonataSmooth application information and version details.";
        }

        private void btnInfo_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void btnCalibrate_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Visible = true;
            int itemCount = lbInitData.Items.Count;
            if (itemCount == 0)
                slblDesc.Text = "To calibrate, add data to the Initial Dataset, choose a Calibration Method, set Smoothing Parameters.";
            else
                slblDesc.Text = "Click to start the calibration process using the selected smoothing method and parameters.";

        }

        private void btnCalibrate_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void btnExport_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Visible = true;
            if (lbRefinedData.Items.Count == 0)
            {
                slblDesc.Text = "To export, first calibrate the data using the Calibrate button.";
            }
            else
            {
                slblDesc.Text = "Click to export the refined dataset to an Excel file with the specified title and settings.";
            }
        }

        private void btnExportSettings_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Visible = true;
            slblDesc.Text = "Click to open the Export Settings, to configure smoothing options and file format preferences for exporting data.";
        }

        private void btnExportSettings_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void txtDatasetTitle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                bool forward = (e.Modifiers & Keys.Shift) == 0;

                e.SuppressKeyPress = true;
                e.Handled = true;

                Control current = sender as Control ?? this.ActiveControl;
                if (current != null)
                {
                    this.SelectNextControl(current, forward, true, true, true);
                }
            }
        }
    }    
    #endregion
}
