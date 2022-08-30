%% BLAST MATLAB Preprocessing
% Version 2.1
clear; close all; clc;

% ---------- Script Outline ---------- %%
% - Input:
%   - launch profile filepath
%   - smoothing factor
% - Acceleration Procesing:
%   - interpolate data
%   - clip max and min accelerations
%   - smooth acceleration
% - Velocity Profile Conversion:
%   - calculate raw (clipped) angular velocity
%   - calculate processed angular velocity
% - Brake Flagging:
%   - find braking points
%   - incorporate application and release delays
% - Jerk Calculations:
%   - calculate raw linear jerk 
%   - calculate clipped linear jerk
%   - calculate processed linear jerk 
%   - calculate raw angular jerk 
%   - calculate processed angular jerk
% - Plotting:
%   - plot original and processed linear accel w/ colored brake sections
%   - plot original and processed angular speed w/ colored brake sections
%   - plot orignal and processed linear jerk
%   - plot original and processed angular jerk 
% - Error Analysis:
%   - linear acceleration rmse
%   - clipped linear acceleration rmse
%   - angular speed rmse
%   - linear jerk rmse
%   - clipped linear jerk rmse
%   - angular jerk rmse
% - Output:
% 	- formatted launch profile
% - Function Definitions:
%   - first derivative
%   - second derivative
%   - root mean square error

% ---------- Script Parameters ---------- %%
% constants
g = 9.81;               % (m/s^2)
dt = 0.01;              % (s) timestep of output data
r = 1.5;                % (m) boom length
b = 0.1;                  % (m) pivot to payload CG

% acceleration clipping
max_accel = 15*g;       % (m/s^2) maximum acceleration allowed
min_accel = 1.005*g;	% (m/s^2) minimum acceleration allowed

% braking
brake_trig = -0.02;     % (rad/s^2) slope to trigger brake
brake_on = 0.1;         % (s) time delay to apply brake
brake_off = 0.02;       % (s) time delay to release brake


%% ---------- Input ---------- %%
%% User Input

% smoothing parameters
tau = 3; % (s) filter time constant

% import file
file = "Data.csv";
% file = "D:\1 School\2 Projects\Blast\Matlab_Preprocessing\Data.csv";
Data = importdata(file);
t = Data(:,1);
ax_raw = Data(:,2);


%% ---------- Acceleration Processing ---------- %%
%% Interpolate Data

% preallocate vectors
dt_old = t(2) - t(1);
t_diff = dt_old/dt;
t_temp = t(1):dt:t(end);
a_temp = zeros(1,length(t_temp));

% interpolation scheme
for i = 1:(length(t)-1)
    t_0 = t(i); t_f = t(i+1);
    a_0 = ax_raw(i); a_f = ax_raw(i+1);
    m = (a_f-a_0)/(t_f-t_0);
    b = a_0 - m*t_0;
    for j = 1:t_diff
        a_temp((i-1)*t_diff + j) =  m*t_temp((i-1)*t_diff + j) + b;
    end
end
a_temp(end) = ax_raw(end);

% overwrite old vectors
t = t_temp;
ax_raw = a_temp;


%% Clip Acceleration Data

% new clipped vector
ax_clip = ax_raw;

% apply lower acceleration limit
mins = find(ax_clip < min_accel);
for i = 1:length(mins)
    ax_clip(mins(i)) = min_accel;
end

% apply upper acceleration limit
maxs = find(ax_clip < min_accel);
for i = 1:length(maxs)
    ax_clip(maxs(i)) = max_accel;
end


%% Smooth Acceleration

% limit cutoff frequency via Nyquist criterion
sample_rate = 1/dt;
fc_max = 0.5*sample_rate;
fc = 1/tau;
if fc >= fc_max
    fc = 0.99*fc_max;
end

% low pass first order butterworth filter
d = designfilt('lowpassiir', ...
               'DesignMethod', 'butter', ...
               'FilterOrder', 1, ...
               'HalfPowerFrequency', fc, ...
               'SampleRate', sample_rate);
   
% forwards-backwards filter data
ax_proc = filtfilt(d, ax_clip);


%% ---------- Velocity Profile Conversion ---------- %%
%% Calculate Raw (Clipped) Velocity

omega_raw = [];
for i = 1:length(ax_clip)
    omega_raw(i) = sqrt((ax_clip(i)^2 - g^2)/(r + b*sqrt(1 - (g/ax_clip(i))^2)));
