using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoiseReductionSample
{
    public partial class FrmMain : Form
    {

        public FrmMain()
        {
            InitializeComponent();
        }

        private void btnCalibrate_Click(object sender, EventArgs e)
        {
            int NoiseReductionKernelWidth = int.Parse(cbxKernelWidth.Text);
            int[] binomialCoefficients = CalcBinomialCoefficients(NoiseReductionKernelWidth + 1);

            listBox2.Items.Clear();
            lblCnt2.Text = "Count : " + listBox2.Items.Count;

            // Rectangular
            if (rbtnRect.Checked == true)
            {
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    // Variable to store the sum of the array
                    double sum = 0;
                    int count = 0;

                    // Subtract the Index by the Kernel Width
                    for (int kernel_index = -NoiseReductionKernelWidth; kernel_index <= NoiseReductionKernelWidth; kernel_index++)
                    {
                        int dataIndex = i + kernel_index;
                        // Ignore data out of range
                        if (dataIndex >= 0 && dataIndex < listBox1.Items.Count)
                        {
                            count++;
                            // Summation
                            sum += Convert.ToDouble(listBox1.Items[dataIndex]);
                        }
                    }
                    // Add the average of the total sum to ListBox2
                    listBox2.Items.Add(sum / count);
                    lblCnt2.Text = "Count : " + listBox2.Items.Count;
                }
            }

            // Binomial coefficient (median)
            else if (rbtnMed.Checked == true)
            {
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    List<double> values = new List<double>();

                    // Kernel Width calculation.
                    for (int kernel_index = -NoiseReductionKernelWidth; kernel_index <= NoiseReductionKernelWidth; kernel_index++)
                    {
                        int dataIndex = i + kernel_index;
                        // Ignore data out of range
                        if (dataIndex >= 0 && dataIndex < listBox1.Items.Count)
                        {
                            // Apply binomial coefficient and add to the list
                            values.Add(Convert.ToDouble(listBox1.Items[dataIndex]));
                        }
                    }

                    // Sort values and calculate median
                    if (values.Count > 0)
                    {
                        values.Sort();
                        double median;
                        int midIndex = values.Count / 2;
                        if (values.Count % 2 == 0)
                        {
                            median = (values[midIndex - 1] + values[midIndex]) / 2.0;
                        }
                        else
                        {
                            median = values[midIndex];
                        }

                        // Add median to ListBox2
                        listBox2.Items.Add(median);
                        lblCnt2.Text = "Count : " + listBox2.Items.Count;
                    }
                }
            }

            // Binomial coefficient (average)
            else if (rbtnAvg.Checked == true)
            {
                binomialCoefficients = AvgCalcBinomialCoefficients(NoiseReductionKernelWidth * 2 + 1);
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    List<double> values = new List<double>();
                    int coefficientSum = 0;

                    // Kernel Width calculation
                    for (int kernel_index = -NoiseReductionKernelWidth; kernel_index <= NoiseReductionKernelWidth; kernel_index++)
                    {
                        int dataIndex = i + kernel_index;
                        // Ignore data out of range
                        if (dataIndex >= 0 && dataIndex < listBox1.Items.Count)
                        {
                            // Apply binomial coefficient and add to the list
                            values.Add(Convert.ToDouble(listBox1.Items[dataIndex]) * binomialCoefficients[NoiseReductionKernelWidth + kernel_index]);
                            coefficientSum += binomialCoefficients[NoiseReductionKernelWidth + kernel_index];
                        }
                    }

                    // Calculate the weighted average
                    if (values.Count > 0)
                    {
                        double sum = values.Sum();
                        double average = sum / coefficientSum;

                        // Add the average to ListBox2
                        listBox2.Items.Add(average);
                        lblCnt2.Text = "Count : " + listBox2.Items.Count;
                    }
                }
            }
        }

        private int[] AvgCalcBinomialCoefficients(int windowSize)
        {
            int[] coefficients = new int[windowSize];
            coefficients[0] = 1;

            for (int i = 1; i < windowSize; i++)
            {
                coefficients[i] = coefficients[i - 1] * (windowSize - i) / i;
            }

            return coefficients;
        }




        private int[] CalcBinomialCoefficients(int windowSize)
        {
            if (windowSize < 1)
                throw new ArgumentException("Window size must be at least 1.", nameof(windowSize));

            int[] coefficients = new int[windowSize];
            coefficients[0] = 1;

            for (int i = 1; i < windowSize; i++)
            {
                coefficients[i] = coefficients[i - 1] * (windowSize - i) / i;
            }

                Array.Sort(coefficients);
                Array.Reverse(coefficients);

            return coefficients;
        }



        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            string patternFindNumbers = @"[+-]?(\d+(,\d{3})*|(?=\.\d))((\.\d+([eE][+-]\d+)?)|)";
            string patternHtmlParse = @"(?<=>.*)[+-]?" + patternFindNumbers + @"(?=[^>]*<)";
            MatchCollection foundNumbers;

            if (e.Data.GetDataPresent("HTML Format"))
            {
                string dataHtmlFormat = e.Data.GetData("HTML Format").ToString();
                if (Regex.Match(dataHtmlFormat, @"(?<=<html).*?(?=>)", RegexOptions.Singleline).Value.Contains("urn:schemas-microsoft-com:office:word"))
                {
                    dataHtmlFormat = dataHtmlFormat.Remove(0, dataHtmlFormat.IndexOf("<body"));
                    MatchCollection matchesP, matchesSpan;
                    matchesP = Regex.Matches(dataHtmlFormat, @"<p.*?</p>", RegexOptions.Singleline); // each line

                    if (matchesP.Count > 0)
                    {
                        dataHtmlFormat = string.Empty;
                        foreach (Match matchP in matchesP)
                        {
                            matchesSpan = Regex.Matches(matchP.Value, @"(?<=>)[^<>]+(?=<)");
                            foreach (Match matchSpan in matchesSpan)
                            {
                                dataHtmlFormat += matchSpan.Value;
                            }
                            dataHtmlFormat += Environment.NewLine;
                        }
                    }
                    else
                    {
                        matchesSpan = Regex.Matches(dataHtmlFormat, @"(?<=>)[^<>]+(?=<)");
                        dataHtmlFormat = string.Empty;
                        foreach (Match matchSpan in matchesSpan)
                        {
                            dataHtmlFormat += matchSpan.Value;
                        }
                        dataHtmlFormat += Environment.NewLine;
                    }
                    foundNumbers = Regex.Matches(dataHtmlFormat, patternFindNumbers);
                }
                else
                {
                    foundNumbers = Regex.Matches(e.Data.GetData("Text").ToString(), patternFindNumbers);
                }
            }
            else
            {
                foundNumbers = Regex.Matches(e.Data.GetData("Text").ToString(), patternFindNumbers);
            }

            int listBoxStartCount = listBox1.Items.Count;

            foreach (Match item in foundNumbers)
            {
                listBox1.Items.Add(Convert.ToDouble(item.Value));
                listBox1.TopIndex = listBox1.Items.Count - 1;
                lblCnt1.Text = "Count : " + listBox1.Items.Count;
            }
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            // this list contains all of the available formats of data you can retrieve from the dragging object
            string[] availableFormats = e.Data.GetFormats();

            // check to see if a desired format is contained in the drag object
            if (e.Data.GetDataPresent("Text"))
            {
                // if it is then set the mode to "Copy", which will leave the data in the excel sheet
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                // if not, then prevent the drop, changing the cursor to the "not allowed" version
                e.Effect = DragDropEffects.None;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {            
            listBox1.Items.Add(txtVariable.Text);
        }

        private void txtVariable_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter) {
                btnAdd.PerformClick();
                txtVariable.Text = String.Empty;
            }
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                if (!listBox1.SelectedIndices.Contains(i))
                {
                    listBox1.SelectedIndices.Add(i);
                }
            }

            listBox1.Select();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            lblCnt1.Text = "Count : " + listBox1.Items.Count;
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(string.Join(Environment.NewLine, listBox1.SelectedItems.Cast<double>().ToArray()));
        }

        private void btnPaste_Click(object sender, EventArgs e)
        {
            string patternFindNumbers = "[+-]?(\\d+(,\\d{3})*|(?=\\.\\d))((\\.\\d+([eE][+-]\\d+)?)|)";
            MatchCollection foundNumbers;
            foundNumbers = Regex.Matches(Clipboard.GetText().ToString(), patternFindNumbers);

            int listBoxStartCount = listBox1.Items.Count;

            foreach (Match item in foundNumbers)
            {
                listBox1.Items.Add(Convert.ToDouble(item.Value));
                listBox1.TopIndex = listBox1.Items.Count - 1;
            }

            lblCnt1.Text = "Count : " + listBox1.Items.Count;

        }

        private void btnSelClear_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                if (listBox1.SelectedIndices.Contains(i))
                {
                    listBox1.SelectedIndices.Remove(i);
                }
            }

            listBox1.Select();
            lblCnt1.Text = "Count : " + listBox1.Items.Count;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            IEnumerator en = listBox1.SelectedIndices.GetEnumerator();
            int deletedItems1;

            if (listBox1.SelectedItems.Count == 0)
            {
                btnCopy.Enabled = false;
                return;
            }

            deletedItems1 = listBox1.SelectedItems.Count;

            while (en.MoveNext())
            {
                listBox1.Items.RemoveAt((int)en.Current);
                en = listBox1.SelectedIndices.GetEnumerator();
            }

            lblCnt1.Text = "Count : " + listBox1.Items.Count;
        }

        private void btnClear2_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            lblCnt2.Text = "Count : " + listBox2.Items.Count;
            btnCopy2.Enabled = false;
            btnSelClear2.Enabled = false;
        }

        private void btnCopy2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(string.Join(Environment.NewLine, listBox2.SelectedItems.Cast<double>().ToArray()));
        }

        private void btnSelectAll2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listBox2.Items.Count; i++)
            {
                if (!listBox2.SelectedIndices.Contains(i))
                {
                    listBox2.SelectedIndices.Add(i);
                }
            }

            listBox2.Select();
        }

        private void btnSelectClear2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listBox2.Items.Count; i++)
            {
                if (listBox2.SelectedIndices.Contains(i))
                {
                    listBox2.SelectedIndices.Remove(i);
                }
            }

            listBox2.Select();
            lblCnt2.Text = "Count : " + listBox2.Items.Count;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            cbxKernelWidth.SelectedIndex = 3;
            btnAdd.Enabled = false;
            btnCopy2.Enabled = false;
            btnSelClear2.Enabled = false;
            btnCopy.Enabled = false;
            btnSelClear.Enabled = false;
            btnDelete.Enabled = false;
        }

        private void txtVariable_TextChanged(object sender, EventArgs e)
        {
            if (txtVariable.Text.Length == 0)
            {
                btnAdd.Enabled = false;
            }
            else
            {
                 btnAdd.Enabled = true;
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedItems.Count == 0)
            {
                btnCopy2.Enabled = false;
                btnSelClear2.Enabled = false;
            }
            else
            {
                btnCopy2.Enabled= true;
                btnSelClear2.Enabled = true;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count == 0)
            {
                btnCopy.Enabled = false;
                btnSelClear.Enabled = false;
                btnDelete.Enabled = false;  
            }
            else
            {
                btnCopy.Enabled = true;
                btnSelClear.Enabled= true;
                btnDelete.Enabled= true;
            }
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete)
            {
                btnDelete.PerformClick();
            }

            if (e.KeyData == (Keys.Control | Keys.Delete))
            {
                btnClear.PerformClick();
            }

            if (e.KeyData == (Keys.Control | Keys.C))
            {
                btnCopy.PerformClick();
            }

            if (e.KeyData == (Keys.Control | Keys.V))
            {
                btnPaste.PerformClick();
            }

            if (e.KeyData == (Keys.Control | Keys.A))
            {
                btnSelectAll.PerformClick();
            }

            if (e.KeyData == Keys.Escape)
            {
                btnSelClear.PerformClick();
            }

            lblCnt1.Text = "Count : " + listBox1.Items.Count;
        }

        private void listBox2_KeyDown(object sender, KeyEventArgs e)
        {
            {
               
                if (e.KeyData == (Keys.Control | Keys.Delete))
                {
                    btnClear2.PerformClick();
                }

                if (e.KeyData == (Keys.Control | Keys.C))
                {
                    btnCopy2.PerformClick();
                }

                if (e.KeyData == (Keys.Control | Keys.A))
                {
                    btnSelectAll2.PerformClick();
                }

                if (e.KeyData == Keys.Escape)
                {
                    btnSelClear2.PerformClick();
                }

                lblCnt2.Text = "Count : " + listBox2.Items.Count;
            }
        }
    }
}

