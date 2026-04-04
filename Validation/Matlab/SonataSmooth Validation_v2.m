%% SonataSmooth (C# ApplySmoothing exact parity) MATLAB Reference
% Matches FrmMain.ApplySmoothing exactly for all smoothing filters,,
% boundary modes, alpha blend, and sigmaFactor behavior.
%
% IMPORTANT
% - This reference now supports BOTH of the C# entry points below :
%   1. FrmMain.ApplySmoothing when derivOrder == 0
%   2. FrmMain.ApplySGDerivative when derivOrder > 0
% - For derivOrder > 0, only Savitzky-Golay derivative output is produced, matching C#.
%
% Filters        : RectAvg, BinomAvg, BinomWMedian, GaussWMedian, Gauss, Savitzky-Golay
% Boundary modes : Symmetric, Replicate, Adaptive, ZeroPad
%
% KEY MATCHING DETAILS (C# FrmMain.cs - ApplySmoothing parity) :
% - Adaptive (Rect / BinomAvg / BinomWMedian / GaussWMedian / Gauss) : symmetric shrinking (zero-phase) :
%       symR = min(r, min(i, n - 1 - i)), start = i - symR, W = 2 * symR + 1
% - Adaptive (SG) : fixed length (2r + 1) when possible, shifted to stay inside [0 ... n - 1]
% - Adaptive Binomial : SLICED from full-size kernel binom[(pos - symR) + r], then renormalized
%   (not recomputed for local W -- matches C# ApplySmoothing exactly)
% - Adaptive Gaussian : uses ORIGINAL sigma = windowSize / sigmaFactor centered on i,
%   truncated at boundaries and renormalized (avoids phase distortion from local sigma)
% - Binomial coefficients : double precision (C# CalcBinomialCoefficients : c[i] = c[i - 1] * (len - i) / i)
% - Binomial weighted median : current C# uses exact even-total / accum == half midpoint handling
% - Gaussian weighted median : uses ORIGINAL sigma centered on i, then selects first value where accum >= half (NO tie-average), matching current C# exactly
% - SG : Householder QR decomposition (C# HouseholderQR + ExtractPseudoInverseRow)

clear; clc;

%% -------------------- USER INPUT & SAMPLE DATA --------------------
% Sample Data
x = [5,5,5,6,5,5,6,5,5,5,5,5,5,5,4,6,6,5,4,5,4,5,5,4,4,5,5,5,5,5,5,5,5,5,7,5,5,6,5,5,5,5,5,6,5,5,4,5,6,4,5,5,5,5,6,6,6,4,5,5,5,5,5,5,6,4,5,7,6,5,5,6,4,5,5,5,5,4,4,5,4,4,5,4,4,4,4,5,4,5,5,4,5,5,5,4,4,4,5,4,6,5,5,5,4,4,4,4,4,4,4,4,4,4,4,4,4,5,5,4,4,4,4,4,5,4,4,5,4,4,4,4,4,4,3,4,4,4,4,4,4,4,3,3,4,5,3,3,4,4,4,4,4,3,3,3,3,3,3,4,4,4,4,3,3,4,4,4,4,4,4,3,4,4,5,4,4,4,4,4,4,5,3,3,3,4,4,3,4,5,5,5,5,4,4,5,5,4,4,5,5,4,4,4,4,4,4,4,5,4,4,5,4,4,4,4,5,4,4,5,5,5,5,4,5,5,4,4,4,5,4,4,4,4,5,4,5,5,5,6,6,6,5,5,5,5,5,6,6,6,6,6,6,7,6,7,6,6,6,5,6,6,7,6,7,7,6,6,6,6,6,6,8,5,7,6,7,6,6,6,6,6,6,7,7,6,6,6,6,6,6,6,7,6,5,5,6,7,6,5,5,6,6,6,6,6,5,5,6,5,6,6,6,5,6,6,6,5,6,7,6,6,6,7,7,7,6,5,6,6,6,6,6,5,6,6,5,5,5,5,6,5,6,6,10,6,5,6,6,5,5,5,5,6,6,5,5,5,6,5,6,6,7,7,7,6,7,6,7,7,7,8,7,8,6,6,7,7,7,7,8,7,7,7,7,6,7,6,6,6,6,6,7,7,6,6,6,7,7,7,7,7,6,7,7,6,6,7,6,6,6,6,5,6,7,6,6,6,7,6,5,6,6,6,6,7,6,6,6,6,6,6,6,6,7,6,6,6,7,6,5,6,5,5,5,6,6,6,5,7,5,6,8,6,5,5,6,6,5,1,8,7,8,7,9,8,8,8,6,8,6,8,6,6,6,7,5,6,7,9,6,6,5,7,6,6,5,6,5,6,6,6,6,6,7,7,6,6,6,6,5,6,8,8,9,7,7,9,7,8,7,9,11,15,10,8,8,9,8,8,6,7,7,7,7,6,8,6,7,6,8,6,9,9,7,7,7,5,8,8,7,7,6,8,10,7,6,6,10,8,6,8,6,7,8,7,7,8,8,7,7,7,6,8,8,8,6,5,7,7,7,8,8,6,7,8,6,8,8,8,6,6,6,6,8,6,7,5,6,6,5,6,7,5,5,5,5,7,5,7,6,5,5,6,8,8,7,7,7,5,8,7,7,7,7,7,7,6,7,6,14,7,6,7,8,5,5,6,5,6,6,5,6,6,6,6,6,6,6,8,7,7,7,6,7,6,8,7,7,7,7,5,7,5,7,6,7,7,7,8,9,7,7,7,6,7,8,6,8,7,7,7,6,6,5,8,7,9,8,8,8,6,9,7,8,7,8,7,8,8,10,7,8,7,9,7,8,8,8,6,9,9,7,7,7,8,7,7,8,7,6,7,9,8,8,8,9,7,8,6,7,7,7,8,10,7,6,8,8,6,11,8,8,7,8,7,6,6,6,7,6,7,9,7,8,8,9,6,8,7,8,8,7,6,7,6,7,8,6,8,9,8,7,6,7,5,6,5,6,6,6,6,6,6,6,7,6,7,7,6,6,7,7,9,7,7,6,7,7,8,7,7,6,8,7,8,9,7,7,9,7,8,6,8,6,7,8,7,8,8,7,6,8,8,8,9,6,12,8,8,6,7,7,6,7,8,9,8,8,7,6,7,7,7,6,7,7,6,5,7,6,6,6,8,7,6,8,6,8,9,7,8,6,7,8,7,6,7,7,8,6,6,6,6,8,6,7,8,7,8,7,7,7,6,5,6,7,7,5,6,6,6,6,7,7,6,7,6,7,6,7,7,6,5,7,7,6,6,7,7,7,7,7,7,9,5,6,6,6,6,4,4,6,6,4,5,6,7,5,5,6,6,6,5,6,5,5,6,5,6,5,7,6,5,6,5,5,7,6,6,7,5,8,6,5,6,6,5,6,5,5,6,8,5,6,6,5,7,5,6,6,5,6,7,6,6,5,5,7,6,5,4,5,6,6,4,5,4,5,4,5,4,5,6,6,7,6,5,6,5,6,6,5,6,6,5,5,6,7,6,6,7,5,6,6,5,6,7,5,7,6,5,7,5,6,6,6,5,4,5,5,5,6,5,5,6,5,4,6,5,6,5,4,5,6,4,6,5,6,5,4,5,5,5,4,5,5,5,5,5,5,4,5,5,4,5,5,5,5,5,5,5,4,4,5,5,4,5,4,5,4,5,5,4,4,5,5,5,4,5,6,5,6,5,6,4,6,5,5,6,6,5,5,7,7,7,7,8,5,7,7,7,7,7,7,10,8,8,7,8,7,7,7,8,10,8,9,9,11,8,9,7,10,9,8,8,8,8,9,10,8,8,7,7,9,10,6,7,6,8,7,6,5,6,7,7,7,7,8,8,6,6,6,6,5,6,5,6,6,8,5,8,6,5,5,6,5,6,5,5,6,6,6,7,6,5,5,7,5,6,24,14,10,6,8,7,7,5,6,6,6,6,7,6,6,7,6,7,6,7,6,6,6,6,6,7,5,7,8,6,6,5,6,6,5,6,6,6,7,6,6,6,6,6,7,6,5,6,5,5,5,6,5,5,5,6,5,5,6,8,6,7,7,6,6,6,5,6,6,5,6,6,5,6,6,6,6,5,6,6,6,6,6,6,7,6,6,7,6,6,7,7,7,8,7,7,7,6,7,8,7,7,8,7,7,8,7,7,7,7,7,6,7,7,7,7,6,6,6,7,7,7,6,7,7,6,6,7,6,10,8,8,8,8,7,7,7,8,7,7,8,7,7,8,7,7,8,8,7,7,8,7,7,7,10,11,9,8,8,7,7,8,9,10,9,10,9,9,9,9,9,9,9,9,10,9,9,9,9,10,10,9,10,9,9,10,9,10,10,9,9,9,9,9,9,9,11,9,8,14,10,8,8,8,10,9,8,10,9,8,8,9,9,8,8,10,9,8,9,7,8,9,8,8,8,8,8,8,8,7,8,8,9,9,8,8,8,8,9,10,8,8,8,11,9,9,8,8,8,8,8,8,8,8,8,8,7,7,7,8,8,8,7,7,7,7,8,7,7,8,7,7,8,7,7,7,7,7,7,8,8,8,8,8,8,8,9,9,8,9,8,8,9,9,8,10,9,9,8,9,9,10,9,8,9,9,8,8,7,8,8,7,8,8,7,8,8,7,7,8,7,7,7,8,7,9,7,7,7,8,7,8,9,9,10,10,10,10,9,10,10,9,10,11,10,10,11,10,9,9,10,9,11,10,10,10,9,10,10,9,9,10,10,9,9,9,10,10,10,11,10,10,9,9,9,9,9,9,8,10,8,7,8,8,7,7,8,9,8,8,9,9,9,9,7,8,8,9,8,9,8,9,8,8,8,8,8,8,8,8,9,8,12,9,9,8,8,8,8,9,8,9,8,8,9,9,9,9,8,8,8,8,8,8,8,8,8,9,9,8,9,9,8,8,8,8,8,8,7,7,7,7,7,6,8,7,7,7,7,8,7,8,7,6,7,7,7,6,6,5,6,7,7,7,7,7,6,7,7,6,6,10,7,8,8,8,7,8,7,7,9,8,8,7,7,7,7,7,8,7,7,7,8,8,7,9,8,7,7,7,9,8,8,8,8,8,8,8,7,8,8,8,9,9,9,10,9,9,9,9,9,8,8,8,9,8,8,9,8,8,9,9,9,8,8,9,9,8,8,8,8,8,8,8,10,9,8,8,8,9,9,9,8,8,9,9,9,9,8,10,9,11,11,11,10,9,10,9,9,9,10,11,9,9,8,9,10,10,10,9,9,9,10,9,12,11,10,11,10,11,13,11,10,11,12,10,11,11,12,14,11,11,10,11,12,11,12,13,13,11,13,12,14,13,14,12,12,13,14,13,12,11,12,12,12,12,12,13,14,13,13,13,14,16,14,14,13,14,13,14,12,14,13,12,13,12,12,14,13,13,13,15,14,15,15,14,15,16,15,16,13,14,14,16,16,16,16,14,14,15,15,16,16,15,15,15,14,14,15,14,13,13,14,13,12,12,13,13,13,13,12,13,15,15,15,13,14,14,15,13,14,13,14,14,14,15,15,13,15,16,14,15,14,15,15,15,13,15,16,15,14,14,14,18,14,14,13,13,13,13,14,13,14,14,13,14,14,14,14,14,14,4,16,18,18,17,15,16,16,15,16,16,16,16,16,16,17,16,17,17,16,16,17,16,16,16,15,15,16,17,18,16,18,16,16,15,17,16,15,16,16,17,17,16,16,16,16,17,16,16,17,17,18,18,18,19,17,17,17,16,17,19,18,18,19,18,18,18,18,18,19,20,19,19,19,20,20,25,28,21,20,21,20,21,19,21,18,18,16,18,19,20,20,20,20,20,19,20,17,19,18,17,17,18,18,20,18,19,20,20,20,19,18,19,19,18,19,19,18,19,19,21,20,20,20,18,20,19,19,18,18,18,18,17,18,18,19,19,17,17,18,17,18,17,18,17,16,16,17,16,17,16,16,16,16,15,16,16,17,15,17,16,16,15,15,15,16,15,15,15,16,15,15,15,15,16,15,15,15,14,15,14,16,16,16,16,16,15,15,16,16,16,17,16,16,16,17,15,16,16,16,17,17,16,16,16,15,16,15,16,15,15,16,14,15,15,16,15,16,16,15,14,15,15,16,16,15,16,15,16,15,14,15,15,16,15,15,17,15,16,16,18,17,16,17,19,18,18,18,18,16,17,16,18,18,18,20,18,20,20,23,21,22,23,20,22,20,21,22,20,23,25,25,24,22,21,22,22,23,22,19,22,21,22,21,21,22,22,23,21,22,22,20,21,22,22,21,19,18,19,18,19,16,18,18,17,16,16,15,15,14,13,13,12,11,11,11,12,14,12,12,12,12,12,12,13,12,12,11,12,11,11,11,10,11,10,11,11,10,10,11,10,12,11,13,14,14,11,11,10,11,12,10,11,12,12,12,12,12,11,12,12,11,11,12,12,10,11,11,11,11,12,12,11,11,11,11,12,12,12,11,12,11,12,12,13,13,12,12,12,12,12,11,11,11,11,11,11,11,11,11,12,12,11,11,11,11,11,11,11,11,12,10,10,10,11,10,11,10,11,11,10,10,10,11,11,12,15,12,11,10,11,10,10,10,11,10,10,11,10,11,11,11,13,12,10,11,10,11,11,10,10,11,10,10,11,10,9,9,10,9,9,10,11,11,9,10,11,11,10,10,10,11,11,10,10,11,11,11,10,11,10,10,10,11,11,11,11,12,11,10,11,11,11,11,11,11,11,11,11,11,10,10,10,9,12,10,10,10,12,10,11,11,11,11,11,12,12,11,11,11,11,10,11,11,11,10,11,11,11,11,12,12,12,12,11,11,12,12,11,11,12,11,11,11,12,11,11,10,11,11,11,11,11,12,10,11,10,11,11,11,10,11,10,11,11,11,11,11,10,10,10,11,12,13,12,12,11,11,10,12,12,10,11,10,11,11,11,11,11,10,11,12,11,11,11,12,11,11,12,11,11,12,11,11,12,11,11,10,11,10,11,12,12,12,14,11,11,12,11,10,11,12,14,11,11,12,10,11,11,11,11,12,11,11,11,11,11,10,11,11,10,11,11,11,13,11,13,10,11,11,12,12,11,11,11,12,12,12,10,11,12,12,11,11,11,12,11,12,11,11,12,11,11,13,10,10,11,11,10,11,12,11,9,12,10,11,10,11,11,11,10,11,11,10,11,11,11,11,11,11,11,11,10,9,11,10,10,13,11,11,11,11,11,10,10,10,10,10,11,11,10,10,10,10,9,11,10,10,9,10,11,13,10,10,9,10,10,11,12,10,10,10,10,10,10,11,10,10,10,10,11,11,10,10,10,11,10,11,9,9,11,9,10,11,10,11,10,11,11,9,10,10,9,9,9,10,13,10,11,10,9,12,10,11,11,11,10,12,10,10,11,10,12,12,10,11,11,12,12,10,11,11,11,11,10,11,11,11,11,11,10,11,10,10,10,10,12,10,12,11,12,9,12,11,12,10,10,11,11,11,10,11,10,12,12,13,11,10,10,10,10,11,10,11,11,12,11,12,11,10,10,11,12,11,12,12,10,11,11,12,10,12,13,11,10,11,10,9,10,11,11,10,9,10,11,9,11,10,11,10,11,10,12,11,10,10,11,10,12,11,11,11,10,11,11,13,10,10,10,11,12,11,11,11,12,14,11,11,12,11,10,11,11,10,11,10,12,11,12,15,12,11,12,10,11,11,11,11,37,19,13,10,11,13,11,11,13,12,12,11,11,11,12,11,12,12,12,10,12,10,10,11,11,12,12,10,11,10,11,12,11,11,11,9,11,11,10,11,11,13,11,11,11,14,11,14,12,12,12,12,11,10,11,11,12,10,11,12,11,13,11,10,11,11,12,11,12,11,13,12,13,12,12,11,12,12,12,11,11,12,11,14,13,14,13,13,11,12,13,14,13,11,13,11,12,15,13,11,13,13,13,14,12,14,13,13,15,15,14,13,13,14,13,13,14,14,13,14,14,14,16,15,17,14,13,13,13,14,14,13,15,15,14,16,13,16,13,13,14,14,13,15,15,14,13,14,12,14,14,16,16,15,16,15,16,15,16,15,15,15,17,16,16,16,17,18,18,19,17,15,16,17,15,16,16,16,17,16,19,17,16,17,18,18,16,19,17,18,16,20,39,19,16,17,17,16,18,16,19,17,19,15,22,18,21,18,20,20,17,16,16,17,16,18,17,17,16,17,16,15,18,15,16,16,16,15,16,15,15,16,15,17,16,17,16,15,16,17,16,16,16,18,14,15,15,16,16,17,13,14,14,16,14,13,14,14,13,15,14,15,13,13,13,13,14,13,13,13,14,14,13,13,14,14,15,13,13,15,13,16,15,12,13,13,14,13,12,11,13,13,12,12,13,13,16,12,12,12,12,13,12,12,12,13,14,12,13,13,14,14,13,14,13,13,14,14,13,13,14,13,16,13,14,14,13,15,14,14,14,13,13,14,14,15,13,13,13,14,14,14,15,14,14,13,13,16,14,13,14,13,12,12,12,12,14,13,12,14,14,15,14,16,14,15,13,15,14,13,16,12,13,13,14,15,15,15,14,13,13,15,14,15,14,13,13,13,14,14,12,13,13,13,13,12,14,14,13,15,15,14,15,14,13,14,16,14,14,15,16,17,16,15,16,15,14,15,15,16,16,16,15,16,16,16,16,18,18,16,18,18,16,18,17,17,16,18,17,16,17,17,16,17,16,16,18,17,17,18,18,19,18,18,18,17,18,19,16,19,19,18,17,18,18,18,17,19,19,20,18,20,18,20,18,17,19,17,18,19,18,18,19,20,34,20,22,15,16,18,19,19,18,17,18,19,19,19,19,19,17,17,17,19,17,17,17,17,18,17,17,17,18,17,17,16,16,17,15,16,17,16,16,16,17,14,16,16,16,16,16,16,16,16,17,17,17,15,17,16,16,3,2,12,13,13,13,12,11,12,11,11,11,11,11,11,11,13,12,12,11,12,13,12,13,11,10,12,10,11,10,10,12,10,10,11,13,11,12,11,13,10,11,11,13,9,12,12,13,11,12,11,11,11,11,10,12,11,13,11,13,11,11,10,12,12,11,10,11,10,11,11,12,13,10,12,10,13,12,11,10,10,11,10,11,10,11,12,12,10,12,10,9,11,11,11,9,10,9,12,11,12,13,11,11,9,11,9,10,10,11,10,11,10,9,11,11,11,11,10,10,11,10,9,11,10,12,10,13,11,12,11,11,12,10,11,13,11,12,9,10,11,11,10,10,10,9,10,12,9,10,11,11,11,12,11,12,11,11,14,12,10,13,11,10,9]';  % <- your data (column)

% Kernel Radius
r = 30;

% Parameters
boundaryMode = "Symmetric";  % "Symmetric" | "Replicate" | "Adaptive" | "ZeroPad"

alpha = 0.50;              % used for BinomAvg / BinomWMedian / GaussWMedian / Gauss only

sigmaFactor = 3.0;         % ApplySmoothing default when sigmaFactor is omitted

polyOrder  = 3;
derivOrder = 2;            % ApplySmoothing parity only (SG smoothing only)

delta      = 1.0;          % kept for signature compatibility; ignored when derivOrder == 0


%% -------------------- RUN --------------------
% Always compute the 6 smoothing-family outputs.
% SG column is :
%   - smoothing result when derivOrder == 0
%   - derivative result when derivOrder > 0

% 1. Always compute smoothing outputs with derivOrder forced to 0
resSmooth = sonata_apply_smoothing_csharp(x, r, polyOrder, 0, delta, boundaryMode, alpha, sigmaFactor);

% 2. Decide what goes into the SG column
if derivOrder == 0
    sgOut = resSmooth.SG;   % normal SG smoothing
else
    sgOut = apply_sg_derivative_csharp(x, r, polyOrder, derivOrder, delta, boundaryMode);  % SG derivative
end

% 3. Always build 7-result table
T = table((1:numel(x))', x, ...
    resSmooth.Rect, ...
    resSmooth.BinomAvg, ...
    resSmooth.BinomWMedian, ...
    resSmooth.GaussWMedian, ...
    resSmooth.Gauss, ...
    sgOut, ...
    'VariableNames', {'Index','Initial','RectAvg','BinomAvg','BinomWMedian','GaussWMedian','Gauss','SG'});

disp(T);

%% ============================================================
%% ===================== LOCAL FUNCTIONS ======================
%% ============================================================

function res = sonata_apply_smoothing_csharp(input, r, polyOrder, derivOrder, delta, boundaryMode, alpha, sigmaFactor)
input = input(:);
n = numel(input);

validate_params(n, r, polyOrder, derivOrder, delta, sigmaFactor);
mode = normalize_mode(boundaryMode);

windowSize = 2 * r + 1;

% NaN / Infinity defense (matches C# safeInput)
safeInput = input;
for idx = 1:n
    if isnan(safeInput(idx)) || isinf(safeInput(idx))
        safeInput(idx) = 0.0;
    end
end

% alpha validation (matches refactored C# contract)
if isnan(alpha) || isinf(alpha) || alpha < 0.0 || alpha > 1.0
    error('alpha must be finite and within [0, 1].');
end
a = alpha;

% Precompute full-size binomial coefficients (double, matching C# CalcBinomialCoefficients)
binomFull = calc_binomial_coeffs_double(windowSize);
binomSum  = sum(binomFull);

% Gaussian : sigma = windowSize / sigmaFactor (matches C# exactly)
sigma = windowSize / sigmaFactor;
gaussFull = compute_gaussian_coeffs(windowSize, sigma);

% Outputs
res.Rect           = zeros(n,1);
res.BinomAvg       = zeros(n,1);
res.BinomWMedian   = zeros(n,1);
res.GaussWMedian   = zeros(n,1);
res.Gauss          = zeros(n,1);
res.SG             = zeros(n,1);

% For non-adaptive SG : compute symmetric SG coefficients once (Householder QR)
sgFull = [];
if mode ~= "adaptive"
    sgFull = sg_coeffs_symmetric_qr(windowSize, polyOrder, derivOrder, delta);
end

for i0 = 0:(n - 1)  % i0 : 0-based index
    i = i0 + 1;     % MATLAB 1-based index

    % ---- RectAvg (no alpha) ----
    if mode == "adaptive"
        [symR, start0] = adaptive_window_center_fixed(i0, n, r);
        W = 2 * symR + 1;
        if W > 0
            seg = safeInput((start0 + 1):(start0 + W));
            res.Rect(i) = sum(seg) / W;
        else
            res.Rect(i) = 0.0;
        end
    else
        s = 0.0;
        for k = -r:r
            s = s + sample_with_boundary(safeInput, i0 + k, mode);
        end
        res.Rect(i) = s / windowSize;
    end

    % ---- Binomial Avg (alpha blend) ----
    % Adaptive : SLICE from full kernel binom[(pos - symR) + r], then renormalize
    if mode == "adaptive"
        [symR, start0] = adaptive_window_center_fixed(i0, n, r);
        W = 2 * symR + 1;
        if W < 1
            filtered = 0.0;
        else
            localSum = 0.0;
            s = 0.0;
            for pos = 0:(W - 1)
                w = binomFull((pos - symR) + r + 1);   % slice from full kernel (1-based)
                localSum = localSum + w;
                s = s + safeInput(start0 + pos + 1) * w;
            end
            if localSum > 0
                filtered = s / localSum;
            else
                filtered = 0.0;
            end
        end
    else
        s = 0.0;
        for k = -r:r
            v = sample_with_boundary(safeInput, i0 + k, mode);
            s = s + v * binomFull(k + r + 1);
        end
        if binomSum > 0
            filtered = s / binomSum;
        else
            filtered = 0.0;
        end
    end
    res.BinomAvg(i) = a * filtered + (1.0 - a) * safeInput(i);

    % ---- Binomial Median (alpha blend; tie = average) ----
    % Adaptive : SLICE from full kernel for weights
    if mode == "adaptive"
        [symR, start0] = adaptive_window_center_fixed(i0, n, r);
        W = 2 * symR + 1;
        if W < 1
            filtered = 0.0;
        else
            localWeights = zeros(W, 1);
            for pos = 0:(W - 1)
                localWeights(pos+1) = binomFull((pos - symR) + r + 1);
            end
            vals = safeInput((start0 + 1):(start0 + W));
            filtered = weighted_median_binom(vals, localWeights);
        end
    else
        W = windowSize;
        vals = zeros(W, 1);
        wts  = binomFull(:);
        for k = -r:r
            vals(k + r + 1) = sample_with_boundary(safeInput, i0 + k, mode);
        end
        filtered = weighted_median_binom(vals, wts);
    end
    res.BinomWMedian(i) = a * filtered + (1.0 - a) * safeInput(i);

    % ---- Gaussian Weighted Median (alpha blend; matches current C# exactly) ----
    % Adaptive : use ORIGINAL sigma centered on i, truncate to the in-range window,
    % renormalize, then sort(values, weights) and pick the first value where
    % accum >= half (NO tie-average).
    if mode == "adaptive"
        [symR, start0] = adaptive_window_center_fixed(i0, n, r);
        W = 2 * symR + 1;
        if W <= 0
            filtered = 0.0;
        else
            twoSigSq = 2.0 * sigma * sigma;
            vals = zeros(W, 1);
            wts  = zeros(W, 1);
            wSum = 0.0;
            for pos = 0:(W - 1)
                offset = pos - symR;
                vals(pos + 1) = safeInput(start0 + pos + 1);
                wts(pos + 1)  = exp(-(offset * offset) / twoSigSq);
                wSum = wSum + wts(pos + 1);
            end
            if wSum > 0.0
                wts = wts / wSum;
            end

            [vs, sortIdx] = sort(vals, 'ascend');
            ws = wts(sortIdx);

            total = sum(ws);
            if total > 0.0
                half = total / 2.0;
                accum = 0.0;
                sel = W;
                for j = 1:W
                    accum = accum + ws(j);
                    if accum >= half
                        sel = j;
                        break;
                    end
                end
                filtered = vs(sel);
            else
                filtered = 0.0;
            end
        end
    else
        W = windowSize;
        vals = zeros(W, 1);
        wts  = gaussFull(:);
        for k = -r:r
            vals(k + r + 1) = sample_with_boundary(safeInput, i0 + k, mode);
        end

        [vs, sortIdx] = sort(vals, 'ascend');
        ws = wts(sortIdx);

        total = sum(ws);
        if total > 0.0
            half = total / 2.0;
            accum = 0.0;
            sel = W;
            for j = 1:W
                accum = accum + ws(j);
                if accum >= half
                    sel = j;
                    break;
                end
            end
            filtered = vs(sel);
        else
            filtered = sample_with_boundary(safeInput, i0, mode);
        end
    end
    res.GaussWMedian(i) = a * filtered + (1.0 - a) * safeInput(i);

    % ---- Gaussian filter (alpha blend) ----
    % Adaptive : uses ORIGINAL sigma centered on i (symmetric shrinking, zero-phase)
    if mode == "adaptive"
        [symR, start0] = adaptive_window_center_fixed(i0, n, r);
        W = 2 * symR + 1;
        if W < 1
            filtered = 0.0;
        else
            twoSigSq = 2.0 * sigma * sigma;
            localGauss = zeros(W, 1);
            wSum = 0.0;
            for pos = 0:(W - 1)
                offset = pos - symR;                        % signed distance from center i
                localGauss(pos + 1) = exp(-(offset * offset) / twoSigSq);
                wSum = wSum + localGauss(pos + 1);
            end
            if wSum > 0.0
                localGauss = localGauss / wSum;
            end
            s = 0.0;
            for pos = 0:(W - 1)
                s = s + localGauss(pos + 1) * safeInput(start0 + pos + 1);
            end
            filtered = s;
        end
    else
        s = 0.0;
        for k = -r:r
            v = sample_with_boundary(safeInput, i0 + k, mode);
            s = s + v * gaussFull(k + r + 1);
        end
        filtered = s;
    end
    res.Gauss(i) = a * filtered + (1.0 - a) * safeInput(i);

    % ---- Savitzky-Golay (no alpha) ----
    if mode == "adaptive"
        [left, right] = get_adaptive_sides(i0, n, r);
        if derivOrder == 0
            coeffs = sg_coeffs_asym_qr(left, right, polyOrder);
        else
            W = left + right + 1;
            effPoly = min(polyOrder, W - 1);
            if derivOrder > effPoly
                error('Edge-adaptive SG window too small for derivative (W=%d, effPoly=%d, deriv=%d).', W, effPoly, derivOrder);
            end
            coeffs = sg_coeffs_asym_deriv_qr(left, right, effPoly, derivOrder, delta);
        end
        acc = 0.0;
        for k = -left:right
            acc = acc + coeffs(k + left + 1) * safeInput((i0 + k) + 1);
        end
        res.SG(i) = acc;
    else
        acc = 0.0;
        for k = -r:r
            acc = acc + sgFull(k + r + 1) * sample_with_boundary(safeInput, i0 + k, mode);
        end
        res.SG(i) = acc;
    end
end

end

function output = apply_sg_derivative_csharp(input, r, polyOrder, derivativeOrder, delta, boundaryMode)
% Matches FrmMain.ApplySGDerivative exactly
input = input(:);
n = numel(input);
validate_sg_derivative_params(n, r, polyOrder, derivativeOrder, delta);
mode = normalize_mode(boundaryMode);

% NaN / Infinity defense (matches C# safeInput)
safeInput = input;
for idx = 1:n
    if isnan(safeInput(idx)) || isinf(safeInput(idx))
        safeInput(idx) = 0.0;
    end
end

output = zeros(n, 1);
windowSize = 2*r + 1;

if mode ~= "adaptive"
    coeffs = sg_coeffs_symmetric_qr(windowSize, polyOrder, derivativeOrder, delta);
    for i0 = 0:(n - 1)
        acc = 0.0;
        for k = -r:r
            acc = acc + coeffs(k + r + 1) * sample_with_boundary(safeInput, i0 + k, mode);
        end
        output(i0 + 1) = acc;
    end
    return;
end

% Adaptive : asymmetric coefficients, maximize window, keep indices in range
for i0 = 0:(n - 1)
    [left, right] = get_adaptive_sides(i0, n, r);
    W = left + right + 1;
    effPoly = min(polyOrder, W - 1);
    if derivativeOrder > effPoly
        error('Edge-adaptive SG window too small for derivative (W=%d, effPoly=%d, derivative=%d).', W, effPoly, derivativeOrder);
    end
    coeffs = sg_coeffs_asym_deriv_qr(left, right, effPoly, derivativeOrder, delta);
    acc = 0.0;
    for k = -left:right
        acc = acc + coeffs(k + left + 1) * safeInput((i0 + k) + 1);
    end
    output(i0 + 1) = acc;
end
end

%% -------------------- Boundary + windows --------------------
function mode = normalize_mode(boundaryMode)
mode = lower(strtrim(string(boundaryMode)));
end

function v = sample_with_boundary(x, idx0, mode)
% GetValueWithBoundary mapping (0-based index)
n = numel(x);
switch mode
    case "symmetric"
        if idx0 < 0
            idx0 = -idx0 - 1;
        elseif idx0 >= n
            idx0 = 2 * n - idx0 - 1;
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
        % NOTE : main Adaptive smoothing paths do not call this helper for Rect / Binom / Gauss families.
        % Those filters use only in-range samples via adaptive_window_center_fixed(...).
        % This fallback remains symmetric for legacy / internal parity with C#.
        if idx0 < 0
            idx0 = -idx0 - 1;
        elseif idx0 >= n
            idx0 = 2 * n - idx0 - 1;
        end
        if idx0 < 0 || idx0 >= n
            v = 0.0;
        else
            v = x(idx0 + 1);
        end

    otherwise
        error('Unknown boundary mode : %s', mode);
end
end

function [symR, start0] = adaptive_window_center_fixed(center0, n, r)
% Used by Rect / BinomAvg / BinomWMedian / GaussWMedian / Gauss (symmetric shrinking, zero-phase)
% symR = min(r, min(center0, (n-1)-center0)) ensures left == right for zero-phase filtering
symR   = min(r, min(center0, (n - 1) - center0));
start0 = center0 - symR;
end

function [left, right] = get_adaptive_sides(i0, n, r)
% Matches C# GetAdaptiveSides exactly : fixed length (2r + 1), shift to stay inside [0 ... n - 1]
desiredW = 2*r + 1;

if n <= 0
    left = 0; right = 0;
    return;
end

if n < desiredW
    left  = min(i0, n - 1);
    right = max(0, (n - 1) - i0);
    return;
end

left  = min(r, i0);
right = desiredW - 1 - left;

maxRight = (n - 1) - i0;
if right > maxRight
    extra = right - maxRight;
    right = maxRight;
    left = min(i0, left + extra);
end

if left > i0, left = i0; end
if right > (n - 1 - i0), right = (n - 1 - i0); end

if left < 0, left = 0; end
if right < 0, right = 0; end
end

%% -------------------- Binomial / Gaussian kernels --------------------
function c = calc_binomial_coeffs_double(len)
% Matches C# CalcBinomialCoefficients exactly (double precision)
% c[i] = c[i - 1] * ((double)(length - i) / i) (0-based in C#)
if len < 1
    error('length must be >= 1');
end

c = zeros(len, 1);
c(1) = 1.0;
for i = 2:len
    c(i) = c(i - 1) * ((len - (i - 1)) / (i - 1));
    if isinf(c(i)) || isnan(c(i))
        error('Binomial coefficient overflow for length = %d at index %d.', len, i);
    end
end
end

function g = compute_gaussian_coeffs(len, sigma_val)
% Matches C# ComputeGaussianCoefficients : normalized exp(-x^2 / (2 * sigma^2))
if len < 1, error('length must be >= 1'); end
if sigma_val <= 0, error('sigma must be > 0'); end

w = floor((len - 1) / 2);
twoSigmaSq = 2 * sigma_val * sigma_val;

g = zeros(len, 1);
s = 0.0;
for ii = 0:(len - 1)
    xv = ii - w;
    g(ii + 1) = exp(-(xv * xv) / twoSigmaSq);
    s = s + g(ii+1);
end
if s <= 0, error('Gaussian kernel sum <= 0'); end
g = g / s;
end

%% -------------------- Weighted medians --------------------
function m = weighted_median_binom(values, weights)
% Binomial weighted median with double weights.
% Matches current C# WeightedMedianDoubleWeights / WeightedMedianAt:
%   sort ascending, accumulate weights
%   tie-break : if mod(total, 2) == 0 & accum == half -> average(current, next)
values = values(:);
weights = weights(:);

[vs, sortIdx] = sort(values, 'ascend');
ws = weights(sortIdx);

total = sum(ws);
if total <= 0.0
    m = 0.0;
    return;
end

half = total / 2.0;
even = mod(total, 2.0) == 0.0;
accum = 0.0;
m = vs(end);

for j = 1:numel(vs)
    accum = accum + ws(j);
    if accum > half
        m = vs(j);
        return;
    end
    if even && accum == half
        if j < numel(vs)
            nextVal = vs(j + 1);
        else
            nextVal = vs(j);
        end
        m = (vs(j) + nextVal) / 2.0;
        return;
    end
end
end


%% -------------------- SG coefficient builders (Householder QR) --------------------
function h = sg_coeffs_symmetric_qr(windowSize, polyOrder, derivOrder, delta)
% Matches C# ComputeSGCoefficients : symmetric SG via Householder QR
if windowSize <= 0, error('windowSize must be > 0'); end
if mod(windowSize, 2) == 0, error('windowSize must be odd'); end
if polyOrder < 0 || polyOrder >= windowSize, error('polyOrder must be 0 ... windowSize - 1'); end
if derivOrder < 0 || derivOrder > polyOrder, error('derivOrder must be <= polyOrder'); end
if delta <= 0, error('delta must be > 0'); end

m = polyOrder;
half_w = floor(windowSize / 2);

A = zeros(windowSize, m + 1);
for ii = -half_w:half_w
    xv = ii;
    pow_val = 1.0;
    for j = 0:m
        A(ii + half_w + 1, j + 1) = pow_val;
        pow_val = pow_val * xv;
    end
end

[Q, R] = householder_qr(A);
h = extract_pseudo_inverse_row(Q, R, derivOrder);

if derivOrder == 0
    hs = sum(h);
    if abs(hs) < 1e-14
        error('SG coeff sum ~ 0');
    end
    h = h / hs;
else
    h = h * (factorial(derivOrder) / (delta^derivOrder));
end
end

function h = sg_coeffs_asym_qr(left, right, polyOrder)
% Matches C# ComputeSGCoefficientsAsymmetric : asymmetric SG via Householder QR
if left < 0 || right < 0, error('left / right must be >= 0.'); end
W = left + right + 1;
if W <= 0, error('W must be > 0'); end

m = min(max(polyOrder, 0), W - 1);

A = zeros(W, m + 1);
for rIdx = 0:(W - 1)
    xv = rIdx - left;
    p = 1.0;
    for c = 0:m
        A(rIdx + 1, c + 1) = p;
        p = p * xv;
    end
end

[Q, R] = householder_qr(A);
h = extract_pseudo_inverse_row(Q, R, 0);

ss = sum(h);
if abs(ss) < 1e-14
    error('Asymmetric smoothing coeff sum ~ 0');
end
h = h / ss;
end

function h = sg_coeffs_asym_deriv_qr(left, right, polyOrder, derivOrder, delta)
% Matches C# ComputeSGCoefficientsAsymmetricDerivative
if left < 0 || right < 0, error('left / right must be >= 0.'); end
if derivOrder < 0, error('derivOrder must be >= 0'); end
if delta <= 0, error('delta must be > 0'); end

W = left + right + 1;
if W <= 0, error('W must be > 0'); end

m = min(max(polyOrder, 0), W - 1);
if derivOrder > m
    error('derivOrder must be <= effective polynomial order');
end

A = zeros(W, m + 1);
for rIdx = 0:(W - 1)
    xv = rIdx - left;
    p = 1.0;
    for c = 0:m
        A(rIdx + 1, c + 1) = p;
        p = p * xv;
    end
end

[Q, R] = householder_qr(A);
h = extract_pseudo_inverse_row(Q, R, derivOrder);

if derivOrder == 0
    ss = sum(h);
    if abs(ss) < 1e-14
        error('Asymmetric smoothing coeff sum ~ 0');
    end
    h = h / ss;
else
    h = h * (factorial(derivOrder) / (delta^derivOrder));
end
end

%% -------------------- Householder QR (matches C# HouseholderQR) --------------------
function [Q, R] = householder_qr(A)
% Thin Householder QR : A = Q * R
% Q is (rows x cols) with orthonormal columns; R is (cols x cols) upper-triangular.
% Sign convention : v(1) = x(1) + sign(x(1)) * ||x||  (matches C#)
[rows, cols] = size(A);
work = A;

vecs = cell(cols, 1);
taus = zeros(cols, 1);

for k = 1:cols
    v = work(k:rows, k);

    nrm = sqrt(sum(v .* v));

    if v(1) >= 0.0
        sgn = 1.0;
    else
        sgn = -1.0;
    end
    v(1) = v(1) + sgn * nrm;

    vNormSq = sum(v .* v);
    if vNormSq > 1e-30
        tau = 2.0 / vNormSq;
    else
        tau = 0.0;
    end

    vecs{k} = v;
    taus(k) = tau;

    for j = k:cols
        d = sum(v .* work(k:rows, j));
        d = d * tau;
        work(k:rows, j) = work(k:rows, j) - d * v;
    end
end

% Extract upper-triangular R (cols x cols)
R = zeros(cols, cols);
for ii = 1:cols
    for jj = ii:cols
        R(ii, jj) = work(ii, jj);
    end
end

% Build thin Q (rows x cols) by applying H1 H2 ... Hn to identity columns
Q = zeros(rows, cols);
for j = 1:cols
    col_vec = zeros(rows, 1);
    col_vec(j) = 1.0;

    % Reflections in reverse order
    for k = cols:-1:1
        v = vecs{k};
        tau = taus(k);

        d = sum(v .* col_vec(k:rows));
        d = d * tau;
        col_vec(k:rows) = col_vec(k:rows) - d * v;
    end

    Q(:, j) = col_vec;
end
end

function h = extract_pseudo_inverse_row(Q, R, rowIndex)
% Extracts row 'rowIndex' (0-based) of pseudo-inverse B = R^-1 * Q'
% Solves R' * c = e_{rowIndex} via forward substitution, then h = Q * c
% Matches C# ExtractPseudoInverseRow exactly
[~, cols] = size(Q);

c = zeros(cols, 1);
for ii = 1:cols
    if ii == (rowIndex + 1)
        rhs = 1.0;
    else
        rhs = 0.0;
    end
    for jj = 1:(ii - 1)
        rhs = rhs - R(jj, ii) * c(jj);   % R'(ii,jj) = R(jj,ii)
    end
    diag_val = R(ii, ii);
    if abs(diag_val) < 1e-14
        error('Design matrix is rank-deficient (near-zero diagonal in R during QR solve).');
    end
    c(ii) = rhs / diag_val;
end

h = Q * c;
end

%% -------------------- Validation --------------------
function validate_sg_derivative_params(n, r, polyOrder, derivativeOrder, delta)
windowSize = 2 * r + 1;
if r < 0 || fix(r) ~= r
    error('r must be non-negative integer.');
end
if windowSize > n
    error('Window size (=2 * r + 1) exceeds data length (%d). Reduce radius.', n);
end
if polyOrder < 0 || fix(polyOrder) ~= polyOrder
    error('polyOrder must be integer >= 0.');
end
if polyOrder >= windowSize
    error('polyOrder must be < windowSize.');
end
if derivativeOrder < 0 || fix(derivativeOrder) ~= derivativeOrder
    error('derivativeOrder must be integer >= 0.');
end
if derivativeOrder > polyOrder
    error('derivativeOrder must be <= polyOrder.');
end
if delta <= 0
    error('delta must be > 0.');
end
end

function validate_params(n, r, polyOrder, derivOrder, delta, sigmaFactor)
windowSize = 2 * r + 1;

if r < 0 || fix(r) ~= r
    error('r must be non-negative integer.');
end
if windowSize > n
    error('windowSize (=2 * r + 1) must be <= dataCount.');
end

if polyOrder < 0 || fix(polyOrder) ~= polyOrder
    error('polyOrder must be integer >= 0.');
end
if polyOrder >= windowSize
    error('polyOrder must be < windowSize.');
end

if derivOrder < 0 || fix(derivOrder) ~= derivOrder
    error('derivOrder must be integer >= 0.');
end
if derivOrder > polyOrder
    error('derivOrder must be <= polyOrder.');
end

if delta <= 0
    error('delta must be > 0.');
end

if isnan(sigmaFactor) || isinf(sigmaFactor) || sigmaFactor <= 0.0
    error('sigmaFactor must be finite and > 0.');
end
end
