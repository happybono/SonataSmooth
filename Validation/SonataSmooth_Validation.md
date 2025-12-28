# SonataSmooth_Validation

## Purpose
Validate numerical equivalence between SonataSmooth outputs and MATLAB reference implementation outputs using CSV artifacts.

## What it does
- Loads two result files :  
  1. `SonataSmooth_Result.csv` (exported from SonataSmooth)  
  2. `MATLAB_Result.csv` (exported from MATLAB reference script)  
- Aligns rows by **Index (1-based)** and compares key output columns :  
  - `RectAvg`, `BinomAvg`, `BinomMedian`, `GaussWMedian`, `Gauss`, `SG`  
- Computes per-filter error metrics :  
  - **MaxAbsDiff**, **MeanAbsDiff**, **RMSE**, mismatch counts under a tolerance  
- Generates a verification report (e.g., `.xlsx` / `.json`) for reproducibility.

## Default Acceptance Criterion
- **tol = 1e - 10** : 0 mismatches (100% match) is treated as *numerically equivalent*.  
- Note : **tol = 0 (bitwise exact)** may show tiny floating-point round-off differences.

## Inputs / Outputs
**Inputs** :
- `SonataSmooth_Result.csv`
- `MATLAB_Result.csv`

**Outputs (Recommended)** : 
- `MATLAB_vs_SonataSmooth_Verification_Report.xlsx`
- `verification_summary.json`

## Usage
1. Run the MATLAB reference pipeline to create `MATLAB_Result.csv`  
2. Export SonataSmooth results to `SonataSmooth_Result.csv`  
3. Run this script :  
   [SonataSmooth_Validation.m](https://github.com/happybono/SonataSmooth/blob/main/Validation/Matlab/SonataSmooth_Validation.m)
