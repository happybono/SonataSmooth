# SonataSmooth
This tool implements three different noise reduction algorithms for smoothing data: Rectangular Averaging, Binomial Median Filtering, and Binomial Averaging. It processes data from a list and displays the results in another list.

## Features & Algorithms
### 1. Rectangular Averaging Algorithm
- **Description**: Calculates the average of the data points within a specified kernel width.
- **How It Works**:
  - For each data point in `listBox1`, sum the values within the range defined by `NoiseReductionKernelWidth`.
  - Divide the sum by the number of data points considered to get the average.
  - Add the average value to `listBox2`.

### 2. Binomial Median Filtering Algorithm
- **Description**: Calculates the median of the data points within a specified kernel width.
- **How It Works**:
  - For each data point in `listBox1`, collect values within the range defined by `NoiseReductionKernelWidth`.
  - Since `NoiseReductionKernelWidth` ensures an odd number of values, there will always be a single middle value. This is because the range `[-NoiseReductionKernelWidth, NoiseReductionKernelWidth]` always includes the center point (0) and extends an equal number of points on either side, resulting in an odd total number of points.
  - Add the median value to `listBox2`.

### 3. Binomial Averaging Algorithm
- **Description**: Use binomial coefficients to calculate a weighted average of the data points within a specified kernel width.
- **How It Works**:
  - Calculate binomial coefficients for the given kernel width.
  - For each data point in `listBox1`, multiply the values within the range defined by `NoiseReductionKernelWidth` by the corresponding binomial coefficients.
  - Sum these weighted values and divide by the sum of the coefficients to get the weighted average.
  - Add the weighted average value to `listBox2`.

## Usage
1. **Kernel Width Selection**: Input the desired kernel width in the `cbxKernelWidth` combo box.
2. **Select Algorithm**: Choose one of the radio buttons (`rbtnRect`, `rbtnMed`, `rbtnAvg`) to select the noise reduction algorithm.
3. **Calibrate**: Click the `btnCalibrate` button to process the data in `listBox1` using the selected algorithm. The processed values will be displayed in `listBox2`.

These algorithms help smooth out noise in data by averaging or filtering based on the chosen method.

## Demonstation
![Final Product](SonataSmooth.png)

## Copyright / End User License
### Copyright
Copyright ⓒ HappyBono 2020 - 2025. All Rights Reserved.

### MIT License
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

## Contact Information
[Jaewoong Mun](mailto:happybono@outlook.com)
