Imports System.Globalization
Imports System.IO
Imports System.Net.Mime.MediaTypeNames
Imports System.Runtime.InteropServices
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Windows.Forms.Application
Imports Excel = Microsoft.Office.Interop.Excel

Public Class FrmMain
    Private isRefinedLoading As Boolean = False

    Dim borderCount As Integer

    Dim dpivalue As Double
    Dim g As Graphics = Me.CreateGraphics
    Dim dpiX = g.DpiX.ToString()
    Dim dpiY = g.DpiY.ToString()

    Private Const ExcelTitlePlaceholder As String = "Click here to enter a title for your dataset."

    Private _isShowingTitleValidationMessage As Boolean
    Private _lastInvalidTitle As String

    Private initList As New List(Of Double)
    Private refinedList As New List(Of Double)

    Private Shared ReadOnly patternFindNumbers As String =
        "[+-]?(\d+(,\d{3})*|(?=\.\d))((\.\d+([eE][+-]\d+)?)|)"
    Private Shared ReadOnly patternHtmlParse As String =
        "(?<=>.*)[+-]?" & patternFindNumbers & "(?=[^>]*<)"

    Private Shared ReadOnly regexNumbers As New Regex(patternFindNumbers, RegexOptions.Compiled)
    Private Shared ReadOnly regexHtmlNumbers As New Regex(patternHtmlParse, RegexOptions.Compiled)
    Private Shared ReadOnly regexStripTags As New Regex("<.*?>", RegexOptions.Compiled)

    Private Enum BoundaryMode
        Symmetric
        Replicate
        ZeroPad
        Adaptive
    End Enum

    Private Function GetSelectedBoundaryMode() As BoundaryMode
        Select Case cbxBoundaryMethod.SelectedItem?.ToString()
            Case "Adaptive" : Return BoundaryMode.Adaptive
            Case "Replicate" : Return BoundaryMode.Replicate
            Case "Zero Padding" : Return BoundaryMode.ZeroPad
            Case Else : Return BoundaryMode.Symmetric
        End Select
    End Function

    ' 표준 메시지 빌더 (고급)
    Private Shared Function BuildParamErrorWindowTooLarge(radius As Integer, windowSize As Integer, dataCount As Integer) As String
        Return $"Kernel radius is too large.{Environment.NewLine}{Environment.NewLine}" &
               $"Window size formula : (2 × radius) + 1{Environment.NewLine}" &
               $"Current : (2 × {radius}) + 1 = {windowSize}{Environment.NewLine}" &
               $"Data count : {dataCount}{Environment.NewLine}{Environment.NewLine}" &
               $"Rule : windowSize ≤ dataCount{Environment.NewLine}" &
               $"Result : {windowSize} ≤ {dataCount} → Violation"
    End Function

    Private Shared Function BuildParamErrorBorderTooLarge(borderCount As Integer, dataCount As Integer) As String
        Return $"Border count is too large.{Environment.NewLine}{Environment.NewLine}" &
               $"Rule : borderCount ≤ dataCount{Environment.NewLine}" &
               $"Result : {borderCount} ≤ {dataCount} → Violation"
    End Function

    Private Shared Function BuildParamErrorBorderWidth(borderCount As Integer, windowSize As Integer) As String
        Return $"Border width is too large relative to the window size.{Environment.NewLine}{Environment.NewLine}" &
               $"Tip : windowSize = (2 × radius) + 1{Environment.NewLine}" &
               $"Rule : 2 × borderCount < windowSize{Environment.NewLine}" &
               $"Result : 2 × {borderCount} < {windowSize} → Violation"
    End Function

    ' KernelWidthFromRadius - Overflow 방지 기능 포함
    Private Shared Function KernelWidthFromRadius(radius As Integer) As Integer
        Dim w64 As Long = CLng(radius) * 2L + 1L
        If w64 > Integer.MaxValue Then Throw New ArgumentOutOfRangeException(NameOf(radius), "Radius is too large.")
        Return CInt(w64)
    End Function

    ' 매개변수 검증 (잘못된 경우 예외 발생)
    Private Shared Sub ThrowIfInvalidSmoothingParameters(dataCount As Integer,
                                                         windowSize As Integer,
                                                         radius As Integer,
                                                         borderCount As Integer,
                                                         useMiddle As Boolean)
        If dataCount < 0 Then Throw New ArgumentOutOfRangeException(NameOf(dataCount), "Data count cannot be negative.")
        If windowSize <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(windowSize), "Window size must be >= 1.")
        If borderCount < 0 Then Throw New ArgumentOutOfRangeException(NameOf(borderCount), "Border count cannot be negative.")

        If windowSize > dataCount Then
            Throw New InvalidOperationException(BuildParamErrorWindowTooLarge(radius, windowSize, dataCount))
        End If
        If borderCount > dataCount Then
            Throw New InvalidOperationException(BuildParamErrorBorderTooLarge(borderCount, dataCount))
        End If
        ' Overflow 방지 : 2 * b >= W  <=>  b >= ceil(W / 2) = (W + 1) \ 2
        If useMiddle AndAlso borderCount >= (windowSize + 1) \ 2 Then
            Throw New InvalidOperationException(BuildParamErrorBorderWidth(borderCount, windowSize))
        End If
    End Sub

    ' 중복된 메시지 박스 표시를 피하기 위한 1 회 표시
    Private NotInheritable Class MessageOnceGate
        Private Sub New()
        End Sub
        Private Shared ReadOnly _shown As New HashSet(Of String)(StringComparer.Ordinal)
        Private Shared ReadOnly _sync As New Object()
        Public Shared Function TryEnter(message As String, Optional holdMillis As Integer = 2000) As Boolean
            SyncLock _sync
                If _shown.Contains(message) Then Return False
                _shown.Add(message)
                Dim t As System.Threading.Timer = Nothing
                t = New System.Threading.Timer(
                    Sub(state As Object)
                        SyncLock _sync
                            _shown.Remove(message)
                        End SyncLock
                        t.Dispose()
                    End Sub,
                    Nothing, holdMillis, Threading.Timeout.Infinite)
                Return True
            End SyncLock
        End Function
    End Class

    ' UI 에서 사용하는 Wrapper : 검증 후 표준 메시지를 표시
    Private Function ValidateSmoothingParametersCanonical(dataCount As Integer,
                                                          radius As Integer,
                                                          borderCount As Integer,
                                                          useMiddle As Boolean) As Boolean
        Try
            Dim windowSize = KernelWidthFromRadius(radius)
            ThrowIfInvalidSmoothingParameters(dataCount, windowSize, radius, borderCount, useMiddle)
            Return True
        Catch ex As Exception
            Dim msg = ex.Message
            If MessageOnceGate.TryEnter(msg) Then
                MessageBox.Show(msg, "Parameter Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
            Return False
        End Try
    End Function

    ' Boundary value 합성은 SignatureMedian.GetValueWithBoundary 와 동일
    Private Shared Function GetValueWithBoundary(data As Double(), idx As Integer, mode As BoundaryMode) As Double
        Dim n As Integer = If(data Is Nothing, 0, data.Length)
        If n = 0 Then Return 0.0

        Select Case mode
            Case BoundaryMode.Symmetric
                If n = 1 Then Return data(0)
                Dim period As Long = 2L * (CLng(n) - 1L)
                Dim m As Long = CLng(idx) Mod period
                If m < 0 Then m += period
                Dim mapped As Long = If(m < n, m, 2L * (CLng(n) - 1L) - m)
                Return data(CInt(mapped))

            Case BoundaryMode.Replicate
                If idx < 0 Then
                    idx = 0
                ElseIf idx >= n Then
                    idx = n - 1
                End If
                Return data(idx)

            Case BoundaryMode.ZeroPad
                If idx < 0 OrElse idx >= n Then Return 0.0
                Return data(idx)

            Case BoundaryMode.Adaptive
                ' Adaptive 모드로 직접 샘플링하는 경우, Symmetric 과 동일한 반사 로직으로 사용 
                If n = 1 Then Return data(0)
                Dim period As Long = 2L * (CLng(n) - 1L)
                Dim m As Long = CLng(idx) Mod period
                If m < 0 Then m += period
                Dim mapped As Long = If(m < n, m, 2L * (CLng(n) - 1L) - m)
                Return data(CInt(mapped))

            Case Else
                If n = 1 Then Return data(0)
                Dim period As Long = 2L * (CLng(n) - 1L)
                Dim m As Long = CLng(idx) Mod period
                If m < 0 Then m += period
                Dim mapped As Long = If(m < n, m, 2L * (CLng(n) - 1L) - m)
                Return data(CInt(mapped))
        End Select
    End Function

    Public Sub New()
        InitializeComponent()

        Me.KeyPreview = True
    End Sub

    'Sub MiddleMedian()
    '    Dim n = initList.Count
    '    If n = 0 Then Return

    '    Dim arr = initList.ToArray()
    '    Dim buffer(n - 1) As Double

    '    buffer(0) = arr(0)
    '    If n > 1 Then buffer(1) = arr(1)
    '    If n > 2 Then buffer(n - 2) = arr(n - 2)
    '    buffer(n - 1) = arr(n - 1)

    '    Parallel.For(2, n - 2, Sub(i)
    '                               Dim win(4) As Double
    '                               ' i - 2 ... i + 2 복사
    '                               For k = 0 To 4
    '                                   win(k) = arr(i + k - 2)
    '                               Next
    '                               Quicksort(win, 0, 4)
    '                               buffer(i) = win(2)
    '                           End Sub)

    '    refinedList.Clear()
    '    refinedList.AddRange(buffer)
    'End Sub


    'Sub AllMedian()
    '    Dim n = initList.Count
    '    If n = 0 Then Return

    '    Dim arr = initList.ToArray()
    '    Dim buffer(n - 1) As Double

    '    Parallel.For(0, n, Sub(i)
    '                           Dim iMin = If(i < 2, 0, i - 2)
    '                           Dim iMax = If(i > n - 3, n - 1, i + 2)
    '                           Dim win(4) As Double
    '                           Dim k = 0
    '                           For j = iMin To iMax
    '                               win(k) = arr(j)
    '                               k += 1
    '                           Next
    '                           Quicksort(win, 0, 4)
    '                           buffer(i) = win(2)
    '                       End Sub)

    '    refinedList.Clear()
    '    refinedList.AddRange(buffer)
    'End Sub

    ' ---------------------------------------------------------------
    ' ComputeMedians  : initList 에서 refinedList 로 이동하며 실행 중간 값을 계산합니다.
    ' useMiddle       : True 일 경우, 처음과 끝의 'borderCount' 개수는 원래 값 그대로 유지합니다.
    ' kernelSize      : 중간 값 커널의 윈도우 길이 (보통 홀수). 반지름 r 이 있으면
    '                   kernelSize = (2 * r) + 1 로 계산합니다.
    ' borderCount     : Middle 모드에서 양쪽 경계 밖에 그대로 남겨 둘 항목의 개수.
    ' progress        : 진행 상황을 보고하기 위한 IProgress(Of Integer).
    ' boundaryMode    : 경계 처리 모드 (useMiddle = False 일 경우에만 사용).
    ' ---------------------------------------------------------------

    Private Sub ComputeMedians(
    useMiddle As Boolean,
    kernelSize As Integer,
    borderCount As Integer,
    progress As IProgress(Of Integer),
    Optional boundaryMode As BoundaryMode = BoundaryMode.Symmetric
)
        ' 전체 데이터 개수 취득
        Dim n = initList.Count

        ' 데이터가 없으면 진행률 0 보고 후 반환
        If n = 0 Then
            progress.Report(0)
            Return
        End If

        ' 원본 데이터를 배열 형태로 복사
        Dim arr = initList.ToArray()

        ' 결과를 저장할 버퍼 배열
        Dim buffer(n - 1) As Double

        ' 커널 (Kernel) 반경을 양쪽으로 분리 (홀수 · 짝수 창 모두 지원)
        Dim offsetLow = (kernelSize - 1) \ 2
        Dim offsetHigh = (kernelSize - 1) - offsetLow

        ' 진행 중 항목 수 계산 및 보고 간격 설정 (약 0.5 % 단위)
        Dim processed As Integer = 0
        Dim reportInterval = Math.Max(1, n \ 200)
        progress.Report(0)

        ' 각 Thread 마다 고유한 윈도우 배열을 제공하는 ThreadLocal
        Dim localWin As New ThreadLocal(Of Double())(
        Function() New Double(kernelSize - 1) {}
    )

        ' 보정하지 않고 원본 값을 사용할 요소 구간 개수를 원본 그대로 유지
        ' (useMiddle = True 인 경우에 한함)
        If useMiddle Then
            For i As Integer = 0 To borderCount - 1
                buffer(i) = arr(i)
                buffer(n - 1 - i) = arr(n - 1 - i)
                processed += 2
                If processed Mod reportInterval = 0 Then
                    progress.Report(Math.Min(processed, n))
                End If
            Next
        End If

        ' 중간 값 계산을 시작할 인덱스와 끝 인덱스 결정
        Dim startIdx = If(useMiddle, borderCount, 0)
        Dim endIdx = If(useMiddle, n - borderCount - 1, n - 1)

        If startIdx > endIdx Then
            refinedList.Clear()
            refinedList.AddRange(arr)
            progress.Report(n)
            Return
        End If

        Parallel.For(startIdx, endIdx + 1, Sub(i)
                                               Dim win = localWin.Value

                                               If useMiddle Then
                                                   ' 가장자리 근처의 Legacy 동작 (가변 길이, 클램프됨)
                                                   Dim iMin = Math.Max(0, i - offsetLow)
                                                   Dim iMax = Math.Min(n - 1, i + offsetHigh)
                                                   Dim length = iMax - iMin + 1
                                                   For k As Integer = 0 To length - 1
                                                       win(k) = arr(iMin + k)
                                                   Next
                                                   buffer(i) = GetWindowMedian(win, length)
                                               Else
                                                   ' AllMedian: BoundaryMode 적용
                                                   If boundaryMode = BoundaryMode.Adaptive Then
                                                       Dim desiredW As Integer = kernelSize
                                                       Dim W As Integer = Math.Min(desiredW, n)
                                                       Dim start As Integer = i - offsetLow
                                                       If start < 0 Then start = 0
                                                       If start > n - W Then start = n - W
                                                       If start < 0 Then start = 0
                                                       For pos As Integer = 0 To W - 1
                                                           win(pos) = arr(start + pos)
                                                       Next
                                                       buffer(i) = GetWindowMedian(win, W)
                                                   Else
                                                       For pos As Integer = 0 To kernelSize - 1
                                                           Dim k As Integer = pos - offsetLow
                                                           win(pos) = GetValueWithBoundary(arr, i + k, boundaryMode)
                                                       Next
                                                       buffer(i) = GetWindowMedian(win, kernelSize)
                                                   End If
                                               End If

                                               Dim cnt = Interlocked.Increment(processed)
                                               If cnt Mod reportInterval = 0 Then progress.Report(Math.Min(n, cnt))
                                           End Sub)

        ' 최종 진행률 보고
        progress.Report(n)

        ' 기존 refinedList 에 항목이 존재하는 경우, 전체 삭제 후 보정된 항목으로 대체
        refinedList.Clear()
        refinedList.AddRange(buffer)
    End Sub

    ' ---------------------------------------------------------------
    ' 반지름 (radius) 값을 받아 (2 * radius + 1) 윈도우 길이로 변환 후
    ' 내부 메서드 <see cref="ComputeMedians"/> 를 호출하는 간단한 Wrapper 입니다
    ' useMiddle    : True 이면 Middle 모드로 양쪽에서 <paramref name="borderCount"/> 개 항목을 원본 그대로 둡니다.
    ' radius       : 커널 반지름 값. 실제 윈도우 길이는 (2 * radius) + 1 로 변환됩니다.
    ' borderCount  : Middle 모드에서 양쪽 경계에 보존할 원소 개수.
    ' progress     : 진행 상황을 보고할 IProgress(Of Integer) 구현.
    ' boundaryMode : useMiddle = False (AllMedian 모드) 일 경우 적용할 경계 처리 방식.
    ' ---------------------------------------------------------------
    Private Sub ComputeMediansByRadius(
    useMiddle As Boolean,
    radius As Integer,
    borderCount As Integer,
    progress As IProgress(Of Integer),
    Optional boundaryMode As BoundaryMode = BoundaryMode.Symmetric
)
        Dim windowSize As Integer = KernelWidthFromRadius(radius)
        ComputeMedians(useMiddle, windowSize, borderCount, progress, boundaryMode)
    End Sub

    ' ---------------------------------------------------------------
    ' GetWindowMedian  : 주어진 배열 조각에서 중간 값을 반환합니다.
    ' win()            : 값이 채워진 배열 (KernelRadius × 2 + 1)
    ' length           : 유효한 값이 들어 있는 요소 개수
    ' ---------------------------------------------------------------
    Private Function GetWindowMedian(win() As Double, length As Integer) As Double
        ' 유효 구간만 잘라낸 새 배열 생성
        Dim slice = win.Take(length).ToArray()

        ' 오름차순 정렬
        Array.Sort(slice)

        ' 중간 인덱스 계산
        Dim mid = length \ 2

        ' 짝수인 경우 인접 두 값의 평균, 홀수일 때 가운데 값 반환
        ' (짝수 창도 지원하지만 일반적으로 홀수 창을 권장)
        If length Mod 2 = 0 Then
            Return (slice(mid - 1) + slice(mid)) / 2.0
        Else
            Return slice(mid)
        End If
    End Function

    Private Sub btnInitAdd_Click(sender As Object, e As EventArgs) Handles btnInitAdd.Click
        Dim inputText = txtInitAdd.Text
        Dim v As Double

        If Double.TryParse(inputText, v) Then
            lbInitData.Items.Add(v)
            lblInitCnt.Text = $"Count : {lbInitData.Items.Count}"
            slblDesc.Visible = True
            slblDesc.Text = $"Value '{v}' has been added to Initial Dataset."
        Else
            txtInitAdd.Focus()
            txtInitAdd.SelectAll()
        End If

        If lbInitData.Items.Count > 0 Then
            UpdatelbInitDataButtonsState(Nothing, EventArgs.Empty)
            UpdatelbRefinedDataButtonsState(Nothing, EventArgs.Empty)
        End If

        txtInitAdd.Text = String.Empty
        txtInitAdd.Select()
    End Sub

    ' ---------------------------------------------------------------            
    ' 빠른 정렬 (QuickSort) 알고리즘
    ' list() : 정렬할 실수 배열
    ' min    : 정렬 구간의 시작 Index
    ' max    : 정렬 구간의 끝 Index    
    ' ---------------------------------------------------------------
    Public Sub Quicksort(ByVal list() As Double, ByVal min As Integer, ByVal max As Integer)
        Dim random_number As New Random  ' 무작위 Pivot 값 선택을 위한 Random 객체
        Dim med_value As Double          ' 피벗 값 저장 변수
        Dim hi As Integer                ' 오른쪽 Index
        Dim lo As Integer                ' 왼쪽 Index
        Dim i As Integer                 ' 피벗 위치를 선택할 임시 변수

        ' 최소 구간이 최대 구간 이상이면 정렬 불필요
        If min >= max Then Exit Sub

        ' 피벗 위치를 무작위로 선택
        i = random_number.Next(min, max + 1)
        med_value = list(i)              ' Pivot 값 저장


        list(i) = list(min)

        ' 좌 / 우 포인터 초기화
        lo = min
        hi = max

        ' 파티션 나누기
        Do
            ' 오른쪽 포인터를 Pivot 보다 작은 값을 찾을 때까지 이동                       
            Do While list(hi) >= med_value
                hi = hi - 1
                If hi <= lo Then Exit Do
            Loop

           ' 포인터가 교차되는 지점이 Pivot 위치
            If hi <= lo Then
                list(lo) = med_value
                Exit Do
            End If

            ' 작은 값을 왼쪽으로 이동 
            list(lo) = list(hi)

            ' 왼쪽 포인터에 대해 Pivot 보다 크거나 같은 값을 찾을 때까지 이동
            lo = lo + 1
            Do While list(lo) < med_value
                lo = lo + 1
                If lo >= hi Then Exit Do ' 포인터 교차되는 지점에서 중단
            Loop

            ' 포인터가 교차되는 지점이 Pivot 위치                                
            If lo >= hi Then
                lo = hi
                list(hi) = med_value
                Exit Do
            End If

            ' 큰 값을 오른쪽으로 이동                                    
            list(hi) = list(lo)
        Loop
                                            
        ' Pivot 을 기준으로 왼쪽 구간 정렬
        Quicksort(list, min, lo - 1)
                                            
        ' Pivot 을 기준으로 오른쪽 구간 정렬
        Quicksort(list, lo + 1, max)
    End Sub

    ' Running Median 이동 중간 값 보정 (Calibrate) 버튼 클릭 이벤트
    Private Async Sub btnCalibrate_Click(sender As Object, e As EventArgs) Handles btnCalibrate.Click
        ' 초기 데이터 유효성
        If lbInitData.Items.Count = 0 Then
            Return
        End If

        ' 원본 데이터 파싱
        Dim parsedList As New List(Of Double)
        For Each item As Object In lbInitData.Items
            Dim strValue As String = item?.ToString()
            Dim dValue As Double
            If Double.TryParse(strValue, dValue) Then
                parsedList.Add(dValue)
            Else
                MessageBox.Show(
                $"The value '{strValue}' could not be converted to a number.",
                "Avocado Smoothie",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )
                Return
            End If
        Next

        initList = parsedList
        Dim total As Integer = initList.Count

        ' 모드 결정 (Middle / All)
        Dim useMiddle As Boolean = rbtnMidMedian.Checked

        ' 커널 파라미터 (UI 는 radius 를 받음 → width 로 변환)
        Dim radius As Integer
        If Not Integer.TryParse(cbxKernelRadius.Text, radius) Then
            MessageBox.Show("Please select a kernel radius.", "Avocado Smoothie",
                        MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        Dim kernelWidth As Integer = 2 * radius + 1

        ' 경계 개수
        Dim borderCount As Integer
        If Not Integer.TryParse(cbxBorderCount.Text, borderCount) Then
            MessageBox.Show("Please select a border count.", "Avocado Smoothie",
                        MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        ' 파라미터 검증
        If Not ValidateSmoothingParametersCanonical(total, radius, borderCount, useMiddle) Then
            Return
        End If

        ' 진행률 표시 초기화
        pbMain.Minimum = 0
        pbMain.Maximum = total
        pbMain.Value = 0

        Dim progress = New Progress(Of Integer)(Sub(v)
                                                    pbMain.Value = Math.Min(v, total)
                                                End Sub)

        ' 연산 중 버튼 비활성화
        btnCalibrate.Enabled = False
        btnInitClear.Enabled = False
        btnInitEdit.Enabled = False
        btnInitPaste.Enabled = False
        btnInitDelete.Enabled = False
        btnInitSelectAll.Enabled = False
        btnInitSelectSync.Enabled = False
        btnRefClear.Enabled = False
        btnRefSelectAll.Enabled = False
        btnRefSelectSync.Enabled = False


        ' 계산 실행 (이 클래스의 ComputeMedians 호출)
        isRefinedLoading = True
        Try
            Dim boundary As BoundaryMode = If(useMiddle, BoundaryMode.Symmetric, GetSelectedBoundaryMode())
            Await Task.Run(Sub()
                               If useMiddle Then
                                   ComputeMedians(True, kernelWidth, borderCount, progress)
                               Else
                                   ComputeMedians(False, kernelWidth, borderCount, progress, boundary)
                               End If
                           End Sub)
        Finally
            isRefinedLoading = False
        End Try

        ' 결과 UI 반영 (refinedList 는 ComputeMedians 내부에서 채움)
        lbRefinedData.BeginUpdate()
        lbRefinedData.Items.Clear()
        lbRefinedData.Items.AddRange(refinedList.Cast(Of Object).ToArray())
        lbRefinedData.EndUpdate()
        lbRefinedData.TopIndex = lbRefinedData.Items.Count - 1

        lblInitCnt.Text = $"Count : {total}"
        lblRefCnt.Text = $"Count : {refinedList.Count}"
        slblCalibratedType.Text = If(useMiddle, "Middle Median", "All Median")
        slblKernelRadius.Text = Integer.Parse(cbxKernelRadius.Text) ' 화면엔 radius 그대로 표기
        slblBorderCount.Text = $"{borderCount}"

        ' Middle 모드일 때만 Border UI 표시
        slblBorderCount.Visible = useMiddle
        tlblBorderCount.Visible = useMiddle
        slblSeparator2.Visible = useMiddle

        ' 버튼 재활성화
        slblDesc.Visible = False
        btnInitPaste.Enabled = True
        btnCalibrate.Enabled = True
        btnInitClear.Enabled = True
        btnInitEdit.Enabled = True
        btnInitDelete.Enabled = True
        btnInitSelectAll.Enabled = True
        btnInitSelectSync.Enabled = True
        btnRefClear.Enabled = True
        btnRefSelectAll.Enabled = True
        btnRefSelectSync.Enabled = True

        ' 상태 반영
        UpdatelbInitDataButtonsState(Nothing, EventArgs.Empty)
        UpdatelbRefinedDataButtonsState(Nothing, EventArgs.Empty)

        ' ProgressBar 초기화
        Await Task.Delay(200)
        pbMain.Value = 0
    End Sub

    Private Sub txtInitAdd_KeyDown(sender As Object, e As KeyEventArgs) Handles txtInitAdd.KeyDown
        If e.KeyCode = Keys.Enter Then
            Dim inputText = txtInitAdd.Text
            Dim v As Double

            If Double.TryParse(inputText, v) Then
                lbInitData.Items.Add(v)
                lblInitCnt.Text = $"Count : {lbInitData.Items.Count}"
                slblDesc.Visible = True
                slblDesc.Text = $"Value '{v}' has been added to Initial Dataset."
            Else
                txtInitAdd.Focus()
                txtInitAdd.SelectAll()
            End If


            UpdatelbInitDataButtonsState(Nothing, EventArgs.Empty)

            txtInitAdd.Clear()
            e.SuppressKeyPress = True
        End If

    End Sub

    Private Sub btnInitCopy_Click(sender As Object, e As EventArgs) Handles btnInitCopy.Click
        Dim doubles As New List(Of Double)
        Dim source = If(lbInitData.SelectedItems.Count > 0,
                    lbInitData.SelectedItems,
                    lbInitData.Items)

        For Each itm As Object In source
            Dim txt = itm.ToString()
            Dim num As Double
            If Double.TryParse(txt, num) Then
                doubles.Add(num)
            Else
                MessageBox.Show($"The value '{txt}' could not be converted to a number.",
                            "Copy Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
        Next

        If doubles.Any() Then
            Clipboard.SetText(String.Join(Environment.NewLine, doubles))

            Dim copiedCount As Integer = doubles.Count
            slblDesc.Visible = True
            slblDesc.Text = If(copiedCount = 1,
                           "Successfully copied 1 item.",
                           $"Successfully copied {copiedCount} items.")
        End If
    End Sub


    Private Sub btnRefCopy_Click(sender As Object, e As EventArgs) Handles btnRefCopy.Click
        Dim doubles As New List(Of Double)
        Dim source = If(lbRefinedData.SelectedItems.Count > 0,
                    lbRefinedData.SelectedItems,
                    lbRefinedData.Items)

        For Each itm As Object In source
            Dim txt = itm.ToString()
            Dim num As Double
            If Double.TryParse(txt, num) Then
                doubles.Add(num)
            Else
                MessageBox.Show($"The value '{txt}' could not be converted to a number.",
                            "Copy Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
        Next

        If doubles.Any() Then
            Clipboard.SetText(String.Join(Environment.NewLine, doubles))

            Dim copiedCount As Integer = doubles.Count
            slblDesc.Visible = True
            slblDesc.Text = If(copiedCount = 1,
                           "Successfully copied 1 item.",
                           $"Successfully copied {copiedCount} items.")
        End If
    End Sub


    Private Sub btnInitClear_Click(sender As Object, e As EventArgs) Handles btnInitClear.Click

        Dim itemCount As Integer = lbInitData.Items.Count
        Dim refItemCount As Integer = lbRefinedData.Items.Count

        Dim itemText As String = If(itemCount = 1, "item", "items")
        Dim refItemText As String = If(refItemCount = 1, "item", "items")

        Dim refMessage As String = If(refItemCount = 0,
                             String.Empty,
                             $"This will also delete all {refItemCount} {refItemText} from the Refined Dataset.")

        Dim message As String

        If String.IsNullOrEmpty(refMessage) Then
            message = $"You are about to delete all {itemCount} {itemText} from the Initial Dataset." &
              $"{vbCrLf}{vbCrLf}Are you sure you want to proceed?"
        Else
            message = $"You are about to delete all {itemCount} {itemText} from the Initial Dataset." &
              $"{vbCrLf}{refMessage}{vbCrLf}{vbCrLf}Are you sure you want to proceed?"
        End If

        Dim result As DialogResult = MessageBox.Show(
            message,
            "Delete Confirmation",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
            )

        If result = DialogResult.No Then
            Return
        End If


        lbInitData.Items.Clear()
        lbRefinedData.Items.Clear()

        txtDatasetTitle.Text = ExcelTitlePlaceholder
        txtDatasetTitle.ForeColor = Color.Gray
        txtDatasetTitle.TextAlign = HorizontalAlignment.Center

        txtInitAdd.Text = String.Empty

        UpdatelbInitDataButtonsState(Nothing, EventArgs.Empty)
        UpdatelbRefinedDataButtonsState(Nothing, EventArgs.Empty)

        slblKernelRadius.Text = "--"
        slblCalibratedType.Text = "--"
        slblBorderCount.Text = "--"

        tlblBorderCount.Visible = False
        slblBorderCount.Visible = False

        slblSeparator2.Visible = False

        lblInitCnt.Text = "Count : " & lbInitData.Items.Count
        lblRefCnt.Text = "Count : " & lbRefinedData.Items.Count

        slblDesc.Visible = True

        Dim initialMsg As String = $"Deleted all {itemCount} item{If(itemCount <> 1, "s", "")} from the initial dataset"
        Dim finalMsg As String = initialMsg

        If refItemCount > 0 Then
            finalMsg &= $" and Deleted all {refItemCount} item{If(refItemCount <> 1, "s", "")} from the Refined Dataset"
        End If

        ' 마침표 추가
        slblDesc.Text = finalMsg + "."


        txtInitAdd.Select()
    End Sub

    Private Sub btnRefClear_Click(sender As Object, e As EventArgs) Handles btnRefClear.Click
        Dim itemCount As Integer = lbRefinedData.Items.Count

        Dim message As String = $"You are about to delete all {itemCount} item{If(itemCount <> 1, "s", "")} from the Refined Dataset." &
                        vbCrLf & vbCrLf &
                        "Are you sure you want to proceed?"

        Dim result As DialogResult = MessageBox.Show(
            message,
            "Delete Confirmation",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
            )

        If result = DialogResult.No Then
            Return
        End If

        lbRefinedData.Items.Clear()
        UpdatelbInitDataButtonsState(Nothing, EventArgs.Empty)
        UpdatelbRefinedDataButtonsState(Nothing, EventArgs.Empty)

        slblKernelRadius.Text = "--"
        slblCalibratedType.Text = "--"
        slblBorderCount.Text = "--"

        tlblBorderCount.Visible = False
        slblBorderCount.Visible = False

        slblSeparator2.Visible = False

        slblDesc.Text = $"Deleted all {itemCount} item{If(itemCount <> 1, "s", "")} from Refined Dataset."
        slblDesc.Visible = True

        lblRefCnt.Text = "Count : " & lbRefinedData.Items.Count
        lbRefinedData.Select()
    End Sub

    Private Async Function DeleteSelectedItemsPreserveSelection(lb As ListBox, progressBar As ProgressBar, lblCount As Label) As Task

        Dim indices = lb.SelectedIndices.Cast(Of Integer)().OrderByDescending(Function(i) i).ToArray()
        Dim total = indices.Length
        If total = 0 Then Return

        progressBar.Minimum = 0
        progressBar.Maximum = total
        progressBar.Value = 0

        Dim newSelections = New List(Of Integer)
        Dim shiftMap = New Dictionary(Of Integer, Boolean)

        For Each idx In indices
            shiftMap(idx) = True
            If idx < lb.Items.Count - 1 Then
                newSelections.Add(idx) ' 동일 위치에 선택 유지 시도
            End If
        Next

        lb.SuspendLayout()
        lb.BeginUpdate()

        Dim updateInterval = Math.Max(1, total \ 100)
        For i = 0 To total - 1
            lb.Items.RemoveAt(indices(i))
            If ((i + 1) Mod updateInterval = 0) OrElse i = total - 1 Then
                progressBar.Value = i + 1
                System.Windows.Forms.Application.DoEvents()
            End If
        Next

        lb.EndUpdate()
        lb.ResumeLayout()

        lb.SelectedIndices.Clear()
        lb.ClearSelected()

        For Each idx In newSelections
            If idx < lb.Items.Count Then lb.SelectedIndices.Add(idx)
        Next

        lblCount.Text = $"Count : {lb.Items.Count}"
        Await Task.Delay(200)
        progressBar.Value = 0
    End Function

    Private Async Sub btnInitDelete_Click(sender As Object, e As EventArgs) Handles btnInitDelete.Click
        Dim totalCount As Integer = lbInitData.Items.Count
        Dim refinedCount As Integer = lbRefinedData.Items.Count
        Dim selectedCount As Integer = lbInitData.SelectedItems.Count

        Dim totalItemText As String = If(totalCount = 1, "item", "items")
        Dim selectedItemText As String = If(selectedCount = 1, "item", "items")
        Dim refItemText As String = If(refinedCount = 1, "item", "items")

        Dim refMessage As String = If(refinedCount = 0,
                               String.Empty,
                               $"This will also delete all {refinedCount} {refItemText} from the Refined Dataset.")

        Dim body As String
        If selectedCount = 0 Then
            body = "No items selected to delete."
        ElseIf selectedCount = totalCount Then
            body = $"You are about to delete all {selectedCount} {totalItemText} from the Initial Dataset." &
           If(String.IsNullOrEmpty(refMessage),
              String.Empty,
              vbCrLf & refMessage)
        Else
            body = $"You are about to delete {selectedCount} selected {selectedItemText} from the Initial Dataset."
        End If

        Dim message As String = body & vbCrLf & vbCrLf & "Are you sure you want to proceed?"

        Dim result As DialogResult = MessageBox.Show(
            message,
            "Delete Confirmation",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
)
        If result = DialogResult.No Then
            Return
        End If

        If selectedCount = totalCount Then
            lbInitData.Items.Clear()
            lbRefinedData.Items.Clear()
            btnInitCopy.Enabled = False
            lblInitCnt.Text = "Count : " & lbInitData.Items.Count
            lblRefCnt.Text = "Count : " & lbRefinedData.Items.Count
            pbMain.Value = 0

            txtDatasetTitle.Text = ExcelTitlePlaceholder
            txtDatasetTitle.ForeColor = Color.Gray
            txtDatasetTitle.TextAlign = HorizontalAlignment.Center

            slblKernelRadius.Text = "--"
            slblCalibratedType.Text = "--"
            slblBorderCount.Text = "--"

            tlblBorderCount.Visible = False
            slblBorderCount.Visible = False

            slblSeparator2.Visible = False

            UpdatelbInitDataButtonsState(Nothing, EventArgs.Empty)
            UpdatelbRefinedDataButtonsState(Nothing, EventArgs.Empty)

            txtInitAdd.Select()
        End If

        Await DeleteSelectedItemsPreserveSelection(lbInitData, pbMain, lblInitCnt)

        slblDesc.Visible = True

        Dim descMessage As String

        If selectedCount = totalCount Then
            Dim refPart As String = If(refinedCount = 0,
                               String.Empty,
                               $" and all {refinedCount} {refItemText} from Refined Dataset")
            descMessage = $"Deleted all {totalCount} {totalItemText} from Initial Dataset{refPart}."
        Else
            descMessage = $"Deleted {selectedCount} selected {selectedItemText} from Initial Dataset."
        End If

        slblDesc.Visible = True
        slblDesc.Text = descMessage

        lblInitCnt.Text = "Count : " & lbInitData.Items.Count

        lbInitData.Select()

        UpdatelbInitDataButtonsState(Nothing, EventArgs.Empty)
        UpdatelbRefinedDataButtonsState(Nothing, EventArgs.Empty)
    End Sub

    Private Async Sub lbInitData_DragDrop(sender As Object, e As DragEventArgs) Handles lbInitData.DragDrop
        Dim beforeCount As Integer = lbInitData.Items.Count
        btnCalibrate.Enabled = False
        pbMain.Style = ProgressBarStyle.Continuous
        pbMain.Minimum = 0
        pbMain.Maximum = 100
        pbMain.Value = 0

        Try
            Dim htmlFormat As String = If(e.Data.GetDataPresent("HTML Format"),
            e.Data.GetData("HTML Format").ToString(),
            String.Empty)

            Dim raw As String = If(
            Not String.IsNullOrEmpty(htmlFormat) AndAlso
            htmlFormat.Contains("urn:schemas-microsoft-com:office:word"),
            regexStripTags.Replace(htmlFormat, ""),
            e.Data.GetData("Text").ToString())

            pbMain.Value = 10

            If String.IsNullOrWhiteSpace(raw) Then Return

            If raw.IndexOf("<html", StringComparison.OrdinalIgnoreCase) >= 0 Then
                raw = Await Task.Run(Function()
                                         Return regexStripTags.Replace(raw, " ")
                                     End Function)
                pbMain.Value = 25

                If String.IsNullOrWhiteSpace(raw) Then Return
            End If

            Dim parsed As Double() = Await Task.Run(Function()
                                                        Dim ci = Globalization.CultureInfo.InvariantCulture
                                                        Return regexNumbers.Matches(raw) _
                .Cast(Of Match)() _
                .AsParallel() _
                .AsOrdered() _
                .WithDegreeOfParallelism(Environment.ProcessorCount) _
                .Select(Function(m)
                            Dim tok = m.Value.Replace(",", "").Trim()
                            Dim d As Double
                            If Double.TryParse(tok, Globalization.NumberStyles.Any, ci, d) Then
                                Return d
                            Else
                                Return Double.NaN
                            End If
                        End Function) _
                .Where(Function(d) Not Double.IsNaN(d)) _
                .ToArray()
                                                    End Function)
            pbMain.Value = 60

            If parsed.Length = 0 Then Return

            Dim baseProgress As Integer = 60
            Dim progressReporter As IProgress(Of Integer) = New Progress(Of Integer)(
            Sub(pct)
                Dim adjustedPct = Math.Max(baseProgress, Math.Min(100, pct))
                pbMain.Value = adjustedPct
                pbMain.Refresh()
            End Sub)

            Await AddItemsInBatches(lbInitData, parsed, progressReporter, baseProgress)

            pbMain.Value = 100
            lblInitCnt.Text = "Count : " & lbInitData.Items.Count
            Await Task.Delay(200)
        Finally
            UpdatelbInitDataButtonsState(Nothing, EventArgs.Empty)

            pbMain.Value = 0
            btnCalibrate.Enabled = True
            UpdateStatusLabel(beforeCount)
        End Try
    End Sub

    Private Async Function AddItemsInBatches(box As System.Windows.Forms.ListBox, items As Double(), progress As IProgress(Of Integer), baseProgress As Integer) As Task
        Const BatchSize As Integer = 1000
        Dim total As Integer = items.Length
        Dim done As Integer = 0

        box.BeginUpdate()

        While done < total
            Dim cnt As Integer = Math.Min(BatchSize, total - done)
            Dim chunk As Object() = items _
            .Skip(done) _
            .Take(cnt) _
            .Cast(Of Object)() _
            .ToArray()

            box.Items.AddRange(chunk)
            done += cnt

            Dim pct As Integer = baseProgress + CInt(done * (100L - baseProgress) / total)
            progress.Report(pct)

            Await Task.Delay(1)
        End While

        box.EndUpdate()
        box.TopIndex = box.Items.Count - 1
    End Function

    Private Sub lbInitData_DragEnter(sender As Object, e As DragEventArgs) Handles lbInitData.DragEnter
        e.Effect = If(
             e.Data.GetDataPresent(DataFormats.Text) OrElse
             e.Data.GetDataPresent("HTML Format"),
             DragDropEffects.Copy, DragDropEffects.None)
    End Sub

    Private Sub UpdateStatusLabel(beforeCount As Integer)
        Dim added As Integer = lbInitData.Items.Count - beforeCount

        If added = 0 Then
            slblDesc.Text = "No items have been added to Initial Dataset."
        ElseIf added = 1 Then
            slblDesc.Text = "1 item has been added to Initial Dataset."
        Else
            slblDesc.Text = $"{added} items have been added to Initial Dataset."
        End If

        slblDesc.Visible = True
    End Sub

    Private Async Sub btnInitPaste_Click(sender As Object, e As EventArgs) Handles btnInitPaste.Click
        pbMain.Style = ProgressBarStyle.Continuous
        pbMain.Minimum = 0
        pbMain.Maximum = 100
        pbMain.Value = 0
        btnCalibrate.Enabled = False

        Dim beforeCount As Integer = lbInitData.Items.Count
        Dim addedCount As Integer = 0

        Try
            Dim text As String = Clipboard.GetText()
            pbMain.Value = 10

            Dim matches = regexNumbers.Matches(text) _
            .Cast(Of Match)() _
            .Where(Function(m) Not String.IsNullOrEmpty(m.Value)) _
            .ToArray()
            pbMain.Value = 30

            Dim values As Double() = Await Task.Run(Function()
                                                        Return matches _
                .AsParallel() _
                .WithDegreeOfParallelism(Environment.ProcessorCount) _
                .Select(Function(m) Double.Parse(
                    m.Value,
                    Globalization.NumberStyles.Any,
                    Globalization.CultureInfo.InvariantCulture
                )) _
                .ToArray()
                                                    End Function)

            pbMain.Value = 70

            If values.Length = 0 Then
                Return
            End If

            lbInitData.BeginUpdate()
            lbInitData.Items.AddRange(values.Cast(Of Object)().ToArray())
            lbInitData.EndUpdate()
            lbInitData.TopIndex = lbInitData.Items.Count - 1

            pbMain.Value = 100
            lblInitCnt.Text = $"Count : {lbInitData.Items.Count}"
            Await Task.Delay(200)
        Finally
            UpdatelbInitDataButtonsState(Nothing, EventArgs.Empty)
            UpdatelbRefinedDataButtonsState(Nothing, EventArgs.Empty)

            UpdateStatusLabel(beforeCount)

            pbMain.Value = 0
            btnCalibrate.Enabled = True
        End Try
    End Sub


    Private Async Sub btnInitSelectAll_Click(sender As Object, e As EventArgs) Handles btnInitSelectAll.Click
        Await SelectAllWithProgress(lbInitData, pbMain)
    End Sub

    Private Async Sub btnRefSelectAll_Click(sender As Object, e As EventArgs) Handles btnRefSelectAll.Click
        Await SelectAllWithProgress(lbRefinedData, pbMain)
    End Sub


    Private Async Function SelectAllWithProgress(lb As ListBox, progressBar As ProgressBar) As Task
        Dim count As Integer = lb.Items.Count
        If count = 0 Then Return

        progressBar.Minimum = 0
        progressBar.Maximum = count
        progressBar.Value = 0

        lb.SuspendLayout()
        lb.BeginUpdate()

        Dim updateInterval As Integer = Math.Max(1, count \ 100)

        For i As Integer = 0 To count - 1
            lb.SetSelected(i, True)

            If ((i + 1) Mod updateInterval = 0) OrElse i = count - 1 Then
                progressBar.Value = i + 1
                System.Windows.Forms.Application.DoEvents()
            End If
        Next

        lb.EndUpdate()
        lb.ResumeLayout()
        lb.Focus()

        progressBar.Value = count

        Await Task.Delay(200)
        progressBar.Value = 0
    End Function

    Private Sub lbInitData_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lbInitData.SelectedIndexChanged

        Dim count As Integer = lbInitData.SelectedItems.Count

        If count = 0 Then
            Return
        End If

        slblDesc.Visible = True
        slblDesc.Text = String.Format("{0} {1} been selected in Initial Dataset.", count, If(count = 1, "item has", "items have"))

        UpdatelbInitDataButtonsState(Nothing, EventArgs.Empty)
    End Sub

    Private Sub lbRefinedData_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lbRefinedData.SelectedIndexChanged
        Dim count As Integer = lbRefinedData.SelectedItems.Count

        If count = 0 Then
            Return
        End If

        slblDesc.Visible = True
        slblDesc.Text = String.Format("{0} {1} been selected in Refined Dataset.", count, If(count = 1, "item has", "items have"))

        UpdatelbRefinedDataButtonsState(Nothing, EventArgs.Empty)
    End Sub

    Private Sub lbInitData_KeyDown(sender As Object, e As KeyEventArgs) Handles lbInitData.KeyDown
        If e.KeyData = Keys.Delete Then
            btnInitDelete.PerformClick()
            UpdatelbInitDataButtonsState(Nothing, EventArgs.Empty)
        End If

        If (e.Modifiers And Keys.Control) = Keys.Control AndAlso e.KeyCode = Keys.Delete Then
            btnInitClear.PerformClick()
            UpdatelbInitDataButtonsState(Nothing, EventArgs.Empty)
        End If

        If (e.Modifiers And Keys.Control) = Keys.Control AndAlso e.KeyCode = Keys.C Then
            btnInitCopy.PerformClick()
        End If

        If (e.Modifiers And Keys.Control) = Keys.Control AndAlso e.KeyCode = Keys.V Then
            btnInitPaste.PerformClick()
            UpdatelbInitDataButtonsState(Nothing, EventArgs.Empty)
        End If

        If (e.Modifiers And Keys.Control) = Keys.Control AndAlso e.KeyCode = Keys.A Then
            btnInitSelectAll.PerformClick()
        End If

        If e.KeyData = Keys.F2 AndAlso lbInitData.SelectedItems.Count > 0 Then
            FrmModify.ShowDialog(Me)
        End If

        If e.KeyData = Keys.Escape Then
            btnInitSelectClr.PerformClick()
        End If

        lblInitCnt.Text = "Count : " & lbInitData.Items.Count
    End Sub

    Private Sub lbRefinedData_KeyDown(sender As Object, e As KeyEventArgs) Handles lbRefinedData.KeyDown
        If (e.Modifiers And Keys.Control) = Keys.Control AndAlso e.KeyCode = Keys.C Then
            e.Handled = True
            btnRefCopy.PerformClick()
        End If

        If (e.Modifiers And Keys.Control) = Keys.Control AndAlso e.KeyCode = Keys.A Then
            e.Handled = True
            btnRefSelectAll.PerformClick()
            UpdatelbRefinedDataButtonsState(Nothing, EventArgs.Empty)
        End If

        If (e.Modifiers And Keys.Control) = Keys.Control AndAlso e.KeyCode = Keys.Delete Then
            btnRefClear.PerformClick()
            UpdatelbRefinedDataButtonsState(Nothing, EventArgs.Empty)
        End If

        If e.KeyData = Keys.Escape Then
            btnRefSelectClr.PerformClick()
            UpdatelbRefinedDataButtonsState(Nothing, EventArgs.Empty)
        End If

        lblRefCnt.Text = "Count : " & lbRefinedData.Items.Count
    End Sub

    Private Async Function ClearSelectionWithProgress(lb As ListBox, progressBar As ProgressBar, lblCount As Label) As Task
        Dim count As Integer = lb.SelectedIndices.Count
        If count = 0 Then Return

        progressBar.Minimum = 0
        progressBar.Maximum = count
        progressBar.Value = 0

        lb.SuspendLayout()
        lb.BeginUpdate()

        ' 선택 Index 복사 (변경 시 오류 방지)
        Dim selectedIndices = lb.SelectedIndices.Cast(Of Integer).ToArray()
        Dim updateInterval As Integer = Math.Max(1, count \ 100)

        For i As Integer = 0 To selectedIndices.Length - 1
            lb.SetSelected(selectedIndices(i), False)

            If ((i + 1) Mod updateInterval = 0) OrElse i = count - 1 Then
                progressBar.Value = i + 1
                System.Windows.Forms.Application.DoEvents()
            End If
        Next

        lb.EndUpdate()
        lb.ResumeLayout()
        lb.Select()

        lblCount.Text = "Count : " & lb.Items.Count

        Await Task.Delay(200)
        progressBar.Value = 0
    End Function

    Private Async Sub btnInitSelectClr_Click(sender As Object, e As EventArgs) Handles btnInitSelectClr.Click
        Dim deselectedCount As Integer = lbInitData.SelectedIndices.Count

        Await ClearSelectionWithProgress(lbInitData, pbMain, lblInitCnt)

        slblDesc.Text = $"Deselected {deselectedCount} selected item{If(Not deselectedCount = 1, "s", "")} from Initial Dataset."
        slblDesc.Visible = True
    End Sub

    Private Async Sub btnRefSelectClr_Click(sender As Object, e As EventArgs) Handles btnRefSelectClr.Click
        Dim deselectedCount As Integer = lbRefinedData.SelectedIndices.Count

        Await ClearSelectionWithProgress(lbRefinedData, pbMain, lblRefCnt)

        slblDesc.Text = $"Deselected {deselectedCount} selected item{If(Not deselectedCount = 1, "s", "")} from Refined Dataset."
        slblDesc.Visible = True
    End Sub

    Private Sub txtInitAdd_TextChanged(sender As Object, e As EventArgs) Handles txtInitAdd.TextChanged
        btnInitAdd.Enabled = CBool(txtInitAdd.TextLength) AndAlso CBool(IsNumeric(txtInitAdd.Text))
    End Sub

    Private Sub FrmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        cbxBorderCount.SelectedIndex = 0
        cbxKernelRadius.SelectedItem = "5"
        If cbxBoundaryMethod.SelectedIndex < 0 Then cbxBoundaryMethod.SelectedIndex = 0

        If String.IsNullOrWhiteSpace(txtDatasetTitle.Text) Then
            txtDatasetTitle.Text = ExcelTitlePlaceholder
            txtDatasetTitle.ForeColor = Color.Gray
        End If

        slblDesc.Size = New Size(731 * dpiX / 96, 19 * dpiY / 96)

        AddHandler lbInitData.SelectedIndexChanged, AddressOf UpdatelbInitDataButtonsState
        AddHandler lbRefinedData.SelectedIndexChanged, AddressOf UpdatelbRefinedDataButtonsState

        UpdatelbInitDataButtonsState(Nothing, EventArgs.Empty)
        UpdatelbRefinedDataButtonsState(Nothing, EventArgs.Empty)
    End Sub

    Private Sub UpdatelbInitDataButtonsState(s As Object, e As EventArgs)
        Dim hasItems As Boolean = (lbInitData.Items.Count > 0)
        Dim hasSelection As Boolean = (lbInitData.SelectedItems.Count > 0)
        Dim titleValid As Boolean = (txtDatasetTitle.TextLength > 0 AndAlso txtDatasetTitle.Text <> ExcelTitlePlaceholder)
        Dim canSync As Boolean = (lbInitData.Items.Count = lbRefinedData.Items.Count) AndAlso hasSelection

        btnInitCopy.Enabled = hasItems
        btnInitEdit.Enabled = hasSelection
        btnInitDelete.Enabled = hasSelection
        btnInitSelectAll.Enabled = hasItems
        btnInitSelectClr.Enabled = hasSelection
        btnInitClear.Enabled = hasItems
        btnCalibrate.Enabled = hasItems
        btnExport.Enabled = hasItems AndAlso titleValid
        btnInitSelectSync.Enabled = canSync
    End Sub

    Private Sub UpdatelbRefinedDataButtonsState(s As Object, e As EventArgs)
        Dim hasItems As Boolean = (lbRefinedData.Items.Count > 0)
        Dim hasSelection As Boolean = (lbRefinedData.SelectedItems.Count > 0)
        Dim canSync As Boolean = (lbRefinedData.Items.Count = lbInitData.Items.Count) AndAlso hasSelection
        btnRefCopy.Enabled = hasItems
        btnRefClear.Enabled = hasItems
        btnRefSelectAll.Enabled = hasItems
        btnRefSelectClr.Enabled = hasSelection
        btnRefSelectSync.Enabled = canSync
    End Sub


    Private Sub rbtnAllMedian_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnAllMedian.CheckedChanged
        If rbtnAllMedian.Checked Then
            lblBorderCount.Enabled = False
            cbxBorderCount.Enabled = False
            cbxBoundaryMethod.Enabled = True
        End If
    End Sub

    Private Sub rbtnMidMedian_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnMidMedian.CheckedChanged
        If rbtnMidMedian.Checked Then
            lblBorderCount.Enabled = True
            cbxBorderCount.Enabled = True
            cbxBoundaryMethod.Enabled = False
        End If
    End Sub

    Private Sub btnEdit_Click(sender As Object, e As EventArgs) Handles btnInitEdit.Click
        FrmModify.ShowDialog(Me)
    End Sub

    Dim EXCEL_MAX_ROW = 1048576

    Private Async Function ExportCsvAsync() As Task
        ' ProgressBar 초기화
        pbMain.Style = ProgressBarStyle.Continuous
        pbMain.Minimum = 0
        pbMain.Maximum = 100
        pbMain.Value = 0

        ' Parameters 읽기
        Dim kernelRadius As Integer
        If Not Integer.TryParse(cbxKernelRadius.Text, kernelRadius) Then
            MessageBox.Show("Please select a kernel radius.", "Export CSV",
                    MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim kernelWidth As Integer = 2 * kernelRadius + 1

        Dim borderCount As Integer
        If Not Integer.TryParse(cbxBorderCount.Text, borderCount) Then
            MessageBox.Show("Please select a border count.", "Export CSV",
                    MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        ' 원본 데이터 파싱
        Dim initialData = lbInitData.Items.Cast(Of Object)() _
                    .Select(Function(x)
                                Dim d As Double
                                Return If(Double.TryParse(x?.ToString(),
                                                          NumberStyles.Any,
                                                          CultureInfo.InvariantCulture,
                                                          d),
                                          d, Double.NaN)
                            End Function) _
                    .Where(Function(d) Not Double.IsNaN(d)) _
                    .ToArray()
        Dim n = initialData.Length
        If n = 0 Then
            MessageBox.Show("No data to export.", "Export CSV",
                    MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        ' 계산을 수행하기에 앞서, 두 가지 모드 (All-Median, Middle-Median) 를 모두 확인해야 합니다.
        If Not ValidateSmoothingParametersCanonical(n, kernelRadius, borderCount, True) Then Return
        If Not ValidateSmoothingParametersCanonical(n, kernelRadius, borderCount, False) Then Return

        Dim boundary = GetSelectedBoundaryMode()

        ' All Median / Middle Median 계산
        Dim middleMedian(n - 1) As Double
        Dim allMedian(n - 1) As Double

        initList = initialData.ToList()
        Dim middleProg = New Progress(Of Integer)(Sub(v) pbMain.Value = Math.Max(pbMain.Minimum, Math.Min(v, pbMain.Maximum)))
        Await Task.Run(Sub()
                           ComputeMedians(True, 2 * kernelRadius + 1, borderCount, middleProg) ' no boundary
                           refinedList.CopyTo(0, middleMedian, 0, n)
                       End Sub)

        initList = initialData.ToList()
        Dim allProg = New Progress(Of Integer)(Sub(v) pbMain.Value = Math.Max(pbMain.Minimum, Math.Min(v, pbMain.Maximum)))
        Await Task.Run(Sub()
                           ComputeMedians(False, kernelWidth, borderCount, allProg, boundary)
                           refinedList.CopyTo(0, allMedian, 0, n)
                       End Sub)

        ' 저장 경로 지정
        Dim basePath As String
        Using dlg As New SaveFileDialog()
            dlg.FileName = $"{txtDatasetTitle.Text}.csv"
            dlg.Filter = "CSV files (*.csv)|*.csv"
            dlg.DefaultExt = "csv"
            dlg.AddExtension = True
            If dlg.ShowDialog(Me) <> DialogResult.OK Then
                pbMain.Value = 0
                Return
            End If
            basePath = dlg.FileName
        End Using

        ' 분할 저장 설정
        Const EXCEL_MAX_ROW As Integer = 1048576
        Const HEADER_LINES As Integer = 12
        Dim maxDataRows = EXCEL_MAX_ROW - HEADER_LINES - 1
        Dim partCount = CInt(Math.Ceiling(n / CDbl(maxDataRows)))

        Dim dir = Path.GetDirectoryName(basePath)
        Dim nameOnly = Path.GetFileNameWithoutExtension(basePath)
        Dim ext = Path.GetExtension(basePath)

        ' 파트별 파일 쓰기
        Dim createdFiles As New List(Of String)()   ' 추가: 생성된 파일 경로 저장용

        For part = 0 To partCount - 1
            Dim startIdx = part * maxDataRows
            Dim count = Math.Min(maxDataRows, n - startIdx)
            Dim filePath As String

            If partCount > 1 Then
                filePath = Path.Combine(dir, $"{nameOnly}_Part{part + 1}{ext}")
            Else
                filePath = basePath
            End If


            Dim wrote As Boolean = False
            Do
                Try
                    Using fs As New FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None),
              sw As New StreamWriter(fs, Encoding.UTF8)

                        ' 제목 & Parameters
                        sw.WriteLine(txtDatasetTitle.Text)
                        sw.WriteLine($"Part {part + 1} of {partCount}")
                        sw.WriteLine()
                        sw.WriteLine("Smoothing Parameters")
                        sw.WriteLine($"Kernel Radius : {kernelRadius}")
                        sw.WriteLine($"Kernel Width : {kernelWidth}")
                        sw.WriteLine($"Border Count : {borderCount}")
                        sw.WriteLine($"Boundary Method : {boundary.ToString()}")
                        sw.WriteLine()
                        sw.WriteLine($"Generated : {DateTime.Now.ToString("G", CultureInfo.CurrentCulture)}")
                        sw.WriteLine()
                        sw.WriteLine("Initial Data,MiddleMedian,AllMedian")

                        ' 데이터 쓰기
                        For i = startIdx To startIdx + count - 1
                            Dim line = String.Join(",",
                        initialData(i).ToString("G17", CultureInfo.InvariantCulture),
                        middleMedian(i).ToString("G17", CultureInfo.InvariantCulture),
                        allMedian(i).ToString("G17", CultureInfo.InvariantCulture))
                            sw.WriteLine(line)

                            ' 진행률 업데이트
                            Dim pct = CInt((i + 1) / CSng(n) * 100)
                            pbMain.Value = Math.Max(pbMain.Minimum, Math.Min(pct, pbMain.Maximum))

                            If pct >= 100 Then
                                ' 100% 채워졌을 때 잠시 표시 유지 후 0 으로 리셋
                                Await Task.Delay(200) ' 필요시 ms 조정 가능
                                pbMain.Value = 0
                            End If
                        Next
                    End Using

                    wrote = True
                Catch ex As IOException
                    Dim res = MessageBox.Show(Me,
                        $"The file '{filePath}' is being used by another process.{Environment.NewLine}{Environment.NewLine}" &
                        "Close the file and click Retry, or click Cancel to abort export.",
                        "Export CSV - File In Use",
                        MessageBoxButtons.RetryCancel,
                        MessageBoxIcon.Warning)

                    If res = DialogResult.Retry Then
                        Continue Do
                    Else
                        pbMain.Value = 0
                        Return
                    End If
                End Try
            Loop Until wrote

            createdFiles.Add(filePath)   ' 추가
        Next

        For Each file In createdFiles
            Try
                Process.Start(New ProcessStartInfo(file) With {
                .UseShellExecute = True
            })
            Catch ex As System.ComponentModel.Win32Exception
                Process.Start(New ProcessStartInfo("rundll32.exe",
                       $"shell32.dll,OpenAs_RunDLL ""{file}""") With {
                .UseShellExecute = True})
                pbMain.Value = 0
                Return
            Catch ex As Exception
                MessageBox.Show($"We're sorry, but the file could not be opened : {file}{vbCrLf}{ex.Message}",
                "Error Opening File", MessageBoxButtons.OK, MessageBoxIcon.Error)
                pbMain.Value = 0
                Return
            End Try
        Next
    End Function

    Private Function ValidateSmoothingParameters(dataCount As Integer, kernelWidth As Integer, borderCount As Integer, useMiddle As Boolean) As Boolean
        Dim radius As Integer = Integer.Parse(cbxKernelRadius.Text)
        Dim windowSize As Integer = kernelWidth
        Dim borderTotalWidth As Integer = borderCount * If(useMiddle, 2, 0)

        ' radius 로 인한 windowSize 초과 검사
        If windowSize > dataCount Then
            MessageBox.Show(
            $"Kernel radius is too large.{Environment.NewLine}{Environment.NewLine}" &
            $"Window size formula : (2 × radius) + 1{Environment.NewLine}" &
            $"Current : (2 × {radius}) + 1 = {windowSize}{Environment.NewLine}" &
            $"Data count : {dataCount}{Environment.NewLine}{Environment.NewLine}" &
            $"Rule : windowSize ≤ dataCount{Environment.NewLine}" &
            $"Result : {windowSize} ≤ {dataCount} → Violation",
            "Parameter Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.[Error]
        )
            Return False
        End If

        ' borderCount 범위 검사
        If borderCount > dataCount Then
            MessageBox.Show(
            $"Border count is too large.{Environment.NewLine}{Environment.NewLine}" &
            $"Rule : borderCount ≤ dataCount{Environment.NewLine}" &
            $"Result : {borderCount} ≤ {dataCount} → Violation",
            "Parameter Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.[Error]
        )
            Return False
        End If

        ' Middle 모드일 때 경계 폭 검사
        If useMiddle AndAlso borderTotalWidth >= windowSize Then
            MessageBox.Show(
            $"Border width is too large relative to the window size.{Environment.NewLine}{Environment.NewLine}" &
            $"Tip : windowSize = (2 × radius) + 1{Environment.NewLine}" &
            $"Rule : totalBorderWidth < windowSize{Environment.NewLine}" &
            $"Result : {borderTotalWidth} < {windowSize} → Violation",
            "Parameter Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.[Error]
        )
            Return False
        End If

        Return True
    End Function

    Private Async Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
        If rbtnCSV.Checked Then
            Await ExportCsvAsync()
            Return
        ElseIf rbtnXLSX.Checked Then
            pbMain.Style = ProgressBarStyle.Continuous
            pbMain.Minimum = 0
            pbMain.Maximum = 100
            pbMain.Value = 0

            ' Kernel / 경계 값 읽기
            Dim kernelRadius As Integer
            If Not Integer.TryParse(cbxKernelRadius.Text, kernelRadius) Then
                MessageBox.Show("Please select a kernel radius.", "Export Excel", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim kernelWidth As Integer = 2 * kernelRadius + 1

            Dim borderCount As Integer
            If Not Integer.TryParse(cbxBorderCount.Text, borderCount) Then
                MessageBox.Show("Please select a border count.", "Export Excel", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim initialData = lbInitData.Items.Cast(Of Object)().
            Select(Function(x)
                       Dim d As Double
                       If Double.TryParse(x.ToString(), d) Then Return d Else Return Double.NaN
                   End Function).
            Where(Function(d) Not Double.IsNaN(d)).
            ToArray()

            Dim n = initialData.Length
            If n = 0 Then
                MessageBox.Show("No data to export.", "Export Excel", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            If Not ValidateSmoothingParametersCanonical(n, kernelRadius, borderCount, True) Then Return

            Dim boundary = GetSelectedBoundaryMode()

            ' Middle Median 계산
            Dim middleMedian(n - 1) As Double
            initList = initialData.ToList()
            Dim middleProgress = New Progress(Of Integer)(Sub(v) pbMain.Value = Math.Max(pbMain.Minimum, Math.Min(v, pbMain.Maximum)))
            Await Task.Run(Sub()
                               ComputeMedians(True, kernelWidth, borderCount, middleProgress) ' no boundary
                               refinedList.CopyTo(0, middleMedian, 0, n)
                           End Sub)

            If Not ValidateSmoothingParametersCanonical(n, kernelRadius, borderCount, False) Then Return

            ' All Median 계산
            Dim allMedian(n - 1) As Double
            initList = initialData.ToList()
            Dim allProgress = New Progress(Of Integer)(Sub(v) pbMain.Value = Math.Max(pbMain.Minimum, Math.Min(v, pbMain.Maximum)))
            Await Task.Run(Sub()
                               ComputeMedians(False, kernelWidth, borderCount, allProgress, boundary)
                               refinedList.CopyTo(0, allMedian, 0, n)
                           End Sub)

            ' COM 참조 관리 추가
            Dim coms As New Stack(Of Object)()

            Dim excel As Excel.Application = Nothing
            Dim workbooks As Excel.Workbooks = Nothing
            Dim wb As Excel.Workbook = Nothing
            Dim sheets As Excel.Sheets = Nothing
            Dim ws As Excel.Worksheet = Nothing

            Dim chartObjects As Excel.ChartObjects = Nothing
            Dim chartObj As Excel.ChartObject = Nothing
            Dim chart As Excel.Chart = Nothing
            Dim seriesCollection As Excel.SeriesCollection = Nothing

            Dim EXCEL_MAX_ROW = 1048576
            Dim DATA_START_ROW = 4

            Try
                excel = New Excel.Application()
                coms.Push(excel)

                workbooks = excel.Workbooks
                coms.Push(workbooks)

                wb = workbooks.Add()
                coms.Push(wb)

                Try
                    Dim smoothieFlavors As String() = {
                        "Creamy Data Fusion",
                        "Velvet Median Harmony",
                        "Tropical Algorithm Twist",
                        "Green Symphony Blend",
                        "Nutty Regression Notes",
                        "Citrus Curve Balance",
                        "Minty Mean Melody",
                        "Silky Quartile Swirl",
                        "Berry Smooth Variance",
                        "Avocado Aria"
                    }

                    Dim rnd As New Random()
                    Dim randomFlavor As String = smoothieFlavors(rnd.Next(smoothieFlavors.Length))

                    ' Excel 파일의 Built-in 속성에 기록.
                    ' wb.BuiltinDocumentProperties("Title") = txtDatasetTitle.Text;
                    ' wb.BuiltinDocumentProperties("Category") = "SonataSmooth Export";
                    ' wb.BuiltinDocumentProperties("Last Author") = Environment.UserName;
                    ' wb.BuiltinDocumentProperties("Keywords") = "AvocadoSmoothie, MedianSmoothing, Export";
                    ' wb.BuiltinDocumentProperties("Subject") = subject;
                    ' wb.BuiltinDocumentProperties("Comments").Value = "Exported from AvocadoSmoothie application";

                    wb.BuiltinDocumentProperties("Title") = txtDatasetTitle.Text
                    wb.BuiltinDocumentProperties("Category") = "Smoothie Blend Results"
                    wb.BuiltinDocumentProperties("Subject") = "AvocadoSmoothie Recipe : Middle Median & All Median, Equal-weight blend"
                    wb.BuiltinDocumentProperties("Author") = "Barista AvocadoSmoothie"
                    wb.BuiltinDocumentProperties("Last Author") = Environment.UserName
                    wb.BuiltinDocumentProperties("Keywords") = "AvocadoSmoothie, Recipe, Blend, MiddleMedian, AllMedian, ExcelExport"

                    Dim comments As String =
                        "A perfectly balanced blend" & Environment.NewLine &
                        randomFlavor & Environment.NewLine &
                        "Freshly pureed by AvocadoSmoothie"

                    If n >= 1048576 Then
                        comments &= Environment.NewLine & Environment.NewLine &
                            "Legendary Recipe Unlocked : The Full Sheet Feast" & Environment.NewLine &
                            "Every single row brimming with flavor."
                    ElseIf n >= 500000 Then
                        comments &= Environment.NewLine & Environment.NewLine &
                            "Secret Recipe Unlocked : The Half-Million Majesty" & Environment.NewLine &
                            "500,000+ sips of supreme smoothness."
                    ElseIf n >= 250000 Then
                        comments &= Environment.NewLine & Environment.NewLine &
                            "Secret Recipe Unlocked : The Quarter-Million Smooth" & Environment.NewLine &
                            "250,000+ sips blended to silk."
                    ElseIf n >= 100000 Then
                        comments &= Environment.NewLine & Environment.NewLine &
                            "Secret Recipe Unlocked : The Hectokilo Harmony" & Environment.NewLine &
                            "100,000+ sips blended into a masterpiece."
                    ElseIf n >= 50000 Then
                        comments &= Environment.NewLine & Environment.NewLine &
                            "Secret Recipe Unlocked : The Grandmaster Blend" & Environment.NewLine &
                            "50,000+ sips of pure harmony."
                    ElseIf n >= 40000 Then
                        comments &= Environment.NewLine & Environment.NewLine &
                            "Secret Recipe Unlocked : The Forty-K Fusion" & Environment.NewLine &
                            "A lush mix of 40,000+ data sips."
                    ElseIf n >= 30000 Then
                        comments &= Environment.NewLine & Environment.NewLine &
                            "Secret Recipe Unlocked : The Triple Ten Symphony" & Environment.NewLine &
                            "30,000+ sips in perfect rhythm."
                    ElseIf n >= 25000 Then
                        comments &= Environment.NewLine & Environment.NewLine &
                            "Secret Recipe Unlocked : The Twenty-Five-K Smooth" & Environment.NewLine &
                            "25,000+ sips blended to silk."
                    ElseIf n >= 20000 Then
                        comments &= Environment.NewLine & Environment.NewLine &
                            "Secret Recipe Unlocked : The Double Ten Delight" & Environment.NewLine &
                            "20,000+ sips of creamy balance."
                    ElseIf n >= 10000 Then
                        comments &= Environment.NewLine & Environment.NewLine &
                            "Secret Recipe Unlocked : The Ten-K Treat" & Environment.NewLine &
                            "10,000+ sips of avocado perfection."
                    ElseIf n >= 5000 Then
                        comments &= Environment.NewLine & Environment.NewLine &
                            "Secret Recipe Unlocked : The Five-K Fusion" & Environment.NewLine &
                            "5,000+ sips in smooth unison."
                    ElseIf n >= 1000 Then
                        comments &= Environment.NewLine & Environment.NewLine &
                            "Secret Recipe Unlocked : The Kilo-Sip Classic" & Environment.NewLine &
                            "1,000+ sips of silky satisfaction."
                    ElseIf n >= 500 Then
                        comments &= Environment.NewLine & Environment.NewLine &
                            "Secret Recipe Unlocked : The Half-K Harmony" & Environment.NewLine &
                            "500+ sips of mellow magic."
                    ElseIf n >= 100 Then
                        comments &= Environment.NewLine &
                            "Secret Recipe Unlocked : The Century Blend" & Environment.NewLine &
                            "A harmony of 100+ data sips."
                    End If

                    wb.BuiltinDocumentProperties("Comments") = comments


                Catch ex As Exception
                    ' Excel Interop 에서 발생할 수 있는 주요 예외를 구분하여 처리
                    If TypeOf ex Is System.Runtime.InteropServices.COMException Then
                        Dim comEx As System.Runtime.InteropServices.COMException = DirectCast(ex, System.Runtime.InteropServices.COMException)
                        ' COM 예외 : Excel 이 설치되어 있지 않거나, 권한 문제, 속성 이름 오탈자 등
                        MessageBox.Show(
                            "Failed to set Excel document properties (COM error)." & vbCrLf & vbCrLf &
                            "Excel may not be installed, the property name may be incorrect, or there may be a permissions issue." & vbCrLf & vbCrLf &
                            "Details : " & comEx.Message,
                            "Excel Property Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                            )
                        ' 필요 시 로그 : Debug.WriteLine(comEx)

                    ElseIf TypeOf ex Is ArgumentException Then
                        Dim argEx As ArgumentException = DirectCast(ex, ArgumentException)
                        ' 잘못된 속성 이름 등
                        MessageBox.Show(
                            "Failed to set Excel document properties (Argument error)." & vbCrLf & vbCrLf &
                            "The built-in property name is incorrect, or an unsupported value has been assigned." & vbCrLf & vbCrLf &
                            "Details : " & argEx.Message,
                            "Excel Property Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                            )

                    ElseIf TypeOf ex Is InvalidCastException Then
                        Dim castEx As InvalidCastException = DirectCast(ex, InvalidCastException)
                        ' 잘못된 타입으로 값 할당 시
                        MessageBox.Show(
                            "Failed to set Excel document properties (Type error)." & vbCrLf & vbCrLf &
                            "The type of value assigned to the property is invalid." & vbCrLf & vbCrLf &
                            "Details : " & castEx.Message,
                            "Excel Property Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                            )

                    ElseIf TypeOf ex Is System.UnauthorizedAccessException Then
                        Dim unauthEx As System.UnauthorizedAccessException = DirectCast(ex, System.UnauthorizedAccessException)
                        ' 파일 또는 속성에 대한 권한 부족
                        MessageBox.Show(
                            "Failed to set Excel document properties (Access denied)." & vbCrLf & vbCrLf &
                            "You do not have sufficient permissions for the Excel file or its properties." & vbCrLf & vbCrLf &
                            "Details : " & unauthEx.Message,
                            "Excel Property Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        )

                    Else
                        ' 기타 예외 처리
                        MessageBox.Show(
                            "An unexpected error occurred while setting Excel document properties." & vbCrLf & vbCrLf &
                            "Details :  " & ex.Message,
                            "Excel Property Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                            )
                    End If

                    ' 필요 시 상세 로그 표시
                    ' System.Diagnostics.Debug.WriteLine(ex.ToString())
                End Try

                sheets = wb.Worksheets
                coms.Push(sheets)

                ws = CType(sheets(1), Excel.Worksheet)
                ws.Name = txtDatasetTitle.Text
                coms.Push(ws)

                ws.Cells(1, 1) = txtDatasetTitle.Text
                ws.Cells(3, 1) = "Smoothing Parameters"
                ws.Cells(4, 1) = $"Kernel Radius : {kernelRadius}"
                ws.Cells(5, 1) = $"Kernel Width : {kernelWidth}"
                ws.Cells(6, 1) = $"Border Count : {borderCount}"
                ws.Cells(7, 1) = $"Boundary Method : {boundary.ToString()}"

                ' 데이터를 분산 저장하는 함수
                Dim WriteDistributed =
                Function(data As Double(), startCol As Integer, title As String) As List(Of Tuple(Of Integer, Integer, Integer))
                    Dim ranges As New List(Of Tuple(Of Integer, Integer, Integer))
                    Dim idx = 0
                    Dim col = startCol
                    Dim firstCol = col
                    While idx < data.Length
                        Dim count = Math.Min(EXCEL_MAX_ROW - DATA_START_ROW + 1, data.Length - idx)
                        Dim arr2D(count - 1, 0) As Object
                        For r = 0 To count - 1
                            arr2D(r, 0) = data(idx)
                            idx += 1
                        Next
                        Dim startRow = DATA_START_ROW
                        Dim endRow = startRow + count - 1
                        If col = firstCol Then ws.Cells(3, col) = title
                        ws.Range(ws.Cells(startRow, col), ws.Cells(endRow, col)).Value2 = arr2D
                        ranges.Add(Tuple.Create(col, startRow, endRow))
                        col += 1
                    End While
                    Return ranges
                End Function

                ' 각 Median 결과를 엑셀에 분산 저장
                Dim initialRanges = WriteDistributed(initialData, 3, "Initial Data")
                pbMain.Value = Math.Max(pbMain.Minimum, Math.Min(30, pbMain.Maximum))
                Dim middleRanges = WriteDistributed(middleMedian, initialRanges.Last.Item1 + 2, "MiddleMedian")
                pbMain.Value = Math.Max(pbMain.Minimum, Math.Min(60, pbMain.Maximum))
                Dim allRanges = WriteDistributed(allMedian, middleRanges.Last.Item1 + 2, "AllMedian")
                pbMain.Value = Math.Max(pbMain.Minimum, Math.Min(80, pbMain.Maximum))

                ' 차트 생성 (원본 로직 유지, 생성한 COM 은 추적)
                Dim lastCol = Math.Max(Math.Max(initialRanges.Last.Item1, middleRanges.Last.Item1), allRanges.Last.Item1)
                Dim chartBaseCol = lastCol + 2
                Dim chartBaseRow = DATA_START_ROW

                chartObjects = CType(ws.ChartObjects(), Excel.ChartObjects)
                coms.Push(chartObjects)

                Dim chartLeft = ws.Cells(chartBaseRow, chartBaseCol).Left
                Dim chartTop = ws.Cells(chartBaseRow, chartBaseCol).Top
                Dim chartWidth = 900
                Dim chartHeight = 600

                chartObj = chartObjects.Add(chartLeft, chartTop, chartWidth, chartHeight)
                coms.Push(chartObj)

                chart = chartObj.Chart
                coms.Push(chart)

                chart.ChartType = Microsoft.Office.Interop.Excel.XlChartType.xlLine
                chart.HasTitle = True
                chart.ChartTitle.Text = txtDatasetTitle.Text
                chart.Axes(Microsoft.Office.Interop.Excel.XlAxisType.xlValue).HasTitle = True
                chart.Axes(Microsoft.Office.Interop.Excel.XlAxisType.xlValue).AxisTitle.Text = "Value"
                chart.Axes(Microsoft.Office.Interop.Excel.XlAxisType.xlCategory).HasTitle = True
                chart.Axes(Microsoft.Office.Interop.Excel.XlAxisType.xlCategory).AxisTitle.Text = "Sequence Number"

                seriesCollection = chart.SeriesCollection()
                coms.Push(seriesCollection)

                Dim GetExcelColumnName As Func(Of Integer, String) =
                Function(columnNumber As Integer) As String
                    Dim colName As String = ""
                    While columnNumber > 0
                        Dim modulo = (columnNumber - 1) Mod 26
                        colName = Chr(65 + modulo) & colName
                        columnNumber = (columnNumber - modulo) \ 26
                    End While
                    Return colName
                End Function

                Dim AddSeries = Sub(ranges As List(Of Tuple(Of Integer, Integer, Integer)), name As String)
                                    Dim multiRange As Excel.Range = Nothing
                                    Dim totalCount As Integer = 0
                                    For Each rng In ranges
                                        Dim col = rng.Item1
                                        Dim startRow = rng.Item2
                                        Dim endRow = rng.Item3
                                        Dim colLetter = GetExcelColumnName(col)
                                        Dim singleRange = ws.Range($"{colLetter}{startRow}:{colLetter}{endRow}")
                                        If multiRange Is Nothing Then
                                            multiRange = singleRange
                                        Else
                                            multiRange = excel.Union(multiRange, singleRange)
                                        End If
                                        totalCount += (endRow - startRow + 1)
                                    Next
                                    Dim series As Excel.Series = CType(seriesCollection.Add(Source:=multiRange, Rowcol:=Microsoft.Office.Interop.Excel.XlRowCol.xlColumns), Excel.Series)
                                    series.Name = name
                                    ' 지역 RCW 정리 : Series 추가 후 Multi-Range 해제
                                    If multiRange IsNot Nothing Then FinalRelease(multiRange)
                                    FinalRelease(series)
                                End Sub

                AddSeries(initialRanges, "Initial Data")
                AddSeries(middleRanges, "Middle Median")
                AddSeries(allRanges, "All Median")

                pbMain.Value = Math.Max(pbMain.Minimum, Math.Min(100, pbMain.Maximum))
                Await Task.Delay(200)
                pbMain.Value = 0

                ' 창 유지 : 사용자에게 권한 이관
                wb.Saved = False ' 닫기 시 저장 대화상자 표시
                excel.Visible = True
                excel.DisplayAlerts = True

            Catch ex As System.Runtime.InteropServices.COMException
                Dim msg As String = "Excel interop error: " & ex.Message & vbCrLf & vbCrLf &
                    "Microsoft Excel does not appear to be installed, or there was a problem starting Excel." & vbCrLf &
                    "If Excel is not installed, you can visit the Microsoft Office website to purchase or install Office." & vbCrLf & vbCrLf &
                    "Would you like to open the Microsoft Office download page now?"
                Dim result = MessageBox.Show(msg, "Export Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2)
                If result = DialogResult.Yes Then
                    Try
                        Process.Start(New ProcessStartInfo("https://www.microsoft.com/microsoft-365/buy/compare-all-microsoft-365-products") With {.UseShellExecute = True})
                    Catch
                        Exit Sub
                    End Try
                End If
            Catch ex As AggregateException
                Dim allMessages = String.Join(Environment.NewLine, ex.InnerExceptions.Select(Function(inner) inner.Message))
                MessageBox.Show("One or more errors occurred during export : " & Environment.NewLine & allMessages, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Catch ex As System.IO.PathTooLongException
                MessageBox.Show("The file path is too long. Please choose a shorter path.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Catch ex As System.IO.DirectoryNotFoundException
                MessageBox.Show("The specified directory was not found. Please check the save location.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Catch ex As System.IO.IOException
                MessageBox.Show("An I/O error occurred while saving the file. Please check disk space and permissions.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Catch ex As System.OutOfMemoryException
                MessageBox.Show("Not enough memory to complete the export. Try closing other applications.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Catch ex As System.BadImageFormatException
                MessageBox.Show("Excel (Office) bitness (32-bit / 64-bit) or Interop DLL mismatch. Please check your Office installation.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Catch ex As Exception
                MessageBox.Show("An unexpected error occurred: " & ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Finally
                ' 생성한 COM 참조를 역순으로 해제
                ReleaseAll(coms)

                ' RCW Finalizer 보장을 위해 2 회 GC
                GC.Collect()
                GC.WaitForPendingFinalizers()
                GC.Collect()
                GC.WaitForPendingFinalizers()
            End Try
        End If
    End Sub

    ' RCW 를 안전하게 해제
    Private Shared Sub FinalRelease(ByVal com As Object)
        Try
            If com IsNot Nothing AndAlso Marshal.IsComObject(com) Then
                Marshal.FinalReleaseComObject(com)
            End If
        Catch
            ' 필요 시 로깅 부분 추가
        End Try
    End Sub

    ' 생성한 COM 개체를 역순으로 모두 해제
    Private Shared Sub ReleaseAll(ByVal stack As Stack(Of Object))
        While stack.Count > 0
            FinalRelease(stack.Pop())
        End While
    End Sub

    Private Sub btnInfo_Click(sender As Object, e As EventArgs) Handles btnInfo.Click
        AboutBox.ShowDialog()
    End Sub

    Private Sub txtDatasetTitle_Enter(sender As Object, e As EventArgs) Handles txtDatasetTitle.Enter
        If txtDatasetTitle.Text = ExcelTitlePlaceholder Then
            txtDatasetTitle.Text = ""
            txtDatasetTitle.ForeColor = Color.Black
        End If
        txtDatasetTitle.TextAlign = HorizontalAlignment.Left
    End Sub

    Private Sub txtDatasetTitle_Leave(sender As Object, e As EventArgs) Handles txtDatasetTitle.Leave
        Dim raw As String = txtDatasetTitle.Text
        Dim title As String = If(raw IsNot Nothing, raw.Trim(), String.Empty)

        If title = ExcelTitlePlaceholder OrElse String.IsNullOrEmpty(raw) Then
            txtDatasetTitle.Text = ExcelTitlePlaceholder
            txtDatasetTitle.ForeColor = Color.Gray
            txtDatasetTitle.TextAlign = HorizontalAlignment.Center
            Exit Sub
        End If

        Const MaxLength As Integer = 31
        Dim invalidChars As String = ":\/?*["
        Dim winInvalidChars As Char() = Path.GetInvalidFileNameChars()
        Dim reservedNames As String() = {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        }

        Dim titleUpper As String = title.ToUpperInvariant()
        Dim errors As New List(Of String)()

        If String.IsNullOrWhiteSpace(title) Then
            errors.Add("The name cannot be blank.")
        End If

        If title.Length > MaxLength Then
            errors.Add($"The name must not exceed {MaxLength} characters.")
        End If

        Dim hasInvalidChar As Boolean = title.IndexOfAny(invalidChars.ToCharArray()) >= 0 OrElse title.Contains("]")
        Dim hasWinInvalidChar As Boolean = title.IndexOfAny(winInvalidChars) >= 0

        If hasInvalidChar OrElse hasWinInvalidChar Then
            Dim winCharsDisplay = String.Join(" ",
                winInvalidChars.
                    Select(Function(c)
                               If c = " "c Then
                                   Return "<space>"
                               Else
                                   Return c.ToString()
                               End If
                           End Function).
                    ToArray()
            )

            errors.Add(
                "The name cannot contain any of the following characters: : \ / ? * [ ]" & vbCrLf &
                "or any Windows file name invalid characters: " & winCharsDisplay
            )
        End If

        If reservedNames.Any(Function(rn) titleUpper.Equals(rn, StringComparison.OrdinalIgnoreCase)) Then
            errors.Add("The name cannot be a reserved Windows file name (e.g., CON, PRN, AUX, NUL, COM1, LPT1, etc.).")
        End If

        If errors.Count > 0 Then
            If _isShowingTitleValidationMessage OrElse
               String.Equals(_lastInvalidTitle, title, StringComparison.Ordinal) Then

                txtDatasetTitle.Text = ExcelTitlePlaceholder
                txtDatasetTitle.ForeColor = Color.Gray
                txtDatasetTitle.TextAlign = HorizontalAlignment.Center
                Exit Sub
            End If

            _isShowingTitleValidationMessage = True
            _lastInvalidTitle = title
            Try
                txtDatasetTitle.Text = ExcelTitlePlaceholder
                txtDatasetTitle.ForeColor = Color.Gray
                txtDatasetTitle.TextAlign = HorizontalAlignment.Center
                MessageBox.Show(
                    String.Join(vbCrLf & vbCrLf, errors),
                    "Invalid Title",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                )
            Finally
                _isShowingTitleValidationMessage = False
            End Try

            Exit Sub
        End If

        _lastInvalidTitle = Nothing
        txtDatasetTitle.ForeColor = SystemColors.WindowText
        txtDatasetTitle.TextAlign = HorizontalAlignment.Left
    End Sub

    Private Sub txtDatasetTitle_TextChanged(sender As Object, e As EventArgs) Handles txtDatasetTitle.TextChanged
        UpdatelbInitDataButtonsState(Nothing, EventArgs.Empty)

        If txtDatasetTitle.Text = ExcelTitlePlaceholder Then
            txtDatasetTitle.TextAlign = HorizontalAlignment.Center
        Else
            txtDatasetTitle.TextAlign = HorizontalAlignment.Left
        End If
    End Sub

    Private Sub btnInitSelectSync_Click(sender As Object, e As EventArgs) Handles btnInitSelectSync.Click
        If lbInitData.Items.Count <> lbRefinedData.Items.Count Then Return
        If lbInitData.SelectedIndices.Count = 0 Then Return

        lbRefinedData.BeginUpdate()
        Try
            lbRefinedData.ClearSelected()
            Dim indices = New Integer(lbInitData.SelectedIndices.Count - 1) {}
            lbInitData.SelectedIndices.CopyTo(indices, 0)
            For i As Integer = 0 To indices.Length - 1
                lbRefinedData.SetSelected(indices(i), True)
            Next

            If lbInitData.TopIndex >= 0 Then
                lbRefinedData.TopIndex = lbInitData.TopIndex
            End If

            slblDesc.Text = $"Synchronized {indices.Length} selected item{If(indices.Length > 1, "s", "")} to Refined Dataset."
            slblDesc.Visible = True
        Finally
            lbRefinedData.EndUpdate()
        End Try
    End Sub

    Private Sub btnRefSelectSync_Click(sender As Object, e As EventArgs) Handles btnRefSelectSync.Click
        If lbRefinedData.Items.Count <> lbInitData.Items.Count Then Return
        If lbRefinedData.SelectedIndices.Count = 0 Then Return

        lbInitData.BeginUpdate()
        Try
            lbInitData.ClearSelected()
            Dim indices = New Integer(lbRefinedData.SelectedIndices.Count - 1) {}
            lbRefinedData.SelectedIndices.CopyTo(indices, 0)
            For i As Integer = 0 To indices.Length - 1
                lbInitData.SetSelected(indices(i), True)
            Next

            If lbRefinedData.TopIndex >= 0 Then
                lbInitData.TopIndex = lbRefinedData.TopIndex
            End If

            slblDesc.Text = $"Synchronized {indices.Length} selected item{If(indices.Length > 1, "s", "")} to Initial Dataset."
            slblDesc.Visible = True
        Finally
            lbInitData.EndUpdate()
        End Try
    End Sub

#Region "Mouse Hover / Leave Handlers"
    Private Sub MouseLeaveHandler(sender As Object, e As EventArgs)
        If isRefinedLoading = True Or lbRefinedData.Items.Count = 0 Then
            slblDesc.Text = "To calibrate, add data to the Initial Dataset, choose a Calibration Method, set Smoothing Parameters."
            slblDesc.Visible = True
        Else
            slblDesc.Visible = False
        End If
    End Sub

    Private Sub txtInitAdd_MouseHover(sender As Object, e As EventArgs) Handles txtInitAdd.MouseHover
        slblDesc.Visible = True
        slblDesc.Text = "Enter a numeric value to add to the Initial Dataset. Press Enter Key or click the Add button to submit."
    End Sub

    Private Sub txtInitAdd_MouseLeave(sender As Object, e As EventArgs) Handles txtInitAdd.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub btnInitAdd_MouseHover(sender As Object, e As EventArgs) Handles btnInitAdd.MouseHover
        slblDesc.Visible = True
        slblDesc.Text = "Add the entered value to the Initial Dataset."
    End Sub

    Private Sub btnInitAdd_MouseLeave(sender As Object, e As EventArgs) Handles btnInitAdd.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub rbtnAllMedian_MouseHover(sender As Object, e As EventArgs) Handles rbtnAllMedian.MouseHover
        slblDesc.Visible = True
        slblDesc.Text = "Applies a median filter to the entire dataset, smoothing out noise while preserving overall trends."
    End Sub

    Private Sub rbtnAllMedian_MouseLeave(sender As Object, e As EventArgs) Handles rbtnAllMedian.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub rbtnMidMedian_MouseHover(sender As Object, e As EventArgs) Handles rbtnMidMedian.MouseHover
        slblDesc.Visible = True
        slblDesc.Text = "Applies a median filter to the middle portion of the dataset, focusing on central trends while reducing edge effects."
    End Sub

    Private Sub rbtnMidMedian_MouseLeave(sender As Object, e As EventArgs) Handles rbtnMidMedian.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub rbtnXLSX_MouseHover(sender As Object, e As EventArgs) Handles rbtnXLSX.MouseHover
        slblDesc.Visible = True
        slblDesc.Text = "Exports the smoothed dataset to an Excel file, allowing for further analysis and visualization."
    End Sub

    Private Sub rbtnXLSX_MouseLeave(sender As Object, e As EventArgs) Handles rbtnXLSX.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub rbtnCSV_MouseHover(sender As Object, e As EventArgs) Handles rbtnCSV.MouseHover
        slblDesc.Visible = True
        slblDesc.Text = "Exports the smoothed dataset to a CSV file, providing a simple text format for data exchange."
    End Sub

    Private Sub rbtnCSV_MouseLeave(sender As Object, e As EventArgs) Handles rbtnCSV.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub lblKernelRadius_MouseHover(sender As Object, e As EventArgs) Handles lblKernelRadius.MouseHover
        slblDesc.Visible = True
        slblDesc.Text = "Select the kernel radius for the median filter. A larger radius smooths more but may lose detail."
    End Sub

    Private Sub lblKernelRadius_MouseLeave(sender As Object, e As EventArgs) Handles lblKernelRadius.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub cbxKernelRadius_MouseHover(sender As Object, e As EventArgs) Handles cbxKernelRadius.MouseHover
        slblDesc.Visible = True
        slblDesc.Text = "Select the kernel radius for the median filter. A larger radius smooths more but may lose detail."
    End Sub

    Private Sub cbxKernelRadius_MouseLeave(sender As Object, e As EventArgs) Handles cbxKernelRadius.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub lblBorderCount_MouseHover(sender As Object, e As EventArgs) Handles lblBorderCount.MouseHover
        slblDesc.Visible = True
        slblDesc.Text = "Select the number of border elements to consider when applying the median filter. More borders can help preserve edge details."
    End Sub

    Private Sub lblBorderCount_MouseLeave(sender As Object, e As EventArgs) Handles lblBorderCount.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub cbxBorderCount_MouseHover(sender As Object, e As EventArgs) Handles cbxBorderCount.MouseHover
        slblDesc.Visible = True
        slblDesc.Text = "Select the number of border elements to consider when applying the median filter. More borders can help preserve edge details."
    End Sub

    Private Sub cbxBorderCount_MouseLeave(sender As Object, e As EventArgs) Handles cbxBorderCount.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub btnInitClear_MouseHover(sender As Object, e As EventArgs) Handles btnInitClear.MouseHover
        slblDesc.Visible = True

        Dim initItemCount As Integer = lbInitData.Items.Count
        Dim refItemCount As Integer = lbRefinedData.Items.Count

        Dim initialMsg As String = If(initItemCount = 1,
                                  "Delete the item from the Initial Dataset",
                                  $"Delete all {initItemCount} items from the Initial Dataset")

        Dim refinedMsg As String = If(refItemCount = 0,
                                 String.Empty,
                                 If(refItemCount = 1,
                                    "and Delete the item from the Refined Dataset",
                                    $"and Delete all {refItemCount} items from the Refined Dataset"))

        slblDesc.Text = If(String.IsNullOrEmpty(refinedMsg),
                       initialMsg & ".",
                       $"{initialMsg} {refinedMsg}.")
    End Sub

    Private Sub btnInitClear_MouseLeave(sender As Object, e As EventArgs) Handles btnInitClear.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub btnInitCopy_MouseHover(sender As Object, e As EventArgs) Handles btnInitCopy.MouseHover
        Dim selCount As Integer = lbInitData.SelectedItems.Count
        Dim totalCount As Integer = lbInitData.Items.Count
        slblDesc.Visible = True

        slblDesc.Text = If(selCount = 0 OrElse selCount = totalCount,
                       $"Copy all {totalCount} items from the Initial Dataset to the clipboard.",
                       If(selCount = 1,
                          "Copy the selected item from the Initial Dataset to the clipboard.",
                          $"Copy {selCount} selected items from the Initial Dataset to the clipboard."))
    End Sub

    Private Sub btnInitCopy_MouseLeave(sender As Object, e As EventArgs) Handles btnInitCopy.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub btnInitPaste_MouseHover(sender As Object, e As EventArgs) Handles btnInitPaste.MouseHover
        slblDesc.Visible = True
        slblDesc.Text = "Paste numeric values from the clipboard into the Initial Dataset."
    End Sub

    Private Sub btnInitPaste_MouseLeave(sender As Object, e As EventArgs) Handles btnInitPaste.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub btnInitEdit_MouseHover(sender As Object, e As EventArgs) Handles btnInitEdit.MouseHover
        Dim selCount As Integer = lbInitData.SelectedItems.Count
        slblDesc.Visible = True

        If selCount = 1 Then
            slblDesc.Text = "Edit the selected item in the Initial Dataset."
        Else
            slblDesc.Text = $"Edit {selCount} selected items in the Initial Dataset."
        End If
    End Sub

    Private Sub btnInitEdit_MouseLeave(sender As Object, e As EventArgs) Handles btnInitEdit.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub btnInitDelete_MouseHover(sender As Object, e As EventArgs) Handles btnInitDelete.MouseHover
        slblDesc.Visible = True

        Dim selCount As Integer = lbInitData.SelectedItems.Count
        Dim totalCount As Integer = lbInitData.Items.Count
        Dim refCount As Integer = lbRefinedData.Items.Count

        If selCount = 1 Then
            slblDesc.Text = "Delete the selected item from the Initial Dataset"
        ElseIf selCount = totalCount AndAlso totalCount > 0 Then
            Dim initMsg As String = $"Delete all {selCount} items from the Initial Dataset"

            If refCount > 0 Then
                Dim refMsg As String = If(refCount = 1,
                                  " and Delete the item from the Refined Dataset",
                                  $" and Delete all {refCount} items from the Refined Dataset")
                slblDesc.Text = initMsg & refMsg & "."
            Else
                slblDesc.Text = initMsg & "."
            End If
        Else
            Dim selText As String = If(selCount = 1, "item", "items")
            slblDesc.Text = $"Delete {selCount} selected {selText} from the Initial Dataset."
        End If
    End Sub

    Private Sub btnInitDelete_MouseLeave(sender As Object, e As EventArgs) Handles btnInitDelete.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub btnInitSelectAll_MouseHover(sender As Object, e As EventArgs) Handles btnInitSelectAll.MouseHover
        Dim itemCount As Integer = lbInitData.Items.Count
        slblDesc.Visible = True

        If (itemCount = 1) Then
            slblDesc.Text = "Select the item in the Initial Dataset"
        ElseIf (itemCount > 1) Then
            slblDesc.Text = $"Select all items in the Initial Dataset."
        End If
    End Sub

    Private Sub btnInitSelectAll_MouseLeave(sender As Object, e As EventArgs) Handles btnInitSelectAll.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub btnInitSelClear_MouseHover(sender As Object, e As EventArgs)
        Dim selCount As Integer = lbInitData.SelectedItems.Count
        slblDesc.Visible = True

        If selCount = 1 Then
            slblDesc.Text = "Deselect the selected item in the Initial Dataset."
        Else
            slblDesc.Text = $"Deselect all {selCount} selected items in the Initial Dataset."
        End If
    End Sub

    Private Sub btnInitSelectClr_MouseLeave(sender As Object, e As EventArgs) Handles btnInitSelectClr.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub btnInitSelectClr_MouseHover(sender As Object, e As EventArgs) Handles btnInitSelectSync.MouseHover, btnInitSelectClr.MouseHover
        Dim selCount As Integer = lbInitData.SelectedItems.Count
        slblDesc.Visible = True

        If selCount = 1 Then
            slblDesc.Text = "Deselect the selected item in the Initial Dataset."
        Else
            slblDesc.Text = $"Deselect all {selCount} selected items in the Initial Dataset."
        End If
    End Sub

    Private Sub btnInitSync_MouseLeave(sender As Object, e As EventArgs) Handles btnInitSelectSync.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub txtDatasetTitle_MouseHover(sender As Object, e As EventArgs) Handles txtDatasetTitle.MouseHover
        slblDesc.Visible = True

        If txtDatasetTitle.Text = ExcelTitlePlaceholder Then
            slblDesc.Text = "Enter a title for the Excel document. This will be used as the main title in the first cell."
        Else
            slblDesc.Text = "Edit the title for the Excel document. This will be used as the main title in the first cell."
        End If
    End Sub

    Private Sub txtDatasetTitle_MouseLeave(sender As Object, e As EventArgs) Handles txtDatasetTitle.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub btnRefClear_MouseHover(sender As Object, e As EventArgs) Handles btnRefClear.MouseHover
        slblDesc.Visible = True

        Dim itemCount As Integer = lbRefinedData.Items.Count

        If (itemCount = 1) Then
            slblDesc.Text = "Delete the item from the Refined Dataset."
        Else
            slblDesc.Text = $"Delete all {itemCount} items from the Refined Dataset."
        End If
    End Sub

    Private Sub btnRefClear_MouseLeave(sender As Object, e As EventArgs) Handles btnRefClear.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub btnRefCopy_MouseHover(sender As Object, e As EventArgs) Handles btnRefCopy.MouseHover
        Dim selCount As Integer = lbRefinedData.SelectedItems.Count
        Dim totalCount As Integer = lbRefinedData.Items.Count
        slblDesc.Visible = True

        slblDesc.Text = If(selCount = 0 OrElse selCount = totalCount,
                       $"Copy all {totalCount} items from the Refined Dataset to the clipboard.",
                       If(selCount = 1,
                          "Copy the selected item from the Refined Dataset to the clipboard.",
                          $"Copy {selCount} selected items from the Refined Dataset to the clipboard."))
    End Sub

    Private Sub btnRefCopy_MouseLeave(sender As Object, e As EventArgs) Handles btnRefCopy.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub btnRefSelectAll_MouseHover(sender As Object, e As EventArgs) Handles btnRefSelectAll.MouseHover
        Dim itemCount As Integer = lbRefinedData.Items.Count
        slblDesc.Visible = True

        If (itemCount = 1) Then
            slblDesc.Text = "Select the item in the Refined Dataset."
        ElseIf (itemCount > 1) Then
            slblDesc.Text = $"Select all items in the Refined Dataset."
        End If
    End Sub

    Private Sub btnRefSelectAll_MouseLeave(sender As Object, e As EventArgs) Handles btnRefSelectAll.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub btnRefSelectClr_MouseHover(sender As Object, e As EventArgs) Handles btnRefSelectClr.MouseHover
        Dim selCount As Integer = lbRefinedData.SelectedItems.Count
        slblDesc.Visible = True

        If selCount = 1 Then
            slblDesc.Text = "Deselect the selected item in the Refined Dataset."
        Else
            slblDesc.Text = $"Deselect all {selCount} selected items in the Refined Dataset."
        End If
    End Sub

    Private Sub btnRefSelectClr_MouseLeave(sender As Object, e As EventArgs) Handles btnRefSelectClr.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub btnRefSync_MouseHover(sender As Object, e As EventArgs) Handles btnRefSelectSync.MouseHover
        Dim selCount As Integer = lbRefinedData.SelectedItems.Count
        slblDesc.Visible = True

        If selCount = 1 Then
            slblDesc.Text = "Select the corresponding item in the Initial Dataset."
        Else
            slblDesc.Text = $"Select {selCount} corresponding items in the Initial Dataset."
        End If
    End Sub

    Private Sub btnRefSync_MouseLeave(sender As Object, e As EventArgs) Handles btnRefSelectSync.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub btnCalibrate_MouseHover(sender As Object, e As EventArgs) Handles btnCalibrate.MouseHover
        Dim itemCount As Integer = lbInitData.Items.Count
        slblDesc.Visible = True

        If (itemCount = 0) Then
            slblDesc.Text = "To calibrate, add data to the Initial Dataset, choose a Calibration Method, set Smoothing Parameters."
        Else
            slblDesc.Text = "Click to start the calibration process using the selected smoothing method and parameters."
        End If
    End Sub

    Private Sub btnCalibrate_MouseLeave(sender As Object, e As EventArgs) Handles btnCalibrate.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub btnExport_MouseHover(sender As Object, e As EventArgs) Handles btnExport.MouseHover
        slblDesc.Visible = True

        Dim itemCount As Integer = lbRefinedData.Items.Count
        If (itemCount = 0) Then
            slblDesc.Text = "To export, first calibrate the data using the Calibrate button."
        ElseIf (rbtnXLSX.Checked) Then
            slblDesc.Text = "Click to export to an Excel file with the specified title and settings."
        ElseIf (rbtnCSV.Checked) Then
            slblDesc.Text = "Click to export to a CSV file with the specified title and settings."
        End If
    End Sub

    Private Sub btnExport_MouseLeave(sender As Object, e As EventArgs) Handles btnExport.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub lblBoundaryMethod_MouseHover(sender As Object, e As EventArgs) Handles lblBoundaryMethod.MouseHover
        slblDesc.Visible = True
        slblDesc.Text = "Specifies how edge data points are treated during smoothing : Symmetric, Replicate, Adaptive, or Zero Padding."
    End Sub

    Private Sub cbxBoundaryMethod_MouseLeave(sender As Object, e As EventArgs) Handles cbxBoundaryMethod.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub

    Private Sub cbxBoundaryMethod_MouseHover(sender As Object, e As EventArgs) Handles cbxBoundaryMethod.MouseHover
        slblDesc.Visible = True
        slblDesc.Text = "Specifies how edge data points are treated during smoothing : Symmetric, Replicate, Adaptive, or Zero Padding."
    End Sub

    Private Sub lblBoundaryMethod_MouseLeave(sender As Object, e As EventArgs) Handles lblBoundaryMethod.MouseLeave
        MouseLeaveHandler(sender, e)
    End Sub
#End Region

End Class

