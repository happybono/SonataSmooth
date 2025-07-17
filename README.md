# SonataSmooth
This tool implements four different noise reduction algorithms for smoothing data : rectangular (uniform) mean, weighted median, binomial (Gaussian-like) average, and Savitzky-Golay polynomial smoothing. It processes data from a list and displays the results in another list.

<div align="center">
<img alt="GitHub Last Commit" src="https://img.shields.io/github/last-commit/happybono/SonataSmooth"> 
<img alt="GitHub Repo Size" src="https://img.shields.io/github/repo-size/happybono/SonataSmooth">
<img alt="GitHub Repo Languages" src="https://img.shields.io/github/languages/count/happybono/SonataSmooth">
<img alt="GitHub Top Languages" src="https://img.shields.io/github/languages/top/HappyBono/SonataSmooth">
</div>

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

### v2.0.0.0
#### July 08, 2025
> Async & Parallel Processing<br><br>
> Batch UI Updates<br><br>
> Stepwise ProgressBar Feedback<br><br>
> True Symmetric Binomial-Weighted Median Filter<br><br>
> ListBox Selection & Deletion Optimization<br><br>
> Regex Performance Tuning<br><br>
> UI-Thread Responsiveness<br><br>
> Median Filter Bias (Fixed the original code’s one-sided kernel bug to correctly include both left and right neighbors in the weighted median.)<br><br>
> Binomial Coefficient Indexing (Resolved mis-mapping by removing unnecessary sort / reverse and using symmetric indexing (binom[k + w]).<br><br>
> UI Flicker Prevention (Added BeginUpdate / EndUpdate around all ListBox modifications to eliminate redraw artifacts.)<br><br>

### v3.0.0.0
#### July 17, 2025
> Overhauled the graphical user interface.<br><br>
> Fixed an issue where the application became unresponsive when calibrating large datasets (over 100,000 entries) with the Noise Reduction Kernel Width set to 7 or higher using the Weighted Median method.<br><br>
> Fixed an issue where the txtVariable textbox was not being cleared after its contents were added to the ListBox.<br><br>
> Reimplemented and optimized the weighted-median calibration algorithm’s procedures, reducing processing time by more than a factor of 16.<br><br>
> Fixed a bug in the median-based calibration algorithm that prevented it from producing correct corrected values.<br><br>
> Minor bugs fixed.<br>
</details>

## Features & Algorithms
### 1. Initialization & Input Processing
#### How it works
When the user clicks **Calibrate**, the handler reads all numeric items from `listBox1`, parses the kernel size from a combo box, computes binomial weights, and sets up a progress reporter for the UI.

#### Principle
Prepare raw data and parameters before any heavy computation. Converting inputs to a simple `double[]`, determining the kernel “radius” **w**, and generating the binomial weight array ensure that the parallel filtering step has everything it needs.

#### Code Implementation
```csharp
// 1. Count input values
int n = listBox1.Items.Count;

// 2. Copy and convert to double[]
var input = new double[n];
for (int i = 0; i < n; i++)
    input[i] = Convert.ToDouble(listBox1.Items[i], CultureInfo.InvariantCulture);

// 3. Read kernel radius w (half-width)
int w = int.Parse(cbxKernelWidth.Text, CultureInfo.InvariantCulture);

// 4. Generate binomial coefficients of length 2 × w + 1
int[] binom = CalcBinomialCoefficients(2 * w + 1);

// 5. Set up a progress reporter for thread-safe UI updates
var progressReporter = new Progress<int>(pct =>
{
     progressBar1.Value = Math.Max(0, Math.Min(100, pct));
});
```

### 2. Parallel Kernel Filtering
#### How it works
All array indices [0 ... n - 1] are processed in parallel using PLINQ. For each position i, the code checks which radio button is selected (rectangular average, weighted median, or binomial average) and computes a filtered value.

#### Principle
Leverage all CPU cores to avoid blocking the UI. PLINQ's .AsOrdered() preserves the original order, and .WithDegreeOfParallelism matches the number of logical processors.

#### Code Implementation
```csharp
double[] results = await Task.Run(() =>
    ParallelEnumerable
        .Range(0, n)                                       // indices 0 ... n - 1
        .AsOrdered()                                       // keep order
        .WithDegreeOfParallelism(Environment.ProcessorCount)
        .Select(i =>
        {
            double value = 0;

            // (Filter implementations go here...)

            return value;
        })
        .ToArray()
);
```

### 2.1 Rectangular (Uniform) Mean Filter
#### How it works
A simple sliding-window average over 2 × w + 1 points, ignoring out-of-bounds indices.

#### Principle
Every neighbor contributes equally (uniform weights).

#### Code Implementation
```csharp
if (rbtnRect.Checked)
{
    double sum = 0, cnt = 0;
    for (int k = -w; k <= w; k++)
    {
        int idx = i + k;
        if (idx >= 0 && idx < n)
        {
            sum += input[idx];
            cnt++;
        }
    }
    if (cnt > 0)
        value = sum / cnt;
}
```

### 2.2 Weighted Median Filter
#### How it works
Each neighbor’s value is replicated according to its binomial weight, then the combined list is sorted to pick the median.

#### Principle
Median filtering is robust against outliers; binomial weights bias the median toward center points.

#### Code Implementation
```csharp
private static double WeightedMedianAt(double[] data, int center, int w, int[] binom)
{
    var pairs = new List<(double Value, int Weight)>(2 * w + 1);
    for (int k = -w; k <= w; k++)
    {
        int idx = center + k;
        if (idx < 0 || idx >= data.Length) continue;
        pairs.Add((data[idx], binom[k + w]));
    }
    if (pairs.Count == 0)
        return 0;

    // Sort by value
    pairs.Sort((a, b) => a.Value.CompareTo(b.Value));

    long totalWeight = pairs.Sum(p => p.Weight);
    long half = totalWeight / 2;
    bool isEvenTotal = (totalWeight % 2 == 0);

    long accum = 0;
    for (int i = 0; i < pairs.Count; i++)
    {
        accum += pairs[i].Weight;

        // If cumulative weight exceeds half : crossed the lower portion, return current value
        if (accum > half)
        {
            return pairs[i].Value;
        }

        // If cumulative weight equals exactly half : take average of current and next value
        if (isEvenTotal && accum == half)
        {
            // Bounds check for safety
            double nextVal = (i + 1 < pairs.Count)
                             ? pairs[i + 1].Value
                             : pairs[i].Value;
            return (pairs[i].Value + nextVal) / 2.0;
        }
    }

    // If value still not found after full accumulation, return the maximum
    return pairs[pairs.Count - 1].Value;
}
```

### 2.3 Binomial (Weighted) Average Filter
#### How it works
Multiply each neighbor by its binomial weight, sum them up, then divide by the total weight sum.

#### Principle
A discrete approximation of Gaussian smoothing (binomial coefficients approximate a normal distribution).

#### Code Implementation
```csharp
else if (useAvg)
{
    // 이항 평균
    double sum = 0; int cs = 0;
    for (int k = -w; k <= w; k++)
    {
        int idx = i + k;
        if (idx < 0 || idx >= n) continue;
        double v = input[idx];
        int c = binom[k + w];
        sum += v * c;
        cs += c;
    }
    return cs > 0 ? sum / cs : 0;
}
```

### 2.4 Savitzky–Golay Filter
#### How it works
Fit a local polynomial of order polyOrder over a window of size 2 * w + 1 and evaluate it at the center. Precomputed SG coefficients are convolved with the data, with mirrored boundaries.

#### Principle
Savitzky–Golay smoothing preserves higher‐order moments (like peaks and widths) better than simple averaging, by performing a least‐squares polynomial fit over the window.

#### Code Implementation
```csharp
else if (useSG)
{
  // Savitzky-Golay 필터
  double sum = 0;
  for (int k = -w; k <= w; k++)
  {
      int idx = i + k;
      double v = (idx < 0) ? input[-idx]
               : (idx >= n) ? input[2 * n - idx - 2]
               : input[idx];
      sum += sgCoeffs[k + w] * v;
  }
  return sum;
}
```

### Results Aggregation & UI Update
#### How it works
After filtering, the results array is handed to AddItemsInBatches, which inserts items into listBox2 in chunks. This avoids freezing the UI and allows incremental progress updates. Finally, controls are reset.

#### Principle
Batch updates and progress reporting keep the UI responsive. A finally block ensures the progress bar always resets on completion or error.

#### Code Implementation
```csharp
// Add filtered values to listBox2 in batches (with progress)
await AddItemsInBatches(listBox2, results, progressReporter);

// Update count label and disable buttons
lblCnt2.Text = "Count : " + listBox2.Items.Count;
slblCalibratedType.Text = useRect ? "Rectangular Average" :
                            useMed ? "Weighted Median" :
                            useAvg ? "Binomial Average" :
                            useSG ? "Savitzky-Golay Filter" : "Unknown";
slblKernelWidth.Text = w.ToString();

btnCopy2.Enabled = false;
btnSelClear2.Enabled = false;
}
finally
{
    // Always clear the progress bar
    progressBar1.Value = 0;
    btnCalibrate.Enabled = true;
}
```

### Binomial Coefficients Computation
#### How it works
Generates one row of Pascal’s triangle (length = 2 * w + 1) by iteratively applying the binomial recurrence.

#### Principle
Leverage the relation
C(n, k) = C(n, k - 1) × (n - (k - 1)) / k
to compute coefficients in O(n) time without factorials.

#### Code Implementation
```csharp
private int[] CalcBinomialCoefficients(int length)
{
    if (length < 1)
        throw new ArgumentException("length must be ≥ 1", nameof(length));

    var c = new int[length];
    c[0] = 1;                          // C(n, 0) = 1
    for (int i = 1; i < length; i++)
        c[i] = c[i - 1] * (length - i) / i;
    return c;
}
```

### Savitzky–Golay Coefficients Computation
#### How it works
Constructs a Vandermonde matrix for the window, computes its normal equations, inverts the Gram matrix, and multiplies back by the transposed design matrix. The first row of the resulting "smoother matrix" yields the filter coefficients.

#### Principle
Savitzky–Golay filters derive from least‐squares polynomial fitting.
- Build matrix A where each row contains powers of the relative offset within the window.
- Form the normal equations (AᵀA), invert them, and multiply by Aᵀ to get the pseudoinverse.
- The convolution coefficients for smoothing (value at central point) are the first row of this pseudoinverse.

#### Code Implementation
```csharp
private static double[] ComputeSavitzkyGolayCoefficients(int windowSize, int polyOrder)
{
    int m    = polyOrder;
    int half = windowSize / 2;

    // 1) Build Vandermonde matrix A (windowSize × (m + 1))
    double[,] A = new double[windowSize, m + 1];
    for (int i = -half; i <= half; i++)
        for (int j = 0; j <= m; j++)
            A[i + half, j] = Math.Pow(i, j);

    // 2) Compute ATA = Aᵀ * A ((m + 1) × (m + 1))
    double[,] ATA = new double[m + 1, m + 1];
    for (int i = 0; i <= m; i++)
        for (int j = 0; j <= m; j++)
            for (int k = 0; k < windowSize; k++)
                ATA[i, j] += A[k, i] * A[k, j];

    // 3) Invert ATA to get invATA
    double[,] invATA = InvertMatrix(ATA);

    // 4) Compute AT = Aᵀ ((m + 1) × windowSize)
    double[,] AT = new double[m + 1, windowSize];
    for (int i = 0; i <= m; i++)
        for (int k = 0; k < windowSize; k++)
            AT[i, k] = A[k, i];

    // 5) Compute filter coefficients h[k] = sum_j invATA[0, j] * AT[j, k]
    var h = new double[windowSize];
    for (int k = 0; k < windowSize; k++)
    {
        double sum = 0;
        for (int j = 0; j <= m; j++)
            sum += invATA[0, j] * AT[j, k];
        h[k] = sum;
    }

    return h;
}
```

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

This project delivers significantly cleaner and more reliable data outputs by combining four complementary smoothing strategies : rectangular (uniform) mean, weighted median, binomial (Gaussian-like) average, and Savitzky-Golay polynomial smoothing. 

In particular :
- Uniform mean filtering provides a fast, simple way to suppress random fluctuations.  
- Weighted median filtering adds robustness against outliers by privileging central values.  
- Binomial averaging approximates a Gaussian blur, yielding gentle, natural-looking smoothing.  
- Savitzky–Golay smoothing fits a local low-order polynomial via least-squares, preserving peaks and higher-order signal characteristics while reducing noise.

Beyond the choice of filter, the implementation harnesses parallel processing (PLINQ) to maximize CPU utilization without blocking the UI, and incremental batch updates with a progress reporter keep the application responsive even on large datasets. The adjustable kernel width and polynomial order give users fine-grained control over the degree and nature of smoothing.

Together, these design decisions ensure that noisy inputs are transformed into clearer, more consistent signals : empowering downstream analysis, visualization, or automated decision-making with higher confidence in the results.


## Demonstation
![Final Product](SonataSmooth.png)</br><br>
![Final Product SG](SonataSmoothSG.png)

## License
This project is licensed under the MIT License. See the `LICENSE` file for details.

## Copyright 
Copyright ⓒ HappyBono 2025. All Rights Reserved.
