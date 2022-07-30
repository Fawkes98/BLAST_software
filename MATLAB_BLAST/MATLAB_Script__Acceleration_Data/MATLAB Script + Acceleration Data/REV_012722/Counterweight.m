%% BLAST MATLAB Counterweight Configuration Calc
% Version 1
clear; close all; clc;

% ---------- Script Outline ----------
% - Input:
%   - experiment mass
% - Procedure:
%   - calculate exact counterweight reaction moment
%   - find configuration via max reaction moments
%   - find closest load number
% - Output:
%   - counterweight configuration
%   - counterweight load number
% - Function Definitions:
%   - get mass
%   - get center of mass
%   - get reaction moment

% ---------- Script Parameters ----------
% global variables
global d_a m_a d_e m_em t_p d_con m_10 d_10 m_5 m_sf d_sf m_bp d_bp ...
    m_pb d_pb m_p d_p

% system parameters
d_a = 0.338;    % (m) arm c.o.m. location
m_a = 18.293;   % (kg) arm mass
d_e = 1.5;      % (m) experiment c.o.m. location
m_em = 2.065;   % (kg) mount assembly mass

% counterweight plate parameters
t_p = 0.0127;   % (m) counterweight plate thickness
d_con = 0.0254;	% (m) configuration offset distance
m_10 = 4.538;   % (kg) 10lb plate mass
d_10 = 0.7068;  % (m) plate c.o.m.
m_5 = 2.267;    % (kg) 5lb plate mass

% counterweight assembly parameters
m_sf = 0.995;   % (kg) shaft and fasteners mass
d_sf = 0.5575;  % (m) shaft and fasteners c.o.m.
m_bp = 0.497;   % (kg) backing plate mass
d_bp = 0.7227;  % (m) backing plate c.o.m.
m_pb = 0.5;     % (kg) pin bar mass
d_pb = 0.3924;  % (m) pin bar c.o.m.
m_p = 0.1059;   % (kg) pin mass
d_p = 0.4004;   % (m) pin c.o.m.

% configuration parameters
load_nums = 0:0.5:16;
max_num = 8:2:16;
conf = 0:4;


%% ---------- Input ----------
%% User Input

% experiment mass
m_ex = 12;          % (kg) experiment mass
m_e = m_em + m_ex;  % (kg) total experiment assembly mass


%% ---------- Procedure ----------
%% Exact Reaction Moment

% arm moment
L_a = d_a*m_a;

% experiment moment
L_e = d_e*m_e;

% required reaction moment
L = L_a + L_e;


%% Select Lowest Possible Configuration

% determine max reaction moments
max_rxns = zeros(1,length(conf));
for i = 1:length(conf)
    max_rxns(i) = getRXN(conf(i), max_num(i));
end

% find smallest config
configuration = -1;
for i = 1:length(max_rxns)
    if L < max_rxns(i)
        configuration = conf(i);
        break
    end
end


%% Select Closest Load Number

% get all reactions for this configuration
deltas = zeros(1,1+2*max_num(configuration+1));
for i = 1:(length(deltas))
    deltas(i) = abs(L - getRXN(configuration, load_nums(i)));
end

% select load number that produces closest reaction
load_number = load_nums(deltas == min(deltas));


%% ---------- Output ----------
%% Two-Vector

% vector format (configuration, load number)
Data_Out = [configuration load_number];


%% ---------- Function Definitions ----------
%% Get Mass

function mass = getMass(load_num)

    % collect global variables
    global m_sf m_bp m_pb m_p m_10 m_5
    
    % calculate mass
    tens = floor(load_num)*m_10;
    fives = 2*(load_num - floor(load_num))*m_5;
    mass = m_sf + m_bp + m_pb + m_p + tens + fives;
    
end


%% Get Center of Mass

function com = getCOM(config, load_num)

    % collect global variables
    global m_sf d_sf m_bp d_bp m_pb d_pb m_p d_p m_10 d_10 m_5 t_p d_con
    
    % non-plate center of mass numerator
    sf_num = m_sf*(d_sf + config*d_con);
    bp_num = m_bp*(d_bp + config*d_con);
    pb_num = m_pb*(d_pb + config*d_con);
    p_num = m_p*(d_p + config*d_con);
    np_num = sf_num + bp_num + pb_num + p_num;
    
    % 10 plate center of mass numerator
    n_10 = floor(load_num);
    tens_mass = n_10*m_10;
    tens_pos = 0;
    if n_10 ~= 0
        positions = zeros(1,n_10);
        for i = 1:n_10
            positions(i) = d_10 + config*d_con - (i-1)*t_p;
        end
        tens_pos = sum(positions)/n_10;
    end
    tens_num = tens_mass*tens_pos;
    
    % 5 plate center of mass numerator
    fives_num = 0;
    if load_num ~= floor(load_num)
        fives_pos = d_10 + config*d_con - n_10*t_p;
        fives_num = m_5*fives_pos;
    end
    
    % total system center of mass
    com = (np_num + tens_num + fives_num)/getMass(load_num);
    
end


%% Get Reaction Moment

function rxn_mom = getRXN(config, load_num)

    % get mass and c.o.m.
    mass = getMass(load_num);
    com = getCOM(config, load_num);
    
    % calculate reaction moment
    rxn_mom = mass*com;
    
end

