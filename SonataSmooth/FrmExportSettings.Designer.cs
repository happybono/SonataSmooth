namespace SonataSmooth
{
    partial class FrmExportSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmExportSettings));
            this.lblExportConfigTtl = new System.Windows.Forms.Label();
            this.gbExportOpts = new System.Windows.Forms.GroupBox();
            this.chbOpenFile = new System.Windows.Forms.CheckBox();
            this.rbtnCSV = new System.Windows.Forms.RadioButton();
            this.rbtnXLSX = new System.Windows.Forms.RadioButton();
            this.statStripExportConfig = new System.Windows.Forms.StatusStrip();
            this.slblDesc = new System.Windows.Forms.ToolStripStatusLabel();
            this.gbSmoothParams = new System.Windows.Forms.GroupBox();
            this.lblKernelWidth = new System.Windows.Forms.Label();
            this.cbxSigmaFactor = new System.Windows.Forms.ComboBox();
            this.lblSigmaFactor = new System.Windows.Forms.Label();
            this.lblAlpha = new System.Windows.Forms.Label();
            this.cbxAlpha = new System.Windows.Forms.ComboBox();
            this.lblDerivOrder = new System.Windows.Forms.Label();
            this.cbxDerivOrder = new System.Windows.Forms.ComboBox();
            this.cbxBoundaryMethod = new System.Windows.Forms.ComboBox();
            this.lblBoundaryMethod = new System.Windows.Forms.Label();
            this.lblPolyOrder = new System.Windows.Forms.Label();
            this.cbxPolyOrder = new System.Windows.Forms.ComboBox();
            this.cbxKernelRadius = new System.Windows.Forms.ComboBox();
            this.lblKernelRadius = new System.Windows.Forms.Label();
            this.gbSmoothMtd = new System.Windows.Forms.GroupBox();
            this.chbGaussMed = new System.Windows.Forms.CheckBox();
            this.chbRect = new System.Windows.Forms.CheckBox();
            this.chbAvg = new System.Windows.Forms.CheckBox();
            this.chbSG = new System.Windows.Forms.CheckBox();
            this.chbMed = new System.Windows.Forms.CheckBox();
            this.chbGauss = new System.Windows.Forms.CheckBox();
            this.btnSetDefault = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.gbExportOpts.SuspendLayout();
            this.statStripExportConfig.SuspendLayout();
            this.gbSmoothParams.SuspendLayout();
            this.gbSmoothMtd.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblExportConfigTtl
            // 
            this.lblExportConfigTtl.AutoSize = true;
            this.lblExportConfigTtl.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExportConfigTtl.Location = new System.Drawing.Point(8, 10);
            this.lblExportConfigTtl.Name = "lblExportConfigTtl";
            this.lblExportConfigTtl.Size = new System.Drawing.Size(191, 26);
            this.lblExportConfigTtl.TabIndex = 31;
            this.lblExportConfigTtl.Text = "Export Configuration";
            // 
            // gbExportOpts
            // 
            this.gbExportOpts.Controls.Add(this.chbOpenFile);
            this.gbExportOpts.Controls.Add(this.rbtnCSV);
            this.gbExportOpts.Controls.Add(this.rbtnXLSX);
            this.gbExportOpts.Font = new System.Drawing.Font("Segoe UI Variable Display Semil", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbExportOpts.Location = new System.Drawing.Point(11, 281);
            this.gbExportOpts.Name = "gbExportOpts";
            this.gbExportOpts.Size = new System.Drawing.Size(783, 85);
            this.gbExportOpts.TabIndex = 12;
            this.gbExportOpts.TabStop = false;
            this.gbExportOpts.Text = "Data Export Options";
            // 
            // chbOpenFile
            // 
            this.chbOpenFile.AutoSize = true;
            this.chbOpenFile.Checked = true;
            this.chbOpenFile.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbOpenFile.Font = new System.Drawing.Font("Segoe UI Variable Display", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chbOpenFile.Location = new System.Drawing.Point(247, 58);
            this.chbOpenFile.Name = "chbOpenFile";
            this.chbOpenFile.Size = new System.Drawing.Size(289, 21);
            this.chbOpenFile.TabIndex = 15;
            this.chbOpenFile.Text = "Open the saved file automatically after saving.";
            this.chbOpenFile.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chbOpenFile.UseVisualStyleBackColor = true;
            this.chbOpenFile.MouseLeave += new System.EventHandler(this.chbOpenFile_MouseLeave);
            this.chbOpenFile.MouseHover += new System.EventHandler(this.chbOpenFile_MouseHover);
            // 
            // rbtnCSV
            // 
            this.rbtnCSV.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbtnCSV.Font = new System.Drawing.Font("Segoe UI Variable Display", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbtnCSV.Location = new System.Drawing.Point(404, 21);
            this.rbtnCSV.Name = "rbtnCSV";
            this.rbtnCSV.Size = new System.Drawing.Size(203, 30);
            this.rbtnCSV.TabIndex = 14;
            this.rbtnCSV.TabStop = true;
            this.rbtnCSV.Text = "Export as CSV";
            this.rbtnCSV.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbtnCSV.UseVisualStyleBackColor = true;
            this.rbtnCSV.MouseLeave += new System.EventHandler(this.rbtnCSV_MouseLeave);
            this.rbtnCSV.MouseHover += new System.EventHandler(this.rbtnCSV_MouseHover);
            // 
            // rbtnXLSX
            // 
            this.rbtnXLSX.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbtnXLSX.Checked = true;
            this.rbtnXLSX.Font = new System.Drawing.Font("Segoe UI Variable Display", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbtnXLSX.Location = new System.Drawing.Point(176, 21);
            this.rbtnXLSX.Name = "rbtnXLSX";
            this.rbtnXLSX.Size = new System.Drawing.Size(203, 30);
            this.rbtnXLSX.TabIndex = 13;
            this.rbtnXLSX.TabStop = true;
            this.rbtnXLSX.Text = "Export as XLSX";
            this.rbtnXLSX.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbtnXLSX.UseVisualStyleBackColor = true;
            this.rbtnXLSX.MouseLeave += new System.EventHandler(this.rbtnXLSX_MouseLeave);
            this.rbtnXLSX.MouseHover += new System.EventHandler(this.rbtnXLSX_MouseHover);
            // 
            // statStripExportConfig
            // 
            this.statStripExportConfig.AutoSize = false;
            this.statStripExportConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(223)))));
            this.statStripExportConfig.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.slblDesc});
            this.statStripExportConfig.Location = new System.Drawing.Point(0, 412);
            this.statStripExportConfig.Name = "statStripExportConfig";
            this.statStripExportConfig.Size = new System.Drawing.Size(806, 24);
            this.statStripExportConfig.SizingGrip = false;
            this.statStripExportConfig.TabIndex = 34;
            this.statStripExportConfig.Text = "statusStrip1";
            // 
            // slblDesc
            // 
            this.slblDesc.AutoSize = false;
            this.slblDesc.Font = new System.Drawing.Font("Segoe UI Variable Display", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.slblDesc.ForeColor = System.Drawing.Color.White;
            this.slblDesc.Name = "slblDesc";
            this.slblDesc.Size = new System.Drawing.Size(804, 19);
            this.slblDesc.Text = "To save the settings, please select the desired options and click the \'Save\' butt" +
    "on.";
            // 
            // gbSmoothParams
            // 
            this.gbSmoothParams.Controls.Add(this.lblKernelWidth);
            this.gbSmoothParams.Controls.Add(this.cbxSigmaFactor);
            this.gbSmoothParams.Controls.Add(this.lblSigmaFactor);
            this.gbSmoothParams.Controls.Add(this.lblAlpha);
            this.gbSmoothParams.Controls.Add(this.cbxAlpha);
            this.gbSmoothParams.Controls.Add(this.lblDerivOrder);
            this.gbSmoothParams.Controls.Add(this.cbxDerivOrder);
            this.gbSmoothParams.Controls.Add(this.cbxBoundaryMethod);
            this.gbSmoothParams.Controls.Add(this.lblBoundaryMethod);
            this.gbSmoothParams.Controls.Add(this.lblPolyOrder);
            this.gbSmoothParams.Controls.Add(this.cbxPolyOrder);
            this.gbSmoothParams.Controls.Add(this.cbxKernelRadius);
            this.gbSmoothParams.Controls.Add(this.lblKernelRadius);
            this.gbSmoothParams.Font = new System.Drawing.Font("Segoe UI Variable Display Semil", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbSmoothParams.Location = new System.Drawing.Point(410, 48);
            this.gbSmoothParams.Name = "gbSmoothParams";
            this.gbSmoothParams.Size = new System.Drawing.Size(384, 219);
            this.gbSmoothParams.TabIndex = 35;
            this.gbSmoothParams.TabStop = false;
            this.gbSmoothParams.Text = "Smoothing Parameters";
            // 
            // lblKernelWidth
            // 
            this.lblKernelWidth.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblKernelWidth.Location = new System.Drawing.Point(225, 123);
            this.lblKernelWidth.Name = "lblKernelWidth";
            this.lblKernelWidth.Size = new System.Drawing.Size(40, 19);
            this.lblKernelWidth.TabIndex = 40;
            this.lblKernelWidth.Text = "w ÷";
            this.lblKernelWidth.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblKernelWidth.MouseLeave += new System.EventHandler(this.lblKernelWidth_MouseLeave);
            this.lblKernelWidth.MouseHover += new System.EventHandler(this.lblKernelWidth_MouseHover);
            // 
            // cbxSigmaFactor
            // 
            this.cbxSigmaFactor.DropDownHeight = 150;
            this.cbxSigmaFactor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxSigmaFactor.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 10.125F, System.Drawing.FontStyle.Bold);
            this.cbxSigmaFactor.FormattingEnabled = true;
            this.cbxSigmaFactor.IntegralHeight = false;
            this.cbxSigmaFactor.ItemHeight = 17;
            this.cbxSigmaFactor.Items.AddRange(new object[] {
            "1.0",
            "2.0",
            "3.0",
            "4.0",
            "5.0",
            "6.0",
            "7.0",
            "8.0",
            "9.0",
            "10.0",
            "11.0",
            "12.0"});
            this.cbxSigmaFactor.Location = new System.Drawing.Point(269, 122);
            this.cbxSigmaFactor.Name = "cbxSigmaFactor";
            this.cbxSigmaFactor.Size = new System.Drawing.Size(87, 25);
            this.cbxSigmaFactor.TabIndex = 39;
            this.cbxSigmaFactor.MouseLeave += new System.EventHandler(this.cbxSigmaFactor_MouseLeave);
            this.cbxSigmaFactor.MouseHover += new System.EventHandler(this.cbxSigmaFactor_MouseHover);
            // 
            // lblSigmaFactor
            // 
            this.lblSigmaFactor.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.lblSigmaFactor.Location = new System.Drawing.Point(28, 123);
            this.lblSigmaFactor.Name = "lblSigmaFactor";
            this.lblSigmaFactor.Size = new System.Drawing.Size(183, 19);
            this.lblSigmaFactor.TabIndex = 38;
            this.lblSigmaFactor.Text = "Sigma Factor :";
            this.lblSigmaFactor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblSigmaFactor.MouseLeave += new System.EventHandler(this.lblSigmaFactor_MouseLeave);
            this.lblSigmaFactor.MouseHover += new System.EventHandler(this.lblSigmaFactor_MouseHover);
            // 
            // lblAlpha
            // 
            this.lblAlpha.Enabled = false;
            this.lblAlpha.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.lblAlpha.Location = new System.Drawing.Point(28, 93);
            this.lblAlpha.Name = "lblAlpha";
            this.lblAlpha.Size = new System.Drawing.Size(183, 19);
            this.lblAlpha.TabIndex = 33;
            this.lblAlpha.Text = "Alpha Blend :";
            this.lblAlpha.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblAlpha.MouseLeave += new System.EventHandler(this.lblAlpha_MouseLeave);
            this.lblAlpha.MouseHover += new System.EventHandler(this.lblAlpha_MouseHover);
            // 
            // cbxAlpha
            // 
            this.cbxAlpha.DropDownHeight = 150;
            this.cbxAlpha.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxAlpha.Enabled = false;
            this.cbxAlpha.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 10.125F, System.Drawing.FontStyle.Bold);
            this.cbxAlpha.FormattingEnabled = true;
            this.cbxAlpha.IntegralHeight = false;
            this.cbxAlpha.ItemHeight = 17;
            this.cbxAlpha.Items.AddRange(new object[] {
            "0.00",
            "0.05",
            "0.10",
            "0.15",
            "0.20",
            "0.25",
            "0.30",
            "0.35",
            "0.40",
            "0.45",
            "0.50",
            "0.55",
            "0.60",
            "0.65",
            "0.70",
            "0.75",
            "0.80",
            "0.85",
            "0.90",
            "0.95",
            "1.00"});
            this.cbxAlpha.Location = new System.Drawing.Point(233, 91);
            this.cbxAlpha.Name = "cbxAlpha";
            this.cbxAlpha.Size = new System.Drawing.Size(123, 25);
            this.cbxAlpha.TabIndex = 34;
            this.cbxAlpha.MouseLeave += new System.EventHandler(this.cbxAlpha_MouseLeave);
            this.cbxAlpha.MouseHover += new System.EventHandler(this.cbxAlpha_MouseHover);
            // 
            // lblDerivOrder
            // 
            this.lblDerivOrder.Enabled = false;
            this.lblDerivOrder.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.lblDerivOrder.Location = new System.Drawing.Point(28, 185);
            this.lblDerivOrder.Name = "lblDerivOrder";
            this.lblDerivOrder.Size = new System.Drawing.Size(183, 19);
            this.lblDerivOrder.TabIndex = 32;
            this.lblDerivOrder.Text = "Derivative Order :";
            this.lblDerivOrder.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblDerivOrder.MouseLeave += new System.EventHandler(this.lblDerivOrder_MouseLeave);
            this.lblDerivOrder.MouseHover += new System.EventHandler(this.lblDerivOrder_MouseHover);
            // 
            // cbxDerivOrder
            // 
            this.cbxDerivOrder.DropDownHeight = 150;
            this.cbxDerivOrder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxDerivOrder.Enabled = false;
            this.cbxDerivOrder.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 10.125F, System.Drawing.FontStyle.Bold);
            this.cbxDerivOrder.FormattingEnabled = true;
            this.cbxDerivOrder.IntegralHeight = false;
            this.cbxDerivOrder.ItemHeight = 17;
            this.cbxDerivOrder.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5"});
            this.cbxDerivOrder.Location = new System.Drawing.Point(233, 183);
            this.cbxDerivOrder.Name = "cbxDerivOrder";
            this.cbxDerivOrder.Size = new System.Drawing.Size(123, 25);
            this.cbxDerivOrder.TabIndex = 11;
            this.cbxDerivOrder.MouseLeave += new System.EventHandler(this.cbxDerivOrder_MouseLeave);
            this.cbxDerivOrder.MouseHover += new System.EventHandler(this.cbxDerivOrder_MouseHover);
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
            "Adaptive",
            "Replicate",
            "Zero Padding"});
            this.cbxBoundaryMethod.Location = new System.Drawing.Point(233, 60);
            this.cbxBoundaryMethod.Name = "cbxBoundaryMethod";
            this.cbxBoundaryMethod.Size = new System.Drawing.Size(123, 25);
            this.cbxBoundaryMethod.TabIndex = 9;
            this.cbxBoundaryMethod.MouseLeave += new System.EventHandler(this.cbxBoundaryMethod_MouseLeave);
            this.cbxBoundaryMethod.MouseHover += new System.EventHandler(this.cbxBoundaryMethod_MouseHover);
            // 
            // lblBoundaryMethod
            // 
            this.lblBoundaryMethod.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.lblBoundaryMethod.Location = new System.Drawing.Point(28, 63);
            this.lblBoundaryMethod.Name = "lblBoundaryMethod";
            this.lblBoundaryMethod.Size = new System.Drawing.Size(183, 19);
            this.lblBoundaryMethod.TabIndex = 30;
            this.lblBoundaryMethod.Text = "Boundary Handling Method :";
            this.lblBoundaryMethod.MouseLeave += new System.EventHandler(this.lblBoundaryMethod_MouseLeave);
            this.lblBoundaryMethod.MouseHover += new System.EventHandler(this.lblBoundaryMethod_MouseHover);
            // 
            // lblPolyOrder
            // 
            this.lblPolyOrder.Enabled = false;
            this.lblPolyOrder.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.lblPolyOrder.Location = new System.Drawing.Point(28, 154);
            this.lblPolyOrder.Name = "lblPolyOrder";
            this.lblPolyOrder.Size = new System.Drawing.Size(183, 19);
            this.lblPolyOrder.TabIndex = 27;
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
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.cbxPolyOrder.Location = new System.Drawing.Point(233, 152);
            this.cbxPolyOrder.Name = "cbxPolyOrder";
            this.cbxPolyOrder.Size = new System.Drawing.Size(123, 25);
            this.cbxPolyOrder.TabIndex = 10;
            this.cbxPolyOrder.MouseLeave += new System.EventHandler(this.cbxPolyOrder_MouseLeave);
            this.cbxPolyOrder.MouseHover += new System.EventHandler(this.cbxPolyOrder_MouseHover);
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
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "30"});
            this.cbxKernelRadius.Location = new System.Drawing.Point(233, 29);
            this.cbxKernelRadius.Margin = new System.Windows.Forms.Padding(2);
            this.cbxKernelRadius.Name = "cbxKernelRadius";
            this.cbxKernelRadius.Size = new System.Drawing.Size(123, 25);
            this.cbxKernelRadius.TabIndex = 8;
            this.cbxKernelRadius.SelectedIndexChanged += new System.EventHandler(this.cbxKernelRadius_SelectedIndexChanged);
            this.cbxKernelRadius.MouseLeave += new System.EventHandler(this.cbxKernelRadius_MouseLeave);
            this.cbxKernelRadius.MouseHover += new System.EventHandler(this.cbxKernelRadius_MouseHover);
            // 
            // lblKernelRadius
            // 
            this.lblKernelRadius.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblKernelRadius.Location = new System.Drawing.Point(28, 31);
            this.lblKernelRadius.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblKernelRadius.Name = "lblKernelRadius";
            this.lblKernelRadius.Size = new System.Drawing.Size(183, 19);
            this.lblKernelRadius.TabIndex = 26;
            this.lblKernelRadius.Text = "Kernel Radius : ";
            this.lblKernelRadius.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblKernelRadius.MouseLeave += new System.EventHandler(this.lblKernelRadius_MouseLeave);
            this.lblKernelRadius.MouseHover += new System.EventHandler(this.lblKernelRadius_MouseHover);
            // 
            // gbSmoothMtd
            // 
            this.gbSmoothMtd.Controls.Add(this.chbGaussMed);
            this.gbSmoothMtd.Controls.Add(this.chbRect);
            this.gbSmoothMtd.Controls.Add(this.chbAvg);
            this.gbSmoothMtd.Controls.Add(this.chbSG);
            this.gbSmoothMtd.Controls.Add(this.chbMed);
            this.gbSmoothMtd.Controls.Add(this.chbGauss);
            this.gbSmoothMtd.Font = new System.Drawing.Font("Segoe UI Variable Display Semil", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbSmoothMtd.Location = new System.Drawing.Point(11, 48);
            this.gbSmoothMtd.Name = "gbSmoothMtd";
            this.gbSmoothMtd.Size = new System.Drawing.Size(384, 219);
            this.gbSmoothMtd.TabIndex = 35;
            this.gbSmoothMtd.TabStop = false;
            this.gbSmoothMtd.Text = "Smoothing Methods";
            // 
            // chbGaussMed
            // 
            this.chbGaussMed.Appearance = System.Windows.Forms.Appearance.Button;
            this.chbGaussMed.Checked = true;
            this.chbGaussMed.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbGaussMed.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.chbGaussMed.Location = new System.Drawing.Point(196, 95);
            this.chbGaussMed.Name = "chbGaussMed";
            this.chbGaussMed.Size = new System.Drawing.Size(176, 40);
            this.chbGaussMed.TabIndex = 8;
            this.chbGaussMed.Text = "Gaussian Median";
            this.chbGaussMed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chbGaussMed.UseVisualStyleBackColor = true;
            this.chbGaussMed.CheckedChanged += new System.EventHandler(this.chbGaussMed_CheckedChanged);
            this.chbGaussMed.MouseLeave += new System.EventHandler(this.chbGaussMed_MouseLeave);
            this.chbGaussMed.MouseHover += new System.EventHandler(this.chbGaussMed_MouseHover);
            // 
            // chbRect
            // 
            this.chbRect.Appearance = System.Windows.Forms.Appearance.Button;
            this.chbRect.Checked = true;
            this.chbRect.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbRect.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.chbRect.Location = new System.Drawing.Point(13, 48);
            this.chbRect.Name = "chbRect";
            this.chbRect.Size = new System.Drawing.Size(176, 40);
            this.chbRect.TabIndex = 2;
            this.chbRect.Text = "Rectangular Averaging";
            this.chbRect.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chbRect.UseVisualStyleBackColor = true;
            this.chbRect.CheckedChanged += new System.EventHandler(this.chbRect_CheckedChanged);
            this.chbRect.MouseLeave += new System.EventHandler(this.chbRect_MouseLeave);
            this.chbRect.MouseHover += new System.EventHandler(this.chbRect_MouseHover);
            // 
            // chbAvg
            // 
            this.chbAvg.Appearance = System.Windows.Forms.Appearance.Button;
            this.chbAvg.Checked = true;
            this.chbAvg.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbAvg.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.chbAvg.Location = new System.Drawing.Point(196, 48);
            this.chbAvg.Name = "chbAvg";
            this.chbAvg.Size = new System.Drawing.Size(176, 40);
            this.chbAvg.TabIndex = 3;
            this.chbAvg.Text = "Binomial Averaging";
            this.chbAvg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chbAvg.UseVisualStyleBackColor = true;
            this.chbAvg.CheckedChanged += new System.EventHandler(this.chbAvg_CheckedChanged);
            this.chbAvg.MouseLeave += new System.EventHandler(this.chbAvg_MouseLeave);
            this.chbAvg.MouseHover += new System.EventHandler(this.chbAvg_MouseHover);
            // 
            // chbSG
            // 
            this.chbSG.Appearance = System.Windows.Forms.Appearance.Button;
            this.chbSG.Checked = true;
            this.chbSG.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbSG.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.chbSG.Location = new System.Drawing.Point(196, 142);
            this.chbSG.Name = "chbSG";
            this.chbSG.Size = new System.Drawing.Size(176, 40);
            this.chbSG.TabIndex = 6;
            this.chbSG.Text = "Savitzky-Golay Filtering";
            this.chbSG.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chbSG.UseVisualStyleBackColor = true;
            this.chbSG.CheckedChanged += new System.EventHandler(this.chbSG_CheckedChanged);
            this.chbSG.MouseLeave += new System.EventHandler(this.chbSG_MouseLeave);
            this.chbSG.MouseHover += new System.EventHandler(this.chbSG_MouseHover);
            // 
            // chbMed
            // 
            this.chbMed.Appearance = System.Windows.Forms.Appearance.Button;
            this.chbMed.Checked = true;
            this.chbMed.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbMed.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.chbMed.Location = new System.Drawing.Point(13, 95);
            this.chbMed.Name = "chbMed";
            this.chbMed.Size = new System.Drawing.Size(176, 40);
            this.chbMed.TabIndex = 4;
            this.chbMed.Text = "Binomial Median";
            this.chbMed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chbMed.UseVisualStyleBackColor = true;
            this.chbMed.CheckedChanged += new System.EventHandler(this.chbMed_CheckedChanged);
            this.chbMed.MouseLeave += new System.EventHandler(this.chbMed_MouseLeave);
            this.chbMed.MouseHover += new System.EventHandler(this.chbMed_MouseHover);
            // 
            // chbGauss
            // 
            this.chbGauss.Appearance = System.Windows.Forms.Appearance.Button;
            this.chbGauss.Checked = true;
            this.chbGauss.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbGauss.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.chbGauss.Location = new System.Drawing.Point(13, 142);
            this.chbGauss.Name = "chbGauss";
            this.chbGauss.Size = new System.Drawing.Size(176, 40);
            this.chbGauss.TabIndex = 5;
            this.chbGauss.Text = "Gaussian Filtering";
            this.chbGauss.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chbGauss.UseVisualStyleBackColor = true;
            this.chbGauss.CheckedChanged += new System.EventHandler(this.chbGauss_CheckedChanged);
            this.chbGauss.MouseLeave += new System.EventHandler(this.chbGauss_MouseLeave);
            this.chbGauss.MouseHover += new System.EventHandler(this.chbGauss_MouseHover);
            // 
            // btnSetDefault
            // 
            this.btnSetDefault.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSetDefault.Location = new System.Drawing.Point(10, 374);
            this.btnSetDefault.Name = "btnSetDefault";
            this.btnSetDefault.Size = new System.Drawing.Size(85, 28);
            this.btnSetDefault.TabIndex = 38;
            this.btnSetDefault.Text = "";
            this.btnSetDefault.UseVisualStyleBackColor = true;
            this.btnSetDefault.Click += new System.EventHandler(this.btnSetDefault_Click);
            this.btnSetDefault.MouseLeave += new System.EventHandler(this.btnSetDefault_MouseLeave);
            this.btnSetDefault.MouseHover += new System.EventHandler(this.btnSetDefault_MouseHover);
            // 
            // btnSave
            // 
            this.btnSave.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(641, 374);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(85, 28);
            this.btnSave.TabIndex = 36;
            this.btnSave.Text = "";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            this.btnSave.MouseLeave += new System.EventHandler(this.btnSave_MouseLeave);
            this.btnSave.MouseHover += new System.EventHandler(this.btnSave_MouseHover);
            // 
            // btnCancel
            // 
            this.btnCancel.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(732, 374);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(62, 28);
            this.btnCancel.TabIndex = 37;
            this.btnCancel.Text = "";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            this.btnCancel.MouseLeave += new System.EventHandler(this.btnCancel_MouseLeave);
            this.btnCancel.MouseHover += new System.EventHandler(this.btnCancel_MouseHover);
            // 
            // FrmExportSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(806, 436);
            this.Controls.Add(this.btnSetDefault);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.gbSmoothMtd);
            this.Controls.Add(this.gbSmoothParams);
            this.Controls.Add(this.statStripExportConfig);
            this.Controls.Add(this.gbExportOpts);
            this.Controls.Add(this.lblExportConfigTtl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmExportSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Export Configuration";
            this.Load += new System.EventHandler(this.FrmExportSettings_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrmExportSettings_KeyDown);
            this.MouseHover += new System.EventHandler(this.FrmExportSettings_MouseHover);
            this.gbExportOpts.ResumeLayout(false);
            this.gbExportOpts.PerformLayout();
            this.statStripExportConfig.ResumeLayout(false);
            this.statStripExportConfig.PerformLayout();
            this.gbSmoothParams.ResumeLayout(false);
            this.gbSmoothMtd.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblExportConfigTtl;
        private System.Windows.Forms.GroupBox gbExportOpts;
        public System.Windows.Forms.RadioButton rbtnCSV;
        public System.Windows.Forms.RadioButton rbtnXLSX;
        public System.Windows.Forms.CheckBox chbOpenFile;
        private System.Windows.Forms.StatusStrip statStripExportConfig;
        private System.Windows.Forms.ToolStripStatusLabel slblDesc;
        private System.Windows.Forms.GroupBox gbSmoothParams;
        private System.Windows.Forms.Label lblAlpha;
        public System.Windows.Forms.ComboBox cbxAlpha;
        private System.Windows.Forms.Label lblDerivOrder;
        public System.Windows.Forms.ComboBox cbxDerivOrder;
        public System.Windows.Forms.ComboBox cbxBoundaryMethod;
        private System.Windows.Forms.Label lblBoundaryMethod;
        private System.Windows.Forms.Label lblPolyOrder;
        public System.Windows.Forms.ComboBox cbxPolyOrder;
        public System.Windows.Forms.ComboBox cbxKernelRadius;
        private System.Windows.Forms.Label lblKernelRadius;
        private System.Windows.Forms.GroupBox gbSmoothMtd;
        public System.Windows.Forms.CheckBox chbRect;
        public System.Windows.Forms.CheckBox chbAvg;
        public System.Windows.Forms.CheckBox chbSG;
        public System.Windows.Forms.CheckBox chbMed;
        public System.Windows.Forms.CheckBox chbGauss;
        public System.Windows.Forms.CheckBox chbGaussMed;
        internal System.Windows.Forms.Button btnSetDefault;
        internal System.Windows.Forms.Button btnSave;
        internal System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblKernelWidth;
        public System.Windows.Forms.ComboBox cbxSigmaFactor;
        private System.Windows.Forms.Label lblSigmaFactor;
    }
}