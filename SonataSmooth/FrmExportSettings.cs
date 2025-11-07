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
        public int KernelRadius { get; set; } = 4;
        public int PolyOrder { get; set; } = 3;
        public int BoundaryMethod { get; set; } = 1;
        public int DerivOrder { get; set; } = 0;

        private const int RecommendedMinRadius = 3;
        private const int RecommendedMaxRadius = 7;
        private const int RecommendedMinPolyOrder = 2;
        private const int RecommendedMaxPolyOrder = 6;

        private double dpiX;
        private double dpiY;

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
                KernelRadius = r;

            if (int.TryParse(cbxPolyOrder.Text, out var p))
                PolyOrder = p;

            BoundaryMethod = cbxBoundaryMethod.SelectedIndex;

            if (int.TryParse(cbxDerivOrder.Text, out var d))
            {
                if (d < 0) d = 0;
                if (d > 10) d = 10;
                DerivOrder = d;
            }

            DoRectAvg = chbRect.Checked;
            DoBinomAvg = chbAvg.Checked;
            DoBinomMed = chbMed.Checked;
            DoGauss = chbGauss.Checked;
            DoSavitzky = chbSG.Checked;
            DoAutoSave = chbOpenFile.Checked;

            DoExcelExport = rbtnXLSX.Checked;
            DoCSVExport = rbtnCSV.Checked;

            _mainForm.SetComboValues(cbxKernelRadius.Text, cbxPolyOrder.Text, cbxBoundaryMethod.Text, cbxDerivOrder.Text);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }


        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            cbxKernelRadius.Text = KernelRadius.ToString();
            cbxPolyOrder.Text = PolyOrder.ToString();

            // 지정된 index 값으로 복원
            // index 가 범위를 벗어날 경우 첫 번째 항목을 사용
            cbxBoundaryMethod.SelectedIndex =
                (BoundaryMethod >= 0 && BoundaryMethod < cbxBoundaryMethod.Items.Count)
                    ? BoundaryMethod
                    : 0;

            cbxDerivOrder.Text = DerivOrder.ToString();

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
            using (Graphics g = this.CreateGraphics())
            {
                dpiX = g.DpiX;
                dpiY = g.DpiY;
            }

            slblDesc.Size = new Size(
                (int)(724 * dpiX / 96),
                (int)(19 * dpiY / 96)
                );

            lblPolyOrder.Enabled = chbSG.Checked;
            cbxPolyOrder.Enabled = chbSG.Checked;
            lblDerivOrder.Enabled = chbSG.Checked;
            cbxDerivOrder.Enabled = chbSG.Checked;
        }

        public void ApplyParameters(string kernelRadius, string polyOrder, string boundaryMethod, string derivOrder)
        {
            cbxKernelRadius.Text = kernelRadius;
            cbxPolyOrder.Text = polyOrder;

            // 화면에 보이는 텍스트를 기준으로 선택 (예: "Standard")
            cbxBoundaryMethod.Text = boundaryMethod;

            cbxDerivOrder.Text = derivOrder;

            if (int.TryParse(kernelRadius, out var r)) this.KernelRadius = r;
            if (int.TryParse(polyOrder, out var p)) this.PolyOrder = p;

            // 선택된 인덱스를 저장 (0 : Symmetric, 1 : Replicate, 2 : Adaptive, 3 : Zero-Pad)
            this.BoundaryMethod = cbxBoundaryMethod.SelectedIndex;

            if (int.TryParse(derivOrder, out var d)) this.DerivOrder = Math.Max(0, Math.Min(10, d));
        }

        private void chbSG_CheckedChanged(object sender, EventArgs e)
        {
            if (chbSG.Checked)
            {
                lblPolyOrder.Enabled = true;
                cbxPolyOrder.Enabled = true;
                lblDerivOrder.Enabled = true;
                cbxDerivOrder.Enabled = true;
            }
            else
            {
                lblPolyOrder.Enabled = false;
                cbxPolyOrder.Enabled = false;
                lblDerivOrder.Enabled = false;
                cbxDerivOrder.Enabled = false;
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
            slblDesc.Text = $"Defines how many data points on each side of the target point are included in the smoothing window. (Recommended : {RecommendedMinRadius} - {RecommendedMaxRadius})";
        }

        private void lblKernelRadius_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void cbxKernelRadius_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Text = $"Defines how many data points on each side of the target point are included in the smoothing window. (Recommended : {RecommendedMinRadius} - {RecommendedMaxRadius})";
        }

        private void cbxKernelRadius_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void lblPolyOrder_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Text = $"Specifies the degree of the polynomial used to fit the data within each smoothing window. (Recommended : {RecommendedMinPolyOrder} - {RecommendedMaxPolyOrder}).";
        }

        private void lblPolyOrder_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void cbxPolyOrder_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Text = $"Specifies the degree of the polynomial used to fit the data within each smoothing window. (Recommended : {RecommendedMinPolyOrder} - {RecommendedMaxPolyOrder}).";
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
            slblDesc.Text = "Enable to automatically open the exported file after saving.";
        }

        private void chbOpenFile_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void btnSave_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Text = "Click to save the current settings and apply them.";
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

        private void FrmExportSettings_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void lblBoundaryMethod_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Text = "Specifies how edge data points are treated during smoothing : Symmetric, Replicate, Adaptive (local polynomial + median), or Zero-Pad.";
        }

        private void lblBoundaryMethod_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void cbxBoundaryMethod_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Text = "Specifies how edge data points are treated during smoothing : Symmetric, Replicate, Adaptive (local polynomial + median), or Zero-Pad.";
        }

        private void cbxBoundaryMethod_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void lblDerivOrder_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Text = $"Specifies the order of the derivative to compute from the smoothed data. (Recommended : 0 - 3).";
        }

        private void lblDerivOrder_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void cbxDerivOrder_MouseHover(object sender, EventArgs e)
        {
            slblDesc.Text = $"Specifies the order of the derivative to compute from the smoothed data. (Recommended : 0 - 3).";
        }

        private void cbxDerivOrder_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }
    }
    #endregion
}
