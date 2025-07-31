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
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.lblPolyOrder = new System.Windows.Forms.Label();
            this.cbxPolyOrder = new System.Windows.Forms.ComboBox();
            this.cbxKernelWidth = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbtnCSV = new System.Windows.Forms.RadioButton();
            this.rbtnXLSX = new System.Windows.Forms.RadioButton();
            this.chbOpenFile = new System.Windows.Forms.CheckBox();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // chbRect
            // 
            this.chbRect.Appearance = System.Windows.Forms.Appearance.Button;
            this.chbRect.Checked = true;
            this.chbRect.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbRect.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.chbRect.Location = new System.Drawing.Point(6, 25);
            this.chbRect.Name = "chbRect";
            this.chbRect.Size = new System.Drawing.Size(163, 30);
            this.chbRect.TabIndex = 0;
            this.chbRect.Text = "Rectangular Averaging";
            this.chbRect.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chbRect.UseVisualStyleBackColor = true;
            // 
            // chbAvg
            // 
            this.chbAvg.Appearance = System.Windows.Forms.Appearance.Button;
            this.chbAvg.Checked = true;
            this.chbAvg.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbAvg.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.chbAvg.Location = new System.Drawing.Point(175, 25);
            this.chbAvg.Name = "chbAvg";
            this.chbAvg.Size = new System.Drawing.Size(163, 30);
            this.chbAvg.TabIndex = 1;
            this.chbAvg.Text = "Binomial Averaging";
            this.chbAvg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chbAvg.UseVisualStyleBackColor = true;
            // 
            // chbMed
            // 
            this.chbMed.Appearance = System.Windows.Forms.Appearance.Button;
            this.chbMed.Checked = true;
            this.chbMed.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbMed.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.chbMed.Location = new System.Drawing.Point(7, 59);
            this.chbMed.Name = "chbMed";
            this.chbMed.Size = new System.Drawing.Size(331, 30);
            this.chbMed.TabIndex = 2;
            this.chbMed.Text = "Binomial Median Filtering";
            this.chbMed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chbMed.UseVisualStyleBackColor = true;
            // 
            // chbGauss
            // 
            this.chbGauss.Appearance = System.Windows.Forms.Appearance.Button;
            this.chbGauss.Checked = true;
            this.chbGauss.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbGauss.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.chbGauss.Location = new System.Drawing.Point(7, 93);
            this.chbGauss.Name = "chbGauss";
            this.chbGauss.Size = new System.Drawing.Size(163, 30);
            this.chbGauss.TabIndex = 3;
            this.chbGauss.Text = "Gaussian Filtering";
            this.chbGauss.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chbGauss.UseVisualStyleBackColor = true;
            // 
            // chbSG
            // 
            this.chbSG.Appearance = System.Windows.Forms.Appearance.Button;
            this.chbSG.Checked = true;
            this.chbSG.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbSG.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F);
            this.chbSG.Location = new System.Drawing.Point(175, 93);
            this.chbSG.Name = "chbSG";
            this.chbSG.Size = new System.Drawing.Size(163, 30);
            this.chbSG.TabIndex = 4;
            this.chbSG.Text = "Savitzky-Golay Filtering";
            this.chbSG.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chbSG.UseVisualStyleBackColor = true;
            this.chbSG.CheckedChanged += new System.EventHandler(this.chbSG_CheckedChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.lblPolyOrder);
            this.groupBox5.Controls.Add(this.cbxPolyOrder);
            this.groupBox5.Controls.Add(this.cbxKernelWidth);
            this.groupBox5.Controls.Add(this.label1);
            this.groupBox5.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox5.Location = new System.Drawing.Point(372, 45);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(344, 130);
            this.groupBox5.TabIndex = 23;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Signal Smoothing Parameters";
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
            this.cbxPolyOrder.TabIndex = 17;
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
            this.cbxKernelWidth.TabIndex = 16;
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
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.chbRect);
            this.groupBox4.Controls.Add(this.chbAvg);
            this.groupBox4.Controls.Add(this.chbSG);
            this.groupBox4.Controls.Add(this.chbMed);
            this.groupBox4.Controls.Add(this.chbGauss);
            this.groupBox4.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 11.25F, System.Drawing.FontStyle.Bold);
            this.groupBox4.Location = new System.Drawing.Point(11, 45);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(344, 130);
            this.groupBox4.TabIndex = 24;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Calibration Method";
            // 
            // btnCancel
            // 
            this.btnCancel.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(654, 272);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(62, 24);
            this.btnCancel.TabIndex = 25;
            this.btnCancel.Text = "";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSave
            // 
            this.btnSave.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(563, 272);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(85, 24);
            this.btnSave.TabIndex = 26;
            this.btnSave.Text = "";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(6, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(508, 26);
            this.label2.TabIndex = 31;
            this.label2.Text = "Export Configuration : Calibration and Smoothing Settings";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chbOpenFile);
            this.groupBox1.Controls.Add(this.rbtnCSV);
            this.groupBox1.Controls.Add(this.rbtnXLSX);
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 11.25F, System.Drawing.FontStyle.Bold);
            this.groupBox1.Location = new System.Drawing.Point(11, 181);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(705, 85);
            this.groupBox1.TabIndex = 32;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Data Export Options";
            // 
            // rbtnCSV
            // 
            this.rbtnCSV.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbtnCSV.Font = new System.Drawing.Font("Segoe UI Variable Display", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbtnCSV.Location = new System.Drawing.Point(385, 21);
            this.rbtnCSV.Name = "rbtnCSV";
            this.rbtnCSV.Size = new System.Drawing.Size(163, 30);
            this.rbtnCSV.TabIndex = 1;
            this.rbtnCSV.TabStop = true;
            this.rbtnCSV.Text = "Export as CSV";
            this.rbtnCSV.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbtnCSV.UseVisualStyleBackColor = true;
            // 
            // rbtnXLSX
            // 
            this.rbtnXLSX.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbtnXLSX.Font = new System.Drawing.Font("Segoe UI Variable Display", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbtnXLSX.Location = new System.Drawing.Point(157, 21);
            this.rbtnXLSX.Name = "rbtnXLSX";
            this.rbtnXLSX.Size = new System.Drawing.Size(163, 30);
            this.rbtnXLSX.TabIndex = 0;
            this.rbtnXLSX.TabStop = true;
            this.rbtnXLSX.Text = "Open in Excel";
            this.rbtnXLSX.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbtnXLSX.UseVisualStyleBackColor = true;
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
            this.chbOpenFile.TabIndex = 2;
            this.chbOpenFile.Text = "Open the saved file automatically after saving.";
            this.chbOpenFile.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chbOpenFile.UseVisualStyleBackColor = true;
            // 
            // FrmExportSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(726, 309);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox5);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmExportSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Export Configuration";
            this.Load += new System.EventHandler(this.FrmExportSettings_Load);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label lblPolyOrder;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox4;
        internal System.Windows.Forms.Button btnCancel;
        internal System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.ComboBox cbxPolyOrder;
        public System.Windows.Forms.ComboBox cbxKernelWidth;
        public System.Windows.Forms.CheckBox chbRect;
        public System.Windows.Forms.CheckBox chbAvg;
        public System.Windows.Forms.CheckBox chbMed;
        public System.Windows.Forms.CheckBox chbGauss;
        public System.Windows.Forms.CheckBox chbSG;
        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.RadioButton rbtnCSV;
        public System.Windows.Forms.RadioButton rbtnXLSX;
        public System.Windows.Forms.CheckBox chbOpenFile;
    }
}