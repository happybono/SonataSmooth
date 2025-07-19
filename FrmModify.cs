using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace NoiseReductionSample
{
    public partial class FrmModify : Form
    {
        public FrmModify()
        {
            InitializeComponent();
        }

        private async void OK_Button_Click(object sender, EventArgs e)
        {
            var mainForm = Application.OpenForms
                                      .OfType<FrmMain>()
                                      .FirstOrDefault();
            if (mainForm == null)
            {
                MessageBox.Show("Main form not found.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(textBox1.Text)
                || !double.TryParse(textBox1.Text, out double numericValue))
            {
                textBox1.Select();
                textBox1.SelectAll();
                return;
            }

            int[] indices = mainForm.listBox1
                                    .SelectedIndices
                                    .Cast<int>()
                                    .OrderBy(x => x)
                                    .ToArray();
            int total = indices.Length;
            if (total == 0)
                return;

            ProgressBar1.Minimum = 0;
            ProgressBar1.Maximum = total;
            ProgressBar1.Value = 0;

            var lb = mainForm.listBox1;
            lb.BeginUpdate();

            string newValue = await Task.Run(() => numericValue.ToString("G"));

            for (int i = 0; i < total; i++)
            {
                lb.Items[indices[i]] = newValue;
                ProgressBar1.Value = i + 1;
            }

            lb.ClearSelected();
            foreach (int idx in indices)
                if (idx >= 0 && idx < lb.Items.Count)
                    lb.SetSelected(idx, true);

            lb.EndUpdate();

            mainForm.BeginInvoke((Action)(() =>
            {
                mainForm.listBox1.Focus();
            }));

            ProgressBar1.Value = 0;
            this.Close();
        }

        private void FrmModify_Load(object sender, EventArgs e)
        {
            // Get the main form instance
            var mainForm = Application.OpenForms
                                      .OfType<FrmMain>()
                                      .FirstOrDefault();
            if (mainForm == null)
            {
                ToolStripStatusLabel1.Text = "Main form not found.";
                return;
            }

            int count = mainForm.listBox1.SelectedItems.Count;
            if (count > 1)
            {
                ToolStripStatusLabel1.Text = $"Modifying {count} selected items...";
            }
            else
            {
                ToolStripStatusLabel1.Text = "Modifying the selected item...";
            }

            textBox1.Text = mainForm.listBox1.SelectedItem.ToString();
            textBox1.SelectAll();
            textBox1.Select();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                OK_Button.PerformClick();
                e.SuppressKeyPress = true;
            }

            if (e.KeyData == Keys.Escape)
            {
                Cancel_Button.PerformClick();
                e.SuppressKeyPress = true;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            OK_Button.Enabled = textBox1.Text.Length > 0 && double.TryParse(textBox1.Text, out _);
        }
    }
}
