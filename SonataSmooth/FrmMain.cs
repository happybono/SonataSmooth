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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SonataSmooth.FrmMain;
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

        // 엑셀 제목 입력 TextBox 의 기본 안내 문구 (Placeholder)
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

        // 경계 처리 방식 열거형
        public enum BoundaryMode { Symmetric, Replicate, ZeroPad }

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

        // 경계 처리 방식에 따른 유효한 Index 계산 메서드
        private int GetIndex(int idx, int n, BoundaryMode mode)
        {
            switch (mode)                                                           // 경계 처리 방식 (Boundary Mode)
            {
                case BoundaryMode.Symmetric:                                        // 대칭 반사 (경계 지점 기준) Mirroring 
                    return idx < 0 ? -idx - 1 : idx >= n ? 2 * n - idx - 1 : idx;
                case BoundaryMode.Replicate:                                        // 가장 자리 값 복제 (Replicate) 
                    return idx < 0 ? 0 : idx >= n ? n - 1 : idx;
                case BoundaryMode.ZeroPad:                                          // 경계 밖의 값을 0 으로 채우는 Zero Padding 
                    return (idx < 0 || idx >= n) ? -1 : idx;                        // -1 은 0 으로 처리
                default:
                    return idx;
            }
        }

        // Smoothing Parameter 유효성 검증 메서드
        private BoundaryMode GetBoundaryMode()
        {
            // ComboBox 에서 선택된 경계 처리 방식을 BoundaryMode 열거형으로 변환
            switch (cbxBoundaryMethod.SelectedItem?.ToString())
            {
                // 기본값은 Symmetric
                case "Symmetric": return BoundaryMode.Symmetric;

                // Replicate
                case "Replicate": return BoundaryMode.Replicate;

                // Zero Padding
                case "Zero Padding": return BoundaryMode.ZeroPad;

                // 그 외의 값은 기본값으로 처리
                default: return BoundaryMode.Symmetric;
            }
        }

        private struct OperationResult
        {
            public bool Success { get; }
            public string Error { get; }
            private OperationResult(bool success, string error)
            {
                Success = success;
                Error = error;
            }
            public static OperationResult OK() => new OperationResult(true, null);
            public static OperationResult Fail(string error) => new OperationResult(false, error);
        }

        private void ShowError(string title, string message)
        {
            MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static bool TryGetDoubleFromItem(object item, out double value)
        {
            if (item is double d)
            {
                value = d;
                return true;
            }
            if (item == null)
            {
                value = 0;
                return false;
            }
            string s = item.ToString();
            // Invariant 우선, 실패 시 현재 문화권
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                return true;
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
                return true;
            value = 0;
            return false;
        }

        /// <summary>
        /// lbInitData 의 모든 항목을 double 형식의 배열로 Parsing 합니다.
        /// 항목이 null 이거나 숫자 변환에 실패하면 MessageBox 로 오류를 표시하고 false 값을 반환합니다.
        /// </summary>
        private OperationResult TryParseInputData(out double[] input, out int n)
        {
            n = lbInitData.Items.Count;
            input = new double[n];
            for (int i = 0; i < n; i++)
            {
                var item = lbInitData.Items[i];
                if (!TryGetDoubleFromItem(item, out input[i]))
                {
                    return OperationResult.Fail(
                        $"Failed to convert item at index {i} : \"{item?.ToString() ?? "<null>"}\"");
                }
            }
            return OperationResult.OK();
        }

        /// <summary>
        /// Kernel 반경과 다항식 차수를 ComboBox 에서 Parsing 합니다.
        /// Parsing 실패 시 MessageBox 로 오류를 알리고 false 값을 반환합니다.
        /// Gaussian 보정 방식에 활용될 sigma 값도 계산합니다.
        /// </summary>
        private OperationResult TryParseParameters(out int r, out int polyOrder, out double sigma)
        {
            r = 0;
            polyOrder = 0;
            sigma = 0;
            if (!int.TryParse(cbxKernelRadius.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out r))
            {
                return OperationResult.Fail($"Failed to parse kernel radius : \"{cbxKernelRadius.Text}\".");
            }
            if (!int.TryParse(cbxPolyOrder.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out polyOrder))
            {
                return OperationResult.Fail($"Failed to parse polynomial order : \"{cbxPolyOrder.Text}\".");
            }
            sigma = (2.0 * r + 1) / 6.0;
            return OperationResult.OK();
        }

        /// <summary>
        /// "Calibrate" 버튼 클릭 시 호출되는 Event Handler 입니다.
        /// 초기 데이터를 기준으로 선택한 보정 필터를 적용해 각각의 값들을 보정하고, 결과를 Refined Dataset 에 표시합니다.
        /// </summary>
        private async void btnCalibrate_Click(object sender, EventArgs e)
        {
            // 현재 선택된 경계 처리 방식 Parsing
            BoundaryMode boundaryMode = GetBoundaryMode();

            // Export 설정 폼에 현재 Kernel 반경 / 다항식 차수 값 동기화
            settingsForm.ApplyParameters(cbxKernelRadius.Text, cbxPolyOrder.Text, cbxBoundaryMethod.Text);

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
                //  ListBox의 데이터를 double[] 형식으로 Parsing (실패 시 반환)
                var parseInputResult = TryParseInputData(out double[] input, out int n);
                if (!parseInputResult.Success)
                {
                    ShowError("Input Parse Error", parseInputResult.Error);
                    return;
                }

                // 입력된 Parameters (Kernel 반경, 다항식 차수, Gaussian 표준 편차) Parsing (실패 시 반환)
                var parseParamResult = TryParseParameters(out int r, out int polyOrder, out double sigma);
                if (!parseParamResult.Success)
                {
                    ShowError("Parameter Parse Error", parseParamResult.Error);
                    return;
                }

                // Parameters 유효성 검증 (윈도우 크기 및 다항식 차수) (실패 시 반환)
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

                double[] results;
                try
                {
                    // 선택된 필터 적용 (Background Thread 에서 실행)
                    results = await Task.Run(() =>
                    {
                        // 통합 ApplySmoothing 함수 호출
                        // 다양한 방식을 동시에 계산 가능하지만, UX 상 선택된 하나만 사용
                        var multi = ApplySmoothing(
                            input,
                            r,
                            polyOrder,
                            boundaryMode,
                            useRect,
                            useAvg,
                            useMed,
                            useGauss,
                            useSG);

                        // 선택된 필터 결과만 반환
                        if (useRect) return multi.Rect;
                        if (useAvg) return multi.Binom;
                        if (useMed) return multi.Median;
                        if (useGauss) return multi.Gauss;
                        if (useSG) return multi.SG;
                        return new double[n];
                    });
                }
                catch (Exception ex)
                {
                    // 필터 계산 중 예외 발생
                    MessageBox.Show($"An unexpected error occurred during computation : {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    UpdatelbRefinedDataBtnsState(null, EventArgs.Empty);
                    return;
                }

                try
                {
                    lbRefinedData.BeginUpdate();
                    lbRefinedData.Items.Clear();

                    // 보정 작업 중 버튼 비활성화
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

                    isRefinedLoading = true;
                    var progressReporter = new Progress<int>(pct =>
                    {
                        pbMain.Value = Math.Max(pbMain.Minimum, Math.Min(pbMain.Maximum, pct));
                    });

                    // 결과 데이터를 ListBox 에 Batch 단위로 추가
                    await AddItemsInBatches(lbRefinedData, results, progressReporter, 60);
                    isRefinedLoading = false;

                    lbRefinedData.EndUpdate();

                    // 결과 항목 개수 표시
                    lblRefCnt.Text = $"Count : {lbRefinedData.Items.Count}";

                    // 상태 표시줄에 적용된 보정 방식 표시
                    slblCalibratedType.Text = useRect ? "Rectangular Average"
                                         : useMed ? "Weighted Median"
                                         : useAvg ? "Binomial Average"
                                         : useSG ? "Savitzky-Golay Filter"
                                         : useGauss ? "Gaussian Filter"
                                                    : "Unknown";

                    // 커널 반경 표시
                    slblKernelRadius.Text = r.ToString();


                    // 다항식 차수 표시 (Savitzky-Golay 방식에서만 사용)
                    bool showPoly = useSG;
                    tlblPolyOrder.Visible = showPoly;
                    tlblSeparator2.Visible = showPoly;
                    slblPolyOrder.Visible = showPoly;
                    if (showPoly) slblPolyOrder.Text = polyOrder.ToString();

                    // 경계 처리 방식 표시
                    tlblSeparator3.Visible = true;
                    tlblBoundaryMethod.Visible = true;
                    slblBoundaryMethod.Visible = true;

                    switch (boundaryMode)
                    {
                        case BoundaryMode.Symmetric: slblBoundaryMethod.Text = "Symmetric"; break;
                        case BoundaryMode.Replicate: slblBoundaryMethod.Text = "Replicate"; break;
                        case BoundaryMode.ZeroPad: slblBoundaryMethod.Text = "Zero Padding"; break;
                        default: slblBoundaryMethod.Text = "Symmetric"; break;
                    }
                }
                catch (Exception ex)
                {
                    // 결과 Binding 중 예외 발생
                    MessageBox.Show($"Error binding results : {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    isRefinedLoading = false;
                }
            }
            finally
            {
                // 버튼 활성화
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

                // 버튼 상태 업데이트
                UpdatelbInitDataBtnsState(null, EventArgs.Empty);
                UpdatelbRefinedDataBtnsState(null, EventArgs.Empty);
            }

            // ProgressBar 초기화
            pbMain.Value = 0;
        }

        // 입력된 데이터에 대해 다양한 보정 방식 (Smoothing Filter) 을 적용하는 메서드.
        private (double[] Rect, double[] Binom, double[] Median, double[] Gauss, double[] SG)
           ApplySmoothing(double[] input, int r, int polyOrder, BoundaryMode boundaryMode,
                          bool doRect, bool doAvg, bool doMed, bool doGauss, bool doSG)
        {
            int n = input.Length;
            int windowSize = 2 * r + 1;

            long[] binom = (doAvg || doMed) ? CalcBinomialCoefficients(windowSize) : null;
            double[] gaussCoeffs = doGauss ? ComputeGaussianCoefficients(windowSize, (2.0 * r + 1) / 6.0) : null;
            double[] sgCoeffs = doSG ? ComputeSavitzkyGolayCoefficients(windowSize, polyOrder) : null;

            var rect = new double[n];
            var binomAvg = new double[n];
            var median = new double[n];
            var gauss = new double[n];
            var sg = new double[n];

            // Rectangular, Binomial Average 의 나누기 / 합계 값 미리 계산
            double invRectDiv = doRect ? 1.0 / windowSize : 0.0;
            double binomSum = 0.0;
            if (doAvg && binom != null)
            {
                for (int i = 0; i < binom.Length; i++) binomSum += binom[i];
            }

            double Sample(int idx) => GetValueWithBoundary(input, idx, boundaryMode);

            // 데이터가 적을 경우 병렬 처리 OverHead 가 더 크므로 직렬로 실행
            bool useParallel = n >= 2000;

            Action<int> smoothingAction = i =>
            {
                // Rectangular
                if (doRect)
                {
                    double sum = 0.0;
                    for (int k = -r; k <= r; k++)
                        sum += Sample(i + k);
                    rect[i] = sum * invRectDiv;
                }

                // Binomial Average
                if (doAvg && binom != null)
                {
                    double sum = 0.0;
                    for (int k = -r; k <= r; k++)
                        sum += Sample(i + k) * binom[k + r];
                    binomAvg[i] = binomSum > 0 ? sum / binomSum : 0.0;
                }

                // Weighted Median
                if (doMed && binom != null)
                {
                    median[i] = WeightedMedianAt(input, i, r, binom, boundaryMode);
                }

                // Gaussian
                if (doGauss && gaussCoeffs != null)
                {
                    double sum = 0.0;
                    for (int k = -r; k <= r; k++)
                        sum += gaussCoeffs[k + r] * Sample(i + k);
                    gauss[i] = sum;
                }

                // Savitzky-Golay
                if (doSG && sgCoeffs != null)
                {
                    // `Sample` 함수가 모든 경계 모드 (ZeroPad 포함) 를 올바르게 처리하므로 별도 분기 없이 통합된 로직 사용.
                    double sum = 0.0;
                    for (int k = -r; k <= r; k++)
                        sum += sgCoeffs[k + r] * Sample(i + k);
                    sg[i] = sum;
                }
            };

            if (useParallel)
            {
                Parallel.For(0, n, smoothingAction);
            }
            else
            {
                for (int i = 0; i < n; i++)
                {
                    smoothingAction(i);
                }
            }

            return (rect, binomAvg, median, gauss, sg);
        }

        // Gaussian 보정 방식 : 계수 계산 메서드
        private static double[] ComputeGaussianCoefficients(int length, double sigma)
        {
            if (length < 1)
                throw new ArgumentException("length must be ≥ 1", nameof(length));
            if (sigma <= 0)
                throw new ArgumentException("sigma must be > 0", nameof(sigma));

            var coeffs = new double[length];        // 계수 배열
            int w = (length - 1) / 2;               // Kernel 중심
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

        /// <summary>
        /// 주어진 인덱스 (idx) 에 대해 BoundaryMode 에 맞춰 경계 처리를 수행한 뒤 값을 반환합니다.
        /// - Symmetric : 경계를 기준으로 대칭 반사(reflection)
        /// - Replicate : 경계 값 복제
        /// - ZeroPad   : 경계 밖은 0 으로 채움
        /// </summary>
        private double GetValueWithBoundary(double[] data, int idx, BoundaryMode mode)
        {
            int n = data.Length;
            switch (mode)
            {
                case BoundaryMode.Symmetric:
                    // Symmetric : 대칭 반사 (경계 지점 기준) Mirroring
                    if (idx < 0) idx = -idx - 1;
                    else if (idx >= n) idx = 2 * n - idx - 1;
                    if (idx < 0) return 0; 
                    return data[idx];

                case BoundaryMode.Replicate:
                    // Replicate : 범위를 벗어나면 가장자리 값을 그대로 사용
                    if (idx < 0) idx = 0;
                    else if (idx >= n) idx = n - 1;
                    return data[idx];

                case BoundaryMode.ZeroPad:
                    // Zero Padding : 범위를 벗어나면 0 반환
                    if (idx < 0 || idx >= n) return 0.0;
                    return data[idx];

                default:
                    // 선택 여부를 알 수 없는 경우, 대칭 반사와 동일하게 처리
                    if (idx < 0) idx = -idx - 1;
                    else if (idx >= n) idx = 2 * n - idx - 1;
                    if (idx < 0) return 0;
                    return data[idx];
            }
        }


        /// <summary>
        /// 이항 계수 가중 이동 중간 값 계산 메서드
        /// - binom[] : 파스칼의 삼각형에서 도출한 이항 계수 배열입니다.
        ///   (예 : w = 2 → [1, 2, 1], w = 3 → [1, 3, 3, 1])
        /// - 중간 값 (median) 계산 시 각 샘플에 이항 계수 만큼의 가중 비율을 부여합니다.
        /// - BoundaryMode 에 따라 경계 밖의 Index 처리 방식을 결정합니다.
        private double WeightedMedianAt(double[] data, int center, int w, long[] binom, BoundaryMode boundaryMode)
        {
            // 값, 가중치 쌍을 담을 리스트 생성 (윈도우 크기 : 2w + 1)
            var pairs = new List<(double Value, long Weight)>(2 * w + 1);

            // 중앙 (Center) 기준으로 좌우 w 개 씩 샘플 수집
            for (int k = -w; k <= w; k++)
            {
                int idx = center + k;
                // 경계 처리된 값 가져오기 (Symmetric, Replicate, ZeroPad)
                double v = GetValueWithBoundary(data, idx, boundaryMode);

                // 파스칼의 삼각형에서 가져온 이항계수 가중치 적용
                // binom[k + w] 는 k 위치에 해당하는 가중치
                long weight = binom[k + w];
                pairs.Add((v, weight));
            }

            if (pairs.Count == 0) return 0.0;

            // 중간 값 계산을 위한 오름차순 정렬
            pairs.Sort((a, b) => a.Value.CompareTo(b.Value));

            // 전체 가중치 합 계산
            long totalWeight = 0;
            for (int i = 0; i < pairs.Count; i++)
                totalWeight += pairs[i].Weight;

            if (totalWeight <= 0) return 0.0;

            // 전체 가중치 짝수 여부 확인
            bool even = (totalWeight & 1L) == 0;
            long half = totalWeight / 2;

            // 누적 가중치 합을 이용해 중간 값 위치 찾기
            long accum = 0;
            for (int i = 0; i < pairs.Count; i++)
            {
                accum += pairs[i].Weight;

                // 누적 가중치가 절반을 초과하면 해당 값이 중간 값
                if (accum > half)
                {
                    return pairs[i].Value;
                }

                // 짝수 가중치 합이고 정확히 절반에 도달하면
                // 다음 값과 평균을 내어 중간 값 결정
                if (even && accum == half)
                {
                    double nextVal = (i + 1 < pairs.Count) ? pairs[i + 1].Value : pairs[i].Value;
                    return (pairs[i].Value + nextVal) / 2.0;
                }
            }

            // 모든 경우를 통과하지 않은 경우 마지막 값 반환
            return pairs[pairs.Count - 1].Value;
        }

        /// <summary>
        /// 주어진 길이(length)에 해당하는 이항계수(파스칼 삼각형의 한 행)를 계산하여 반환합니다.
        /// 예: length = 5 → [1, 4, 6, 4, 1]
        /// 
        /// - 파스칼의 삼각형 n 번째 행의 각 원소는 이항계수 C(n, k) = n! / (k! * (n - k)!) 로 계산됩니다.
        /// - 여기서는 length개의 계수를 구하므로, n = length - 1 에 해당하는 행을 생성합니다.
        /// - 이항계수는 대칭적이며, Binomial Average, Weighted Median 등 스무딩 필터의 가중치로 자주 사용됩니다.
        /// 
        /// - 첫 번째 계수 C(n, 0)은 항상 1
        /// - 이후 C(n, k) = C(n, k-1) * (n - (k-1)) / k 공식을 이용해 반복 계산
        /// - checked 블록으로 Overflow 감지
        /// 
        /// <param name="length">Kernel (윈도우) 의 크기. 반드시 1 이상이어야 합니다.</param>
        private static long[] CalcBinomialCoefficients(int length)
        {
            if (length < 1)
                throw new ArgumentException("length must be ≥ 1", nameof(length));

            // 64비트 long 범위 내에서 안전하게 합계를 계산하기 위해 length 제한
            // (2 ^ (length - 1) ≤ 2 ^ 62 조건)
            if (length > 63)
                throw new ArgumentOutOfRangeException(nameof(length),
                    "length must be ≤ 63 to avoid 64-bit weight sum overflow (2 ^ (length - 1) <= 2 ^ 62). Reduce kernel radius.");

            var c = new long[length];
            c[0] = 1; // 첫 번째 계수는 항상 1


            try
            {
                checked // Overflow 발생 시 예외 발생
                {
                    for (int i = 1; i < length; i++)
                        c[i] = c[i - 1] * (length - i) / i;
                }
            }
            catch (OverflowException ex)
            {
                throw new InvalidOperationException(
                    $"Binomial coefficient overflow for length = {length}. Try a smaller kernel radius.", ex);
            }
            return c;
        }

        // Savitzky-Golay 보정 방식 : 계수 계산 메서드
        private static double[] ComputeSavitzkyGolayCoefficients(int windowSize, int polyOrder)
        {
            if (windowSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(windowSize), "windowSize must be > 0.");
            if ((windowSize & 1) == 0)
                throw new ArgumentException("windowSize must be odd (2 * r + 1).", nameof(windowSize));
            if (polyOrder < 0)
                throw new ArgumentOutOfRangeException(nameof(polyOrder), "polyOrder must be ≥ 0.");
            if (polyOrder >= windowSize)
                throw new ArgumentException("polyOrder must be < windowSize.", nameof(polyOrder));

            int m = polyOrder;
            int half = windowSize / 2;
            var A = new double[windowSize, m + 1];

            // Build Vandermonde matrix
            for (int i = -half; i <= half; i++)
            {
                double x = i;
                double pow = 1.0;
                for (int j = 0; j <= m; j++)
                {
                    A[i + half, j] = pow;
                    pow *= x;
                }
            }

            // Compute ATA = A^T * A
            var ATA = new double[m + 1, m + 1];
            for (int i = 0; i <= m; i++)
                for (int j = 0; j <= m; j++)
                {
                    double s = 0;
                    for (int k = 0; k < windowSize; k++)
                        s += A[k, i] * A[k, j];
                    ATA[i, j] = s;
                }

            // 실패 시 예외 발생
            var invATA = InvertMatrixStrict(ATA);

            // A^T
            var AT = new double[m + 1, windowSize];
            for (int i = 0; i <= m; i++)
                for (int k = 0; k < windowSize; k++)
                    AT[i, k] = A[k, i];

            // h = e0^T * (A^T A)^(-1) * A^T  (0 번째 행 선택)
            var h = new double[windowSize];
            for (int k = 0; k < windowSize; k++)
            {
                double sum = 0;
                for (int j = 0; j <= m; j++)
                    sum += invATA[0, j] * AT[j, k];
                h[k] = sum;
            }

            // Smoothing Kernel 의 계수 합이 1 이 되도록 정규화.
            // (필터 적용 후 전체 값의 크기가 변하지 않도록 Kernel 을 비례 조정.)
            double hSum = 0;
            for (int i = 0; i < windowSize; i++) hSum += h[i];
            if (Math.Abs(hSum) < 1e-20)
                throw new InvalidOperationException("Computed Savitzky-Golay coefficients sum to ~ 0.");
            for (int i = 0; i < windowSize; i++) h[i] /= hSum;

#if DEBUG
            // 선택 : 다항식 재현성 (polynomial reproduction) 을 확인.
            // (차수 ≤ m 조건을 만족하는 다항식에 대해 본래 함수를 재현하는지 여부 검증)

            for (int deg = 0; deg <= m; deg++)
            {
                double acc = 0;
                for (int i = -half; i <= half; i++)
                {
                    double val = 1.0;
                    if (deg > 0)
                        val = Math.Pow(i, deg);
                    acc += h[i + half] * val;
                }

                // 중심점 x = 0 ^ {\text{deg}} 에서의 기대 값:
                // 차수 (degree) > 0 일 때 : 0
                // 차수 = 0 일 때 : 1

                double expected = (deg == 0) ? 1.0 : 0.0;
                if (Math.Abs(acc - expected) > 1e-8)
                    System.Diagnostics.Debug.WriteLine($"[SG CHECK] Degree {deg} reproduction deviation: {acc - expected}");
            }
#endif
            return h;
        }

        // 행렬 역행렬 (실패 시 예외 Throw)

        private static double[,] InvertMatrixStrict(double[,] a)
        {
            int n = a.GetLength(0);
            if (a.GetLength(1) != n)
                throw new ArgumentException("Matrix must be square.", nameof(a));

            var aug = new double[n, 2 * n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    aug[i, j] = a[i, j];
                aug[i, n + i] = 1.0;
            }

            for (int i = 0; i < n; i++)
            {
                // Pivot Selection:
                // 현재 열 i 에서 절대값이 가장 큰 원소 탐색
                // 수치적 안정성을 높이고, 0 또는 매우 작은 Pivot 으로 인한 계산 불안정을 방지.

                int maxRow = i;
                double maxVal = Math.Abs(aug[i, i]);
                for (int r = i + 1; r < n; r++)
                {
                    double v = Math.Abs(aug[r, i]);
                    if (v > maxVal)
                    {
                        maxVal = v;
                        maxRow = r;
                    }
                }

                // Row Swap:
                // 찾은 Pivot 행 (maxRow) 이 현재 행 (i) 와 다르면 두 행을 교환.
                // Pivot 을 현재 위치로 가져와 이후 소거 단계에서 사용.

                if (maxRow != i)
                {
                    for (int c = 0; c < 2 * n; c++)
                    {
                        double tmp = aug[i, c];
                        aug[i, c] = aug[maxRow, c];
                        aug[maxRow, c] = tmp;
                    }
                }

                // Pivot 요소 확인.
                double pivot = aug[i, i];
                double rowScale = 0;
                for (int c = i; c < n; c++)
                    rowScale = Math.Max(rowScale, Math.Abs(aug[i, c]));
                double tol = Math.Max(rowScale * 1e-14, double.Epsilon);
                if (Math.Abs(pivot) < tol)
                    throw new InvalidOperationException("Matrix is singular or ill-conditioned for inversion.");

                // 피벗이 있는 행 정규화
                for (int c = 0; c < 2 * n; c++)
                    aug[i, c] /= pivot;

                // 소거 (Eliminate)
                for (int r = 0; r < n; r++)
                {
                    if (r == i) continue;
                    double factor = aug[r, i];
                    if (Math.Abs(factor) < 1e-20) continue;
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

        // 입력된 파라미터의 유효성 검사 메서드 (윈도우 크기 및 다항식 차수)
        private OperationResult ValidateSmoothingParameters(int dataCount, int w, int polyOrder)
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
                return OperationResult.Fail(msg);
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
                return OperationResult.Fail(msg);
            }

            return OperationResult.OK(); // 모든 조건 만족 후 통과 시 Ok 반환
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
            _ctsInitSelectAll?.Cancel();                            // 이전에 진행 중인 작업 중단
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

        public void SetComboValues(string kernelRadius, string polyOrder, string boundaryMethod)
        {
            cbxKernelRadius.Text = kernelRadius;
            cbxPolyOrder.Text = polyOrder;
            cbxBoundaryMethod.Text = boundaryMethod;
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

private async Task AddItemsInBatches(ListBox box, double[] items, IProgress<int> progress, int baseProgress)
{
    const int BatchSize = 1000;
    int total = items.Length;
    int done = 0;
    box.BeginUpdate();
    try
    {
        while (done < total)
        {
            int cnt = Math.Min(BatchSize, total - done);
            var buffer = new object[cnt];
            for (int i = 0; i < cnt; i++)
                buffer[i] = items[done + i];
            box.Items.AddRange(buffer);
            done += cnt;
            int pct = baseProgress + (int)((long)done * (100 - baseProgress) / total);
            progress.Report(pct);
            await Task.Yield();
        }
    }
    finally
    {
        box.EndUpdate();
        box.TopIndex = box.Items.Count - 1;
    }
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
            btnInitPaste.Enabled = true;
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
                SetBoundaryMethod("Symmetric");
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
            cbxBoundaryMethod.SelectedIndex = 1;

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
            settingsForm.cbxBoundaryMethod.Text = cbxBoundaryMethod.Text;

            UpdatelbInitDataBtnsState(null, EventArgs.Empty);
            UpdatelbRefinedDataBtnsState(null, EventArgs.Empty);

            dataCount = lbInitData.Items.Count;

            // ComboBox 값으로부터 Parsing
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

            int r = 4, polyOrder = 3, n = 0;
            bool doRect = false, doAvg = false, doMed = false, doGauss = false, doSG = false;
            var boundaryMode = GetBoundaryMode();
            string excelTitle = "";
            double[] initialData = null;

            // UI 에서 설정 값 및 데이터 읽기
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    r = int.TryParse(cbxKernelRadius.Text, out var tmpW) ? tmpW : 2;
                    polyOrder = int.TryParse(cbxPolyOrder.Text, out var tmpP) ? tmpP : 2;
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
                r = int.TryParse(cbxKernelRadius.Text, out var tmpW) ? tmpW : 2;
                polyOrder = int.TryParse(cbxPolyOrder.Text, out var tmpP) ? tmpP : 2;
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
            long[] binom = CalcBinomialCoefficients(2 * r + 1);

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
                    ApplySmoothing(initialData, r, polyOrder, boundaryMode, doRect, doAvg, doMed, doGauss, doSG);

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
                1 + // Boundary Method
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
                        await sw.WriteLineAsync($"Boundary Method : {GetBoundaryMethodText(boundaryMode)}");

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

        // ComboBox 텍스트로부터 BoundaryMode Parsing
        private string GetBoundaryMethodText(BoundaryMode mode)
        {
            switch (mode)
            {
                case BoundaryMode.Symmetric: return "Symmetric (Mirror)";
                case BoundaryMode.Replicate: return "Replicate (Nearest)";
                case BoundaryMode.ZeroPad: return "Zero Padding";
                default: return "Symmetric (Mirror)";
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

            int r = int.TryParse(cbxKernelRadius.Text, out var tmpW) ? tmpW : 2;
            int polyOrder = int.TryParse(cbxPolyOrder.Text, out var tmpP) ? tmpP : 2;
            var boundaryMode = GetBoundaryMode();

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
            long[] binom = CalcBinomialCoefficients(2 * r + 1);

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
                //  변수 Shadowing 을 방지하기 위해, ApplySmoothing() 은 Loop 밖에서 1 회만 호출.
                var (rectAvgResult, binomAvgResult, medianResult, gaussResult, sgResult) =
                    ApplySmoothing(initialData, r, polyOrder, boundaryMode, doRect, doAvg, doMed, doGauss, doSG);

                // 결과를 내보내기 위해 배열에 값을 할당.
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
                ws.Cells[7, 1] = $"Boundary Method : {GetBoundaryMethodText(boundaryMode)}";

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
            if (int.TryParse(cbxKernelRadius.Text, out var r))
            {
                settingsForm.KernelRadius = r;
                settingsForm.cbxKernelRadius.Text = cbxKernelRadius.Text;
            }
        }

        private void cbxPolyOrder_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (int.TryParse(cbxPolyOrder.Text, out var p))
            {
                settingsForm.PolyOrder = p;
                settingsForm.cbxPolyOrder.Text = cbxPolyOrder.Text;
            }
        }

        private void cbxBoundaryMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            settingsForm.BoundaryMethod = cbxBoundaryMethod.SelectedIndex;
            settingsForm.cbxBoundaryMethod.Text = cbxBoundaryMethod.Text;
        }

        private void btnExportSettings_Click(object sender, EventArgs e)
        {
            settingsForm.ApplyParameters(cbxKernelRadius.Text, cbxPolyOrder.Text, cbxBoundaryMethod.Text);
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

        private void SetBoundaryMethod(string target)
        {
            if (string.IsNullOrWhiteSpace(target) || cbxBoundaryMethod.Items.Count == 0)
                return;

            for (int i = 0; i < cbxBoundaryMethod.Items.Count; i++)
            {
                var txt = cbxBoundaryMethod.Items[i]?.ToString();
                if (string.Equals(txt, target, StringComparison.OrdinalIgnoreCase))
                {
                    cbxBoundaryMethod.SelectedIndex = i;
                    // Keep settings form in sync if it exists
                    if (settingsForm != null)
                    {
                        settingsForm.BoundaryMethod = i;
                        settingsForm.cbxBoundaryMethod.Text = txt;
                    }
                    return;
                }
            }
        }

        private void rbtnRect_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnRect.Checked == true)
            {
                SetBoundaryMethod("Replicate");
            }
        }

        private void rbtnAvg_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnAvg.Checked == true)
            {
                SetBoundaryMethod("Symmetric");
            }
        }

        private void rbtnMed_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnMed.Checked == true)
            {
                SetBoundaryMethod("Symmetric");
            }
        }

        private void rbtnGauss_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnGauss.Checked == true)
            {
                SetBoundaryMethod("Symmetric");
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

        #region Mouse Hover and Leave Events
        private void MouseLeaveHandler(object sender, EventArgs e)
        {
            if (isRefinedLoading || lbRefinedData.Items.Count == 0)
            {
                slblDesc.Text = "To start smoothing, add data to the Initial Dataset, choose a Smoothing Method, and set Smoothing Parameters.";
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
                ? $"Copy all {totalCount} items from the Initial Dataset to the clipboard."
                : selCount == 1
                    ? "Copy the selected item from the Initial Dataset to the clipboard."
                    : $"Copy {selCount} selected items from the Initial Dataset to the clipboard.";
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
            int selCount = lbRefinedData.SelectedIndices.Count;
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
                slblDesc.Text = "To start smoothing, add data to the Initial Dataset, choose a Smoothing Method, and set Smoothing Parameters.";
            else
                slblDesc.Text = "Click to start the smoothing process with the selected method and parameters.";

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
                slblDesc.Text = "To export, first smooth the data using the Start Smoothing button.";
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

        private void lblBoundaryMethod_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Visible = true;
            slblDesc.Text = "Specifies how edge data points are treated during smoothing : Symmetric (mirror), Replicate (repeat), or Zero-Pad (fill with zero).";
        }

        private void lblBoundaryMethod_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void cbxBoundaryMethod_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Visible = true;
            slblDesc.Text = "Specifies how edge data points are treated during smoothing : Symmetric (mirror), Replicate (repeat), or Zero-Pad (fill with zero).";
        }

        private void cbxBoundaryMethod_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }
    }
    #endregion
}