end


%% Calculate Processed Velocity

omega_proc = [];
for i = 1:length(ax_proc)
    omega_proc(i) = sqrt((ax_proc(i)^2 - g^2)/(r + b*sqrt(1 - (g/ax_proc(i))^2)));
end


%% ---------- Position Profile Conversion ---------- %%
%% Calculate Raw (Clipped) Position
% before smashing that for-loop button i'm trying symbolic integration of
% the velocity equation above (derived from sys. dynamics)

%fun = @(x) sqrt((x^2 - g^2)/(r+b*sqrt(1 - (g/x)^2)));
%intfun = integral(fun,0,Inf);

% the above efforts were halted once it became clear our dynamic equation
% has no indefinite integral that can be calculated symbollically here...
% on to that for-loop

theta_raw = [];
for i = 1:length(ax_clip)
    %theta_raw(i) = 
        %some looped iterative integral of the velocity equation
    %velocity equation (still bullshit): sqrt((ax_clip(i)^2 - g^2)/(r + b*sqrt(1 - (g/ax_clip(i))^2)));
end


%% Calculate Processed Position

theta_proc = [];
for i = 1:length(ax_proc)
    theta_proc(i) = sqrt((ax_proc(i)^2 - g^2)/(r + b*sqrt(1 - (g/ax_proc(i))^2)));
end


%% ---------- Brake Flagging ---------- %%
%% Find Braking Points

% determine everywhere brake should be applied
brakes = zeros(1,length(t));
for i = 1:(length(t)-1)
    if (omega_proc(i+1)-omega_proc(i))/(t(i+1)/t(i)) < brake_trig
        brakes(i) = 1;
    end
end


%% Application and Release Delays

% determine start and end points of braking section
starts = [];
ends = [];
status = 0;
for i = 1:(length(brakes)-1)
    if (brakes(i+1) == 1) && (status == 0)
        starts = [starts i];
        status = 1;
    elseif (brakes(i+1) == 0) && (status == 1)
        ends = [ends i];
        status = 0;
    end
end

% implement brake delays
for i = 1:length(starts)
    starts(i) = starts(i) - round(brake_on/dt);
    ends(i) = ends(i) - round(brake_off/dt);
end

% combine overlapping brake periods 
for i = 1:(length(starts)-1)
    if ends(i) >= starts(i+1)
        ends(i) = 0;
        starts(i+1) = 0;
    end
end
starts(starts == 0) = [];
ends(ends == 0) = [];

% finalize brake flags vector
brakes = zeros(1,length(t));
for i = 1:length(starts)
    brakes(starts(i):ends(i)) = 1;
end


%% ---------- Jerk Calculations ---------- %%
%% First Derivatives (linear)

% raw linear jerk
j_lin_raw = firstDeriv(ax_raw, t);

% clipped linear jerk
j_lin_clip = firstDeriv(ax_clip, t);

% processed linear jerk
j_lin_proc = firstDeriv(ax_proc, t);


%% Second Derivatives (angular)

% raw angular jerk
j_ang_raw = secondDeriv(omega_raw, t);

% processed angular jerk
j_ang_proc = secondDeriv(omega_proc, t);


%% ---------- Plotting ---------- %%
%% Original and Processed Linear Acceleration

figure(1)
hold on
grid on
plot(t, ax_raw/g, "k", "linewidth", 2)
plot(t, ax_proc/g, "r", "linewidth", 2)
plot(t(brakes == 1), ax_proc(brakes == 1)/g, ".b", "markersize", 10) 
title("Linear Acceleration")
xlabel("Time (s)")
ylabel("Acceleration (g)")
legend("Original", "Processed", "Braking Section", "location", "north")


%% Original and Processed Angular Speed

figure(2)
hold on
grid on
plot(t, omega_raw, "k", "linewidth", 2)
plot(t, omega_proc, "r", "linewidth", 2)
plot(t(brakes == 1), omega_proc(brakes == 1), ".b", "markersize", 10) 
title("Angular Speed")
xlabel("Time (s)")
ylabel("\omega (rad/s)")
legend("Original", "Processed", "Braking Section", "location", "north")


%% Original and Processed Linear Jerk

figure(3)
hold on
grid on
plot(t, j_lin_raw, "k", "linewidth", 2)
plot(t, j_lin_proc, "r", "linewidth", 2) 
title("Linear Jerk")
xlabel("Time (s)")
ylabel("Jerk (m/s^3)")
legend("Original", "Processed", "location", "north")


