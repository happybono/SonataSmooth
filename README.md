# SonataSmooth
This tool implements three different noise reduction algorithms for smoothing out data: Rectangular Averaging, Binomial Median Filtering, and Binomial Averaging. It processes data from a list and displays the results in another list.

## Algorithms
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
  - Sort the collected values and find the median (middle value).
  - If the number of values is even, calculate the average of the two middle values.
  - Add the median value to `listBox2`.

### 3. Binomial Averaging Algorithm
- **Description**: Uses binomial coefficients to calculate a weighted average of the data points within a specified kernel width.
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
