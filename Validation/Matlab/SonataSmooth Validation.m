
%% SonataSmooth (C# ApplySmoothing-compatible) MATLAB Reference
% - Matches BoundaryMode mapping + Adaptive procedures + Alpha blend
% - Filters: RectAvg, BinomAvg, BinomMedian, GaussWMedian, Gauss, Savitzky-Golay
% - Boundary modes: Symmetric, Replicate, Adaptive, ZeroPad
%
% IMPORTANT :
% - Adaptive (Rect / BinomAvg / BinomMedian / GaussWMed / Gauss): center fixed, window shrinks :
%       left = min(r, i0), right = min(r, n - 1 - i0), start = i0 - left
% - Adaptive (SG) : fixed length (2r + 1) but shift window to stay inside [0 .. n - 1]
% - Binomial / Gaussian kernels in Adaptive are RECOMPUTED for local W (not slice + renorm)
% - Binomial weighted median tie-break: if totalWeight even & accum == half -> average (current, next)
% - Gaussian weighted median: sort(values, weights), pick first where accum >= half (NO averaging)

clear; clc;

%% -------------------- USER INPUT & SAMPLE DATA --------------------
% Sample Data
x = [5,5,5,6,5,5,6,5,5,5,5,5,5,5,4,6,6,5,4,5,4,5,5,4,4,5,5,5,5,5,5,5,5,5,7,5,5,6,5,5,5,5,5,6,5,5,4,5,6,4,5,5,5,5,6,6,6,4,5,5,5,5,5,5,6,4,5,7,6,5,5,6,4,5,5,5,5,4,4,5,4,4,5,4,4,4,4,5,4,5,5,4,5,5,5,4,4,4,5,4,6,5,5,5,4,4,4,4,4,4,4,4,4,4,4,4,4,5,5,4,4,4,4,4,5,4,4,5,4,4,4,4,4,4,3,4,4,4,4,4,4,4,3,3,4,5,3,3,4,4,4,4,4,3,3,3,3,3,3,4,4,4,4,3,3,4,4,4,4,4,4,3,4,4,5,4,4,4,4,4,4,5,3,3,3,4,4,3,4,5,5,5,5,4,4,5,5,4,4,5,5,4,4,4,4,4,4,4,5,4,4,5,4,4,4,4,5,4,4,5,5,5,5,4,5,5,4,4,4,5,4,4,4,4,5,4,5,5,5,6,6,6,5,5,5,5,5,6,6,6,6,6,6,7,6,7,6,6,6,5,6,6,7,6,7,7,6,6,6,6,6,6,8,5,7,6,7,6,6,6,6,6,6,7,7,6,6,6,6,6,6,6,7,6,5,5,6,7,6,5,5,6,6,6,6,6,5,5,6,5,6,6,6,5,6,6,6,5,6,7,6,6,6,7,7,7,6,5,6,6,6,6,6,5,6,6,5,5,5,5,6,5,6,6,10,6,5,6,6,5,5,5,5,6,6,5,5,5,6,5,6,6,7,7,7,6,7,6,7,7,7,8,7,8,6,6,7,7,7,7,8,7,7,7,7,6,7,6,6,6,6,6,7,7,6,6,6,7,7,7,7,7,6,7,7,6,6,7,6,6,6,6,5,6,7,6,6,6,7,6,5,6,6,6,6,7,6,6,6,6,6,6,6,6,7,6,6,6,7,6,5,6,5,5,5,6,6,6,5,7,5,6,8,6,5,5,6,6,5,1,8,7,8,7,9,8,8,8,6,8,6,8,6,6,6,7,5,6,7,9,6,6,5,7,6,6,5,6,5,6,6,6,6,6,7,7,6,6,6,6,5,6,8,8,9,7,7,9,7,8,7,9,11,15,10,8,8,9,8,8,6,7,7,7,7,6,8,6,7,6,8,6,9,9,7,7,7,5,8,8,7,7,6,8,10,7,6,6,10,8,6,8,6,7,8,7,7,8,8,7,7,7,6,8,8,8,6,5,7,7,7,8,8,6,7,8,6,8,8,8,6,6,6,6,8,6,7,5,6,6,5,6,7,5,5,5,5,7,5,7,6,5,5,6,8,8,7,7,7,5,8,7,7,7,7,7,7,6,7,6,14,7,6,7,8,5,5,6,5,6,6,5,6,6,6,6,6,6,6,8,7,7,7,6,7,6,8,7,7,7,7,5,7,5,7,6,7,7,7,8,9,7,7,7,6,7,8,6,8,7,7,7,6,6,5,8,7,9,8,8,8,6,9,7,8,7,8,7,8,8,10,7,8,7,9,7,8,8,8,6,9,9,7,7,7,8,7,7,8,7,6,7,9,8,8,8,9,7,8,6,7,7,7,8,10,7,6,8,8,6,11,8,8,7,8,7,6,6,6,7,6,7,9,7,8,8,9,6,8,7,8,8,7,6,7,6,7,8,6,8,9,8,7,6,7,5,6,5,6,6,6,6,6,6,6,7,6,7,7,6,6,7,7,9,7,7,6,7,7,8,7,7,6,8,7,8,9,7,7,9,7,8,6,8,6,7,8,7,8,8,7,6,8,8,8,9,6,12,8,8,6,7,7,6,7,8,9,8,8,7,6,7,7,7,6,7,7,6,5,7,6,6,6,8,7,6,8,6,8,9,7,8,6,7,8,7,6,7,7,8,6,6,6,6,8,6,7,8,7,8,7,7,7,6,5,6,7,7,5,6,6,6,6,7,7,6,7,6,7,6,7,7,6,5,7,7,6,6,7,7,7,7,7,7,9,5,6,6,6,6,4,4,6,6,4,5,6,7,5,5,6,6,6,5,6,5,5,6,5,6,5,7,6,5,6,5,5,7,6,6,7,5,8,6,5,6,6,5,6,5,5,6,8,5,6,6,5,7,5,6,6,5,6,7,6,6,5,5,7,6,5,4,5,6,6,4,5,4,5,4,5,4,5,6,6,7,6,5,6,5,6,6,5,6,6,5,5,6,7,6,6,7,5,6,6,5,6,7,5,7,6,5,7,5,6,6,6,5,4,5,5,5,6,5,5,6,5,4,6,5,6,5,4,5,6,4,6,5,6,5,4,5,5,5,4,5,5,5,5,5,5,4,5,5,4,5,5,5,5,5,5,5,4,4,5,5,4,5,4,5,4,5,5,4,4,5,5,5,4,5,6,5,6,5,6,4,6,5,5,6,6,5,5,7,7,7,7,8,5,7,7,7,7,7,7,10,8,8,7,8,7,7,7,8,10,8,9,9,11,8,9,7,10,9,8,8,8,8,9,10,8,8,7,7,9,10,6,7,6,8,7,6,5,6,7,7,7,7,8,8,6,6,6,6,5,6,5,6,6,8,5,8,6,5,5,6,5,6,5,5,6,6,6,7,6,5,5,7,5,6,24,14,10,6,8,7,7,5,6,6,6,6,7,6,6,7,6,7,6,7,6,6,6,6,6,7,5,7,8,6,6,5,6,6,5,6,6,6,7,6,6,6,6,6,7,6,5,6,5,5,5,6,5,5,5,6,5,5,6,8,6,7,7,6,6,6,5,6,6,5,6,6,5,6,6,6,6,5,6,6,6,6,6,6,7,6,6,7,6,6,7,7,7,8,7,7,7,6,7,8,7,7,8,7,7,8,7,7,7,7,7,6,7,7,7,7,6,6,6,7,7,7,6,7,7,6,6,7,6,10,8,8,8,8,7,7,7,8,7,7,8,7,7,8,7,7,8,8,7,7,8,7,7,7,10,11,9,8,8,7,7,8,9,10,9,10,9,9,9,9,9,9,9,9,10,9,9,9,9,10,10,9,10,9,9,10,9,10,10,9,9,9,9,9,9,9,11,9,8,14,10,8,8,8,10,9,8,10,9,8,8,9,9,8,8,10,9,8,9,7,8,9,8,8,8,8,8,8,8,7,8,8,9,9,8,8,8,8,9,10,8,8,8,11,9,9,8,8,8,8,8,8,8,8,8,8,7,7,7,8,8,8,7,7,7,7,8,7,7,8,7,7,8,7,7,7,7,7,7,8,8,8,8,8,8,8,9,9,8,9,8,8,9,9,8,10,9,9,8,9,9,10,9,8,9,9,8,8,7,8,8,7,8,8,7,8,8,7,7,8,7,7,7,8,7,9,7,7,7,8,7,8,9,9,10,10,10,10,9,10,10,9,10,11,10,10,11,10,9,9,10,9,11,10,10,10,9,10,10,9,9,10,10,9,9,9,10,10,10,11,10,10,9,9,9,9,9,9,8,10,8,7,8,8,7,7,8,9,8,8,9,9,9,9,7,8,8,9,8,9,8,9,8,8,8,8,8,8,8,8,9,8,12,9,9,8,8,8,8,9,8,9,8,8,9,9,9,9,8,8,8,8,8,8,8,8,8,9,9,8,9,9,8,8,8,8,8,8,7,7,7,7,7,6,8,7,7,7,7,8,7,8,7,6,7,7,7,6,6,5,6,7,7,7,7,7,6,7,7,6,6,10,7,8,8,8,7,8,7,7,9,8,8,7,7,7,7,7,8,7,7,7,8,8,7,9,8,7,7,7,9,8,8,8,8,8,8,8,7,8,8,8,9,9,9,10,9,9,9,9,9,8,8,8,9,8,8,9,8,8,9,9,9,8,8,9,9,8,8,8,8,8,8,8,10,9,8,8,8,9,9,9,8,8,9,9,9,9,8,10,9,11,11,11,10,9,10,9,9,9,10,11,9,9,8,9,10,10,10,9,9,9,10,9,12,11,10,11,10,11,13,11,10,11,12,10,11,11,12,14,11,11,10,11,12,11,12,13,13,11,13,12,14,13,14,12,12,13,14,13,12,11,12,12,12,12,12,13,14,13,13,13,14,16,14,14,13,14,13,14,12,14,13,12,13,12,12,14,13,13,13,15,14,15,15,14,15,16,15,16,13,14,14,16,16,16,16,14,14,15,15,16,16,15,15,15,14,14,15,14,13,13,14,13,12,12,13,13,13,13,12,13,15,15,15,13,14,14,15,13,14,13,14,14,14,15,15,13,15,16,14,15,14,15,15,15,13,15,16,15,14,14,14,18,14,14,13,13,13,13,14,13,14,14,13,14,14,14,14,14,14,4,16,18,18,17,15,16,16,15,16,16,16,16,16,16,17,16,17,17,16,16,17,16,16,16,15,15,16,17,18,16,18,16,16,15,17,16,15,16,16,17,17,16,16,16,16,17,16,16,17,17,18,18,18,19,17,17,17,16,17,19,18,18,19,18,18,18,18,18,19,20,19,19,19,20,20,25,28,21,20,21,20,21,19,21,18,18,16,18,19,20,20,20,20,20,19,20,17,19,18,17,17,18,18,20,18,19,20,20,20,19,18,19,19,18,19,19,18,19,19,21,20,20,20,18,20,19,19,18,18,18,18,17,18,18,19,19,17,17,18,17,18,17,18,17,16,16,17,16,17,16,16,16,16,15,16,16,17,15,17,16,16,15,15,15,16,15,15,15,16,15,15,15,15,16,15,15,15,14,15,14,16,16,16,16,16,15,15,16,16,16,17,16,16,16,17,15,16,16,16,17,17,16,16,16,15,16,15,16,15,15,16,14,15,15,16,15,16,16,15,14,15,15,16,16,15,16,15,16,15,14,15,15,16,15,15,17,15,16,16,18,17,16,17,19,18,18,18,18,16,17,16,18,18,18,20,18,20,20,23,21,22,23,20,22,20,21,22,20,23,25,25,24,22,21,22,22,23,22,19,22,21,22,21,21,22,22,23,21,22,22,20,21,22,22,21,19,18,19,18,19,16,18,18,17,16,16,15,15,14,13,13,12,11,11,11,12,14,12,12,12,12,12,12,13,12,12,11,12,11,11,11,10,11,10,11,11,10,10,11,10,12,11,13,14,14,11,11,10,11,12,10,11,12,12,12,12,12,11,12,12,11,11,12,12,10,11,11,11,11,12,12,11,11,11,11,12,12,12,11,12,11,12,12,13,13,12,12,12,12,12,11,11,11,11,11,11,11,11,11,12,12,11,11,11,11,11,11,11,11,12,10,10,10,11,10,11,10,11,11,10,10,10,11,11,12,15,12,11,10,11,10,10,10,11,10,10,11,10,11,11,11,13,12,10,11,10,11,11,10,10,11,10,10,11,10,9,9,10,9,9,10,11,11,9,10,11,11,10,10,10,11,11,10,10,11,11,11,10,11,10,10,10,11,11,11,11,12,11,10,11,11,11,11,11,11,11,11,11,11,10,10,10,9,12,10,10,10,12,10,11,11,11,11,11,12,12,11,11,11,11,10,11,11,11,10,11,11,11,11,12,12,12,12,11,11,12,12,11,11,12,11,11,11,12,11,11,10,11,11,11,11,11,12,10,11,10,11,11,11,10,11,10,11,11,11,11,11,10,10,10,11,12,13,12,12,11,11,10,12,12,10,11,10,11,11,11,11,11,10,11,12,11,11,11,12,11,11,12,11,11,12,11,11,12,11,11,10,11,10,11,12,12,12,14,11,11,12,11,10,11,12,14,11,11,12,10,11,11,11,11,12,11,11,11,11,11,10,11,11,10,11,11,11,13,11,13,10,11,11,12,12,11,11,11,12,12,12,10,11,12,12,11,11,11,12,11,12,11,11,12,11,11,13,10,10,11,11,10,11,12,11,9,12,10,11,10,11,11,11,10,11,11,10,11,11,11,11,11,11,11,11,10,9,11,10,10,13,11,11,11,11,11,10,10,10,10,10,11,11,10,10,10,10,9,11,10,10,9,10,11,13,10,10,9,10,10,11,12,10,10,10,10,10,10,11,10,10,10,10,11,11,10,10,10,11,10,11,9,9,11,9,10,11,10,11,10,11,11,9,10,10,9,9,9,10,13,10,11,10,9,12,10,11,11,11,10,12,10,10,11,10,12,12,10,11,11,12,12,10,11,11,11,11,10,11,11,11,11,11,10,11,10,10,10,10,12,10,12,11,12,9,12,11,12,10,10,11,11,11,10,11,10,12,12,13,11,10,10,10,10,11,10,11,11,12,11,12,11,10,10,11,12,11,12,12,10,11,11,12,10,12,13,11,10,11,10,9,10,11,11,10,9,10,11,9,11,10,11,10,11,10,12,11,10,10,11,10,12,11,11,11,10,11,11,13,10,10,10,11,12,11,11,11,12,14,11,11,12,11,10,11,11,10,11,10,12,11,12,15,12,11,12,10,11,11,11,11,37,19,13,10,11,13,11,11,13,12,12,11,11,11,12,11,12,12,12,10,12,10,10,11,11,12,12,10,11,10,11,12,11,11,11,9,11,11,10,11,11,13,11,11,11,14,11,14,12,12,12,12,11,10,11,11,12,10,11,12,11,13,11,10,11,11,12,11,12,11,13,12,13,12,12,11,12,12,12,11,11,12,11,14,13,14,13,13,11,12,13,14,13,11,13,11,12,15,13,11,13,13,13,14,12,14,13,13,15,15,14,13,13,14,13,13,14,14,13,14,14,14,16,15,17,14,13,13,13,14,14,13,15,15,14,16,13,16,13,13,14,14,13,15,15,14,13,14,12,14,14,16,16,15,16,15,16,15,16,15,15,15,17,16,16,16,17,18,18,19,17,15,16,17,15,16,16,16,17,16,19,17,16,17,18,18,16,19,17,18,16,20,39,19,16,17,17,16,18,16,19,17,19,15,22,18,21,18,20,20,17,16,16,17,16,18,17,17,16,17,16,15,18,15,16,16,16,15,16,15,15,16,15,17,16,17,16,15,16,17,16,16,16,18,14,15,15,16,16,17,13,14,14,16,14,13,14,14,13,15,14,15,13,13,13,13,14,13,13,13,14,14,13,13,14,14,15,13,13,15,13,16,15,12,13,13,14,13,12,11,13,13,12,12,13,13,16,12,12,12,12,13,12,12,12,13,14,12,13,13,14,14,13,14,13,13,14,14,13,13,14,13,16,13,14,14,13,15,14,14,14,13,13,14,14,15,13,13,13,14,14,14,15,14,14,13,13,16,14,13,14,13,12,12,12,12,14,13,12,14,14,15,14,16,14,15,13,15,14,13,16,12,13,13,14,15,15,15,14,13,13,15,14,15,14,13,13,13,14,14,12,13,13,13,13,12,14,14,13,15,15,14,15,14,13,14,16,14,14,15,16,17,16,15,16,15,14,15,15,16,16,16,15,16,16,16,16,18,18,16,18,18,16,18,17,17,16,18,17,16,17,17,16,17,16,16,18,17,17,18,18,19,18,18,18,17,18,19,16,19,19,18,17,18,18,18,17,19,19,20,18,20,18,20,18,17,19,17,18,19,18,18,19,20,34,20,22,15,16,18,19,19,18,17,18,19,19,19,19,19,17,17,17,19,17,17,17,17,18,17,17,17,18,17,17,16,16,17,15,16,17,16,16,16,17,14,16,16,16,16,16,16,16,16,17,17,17,15,17,16,16,3,2,12,13,13,13,12,11,12,11,11,11,11,11,11,11,13,12,12,11,12,13,12,13,11,10,12,10,11,10,10,12,10,10,11,13,11,12,11,13,10,11,11,13,9,12,12,13,11,12,11,11,11,11,10,12,11,13,11,13,11,11,10,12,12,11,10,11,10,11,11,12,13,10,12,10,13,12,11,10,10,11,10,11,10,11,12,12,10,12,10,9,11,11,11,9,10,9,12,11,12,13,11,11,9,11,9,10,10,11,10,11,10,9,11,11,11,11,10,10,11,10,9,11,10,12,10,13,11,12,11,11,12,10,11,13,11,12,9,10,11,11,10,10,10,9,10,12,9,10,11,11,11,12,11,12,11,11,14,12,10,13,11,10,9]';  % <- your data (column)

% Kernel Radius
r = 6;

% Parameters
polyOrder  = 5;
derivOrder = 0;
delta      = 1.0;

alpha = 0.7;           % used for BinomAvg / BinomMedian / GaussWMed / Gauss only
sigmaFactor = 10.0;    % sigma = (2r + 1) / sigmaFactor, allowed range [1.0..12.0]

boundaryMode = "Adaptive";  % "Symmetric" | "Replicate" | "Adaptive" | "ZeroPad"

%% -------------------- RUN --------------------
res = sonata_apply_smoothing_csharp(x, r, polyOrder, derivOrder, delta, boundaryMode, alpha, sigmaFactor);

T = table((1:numel(x))', x, ...
    res.Rect, res.BinomAvg, res.BinomMedian, res.GaussWMed, res.Gauss, res.SG, ...
    'VariableNames', {'Index','Initial','RectAvg','BinomAvg','BinomMedian','GaussWMedian','Gauss','SG'});

disp(T);

%% ============================================================
%% ===================== LOCAL FUNCTIONS ======================
%% ============================================================

function res = sonata_apply_smoothing_csharp(input, r, polyOrder, derivOrder, delta, boundaryMode, alpha, sigmaFactor)
input = input(:);
n = numel(input);

validate_params(n, r, polyOrder, derivOrder, delta, sigmaFactor);
mode = normalize_mode(boundaryMode);

windowSize = 2*r + 1;

% alpha clamp 
a = alpha;
if isnan(a) || isinf(a), a = 1.0; end
a = max(0.0, min(1.0, a));

% Precompute non-adaptive binomial  /  gaussian kernels 
binomFull = calc_binomial_coeffs_long(windowSize); % int64 weights
binomSum  = sum(double(binomFull));

sigmaFull = windowSize  /  sigmaFactor;                       % sigma = (2r + 1) / sigmaFactor (NO square)
gaussFull = compute_gaussian_coeffs(windowSize, sigmaFull);   % normalized double weights

% Outputs
res.Rect        = zeros(n,1);
res.BinomAvg    = zeros(n,1);
res.BinomMedian = zeros(n,1);
res.GaussWMed   = zeros(n,1);
res.Gauss       = zeros(n,1);
res.SG          = zeros(n,1);

% For SG (non-adaptive): compute symmetric SG coefficients once if needed
sgFull = [];
if mode ~= "adaptive"
    sgFull = sg_coeffs_symmetric(windowSize, polyOrder, derivOrder, delta);
end

for i0 = 0:(n-1)  % i0 : 0-based index
    i = i0 + 1;  % MATLAB index

    % ---- RectAvg (no alpha) ----
    if mode == "adaptive"
        [left,right,start0] = adaptive_window_center_fixed(i0, n, r);
        W = left + right + 1;
        if W > 0
            seg = input((start0+1):(start0+W));
            res.Rect(i) = sum(seg)  /  W;
        else
            res.Rect(i) = 0.0;
        end
    else
        s = 0.0;
        for k = -r:r
            s = s + sample_with_boundary(input, i0 + k, mode);
        end
        res.Rect(i) = s  /  windowSize;
    end

    % ---- Binomial Avg (alpha blend) ----
    if mode == "adaptive"
        [left,right,start0] = adaptive_window_center_fixed(i0, n, r);
        W = left + right + 1;
        if W < 1
            filtered = 0.0;
        else
            localBinom = calc_binomial_coeffs_long(W);
            localSum   = sum(double(localBinom));
            seg = input((start0+1):(start0+W));
            filtered = sum(seg(:) .* double(localBinom(:)))  /  localSum;
        end
    else
        s = 0.0;
        for k = -r:r
            v = sample_with_boundary(input, i0 + k, mode);
            s = s + v * double(binomFull(k + r + 1));
        end
        filtered = s  /  binomSum;
    end
    res.BinomAvg(i) = a * filtered + (1.0 - a) * input(i);

    % ---- Binomial Median (alpha blend; tie = average) ----
    if mode == "adaptive"
        [left,right,start0] = adaptive_window_center_fixed(i0, n, r);
        W = left + right + 1;
        if W < 1
            filtered = 0.0;
        else
            localBinom = calc_binomial_coeffs_long(W);
            vals = input((start0+1):(start0+W));
            filtered = weighted_median_long(vals, localBinom); % tie = average
        end
    else
        W = windowSize;
        vals = zeros(W,1);
        wts  = binomFull(:);
        for k = -r:r
            vals(k+r+1) = sample_with_boundary(input, i0 + k, mode);
        end
        filtered = weighted_median_long(vals, wts); % same rule
    end
    res.BinomMedian(i) = a * filtered + (1.0 - a) * input(i);

    % ---- Gaussian Weighted Median (alpha blend; accum >= half; NO tie-average) ----
    if mode == "adaptive"
        [left,right,start0] = adaptive_window_center_fixed(i0, n, r);
        W = left + right + 1;
        if W < 1
            filtered = 0.0;
        else
            sigmaLocal = W  /  sigmaFactor;                       % sigma = W  /  sigmaFactor (NO square)
            wts = compute_gaussian_coeffs(W, sigmaLocal);         % normalized
            vals = input((start0+1):(start0+W));                  % adaptive uses in-range segment
            filtered = weighted_median_double(vals, wts, true);   % use >= half
        end
    else
        W = windowSize;
        vals = zeros(W,1);
        wts  = gaussFull(:);
        for k = -r:r
            vals(k+r+1) = sample_with_boundary(input, i0 + k, mode);
        end
        total = sum(wts);
        if total > 0
            filtered = weighted_median_double(vals, wts, true);   % accum >= half
        else
            filtered = sample_with_boundary(input, i0, mode);     % fallback
        end
    end
    res.GaussWMed(i) = a * filtered + (1.0 - a) * input(i);

    % ---- Gaussian filter (alpha blend) ----
    if mode == "adaptive"
        [left,right,start0] = adaptive_window_center_fixed(i0, n, r);
        W = left + right + 1;
        if W < 1
            filtered = 0.0;
        else
            sigmaLocal = W  /  sigmaFactor;
            wts = compute_gaussian_coeffs(W, sigmaLocal);         % normalized
            vals = input((start0+1):(start0+W));
            filtered = sum(wts(:) .* vals(:));                    % weights sum to 1
        end
    else
        s = 0.0;
        for k = -r:r
            v = sample_with_boundary(input, i0 + k, mode);
            s = s + v * gaussFull(k + r + 1);
        end
        filtered = s;                                             % already normalized weights
    end
    res.Gauss(i) = a * filtered + (1.0 - a) * input(i);

    % ---- Savitzky-Golay (no alpha) ----
    if mode == "adaptive"
        % Adaptive SG uses fixed length windowSize and shifts to keep inside
        [left,right] = sg_adaptive_left_right(i0, n, r);
        if derivOrder == 0
            coeffs = sg_coeffs_asym(left, right, polyOrder);      % normalized sum=1
        else
            W = left + right + 1;
            effPoly = min(polyOrder, W - 1);
            if derivOrder > effPoly
                error("Edge-adaptive SG window too small for derivative (W=%d, effPoly=%d, deriv=%d).", W, effPoly, derivOrder);
            end
            coeffs = sg_coeffs_asym_deriv(left, right, effPoly, derivOrder, delta);
        end
        acc = 0.0;
        for k = -left:right
            % NOTE : always in-range by construction
            acc = acc + coeffs(k + left + 1) * input((i0 + k) + 1);
        end
        res.SG(i) = acc;
    else
        acc = 0.0;
        for k = -r:r
            acc = acc + sgFull(k + r + 1) * sample_with_boundary(input, i0 + k, mode);
        end
        res.SG(i) = acc;
    end
end

end

%% ---------------- Boundary + windows ----------------
function mode = normalize_mode(boundaryMode)
mode = lower(strtrim(string(boundaryMode)));
end

function v = sample_with_boundary(x, idx0, mode)
% GetValueWithBoundary mapping (0-based)
n = numel(x);
switch mode
    case "symmetric"
        if idx0 < 0
            idx0 = -idx0 - 1;
        elseif idx0 >= n
            idx0 = 2*n - idx0 - 1;
        end
        if idx0 < 0 || idx0 >= n
            v = 0.0;
        else
            v = x(idx0 + 1);
        end

    case "replicate"
        if idx0 < 0
            idx0 = 0;
        elseif idx0 >= n
            idx0 = n - 1;
        end
        v = x(idx0 + 1);

    case "zeropad"
        if idx0 < 0 || idx0 >= n
            v = 0.0;
        else
            v = x(idx0 + 1);
        end

    case "adaptive"
        % single-sample access uses symmetric mapping
        if idx0 < 0
            idx0 = -idx0 - 1;
        elseif idx0 >= n
            idx0 = 2*n - idx0 - 1;
        end
        if idx0 < 0 || idx0 >= n
            v = 0.0;
        else
            v = x(idx0 + 1);
        end

    otherwise
        error("Unknown boundary mode: %s", mode);
end
end

function [left,right,start0] = adaptive_window_center_fixed(center0, n, r)
% GetAdaptiveWindow(i): left = min(r, i), right=min(r, n - 1 - i), start = i-left
left  = min(r, center0);
right = min(r, (n-1) - center0);
start0 = center0 - left;
end

function [left,right] = sg_adaptive_left_right(i0, n, r)
% Adaptive SG: fixed length (2r + 1), shift window to stay inside
desiredW = 2*r + 1;

left  = min(r, i0);
right = desiredW - 1 - left;

if (i0 + right) > (n - 1)
    shift = (i0 + right) - (n - 1);
    right = right - shift;
    left  = left + shift;
end

if left < 0, left = 0; end
if right < 0, right = 0; end
end

%% ---------------- Binomial  /  Gaussian kernels ----------------
function c = calc_binomial_coeffs_long(len)
% Mirrors CalcBinomialCoefficients(len) with length <= 63 check
if len < 1
    error("length must be >= 1");
end
if len > 63
    error("length must be <= 63 to avoid 64-bit overflow.");
end

c = zeros(len,1,'int64');
c(1) = int64(1);
for i = 2:len
    % c[i] = c[i - 1] * (len - i) / i (integer math, exact division)
    c(i) = idivide(c(i-1) * int64(len - (i-1)), int64(i-1), 'floor');
end
end

function g = compute_gaussian_coeffs(len, sigma)
% Mirrors ComputeGaussianCoefficients(length, sigma) : normalized exp(-x^2 / (2 * sigma^2))
if len < 1, error("length must be >= 1"); end
if sigma <= 0, error("sigma must be > 0"); end

w = floor((len - 1) / 2);                         % int w = (length - 1) / 2
twoSigmaSq = 2 * sigma * sigma;

g = zeros(len,1);
s = 0.0;
for i = 0:(len-1)
    x = i - w;
    g(i+1) = exp(-(x*x)  /  twoSigmaSq);
    s = s + g(i+1);
end
if s <= 0, error("Gaussian kernel sum <= 0"); end
g = g  /  s;
end

%% ---------------- Weighted medians ----------------
function m = weighted_median_long(values, weightsLong)
% Binomial weighted median: sort by Value, accumulate int64 weights
% if totalWeight even and accum==half -> average(current,next)
values = values(:);
w = int64(weightsLong(:));

[vs, idx] = sort(values, 'ascend');               
ws = w(idx);

totalW = sum(ws, 'native');                       % int64
if totalW <= 0
    m = 0.0;
    return;
end

even = bitand(totalW, int64(1)) == 0;
half = idivide(totalW, int64(2), 'floor');
acc = int64(0);

m = vs(end);                                      % default
for j = 1:numel(vs)
    acc = acc + ws(j);
    if acc > half
        m = vs(j);
        return;
    end
    if even && acc == half
        if j < numel(vs)
            nextVal = vs(j+1);
        else
            nextVal = vs(j);
        end
        m = (vs(j) + nextVal)  /  2.0;
        return;
    end
end
end

function m = weighted_median_double(values, weights, useGE)
% Gaussian-weighted median uses doubles and selects first accum >= half
values  = values(:);
weights = weights(:);

[vs, idx] = sort(values, 'ascend');
ws = weights(idx);

total = sum(ws);
if total <= 0
    m = 0.0;
    return;
end

half = total  /  2.0;
acc = 0.0;
sel = numel(vs);

for j = 1:numel(vs)
    acc = acc + ws(j);
    if useGE
        if acc >= half
            sel = j;
            break;
        end
    else
        if acc > half
            sel = j;
            break;
        end
    end
end

m = vs(sel);
end

%% ---------------- SG coefficient builders ----------------
function h = sg_coeffs_symmetric(windowSize, polyOrder, derivOrder, delta)
% ComputeSavitzkyGolayCoefficients(windowSize, polyOrder, derivOrder, delta)
if windowSize <= 0, error("windowSize must be > 0"); end
if mod(windowSize,2) == 0, error("windowSize must be odd"); end
if polyOrder < 0 || polyOrder >= windowSize, error("polyOrder must be 0..windowSize-1"); end
if derivOrder < 0 || derivOrder > polyOrder, error("derivOrder must be <= polyOrder"); end
if delta <= 0, error("delta must be > 0"); end

m = polyOrder;
half = floor(windowSize / 2);

A = zeros(windowSize, m+1);
row = 1;
for xi = -half:half
    pow = 1.0;
    for j = 0:m
        A(row, j+1) = pow;
        pow = pow * xi;
    end
    row = row + 1;
end

ATA = A' * A;
invATA = inv_strict(ATA);
AT = A';

h = zeros(windowSize,1);
for k = 1:windowSize
    h(k) = invATA(derivOrder+1, :) * AT(:, k);
end

if derivOrder == 0
    hs = sum(h);
    if abs(hs) < 1e-20
        error("SG coeff sum ~ 0");
    end
    h = h  /  hs;
else
    h = h * (factorial(derivOrder)  /  (delta^derivOrder));
end
end

function h = sg_coeffs_asym(left, right, polyOrder)
% ComputeSGCoefficientsAsymmetric(left, right, polyOrder)
W = left + right + 1;
m = min(max(polyOrder,0), W-1);

xv = (-left:right).';
A = zeros(W, m+1);
for p = 0:m
    A(:, p+1) = xv.^p;
end

ATA = A' * A;
invATA = inv_strict(ATA);
AT = A';

h = zeros(W,1);
for k = 1:W
    h(k) = invATA(1,:) * AT(:,k);     % derivative 0 row at center
end

ss = sum(h);
if abs(ss) > 0
    h = h  /  ss;
end
end

function h = sg_coeffs_asym_deriv(left, right, polyOrder, derivOrder, delta)
% ComputeSGCoefficientsAsymmetricDerivative(left, right, polyOrder, derivOrder, delta)
W = left + right + 1;
m = min(max(polyOrder,0), W-1);
if derivOrder > m
    error("derivOrder must be <= effective polyOrder");
end

xv = (-left:right).';
A = zeros(W, m+1);
for p = 0:m
    A(:, p+1) = xv.^p;
end

ATA = A' * A;
invATA = inv_strict(ATA);
AT = A';

h = zeros(W,1);
for k = 1:W
    h(k) = invATA(derivOrder+1,:) * AT(:,k);
end

if derivOrder == 0
    ss = sum(h);
    if abs(ss) < 1e-20
        error("Asymmetric smoothing coeff sum ~ 0");
    end
    h = h  /  ss;
else
    h = h * (factorial(derivOrder)  /  (delta^derivOrder));
end
end

function invA = inv_strict(A)
% Mirrors InvertMatrixStrict behavior (simplified guard)
if size(A,1) ~= size(A,2)
    error("Matrix must be square");
end
if rcond(A) < 1e-14
    error("Matrix is singular or ill-conditioned for inversion.");
end
invA = inv(A);
end

function validate_params(n, r, polyOrder, derivOrder, delta, sigmaFactor)
windowSize = 2*r + 1;

if r < 0 || fix(r) ~= r
    error("r must be non-negative integer.");
end
if windowSize > n
    error("windowSize (=2*r+1) must be <= dataCount.");
end

if polyOrder < 0 || fix(polyOrder) ~= polyOrder
    error("polyOrder must be integer >= 0.");
end
if polyOrder >= windowSize
    error("polyOrder must be < windowSize.");
end

if derivOrder < 0 || fix(derivOrder) ~= derivOrder
    error("derivOrder must be integer >= 0.");
end
if derivOrder > polyOrder
    error("derivOrder must be <= polyOrder.");
end

if delta <= 0
    error("delta must be > 0.");
end

if sigmaFactor < 1.0 || sigmaFactor > 12.0 || isnan(sigmaFactor) || isinf(sigmaFactor)
    error("sigmaFactor out of range (1..12)");
end
end
