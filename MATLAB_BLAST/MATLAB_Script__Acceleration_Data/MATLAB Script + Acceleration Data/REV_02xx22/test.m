clear all; clc;

fprintf('Choose .csv to upload as data spreadsheet. Hit enter for default selection ''Data.csv''\n');
fprintf('   Note: must be in same folder as ''Preprocessing'' version\n');
file = input('Input desired .csv file name:');
    % if empty condition
    if isempty(file)
    file = 'Data.csv';
    end
% Import into code
Data = importdata(file);
t = Data(:,1);
ax_raw = Data(:,2);