Public Class AboutBox
    Private Sub AboutBox_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Label1.Select()
        lblVersion.Text = " v." & ProductVersion
    End Sub

    Private Sub lblCopyright_Click(sender As Object, e As EventArgs) Handles lblCopyright.Click
        Process.Start("https://www.github.com/happybono")
    End Sub

    Private Sub btnDonation_Click(sender As Object, e As EventArgs) Handles btnDonation.Click
        Process.Start("https://www.paypal.com/ncp/payment/ZYELY39LHBSUU", vbMinimizedNoFocus)
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        Me.Close()
    End Sub
End Class