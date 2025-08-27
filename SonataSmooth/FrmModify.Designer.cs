namespace SonataSmooth
{
    partial class FrmModify
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmModify));
            this.sstripModify = new System.Windows.Forms.StatusStrip();
            this.slblModify = new System.Windows.Forms.ToolStripStatusLabel();
            this.txtInitEdit = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.lblModifyDscr = new System.Windows.Forms.Label();
            this.pbModify = new System.Windows.Forms.ProgressBar();
            this.lblModifyTtl = new System.Windows.Forms.Label();
            this.sstripModify.SuspendLayout();
            this.SuspendLayout();
            // 
            // sstripModify
            // 
            this.sstripModify.AutoSize = false;
            this.sstripModify.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(223)))));
            this.sstripModify.Font = new System.Drawing.Font("Segoe UI Variable Display", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sstripModify.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.sstripModify.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.slblModify});
            this.sstripModify.Location = new System.Drawing.Point(0, 119);
            this.sstripModify.Name = "sstripModify";
            this.sstripModify.Size = new System.Drawing.Size(438, 24);
            this.sstripModify.SizingGrip = false;
            this.sstripModify.TabIndex = 28;
            this.sstripModify.Text = "StatusStrip1";
            // 
            // slblModify
            // 
            this.slblModify.AutoSize = false;
            this.slblModify.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(223)))));
            this.slblModify.Font = new System.Drawing.Font("Segoe UI Variable Display", 9F);
            this.slblModify.ForeColor = System.Drawing.Color.White;
            this.slblModify.Name = "slblModify";
            this.slblModify.Size = new System.Drawing.Size(437, 19);
            this.slblModify.Text = "Modifying 2147483647 selected items... ";
            this.slblModify.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtInitEdit
            // 
            this.txtInitEdit.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtInitEdit.Location = new System.Drawing.Point(14, 81);
            this.txtInitEdit.Name = "txtInitEdit";
            this.txtInitEdit.Size = new System.Drawing.Size(411, 25);
            this.txtInitEdit.TabIndex = 1;
            this.txtInitEdit.TextChanged += new System.EventHandler(this.txtInitEdit_TextChanged);
            this.txtInitEdit.Enter += new System.EventHandler(this.txtInitEdit_Enter);
            this.txtInitEdit.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtInitEdit_KeyDown);
            this.txtInitEdit.MouseLeave += new System.EventHandler(this.txtInitEdit_MouseLeave);
            this.txtInitEdit.MouseHover += new System.EventHandler(this.txtInitEdit_MouseHover);
            // 
            // btnCancel
            // 
            this.btnCancel.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(340, 53);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(85, 24);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            this.btnCancel.MouseLeave += new System.EventHandler(this.btnCancel_MouseLeave);
            this.btnCancel.MouseHover += new System.EventHandler(this.btnCancel_MouseHover);
            // 
            // btnOk
            // 
            this.btnOk.Font = new System.Drawing.Font("Segoe Fluent Icons", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOk.Location = new System.Drawing.Point(340, 7);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(85, 44);
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            this.btnOk.MouseLeave += new System.EventHandler(this.btnOk_MouseLeave);
            this.btnOk.MouseHover += new System.EventHandler(this.btnOk_MouseHover);
            // 
            // lblModifyDscr
            // 
            this.lblModifyDscr.AutoSize = true;
            this.lblModifyDscr.Font = new System.Drawing.Font("Segoe UI Variable Display", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblModifyDscr.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lblModifyDscr.Location = new System.Drawing.Point(12, 39);
            this.lblModifyDscr.Name = "lblModifyDscr";
            this.lblModifyDscr.Size = new System.Drawing.Size(270, 34);
            this.lblModifyDscr.TabIndex = 24;
            this.lblModifyDscr.Text = "Apply changes to the selected items. \r\nEnter the numeric value you would like to " +
    "set :";
            // 
            // pbModify
            // 
            this.pbModify.Location = new System.Drawing.Point(0, 114);
            this.pbModify.Name = "pbModify";
            this.pbModify.Size = new System.Drawing.Size(438, 5);
            this.pbModify.TabIndex = 29;
            // 
            // lblModifyTtl
            // 
            this.lblModifyTtl.AutoSize = true;
            this.lblModifyTtl.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblModifyTtl.Location = new System.Drawing.Point(9, 7);
            this.lblModifyTtl.Name = "lblModifyTtl";
            this.lblModifyTtl.Size = new System.Drawing.Size(214, 26);
            this.lblModifyTtl.TabIndex = 30;
            this.lblModifyTtl.Text = "Modify Selected Entries";
            // 
            // FrmModify
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(438, 143);
            this.Controls.Add(this.lblModifyTtl);
            this.Controls.Add(this.sstripModify);
            this.Controls.Add(this.txtInitEdit);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.lblModifyDscr);
            this.Controls.Add(this.pbModify);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmModify";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Modify Selected Entries";
            this.Load += new System.EventHandler(this.FrmModify_Load);
            this.MouseHover += new System.EventHandler(this.FrmModify_MouseHover);
            this.sstripModify.ResumeLayout(false);
            this.sstripModify.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.StatusStrip sstripModify;
        internal System.Windows.Forms.ToolStripStatusLabel slblModify;
        internal System.Windows.Forms.TextBox txtInitEdit;
        internal System.Windows.Forms.Button btnCancel;
        internal System.Windows.Forms.Button btnOk;
        internal System.Windows.Forms.Label lblModifyDscr;
        internal System.Windows.Forms.ProgressBar pbModify;
        private System.Windows.Forms.Label lblModifyTtl;
    }
}