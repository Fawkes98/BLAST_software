﻿/* BOARDS Launch Acceleration Simulation Tool (BLAST) Motion Control Software (C# Component)
 * 
 * This script will ideally handle the following:
 * - Receive calls from MATLAB Component of Software
 * - Read and import MATLAB Component output 'Processed_Profile.csv'
 * - Deploy Motion Profile to HERO and enable closed-loop control
 * - Store and export encoder data to .csv for error evaluation in MATLAB

*/
/*PREVIOUSLY
 /** Example demonstrating the motion profile control mode of Talon SRX.
 */

/*#define AUSX CTRE.HERO.IO.Port1
#define Z1 CTRE.HERO.IO.Port2
#define PY CTRE.HERO.IO.Port3
#define IKUX CTRE.HERO.IO.Port4
#define FY CTRE.HERO.IO.Port5
#define IUX CTRE.HERO.IO.Port6
#define Z2 CTRE.HERO.IO.Port7
#define ADSX CTRE.HERO.IO.Port8
*/
using CTRE.Phoenix;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.Motion;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.MotorControl.CAN;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
//using System;
//using System.IO;
using System.Text;
using System.Threading;

namespace HERO_Motion_Profile_Example
{

    //    /** CSV Import */
    //    public class Import
    //    {
    //        public object Filepath
    //        {
    //            get
    //            {
    //                return Filepath;
    //            }
    //            set
    //            {
    //                Filepath = @"C:\Users\viola\Desktop\BLAST Software Package\MATLAB Script + Acceleration Data\REV_012722\Processed_Profile.csv";
    //            }
    //        }

    //        public double Time { get; set; }
    //        public double Velocity { get; set; }
    //        public bool IsBraking { get; set; }

    //        //NearlyGenericArrayList inputProfile = new NearlyGenericArrayList(typeof(Step));


    //        //StringBuilder _motionProfile = new StringBuilder();




    //        //Console.WriteLine(filepath);

    //        //var reader = new StreamReader(filepath); /** problematic */
    //        //var steps = new List<Step>(); /** problematic */
    //        //while (!reader.EndOfStream)
    //        //{
    //        //    var line = reader.ReadLine();
    //        //    var values = line.Split(',');

    //        //    steps.Add(new Step
    //        //    {
    //        //        Time = Convert.ToDouble(values[0]),
    //        //        Velocity = Convert.ToDouble(values[1]),
    //        //        IsBraking = values[2] == "1",
    //        //    });
    //        //}

    //        //foreach (var step in steps)
    //        //{
    //        //    Console.WriteLine($"Time-{step.Time},Velocity-{step.Velocity},IsBraking-{step.IsBraking}");
    //        //}

    //        //Console.WriteLine($"{steps.Count()}");

    //        //}

    //    }

    public class Program
    {
        /** Simple stub to start our project */

        static RobotApplication _robotApp = new RobotApplication();
        public static void Main()
        {
            while (true)
            {
                //will always run immediately
                _robotApp.Run();
            }
        }

    }

    /** The custom robot application */

    public class RobotApplication
    {

        int DUTY_CYCLE_PERIOD = 250; //ms

        double[] PID = new double[] { 0.8, 0, 0.02 };

        TalonFX _talon = new TalonFX(0);
        bool[] _btns = new bool[10];
        bool[] _btnsLast = new bool[10];
        StringBuilder _sb = new StringBuilder();
        //StringBuilder _watchSB = new StringBuilder();
        int _timeToPrint = 0;
        int _timeToColumns = 0;
        const int kTicksPerRotation = 4096;

        const int falcon500ticksPerRotation = 2048;

        bool oneshot = false;
        bool[] brakeFlag = new bool[HERO_Motion_Profile_Example.MotionProfile.kNumPoints];

        private GameController _gamepad = new GameController(UsbHostDevice.GetInstance(0));

        OutputPort brakeSSR = new OutputPort(CTRE.HERO.IO.Port5.Pin5, false);
        bool brakeToggle = false;
        double brakeThreshold = 0;

        Stopwatch timer = new Stopwatch();

        int pointIndex = 0;

        long brakeTimeStart = 0;

        InputPort digitalInKey = new InputPort(CTRE.HERO.IO.Port5.Pin4, false, Port.ResistorMode.PullDown);

