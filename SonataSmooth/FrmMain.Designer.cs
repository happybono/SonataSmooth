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
            this.lbInitData = new System.Windows.Forms.ListBox();
            this.lbRefinedData = new System.Windows.Forms.ListBox();
            this.rbtnAvg = new System.Windows.Forms.RadioButton();
            this.rbtnMed = new System.Windows.Forms.RadioButton();
            this.btnCalibrate = new System.Windows.Forms.Button();
            this.btnInitAdd = new System.Windows.Forms.Button();
            this.txtInitAdd = new System.Windows.Forms.TextBox();
            this.rbtnRect = new System.Windows.Forms.RadioButton();
            this.btnInitClear = new System.Windows.Forms.Button();
            this.btnInitDelete = new System.Windows.Forms.Button();
            this.btnInitCopy = new System.Windows.Forms.Button();
            this.btnInitPaste = new System.Windows.Forms.Button();
            this.btnInitSelectAll = new System.Windows.Forms.Button();
            this.btnInitSelectClr = new System.Windows.Forms.Button();
            this.cbxKernelRadius = new System.Windows.Forms.ComboBox();
            this.lblKernelRadius = new System.Windows.Forms.Label();
            this.btnRefSelectClr = new System.Windows.Forms.Button();
            this.btnRefSelectAll = new System.Windows.Forms.Button();
            this.btnRefCopy = new System.Windows.Forms.Button();
            this.btnRefClear = new System.Windows.Forms.Button();
            this.lblPolyOrder = new System.Windows.Forms.Label();
            this.cbxPolyOrder = new System.Windows.Forms.ComboBox();
            this.rbtnSG = new System.Windows.Forms.RadioButton();
            this.gbInitData = new System.Windows.Forms.GroupBox();
            this.btnInitSelectSync = new System.Windows.Forms.Button();
            this.btnInitEdit = new System.Windows.Forms.Button();
            this.lblInitCnt = new System.Windows.Forms.Label();
            this.pbMain = new System.Windows.Forms.ProgressBar();
            this.gbRefinedData = new System.Windows.Forms.GroupBox();
            this.btnRefSelectSync = new System.Windows.Forms.Button();
            this.lblRefCnt = new System.Windows.Forms.Label();
            this.gbSmoothMtd = new System.Windows.Forms.GroupBox();
            this.rbtnGauss = new System.Windows.Forms.RadioButton();
            this.gbSmoothParams = new System.Windows.Forms.GroupBox();
            this.cbxBoundaryMethod = new System.Windows.Forms.ComboBox();
            this.lblBoundaryMethod = new System.Windows.Forms.Label();
            this.ttipMain = new System.Windows.Forms.ToolTip(this.components);
            this.btnExportSettings = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnInfo = new System.Windows.Forms.Button();
            this.txtDatasetTitle = new System.Windows.Forms.TextBox();
            this.slblDesc = new System.Windows.Forms.ToolStripStatusLabel();
            this.tlblCalibratedType = new System.Windows.Forms.ToolStripStatusLabel();
            this.slblCalibratedType = new System.Windows.Forms.ToolStripStatusLabel();
            this.tlblSeparator1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tlblKernelRadius = new System.Windows.Forms.ToolStripStatusLabel();
            this.slblKernelRadius = new System.Windows.Forms.ToolStripStatusLabel();
            this.tlblSeparator2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tlblPolyOrder = new System.Windows.Forms.ToolStripStatusLabel();
            this.slblPolyOrder = new System.Windows.Forms.ToolStripStatusLabel();
            this.tlblSeparator3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tlblBoundaryMethod = new System.Windows.Forms.ToolStripStatusLabel();
            this.statStripMain = new System.Windows.Forms.StatusStrip();
            this.slblBoundaryMethod = new System.Windows.Forms.ToolStripStatusLabel();
            this.gbInitData.SuspendLayout();
            this.gbRefinedData.SuspendLayout();
            this.gbSmoothMtd.SuspendLayout();
            this.gbSmoothParams.SuspendLayout();
            this.statStripMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbInitData
            // 
            this.lbInitData.AllowDrop = true;
            this.lbInitData.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 10.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbInitData.FormattingEnabled = true;
            this.lbInitData.ItemHeight = 17;
            this.lbInitData.Location = new System.Drawing.Point(8, 31);
            this.lbInitData.Margin = new System.Windows.Forms.Padding(2);
            this.lbInitData.Name = "lbInitData";
            this.lbInitData.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbInitData.Size = new System.Drawing.Size(294, 514);
            this.lbInitData.TabIndex = 4;
            this.lbInitData.SelectedIndexChanged += new System.EventHandler(this.lbInitData_SelectedIndexChanged);
            this.lbInitData.DragDrop += new System.Windows.Forms.DragEventHandler(this.lbInitData_DragDrop);
            this.lbInitData.DragEnter += new System.Windows.Forms.DragEventHandler(this.lbInitData_DragEnter);
            this.lbInitData.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lbInitData_KeyDown);
            // 
            // lbRefinedData
            // 
            this.lbRefinedData.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 10.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbRefinedData.FormattingEnabled = true;
            this.lbRefinedData.ItemHeight = 17;
            this.lbRefinedData.Location = new System.Drawing.Point(8, 31);
            this.lbRefinedData.Margin = new System.Windows.Forms.Padding(2);
            this.lbRefinedData.Name = "lbRefinedData";
            this.lbRefinedData.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbRefinedData.Size = new System.Drawing.Size(294, 514);
            this.lbRefinedData.TabIndex = 24;
            this.lbRefinedData.SelectedIndexChanged += new System.EventHandler(this.lbRefinedData_SelectedIndexChanged);
            this.lbRefinedData.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lbRefinedData_KeyDown);
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
            this.rbtnAvg.CheckedChanged += new System.EventHandler(this.rbtnAvg_CheckedChanged);
            this.rbtnAvg.MouseLeave += new System.EventHandler(this.rbtnAvg_MouseLeave);
            this.rbtnAvg.MouseHover += new System.EventHandler(this.rbtnAvg_MouseHover);
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
            this.rbtnMed.CheckedChanged += new System.EventHandler(this.rbtnMed_CheckedChanged);
            this.rbtnMed.MouseLeave += new System.EventHandler(this.rbtnMed_MouseLeave);
            this.rbtnMed.MouseHover += new System.EventHandler(this.rbtnMed_MouseHover);
            // 
            // btnCalibrate
            // 
            this.btnCalibrate.Font = new System.Drawing.Font("Segoe Fluent Icons", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCalibrate.Location = new System.Drawing.Point(14, 783);
            this.btnCalibrate.Margin = new System.Windows.Forms.Padding(2);
            this.btnCalibrate.Name = "btnCalibrate";
            this.btnCalibrate.Size = new System.Drawing.Size(466, 30);
            this.btnCalibrate.TabIndex = 22;
            this.btnCalibrate.Text = "";
            this.ttipMain.SetToolTip(this.btnCalibrate, "Calibrate");
            this.btnCalibrate.UseVisualStyleBackColor = true;
            this.btnCalibrate.Click += new System.EventHandler(this.btnCalibrate_Click);
            this.btnCalibrate.MouseLeave += new System.EventHandler(this.btnCalibrate_MouseLeave);
            this.btnCalibrate.MouseHover += new System.EventHandler(this.btnCalibrate_MouseHover);
            // 
            // btnInitAdd
            // 
            this.btnInitAdd.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInitAdd.Location = new System.Drawing.Point(292, 14);
            this.btnInitAdd.Margin = new System.Windows.Forms.Padding(2);
            this.btnInitAdd.Name = "btnInitAdd";
            this.btnInitAdd.Size = new System.Drawing.Size(67, 30);
            this.btnInitAdd.TabIndex = 2;
            this.btnInitAdd.Text = "";
            this.ttipMain.SetToolTip(this.btnInitAdd, "Add");
            this.btnInitAdd.UseVisualStyleBackColor = true;
            this.btnInitAdd.Click += new System.EventHandler(this.btnInitAdd_Click);
            this.btnInitAdd.MouseLeave += new System.EventHandler(this.btnInitAdd_MouseLeave);
            this.btnInitAdd.MouseHover += new System.EventHandler(this.btnInitAdd_MouseHover);
            // 
            // txtInitAdd
            // 
            this.txtInitAdd.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 10.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtInitAdd.Location = new System.Drawing.Point(26, 16);
            this.txtInitAdd.Margin = new System.Windows.Forms.Padding(2);
            this.txtInitAdd.Name = "txtInitAdd";
            this.txtInitAdd.Size = new System.Drawing.Size(262, 25);
            this.txtInitAdd.TabIndex = 1;
            this.txtInitAdd.TextChanged += new System.EventHandler(this.txtInitAdd_TextChanged);
            this.txtInitAdd.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtInitAdd_KeyDown);
            this.txtInitAdd.MouseLeave += new System.EventHandler(this.txtInitAdd_MouseLeave);
            this.txtInitAdd.MouseHover += new System.EventHandler(this.txtInitAdd_MouseHover);
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
            this.rbtnRect.CheckedChanged += new System.EventHandler(this.rbtnRect_CheckedChanged);
            this.rbtnRect.MouseLeave += new System.EventHandler(this.rbtnRect_MouseLeave);
            this.rbtnRect.MouseHover += new System.EventHandler(this.rbtnRect_MouseHover);
            // 
            // btnInitClear
            // 
            this.btnInitClear.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInitClear.Location = new System.Drawing.Point(306, 31);
            this.btnInitClear.Margin = new System.Windows.Forms.Padding(2);
            this.btnInitClear.Name = "btnInitClear";
            this.btnInitClear.Size = new System.Drawing.Size(30, 30);
            this.btnInitClear.TabIndex = 5;
            this.btnInitClear.Text = "";
            this.ttipMain.SetToolTip(this.btnInitClear, "Clear");
            this.btnInitClear.UseVisualStyleBackColor = true;
            this.btnInitClear.Click += new System.EventHandler(this.btnInitClear_Click);
            this.btnInitClear.MouseLeave += new System.EventHandler(this.btnInitClear_MouseLeave);
            this.btnInitClear.MouseHover += new System.EventHandler(this.btnInitClear_MouseHover);
            // 
            // btnInitDelete
            // 
            this.btnInitDelete.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInitDelete.Location = new System.Drawing.Point(306, 167);
            this.btnInitDelete.Margin = new System.Windows.Forms.Padding(2);
            this.btnInitDelete.Name = "btnInitDelete";
            this.btnInitDelete.Size = new System.Drawing.Size(30, 30);
            this.btnInitDelete.TabIndex = 9;
            this.btnInitDelete.Text = "";
            this.ttipMain.SetToolTip(this.btnInitDelete, "Delete");
            this.btnInitDelete.UseVisualStyleBackColor = true;
            this.btnInitDelete.Click += new System.EventHandler(this.btnInitDelete_Click);
            this.btnInitDelete.MouseLeave += new System.EventHandler(this.btnInitDelete_MouseLeave);
            this.btnInitDelete.MouseHover += new System.EventHandler(this.btnInitDelete_MouseHover);
            // 
            // btnInitCopy
            // 
            this.btnInitCopy.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInitCopy.Location = new System.Drawing.Point(306, 65);
            this.btnInitCopy.Margin = new System.Windows.Forms.Padding(2);
            this.btnInitCopy.Name = "btnInitCopy";
            this.btnInitCopy.Size = new System.Drawing.Size(30, 30);
            this.btnInitCopy.TabIndex = 6;
            this.btnInitCopy.Text = "";
            this.ttipMain.SetToolTip(this.btnInitCopy, "Copy");
            this.btnInitCopy.UseVisualStyleBackColor = true;
            this.btnInitCopy.Click += new System.EventHandler(this.btnInitCopy_Click);
            this.btnInitCopy.MouseLeave += new System.EventHandler(this.btnInitCopy_MouseLeave);
            this.btnInitCopy.MouseHover += new System.EventHandler(this.btnInitCopy_MouseHover);
            // 
            // btnInitPaste
            // 
            this.btnInitPaste.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInitPaste.Location = new System.Drawing.Point(306, 99);
            this.btnInitPaste.Margin = new System.Windows.Forms.Padding(2);
            this.btnInitPaste.Name = "btnInitPaste";
            this.btnInitPaste.Size = new System.Drawing.Size(30, 30);
            this.btnInitPaste.TabIndex = 7;
            this.btnInitPaste.Text = "";
            this.ttipMain.SetToolTip(this.btnInitPaste, "Paste");
            this.btnInitPaste.UseVisualStyleBackColor = true;
            this.btnInitPaste.Click += new System.EventHandler(this.btnInitPaste_Click);
            this.btnInitPaste.MouseLeave += new System.EventHandler(this.btnInitPaste_MouseLeave);
            this.btnInitPaste.MouseHover += new System.EventHandler(this.btnInitPaste_MouseHover);
            // 
            // btnInitSelectAll
            // 
            this.btnInitSelectAll.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInitSelectAll.Location = new System.Drawing.Point(306, 201);
            this.btnInitSelectAll.Margin = new System.Windows.Forms.Padding(2);
            this.btnInitSelectAll.Name = "btnInitSelectAll";
            this.btnInitSelectAll.Size = new System.Drawing.Size(30, 30);
            this.btnInitSelectAll.TabIndex = 10;
            this.btnInitSelectAll.Text = "";
            this.ttipMain.SetToolTip(this.btnInitSelectAll, "Select All");
            this.btnInitSelectAll.UseVisualStyleBackColor = true;
            this.btnInitSelectAll.Click += new System.EventHandler(this.btnInitSelectAll_Click);
            this.btnInitSelectAll.MouseLeave += new System.EventHandler(this.btnInitSelectAll_MouseLeave);
            this.btnInitSelectAll.MouseHover += new System.EventHandler(this.btnInitSelectAll_MouseHover);
            // 
            // btnInitSelectClr
            // 
            this.btnInitSelectClr.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInitSelectClr.Location = new System.Drawing.Point(306, 235);
            this.btnInitSelectClr.Margin = new System.Windows.Forms.Padding(2);
            this.btnInitSelectClr.Name = "btnInitSelectClr";
            this.btnInitSelectClr.Size = new System.Drawing.Size(30, 30);
            this.btnInitSelectClr.TabIndex = 11;
            this.btnInitSelectClr.Text = "";
            this.ttipMain.SetToolTip(this.btnInitSelectClr, "Deselect All");
            this.btnInitSelectClr.UseVisualStyleBackColor = true;
            this.btnInitSelectClr.Click += new System.EventHandler(this.btnInitSelectClr_Click);
            this.btnInitSelectClr.MouseHover += new System.EventHandler(this.btnInitSelectClr_MouseHover);
            // 
            // cbxKernelRadius
            // 
            this.cbxKernelRadius.DropDownHeight = 150;
            this.cbxKernelRadius.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxKernelRadius.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 10.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbxKernelRadius.ForeColor = System.Drawing.Color.Black;
            this.cbxKernelRadius.FormattingEnabled = true;
            this.cbxKernelRadius.IntegralHeight = false;
            this.cbxKernelRadius.ItemHeight = 17;
            this.cbxKernelRadius.Items.AddRange(new object[] {
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
            this.cbxKernelRadius.Location = new System.Drawing.Point(223, 29);
            this.cbxKernelRadius.Margin = new System.Windows.Forms.Padding(2);
            this.cbxKernelRadius.Name = "cbxKernelRadius";
            this.cbxKernelRadius.Size = new System.Drawing.Size(103, 25);
            this.cbxKernelRadius.TabIndex = 20;
            this.cbxKernelRadius.SelectedIndexChanged += new System.EventHandler(this.cbxKernelRadius_SelectedIndexChanged);
            this.cbxKernelRadius.MouseLeave += new System.EventHandler(this.cbxKernelRadius_MouseLeave);
            this.cbxKernelRadius.MouseHover += new System.EventHandler(this.cbxKernelRadius_MouseHover);
            // 
            // lblKernelRadius
            // 
            this.lblKernelRadius.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblKernelRadius.Location = new System.Drawing.Point(18, 31);
            this.lblKernelRadius.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblKernelRadius.Name = "lblKernelRadius";
            this.lblKernelRadius.Size = new System.Drawing.Size(183, 19);
            this.lblKernelRadius.TabIndex = 17;
            this.lblKernelRadius.Text = "Kernel Radius : ";
            this.lblKernelRadius.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblKernelRadius.MouseLeave += new System.EventHandler(this.lblKernelRadius_MouseLeave);
            this.lblKernelRadius.MouseHover += new System.EventHandler(this.lblKernelRadius_MouseHover);
            // 
            // btnRefSelectClr
            // 
            this.btnRefSelectClr.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefSelectClr.Location = new System.Drawing.Point(306, 133);
            this.btnRefSelectClr.Margin = new System.Windows.Forms.Padding(2);
            this.btnRefSelectClr.Name = "btnRefSelectClr";
            this.btnRefSelectClr.Size = new System.Drawing.Size(30, 30);
            this.btnRefSelectClr.TabIndex = 28;
            this.btnRefSelectClr.Text = "";
            this.ttipMain.SetToolTip(this.btnRefSelectClr, "Deselect All");
            this.btnRefSelectClr.UseVisualStyleBackColor = true;
            this.btnRefSelectClr.Click += new System.EventHandler(this.btnRefSelectClr_Click);
            this.btnRefSelectClr.MouseHover += new System.EventHandler(this.btnRefSelectClr_MouseHover);
            // 
            // btnRefSelectAll
            // 
            this.btnRefSelectAll.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefSelectAll.Location = new System.Drawing.Point(306, 99);
            this.btnRefSelectAll.Margin = new System.Windows.Forms.Padding(2);
            this.btnRefSelectAll.Name = "btnRefSelectAll";
            this.btnRefSelectAll.Size = new System.Drawing.Size(30, 30);
            this.btnRefSelectAll.TabIndex = 27;
            this.btnRefSelectAll.Text = "";
            this.ttipMain.SetToolTip(this.btnRefSelectAll, "Select All");
            this.btnRefSelectAll.UseVisualStyleBackColor = true;
            this.btnRefSelectAll.Click += new System.EventHandler(this.btnRefSelectAll_Click);
            this.btnRefSelectAll.MouseLeave += new System.EventHandler(this.btnRefSelectAll_MouseLeave);
            this.btnRefSelectAll.MouseHover += new System.EventHandler(this.btnRefSelectAll_MouseHover);
            // 
            // btnRefCopy
            // 
            this.btnRefCopy.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefCopy.Location = new System.Drawing.Point(306, 65);
            this.btnRefCopy.Margin = new System.Windows.Forms.Padding(2);
            this.btnRefCopy.Name = "btnRefCopy";
            this.btnRefCopy.Size = new System.Drawing.Size(30, 30);
            this.btnRefCopy.TabIndex = 26;
            this.btnRefCopy.Text = "";
            this.ttipMain.SetToolTip(this.btnRefCopy, "Copy");
            this.btnRefCopy.UseVisualStyleBackColor = true;
            this.btnRefCopy.Click += new System.EventHandler(this.btnRefCopy_Click);
            this.btnRefCopy.MouseLeave += new System.EventHandler(this.btnRefCopy_MouseLeave);
            this.btnRefCopy.MouseHover += new System.EventHandler(this.btnRefCopy_MouseHover);
            // 
            // btnRefClear
            // 
            this.btnRefClear.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefClear.Location = new System.Drawing.Point(306, 31);
            this.btnRefClear.Margin = new System.Windows.Forms.Padding(2);
            this.btnRefClear.Name = "btnRefClear";
            this.btnRefClear.Size = new System.Drawing.Size(30, 30);
            this.btnRefClear.TabIndex = 25;
            this.btnRefClear.Text = "";
            this.ttipMain.SetToolTip(this.btnRefClear, "Clear");
            this.btnRefClear.UseVisualStyleBackColor = true;
            this.btnRefClear.Click += new System.EventHandler(this.btnRefClear_Click);
            this.btnRefClear.MouseLeave += new System.EventHandler(this.btnRefClear_MouseLeave);
            this.btnRefClear.MouseHover += new System.EventHandler(this.btnRefClear_MouseHover);
            // 
            // lblPolyOrder
            // 
            this.lblPolyOrder.Enabled = false;
            this.lblPolyOrder.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.lblPolyOrder.Location = new System.Drawing.Point(18, 65);
            this.lblPolyOrder.Name = "lblPolyOrder";
            this.lblPolyOrder.Size = new System.Drawing.Size(183, 19);
            this.lblPolyOrder.TabIndex = 20;
            this.lblPolyOrder.Text = "Polynomial Order :";
            this.lblPolyOrder.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPolyOrder.MouseLeave += new System.EventHandler(this.lblPolyOrder_MouseLeave);
            this.lblPolyOrder.MouseHover += new System.EventHandler(this.lblPolyOrder_MouseHover);
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
            "7",
            "8",
            "9",
            "10"});
            this.cbxPolyOrder.Location = new System.Drawing.Point(223, 63);
            this.cbxPolyOrder.Name = "cbxPolyOrder";
            this.cbxPolyOrder.Size = new System.Drawing.Size(103, 25);
            this.cbxPolyOrder.TabIndex = 21;
            this.cbxPolyOrder.SelectedIndexChanged += new System.EventHandler(this.cbxPolyOrder_SelectedIndexChanged);
            this.cbxPolyOrder.MouseLeave += new System.EventHandler(this.cbxPolyOrder_MouseLeave);
            this.cbxPolyOrder.MouseHover += new System.EventHandler(this.cbxPolyOrder_MouseHover);
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
            this.rbtnSG.MouseLeave += new System.EventHandler(this.rbtnSG_MouseLeave);
            this.rbtnSG.MouseHover += new System.EventHandler(this.rbtnSG_MouseHover);
            // 
            // gbInitData
            // 
            this.gbInitData.Controls.Add(this.btnInitSelectSync);
            this.gbInitData.Controls.Add(this.btnInitEdit);
            this.gbInitData.Controls.Add(this.lbInitData);
            this.gbInitData.Controls.Add(this.lblInitCnt);
            this.gbInitData.Controls.Add(this.btnInitClear);
            this.gbInitData.Controls.Add(this.btnInitDelete);
            this.gbInitData.Controls.Add(this.btnInitCopy);
            this.gbInitData.Controls.Add(this.btnInitPaste);
            this.gbInitData.Controls.Add(this.btnInitSelectClr);
            this.gbInitData.Controls.Add(this.btnInitSelectAll);
            this.gbInitData.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbInitData.Location = new System.Drawing.Point(15, 52);
            this.gbInitData.Name = "gbInitData";
            this.gbInitData.Size = new System.Drawing.Size(344, 586);
            this.gbInitData.TabIndex = 3;
            this.gbInitData.TabStop = false;
            this.gbInitData.Text = "Initial Dataset";
            // 
            // btnInitSelectSync
            // 
            this.btnInitSelectSync.Font = new System.Drawing.Font("Segoe Fluent Icons", 11.25F);
            this.btnInitSelectSync.Location = new System.Drawing.Point(306, 269);
            this.btnInitSelectSync.Name = "btnInitSelectSync";
            this.btnInitSelectSync.Size = new System.Drawing.Size(30, 30);
            this.btnInitSelectSync.TabIndex = 12;
            this.btnInitSelectSync.Text = "";
            this.ttipMain.SetToolTip(this.btnInitSelectSync, "Match Selection\r\n( ▶ Refined Dataset )");
            this.btnInitSelectSync.UseVisualStyleBackColor = true;
            this.btnInitSelectSync.Click += new System.EventHandler(this.btnInitSelectSync_Click);
            this.btnInitSelectSync.MouseHover += new System.EventHandler(this.btnInitSelectSync_MouseHover);
            // 
            // btnInitEdit
            // 
            this.btnInitEdit.Enabled = false;
            this.btnInitEdit.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInitEdit.Location = new System.Drawing.Point(306, 133);
            this.btnInitEdit.Margin = new System.Windows.Forms.Padding(2);
            this.btnInitEdit.Name = "btnInitEdit";
            this.btnInitEdit.Size = new System.Drawing.Size(30, 30);
            this.btnInitEdit.TabIndex = 8;
            this.btnInitEdit.Text = "";
            this.ttipMain.SetToolTip(this.btnInitEdit, "Edit");
            this.btnInitEdit.UseVisualStyleBackColor = true;
            this.btnInitEdit.Click += new System.EventHandler(this.btnInitEdit_Click);
            this.btnInitEdit.MouseLeave += new System.EventHandler(this.btnInitEdit_MouseLeave);
            this.btnInitEdit.MouseHover += new System.EventHandler(this.btnInitEdit_MouseHover);
            // 
            // lblInitCnt
            // 
            this.lblInitCnt.AutoSize = true;
            this.lblInitCnt.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 10.125F, System.Drawing.FontStyle.Bold);
            this.lblInitCnt.Location = new System.Drawing.Point(7, 555);
            this.lblInitCnt.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblInitCnt.Name = "lblInitCnt";
            this.lblInitCnt.Size = new System.Drawing.Size(65, 19);
            this.lblInitCnt.TabIndex = 7;
            this.lblInitCnt.Text = "Count : 0";
            // 
            // pbMain
            // 
            this.pbMain.Location = new System.Drawing.Point(0, 826);
            this.pbMain.Name = "pbMain";
            this.pbMain.Size = new System.Drawing.Size(734, 5);
            this.pbMain.TabIndex = 16;
            // 
            // gbRefinedData
            // 
            this.gbRefinedData.Controls.Add(this.btnRefSelectSync);
            this.gbRefinedData.Controls.Add(this.lbRefinedData);
            this.gbRefinedData.Controls.Add(this.lblRefCnt);
            this.gbRefinedData.Controls.Add(this.btnRefClear);
            this.gbRefinedData.Controls.Add(this.btnRefSelectClr);
            this.gbRefinedData.Controls.Add(this.btnRefCopy);
            this.gbRefinedData.Controls.Add(this.btnRefSelectAll);
            this.gbRefinedData.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbRefinedData.Location = new System.Drawing.Point(376, 52);
            this.gbRefinedData.Name = "gbRefinedData";
            this.gbRefinedData.Size = new System.Drawing.Size(344, 586);
            this.gbRefinedData.TabIndex = 23;
            this.gbRefinedData.TabStop = false;
            this.gbRefinedData.Text = "Refined Dataset";
            // 
            // btnRefSelectSync
            // 
            this.btnRefSelectSync.Font = new System.Drawing.Font("Segoe Fluent Icons", 11.25F);
            this.btnRefSelectSync.Location = new System.Drawing.Point(306, 167);
            this.btnRefSelectSync.Name = "btnRefSelectSync";
            this.btnRefSelectSync.Size = new System.Drawing.Size(30, 30);
            this.btnRefSelectSync.TabIndex = 29;
            this.btnRefSelectSync.Text = "";
            this.ttipMain.SetToolTip(this.btnRefSelectSync, "Match Selection \r\n( ◀ Initial Dataset )");
            this.btnRefSelectSync.UseVisualStyleBackColor = true;
            this.btnRefSelectSync.Click += new System.EventHandler(this.btnRefSelectSync_Click);
            this.btnRefSelectSync.MouseHover += new System.EventHandler(this.btnRefSelectSync_MouseHover);
            // 
            // lblRefCnt
            // 
            this.lblRefCnt.AutoSize = true;
            this.lblRefCnt.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 10.125F, System.Drawing.FontStyle.Bold);
            this.lblRefCnt.Location = new System.Drawing.Point(7, 555);
            this.lblRefCnt.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblRefCnt.Name = "lblRefCnt";
            this.lblRefCnt.Size = new System.Drawing.Size(65, 19);
            this.lblRefCnt.TabIndex = 8;
            this.lblRefCnt.Text = "Count : 0";
            // 
            // gbSmoothMtd
            // 
            this.gbSmoothMtd.Controls.Add(this.rbtnGauss);
            this.gbSmoothMtd.Controls.Add(this.rbtnSG);
            this.gbSmoothMtd.Controls.Add(this.rbtnRect);
            this.gbSmoothMtd.Controls.Add(this.rbtnAvg);
            this.gbSmoothMtd.Controls.Add(this.rbtnMed);
            this.gbSmoothMtd.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 11.25F, System.Drawing.FontStyle.Bold);
            this.gbSmoothMtd.Location = new System.Drawing.Point(15, 644);
            this.gbSmoothMtd.Name = "gbSmoothMtd";
            this.gbSmoothMtd.Size = new System.Drawing.Size(344, 130);
            this.gbSmoothMtd.TabIndex = 13;
            this.gbSmoothMtd.TabStop = false;
            this.gbSmoothMtd.Text = "Smoothing Method";
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
            this.rbtnGauss.CheckedChanged += new System.EventHandler(this.rbtnGauss_CheckedChanged);
            this.rbtnGauss.MouseLeave += new System.EventHandler(this.rbtnGauss_MouseLeave);
            this.rbtnGauss.MouseHover += new System.EventHandler(this.rbtnGauss_MouseHover);
            // 
            // gbSmoothParams
            // 
            this.gbSmoothParams.Controls.Add(this.cbxBoundaryMethod);
            this.gbSmoothParams.Controls.Add(this.lblBoundaryMethod);
            this.gbSmoothParams.Controls.Add(this.lblPolyOrder);
            this.gbSmoothParams.Controls.Add(this.cbxPolyOrder);
            this.gbSmoothParams.Controls.Add(this.cbxKernelRadius);
            this.gbSmoothParams.Controls.Add(this.lblKernelRadius);
            this.gbSmoothParams.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbSmoothParams.Location = new System.Drawing.Point(376, 644);
            this.gbSmoothParams.Name = "gbSmoothParams";
            this.gbSmoothParams.Size = new System.Drawing.Size(344, 130);
            this.gbSmoothParams.TabIndex = 19;
            this.gbSmoothParams.TabStop = false;
            this.gbSmoothParams.Text = "Signal Smoothing Parameters";
            // 
            // cbxBoundaryMethod
            // 
            this.cbxBoundaryMethod.DropDownHeight = 150;
            this.cbxBoundaryMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxBoundaryMethod.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 10.125F, System.Drawing.FontStyle.Bold);
            this.cbxBoundaryMethod.FormattingEnabled = true;
            this.cbxBoundaryMethod.IntegralHeight = false;
            this.cbxBoundaryMethod.ItemHeight = 17;
            this.cbxBoundaryMethod.Items.AddRange(new object[] {
            "Symmetric",
            "Replicate",
            "Zero Padding"});
            this.cbxBoundaryMethod.Location = new System.Drawing.Point(223, 97);
            this.cbxBoundaryMethod.Name = "cbxBoundaryMethod";
            this.cbxBoundaryMethod.Size = new System.Drawing.Size(103, 25);
            this.cbxBoundaryMethod.TabIndex = 23;
            this.cbxBoundaryMethod.SelectedIndexChanged += new System.EventHandler(this.cbxBoundaryMethod_SelectedIndexChanged);
            this.cbxBoundaryMethod.MouseLeave += new System.EventHandler(this.cbxBoundaryMethod_MouseLeave);
            this.cbxBoundaryMethod.MouseHover += new System.EventHandler(this.cbxBoundaryMethod_MouseHover);
            // 
            // lblBoundaryMethod
            // 
            this.lblBoundaryMethod.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.lblBoundaryMethod.Location = new System.Drawing.Point(18, 100);
            this.lblBoundaryMethod.Name = "lblBoundaryMethod";
            this.lblBoundaryMethod.Size = new System.Drawing.Size(183, 19);
            this.lblBoundaryMethod.TabIndex = 22;
            this.lblBoundaryMethod.Text = "Boundary Handling Method :";
            this.lblBoundaryMethod.MouseLeave += new System.EventHandler(this.lblBoundaryMethod_MouseLeave);
            this.lblBoundaryMethod.MouseHover += new System.EventHandler(this.lblBoundaryMethod_MouseHover);
            // 
            // btnExportSettings
            // 
            this.btnExportSettings.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExportSettings.Location = new System.Drawing.Point(655, 14);
            this.btnExportSettings.Margin = new System.Windows.Forms.Padding(2);
            this.btnExportSettings.Name = "btnExportSettings";
            this.btnExportSettings.Size = new System.Drawing.Size(30, 30);
            this.btnExportSettings.TabIndex = 31;
            this.btnExportSettings.Text = "";
            this.ttipMain.SetToolTip(this.btnExportSettings, "Export Settings");
            this.btnExportSettings.UseVisualStyleBackColor = true;
            this.btnExportSettings.Click += new System.EventHandler(this.btnExportSettings_Click);
            this.btnExportSettings.MouseLeave += new System.EventHandler(this.btnExportSettings_MouseLeave);
            this.btnExportSettings.MouseHover += new System.EventHandler(this.btnExportSettings_MouseHover);
            // 
            // btnExport
            // 
            this.btnExport.Font = new System.Drawing.Font("Segoe Fluent Icons", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExport.Location = new System.Drawing.Point(486, 783);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(234, 30);
            this.btnExport.TabIndex = 32;
            this.btnExport.Text = "";
            this.ttipMain.SetToolTip(this.btnExport, "Export");
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            this.btnExport.MouseHover += new System.EventHandler(this.btnExport_MouseHover);
            // 
            // btnInfo
            // 
            this.btnInfo.Font = new System.Drawing.Font("Segoe Fluent Icons", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInfo.ForeColor = System.Drawing.Color.MediumSlateBlue;
            this.btnInfo.Location = new System.Drawing.Point(689, 14);
            this.btnInfo.Margin = new System.Windows.Forms.Padding(2);
            this.btnInfo.Name = "btnInfo";
            this.btnInfo.Size = new System.Drawing.Size(30, 30);
            this.btnInfo.TabIndex = 33;
            this.btnInfo.Text = "";
            this.ttipMain.SetToolTip(this.btnInfo, "About");
            this.btnInfo.UseVisualStyleBackColor = true;
            this.btnInfo.Click += new System.EventHandler(this.btnInfo_Click);
            this.btnInfo.MouseLeave += new System.EventHandler(this.btnInfo_MouseLeave);
            this.btnInfo.MouseHover += new System.EventHandler(this.btnInfo_MouseHover);
            // 
            // txtDatasetTitle
            // 
            this.txtDatasetTitle.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 10.125F, System.Drawing.FontStyle.Bold);
            this.txtDatasetTitle.Location = new System.Drawing.Point(385, 16);
            this.txtDatasetTitle.Name = "txtDatasetTitle";
            this.txtDatasetTitle.Size = new System.Drawing.Size(265, 25);
            this.txtDatasetTitle.TabIndex = 30;
            this.txtDatasetTitle.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtDatasetTitle.TextChanged += new System.EventHandler(this.txtDatasetTitle_TextChanged);
            this.txtDatasetTitle.Enter += new System.EventHandler(this.txtDatasetTitle_Enter);
            this.txtDatasetTitle.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtDatasetTitle_KeyDown);
            this.txtDatasetTitle.Leave += new System.EventHandler(this.txtDatasetTitle_Leave);
            this.txtDatasetTitle.MouseLeave += new System.EventHandler(this.txtDatasetTitle_MouseLeave);
            this.txtDatasetTitle.MouseHover += new System.EventHandler(this.txtDatasetTitle_MouseHover);
            // 
            // slblDesc
            // 
            this.slblDesc.AutoSize = false;
            this.slblDesc.Font = new System.Drawing.Font("Segoe UI Variable Display", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.slblDesc.ForeColor = System.Drawing.Color.White;
            this.slblDesc.Name = "slblDesc";
            this.slblDesc.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.slblDesc.Size = new System.Drawing.Size(731, 19);
            this.slblDesc.Text = "To calibrate, add data to the Initial Dataset, choose a Calibration Method , set " +
    "Smoothing Parameters.";
            // 
            // tlblCalibratedType
            // 
            this.tlblCalibratedType.Font = new System.Drawing.Font("Segoe UI Variable Display", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tlblCalibratedType.ForeColor = System.Drawing.Color.White;
            this.tlblCalibratedType.Name = "tlblCalibratedType";
            this.tlblCalibratedType.Size = new System.Drawing.Size(114, 19);
            this.tlblCalibratedType.Text = "Applied Calibration :";
            // 
            // slblCalibratedType
            // 
            this.slblCalibratedType.Font = new System.Drawing.Font("Segoe UI Variable Display", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.slblCalibratedType.ForeColor = System.Drawing.Color.White;
            this.slblCalibratedType.Name = "slblCalibratedType";
            this.slblCalibratedType.Size = new System.Drawing.Size(17, 16);
            this.slblCalibratedType.Text = "--";
            // 
            // tlblSeparator1
            // 
            this.tlblSeparator1.Font = new System.Drawing.Font("Segoe UI Variable Display", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tlblSeparator1.ForeColor = System.Drawing.Color.White;
            this.tlblSeparator1.Name = "tlblSeparator1";
            this.tlblSeparator1.Size = new System.Drawing.Size(19, 16);
            this.tlblSeparator1.Text = "｜";
            // 
            // tlblKernelRadius
            // 
            this.tlblKernelRadius.Font = new System.Drawing.Font("Segoe UI Variable Display", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tlblKernelRadius.ForeColor = System.Drawing.Color.White;
            this.tlblKernelRadius.Name = "tlblKernelRadius";
            this.tlblKernelRadius.Size = new System.Drawing.Size(86, 16);
            this.tlblKernelRadius.Text = "Kernel Radius : ";
            // 
            // slblKernelRadius
            // 
            this.slblKernelRadius.Font = new System.Drawing.Font("Segoe UI Variable Display", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.slblKernelRadius.ForeColor = System.Drawing.Color.White;
            this.slblKernelRadius.Name = "slblKernelRadius";
            this.slblKernelRadius.Size = new System.Drawing.Size(17, 16);
            this.slblKernelRadius.Text = "--";
            // 
            // tlblSeparator2
            // 
            this.tlblSeparator2.Font = new System.Drawing.Font("Segoe UI Variable Display", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tlblSeparator2.ForeColor = System.Drawing.Color.White;
            this.tlblSeparator2.Name = "tlblSeparator2";
            this.tlblSeparator2.Size = new System.Drawing.Size(19, 16);
            this.tlblSeparator2.Text = "｜";
            this.tlblSeparator2.Visible = false;
            // 
            // tlblPolyOrder
            // 
            this.tlblPolyOrder.Font = new System.Drawing.Font("Segoe UI Variable Display", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tlblPolyOrder.ForeColor = System.Drawing.Color.White;
            this.tlblPolyOrder.Name = "tlblPolyOrder";
            this.tlblPolyOrder.Size = new System.Drawing.Size(107, 16);
            this.tlblPolyOrder.Text = "Polynomial Order : ";
            this.tlblPolyOrder.Visible = false;
            // 
            // slblPolyOrder
            // 
            this.slblPolyOrder.Font = new System.Drawing.Font("Segoe UI Variable Display", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.slblPolyOrder.ForeColor = System.Drawing.Color.White;
            this.slblPolyOrder.Name = "slblPolyOrder";
            this.slblPolyOrder.Size = new System.Drawing.Size(17, 16);
            this.slblPolyOrder.Text = "--";
            this.slblPolyOrder.Visible = false;
            // 
            // tlblSeparator3
            // 
            this.tlblSeparator3.Font = new System.Drawing.Font("Segoe UI Variable Display", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tlblSeparator3.ForeColor = System.Drawing.Color.White;
            this.tlblSeparator3.Name = "tlblSeparator3";
            this.tlblSeparator3.Size = new System.Drawing.Size(19, 16);
            this.tlblSeparator3.Text = "｜";
            this.tlblSeparator3.Visible = false;
            // 
            // tlblBoundaryMethod
            // 
            this.tlblBoundaryMethod.Font = new System.Drawing.Font("Segoe UI Variable Display", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tlblBoundaryMethod.ForeColor = System.Drawing.Color.White;
            this.tlblBoundaryMethod.Name = "tlblBoundaryMethod";
            this.tlblBoundaryMethod.Size = new System.Drawing.Size(117, 16);
            this.tlblBoundaryMethod.Text = "Boundary Handling : ";
            this.tlblBoundaryMethod.Visible = false;
            // 
            // statStripMain
            // 
            this.statStripMain.AutoSize = false;
            this.statStripMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(223)))));
            this.statStripMain.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.statStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.slblDesc,
            this.tlblCalibratedType,
            this.slblCalibratedType,
            this.tlblSeparator1,
            this.tlblKernelRadius,
            this.slblKernelRadius,
            this.tlblSeparator2,
            this.tlblPolyOrder,
            this.slblPolyOrder,
            this.tlblSeparator3,
            this.tlblBoundaryMethod,
            this.slblBoundaryMethod});
            this.statStripMain.Location = new System.Drawing.Point(0, 831);
            this.statStripMain.Name = "statStripMain";
            this.statStripMain.Size = new System.Drawing.Size(734, 24);
            this.statStripMain.SizingGrip = false;
            this.statStripMain.TabIndex = 27;
            this.statStripMain.Text = "statusStrip1";
            // 
            // slblBoundaryMethod
            // 
            this.slblBoundaryMethod.Font = new System.Drawing.Font("Segoe UI Variable Display", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.slblBoundaryMethod.ForeColor = System.Drawing.Color.White;
            this.slblBoundaryMethod.Name = "slblBoundaryMethod";
            this.slblBoundaryMethod.Size = new System.Drawing.Size(17, 16);
            this.slblBoundaryMethod.Text = "--";
            this.slblBoundaryMethod.Visible = false;
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(734, 855);
            this.Controls.Add(this.btnInfo);
            this.Controls.Add(this.btnExportSettings);
            this.Controls.Add(this.txtDatasetTitle);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.gbSmoothParams);
            this.Controls.Add(this.pbMain);
            this.Controls.Add(this.gbSmoothMtd);
            this.Controls.Add(this.statStripMain);
            this.Controls.Add(this.gbRefinedData);
            this.Controls.Add(this.gbInitData);
            this.Controls.Add(this.txtInitAdd);
            this.Controls.Add(this.btnInitAdd);
            this.Controls.Add(this.btnCalibrate);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SonataSmooth";
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.gbInitData.ResumeLayout(false);
            this.gbInitData.PerformLayout();
            this.gbRefinedData.ResumeLayout(false);
            this.gbRefinedData.PerformLayout();
            this.gbSmoothMtd.ResumeLayout(false);
            this.gbSmoothParams.ResumeLayout(false);
            this.statStripMain.ResumeLayout(false);
            this.statStripMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.RadioButton rbtnAvg;
        private System.Windows.Forms.RadioButton rbtnMed;
        private System.Windows.Forms.Button btnCalibrate;
        private System.Windows.Forms.Button btnInitAdd;
        private System.Windows.Forms.TextBox txtInitAdd;
        private System.Windows.Forms.RadioButton rbtnRect;
        private System.Windows.Forms.Button btnInitClear;
        private System.Windows.Forms.Button btnInitDelete;
        private System.Windows.Forms.Button btnInitCopy;
        private System.Windows.Forms.Button btnInitPaste;
        private System.Windows.Forms.Button btnInitSelectAll;
        private System.Windows.Forms.Button btnInitSelectClr;
        private System.Windows.Forms.Label lblKernelRadius;
        private System.Windows.Forms.Button btnRefSelectClr;
        private System.Windows.Forms.Button btnRefSelectAll;
        private System.Windows.Forms.Button btnRefCopy;
        private System.Windows.Forms.Button btnRefClear;
        private System.Windows.Forms.GroupBox gbInitData;
        private System.Windows.Forms.GroupBox gbRefinedData;
        private System.Windows.Forms.ProgressBar pbMain;
        private System.Windows.Forms.RadioButton rbtnSG;
        private System.Windows.Forms.Label lblPolyOrder;
        private System.Windows.Forms.Label lblRefCnt;
        private System.Windows.Forms.GroupBox gbSmoothMtd;
        private System.Windows.Forms.GroupBox gbSmoothParams;
        private System.Windows.Forms.RadioButton rbtnGauss;
        private System.Windows.Forms.ToolTip ttipMain;
        public System.Windows.Forms.ListBox lbInitData;
        public System.Windows.Forms.ListBox lbRefinedData;
        private System.Windows.Forms.Button btnInitEdit;
        public System.Windows.Forms.Label lblInitCnt;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.TextBox txtDatasetTitle;
        private System.Windows.Forms.Button btnExportSettings;
        public System.Windows.Forms.ComboBox cbxKernelRadius;
        private System.Windows.Forms.ComboBox cbxPolyOrder;
        private System.Windows.Forms.Button btnInfo;
        private System.Windows.Forms.Button btnInitSelectSync;
        private System.Windows.Forms.Button btnRefSelectSync;
        private System.Windows.Forms.Label lblBoundaryMethod;
        private System.Windows.Forms.ComboBox cbxBoundaryMethod;
        private System.Windows.Forms.ToolStripStatusLabel slblDesc;
        private System.Windows.Forms.ToolStripStatusLabel tlblCalibratedType;
        private System.Windows.Forms.ToolStripStatusLabel slblCalibratedType;
        private System.Windows.Forms.ToolStripStatusLabel tlblSeparator1;
        private System.Windows.Forms.ToolStripStatusLabel tlblKernelRadius;
        private System.Windows.Forms.ToolStripStatusLabel slblKernelRadius;
        private System.Windows.Forms.ToolStripStatusLabel tlblSeparator2;
        private System.Windows.Forms.ToolStripStatusLabel tlblPolyOrder;
        private System.Windows.Forms.ToolStripStatusLabel slblPolyOrder;
        private System.Windows.Forms.ToolStripStatusLabel tlblSeparator3;
        private System.Windows.Forms.ToolStripStatusLabel tlblBoundaryMethod;
        private System.Windows.Forms.StatusStrip statStripMain;
        private System.Windows.Forms.ToolStripStatusLabel slblBoundaryMethod;
    }
}

