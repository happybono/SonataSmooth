% T represents the table (Index, Initial, RectAvg, BinomAvg, BinomMedian, GaussWMedian, Gauss, SG)
writetable(T, 'MATLAB_Result.csv', ...
    'Delimiter', ',', ...
    'WriteVariableNames', true);

% Verification (Optional)
disp("Saved: MATLAB_Result.csv");
