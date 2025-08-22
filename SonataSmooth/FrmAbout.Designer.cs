namespace SonataSmooth
{
    partial class FrmAbout
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmAbout));
            this.sstripAbout = new System.Windows.Forms.StatusStrip();
            this.slblCopyright = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblLicenseTerms = new System.Windows.Forms.Label();
            this.txtLicenseTerms = new System.Windows.Forms.TextBox();
            this.lblAppVersion = new System.Windows.Forms.Label();
            this.lblAppTtl = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnDonation = new System.Windows.Forms.Button();
            this.picboxAppLogo = new System.Windows.Forms.PictureBox();
            this.sstripAbout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picboxAppLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // sstripAbout
            // 
            this.sstripAbout.AutoSize = false;
            this.sstripAbout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(223)))));
            this.sstripAbout.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.sstripAbout.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.slblCopyright});
            this.sstripAbout.Location = new System.Drawing.Point(0, 267);
            this.sstripAbout.Name = "sstripAbout";
            this.sstripAbout.Size = new System.Drawing.Size(367, 24);
            this.sstripAbout.SizingGrip = false;
            this.sstripAbout.Stretch = false;
            this.sstripAbout.TabIndex = 31;
            // 
            // slblCopyright
            // 
            this.slblCopyright.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(207)))), ((int)(((byte)(252)))));
            this.slblCopyright.AutoSize = false;
            this.slblCopyright.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(223)))));
            this.slblCopyright.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.slblCopyright.DoubleClickEnabled = true;
            this.slblCopyright.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.slblCopyright.ForeColor = System.Drawing.Color.White;
            this.slblCopyright.IsLink = true;
            this.slblCopyright.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.slblCopyright.LinkColor = System.Drawing.Color.White;
            this.slblCopyright.Margin = new System.Windows.Forms.Padding(0);
            this.slblCopyright.Name = "slblCopyright";
            this.slblCopyright.Size = new System.Drawing.Size(321, 24);
            this.slblCopyright.Spring = true;
            this.slblCopyright.Text = "ⓒ 2025 HappyBono. All rights reserved.";
            // 
            // lblLicenseTerms
            // 
            this.lblLicenseTerms.AutoSize = true;
            this.lblLicenseTerms.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLicenseTerms.Location = new System.Drawing.Point(17, 78);
            this.lblLicenseTerms.Name = "lblLicenseTerms";
            this.lblLicenseTerms.Size = new System.Drawing.Size(235, 17);
            this.lblLicenseTerms.TabIndex = 30;
            this.lblLicenseTerms.Text = "SonataSmooth Software License Terms";
            // 
            // txtLicenseTerms
            // 
            this.txtLicenseTerms.Font = new System.Drawing.Font("Microsoft NeoGothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLicenseTerms.Location = new System.Drawing.Point(19, 100);
            this.txtLicenseTerms.Multiline = true;
            this.txtLicenseTerms.Name = "txtLicenseTerms";
            this.txtLicenseTerms.ReadOnly = true;
            this.txtLicenseTerms.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLicenseTerms.Size = new System.Drawing.Size(328, 124);
            this.txtLicenseTerms.TabIndex = 28;
            this.txtLicenseTerms.Text = resources.GetString("txtLicenseTerms.Text");
            // 
            // lblAppVersion
            // 
            this.lblAppVersion.AutoSize = true;
            this.lblAppVersion.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAppVersion.Location = new System.Drawing.Point(13, 48);
            this.lblAppVersion.Name = "lblAppVersion";
            this.lblAppVersion.Size = new System.Drawing.Size(67, 20);
            this.lblAppVersion.TabIndex = 27;
            this.lblAppVersion.Text = " v.1.0.0.0";
            // 
            // lblAppTtl
            // 
            this.lblAppTtl.AutoSize = true;
            this.lblAppTtl.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAppTtl.Location = new System.Drawing.Point(12, 9);
            this.lblAppTtl.Name = "lblAppTtl";
            this.lblAppTtl.Size = new System.Drawing.Size(152, 28);
            this.lblAppTtl.TabIndex = 26;
            this.lblAppTtl.Text = "SonataSmooth";
            // 
            // btnOK
            // 
            this.btnOK.Font = new System.Drawing.Font("Microsoft NeoGothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnOK.Location = new System.Drawing.Point(268, 232);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(79, 26);
            this.btnOK.TabIndex = 33;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnDonation
            // 
            this.btnDonation.Font = new System.Drawing.Font("Microsoft NeoGothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnDonation.Location = new System.Drawing.Point(19, 232);
            this.btnDonation.Name = "btnDonation";
            this.btnDonation.Size = new System.Drawing.Size(125, 26);
            this.btnDonation.TabIndex = 32;
            this.btnDonation.Text = "Buy Me a Coffee";
            this.btnDonation.UseVisualStyleBackColor = true;
            this.btnDonation.Click += new System.EventHandler(this.btnDonation_Click);
            // 
            // picboxAppLogo
            // 
            this.picboxAppLogo.Image = ((System.Drawing.Image)(resources.GetObject("picboxAppLogo.Image")));
            this.picboxAppLogo.Location = new System.Drawing.Point(277, 9);
            this.picboxAppLogo.Name = "picboxAppLogo";
            this.picboxAppLogo.Size = new System.Drawing.Size(70, 70);
            this.picboxAppLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picboxAppLogo.TabIndex = 34;
            this.picboxAppLogo.TabStop = false;
            // 
            // FrmAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(367, 291);
            this.Controls.Add(this.picboxAppLogo);
            this.Controls.Add(this.sstripAbout);
            this.Controls.Add(this.lblLicenseTerms);
            this.Controls.Add(this.txtLicenseTerms);
            this.Controls.Add(this.lblAppVersion);
            this.Controls.Add(this.lblAppTtl);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnDonation);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmAbout";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About SonataSmooth";
            this.Load += new System.EventHandler(this.FrmAbout_Load);
            this.sstripAbout.ResumeLayout(false);
            this.sstripAbout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picboxAppLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.StatusStrip sstripAbout;
        internal System.Windows.Forms.ToolStripStatusLabel slblCopyright;
        internal System.Windows.Forms.Label lblLicenseTerms;
        internal System.Windows.Forms.TextBox txtLicenseTerms;
        internal System.Windows.Forms.Label lblAppVersion;
        internal System.Windows.Forms.Label lblAppTtl;
        internal System.Windows.Forms.Button btnOK;
        internal System.Windows.Forms.Button btnDonation;
        private System.Windows.Forms.PictureBox picboxAppLogo;
    }
}