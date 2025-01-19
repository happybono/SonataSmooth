# SonataSmooth
This tool implements three different noise reduction algorithms for smoothing data: Rectangular Averaging, Binomial Median Filtering, and Binomial Averaging. It processes data from a list and displays the results in another list.

## What's New
<details>
<summary>Click to Expand</summary>
  
### v1.0.0.0
#### January 19, 2025
>[Initial release.](https://github.com/happybono/SonataSmooth/commit/1c9911992e2b0ec6b984828519ac78cbcb5a0a51)<br>

### v1.0.1.0
#### January 19, 2025
> [Minor bugs fixed.](https://github.com/happybono/SonataSmooth/commit/a8a9cfd481aa7616bdbc14e27d71a9a6616d171b)<br><br>
> [Explained NoiseReductionKernelWidth and updated algorithm details in README.md.](https://github.com/happybono/SonataSmooth/commit/dbad0337d5c7534902db7f22f6dc23ff60a54a4e)

### v1.0.2.0
#### January 20, 2025
> [Bugs fixed.](https://github.com/happybono/SonataSmooth/commit/f7d0568b4ebf30ed7868885a9bff92960e757b13)<br>
</details>

## Features & Algorithms
1. **Rectangular Method**
   - **How it works**: This method calculates the average of a window of values centered around each data point. The width of the window (kernel width) determines the number of neighboring data points included in the averaging process.
   - **Principle**: By averaging the values within the window, the noise (random fluctuations) is smoothed out, resulting in a clearer, more stable signal.
   - **Code Implementation**:
     ```csharp
     // Rectangular
     if (rbtnRect.Checked == true)
     {
         for (int i = 0; i < listBox1.Items.Count; i++)
         {
             double sum = 0;
             int count = 0;

             for (int kernel_index = -NoiseReductionKernelWidth; kernel_index <= NoiseReductionKernelWidth; kernel_index++)
             {
                 int dataIndex = i + kernel_index;
                 if (dataIndex >= 0 && dataIndex < listBox1.Items.Count)
                 {
                     count++;
                     sum += Convert.ToDouble(listBox1.Items[dataIndex]);
                 }
             }

             listBox2.Items.Add(sum / count);
             lblCnt2.Text = "Count : " + listBox2.Items.Count;
         }
     }
     ```

2. **Binomial Coefficients Method (Median)**
   - **How it works**: This method uses the binomial coefficients to weight the data points within the window. The median of the weighted values is then calculated.
   - **Principle**: The median is less sensitive to extreme values (outliers) than the average, making it effective for noise reduction while preserving the overall trend of the data.
   - **Code Implementation**:
     ```csharp
     // Binomial coefficient (median)
     else if (rbtnMed.Checked == true)
     {
         for (int i = 0; i < listBox1.Items.Count; i++)
         {
             List<double> values = new List<double>();

             for (int kernel_index = -NoiseReductionKernelWidth; kernel_index <= NoiseReductionKernelWidth; kernel_index++)
             {
                 int dataIndex = i + kernel_index;
                 if (dataIndex >= 0 && dataIndex < listBox1.Items.Count)
                 {
                     values.Add(Convert.ToDouble(listBox1.Items[dataIndex]));
                 }
             }

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

                 listBox2.Items.Add(median);
                 lblCnt2.Text = "Count : " + listBox2.Items.Count;
             }
         }
     }
     ```

3. **Binomial Coefficients Method (Average)**
   - **How it works**: This method uses binomial coefficients to weight the values within the kernel. The weighted average of these values is then calculated.
   - **Principle**: Binomial coefficients give more weight to the central values in the window, which helps to preserve the central trend while smoothing out noise.
   - **Code Implementation**:
     ```csharp
     // Binomial coefficient (average)
     else if (rbtnAvg.Checked == true)
     {
         binomialCoefficients = AvgCalcBinomialCoefficients(NoiseReductionKernelWidth * 2 + 1);
         for (int i = 0; i < listBox1.Items.Count; i++)
         {
             List<double> values = new List<double>();
             int coefficientSum = 0;

             for (int kernel_index = -NoiseReductionKernelWidth; kernel_index <= NoiseReductionKernelWidth; kernel_index++)
             {
                 int dataIndex = i + kernel_index;
                 if (dataIndex >= 0 && dataIndex < listBox1.Items.Count)
                 {
                     values.Add(Convert.ToDouble(listBox1.Items[dataIndex]) * binomialCoefficients[NoiseReductionKernelWidth + kernel_index]);
                     coefficientSum += binomialCoefficients[NoiseReductionKernelWidth + kernel_index];
                 }
             }

             if (values.Count > 0)
             {
                 double sum = values.Sum();
                 double average = sum / coefficientSum;

                 listBox2.Items.Add(average);
                 lblCnt2.Text = "Count : " + listBox2.Items.Count;
             }
         }
     }
     ```

### Binomial Coefficients Calculation
1. **Calculating Binomial Coefficients**
   - **Principle**: Binomial coefficients are derived from Pascal's triangle and represent the coefficients in the expansion of a binomial expression.
   - **Code Implementation**:
     ```csharp
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
     ```

These algorithms help smooth out noise in data by averaging or filtering based on the chosen method.

### Data Handling and Processing
- Implemented drag-and-drop functionality to allow users to easily add data to the application.
- Used regular expressions to extract and parse numerical data from various formats.

### User Interface and Interaction
- Designed and developed a user-friendly interface with interactive elements like buttons and list boxes.
- Provided real-time feedback to the user by updating counts and results dynamically.

### Customization and Configuration
- Allowed users to select the noise reduction method and kernel width through the interface.
- Enabled users to calibrate and fine-tune the noise reduction process based on their specific needs.

## Conclusion
By implementing these techniques, this project effectively reduces noise from the given data, providing clearer and more reliable results.

## Demonstation
![Final Product](SonataSmooth.png)

## Copyright / End User License
### Copyright
Copyright ⓒ HappyBono 2025. All Rights Reserved.

### MIT License
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

## Contact Information
[Jaewoong Mun](mailto:happybono@outlook.com)
