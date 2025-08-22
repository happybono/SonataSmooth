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
    public partial class FrmAbout : Form
    {
        public FrmAbout()
        {
            InitializeComponent();
            lblAppVersion.Text = $"v.{Application.ProductVersion}";
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FrmAbout_Load(object sender, EventArgs e)
        {
            lblAppVersion.Select();
        }

        private void btnDonation_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.paypal.com/ncp/payment/UF8ANWF5TVQS2");
        }
    }
}
