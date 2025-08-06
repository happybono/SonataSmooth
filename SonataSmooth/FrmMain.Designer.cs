namespace SonataSmooth
{
    partial class FrmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.rbtnAvg = new System.Windows.Forms.RadioButton();
            this.rbtnMed = new System.Windows.Forms.RadioButton();
            this.btnCalibrate = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.txtVariable = new System.Windows.Forms.TextBox();
            this.rbtnRect = new System.Windows.Forms.RadioButton();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnCopy = new System.Windows.Forms.Button();
            this.btnPaste = new System.Windows.Forms.Button();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.btnSelClear = new System.Windows.Forms.Button();
            this.cbxKernelWidth = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSelClear2 = new System.Windows.Forms.Button();
            this.btnSelectAll2 = new System.Windows.Forms.Button();
            this.btnCopy2 = new System.Windows.Forms.Button();
            this.btnClear2 = new System.Windows.Forms.Button();
            this.lblPolyOrder = new System.Windows.Forms.Label();
            this.cbxPolyOrder = new System.Windows.Forms.ComboBox();
            this.rbtnSG = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnSync1 = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.lblCnt1 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnSync2 = new System.Windows.Forms.Button();
            this.lblCnt2 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.slblCalibratedType = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.slblKernelWidth = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel6 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel5 = new System.Windows.Forms.ToolStripStatusLabel();
            this.slblPolynomialOrder = new System.Windows.Forms.ToolStripStatusLabel();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.rbtnGauss = new System.Windows.Forms.RadioButton();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnExportSettings = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnInfo = new System.Windows.Forms.Button();
            this.txtDatasetTitle = new System.Windows.Forms.TextBox();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.AllowDrop = true;
            this.listBox1.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 10.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 17;
            this.listBox1.Location = new System.Drawing.Point(8, 31);
            this.listBox1.Margin = new System.Windows.Forms.Padding(2);
            this.listBox1.Name = "listBox1";
            this.listBox1.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBox1.Size = new System.Drawing.Size(294, 514);
            this.listBox1.TabIndex = 4;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            this.listBox1.DragDrop += new System.Windows.Forms.DragEventHandler(this.listBox1_DragDrop);
            this.listBox1.DragEnter += new System.Windows.Forms.DragEventHandler(this.listBox1_DragEnter);
            this.listBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBox1_KeyDown);
            // 
            // listBox2
            // 
            this.listBox2.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 10.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBox2.FormattingEnabled = true;
            this.listBox2.ItemHeight = 17;
            this.listBox2.Location = new System.Drawing.Point(8, 31);
            this.listBox2.Margin = new System.Windows.Forms.Padding(2);
            this.listBox2.Name = "listBox2";
            this.listBox2.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBox2.Size = new System.Drawing.Size(294, 514);
            this.listBox2.TabIndex = 24;
            this.listBox2.SelectedIndexChanged += new System.EventHandler(this.listBox2_SelectedIndexChanged);
            this.listBox2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBox2_KeyDown);
            // 
            // rbtnAvg
            // 
            this.rbtnAvg.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbtnAvg.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbtnAvg.Location = new System.Drawing.Point(175, 25);
            this.rbtnAvg.Margin = new System.Windows.Forms.Padding(2);
            this.rbtnAvg.Name = "rbtnAvg";
            this.rbtnAvg.Size = new System.Drawing.Size(163, 30);
            this.rbtnAvg.TabIndex = 15;
            this.rbtnAvg.Text = "Binomial Averaging";
            this.rbtnAvg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbtnAvg.UseVisualStyleBackColor = true;
            // 
            // rbtnMed
            // 
            this.rbtnMed.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbtnMed.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbtnMed.Location = new System.Drawing.Point(7, 59);
            this.rbtnMed.Margin = new System.Windows.Forms.Padding(2);
            this.rbtnMed.Name = "rbtnMed";
            this.rbtnMed.Size = new System.Drawing.Size(331, 30);
            this.rbtnMed.TabIndex = 16;
            this.rbtnMed.Text = "Binomial Median Filtering";
            this.rbtnMed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbtnMed.UseVisualStyleBackColor = true;
            // 
            // btnCalibrate
            // 
            this.btnCalibrate.Font = new System.Drawing.Font("Segoe Fluent Icons", 15.75F);
            this.btnCalibrate.Location = new System.Drawing.Point(14, 782);
            this.btnCalibrate.Margin = new System.Windows.Forms.Padding(2);
            this.btnCalibrate.Name = "btnCalibrate";
            this.btnCalibrate.Size = new System.Drawing.Size(466, 40);
            this.btnCalibrate.TabIndex = 22;
            this.btnCalibrate.Text = "";
            this.toolTip1.SetToolTip(this.btnCalibrate, "Calibrate");
            this.btnCalibrate.UseVisualStyleBackColor = true;
            this.btnCalibrate.Click += new System.EventHandler(this.btnCalibrate_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAdd.Location = new System.Drawing.Point(292, 12);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(2);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(67, 30);
            this.btnAdd.TabIndex = 2;
            this.btnAdd.Text = "";
            this.toolTip1.SetToolTip(this.btnAdd, "Add");
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // txtVariable
            // 
            this.txtVariable.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 10.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtVariable.Location = new System.Drawing.Point(26, 14);
            this.txtVariable.Margin = new System.Windows.Forms.Padding(2);
            this.txtVariable.Name = "txtVariable";
            this.txtVariable.Size = new System.Drawing.Size(262, 25);
            this.txtVariable.TabIndex = 1;
            this.txtVariable.TextChanged += new System.EventHandler(this.txtVariable_TextChanged);
            this.txtVariable.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtVariable_KeyDown);
            // 
            // rbtnRect
            // 
            this.rbtnRect.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbtnRect.Checked = true;
            this.rbtnRect.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbtnRect.Location = new System.Drawing.Point(7, 25);
            this.rbtnRect.Margin = new System.Windows.Forms.Padding(2);
            this.rbtnRect.Name = "rbtnRect";
            this.rbtnRect.Size = new System.Drawing.Size(163, 30);
            this.rbtnRect.TabIndex = 14;
            this.rbtnRect.TabStop = true;
            this.rbtnRect.Text = "Rectangular Averaging";
            this.rbtnRect.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbtnRect.UseVisualStyleBackColor = true;
            // 
            // btnClear
            // 
            this.btnClear.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClear.Location = new System.Drawing.Point(306, 31);
            this.btnClear.Margin = new System.Windows.Forms.Padding(2);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(30, 30);
            this.btnClear.TabIndex = 5;
            this.btnClear.Text = "";
            this.toolTip1.SetToolTip(this.btnClear, "Clear");
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDelete.Location = new System.Drawing.Point(306, 167);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(2);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(30, 30);
            this.btnDelete.TabIndex = 9;
            this.btnDelete.Text = "";
            this.toolTip1.SetToolTip(this.btnDelete, "Delete");
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCopy.Location = new System.Drawing.Point(306, 65);
            this.btnCopy.Margin = new System.Windows.Forms.Padding(2);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(30, 30);
            this.btnCopy.TabIndex = 6;
            this.btnCopy.Text = "";
            this.toolTip1.SetToolTip(this.btnCopy, "Copy");
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnPaste
            // 
            this.btnPaste.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPaste.Location = new System.Drawing.Point(306, 99);
            this.btnPaste.Margin = new System.Windows.Forms.Padding(2);
            this.btnPaste.Name = "btnPaste";
            this.btnPaste.Size = new System.Drawing.Size(30, 30);
            this.btnPaste.TabIndex = 7;
            this.btnPaste.Text = "";
            this.toolTip1.SetToolTip(this.btnPaste, "Paste");
            this.btnPaste.UseVisualStyleBackColor = true;
            this.btnPaste.Click += new System.EventHandler(this.btnPaste_Click);
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelectAll.Location = new System.Drawing.Point(306, 201);
            this.btnSelectAll.Margin = new System.Windows.Forms.Padding(2);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(30, 30);
            this.btnSelectAll.TabIndex = 10;
            this.btnSelectAll.Text = "";
            this.toolTip1.SetToolTip(this.btnSelectAll, "Select All");
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // btnSelClear
            // 
            this.btnSelClear.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelClear.Location = new System.Drawing.Point(306, 235);
            this.btnSelClear.Margin = new System.Windows.Forms.Padding(2);
            this.btnSelClear.Name = "btnSelClear";
            this.btnSelClear.Size = new System.Drawing.Size(30, 30);
            this.btnSelClear.TabIndex = 11;
            this.btnSelClear.Text = "";
            this.toolTip1.SetToolTip(this.btnSelClear, "Deselect All");
            this.btnSelClear.UseVisualStyleBackColor = true;
            this.btnSelClear.Click += new System.EventHandler(this.btnSelClear_Click);
            // 
            // cbxKernelWidth
            // 
            this.cbxKernelWidth.DropDownHeight = 150;
            this.cbxKernelWidth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxKernelWidth.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 10.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbxKernelWidth.FormattingEnabled = true;
            this.cbxKernelWidth.IntegralHeight = false;
            this.cbxKernelWidth.ItemHeight = 17;
            this.cbxKernelWidth.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13"});
            this.cbxKernelWidth.Location = new System.Drawing.Point(232, 42);
            this.cbxKernelWidth.Margin = new System.Windows.Forms.Padding(2);
            this.cbxKernelWidth.Name = "cbxKernelWidth";
            this.cbxKernelWidth.Size = new System.Drawing.Size(80, 25);
            this.cbxKernelWidth.TabIndex = 20;
            this.cbxKernelWidth.SelectedIndexChanged += new System.EventHandler(this.cbxKernelWidth_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(32, 45);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(198, 19);
            this.label1.TabIndex = 17;
            this.label1.Text = "Noise Reduction Kernel Width : ";
            // 
            // btnSelClear2
            // 
            this.btnSelClear2.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelClear2.Location = new System.Drawing.Point(306, 133);
            this.btnSelClear2.Margin = new System.Windows.Forms.Padding(2);
            this.btnSelClear2.Name = "btnSelClear2";
            this.btnSelClear2.Size = new System.Drawing.Size(30, 30);
            this.btnSelClear2.TabIndex = 28;
            this.btnSelClear2.Text = "";
            this.toolTip1.SetToolTip(this.btnSelClear2, "Deselect All");
            this.btnSelClear2.UseVisualStyleBackColor = true;
            this.btnSelClear2.Click += new System.EventHandler(this.btnSelectClear2_Click);
            // 
            // btnSelectAll2
            // 
            this.btnSelectAll2.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelectAll2.Location = new System.Drawing.Point(306, 99);
            this.btnSelectAll2.Margin = new System.Windows.Forms.Padding(2);
            this.btnSelectAll2.Name = "btnSelectAll2";
            this.btnSelectAll2.Size = new System.Drawing.Size(30, 30);
            this.btnSelectAll2.TabIndex = 27;
            this.btnSelectAll2.Text = "";
            this.toolTip1.SetToolTip(this.btnSelectAll2, "Select All");
            this.btnSelectAll2.UseVisualStyleBackColor = true;
            this.btnSelectAll2.Click += new System.EventHandler(this.btnSelectAll2_Click);
            // 
            // btnCopy2
            // 
            this.btnCopy2.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCopy2.Location = new System.Drawing.Point(306, 65);
            this.btnCopy2.Margin = new System.Windows.Forms.Padding(2);
            this.btnCopy2.Name = "btnCopy2";
            this.btnCopy2.Size = new System.Drawing.Size(30, 30);
            this.btnCopy2.TabIndex = 26;
            this.btnCopy2.Text = "";
            this.toolTip1.SetToolTip(this.btnCopy2, "Copy");
            this.btnCopy2.UseVisualStyleBackColor = true;
            this.btnCopy2.Click += new System.EventHandler(this.btnCopy2_Click);
            // 
            // btnClear2
            // 
            this.btnClear2.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClear2.Location = new System.Drawing.Point(306, 31);
            this.btnClear2.Margin = new System.Windows.Forms.Padding(2);
            this.btnClear2.Name = "btnClear2";
            this.btnClear2.Size = new System.Drawing.Size(30, 30);
            this.btnClear2.TabIndex = 25;
            this.btnClear2.Text = "";
            this.toolTip1.SetToolTip(this.btnClear2, "Clear");
            this.btnClear2.UseVisualStyleBackColor = true;
            this.btnClear2.Click += new System.EventHandler(this.btnClear2_Click);
            // 
            // lblPolyOrder
            // 
            this.lblPolyOrder.AutoSize = true;
            this.lblPolyOrder.Enabled = false;
            this.lblPolyOrder.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.lblPolyOrder.Location = new System.Drawing.Point(72, 83);
            this.lblPolyOrder.Name = "lblPolyOrder";
            this.lblPolyOrder.Size = new System.Drawing.Size(119, 19);
            this.lblPolyOrder.TabIndex = 20;
            this.lblPolyOrder.Text = "Polynomial Order :";
            // 
            // cbxPolyOrder
            // 
            this.cbxPolyOrder.DropDownHeight = 150;
            this.cbxPolyOrder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxPolyOrder.Enabled = false;
            this.cbxPolyOrder.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 10.125F, System.Drawing.FontStyle.Bold);
            this.cbxPolyOrder.FormattingEnabled = true;
            this.cbxPolyOrder.IntegralHeight = false;
            this.cbxPolyOrder.ItemHeight = 17;
            this.cbxPolyOrder.Items.AddRange(new object[] {
            "2",
            "3",
            "4",
            "5",
            "6",
            "7"});
            this.cbxPolyOrder.Location = new System.Drawing.Point(193, 80);
            this.cbxPolyOrder.Name = "cbxPolyOrder";
            this.cbxPolyOrder.Size = new System.Drawing.Size(80, 25);
            this.cbxPolyOrder.TabIndex = 21;
            this.cbxPolyOrder.SelectedIndexChanged += new System.EventHandler(this.cbxPolyOrder_SelectedIndexChanged);
            // 
            // rbtnSG
            // 
            this.rbtnSG.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbtnSG.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbtnSG.Location = new System.Drawing.Point(175, 93);
            this.rbtnSG.Margin = new System.Windows.Forms.Padding(2);
            this.rbtnSG.Name = "rbtnSG";
            this.rbtnSG.Size = new System.Drawing.Size(163, 30);
            this.rbtnSG.TabIndex = 18;
            this.rbtnSG.Text = "Savitzky-Golay Filtering";
            this.rbtnSG.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbtnSG.UseVisualStyleBackColor = true;
            this.rbtnSG.CheckedChanged += new System.EventHandler(this.rbtnSG_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnSync1);
            this.groupBox2.Controls.Add(this.btnEdit);
            this.groupBox2.Controls.Add(this.listBox1);
            this.groupBox2.Controls.Add(this.lblCnt1);
            this.groupBox2.Controls.Add(this.btnClear);
            this.groupBox2.Controls.Add(this.btnDelete);
            this.groupBox2.Controls.Add(this.btnCopy);
            this.groupBox2.Controls.Add(this.btnPaste);
            this.groupBox2.Controls.Add(this.btnSelClear);
            this.groupBox2.Controls.Add(this.btnSelectAll);
            this.groupBox2.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(15, 52);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(344, 586);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Initial Dataset";
            // 
            // btnSync1
            // 
            this.btnSync1.Font = new System.Drawing.Font("Segoe Fluent Icons", 11.25F);
            this.btnSync1.Location = new System.Drawing.Point(306, 269);
            this.btnSync1.Name = "btnSync1";
            this.btnSync1.Size = new System.Drawing.Size(30, 30);
            this.btnSync1.TabIndex = 12;
            this.btnSync1.Text = "";
            this.toolTip1.SetToolTip(this.btnSync1, "Match Selection\r\n( ▶ Refined Dataset )");
            this.btnSync1.UseVisualStyleBackColor = true;
            this.btnSync1.Click += new System.EventHandler(this.btnSync1_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.Enabled = false;
            this.btnEdit.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEdit.Location = new System.Drawing.Point(306, 133);
            this.btnEdit.Margin = new System.Windows.Forms.Padding(2);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(30, 30);
            this.btnEdit.TabIndex = 8;
            this.btnEdit.Text = "";
            this.toolTip1.SetToolTip(this.btnEdit, "Edit");
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // lblCnt1
            // 
            this.lblCnt1.AutoSize = true;
            this.lblCnt1.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 10.125F, System.Drawing.FontStyle.Bold);
            this.lblCnt1.Location = new System.Drawing.Point(7, 555);
            this.lblCnt1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCnt1.Name = "lblCnt1";
            this.lblCnt1.Size = new System.Drawing.Size(65, 19);
            this.lblCnt1.TabIndex = 7;
            this.lblCnt1.Text = "Count : 0";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(0, 832);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(734, 5);
            this.progressBar1.TabIndex = 16;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnSync2);
            this.groupBox3.Controls.Add(this.listBox2);
            this.groupBox3.Controls.Add(this.lblCnt2);
            this.groupBox3.Controls.Add(this.btnClear2);
            this.groupBox3.Controls.Add(this.btnSelClear2);
            this.groupBox3.Controls.Add(this.btnCopy2);
            this.groupBox3.Controls.Add(this.btnSelectAll2);
            this.groupBox3.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(376, 52);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(344, 586);
            this.groupBox3.TabIndex = 23;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Refined Dataset";
            // 
            // btnSync2
            // 
            this.btnSync2.Font = new System.Drawing.Font("Segoe Fluent Icons", 11.25F);
            this.btnSync2.Location = new System.Drawing.Point(306, 167);
            this.btnSync2.Name = "btnSync2";
            this.btnSync2.Size = new System.Drawing.Size(30, 30);
            this.btnSync2.TabIndex = 29;
            this.btnSync2.Text = "";
            this.toolTip1.SetToolTip(this.btnSync2, "Match Selection \r\n( ◀ Initial Dataset )");
            this.btnSync2.UseVisualStyleBackColor = true;
            this.btnSync2.Click += new System.EventHandler(this.btnSync2_Click);
            // 
            // lblCnt2
            // 
            this.lblCnt2.AutoSize = true;
            this.lblCnt2.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 10.125F, System.Drawing.FontStyle.Bold);
            this.lblCnt2.Location = new System.Drawing.Point(7, 555);
            this.lblCnt2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCnt2.Name = "lblCnt2";
            this.lblCnt2.Size = new System.Drawing.Size(65, 19);
            this.lblCnt2.TabIndex = 8;
            this.lblCnt2.Text = "Count : 0";
            // 
            // statusStrip1
            // 
            this.statusStrip1.AutoSize = false;
            this.statusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(223)))));
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.slblCalibratedType,
            this.toolStripStatusLabel2,
            this.toolStripStatusLabel3,
            this.slblKernelWidth,
            this.toolStripStatusLabel6,
            this.toolStripStatusLabel5,
            this.slblPolynomialOrder});
            this.statusStrip1.Location = new System.Drawing.Point(0, 837);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(734, 24);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 27;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Font = new System.Drawing.Font("Segoe UI Variable Display", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripStatusLabel1.ForeColor = System.Drawing.Color.White;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(114, 19);
            this.toolStripStatusLabel1.Text = "Applied Calibration :";
            // 
            // slblCalibratedType
            // 
            this.slblCalibratedType.Font = new System.Drawing.Font("Segoe UI Variable Display", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.slblCalibratedType.ForeColor = System.Drawing.Color.White;
            this.slblCalibratedType.Name = "slblCalibratedType";
            this.slblCalibratedType.Size = new System.Drawing.Size(17, 19);
            this.slblCalibratedType.Text = "--";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Font = new System.Drawing.Font("Segoe UI Variable Display", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripStatusLabel2.ForeColor = System.Drawing.Color.White;
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(19, 19);
            this.toolStripStatusLabel2.Text = "｜";
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Font = new System.Drawing.Font("Segoe UI Variable Display", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripStatusLabel3.ForeColor = System.Drawing.Color.White;
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(173, 19);
            this.toolStripStatusLabel3.Text = "Noise Reduction Kernel Width : ";
            // 
            // slblKernelWidth
            // 
            this.slblKernelWidth.Font = new System.Drawing.Font("Segoe UI Variable Display", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.slblKernelWidth.ForeColor = System.Drawing.Color.White;
            this.slblKernelWidth.Name = "slblKernelWidth";
            this.slblKernelWidth.Size = new System.Drawing.Size(17, 19);
            this.slblKernelWidth.Text = "--";
            // 
            // toolStripStatusLabel6
            // 
            this.toolStripStatusLabel6.Font = new System.Drawing.Font("Segoe UI Variable Display", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripStatusLabel6.ForeColor = System.Drawing.Color.White;
            this.toolStripStatusLabel6.Name = "toolStripStatusLabel6";
            this.toolStripStatusLabel6.Size = new System.Drawing.Size(19, 19);
            this.toolStripStatusLabel6.Text = "｜";
            this.toolStripStatusLabel6.Visible = false;
            // 
            // toolStripStatusLabel5
            // 
            this.toolStripStatusLabel5.Font = new System.Drawing.Font("Segoe UI Variable Display", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripStatusLabel5.ForeColor = System.Drawing.Color.White;
            this.toolStripStatusLabel5.Name = "toolStripStatusLabel5";
            this.toolStripStatusLabel5.Size = new System.Drawing.Size(107, 19);
            this.toolStripStatusLabel5.Text = "Polynomial Order : ";
            this.toolStripStatusLabel5.Visible = false;
            // 
            // slblPolynomialOrder
            // 
            this.slblPolynomialOrder.Font = new System.Drawing.Font("Segoe UI Variable Display", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.slblPolynomialOrder.ForeColor = System.Drawing.Color.White;
            this.slblPolynomialOrder.Name = "slblPolynomialOrder";
            this.slblPolynomialOrder.Size = new System.Drawing.Size(17, 19);
            this.slblPolynomialOrder.Text = "--";
            this.slblPolynomialOrder.Visible = false;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.rbtnGauss);
            this.groupBox4.Controls.Add(this.rbtnSG);
            this.groupBox4.Controls.Add(this.rbtnRect);
            this.groupBox4.Controls.Add(this.rbtnAvg);
            this.groupBox4.Controls.Add(this.rbtnMed);
            this.groupBox4.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 11.25F, System.Drawing.FontStyle.Bold);
            this.groupBox4.Location = new System.Drawing.Point(15, 644);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(344, 130);
            this.groupBox4.TabIndex = 13;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Calibration Method";
            // 
            // rbtnGauss
            // 
            this.rbtnGauss.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbtnGauss.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbtnGauss.Location = new System.Drawing.Point(7, 93);
            this.rbtnGauss.Margin = new System.Windows.Forms.Padding(2);
            this.rbtnGauss.Name = "rbtnGauss";
            this.rbtnGauss.Size = new System.Drawing.Size(163, 30);
            this.rbtnGauss.TabIndex = 17;
            this.rbtnGauss.Text = "Gaussian Filtering";
            this.rbtnGauss.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbtnGauss.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.lblPolyOrder);
            this.groupBox5.Controls.Add(this.cbxPolyOrder);
            this.groupBox5.Controls.Add(this.cbxKernelWidth);
            this.groupBox5.Controls.Add(this.label1);
            this.groupBox5.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox5.Location = new System.Drawing.Point(376, 644);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(344, 130);
            this.groupBox5.TabIndex = 19;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Signal Smoothing Parameters";
            // 
            // btnExportSettings
            // 
            this.btnExportSettings.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExportSettings.Location = new System.Drawing.Point(655, 12);
            this.btnExportSettings.Margin = new System.Windows.Forms.Padding(2);
            this.btnExportSettings.Name = "btnExportSettings";
            this.btnExportSettings.Size = new System.Drawing.Size(30, 30);
            this.btnExportSettings.TabIndex = 31;
            this.btnExportSettings.Text = "";
            this.toolTip1.SetToolTip(this.btnExportSettings, "Export Settings");
            this.btnExportSettings.UseVisualStyleBackColor = true;
            this.btnExportSettings.Click += new System.EventHandler(this.btnExportSettings_Click);
            // 
            // btnExport
            // 
            this.btnExport.Font = new System.Drawing.Font("Segoe Fluent Icons", 14.75F);
            this.btnExport.Location = new System.Drawing.Point(486, 782);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(234, 40);
            this.btnExport.TabIndex = 32;
            this.btnExport.Text = "";
            this.toolTip1.SetToolTip(this.btnExport, "Export");
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnInfo
            // 
            this.btnInfo.Font = new System.Drawing.Font("Segoe Fluent Icons", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInfo.ForeColor = System.Drawing.Color.MediumSlateBlue;
            this.btnInfo.Location = new System.Drawing.Point(689, 12);
            this.btnInfo.Margin = new System.Windows.Forms.Padding(2);
            this.btnInfo.Name = "btnInfo";
            this.btnInfo.Size = new System.Drawing.Size(30, 30);
            this.btnInfo.TabIndex = 33;
            this.btnInfo.Text = "";
            this.toolTip1.SetToolTip(this.btnInfo, "About");
            this.btnInfo.UseVisualStyleBackColor = true;
            this.btnInfo.Click += new System.EventHandler(this.btnInfo_Click);
            // 
            // txtDatasetTitle
            // 
            this.txtDatasetTitle.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 10.125F, System.Drawing.FontStyle.Bold);
            this.txtDatasetTitle.Location = new System.Drawing.Point(385, 14);
            this.txtDatasetTitle.Name = "txtDatasetTitle";
            this.txtDatasetTitle.Size = new System.Drawing.Size(265, 25);
            this.txtDatasetTitle.TabIndex = 30;
            this.txtDatasetTitle.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtDatasetTitle.TextChanged += new System.EventHandler(this.txtExcelTitle_TextChanged);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(734, 861);
            this.Controls.Add(this.btnInfo);
            this.Controls.Add(this.btnExportSettings);
            this.Controls.Add(this.txtDatasetTitle);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.txtVariable);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnCalibrate);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SonataSmooth";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.RadioButton rbtnAvg;
        private System.Windows.Forms.RadioButton rbtnMed;
        private System.Windows.Forms.Button btnCalibrate;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.TextBox txtVariable;
        private System.Windows.Forms.RadioButton rbtnRect;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Button btnPaste;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Button btnSelClear;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSelClear2;
        private System.Windows.Forms.Button btnSelectAll2;
        private System.Windows.Forms.Button btnCopy2;
        private System.Windows.Forms.Button btnClear2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.RadioButton rbtnSG;
        private System.Windows.Forms.Label lblPolyOrder;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel5;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel6;
        private System.Windows.Forms.Label lblCnt2;
        private System.Windows.Forms.ToolStripStatusLabel slblCalibratedType;
        private System.Windows.Forms.ToolStripStatusLabel slblKernelWidth;
        private System.Windows.Forms.ToolStripStatusLabel slblPolynomialOrder;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.RadioButton rbtnGauss;
        private System.Windows.Forms.ToolTip toolTip1;
        public System.Windows.Forms.ListBox listBox1;
        public System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.Button btnEdit;
        public System.Windows.Forms.Label lblCnt1;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.TextBox txtDatasetTitle;
        private System.Windows.Forms.Button btnExportSettings;
        public System.Windows.Forms.ComboBox cbxKernelWidth;
        private System.Windows.Forms.ComboBox cbxPolyOrder;
        private System.Windows.Forms.Button btnInfo;
        private System.Windows.Forms.Button btnSync1;
        private System.Windows.Forms.Button btnSync2;
    }
}