%% Original and Processed Angular Jerk

figure(4)
hold on
grid on
plot(t, j_ang_raw, "k", "linewidth", 2)
plot(t, j_ang_proc, "r", "linewidth", 2) 
title("Angular Jerk")
xlabel("Time (s)")
ylabel("Jerk (rad/s^3)")
legend("Original", "Processed", "location", "north")


%% ---------- Error Analysis ---------- %%
%% Acceleration RMSEs

% raw RMSE
ax_raw_rmse = rmse(ax_raw, ax_proc);

% clipped RMSE
ax_clip_rmse = rmse(ax_clip, ax_proc);


%% Angular Speed RMSE

% angular rmse
omega_rmse = rmse(omega_raw, omega_proc);


%% Jerk RMSEs

% raw linear RMSE
j_lin_raw_rmse = rmse(j_lin_raw, j_lin_proc);

% clipped linear RMSE
j_lin_clip_rmse = rmse(j_lin_clip, j_lin_proc);

% angular RMSE
j_ang_rmse = rmse(j_ang_raw, j_ang_proc);


%% ---------- Output ---------- %%
%% Launch Profile

% Data Format: [time (s), angular position (rad), angular speed (rad/s), brake flag (0/1)]
Data_Out = [t' omega_proc' brakes'];
%writematrix(Data_Out, "Processed_Profile.csv")

%% Hard-coding Processed Profile to MotionProfile.cs
% To be written in format friendly to HERO_Motion_Profile_Example

% Commented below for note:
% //This can be pasted into an array for direct use in C#.
% //  Position (rotations),   Velocity (RPM)
% namespace HERO_Motion_Profile_Example {
% public class MotionProfile {
%     public const uint kNumPoints = ___;
%     public const uint kDurationMs = __;
%     public static double [][] Points = new double [][]{
%     new double[]{x_1, v_1, b_1    },
%     new double[]{x_2, v_2, b_2    },
%     ...
%     new double[](x_kNumPoints, v_kNumPoints, b_kNumPoints    }};
%     }}

header4 = sprintf('public const uint kNumPoints = %d;', length(Data_Out));
header5 = sprintf('public const uint kDurationMs = %2.0f;', dt*1000);

headerArray = {'//  Position (rotations),   Velocity (RPM)';...
               'namespace HERO_Motion_Profile_Example {';...
               'public class MotionProfile {';
               header4;
               header5;
               'public static double [][] Points = new double [][]{'};

expFile = fopen('MotionProfile.txt','w');
formatSpecHeader = '%s\n';
for i = 1:length(headerArray)
    fprintf(expFile,formatSpecHeader,headerArray{i,:});
end

fclose(expFile);

motionProfile = zeros(length(Data_Out),3);
% for i = 1:length(Data_Out)
%     
% 
% end

%% ---------- Function Definitions ---------- %%
%% First Derivative 

function yout = firstDeriv(yin, xin)
    % initialize
    yout = [];
    % fwd difference approx
    yout(1) = (yin(2) - yin(1))/(xin(2) - xin(1));
    % central difference approx
    for i = 2:(length(yin) - 1)
        yout(i) = (yin(i+1) - yin(i-1))/(xin(i+1) - xin(i-1));
    end
    % backward difference approx
    yout = [yout (yin(end) - yin(end-1))/(xin(end) - xin(end-1))];
end


%% Second Derivative

function yout = secondDeriv(yin, xin)
    % initialize
    yout = [];
    % second order forward difference
    yout(1) = (yin(3) - 2*yin(2) + yin(1))/(xin(2)-xin(1))^2;
    % second order central difference
    for i = 2:(length(yin)-1)
        yout(i) = (yin(i+1) - 2*yin(i) + yin(i-1))/(xin(i+1)-xin(i))^2;
    end
    % second order backward difference
    yout = [yout (yin(end) - 2*yin(end-1) + yin(end-2))/(xin(end)-xin(end-1))^2];
end


%% Root Mean Square Error

function err = rmse(true, processed)
    % initialize square error
    se = [];
    % callculate square errors
    for i = 1:length(true)
        se(i) = (true(i) - processed(i))^2;
    end
    % calculate root mean
    err = sqrt(sum(se)/length(true));
end
