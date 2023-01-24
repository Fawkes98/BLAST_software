clear all; close all; clc;

%% Variables - Changable/Desired
 G = 0;                 % grav. coefficient
 t = .1;                  % [s] stopping-time
 omega1 = 8.901;         % [rad/s] specified arm angular velocity
                        % NOTE: if want max torque based on Gs, set to '0'
 

%% Variables - Fixed
  % Raw
    g = 9.81;           % [m/(s^2)]
    r = 1.5;            % [m] boom length
    b = .1;             % [m] pivot to payload CG

    % component moments of inertia about rotational axis
    Iyy_s = .0002;      % [kg(m^2)] shaft
    Iyy_a = 7.5519;     % [kg(m^2)] arm
    Iyy_cw = 6.8559;    % [kg(m^2)] counter weight
    Iyy_ex = 34.0264;   % [kg(m^2)] experiment
    Iyy_hex = 0;        % [kg(m^2)] hex shaft

    %hex shaft
    hex = .5;                   % [in] face-to-face diameter of hex shaft
    shaft = 1;                  % [in] diameter of main keyed shaft
    d_hex = hex*.0254;          % [m] face-to-face diameter of hex shaft
    r_hex = d_hex/2;            % [m] center-to-face radius (shortest) of hex shaft
    J = .12*(d_hex^4);          % [m^4] Polar moment of inertia for hex section (https://www.engineersedge.com/polar-moment-inertia.htm)
    Js = (pi/2)*((shaft/2)^4);  % [m^4] Polar moment of inertia for shaft
    
  % Calculated
    % solve for omega based on desired Gs
    %omega1 = sqrt((g*G)/r)
    if omega1 == 0
        omega1 = sqrt(((g*G) - g)/(r + b*sqrt(1 - (g/(g*G))^2))) %from Preprocessing
    end

    
%% Finding torque experienced by shaft
T = (Iyy_s + Iyy_a + Iyy_cw + Iyy_ex)*(omega1/t) % (neg)[N*m]


%% Finding stress on shaft (assum. no amp torque or mean moment)
% Tau_mHEX = (T*r_hex)/J       % Shear felt by hex shaft [N/(m^2)]
% Sig_a = Tau_mHEX             % [N/(m^2)] due to no other applied bending moment (principal stress eq)
Tau_mSFT = (T*(shaft/2))/Js  % Shear felt by main keyed shaft [N/(m^2)]
