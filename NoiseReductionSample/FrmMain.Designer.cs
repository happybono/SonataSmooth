namespace NoiseReductionSample
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.rbtnAvg = new System.Windows.Forms.RadioButton();
            this.rbtnMed = new System.Windows.Forms.RadioButton();
            this.btnCalibrate = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.txtVariable = new System.Windows.Forms.TextBox();
            this.lblCnt1 = new System.Windows.Forms.Label();
            this.lblCnt2 = new System.Windows.Forms.Label();
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.AllowDrop = true;
            this.listBox1.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 10.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 17;
            this.listBox1.Location = new System.Drawing.Point(36, 25);
            this.listBox1.Margin = new System.Windows.Forms.Padding(2);
            this.listBox1.Name = "listBox1";
            this.listBox1.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBox1.Size = new System.Drawing.Size(276, 412);
            this.listBox1.TabIndex = 0;
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
            this.listBox2.Location = new System.Drawing.Point(36, 25);
            this.listBox2.Margin = new System.Windows.Forms.Padding(2);
            this.listBox2.Name = "listBox2";
            this.listBox2.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBox2.Size = new System.Drawing.Size(276, 412);
            this.listBox2.TabIndex = 1;
            this.listBox2.SelectedIndexChanged += new System.EventHandler(this.listBox2_SelectedIndexChanged);
            this.listBox2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBox2_KeyDown);
            // 
            // rbtnAvg
            // 
            this.rbtnAvg.AutoSize = true;
            this.rbtnAvg.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbtnAvg.Location = new System.Drawing.Point(327, 38);
            this.rbtnAvg.Margin = new System.Windows.Forms.Padding(2);
            this.rbtnAvg.Name = "rbtnAvg";
            this.rbtnAvg.Size = new System.Drawing.Size(77, 23);
            this.rbtnAvg.TabIndex = 2;
            this.rbtnAvg.Text = "Average";
            this.rbtnAvg.UseVisualStyleBackColor = true;
            // 
            // rbtnMed
            // 
            this.rbtnMed.AutoSize = true;
            this.rbtnMed.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbtnMed.Location = new System.Drawing.Point(411, 38);
            this.rbtnMed.Margin = new System.Windows.Forms.Padding(2);
            this.rbtnMed.Name = "rbtnMed";
            this.rbtnMed.Size = new System.Drawing.Size(72, 23);
            this.rbtnMed.TabIndex = 3;
            this.rbtnMed.Text = "Median";
            this.rbtnMed.UseVisualStyleBackColor = true;
            // 
            // btnCalibrate
            // 
            this.btnCalibrate.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCalibrate.Location = new System.Drawing.Point(13, 721);
            this.btnCalibrate.Margin = new System.Windows.Forms.Padding(2);
            this.btnCalibrate.Name = "btnCalibrate";
            this.btnCalibrate.Size = new System.Drawing.Size(707, 40);
            this.btnCalibrate.TabIndex = 4;
            this.btnCalibrate.Text = "Calibrate";
            this.btnCalibrate.UseVisualStyleBackColor = true;
            this.btnCalibrate.Click += new System.EventHandler(this.btnCalibrate_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAdd.Location = new System.Drawing.Point(292, 14);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(2);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(67, 28);
            this.btnAdd.TabIndex = 5;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // txtVariable
            // 
            this.txtVariable.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 10.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtVariable.Location = new System.Drawing.Point(26, 16);
            this.txtVariable.Margin = new System.Windows.Forms.Padding(2);
            this.txtVariable.Name = "txtVariable";
            this.txtVariable.Size = new System.Drawing.Size(262, 25);
            this.txtVariable.TabIndex = 6;
            this.txtVariable.TextChanged += new System.EventHandler(this.txtVariable_TextChanged);
            this.txtVariable.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtVariable_KeyDown);
            // 
            // lblCnt1
            // 
            this.lblCnt1.AutoSize = true;
            this.lblCnt1.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCnt1.Location = new System.Drawing.Point(32, 556);
            this.lblCnt1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCnt1.Name = "lblCnt1";
            this.lblCnt1.Size = new System.Drawing.Size(65, 19);
            this.lblCnt1.TabIndex = 7;
            this.lblCnt1.Text = "Count : 0";
            // 
            // lblCnt2
            // 
            this.lblCnt2.AutoSize = true;
            this.lblCnt2.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCnt2.Location = new System.Drawing.Point(32, 556);
            this.lblCnt2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCnt2.Name = "lblCnt2";
            this.lblCnt2.Size = new System.Drawing.Size(65, 19);
            this.lblCnt2.TabIndex = 8;
            this.lblCnt2.Text = "Count : 0";
            // 
            // rbtnRect
            // 
            this.rbtnRect.AutoSize = true;
            this.rbtnRect.Checked = true;
            this.rbtnRect.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbtnRect.Location = new System.Drawing.Point(221, 38);
            this.rbtnRect.Margin = new System.Windows.Forms.Padding(2);
            this.rbtnRect.Name = "rbtnRect";
            this.rbtnRect.Size = new System.Drawing.Size(100, 23);
            this.rbtnRect.TabIndex = 9;
            this.rbtnRect.TabStop = true;
            this.rbtnRect.Text = "Rectangular";
            this.rbtnRect.UseVisualStyleBackColor = true;
            // 
            // btnClear
            // 
            this.btnClear.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClear.Location = new System.Drawing.Point(36, 449);
            this.btnClear.Margin = new System.Windows.Forms.Padding(2);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(133, 30);
            this.btnClear.TabIndex = 10;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDelete.Location = new System.Drawing.Point(179, 449);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(2);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(133, 30);
            this.btnDelete.TabIndex = 11;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCopy.Location = new System.Drawing.Point(36, 482);
            this.btnCopy.Margin = new System.Windows.Forms.Padding(2);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(133, 30);
            this.btnCopy.TabIndex = 12;
            this.btnCopy.Text = "Copy";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnPaste
            // 
            this.btnPaste.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPaste.Location = new System.Drawing.Point(179, 482);
            this.btnPaste.Margin = new System.Windows.Forms.Padding(2);
            this.btnPaste.Name = "btnPaste";
            this.btnPaste.Size = new System.Drawing.Size(133, 30);
            this.btnPaste.TabIndex = 13;
            this.btnPaste.Text = "Paste";
            this.btnPaste.UseVisualStyleBackColor = true;
            this.btnPaste.Click += new System.EventHandler(this.btnPaste_Click);
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelectAll.Location = new System.Drawing.Point(36, 515);
            this.btnSelectAll.Margin = new System.Windows.Forms.Padding(2);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(133, 30);
            this.btnSelectAll.TabIndex = 14;
            this.btnSelectAll.Text = "Select All";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // btnSelClear
            // 
            this.btnSelClear.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelClear.Location = new System.Drawing.Point(179, 515);
            this.btnSelClear.Margin = new System.Windows.Forms.Padding(2);
            this.btnSelClear.Name = "btnSelClear";
            this.btnSelClear.Size = new System.Drawing.Size(133, 30);
            this.btnSelClear.TabIndex = 15;
            this.btnSelClear.Text = "Selection Clear";
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
            "12"});
            this.cbxKernelWidth.Location = new System.Drawing.Point(412, 11);
            this.cbxKernelWidth.Margin = new System.Windows.Forms.Padding(2);
            this.cbxKernelWidth.Name = "cbxKernelWidth";
            this.cbxKernelWidth.Size = new System.Drawing.Size(80, 25);
            this.cbxKernelWidth.TabIndex = 16;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(212, 14);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(198, 19);
            this.label1.TabIndex = 17;
            this.label1.Text = "Noise Reduction Kernel Width : ";
            // 
            // btnSelClear2
            // 
            this.btnSelClear2.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelClear2.Location = new System.Drawing.Point(179, 513);
            this.btnSelClear2.Margin = new System.Windows.Forms.Padding(2);
            this.btnSelClear2.Name = "btnSelClear2";
            this.btnSelClear2.Size = new System.Drawing.Size(133, 30);
            this.btnSelClear2.TabIndex = 23;
            this.btnSelClear2.Text = "Selection Clear";
            this.btnSelClear2.UseVisualStyleBackColor = true;
            this.btnSelClear2.Click += new System.EventHandler(this.btnSelectClear2_Click);
            // 
            // btnSelectAll2
            // 
            this.btnSelectAll2.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelectAll2.Location = new System.Drawing.Point(36, 513);
            this.btnSelectAll2.Margin = new System.Windows.Forms.Padding(2);
            this.btnSelectAll2.Name = "btnSelectAll2";
            this.btnSelectAll2.Size = new System.Drawing.Size(133, 30);
            this.btnSelectAll2.TabIndex = 22;
            this.btnSelectAll2.Text = "Select All";
            this.btnSelectAll2.UseVisualStyleBackColor = true;
            this.btnSelectAll2.Click += new System.EventHandler(this.btnSelectAll2_Click);
            // 
            // btnCopy2
            // 
            this.btnCopy2.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCopy2.Location = new System.Drawing.Point(36, 480);
            this.btnCopy2.Margin = new System.Windows.Forms.Padding(2);
            this.btnCopy2.Name = "btnCopy2";
            this.btnCopy2.Size = new System.Drawing.Size(276, 30);
            this.btnCopy2.TabIndex = 20;
            this.btnCopy2.Text = "Copy";
            this.btnCopy2.UseVisualStyleBackColor = true;
            this.btnCopy2.Click += new System.EventHandler(this.btnCopy2_Click);
            // 
            // btnClear2
            // 
            this.btnClear2.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClear2.Location = new System.Drawing.Point(36, 447);
            this.btnClear2.Margin = new System.Windows.Forms.Padding(2);
            this.btnClear2.Name = "btnClear2";
            this.btnClear2.Size = new System.Drawing.Size(276, 30);
            this.btnClear2.TabIndex = 18;
            this.btnClear2.Text = "Clear";
            this.btnClear2.UseVisualStyleBackColor = true;
            this.btnClear2.Click += new System.EventHandler(this.btnClear2_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbtnRect);
            this.groupBox1.Controls.Add(this.cbxKernelWidth);
            this.groupBox1.Controls.Add(this.rbtnAvg);
            this.groupBox1.Controls.Add(this.rbtnMed);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(13, 645);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(707, 72);
            this.groupBox1.TabIndex = 24;
            this.groupBox1.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.progressBar1);
            this.groupBox2.Controls.Add(this.listBox1);
            this.groupBox2.Controls.Add(this.lblCnt1);
            this.groupBox2.Controls.Add(this.btnClear);
            this.groupBox2.Controls.Add(this.btnDelete);
            this.groupBox2.Controls.Add(this.btnCopy);
            this.groupBox2.Controls.Add(this.btnPaste);
            this.groupBox2.Controls.Add(this.btnSelClear);
            this.groupBox2.Controls.Add(this.btnSelectAll);
            this.groupBox2.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(15, 54);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(344, 586);
            this.groupBox2.TabIndex = 25;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Original Data";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(36, 437);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(276, 4);
            this.progressBar1.TabIndex = 16;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.progressBar2);
            this.groupBox3.Controls.Add(this.listBox2);
            this.groupBox3.Controls.Add(this.lblCnt2);
            this.groupBox3.Controls.Add(this.btnClear2);
            this.groupBox3.Controls.Add(this.btnSelClear2);
            this.groupBox3.Controls.Add(this.btnCopy2);
            this.groupBox3.Controls.Add(this.btnSelectAll2);
            this.groupBox3.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(376, 54);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(344, 586);
            this.groupBox3.TabIndex = 26;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Calibrated Data";
            // 
            // progressBar2
            // 
            this.progressBar2.Location = new System.Drawing.Point(36, 437);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(276, 4);
            this.progressBar2.TabIndex = 17;
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(734, 768);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.txtVariable);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnCalibrate);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.Name = "FrmMain";
            this.Text = "Sonata Smooth";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.RadioButton rbtnAvg;
        private System.Windows.Forms.RadioButton rbtnMed;
        private System.Windows.Forms.Button btnCalibrate;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.TextBox txtVariable;
        private System.Windows.Forms.Label lblCnt1;
        private System.Windows.Forms.Label lblCnt2;
        private System.Windows.Forms.RadioButton rbtnRect;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Button btnPaste;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Button btnSelClear;
        private System.Windows.Forms.ComboBox cbxKernelWidth;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSelClear2;
        private System.Windows.Forms.Button btnSelectAll2;
        private System.Windows.Forms.Button btnCopy2;
        private System.Windows.Forms.Button btnClear2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.ProgressBar progressBar2;
    }
}

