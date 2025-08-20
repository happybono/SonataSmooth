Imports System
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports System.Runtime.CompilerServices


Module ControlExtensions
    <Extension()>
    Public Function InvokeAsync(ctrl As Control, action As Action) As Task
        If ctrl.InvokeRequired Then
            Return Task.Factory.StartNew(Sub() ctrl.Invoke(action), Threading.CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default)
        Else
            action()
            Return Task.CompletedTask
        End If
    End Function
End Module

Public Class FrmModify

    Private Async Sub OK_Button_Click(sender As Object, e As EventArgs) Handles OK_Button.Click
        Dim numericValue As Double

        ' mainForm 인스턴스 가져오기
        Dim mainForm = Application.OpenForms _
                        .OfType(Of FrmMain)() _
                        .FirstOrDefault()

        If mainForm Is Nothing Then
            MessageBox.Show("Main form not found.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        ' 입력 유효성 검사
        If String.IsNullOrEmpty(textBox1.Text) OrElse
           Not Double.TryParse(textBox1.Text, numericValue) Then
            textBox1.Select()
            textBox1.SelectAll()
            Return
        End If

        ' 선택된 Index 정렬 후 배열로
        Dim indices = mainForm.ListBox1 _
                        .SelectedIndices _
                        .Cast(Of Integer)() _
                        .OrderBy(Function(x) x) _
                        .ToArray()
        Dim total = indices.Length
        If total = 0 Then Return

        ' ProgressBar 초기화
        ProgressBar1.Minimum = 0
        ProgressBar1.Maximum = total
        ProgressBar1.Value = 0

        ' ListBox 업데이트 일시 중지
        Dim lb = mainForm.ListBox1
        lb.BeginUpdate()

        ' 새로운 값 미리 생성
        Dim newValue As String = numericValue.ToString("G")

        Const BatchSize As Integer = 1000
        Dim done As Integer = 0

        ' 병렬 / 비동기 Batch 처리
        While done < total
            Dim cnt As Integer = Math.Min(BatchSize, total - done)
            Dim batchIndices = indices.Skip(done).Take(cnt).ToArray()
            Dim batchValues = Enumerable.Repeat(newValue, batchIndices.Length).ToArray()

            ' UI Thread 에서 항목 변경
            Await Me.InvokeAsync(Sub()
                                     For i = 0 To batchIndices.Length - 1
                                         lb.Items(batchIndices(i)) = batchValues(i)
                                     Next
                                     ProgressBar1.Value = Math.Min(done + cnt, total)
                                 End Sub)

            done += cnt
            Await Task.Yield()
        End While

        ' UI Thread 에서 변경된 항목 재선택
        Await Me.InvokeAsync(Sub()
                                 lb.ClearSelected()
                                 For Each idx In indices
                                     If idx >= 0 AndAlso idx < lb.Items.Count Then
                                         lb.SetSelected(idx, True)
                                     End If
                                 Next
                                 lb.EndUpdate()
                                 mainForm.ListBox1.Focus()
                                 ProgressBar1.Value = 0
                                 Me.Close()
                             End Sub)
    End Sub

    Private Sub FrmModify_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' mainForm 인스턴스 가져오기
        Dim mainForm = Application.OpenForms _
                            .OfType(Of FrmMain)() _
                            .FirstOrDefault()
        If mainForm Is Nothing Then
            ToolStripStatusLabel1.Text = "Main form not found."
            Return
        End If

        Dim count = mainForm.ListBox1.SelectedItems.Count
        If count > 1 Then
            ToolStripStatusLabel1.Text = $"Modifying {count} selected items..."
        Else
            ToolStripStatusLabel1.Text = "Modifying the selected item..."
        End If

        textBox1.Text = mainForm.ListBox1.SelectedItem.ToString()
        textBox1.SelectAll()
        textBox1.Select()
    End Sub

    Private Sub textBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles textBox1.KeyDown
        If e.KeyData = Keys.Enter Then
            OK_Button.PerformClick()
            e.SuppressKeyPress = True
        ElseIf e.KeyData = Keys.Escape Then
            Cancel_Button.PerformClick()
            e.SuppressKeyPress = True
        End If
    End Sub

    Private Sub textBox1_TextChanged(sender As Object, e As EventArgs) Handles textBox1.TextChanged
        OK_Button.Enabled = textBox1.Text.Length > 0 AndAlso
                                Double.TryParse(textBox1.Text, Nothing)
    End Sub

    Private Sub Cancel_Button_Click(sender As Object, e As EventArgs) Handles Cancel_Button.Click
        Me.Close()
    End Sub
End Class
