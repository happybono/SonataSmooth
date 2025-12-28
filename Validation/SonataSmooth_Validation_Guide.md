# SonataSmooth Validation Guide

## Purpose
Validate numerical equivalence between **SonataSmooth (C#)** outputs and **MATLAB** reference outputs using CSV artifacts and an Excel verification template.

## Acceptance Criterion
- **tol = 1e - 10**
- **100% match (MismatchCount = 0)** is treated as *numerically equivalent*.
- Note : **tol = 0** (bitwise exact) may show tiny floating-point round-off differences.

## Inputs
- `SonataSmooth_Result.csv` (exported from SonataSmooth / SonataSmooth.Tune)
- `MATLAB_Result.csv` (exported from MATLAB reference pipeline)

## Output
- Updated verification summary in the Excel template :
  - `Overview` sheet : `Match%`, `MatchCount`, `MismatchCount`, `MaxAbsDiff`, `MAE`

---

## Usage (Step-by-Step)

### 1. Generate the MATLAB reference CSV
1. Run the MATLAB reference pipeline to create `MATLAB_Result.csv`.
   - Run this script :
     [`SonataSmooth_Validation.m`](https://github.com/happybono/SonataSmooth/blob/main/Validation/Matlab/SonataSmooth_Validation.m)

### 2. Export the SonataSmooth CSV
1. Run SonataSmooth with the same input + parameters used in MATLAB.
2. Export results to `SonataSmooth_Result.csv`.

> **Important** : Both CSVs must be produced with the same row order (Index-aligned) so that row-by-row comparison is valid.

### 3. Paste CSV data into the Excel validation template
1. Open the Excel verification template and go to the **`SonataSmooth_Result`** sheet.  
2. Paste values starting from **row 16** (down to the last sample row).  
3. Paste the **original input series** into :
   - Column **A** (row 16 → last row)

4. Paste the **result pairs** for each method into the following column pairs (row 16 → last row) :

   - Rectangular Averaging → **C : D**
   - Binomial Averaging → **F : G**
   - Binomial Median Filtering → **I : J**
   - Gaussian Weighted Median Filtering → **L : M**
   - Gaussian Filtering → **O : P**
   - Savitzky–Golay Filtering → **R : S**

5. **Column mapping rule (fixed)** : 
   - **Left column = SonataSmooth**
   - **Right column = MATLAB**

   Examples : 
   - `C` = SonataSmooth Rectangular Averaging, `D` = MATLAB Rectangular Averaging  
   - `F` = SonataSmooth Binomial Averaging, `G` = MATLAB Binomial Averaging  
   - … use the same left / right mapping for every method pair.

> Recommended : Use **Paste Special → Values** to avoid bringing in formatting / formulas.

### 4) Refresh calculations and PivotTables
1. In Excel, run **Data → Refresh All**.
2. If PivotTables are used, you can also right-click the PivotTable → **Refresh**.

### 5) Check the summary in `Overview`
1. Go to the **`Overview`** sheet and confirm metrics are updated : 
   - **Match%**
   - **MatchCount**
   - **MismatchCount**
   - **MaxAbsDiff**
   - **MAE**
2. A successful run should show : 
   - **MismatchCount = 0**
   - **Match% = 100% **  
   for every method (and Grand Total).

---

## Troubleshooting (Quick Checks)
- **Mismatch appears only for one method** : verify that method's column pair was pasted into the correct columns and the left / right mapping was not swapped.
- **Mismatch everywhere** : confirm both CSVs use the same input, same parameters, and same row order (Index-aligned).
- **Overview not updating** : make sure you ran **Data → Refresh All**.
