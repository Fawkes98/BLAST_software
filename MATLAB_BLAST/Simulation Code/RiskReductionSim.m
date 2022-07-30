close all;
clear;
clc;

%% Define Constants
k = 0.063; % Motor constant [Vs/rad]
V = 12;    % Applied voltage [V]
R = 0.3;   % Rotor Resistance [ohm]
Tf = 0.2;  % Viscous torque acting against direction of motion [Nm]
I = 60;    % Boom inertia [kg*m^2]
m = 13;    % CubeLab mass [kg]
a = 1.5;   % Boom length [m]
b = 0.0635;  % Bucket length [m]
b0 = 0.16; % Radial length of CubeLab [m]
kp = 40;   % Proportional constant
ki = 0.1;    % Integral constant
kd = -0.1;   % Derivative constant
Tp = 120;    % Peak torque [Nm]
Tb = -128;   % Max braking torque [Nm]
Wp = 11;  % Free speed [rad/s]
% Motor follows torque-speed curve where T <= (1-(w/wp))*Tp

%% Define Time Curve
time = 0:0.01:700;

%% Define Target Acceleration Curve
% acc_tgt(1:16001) = (time(1:16001).^2).*(40/25600) + 1; %Estimate of required profile, or a bunch of reasonable profiles
% acc_tgt(16002:20001) = time(16002:20001) * (5/40) -20;
% w_tgt = sqrt(acc_tgt./r); % Convert to target angular velocity

% Piecewise definition
acc_tgt(1) = 12;
acc_tgt(2:13951) = (time(2:13951).^2).*((37-12)/19460) + 12;
acc_tgt(13952:14001) = (time(13952:14001)-139.5).* (-8/0.5) + 37;
acc_tgt(14002:16001) = (time(14002:16001)-140).* (9.5/20) + 29;
acc_tgt(16002:16051) = (time(16002:16051)-160) .*(-38.5/0.5) + 38.5;
acc_tgt(16052:16551) = (time(16052:16551)-160.5) .* (7.5/5);
acc_tgt(16552:46001) = ((time(16552:46001)-165.5).^2) .* (10/86730) +7.5;
acc_tgt(46002:46051) = (time(46002:46051)-460) .*(-3.5/0.5) + 17.5;
acc_tgt(46052:50001) = (time(46052:50001) - 460.5) .* (2.5/39.5) + 14;
acc_tgt(50002:50051) = (time(50002:50051) - 500) .* (-4/0.5) + 16.5;
acc_tgt(50052:55001) = (time(50052:55001) - 500.5) .* (1.5/49.5) + 12.5;
acc_tgt(55002:55051) = (time(55002:55051) - 550) .* (-14/0.5) + 14;
acc_tgt(55052:55551) = (time(55052:55551) - 550.5) .* (5/5) + 0;
acc_tgt(55552:69951) = (time(55552:69951) - 555.5) .* (2/144) + 5;
acc_tgt(69952:70001) = (time(69952:70001) - 699.5) .* (-7/0.5) + 7;

w_tgt = sqrt(acc_tgt./a);  % Convert to target velocity profile


%% Generate Material for Video
fig = figure('position', [100 100 850 600]);

%% Set Initial Conditions
err0 = zeros(3, 70001);   % Error vector for 0 DOF. First row is P, second row I, third row D
err1 = zeros(3, 70001);   % Error vector for 1 DOF. Same rows
T0 = zeros(1, 70001);     % Vector containing torque values for 0 DOF system as function of time
T1 = zeros(1, 70001);     % Torque values for 1 DOF system
phi0 = zeros(2, 70001);   % Vector containing angular velocity for 0 DOF system. First row is angular acceleration, second row angular velocity
phi1 = zeros(3, 70001);   % Angular velocity for 1 DOF system. First row is angular acceleration, second row angular velocity
theta1 = zeros(3, 70001); % Information about bucket angle. First row is acceleration, second row angular velocity, third row position. Ranges from 0 to pi/2
A0 = zeros(3, 70001);     % Component accelerations for 0 DOF system. Positive x towards center of rotation, positive y in direction of rotation, positive z facing up
A1 = zeros(6, 70001);     % Component accelerations for 1 DOF system. Positive x towards bucket joint along theta, positive y in direction of rotation, positive z = y cross x.
                          % Rows 4 and 5 are pre-transformed coordinates
                          % positive x along negative a and positive z up
                          % Row six is at b+b0
A2 = zeros(6, 70001);     % Component accelerations for 1 DOF system. Positive x towards bucket joint along theta, positive y in direction of rotation, positive z = y cross x.
                          % Rows 4 and 5 are pre-transformed coordinates
                          % positive x along negative a and positive z up
                          % Row six is at b+b0
