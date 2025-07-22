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
            // mainForm 인스턴스 가져오기
            var mainForm = Application.OpenForms
                                      .OfType<FrmMain>()
                                      .FirstOrDefault();

            if (mainForm == null)
            {
                MessageBox.Show("Main form not found.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 입력 유효성 검사
            if (string.IsNullOrEmpty(textBox1.Text) ||
                !double.TryParse(textBox1.Text, out double numericValue))
            {
                textBox1.Select();
                textBox1.SelectAll();
                return;
            }

            // 선택된 Index 정렬하여 배열로 전환
            int[] indices = mainForm.listBox1
                                    .SelectedIndices
                                    .Cast<int>()
                                    .OrderBy(x => x)
                                    .ToArray();
            int total = indices.Length;
            if (total == 0) return;

            ProgressBar1.Minimum = 0;
            ProgressBar1.Maximum = total;
            ProgressBar1.Value = 0;

            var lb = mainForm.listBox1;
            lb.BeginUpdate();

            string newValue = numericValue.ToString("G");

            const int BatchSize = 1000;
            int done = 0;

            // 배치 단위로 UI 스레드에서 항목 변경
            while (done < total)
            {
                int cnt = Math.Min(BatchSize, total - done);
                int[] batchIndices = indices.Skip(done).Take(cnt).ToArray();

                // UI 스레드에서 항목 변경
                if (lb.InvokeRequired)
                {
                    lb.Invoke((Action)(() =>
                    {
                        for (int i = 0; i < batchIndices.Length; i++)
                        {
                            lb.Items[batchIndices[i]] = newValue;
                        }
                        ProgressBar1.Value = Math.Min(done + cnt, total);
                    }));
                }
                else
                {
                    for (int i = 0; i < batchIndices.Length; i++)
                    {
                        lb.Items[batchIndices[i]] = newValue;
                    }
                    ProgressBar1.Value = Math.Min(done + cnt, total);
                }

                done += cnt;
                await Task.Yield();
            }

            // 변경된 항목 재선택 및 마무리
            if (lb.InvokeRequired)
            {
                lb.Invoke((Action)(() =>
                {
                    lb.ClearSelected();
                    foreach (int idx in indices)
                    {
                        if (idx >= 0 && idx < lb.Items.Count)
                            lb.SetSelected(idx, true);
                    }
                    lb.EndUpdate();
                    mainForm.listBox1.Focus();
                    ProgressBar1.Value = 0;
                    this.Close();
                }));
            }
            else
            {
                lb.ClearSelected();
                foreach (int idx in indices)
                {
                    if (idx >= 0 && idx < lb.Items.Count)
                        lb.SetSelected(idx, true);
                }
                lb.EndUpdate();
                mainForm.listBox1.Focus();
                ProgressBar1.Value = 0;
                this.Close();
            }
        }

        private void FrmModify_Load(object sender, EventArgs e)
        {
            // mainForm 인스턴스 가져오기
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

        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