        public void Run()
        {
            UsbHostDevice.GetInstance(0).SetSelectableXInputFilter(UsbHostDevice.SelectableXInputFilter.XInputDevices);
            //_talon.SetControlMode(TalonFX.ControlMode.kVoltage);

            //_talon.ConfigFactoryDefault();

            /**define feedback device (CTRE Magnetic Encoder, Absolute Pos. Indexing)*/
            _talon.ConfigSelectedFeedbackSensor((FeedbackDevice)TalonFXFeedbackDevice.IntegratedSensor, 0);
            _talon.ConfigIntegratedSensorInitializationStrategy(CTRE.Phoenix.Sensors.SensorInitializationStrategy.BootToZero, 50);

            _talon.SetNeutralMode(NeutralMode.Coast);

            //set encoder direction
            _talon.SetSensorPhase(true);

            //reset sensor position
            _talon.SetSelectedSensorPosition(0);

            //set motor control parameters
            
            _talon.Config_kP(0, 0.55f);
            _talon.Config_kI(0, 0f);
            _talon.Config_kD(0, 1.4f);
            _talon.Config_kF(0, 0.04f);

            //_talon.Config_kP(1, 0f);
            //_talon.Config_kI(1, 0f);
            //_talon.Config_kD(1, 0f);
            //_talon.Config_kF(1, 0f);

            _talon.SelectProfileSlot(0, 0);
            _talon.ConfigNominalOutputForward(0f, 50);
            _talon.ConfigNominalOutputReverse(0f, 50);
            _talon.ConfigPeakOutputForward(+1f, 50);
            _talon.ConfigPeakOutputReverse(-0.0f, 50);
            _talon.ChangeMotionControlFramePeriod(5);
            
            //set GPIO pins and states

            //digitalOutKey.Write(true); //sets Output to Logic High

            
            //OutputPort digitalOutKey = new OutputPort(CTRE.HERO.IO.Port5.Pin4,false);

            bool Ready = false;

            /** Wait for "GO" input from operator */

            while (!Ready)
            {
                //_sb.Clear();
                //_sb.Append(Ready);
                //Debug.Print(_sb.ToString());

                Ready = digitalInKey.Read();

                if (Ready)
                {
                    Thread.Sleep(1000);
                    break;
                }
            }

            //  StopBraking();
            /* loop forever */
            timer.Start();
            while (true)
            {
                double dTime = HERO_Motion_Profile_Example.MotionProfile.timeArray[pointIndex + 1] - HERO_Motion_Profile_Example.MotionProfile.timeArray[pointIndex];
                double dVelocity = HERO_Motion_Profile_Example.MotionProfile.velocityArray[pointIndex + 1] - HERO_Motion_Profile_Example.MotionProfile.velocityArray[pointIndex];
                double interpolatedSpeed = (timer.DurationMs - HERO_Motion_Profile_Example.MotionProfile.timeArray[pointIndex]) * dVelocity / dTime;
                double tickSpeed = (HERO_Motion_Profile_Example.MotionProfile.velocityArray[pointIndex] + interpolatedSpeed) * (float)falcon500ticksPerRotation / 600.0 * 60;
                
                short printMode = 2; // 0 is none, 1 is complex, 2 is graph-printing
                if (printMode == 1)
                {
                    Debug.Print("[" + timer.DurationMs / 1000.0 + "s] " + "dTime:" + dTime + "\tdVelocity: " + dVelocity + "\tinterpolated: " + interpolatedSpeed + "\tdesired: " + (HERO_Motion_Profile_Example.MotionProfile.velocityArray[pointIndex] + interpolatedSpeed) + "\tactual: " + _talon.GetSelectedSensorVelocity(0) / (float)falcon500ticksPerRotation * 10 + "\tpointIndex[" + pointIndex + "]\tsentTps: " + tickSpeed + "\tpowerOut: " + _talon.GetMotorOutputPercent() + "brakePercent: " + ((float)dVelocity/dTime * -50) );
                }else if(printMode == 2){
                    Debug.Print("" + (HERO_Motion_Profile_Example.MotionProfile.velocityArray[pointIndex] + interpolatedSpeed) + "\t" + _talon.GetSelectedSensorVelocity(0) / (float)falcon500ticksPerRotation * 10 + "\t" + _talon.GetMotorOutputPercent());
                }

                if (!brakeToggle)
                {
                    _talon.Set(ControlMode.Velocity, tickSpeed);
                }

                if(dVelocity / dTime < -brakeThreshold && brakeToggle == false)
                {
                    StartBraking();
                }
                else if (dVelocity / dTime > -brakeThreshold && brakeToggle == true)
                {
                    StopBraking();
                }

                brake((double)dVelocity * -25.0/dTime);

                CTRE.Phoenix.Watchdog.Feed();
                if (timer.DurationMs > HERO_Motion_Profile_Example.MotionProfile.timeArray[pointIndex + 1])
                {
                    pointIndex += 1;
                }
                if(pointIndex + 1 == HERO_Motion_Profile_Example.MotionProfile.kNumPoints)
                {
                    break;
                }
                Thread.Sleep(1);
            }
            while (true)
            {
                _talon.Set(ControlMode.PercentOutput, 0);
                brakeSSR.Write(false);
            }
        }
        
        public void brake(double percent){ //0 to 1, double
            if(brakeToggle){
                long currentTime = timer.DurationMs;
                long dTime = currentTime - brakeTimeStart;

                long timeInPeriod = dTime % DUTY_CYCLE_PERIOD;
                if(((double)timeInPeriod / DUTY_CYCLE_PERIOD) < percent){
                    brakeSSR.Write(false);
                    //Debug.Print("On " + timeInPeriod + " %:" + percent);
                }else{
                    brakeSSR.Write(true);
                    //Debug.Print("Off " + timeInPeriod + " %:" + percent);
                }
            }
        }       

        //here's where we actually cause the braking to occur
        public void StartBraking()
        {
            brakeToggle = true;
            //Debug.Print("HALT");

            brakeTimeStart = timer.DurationMs;

            brakeSSR.Write(false); //WHEN YOU ENABLE THIS, REMEMBER TO MAKE MOTOR COAST DURING THIS PART
            _talon.Set(ControlMode.PercentOutput,0);
        }

        //here's where we remove the brake;
        public void StopBraking()
        {
            brakeToggle = false;
            //Debug.Print("STOP HALTING");

            brakeSSR.Write(true); //WHEN YOU ENABLE THIS, REMEMBER TO STOP MOTOR COAST DURING THIS PART
        }


    }
}