A3 = zeros(6, 46001);     % Component accelerations for 1 DOF system. Positive x towards bucket joint along theta, positive y in direction of rotation, positive z = y cross x.
                          % Rows 4 and 5 are pre-transformed coordinates
                          % positive x along negative a and positive z up
                          % Row six is at b+b0                          

%% Define Velocity Curve
for i = 1:length(time)
%     % Start with 0 DOF system
%     
%     % Calculate angular acceleration using torque from previous timestep
%     if i > 1
%         phi0(1, i) = T0(1, i-1)/(I+m*a^2);
%     else
%         phi0(1, i) = 0;
%     end
%     
%     % Calculate angular velocity using current angular acceleration and
%     % previous angular velocity. Euler's Method
%     if i > 1
%         phi0(2, i) = phi0(1, i)*0.01 + phi0(2, i-1);
%     else
%         phi0(2, i) = 0;
%     end
%     
%     % Calculate component accelerations. Derivation is available on the
%     % team website
%     A0(1, i) = a * phi0(2, i)^2;
%     A0(2, i) = a * phi0(1, i);
%     A0(3, i) = -9.81;
%     
%     % Error calculations
%     % Current error taken by difference of current target and current axial
%     % (x) acceleration
%     err0(1, i) = acc_tgt(i) - A0(1, i);
%     
%     % Integral error is current error plus previous integral error
%     if i > 1
%         err0(2, i) = err0(1, i)*0.01 + err0(2, i-1);
%     else 
%         err0(2, i) = err0(1, i)*0.01;
%     end
%     
%     % Derivative error is current - previous error divided by timestep
%     if i > 1
%         err0(3, i) = (err0(1, i) - err0(1, i-1))/0.01;
%     else
%         % Derivative is current error divided by timestep (previous is 0)
%         err0(3, i) = err0(1, i)/0.01;
%     end
%     
%     % Calcultate closed-loop torque based on acceleration error
%     T0(1, i) = kp*err0(1, i) + ki*err0(2, i) + kd*err0(3, i);
%     
%     % Check for saturation of the motor torque
%     if T0(1, i) > Tp*(1-(phi0(2, i)/Wp))
%         T0(1, i) = Tp * (1-(phi0(2, i)/Wp));
%     elseif T0(1, i) < Tb
%         T0(1, i) = Tb;
%     end
%     
%     % Apply viscous torque
%     T0(1, i) = T0(1,i) + (-1 * Tf * sign(phi0(2, i)));
    
    
    % 1 DOF System
    
    % Start with angular acceleration using previous torque and inertia
    if i > 1
        phi1(1, i) = T1(1, i-1)/(I + m*(a + b*sin(theta1(3, i-1)))^2);
    else
        phi1(1, i) = 0;
    end
    
    % Calculate angular velocity using current acceleration and previous
    % velocity
    if i > 1
        phi1(2, i) = phi1(2, i-1) + phi1(1, i)*0.01;
    else
        phi1(2, i) = 0;
    end
    
    if i > 1
        phi1(3, i) = phi1(3, i-1) + phi1(2, i)*0.01;
    else
        phi1(3, i) = 0;
    end
    
    
    % Determine theta based on component accelerations on the mass
    if i > 1
    theta1(3, i) = atan((-1*A1(4, i-1))/(1*A1(5, i-1)));
    else
        theta1(3, i) = 0;
    end
    
    if theta1(3, i) < 0
        theta1(3, i) = 0;
    end
    
    % Take derivative to find theta dot
%     theta1(2, i) = 0;
    if i > 1
        theta1(2, i) = (theta1(3, i) - theta1(3, i-1))/0.01;
    else
        theta1(2, i) = theta1(3, i)/0.01;
    end
    
    % Theta double dot = 0 by quasi-static assumption
    theta1(1, i) = 0;
    % Take derivative to find theta double dot
