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
        public int KernelWidth { get; set; } = 4;
        public int PolyOrder { get; set; } = 3;
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
            if (int.TryParse(cbxKernelWidth.Text, out var w))
                KernelWidth = w;

            if (int.TryParse(cbxPolyOrder.Text, out var p))
                PolyOrder = p;

            DoRectAvg = chbRect.Checked;
            DoBinomAvg = chbAvg.Checked;
            DoBinomMed = chbMed.Checked;
            DoGauss = chbGauss.Checked;
            DoSavitzky = chbSG.Checked;
            DoAutoSave = chbOpenFile.Checked;

            DoExcelExport = rbtnXLSX.Checked;
            DoCSVExport = rbtnCSV.Checked;

            _mainForm.SetComboValues(cbxKernelWidth.Text, cbxPolyOrder.Text);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }


        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            cbxKernelWidth.Text = KernelWidth.ToString();
            cbxPolyOrder.Text = PolyOrder.ToString();

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

        public void ApplyParameters(string kernelWidth, string polyOrder)
        {
            cbxKernelWidth.Text = kernelWidth;
            cbxPolyOrder.Text = polyOrder;

            if (int.TryParse(kernelWidth, out var w)) KernelWidth = w;
            if (int.TryParse(polyOrder, out var p)) PolyOrder = p;
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
    }
}
