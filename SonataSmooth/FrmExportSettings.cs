using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SonataSmooth
{
    public partial class FrmExportSettings : Form
    {
        public int kernelRadius { get; set; } = 4;
        public int polyOrder { get; set; } = 3;
        private FrmMain _mainForm;

        public bool DoRectAvg { get; set; } = true;
        public bool DoBinomAvg { get; set; } = true;
        public bool DoBinomMed { get; set; } = true;
        public bool DoGauss { get; set; } = true;
        public bool DoSavitzky { get; set; } = true;

        public bool DoExcelExport { get; set; } = false;
        public bool DoCSVExport { get; set; } = true;
        public bool DoAutoSave { get; set; } = true;


        public FrmExportSettings(FrmMain mainForm)
        {
            InitializeComponent();
            _mainForm = mainForm;
            this.btnSave.Click += btnSave_Click;
            this.btnCancel.Click += btnCancel_Click;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (int.TryParse(cbxKernelRadius.Text, out var r))
                kernelRadius = r;

            if (int.TryParse(cbxPolyOrder.Text, out var p))
                polyOrder = p;

            DoRectAvg = chbRect.Checked;
            DoBinomAvg = chbAvg.Checked;
            DoBinomMed = chbMed.Checked;
            DoGauss = chbGauss.Checked;
            DoSavitzky = chbSG.Checked;
            DoAutoSave = chbOpenFile.Checked;

            DoExcelExport = rbtnXLSX.Checked;
            DoCSVExport = rbtnCSV.Checked;

            _mainForm.SetComboValues(cbxKernelRadius.Text, cbxPolyOrder.Text);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }


        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            cbxKernelRadius.Text = kernelRadius.ToString();
            cbxPolyOrder.Text = cbxPolyOrder.ToString();

            chbRect.Checked = DoRectAvg;
            chbAvg.Checked = DoBinomAvg;
            chbMed.Checked = DoBinomMed;
            chbGauss.Checked = DoGauss;
            chbSG.Checked = DoSavitzky;
            chbOpenFile.Checked = DoAutoSave;

            rbtnXLSX.Checked = DoExcelExport;
            rbtnCSV.Checked = DoCSVExport;
        }


        private void FrmExportSettings_Load(object sender, EventArgs e)
        {         
            lblPolyOrder.Enabled = chbSG.Checked;
            cbxPolyOrder.Enabled = chbSG.Checked;
        }

        public void ApplyParameters(string kernelRadius, string polyOrder)
        {
            cbxKernelRadius.Text = kernelRadius;
            cbxPolyOrder.Text = polyOrder;

            if (int.TryParse(kernelRadius, out var r)) this.kernelRadius = r;
            if (int.TryParse(polyOrder, out var p)) this.polyOrder = p;
        }

        private void chbSG_CheckedChanged(object sender, EventArgs e)
        {
            if (chbSG.Checked)
            {
                lblPolyOrder.Enabled = true;
                cbxPolyOrder.Enabled = true;
            }
            else
            {
                lblPolyOrder.Enabled = false;
                cbxPolyOrder.Enabled = false;
            }
        }

        #region Mouse Hover and Leave Events
        private void MouseLeaveHandler(object sender, EventArgs e)
        {
            slblDesc.Text = "To save the settings, please select the desired options and click the 'Save' button.";
        }

        private void FrmExportSettings_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Text = "To save the settings, please select the desired options and click the 'Save' button.";
        }

        private void chbRect_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Text = "Applies a simple moving average using equal weights within the kernel window.";
        }

        private void chbRect_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void chbAvg_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Text = "Smooths data using binomial coefficients for weighted averaging within the kernel window.";
        }

        private void chbAvg_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void chbMed_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Text = "Reduces noise by computing the weighted median using binomial coefficients within the kernel window.";
        }

        private void chbMed_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void chbGauss_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Text = "Smooths data using a Gaussian kernel for weighted averaging, emphasizing central values.";
        }

        private void chbGauss_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void chbSG_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Text = "Fits a polynomial to the data within the kernel window for advanced smoothing and trend preservation.";
        }

        private void chbSG_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void lblKernelRadius_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Text = "Defines the number of data points on each side of the target point used for smoothing.";
        }

        private void lblKernelRadius_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void cbxKernelRadius_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Text = "Defines the number of data points on each side of the target point used for smoothing.";
        }

        private void cbxKernelRadius_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void lblPolyOrder_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Text = "Specifies the degree of the polynomial used to fit the data within each smoothing window.";
        }

        private void lblPolyOrder_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void cbxPolyOrder_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Text = "Specifies the degree of the polynomial used to fit the data within each smoothing window.";
        }

        private void cbxPolyOrder_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void rbtnXLSX_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Text = "Exports the data to an Excel file in XLSX format.";
        }

        private void rbtnXLSX_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void rbtnCSV_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Text = "Exports the data to a CSV file, which can be opened in spreadsheet applications.";
        }

        private void rbtnCSV_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void chbOpenFile_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Text = "Automatically opens the exported file after saving.";
        }

        private void chbOpenFile_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void btnSave_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Text = "Click to save the selected settings and apply them.";
        }

        private void btnSave_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void btnCancel_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Text = "Click to cancel and close the settings without saving changes.";
        }

        private void btnCancel_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }
    }
    #endregion
}