%     if i > 1
%         theta1(1, i) = (theta1(2, i) - theta1(2, i-1))/0.01;
%     else
%         theta1(1, i) = theta1(2, i)/0.01;
%     end
    
    % Find component accelerations using current angular information.
    % Derivations can be found on the team website
    % Start with row 4 (non-transformed x)
    A1(4, i) = (-b*theta1(1, i)*cos(theta1(3, i))) + (b*theta1(2, i)^2*sin(theta1(3, i))) + (phi1(2, i)^2*(a + b*sin(theta1(3, i))));
    % Row 5 (non-transformed z)
    A1(5, i) = (b*theta1(2, i)^2*cos(theta1(3, i))) + (b*theta1(1, i)*sin(theta1(3, i))) - 9.81;
    % Row 2 (y)
    A1(2, i) = (2*b*theta1(2, i)*phi1(2, i)*cos(theta1(3, i))) + (phi1(1, i)*(a + (b * sin(theta1(3, i)))));
    % Row 1 (transformed x)
    A1(1, i) = (A1(4, i) * sin(theta1(3, i))) - (A1(5, i) * cos(theta1(3, i)));
    % Row 3 (transformed z)
    A1(3, i) = +(A1(5, i) * sin(theta1(3, i))) + (A1(4, i) * cos(theta1(3, i)));
    % Row 6
    A1(6, i) = (((-(b+b0)*theta1(1, i)*cos(theta1(3, i))) + ((b+b0)*theta1(2, i)^2*sin(theta1(3, i))) + (phi1(2, i)^2*(a + (b+b0)*sin(theta1(3, i))))) * sin(theta1(3, i))) - ((((b+b0)*theta1(2, i)^2*cos(theta1(3, i))) + ((b+b0)*theta1(1, i)*sin(theta1(3, i))) - 9.81) * cos(theta1(3, i)));

    % Calculate error from average acceleration
    avg = (A1(6, i) + A1(1, i))/2;
    
    % Current error
    err1(1, i) = acc_tgt(i) - avg; %A1(1, i);
    
    if acc_tgt(i) < 9.81
        err1(1, i) = 9.81 - avg; %A1(1, i);
    end
    
    % Integral error
    if i > 1
        err1(2, i) = err1(1, i)*0.01 + err1(2, i-1);
    else
        err1(2, i) = err1(1, i)*0.01;
    end
    
    % Derivative error
    if i > 1
        err1(3, i) = (err1(1, i) - err1(1, i-1))/0.01;
    else
        err1(3, i) = err1(1, i)/0.01;
    end
    
    % Calcultate closed-loop torque based on acceleration error
    T1(1, i) = kp*err1(1, i) + ki*err1(2, i) + kd*err1(3, i);
    
    % Check for saturation of the motor torque
    if T1(1, i) > Tp*(1-(phi1(2, i)/Wp))
        T1(1, i) = Tp * (1-(phi1(2, i)/Wp));
    elseif T1(1, i) < Tb
        T1(1, i) = Tb;
    end
    
    % Check direction of torque
    if err1(1, i) < 0 % Too much acceleration
        if sign(T1(1, i)) == sign(phi1(2, i)) % torque and speed are in the same direction
            T1(1,i) = -1*T1(1, i); % Reverse direction of torque
        end
    end
    
    if phi1(2, i) < 0 % Spinning backwards
        if sign(T1(1, i)) == -1
            T1(1, i) = -1*T1(1, i);
        end
    end
    
    % Apply reaction torque
    T1(1, i) = T1(1,i) + (-1 * Tf * sign(phi1(2, i)));
    
    
    %% Video section
%     if mod(i, 100) == 0
%         clf(fig);
%         subplot(2, 2, 1);
%         plot(time, acc_tgt, 'k');
%         hold on;
%         plot(time(1:i), A1(1,1:i), 'r', time(1:i), A1(2,1:i), 'g', time(1:i), A1(3, 1:i), 'm');
%         xlabel('Time (s)');ylabel('Acceleration (m/s^2)');                                              %
%         legend('Target','A_x', 'A_y', 'A_z', 'Location','northeast');                                   %
%         title('Target vs. Simulated Acceleration Profile');                                     %
%         subplot(2, 2, 2);                                                                               %Uhhh... stuff
%         plot(time, w_tgt, 'k');                                                                         %
%         hold on;                                                                                        %
%         plot(time(1:i), phi1(2, 1:i), 'r');
%         xlabel('Time (s)');ylabel('Angular Velocity (rad/s)');
%         legend('Target', 'Simulated', 'Location', 'southeast');
%         title('Target vs. Simulated Velocity Profile');
%         subplot(2,2,[3,4]);
%         axis equal;
%         plot([0,1.5,1.5,0,0], [-0.05,-0.05,0.05,0.05,-0.05], 'k')
%         hold on;
%         plot([1.5, 1.5+0.15*sin(theta1(3, i))], [0, -0.15*cos(theta1(3, i))], 'k'); %Sling
%         s1 = sin(theta1(3, i));
%         c1 = cos(theta1(3, i));
%         plot([1.5+(0.15-.08128)*s1-0.103*c1, 1.5+(0.15+.08128)*s1-0.103*c1, 1.5+(0.15+.08128)*s1+0.103*c1, 1.5+(0.15-.08128)*s1+.103*c1, 1.5+(0.15-.08128)*s1-.103*c1], [-(.15-.08128)*c1-.103*s1, -(.15+.08128)*c1-.103*s1, -(.15+.08128)*c1+.103*s1, -(.15-.08128)*c1+.103*s1, -(.15-.08128)*c1-.103*s1],'r');
%         M(i/100) = getframe(fig);
%     end
end


