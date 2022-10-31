%% BLAST Velocity Profiling
clear all; close all; clc;

%% NOTE: everything is in terms of motor output, not arm output
% Input: 2 column matrix [ t', RPM' ]
% Output: 3 col matrix   [ t', rotations', RPM' ]

%% ------------ Script Parameters ------------ %%
% constants
g = 9.81;               % (m/s^2)
dt = 0.01;              % (s) timestep of output data
r = 1.5;                % (m) boom length
b = .1;                 % (m) pivot to payload CG
G = 60;                 % gear reduction ratio G:1
t = 0; v = 0; grav = 1; % base vector values for 'Wv' & 'Wg'


%% ------------ Input ------------ %%

% Vectors
t(2:end) = [5 10 15];   % 
v(2:end) = [];          % angular speed in rotations per second
grav(2:end) = [2 5 1];  % 

% Method Decision
chosen = 'I' % choose 'I' import, 'Wv' writen velocity, or 'Wg' writen gravity
    % import uses 2 column matrix from excel sheet
    % writen uses 2 vectors writen into matlab

rotation = 'arm'    % choose 'arm' if w is arm rotation,
                    % 'motor' if w is based on the motor


%% ------------ Calculation Tree ------------ %%
switch rotation
    case 'arm'
        w = 60*(2*pi)*w; % radians per rotation seen by motor
    case 'motor'
        w = (2*pi)*w; % radians per rotation seen by motor
end

switch chosen
% Write profile with velocities
case 'Wv'


% Write profile with gravity coef.
case 'Wg'
    w = 0;
    for j = 1:length(grav)
        w(i) = sqrt((grav(i) - g)/(r + b*sqrt(1 - (g/grav(i))^2)));
    end

% Import file
case 'I'
    % choose import data
    fprintf('Choose .csv to upload as data spreadsheet. Hit enter for default selection ''testVelocity60RPMflat.csv''\n');
    fprintf('   Note: must be in same folder as ''VelocityProfile.m'' version\n');
    file = input('Input desired .csv file name: ', 's');
    % if empty condition (can change default)
    if isempty(file)
    file = "testVelocity60RPMflat.csv";
    %elseif file = '\'
    end
% file = "D:\1 School\2 Projects\Blast\Matlab_Preprocessing\Data.csv";
Data = importdata(file);
t = Data(:,1);
v = Data(:,2); % in terms of rotations/second, NOT RPM
end


%% ---------- Time & Velocity Processing ---------- %%
%% Interpolate Data for motor

% Need to make interpolate & expand info given if not spaced by desired 'dt'
% Creation of new velocity based profile (Linear fit)

i = 2; %counter
while i <= length(t) %t_length %setting up open loop condition (NOTE: allows t to change)
    t_width = t(i)-t(i-1);
    if t_width > dt && (t_width/dt) > 1.000000001 %assuming dt < (time step), & calc error removal (1e-9)
       % --- velocity fit ---
       vDummyL = v(1:(i-1)); vDummyR = v(i:end); %bracket sections around discrepancy
       vDumCenter(1) = v(i-1) + dt; %generate first new t point
       V_lin = polyfit([t(i-1) t(i)], [v(i-1) v(i)], 1); %linear velocity between 2 points
       % --- time revector ---
       dummyL = t(1:(i-1)); dummyR = t(i:end); %bracket sections around discrepancy
       dumCenter(1) = t(i-1) + dt; %generate first new t point
       for j = 2:((t_width/dt)-1) % # of steps needed inserted, minus end condition
           dumCenter(j) = dumCenter(j-1) + dt; %additional time steps between t(i-1) & t(i)
           vDumCenter(j) = (V_lin(1)*dumCenter(j)) + V_lin(2); %linear stepped velocity at calc time step
       end
       % --- time expansion ---
       t = dummyL; 
       t((end+1):(end+length(dumCenter))) = dumCenter; 
       t((end+1):(end+length(dummyR))) = dummyR;
       % --- velocity expansion ---
       v = vDummyL; 
       v((end+1):(end+length(vDumCenter))) = vDumCenter; 
       v((end+1):(end+length(vDummyR))) = vDummyR;
       % --- advance ---
       i = i+1; %advance to next step
    else
       i = i+1; %advance to next input
    end
end


%% ---------- Position Processing ---------- %%
% Rotational Position experienced by motor
pos = [0];
for i = 2:length(t)
    pos(i) = pos(i-1) + (v(i-1)*dt); % rotations by integration
end


%% ------------ Output ------------ %%
VFit_Out = [t' pos' v'];

figure(1); title('Arm Rotations per Second'); xlabel('Seconds [s]'); ylabel('Arm Rotations [RPS]');
plot(t, v, 'LineWidth', 2, 'b-');


%% ---- old output ---- %%
% 
% 
% % data format = [time (s), postion (rotations), angular speed (RPM)]
% VFit_Out = [t' pos' v'];
% %writematrix(VFit_Out, "VFit_Processed_Profile.csv")
% 
% % Hard-coding Processed Profile to MotionProfile.cs
% % To be written in format friendly to HERO_Motion_Profile_Example
% 
% % Commented below for note:
% % // This can be pasted into an array for direct use in C#.
% % // Position (rotations),   Velocity (RPM)
% % namespace HERO_Motion_Profile_Example {
% % public class MotionProfile {
% %     public const uint kNumPoints = ___;
% %     public const uint kDurationMs = __;
% %     public static double [][] Points = new double [][]{
% %     new double[]{x_1, v_1,    },
% %     new double[]{x_2, v_2,    },
% %     ...
% %     new double[](x_kNumPoints, v_kNumPoints, b_kNumPoints    }};
% %     }}
% 
% header4 = sprintf('public const uint kNumPoints = %d;', length(VFit_Out));
% header5 = sprintf('public const uint kDurationMs = %2.0f;', dt*1000);
% 
% headerArray = {'//  Position (rotations),   Velocity (RPM),   Brake (bool)';...
%                'namespace HERO_Motion_Profile_Example {';...
%                'public class MotionProfile {';
%                header4;
%                header5;
%                'public static double[] PointsPosition = new double []{'};
% 
% expFile = fopen('MotionProfile.txt','w');
% formatSpecHeader = '%s\n';
% for i = 1:length(headerArray)
%     fprintf(expFile,formatSpecHeader,headerArray{i,:});
% end
% 
% 
% % use fprintf to write line by line (for loop) into file based on above
% % --- now position specific values
%     for i = 1:length(VFit_Out-1) 
%         fprintf(expFile,['%2.9f,\n'], VFit_Out(i, 2)); %calling theta sections of VFit_Out
%     end
% % (intermediate) specific end point write condition 
%     i = length(VFit_Out); 
%     fprintf(expFile,['%2.9f\n};\n\n' ...
%         'public static double[] PointsVelocity = newdouble[]{\n'], VFit_Out(i, 2));
% % use fprintf to write velocity part
%     for i = 1:length(VFit_Out-1) 
%         fprintf(expFile,['%2.9f,\n'], VFit_Out(i, 3)); %calling omega sections of VFit_Out
%     end
% 
% % ---- no brake in velocity, created as false for ease ----
% % (intermediate) specific end point write condition 
%     i = length(VFit_Out); 
%     fprintf(expFile,['%2.9f\n};\n\n' ...
%         'public static bool[] BrakeFlag = newbool[]{' ...
%         '\nfalse\n};}}'], VFit_Out(i, 3));    
%     
% fclose(expFile); %close/ending writing to .txt


%% ------------ Transcribed Output (in terms of arm, not motor) ------------ %%
% will eventually do:
%  - vector print to see (see MAE107 column fprintf for loop)
%  - transcription of total time
%  - total rotations
%  - max velocity & time

