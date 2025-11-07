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
            this.chbRect = new System.Windows.Forms.CheckBox();
            this.chbAvg = new System.Windows.Forms.CheckBox();
            this.chbMed = new System.Windows.Forms.CheckBox();
            this.chbGauss = new System.Windows.Forms.CheckBox();
            this.chbSG = new System.Windows.Forms.CheckBox();
            this.gbSmoothParams = new System.Windows.Forms.GroupBox();
            this.lblDerivOrder = new System.Windows.Forms.Label();
            this.cbxDerivOrder = new System.Windows.Forms.ComboBox();
            this.cbxBoundaryMethod = new System.Windows.Forms.ComboBox();
            this.lblBoundaryMethod = new System.Windows.Forms.Label();
            this.lblPolyOrder = new System.Windows.Forms.Label();
            this.cbxPolyOrder = new System.Windows.Forms.ComboBox();
            this.cbxKernelRadius = new System.Windows.Forms.ComboBox();
            this.lblKernelRadius = new System.Windows.Forms.Label();
            this.gbSmoothMtd = new System.Windows.Forms.GroupBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.lblExportConfigTtl = new System.Windows.Forms.Label();
            this.gbExportOpts = new System.Windows.Forms.GroupBox();
            this.chbOpenFile = new System.Windows.Forms.CheckBox();
            this.rbtnCSV = new System.Windows.Forms.RadioButton();
            this.rbtnXLSX = new System.Windows.Forms.RadioButton();
            this.statStripExportConfig = new System.Windows.Forms.StatusStrip();
            this.slblDesc = new System.Windows.Forms.ToolStripStatusLabel();
            this.gbSmoothParams.SuspendLayout();
            this.gbSmoothMtd.SuspendLayout();
            this.gbExportOpts.SuspendLayout();
            this.statStripExportConfig.SuspendLayout();
            this.SuspendLayout();
            // 
            // chbRect
            // 
            this.chbRect.Appearance = System.Windows.Forms.Appearance.Button;
            this.chbRect.Checked = true;
            this.chbRect.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbRect.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.chbRect.Location = new System.Drawing.Point(6, 46);
            this.chbRect.Name = "chbRect";
            this.chbRect.Size = new System.Drawing.Size(163, 30);
            this.chbRect.TabIndex = 1;
            this.chbRect.Text = "Rectangular Averaging";
            this.chbRect.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chbRect.UseVisualStyleBackColor = true;
            this.chbRect.MouseLeave += new System.EventHandler(this.chbRect_MouseLeave);
            this.chbRect.MouseHover += new System.EventHandler(this.chbRect_MouseHover);
            // 
            // chbAvg
            // 
            this.chbAvg.Appearance = System.Windows.Forms.Appearance.Button;
            this.chbAvg.Checked = true;
            this.chbAvg.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbAvg.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.chbAvg.Location = new System.Drawing.Point(175, 46);
            this.chbAvg.Name = "chbAvg";
            this.chbAvg.Size = new System.Drawing.Size(163, 30);
            this.chbAvg.TabIndex = 2;
            this.chbAvg.Text = "Binomial Averaging";
            this.chbAvg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chbAvg.UseVisualStyleBackColor = true;
            this.chbAvg.MouseLeave += new System.EventHandler(this.chbAvg_MouseLeave);
            this.chbAvg.MouseHover += new System.EventHandler(this.chbAvg_MouseHover);
            // 
            // chbMed
            // 
            this.chbMed.Appearance = System.Windows.Forms.Appearance.Button;
            this.chbMed.Checked = true;
            this.chbMed.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbMed.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.chbMed.Location = new System.Drawing.Point(7, 80);
            this.chbMed.Name = "chbMed";
            this.chbMed.Size = new System.Drawing.Size(331, 30);
            this.chbMed.TabIndex = 3;
            this.chbMed.Text = "Binomial Median Filtering";
            this.chbMed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chbMed.UseVisualStyleBackColor = true;
            this.chbMed.MouseLeave += new System.EventHandler(this.chbMed_MouseLeave);
            this.chbMed.MouseHover += new System.EventHandler(this.chbMed_MouseHover);
            // 
            // chbGauss
            // 
            this.chbGauss.Appearance = System.Windows.Forms.Appearance.Button;
            this.chbGauss.Checked = true;
            this.chbGauss.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbGauss.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.chbGauss.Location = new System.Drawing.Point(7, 114);
            this.chbGauss.Name = "chbGauss";
            this.chbGauss.Size = new System.Drawing.Size(163, 30);
            this.chbGauss.TabIndex = 4;
            this.chbGauss.Text = "Gaussian Filtering";
            this.chbGauss.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chbGauss.UseVisualStyleBackColor = true;
            this.chbGauss.MouseLeave += new System.EventHandler(this.chbGauss_MouseLeave);
            this.chbGauss.MouseHover += new System.EventHandler(this.chbGauss_MouseHover);
            // 
            // chbSG
            // 
            this.chbSG.Appearance = System.Windows.Forms.Appearance.Button;
            this.chbSG.Checked = true;
            this.chbSG.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbSG.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.chbSG.Location = new System.Drawing.Point(175, 114);
            this.chbSG.Name = "chbSG";
            this.chbSG.Size = new System.Drawing.Size(163, 30);
            this.chbSG.TabIndex = 5;
            this.chbSG.Text = "Savitzky-Golay Filtering";
            this.chbSG.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chbSG.UseVisualStyleBackColor = true;
            this.chbSG.CheckedChanged += new System.EventHandler(this.chbSG_CheckedChanged);
            this.chbSG.MouseLeave += new System.EventHandler(this.chbSG_MouseLeave);
            this.chbSG.MouseHover += new System.EventHandler(this.chbSG_MouseHover);
            // 
            // gbSmoothParams
            // 
            this.gbSmoothParams.Controls.Add(this.lblDerivOrder);
            this.gbSmoothParams.Controls.Add(this.cbxDerivOrder);
            this.gbSmoothParams.Controls.Add(this.cbxBoundaryMethod);
            this.gbSmoothParams.Controls.Add(this.lblBoundaryMethod);
            this.gbSmoothParams.Controls.Add(this.lblPolyOrder);
            this.gbSmoothParams.Controls.Add(this.cbxPolyOrder);
            this.gbSmoothParams.Controls.Add(this.cbxKernelRadius);
            this.gbSmoothParams.Controls.Add(this.lblKernelRadius);
            this.gbSmoothParams.Font = new System.Drawing.Font("Segoe UI Variable Display Semil", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbSmoothParams.Location = new System.Drawing.Point(372, 48);
            this.gbSmoothParams.Name = "gbSmoothParams";
            this.gbSmoothParams.Size = new System.Drawing.Size(344, 173);
            this.gbSmoothParams.TabIndex = 23;
            this.gbSmoothParams.TabStop = false;
            this.gbSmoothParams.Text = "Smoothing Parameters";
            // 
            // lblDerivOrder
            // 
            this.lblDerivOrder.Enabled = false;
            this.lblDerivOrder.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.lblDerivOrder.Location = new System.Drawing.Point(18, 132);
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
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.cbxDerivOrder.Location = new System.Drawing.Point(223, 130);
            this.cbxDerivOrder.Name = "cbxDerivOrder";
            this.cbxDerivOrder.Size = new System.Drawing.Size(103, 25);
            this.cbxDerivOrder.TabIndex = 33;
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
            this.cbxBoundaryMethod.Location = new System.Drawing.Point(223, 68);
            this.cbxBoundaryMethod.Name = "cbxBoundaryMethod";
            this.cbxBoundaryMethod.Size = new System.Drawing.Size(103, 25);
            this.cbxBoundaryMethod.TabIndex = 31;
            this.cbxBoundaryMethod.MouseLeave += new System.EventHandler(this.cbxBoundaryMethod_MouseLeave);
            this.cbxBoundaryMethod.MouseHover += new System.EventHandler(this.cbxBoundaryMethod_MouseHover);
            // 
            // lblBoundaryMethod
            // 
            this.lblBoundaryMethod.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.lblBoundaryMethod.Location = new System.Drawing.Point(18, 71);
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
            this.lblPolyOrder.Location = new System.Drawing.Point(18, 101);
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
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.cbxPolyOrder.Location = new System.Drawing.Point(223, 99);
            this.cbxPolyOrder.Name = "cbxPolyOrder";
            this.cbxPolyOrder.Size = new System.Drawing.Size(103, 25);
            this.cbxPolyOrder.TabIndex = 29;
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
            "25"});
            this.cbxKernelRadius.Location = new System.Drawing.Point(223, 37);
            this.cbxKernelRadius.Margin = new System.Windows.Forms.Padding(2);
            this.cbxKernelRadius.Name = "cbxKernelRadius";
            this.cbxKernelRadius.Size = new System.Drawing.Size(103, 25);
            this.cbxKernelRadius.TabIndex = 28;
            this.cbxKernelRadius.MouseLeave += new System.EventHandler(this.cbxKernelRadius_MouseLeave);
            this.cbxKernelRadius.MouseHover += new System.EventHandler(this.cbxKernelRadius_MouseHover);
            // 
            // lblKernelRadius
            // 
            this.lblKernelRadius.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblKernelRadius.Location = new System.Drawing.Point(18, 39);
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
            this.gbSmoothMtd.Controls.Add(this.chbRect);
            this.gbSmoothMtd.Controls.Add(this.chbAvg);
            this.gbSmoothMtd.Controls.Add(this.chbSG);
            this.gbSmoothMtd.Controls.Add(this.chbMed);
            this.gbSmoothMtd.Controls.Add(this.chbGauss);
            this.gbSmoothMtd.Font = new System.Drawing.Font("Segoe UI Variable Display Semil", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbSmoothMtd.Location = new System.Drawing.Point(11, 48);
            this.gbSmoothMtd.Name = "gbSmoothMtd";
            this.gbSmoothMtd.Size = new System.Drawing.Size(344, 173);
            this.gbSmoothMtd.TabIndex = 24;
            this.gbSmoothMtd.TabStop = false;
            this.gbSmoothMtd.Text = "Smoothing Methods";
            // 
            // btnCancel
            // 
            this.btnCancel.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(654, 318);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(62, 24);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            this.btnCancel.MouseLeave += new System.EventHandler(this.btnCancel_MouseLeave);
            this.btnCancel.MouseHover += new System.EventHandler(this.btnCancel_MouseHover);
            // 
            // btnSave
            // 
            this.btnSave.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(563, 318);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(85, 24);
            this.btnSave.TabIndex = 11;
            this.btnSave.Text = "";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            this.btnSave.MouseLeave += new System.EventHandler(this.btnSave_MouseLeave);
            this.btnSave.MouseHover += new System.EventHandler(this.btnSave_MouseHover);
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
            this.gbExportOpts.Location = new System.Drawing.Point(11, 227);
            this.gbExportOpts.Name = "gbExportOpts";
            this.gbExportOpts.Size = new System.Drawing.Size(705, 85);
            this.gbExportOpts.TabIndex = 32;
            this.gbExportOpts.TabStop = false;
            this.gbExportOpts.Text = "Data Export Options";
            // 
            // chbOpenFile
            // 
            this.chbOpenFile.AutoSize = true;
            this.chbOpenFile.Checked = true;
            this.chbOpenFile.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbOpenFile.Font = new System.Drawing.Font("Segoe UI Variable Display", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chbOpenFile.Location = new System.Drawing.Point(208, 58);
            this.chbOpenFile.Name = "chbOpenFile";
            this.chbOpenFile.Size = new System.Drawing.Size(289, 21);
            this.chbOpenFile.TabIndex = 10;
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
            this.rbtnCSV.Location = new System.Drawing.Point(385, 21);
            this.rbtnCSV.Name = "rbtnCSV";
            this.rbtnCSV.Size = new System.Drawing.Size(163, 30);
            this.rbtnCSV.TabIndex = 9;
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
            this.rbtnXLSX.Location = new System.Drawing.Point(157, 21);
            this.rbtnXLSX.Name = "rbtnXLSX";
            this.rbtnXLSX.Size = new System.Drawing.Size(163, 30);
            this.rbtnXLSX.TabIndex = 8;
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
            this.statStripExportConfig.Location = new System.Drawing.Point(0, 351);
            this.statStripExportConfig.Name = "statStripExportConfig";
            this.statStripExportConfig.Size = new System.Drawing.Size(726, 24);
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
            this.slblDesc.Size = new System.Drawing.Size(724, 19);
            this.slblDesc.Text = "To save the settings, please select the desired options and click the \'Save\' butt" +
    "on.";
            // 
            // FrmExportSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(726, 375);
            this.Controls.Add(this.statStripExportConfig);
            this.Controls.Add(this.gbExportOpts);
            this.Controls.Add(this.lblExportConfigTtl);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.gbSmoothMtd);
            this.Controls.Add(this.gbSmoothParams);
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
            this.gbSmoothParams.ResumeLayout(false);
            this.gbSmoothMtd.ResumeLayout(false);
            this.gbExportOpts.ResumeLayout(false);
            this.gbExportOpts.PerformLayout();
            this.statStripExportConfig.ResumeLayout(false);
            this.statStripExportConfig.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox gbSmoothParams;
        private System.Windows.Forms.GroupBox gbSmoothMtd;
        internal System.Windows.Forms.Button btnCancel;
        internal System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblExportConfigTtl;
        public System.Windows.Forms.CheckBox chbRect;
        public System.Windows.Forms.CheckBox chbAvg;
        public System.Windows.Forms.CheckBox chbMed;
        public System.Windows.Forms.CheckBox chbGauss;
        public System.Windows.Forms.CheckBox chbSG;
        private System.Windows.Forms.GroupBox gbExportOpts;
        public System.Windows.Forms.RadioButton rbtnCSV;
        public System.Windows.Forms.RadioButton rbtnXLSX;
        public System.Windows.Forms.CheckBox chbOpenFile;
        private System.Windows.Forms.StatusStrip statStripExportConfig;
        private System.Windows.Forms.ToolStripStatusLabel slblDesc;
        private System.Windows.Forms.Label lblDerivOrder;
        private System.Windows.Forms.Label lblBoundaryMethod;
        private System.Windows.Forms.Label lblPolyOrder;
        public System.Windows.Forms.ComboBox cbxKernelRadius;
        private System.Windows.Forms.Label lblKernelRadius;
        public System.Windows.Forms.ComboBox cbxDerivOrder;
        public System.Windows.Forms.ComboBox cbxBoundaryMethod;
        public System.Windows.Forms.ComboBox cbxPolyOrder;
    }
}