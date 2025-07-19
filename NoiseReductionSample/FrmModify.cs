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
            // 1) 메인 폼 가져오기
            var mainForm = Application.OpenForms
                                      .OfType<FrmMain>()
                                      .FirstOrDefault();
            if (mainForm == null)
            {
                MessageBox.Show("Main form not found.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 2) 입력 검사
            if (string.IsNullOrEmpty(textBox1.Text)
                || !double.TryParse(textBox1.Text, out double numericValue))
            {
                textBox1.Select();
                textBox1.SelectAll();
                return;
            }

            // 3) 선택 인덱스 캡처 & 정렬
            int[] indices = mainForm.listBox1
                                    .SelectedIndices
                                    .Cast<int>()
                                    .OrderBy(x => x)
                                    .ToArray();
            int total = indices.Length;
            if (total == 0)
                return;

            // 4) ProgressBar 초기화
            ProgressBar1.Minimum = 0;
            ProgressBar1.Maximum = total;
            ProgressBar1.Value = 0;

            // 5) BeginUpdate
            var lb = mainForm.listBox1;
            lb.BeginUpdate();

            // 6) 비동기 “무거운 계산” (예: ToString 포맷)
            string newValue = await Task.Run(() => numericValue.ToString("G"));

            // 7) 싱글 스레드 루프: 아이템 교체 + 프로그래스바 업데이트
            for (int i = 0; i < total; i++)
            {
                lb.Items[indices[i]] = newValue;
                ProgressBar1.Value = i + 1;
            }

            // 8) 선택 재설정
            lb.ClearSelected();
            foreach (int idx in indices)
                if (idx >= 0 && idx < lb.Items.Count)
                    lb.SetSelected(idx, true);

            // 9) EndUpdate
            lb.EndUpdate();

            // 10) 다이얼로그 닫힌 후 ListBox에 Focus 예약
            //     → BeginInvoke를 쓰면 this.Close() 이후에 실행됩니다.
            mainForm.BeginInvoke((Action)(() =>
            {
                mainForm.listBox1.Focus();
            }));

            // 11) 상태바 리셋 및 다이얼로그 닫기
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

            textBox1.Select();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                OK_Button.PerformClick();
            }

            if (e.KeyData == Keys.Escape)
            {
                Cancel_Button.PerformClick();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            OK_Button.Enabled = textBox1.Text.Length > 0 && double.TryParse(textBox1.Text, out _);
        }
    }
}
