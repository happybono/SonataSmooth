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

namespace SonataSmooth
{
    public partial class FrmModify : Form
    {
        private double dpiX;
        private double dpiY;

        FrmMain mainForm = Application.OpenForms
                                       .OfType<FrmMain>()
                                       .FirstOrDefault();

        public FrmModify()
        {
            InitializeComponent();
        }

        private async void btnOk_Click(object sender, EventArgs e)
        {
            if (mainForm == null)
            {
                MessageBox.Show("Main form not found.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 입력 유효성 검사
            if (string.IsNullOrEmpty(txtInitEdit.Text) ||
                !double.TryParse(txtInitEdit.Text, out double numericValue))
            {
                txtInitEdit.Select();
                txtInitEdit.SelectAll();
                return;
            }

            // 선택된 Index 정렬하여 배열로 전환
            int[] indices = mainForm.lbInitData
                                    .SelectedIndices
                                    .Cast<int>()
                                    .OrderBy(x => x)
                                    .ToArray();
            int total = indices.Length;
            if (total == 0) return;

            pbModify.Minimum = 0;
            pbModify.Maximum = total;
            pbModify.Value = 0;

            var lb = mainForm.lbInitData;
            lb.BeginUpdate();

            string newValue = numericValue.ToString("G");

            const int BatchSize = 1000;
            int done = 0;

            // Batch 단위로 UI Thread 에서 항목 변경
            while (done < total)
            {
                int cnt = Math.Min(BatchSize, total - done);
                int[] batchIndices = indices.Skip(done).Take(cnt).ToArray();

                // UI Thread 에서 항목 변경
                if (lb.InvokeRequired)
                {
                    lb.Invoke((Action)(() =>
                    {
                        for (int i = 0; i < batchIndices.Length; i++)
                        {
                            lb.Items[batchIndices[i]] = newValue;
                        }
                        pbModify.Value = Math.Min(done + cnt, total);

                        int count = mainForm.lbInitData.SelectedItems.Count;
                        if (count > 1)
                        {
                            slblModify.Text = $"Modifying {count} selected items...";
                        }
                        else
                        {
                            slblModify.Text = "Modifying the selected item...";
                        }
                    }));
                }
                else
                {
                    for (int i = 0; i < batchIndices.Length; i++)
                    {
                        lb.Items[batchIndices[i]] = newValue;
                    }
                    pbModify.Value = Math.Min(done + cnt, total);
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
                    mainForm.lbInitData.Focus();
                    pbModify.Value = 0;
                    mainForm.ShowStatusMessage($"Modified {total} item{(total > 1 ? "s" : "")} to '{newValue}' in Initial Dataset.");
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
                mainForm.lbInitData.Focus();
                pbModify.Value = 0;
                mainForm.ShowStatusMessage($"Modified {total} item{(total > 1 ? "s" : "")} to '{newValue}' in Initial Dataset.");
                this.Close();
            }
        }

        private void FrmModify_Load(object sender, EventArgs e)
        {
            if (mainForm == null)
            {
                slblModify.Text = "Main form not found.";
                return;
            }

            using (Graphics g = this.CreateGraphics())
            {
                dpiX = g.DpiX;
                dpiY = g.DpiY;
            }

            pbModify.Size = new Size(
                (int)(438 * dpiX / 96),
                (int)(5 * dpiY / 96)
            );

            slblModify.Size = new Size(
                (int)(437 * dpiX / 96),
                (int)(19 * dpiY / 96)
            );

            int count = mainForm.lbInitData.SelectedItems.Count;
            if (count > 1)
            {
                slblModify.Text = $"Enter the new value for the {count} selected items.";
            }
            else
            {
                slblModify.Text = "Enter the new value for the selected item.";
            }

            txtInitEdit.Text = mainForm.lbInitData.SelectedItem.ToString();
            txtInitEdit.SelectAll();
            txtInitEdit.Select();
        }

        private void txtInitEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                btnOk.PerformClick();
                e.SuppressKeyPress = true;
            }

            if (e.KeyData == Keys.Escape)
            {
                btnCancel.PerformClick();
                e.SuppressKeyPress = true;
            }
        }

        private void txtInitEdit_TextChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = txtInitEdit.Text.Length > 0 && double.TryParse(txtInitEdit.Text, out _);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region Mouse Hover & Leave Events

        private void MouseLeaveHandler(object sender, EventArgs e)
        {
            if (mainForm == null)
            {
                slblModify.Text = "Main form not found.";
                return;
            }

            int count = mainForm.lbInitData.SelectedItems.Count;
            if (count > 1)
            {
                // 다수의 항목이 선택된 경우
                slblModify.Text = $"Enter the new value for the {count} selected items.";
            }
            else
            {
                slblModify.Text = "Enter the new value for the selected item.";
            }
        }

        private void FrmModify_MouseHover(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void txtInitEdit_MouseHover(object sender, EventArgs e)
        {
            slblModify.Text = "To modify the selected items, enter a new value and click 'OK'.";
        }

        private void btnOk_MouseHover(object sender, EventArgs e)
        {
            slblModify.Text = "Click to apply the new value to the selected items.";
        }

        private void btnOk_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void btnCancel_MouseHover(object sender, EventArgs e)
        {
            slblModify.Text = "Click to cancel the modification and close the dialog.";
        }

        private void btnCancel_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void txtInitEdit_Enter(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }

        private void txtInitEdit_MouseLeave(object sender, EventArgs e)
        {
            MouseLeaveHandler(sender, e);
        }
    }
    #endregion
}