%% Debug
debug = [T1; phi1; theta1; A1; acc_tgt; err1];


%% Plots
clf;
% subplot(1,2,1);
% plot(time, acc_tgt, 'k', time, A0(1, :), 'r', time, A0(2, :), 'g', time, A0(3, :), 'm');
% xlabel('Time (s)');ylabel('Acceleration (m/s^2)');
% legend('Target','A_x', 'A_y', 'A_z', 'Location','northeast');
% title('Target vs. Simulated Acceleration Profile: Fixed Boom');
% shg;
% 
% subplot(1,2,2);
% plot(time, w_tgt, 'k', time, phi0(2, :), 'r');
% xlabel('Time (s)');ylabel('Angular Velocity (rad/s)');
% legend('Target', 'Simulated', 'Location', 'southeast');
% title('Target vs Simulated Velocity Profile: Fixed Boom');
% shg;

figure(1)
plot(time, acc_tgt, 'k', time, A1(1, :), 'r', time, A1(2, :), 'g', time, A1(3, :), 'm');
hold on;
xlabel('Time (s)');ylabel('Acceleration (m/s^2)');
legend('Target','A_x', 'A_y', 'A_z', 'Location','northeast');
fig1T = title('Target vs. Simulated Acceleration Profile');
fig1T.FontSize = 12;
shg;

% figure(2)
% plot(time, w_tgt, 'k', time, phi1(2, :), 'r');
% xlabel('Time (s)');ylabel('Angular Velocity (rad/s)');
% legend('Target', 'Simulated', 'Location', 'southeast');
% title('Target vs. Simulated Velocity Profile');
% shg;

figure(2);
plot(time, acc_tgt, 'k--', time, A1(1, :), 'r', time, A1(6, :), 'b');
xlabel('Time (s)');ylabel('Acceleration (m/s^2)');
legend('Target','Inside of Experiment','Outside of Experiment','Location','northeast');
fig2T = title('Comparision of Acceleration along Radial Distance of Experiment');
fig2T.FontSize = 12;
shg;

figure(3);
hold on;
plot(T0, acc_tgt, 'k');
xlabel('Time (s)');ylabel('Acceleration (m/s^2)');
title('Example Acceleration Curve vs. Time','Multi-stage Rocket Launch to LEO')
grid minor;
hold off;
% figure()
% hold on;
% 
% subplot(3,1,1)
% plot(time, theta1(3,:))
% xlabel('Time (s)');ylabel('Theta (rad)');
% title('Theta vs. Time','FontSize',12)
% 
% subplot(3,1,2)
% plot(time, theta1(2,:))
% xlabel('Time (s)');ylabel('dTheta/dt (rad/s)');
% title('dTheta/dt vs. Time','FontSize',12)
% 
% subplot(3,1,3)
% plot(time, theta1(1,:))
% xlabel('Time (s)');ylabel('d^2Theta/dt^2 (rad/s^2)');
% title('d^2Theta/dt^2 vs. Time','FontSize',12)
% 
% hold off;

%% Process Video
% close all;
% [h, w, p] = size(M(1).cdata);
% hf = figure;
% set(hf, 'position', [150 150 w h]);
% axis off;
% movie(hf, M);
% mplay(M);

% set prev err to 0 for start
% int_err(0) = 0;
% 
% Propagate at time i:
% Find error at time i
%     Compare w(i) to w_tgt(i)
%     Save as curr_err(i)
%     add curr_err as int_err(i)
%     diff_err(i) = curr_err(i) - curr_err(i-1);
%         curr_err, only for the first step
% use err(i) to find torque based on PID constants to get T
% 
% dw = (T - tf)/inertia
% w(i+1) = w(i)+ delta.t * dw

%     % Calculate errors
%     err(1, i) = w_tgt(i) - w(i); % Find current error based on target and current velocity
%     if i == 1
%         err(2, i) = err(1, i); % to account for first iteration
%     else
%         err(2, i) = err(2, i) + err(2, i-1); % Add current error to previous integral
%     end
%     if i == 1
%         err(3, i) = err(1, i);
%     else
%         err(3, i) = (err(1, i) - err(1, i-1))/0.01; % Take difference of error
%     end
%     % Calculate torque
%     T(i) = kp*err(1, i) + ki*err(2, i) + kd*err(3, i);
%     % Account for saturation
%     if T(i) > (1-(w(i)/Wp))*Tp
%         T(i) = (1-(w(i)/Wp))*Tp;
%     end
%     dw = (T(i) - Tf)/I;
%     w(i+1) = w(i) + 0.01*dw;