<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmMain))
        Me.ListBox1 = New System.Windows.Forms.ListBox()
        Me.ListBox2 = New System.Windows.Forms.ListBox()
        Me.calcButton = New System.Windows.Forms.Button()
        Me.RadioButton1 = New System.Windows.Forms.RadioButton()
        Me.RadioButton2 = New System.Windows.Forms.RadioButton()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.addButton = New System.Windows.Forms.Button()
        Me.copyButton1 = New System.Windows.Forms.Button()
        Me.cbxBorderCount = New System.Windows.Forms.ComboBox()
        Me.clearButton1 = New System.Windows.Forms.Button()
        Me.clearButton2 = New System.Windows.Forms.Button()
        Me.deleteButton1 = New System.Windows.Forms.Button()
        Me.pasteButton = New System.Windows.Forms.Button()
        Me.copyButton2 = New System.Windows.Forms.Button()
        Me.selectAllButton1 = New System.Windows.Forms.Button()
        Me.selectAllButton2 = New System.Windows.Forms.Button()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.syncButton1 = New System.Windows.Forms.Button()
        Me.editButton = New System.Windows.Forms.Button()
        Me.lblCnt1 = New System.Windows.Forms.Label()
        Me.sClrButton1 = New System.Windows.Forms.Button()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.syncButton2 = New System.Windows.Forms.Button()
        Me.lblCnt2 = New System.Windows.Forms.Label()
        Me.sClrButton2 = New System.Windows.Forms.Button()
        Me.progressBar1 = New System.Windows.Forms.ProgressBar()
        Me.statusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.tlblCalibratedType = New System.Windows.Forms.ToolStripStatusLabel()
        Me.slblCalibratedType = New System.Windows.Forms.ToolStripStatusLabel()
        Me.slblSeparator1 = New System.Windows.Forms.ToolStripStatusLabel()
        Me.tlblKernelWidth = New System.Windows.Forms.ToolStripStatusLabel()
        Me.slblKernelWidth = New System.Windows.Forms.ToolStripStatusLabel()
        Me.slblSeparator2 = New System.Windows.Forms.ToolStripStatusLabel()
        Me.tlblBorderCount = New System.Windows.Forms.ToolStripStatusLabel()
        Me.slblBorderCount = New System.Windows.Forms.ToolStripStatusLabel()
        Me.groupBox5 = New System.Windows.Forms.GroupBox()
        Me.lblBorderCount = New System.Windows.Forms.Label()
        Me.cbxKernelRadius = New System.Windows.Forms.ComboBox()
        Me.label1 = New System.Windows.Forms.Label()
        Me.groupBox4 = New System.Windows.Forms.GroupBox()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.btnExport = New System.Windows.Forms.Button()
        Me.btnInfo = New System.Windows.Forms.Button()
        Me.txtDatasetTitle = New System.Windows.Forms.TextBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.rbtnCSV = New System.Windows.Forms.RadioButton()
        Me.rbtnXLSX = New System.Windows.Forms.RadioButton()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.statusStrip1.SuspendLayout()
        Me.groupBox5.SuspendLayout()
        Me.groupBox4.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'ListBox1
        '
        Me.ListBox1.AllowDrop = True
        Me.ListBox1.Font = New System.Drawing.Font("Segoe UI Variable Display Semib", 10.125!, System.Drawing.FontStyle.Bold)
        Me.ListBox1.FormattingEnabled = True
        Me.ListBox1.ItemHeight = 17
        Me.ListBox1.Location = New System.Drawing.Point(7, 31)
        Me.ListBox1.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.ListBox1.Name = "ListBox1"
        Me.ListBox1.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.ListBox1.Size = New System.Drawing.Size(294, 497)
        Me.ListBox1.TabIndex = 4
        '
        'ListBox2
        '
        Me.ListBox2.Font = New System.Drawing.Font("Segoe UI Variable Display Semib", 10.125!, System.Drawing.FontStyle.Bold)
        Me.ListBox2.FormattingEnabled = True
        Me.ListBox2.ItemHeight = 17
        Me.ListBox2.Location = New System.Drawing.Point(7, 31)
        Me.ListBox2.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.ListBox2.Name = "ListBox2"
        Me.ListBox2.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.ListBox2.Size = New System.Drawing.Size(294, 497)
        Me.ListBox2.TabIndex = 21
        '
        'calcButton
        '
        Me.calcButton.Font = New System.Drawing.Font("Segoe Fluent Icons", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.calcButton.Location = New System.Drawing.Point(14, 782)
        Me.calcButton.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.calcButton.Name = "calcButton"
        Me.calcButton.Size = New System.Drawing.Size(465, 40)
        Me.calcButton.TabIndex = 19
        Me.calcButton.Text = ""
        Me.ToolTip1.SetToolTip(Me.calcButton, "Calibrate")
        Me.calcButton.UseVisualStyleBackColor = True
        '
        'RadioButton1
        '
        Me.RadioButton1.Appearance = System.Windows.Forms.Appearance.Button
        Me.RadioButton1.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.125!)
        Me.RadioButton1.Location = New System.Drawing.Point(175, 24)
        Me.RadioButton1.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.RadioButton1.Name = "RadioButton1"
        Me.RadioButton1.Size = New System.Drawing.Size(150, 30)
        Me.RadioButton1.TabIndex = 15
        Me.RadioButton1.Text = "Middle Median"
        Me.RadioButton1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.RadioButton1.UseVisualStyleBackColor = True
        '
        'RadioButton2
        '
        Me.RadioButton2.Appearance = System.Windows.Forms.Appearance.Button
        Me.RadioButton2.Checked = True
        Me.RadioButton2.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.125!)
        Me.RadioButton2.Location = New System.Drawing.Point(19, 24)
        Me.RadioButton2.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.RadioButton2.Name = "RadioButton2"
        Me.RadioButton2.Size = New System.Drawing.Size(150, 30)
        Me.RadioButton2.TabIndex = 14
        Me.RadioButton2.TabStop = True
        Me.RadioButton2.Text = "All Median"
        Me.RadioButton2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.RadioButton2.UseVisualStyleBackColor = True
        '
        'TextBox1
        '
        Me.TextBox1.Font = New System.Drawing.Font("Segoe UI Variable Display Semib", 10.125!, System.Drawing.FontStyle.Bold)
        Me.TextBox1.Location = New System.Drawing.Point(26, 14)
        Me.TextBox1.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(262, 25)
        Me.TextBox1.TabIndex = 1
        '
        'addButton
        '
        Me.addButton.Enabled = False
        Me.addButton.Font = New System.Drawing.Font("Segoe Fluent Icons", 12.75!)
        Me.addButton.Location = New System.Drawing.Point(292, 12)
        Me.addButton.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.addButton.Name = "addButton"
        Me.addButton.Size = New System.Drawing.Size(67, 30)
        Me.addButton.TabIndex = 2
        Me.addButton.Text = ""
        Me.ToolTip1.SetToolTip(Me.addButton, "Add")
        Me.addButton.UseVisualStyleBackColor = True
        '
        'copyButton1
        '
        Me.copyButton1.Enabled = False
        Me.copyButton1.Font = New System.Drawing.Font("Segoe Fluent Icons", 12.75!)
        Me.copyButton1.Location = New System.Drawing.Point(307, 65)
        Me.copyButton1.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.copyButton1.Name = "copyButton1"
        Me.copyButton1.Size = New System.Drawing.Size(30, 30)
        Me.copyButton1.TabIndex = 6
        Me.copyButton1.Text = ""
        Me.ToolTip1.SetToolTip(Me.copyButton1, "Copy")
        Me.copyButton1.UseVisualStyleBackColor = True
        '
        'cbxBorderCount
        '
        Me.cbxBorderCount.DropDownHeight = 150
        Me.cbxBorderCount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbxBorderCount.Font = New System.Drawing.Font("Segoe UI Variable Display Semib", 10.125!, System.Drawing.FontStyle.Bold)
        Me.cbxBorderCount.FormattingEnabled = True
        Me.cbxBorderCount.IntegralHeight = False
        Me.cbxBorderCount.Items.AddRange(New Object() {"1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21"})
        Me.cbxBorderCount.Location = New System.Drawing.Point(193, 80)
        Me.cbxBorderCount.Name = "cbxBorderCount"
        Me.cbxBorderCount.Size = New System.Drawing.Size(80, 25)
        Me.cbxBorderCount.TabIndex = 18
        '
        'clearButton1
        '
        Me.clearButton1.Font = New System.Drawing.Font("Segoe Fluent Icons", 12.75!)
        Me.clearButton1.Location = New System.Drawing.Point(307, 31)
        Me.clearButton1.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.clearButton1.Name = "clearButton1"
        Me.clearButton1.Size = New System.Drawing.Size(30, 30)
        Me.clearButton1.TabIndex = 5
        Me.clearButton1.Text = ""
        Me.ToolTip1.SetToolTip(Me.clearButton1, "Clear")
        Me.clearButton1.UseVisualStyleBackColor = True
        '
        'clearButton2
        '
        Me.clearButton2.Font = New System.Drawing.Font("Segoe Fluent Icons", 12.75!)
        Me.clearButton2.Location = New System.Drawing.Point(307, 31)
        Me.clearButton2.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.clearButton2.Name = "clearButton2"
        Me.clearButton2.Size = New System.Drawing.Size(30, 30)
        Me.clearButton2.TabIndex = 22
        Me.clearButton2.Text = ""
        Me.ToolTip1.SetToolTip(Me.clearButton2, "Clear")
        Me.clearButton2.UseVisualStyleBackColor = True
        '
        'deleteButton1
        '
        Me.deleteButton1.Font = New System.Drawing.Font("Segoe Fluent Icons", 12.75!)
        Me.deleteButton1.Location = New System.Drawing.Point(307, 167)
        Me.deleteButton1.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.deleteButton1.Name = "deleteButton1"
        Me.deleteButton1.Size = New System.Drawing.Size(30, 30)
        Me.deleteButton1.TabIndex = 9
        Me.deleteButton1.Text = ""
        Me.ToolTip1.SetToolTip(Me.deleteButton1, "Delete")
        Me.deleteButton1.UseVisualStyleBackColor = True
        '
        'pasteButton
        '
        Me.pasteButton.Font = New System.Drawing.Font("Segoe Fluent Icons", 12.75!)
        Me.pasteButton.Location = New System.Drawing.Point(307, 99)
        Me.pasteButton.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.pasteButton.Name = "pasteButton"
        Me.pasteButton.Size = New System.Drawing.Size(30, 30)
        Me.pasteButton.TabIndex = 7
        Me.pasteButton.Text = ""
        Me.ToolTip1.SetToolTip(Me.pasteButton, "Paste")
        Me.pasteButton.UseVisualStyleBackColor = True
        '
        'copyButton2
        '
        Me.copyButton2.Enabled = False
        Me.copyButton2.Font = New System.Drawing.Font("Segoe Fluent Icons", 12.75!)
        Me.copyButton2.Location = New System.Drawing.Point(307, 65)
        Me.copyButton2.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.copyButton2.Name = "copyButton2"
        Me.copyButton2.Size = New System.Drawing.Size(30, 30)
        Me.copyButton2.TabIndex = 23
        Me.copyButton2.Text = ""
        Me.ToolTip1.SetToolTip(Me.copyButton2, "Copy")
        Me.copyButton2.UseVisualStyleBackColor = True
        '
        'selectAllButton1
        '
        Me.selectAllButton1.Font = New System.Drawing.Font("Segoe Fluent Icons", 12.75!)
        Me.selectAllButton1.Location = New System.Drawing.Point(307, 201)
        Me.selectAllButton1.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.selectAllButton1.Name = "selectAllButton1"
        Me.selectAllButton1.Size = New System.Drawing.Size(30, 30)
        Me.selectAllButton1.TabIndex = 10
        Me.selectAllButton1.Text = ""
        Me.ToolTip1.SetToolTip(Me.selectAllButton1, "Select All")
        Me.selectAllButton1.UseVisualStyleBackColor = True
        '
        'selectAllButton2
        '
        Me.selectAllButton2.Font = New System.Drawing.Font("Segoe Fluent Icons", 12.75!)
        Me.selectAllButton2.Location = New System.Drawing.Point(307, 99)
        Me.selectAllButton2.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.selectAllButton2.Name = "selectAllButton2"
        Me.selectAllButton2.Size = New System.Drawing.Size(30, 30)
        Me.selectAllButton2.TabIndex = 24
        Me.selectAllButton2.Text = ""
        Me.ToolTip1.SetToolTip(Me.selectAllButton2, "Select All")
        Me.selectAllButton2.UseVisualStyleBackColor = True
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.syncButton1)
        Me.GroupBox2.Controls.Add(Me.editButton)
        Me.GroupBox2.Controls.Add(Me.lblCnt1)
        Me.GroupBox2.Controls.Add(Me.sClrButton1)
        Me.GroupBox2.Controls.Add(Me.selectAllButton1)
        Me.GroupBox2.Controls.Add(Me.pasteButton)
        Me.GroupBox2.Controls.Add(Me.deleteButton1)
        Me.GroupBox2.Controls.Add(Me.clearButton1)
        Me.GroupBox2.Controls.Add(Me.copyButton1)
        Me.GroupBox2.Controls.Add(Me.ListBox1)
        Me.GroupBox2.Font = New System.Drawing.Font("Segoe UI Variable Display Semib", 11.25!, System.Drawing.FontStyle.Bold)
        Me.GroupBox2.Location = New System.Drawing.Point(14, 52)
        Me.GroupBox2.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Padding = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.GroupBox2.Size = New System.Drawing.Size(344, 586)
        Me.GroupBox2.TabIndex = 3
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Initial Dataset"
        '
        'syncButton1
        '
        Me.syncButton1.Font = New System.Drawing.Font("Segoe Fluent Icons", 11.25!)
        Me.syncButton1.Location = New System.Drawing.Point(307, 269)
        Me.syncButton1.Name = "syncButton1"
        Me.syncButton1.Size = New System.Drawing.Size(30, 30)
        Me.syncButton1.TabIndex = 12
        Me.syncButton1.Text = ""
        Me.ToolTip1.SetToolTip(Me.syncButton1, "Match Selection" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "( ▶ Refined Dataset )")
        Me.syncButton1.UseVisualStyleBackColor = True
        '
        'editButton
        '
        Me.editButton.Enabled = False
        Me.editButton.Font = New System.Drawing.Font("Segoe Fluent Icons", 12.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.editButton.Location = New System.Drawing.Point(307, 133)
        Me.editButton.Margin = New System.Windows.Forms.Padding(2, 2, 2, 2)
        Me.editButton.Name = "editButton"
        Me.editButton.Size = New System.Drawing.Size(30, 30)
        Me.editButton.TabIndex = 8
        Me.editButton.Text = ""
        Me.ToolTip1.SetToolTip(Me.editButton, "Edit")
        Me.editButton.UseVisualStyleBackColor = True
        '
        'lblCnt1
        '
        Me.lblCnt1.AutoSize = True
        Me.lblCnt1.Font = New System.Drawing.Font("Segoe UI Variable Display Semib", 10.125!, System.Drawing.FontStyle.Bold)
        Me.lblCnt1.Location = New System.Drawing.Point(7, 555)
        Me.lblCnt1.Margin = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.lblCnt1.Name = "lblCnt1"
        Me.lblCnt1.Size = New System.Drawing.Size(65, 19)
        Me.lblCnt1.TabIndex = 26
        Me.lblCnt1.Text = "Count : 0"
        '
        'sClrButton1
        '
        Me.sClrButton1.Font = New System.Drawing.Font("Segoe Fluent Icons", 12.75!)
        Me.sClrButton1.Location = New System.Drawing.Point(307, 235)
        Me.sClrButton1.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.sClrButton1.Name = "sClrButton1"
        Me.sClrButton1.Size = New System.Drawing.Size(30, 30)
        Me.sClrButton1.TabIndex = 11
        Me.sClrButton1.Text = ""
        Me.ToolTip1.SetToolTip(Me.sClrButton1, "Deselect All")
        Me.sClrButton1.UseVisualStyleBackColor = True
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.syncButton2)
        Me.GroupBox3.Controls.Add(Me.lblCnt2)
        Me.GroupBox3.Controls.Add(Me.sClrButton2)
        Me.GroupBox3.Controls.Add(Me.selectAllButton2)
        Me.GroupBox3.Controls.Add(Me.copyButton2)
        Me.GroupBox3.Controls.Add(Me.clearButton2)
        Me.GroupBox3.Controls.Add(Me.ListBox2)
        Me.GroupBox3.Font = New System.Drawing.Font("Segoe UI Variable Display Semib", 11.25!, System.Drawing.FontStyle.Bold)
        Me.GroupBox3.Location = New System.Drawing.Point(375, 52)
        Me.GroupBox3.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Padding = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.GroupBox3.Size = New System.Drawing.Size(344, 586)
        Me.GroupBox3.TabIndex = 20
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Refined Dataset"
        '
        'syncButton2
        '
        Me.syncButton2.Font = New System.Drawing.Font("Segoe Fluent Icons", 11.25!)
        Me.syncButton2.Location = New System.Drawing.Point(307, 167)
        Me.syncButton2.Name = "syncButton2"
        Me.syncButton2.Size = New System.Drawing.Size(30, 30)
        Me.syncButton2.TabIndex = 26
        Me.syncButton2.Text = ""
        Me.ToolTip1.SetToolTip(Me.syncButton2, "Match Selection " & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "( ◀ Initial Dataset )")
        Me.syncButton2.UseVisualStyleBackColor = True
        '
        'lblCnt2
        '
        Me.lblCnt2.AutoSize = True
        Me.lblCnt2.Font = New System.Drawing.Font("Segoe UI Variable Display Semib", 10.125!, System.Drawing.FontStyle.Bold)
        Me.lblCnt2.Location = New System.Drawing.Point(7, 555)
        Me.lblCnt2.Margin = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.lblCnt2.Name = "lblCnt2"
        Me.lblCnt2.Size = New System.Drawing.Size(65, 19)
        Me.lblCnt2.TabIndex = 27
        Me.lblCnt2.Text = "Count : 0"
        '
        'sClrButton2
        '
        Me.sClrButton2.Font = New System.Drawing.Font("Segoe Fluent Icons", 12.75!)
        Me.sClrButton2.Location = New System.Drawing.Point(307, 133)
        Me.sClrButton2.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.sClrButton2.Name = "sClrButton2"
        Me.sClrButton2.Size = New System.Drawing.Size(30, 30)
        Me.sClrButton2.TabIndex = 25
        Me.sClrButton2.Text = ""
        Me.ToolTip1.SetToolTip(Me.sClrButton2, "Deselect All")
        Me.sClrButton2.UseVisualStyleBackColor = True
        '
        'progressBar1
        '
        Me.progressBar1.Location = New System.Drawing.Point(0, 832)
        Me.progressBar1.Name = "progressBar1"
        Me.progressBar1.Size = New System.Drawing.Size(734, 5)
        Me.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous
        Me.progressBar1.TabIndex = 28
        '
        'statusStrip1
        '
        Me.statusStrip1.AutoSize = False
        Me.statusStrip1.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.statusStrip1.ImageScalingSize = New System.Drawing.Size(32, 32)
        Me.statusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.tlblCalibratedType, Me.slblCalibratedType, Me.slblSeparator1, Me.tlblKernelWidth, Me.slblKernelWidth, Me.slblSeparator2, Me.tlblBorderCount, Me.slblBorderCount})
        Me.statusStrip1.Location = New System.Drawing.Point(0, 837)
        Me.statusStrip1.Name = "statusStrip1"
        Me.statusStrip1.Size = New System.Drawing.Size(734, 24)
        Me.statusStrip1.SizingGrip = False
        Me.statusStrip1.TabIndex = 29
        Me.statusStrip1.Text = "statusStrip1"
        '
        'tlblCalibratedType
        '
        Me.tlblCalibratedType.Font = New System.Drawing.Font("Segoe UI Variable Display", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tlblCalibratedType.ForeColor = System.Drawing.Color.White
        Me.tlblCalibratedType.Name = "tlblCalibratedType"
        Me.tlblCalibratedType.Size = New System.Drawing.Size(114, 19)
        Me.tlblCalibratedType.Text = "Applied Calibration :"
        '
        'slblCalibratedType
        '
        Me.slblCalibratedType.Font = New System.Drawing.Font("Segoe UI Variable Display", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.slblCalibratedType.ForeColor = System.Drawing.Color.White
        Me.slblCalibratedType.Name = "slblCalibratedType"
        Me.slblCalibratedType.Size = New System.Drawing.Size(17, 19)
        Me.slblCalibratedType.Text = "--"
        '
        'slblSeparator1
        '
        Me.slblSeparator1.Font = New System.Drawing.Font("Segoe UI Variable Display", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.slblSeparator1.ForeColor = System.Drawing.Color.White
        Me.slblSeparator1.Name = "slblSeparator1"
        Me.slblSeparator1.Size = New System.Drawing.Size(16, 19)
        Me.slblSeparator1.Text = " | "
        '
        'tlblKernelWidth
        '
        Me.tlblKernelWidth.Font = New System.Drawing.Font("Segoe UI Variable Display", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tlblKernelWidth.ForeColor = System.Drawing.Color.White
        Me.tlblKernelWidth.Name = "tlblKernelWidth"
        Me.tlblKernelWidth.Size = New System.Drawing.Size(173, 19)
        Me.tlblKernelWidth.Text = "Noise Reduction Kernel Radius :"
        '
        'slblKernelWidth
        '
        Me.slblKernelWidth.Font = New System.Drawing.Font("Segoe UI Variable Display", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.slblKernelWidth.ForeColor = System.Drawing.Color.White
        Me.slblKernelWidth.Name = "slblKernelWidth"
        Me.slblKernelWidth.Size = New System.Drawing.Size(17, 19)
        Me.slblKernelWidth.Text = "--"
        '
        'slblSeparator2
        '
        Me.slblSeparator2.Font = New System.Drawing.Font("Segoe UI Variable Display", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.slblSeparator2.ForeColor = System.Drawing.Color.White
        Me.slblSeparator2.Name = "slblSeparator2"
        Me.slblSeparator2.Size = New System.Drawing.Size(16, 19)
        Me.slblSeparator2.Text = " | "
        Me.slblSeparator2.Visible = False
        '
        'tlblBorderCount
        '
        Me.tlblBorderCount.Font = New System.Drawing.Font("Segoe UI Variable Display", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tlblBorderCount.ForeColor = System.Drawing.Color.White
        Me.tlblBorderCount.Name = "tlblBorderCount"
        Me.tlblBorderCount.Size = New System.Drawing.Size(83, 19)
        Me.tlblBorderCount.Text = "Border Count :"
        Me.tlblBorderCount.Visible = False
        '
        'slblBorderCount
        '
        Me.slblBorderCount.Font = New System.Drawing.Font("Segoe UI Variable Display", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.slblBorderCount.ForeColor = System.Drawing.Color.White
        Me.slblBorderCount.Name = "slblBorderCount"
        Me.slblBorderCount.Size = New System.Drawing.Size(17, 19)
        Me.slblBorderCount.Text = "--"
        Me.slblBorderCount.Visible = False
        '
        'groupBox5
        '
        Me.groupBox5.Controls.Add(Me.cbxBorderCount)
        Me.groupBox5.Controls.Add(Me.lblBorderCount)
        Me.groupBox5.Controls.Add(Me.cbxKernelRadius)
        Me.groupBox5.Controls.Add(Me.label1)
        Me.groupBox5.Font = New System.Drawing.Font("Segoe UI Variable Display Semib", 11.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.groupBox5.Location = New System.Drawing.Point(375, 643)
        Me.groupBox5.Name = "groupBox5"
        Me.groupBox5.Size = New System.Drawing.Size(344, 130)
        Me.groupBox5.TabIndex = 16
        Me.groupBox5.TabStop = False
        Me.groupBox5.Text = "Signal Smoothing Parameters"
        '
        'lblBorderCount
        '
        Me.lblBorderCount.AutoSize = True
        Me.lblBorderCount.Enabled = False
        Me.lblBorderCount.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.125!)
        Me.lblBorderCount.Location = New System.Drawing.Point(72, 83)
        Me.lblBorderCount.Name = "lblBorderCount"
        Me.lblBorderCount.Size = New System.Drawing.Size(98, 19)
        Me.lblBorderCount.TabIndex = 20
        Me.lblBorderCount.Text = "Border Count :"
        '
        'cbxKernelRadius
        '
        Me.cbxKernelRadius.DropDownHeight = 150
        Me.cbxKernelRadius.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbxKernelRadius.Font = New System.Drawing.Font("Segoe UI Variable Display Semib", 10.125!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbxKernelRadius.FormattingEnabled = True
        Me.cbxKernelRadius.IntegralHeight = False
        Me.cbxKernelRadius.ItemHeight = 17
        Me.cbxKernelRadius.Items.AddRange(New Object() {"1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21"})
        Me.cbxKernelRadius.Location = New System.Drawing.Point(232, 42)
        Me.cbxKernelRadius.Margin = New System.Windows.Forms.Padding(2, 2, 2, 2)
        Me.cbxKernelRadius.Name = "cbxKernelRadius"
        Me.cbxKernelRadius.Size = New System.Drawing.Size(80, 25)
        Me.cbxKernelRadius.TabIndex = 17
        '
        'label1
        '
        Me.label1.AutoSize = True
        Me.label1.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.125!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.label1.Location = New System.Drawing.Point(32, 45)
        Me.label1.Margin = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.label1.Name = "label1"
        Me.label1.Size = New System.Drawing.Size(201, 19)
        Me.label1.TabIndex = 17
        Me.label1.Text = "Noise Reduction Kernel Radius : "
        '
        'groupBox4
        '
        Me.groupBox4.Controls.Add(Me.RadioButton1)
        Me.groupBox4.Controls.Add(Me.RadioButton2)
        Me.groupBox4.Font = New System.Drawing.Font("Segoe UI Variable Display Semib", 11.25!, System.Drawing.FontStyle.Bold)
        Me.groupBox4.Location = New System.Drawing.Point(14, 643)
        Me.groupBox4.Name = "groupBox4"
        Me.groupBox4.Size = New System.Drawing.Size(344, 64)
        Me.groupBox4.TabIndex = 13
        Me.groupBox4.TabStop = False
        Me.groupBox4.Text = "Calibration Method"
        '
        'btnExport
        '
        Me.btnExport.Font = New System.Drawing.Font("Segoe Fluent Icons", 14.75!)
        Me.btnExport.Location = New System.Drawing.Point(485, 782)
        Me.btnExport.Name = "btnExport"
        Me.btnExport.Size = New System.Drawing.Size(234, 40)
        Me.btnExport.TabIndex = 31
        Me.btnExport.Tag = ""
        Me.btnExport.Text = ""
        Me.ToolTip1.SetToolTip(Me.btnExport, "Export")
        Me.btnExport.UseVisualStyleBackColor = True
        '
        'btnInfo
        '
        Me.btnInfo.Font = New System.Drawing.Font("Segoe Fluent Icons", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnInfo.ForeColor = System.Drawing.Color.DarkOliveGreen
        Me.btnInfo.Location = New System.Drawing.Point(689, 12)
        Me.btnInfo.Margin = New System.Windows.Forms.Padding(2, 2, 2, 2)
        Me.btnInfo.Name = "btnInfo"
        Me.btnInfo.Size = New System.Drawing.Size(30, 30)
        Me.btnInfo.TabIndex = 32
        Me.btnInfo.Text = ""
        Me.ToolTip1.SetToolTip(Me.btnInfo, "About")
        Me.btnInfo.UseVisualStyleBackColor = True
        '
        'txtDatasetTitle
        '
        Me.txtDatasetTitle.Font = New System.Drawing.Font("Segoe UI Variable Display Semib", 10.125!, System.Drawing.FontStyle.Bold)
        Me.txtDatasetTitle.Location = New System.Drawing.Point(384, 14)
        Me.txtDatasetTitle.Name = "txtDatasetTitle"
        Me.txtDatasetTitle.Size = New System.Drawing.Size(300, 25)
        Me.txtDatasetTitle.TabIndex = 27
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.rbtnCSV)
        Me.GroupBox1.Controls.Add(Me.rbtnXLSX)
        Me.GroupBox1.Font = New System.Drawing.Font("Segoe UI Variable Display Semib", 11.25!, System.Drawing.FontStyle.Bold)
        Me.GroupBox1.Location = New System.Drawing.Point(14, 709)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(344, 64)
        Me.GroupBox1.TabIndex = 28
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Data Export Options"
        '
        'rbtnCSV
        '
        Me.rbtnCSV.Appearance = System.Windows.Forms.Appearance.Button
        Me.rbtnCSV.Checked = True
        Me.rbtnCSV.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.125!)
        Me.rbtnCSV.Location = New System.Drawing.Point(175, 24)
        Me.rbtnCSV.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.rbtnCSV.Name = "rbtnCSV"
        Me.rbtnCSV.Size = New System.Drawing.Size(150, 30)
        Me.rbtnCSV.TabIndex = 30
        Me.rbtnCSV.TabStop = True
        Me.rbtnCSV.Text = "Save as CSV"
        Me.rbtnCSV.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.rbtnCSV.UseVisualStyleBackColor = True
        '
        'rbtnXLSX
        '
        Me.rbtnXLSX.Appearance = System.Windows.Forms.Appearance.Button
        Me.rbtnXLSX.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.125!)
        Me.rbtnXLSX.Location = New System.Drawing.Point(19, 24)
        Me.rbtnXLSX.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.rbtnXLSX.Name = "rbtnXLSX"
        Me.rbtnXLSX.Size = New System.Drawing.Size(150, 30)
        Me.rbtnXLSX.TabIndex = 29
        Me.rbtnXLSX.Text = "Open in Excel"
        Me.rbtnXLSX.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.rbtnXLSX.UseVisualStyleBackColor = True
        '
        'FrmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(734, 861)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.btnInfo)
        Me.Controls.Add(Me.btnExport)
        Me.Controls.Add(Me.txtDatasetTitle)
        Me.Controls.Add(Me.groupBox4)
        Me.Controls.Add(Me.groupBox5)
        Me.Controls.Add(Me.progressBar1)
        Me.Controls.Add(Me.statusStrip1)
        Me.Controls.Add(Me.GroupBox3)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.addButton)
        Me.Controls.Add(Me.TextBox1)
        Me.Controls.Add(Me.calcButton)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.MaximizeBox = False
        Me.Name = "FrmMain"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Avocado Smoothie"
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.statusStrip1.ResumeLayout(False)
        Me.statusStrip1.PerformLayout()
        Me.groupBox5.ResumeLayout(False)
        Me.groupBox5.PerformLayout()
        Me.groupBox4.ResumeLayout(False)
        Me.GroupBox1.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents ListBox1 As ListBox
    Friend WithEvents ListBox2 As ListBox
    Friend WithEvents calcButton As Button
    Friend WithEvents RadioButton1 As RadioButton
    Friend WithEvents RadioButton2 As RadioButton
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents addButton As Button
    Friend WithEvents copyButton1 As Button
    Friend WithEvents clearButton1 As Button
    Friend WithEvents clearButton2 As Button
    Friend WithEvents deleteButton1 As Button
    Friend WithEvents pasteButton As Button
    Friend WithEvents copyButton2 As Button
    Friend WithEvents selectAllButton1 As Button
    Friend WithEvents selectAllButton2 As Button
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents GroupBox3 As GroupBox
    Friend WithEvents sClrButton1 As Button
    Friend WithEvents sClrButton2 As Button
    Private WithEvents progressBar1 As ProgressBar
    Private WithEvents statusStrip1 As StatusStrip
    Private WithEvents tlblCalibratedType As ToolStripStatusLabel
    Private WithEvents slblSeparator1 As ToolStripStatusLabel
    Public WithEvents lblCnt1 As Label
    Private WithEvents lblCnt2 As Label
    Friend WithEvents cbxBorderCount As ComboBox
    Private WithEvents groupBox5 As GroupBox
    Private WithEvents lblBorderCount As Label
    Private WithEvents cbxKernelRadius As ComboBox
    Private WithEvents label1 As Label
    Private WithEvents groupBox4 As GroupBox
    Private WithEvents editButton As Button
    Private WithEvents slblCalibratedType As ToolStripStatusLabel
    Private WithEvents tlblKernelWidth As ToolStripStatusLabel
    Private WithEvents slblKernelWidth As ToolStripStatusLabel
    Private WithEvents slblSeparator2 As ToolStripStatusLabel
    Private WithEvents tlblBorderCount As ToolStripStatusLabel
    Private WithEvents slblBorderCount As ToolStripStatusLabel
    Friend WithEvents ToolTip1 As ToolTip
    Friend WithEvents txtDatasetTitle As TextBox
    Friend WithEvents btnExport As Button
    Private WithEvents btnInfo As Button
    Private WithEvents GroupBox1 As GroupBox
    Friend WithEvents rbtnCSV As RadioButton
    Friend WithEvents rbtnXLSX As RadioButton
    Private WithEvents syncButton1 As Button
    Private WithEvents syncButton2 As Button
End Class
