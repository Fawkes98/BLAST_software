/* BOARDS Launch Acceleration Simulation Tool (BLAST) Motion Control Software (C# Component)
 * 
 * This script handles the following:
 * - Receive calls from MATLAB Component of Software
 * - Read and import MATLAB Component output 'Processed_Profile.csv'
 * - 

*/
/*PREVIOUSLY
 /** Example demonstrating the motion profile control mode of Talon SRX.
 */
using CTRE.Phoenix;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.Motion;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.MotorControl.CAN;
using HERO_Motion_Profile_Example;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
//using System.Linq;
//using System.Collections.Generic;
using System.Text;
using System.Threading;
//using Microsoft.VisualBasic.FileIO;


namespace Hero_Motion_Profile_Example
{
    public class Step
    {
        public double Time { get; set; }
        public double Velocity { get; set; }
        public bool IsBraking { get; set; }
    }

    //public class Program
    //{
        //static void Main(string[] args)
        //{
        //    var filepath = @"C:\Users\viola\Desktop\BLAST Software Package\MATLAB Script + Acceleration Data\REV_012722\Processed_Profile.csv";
        //    Console.WriteLine(filepath);

        //    var reader = new StreamReader(filepath);
        //    var steps = new List<Step>();
        //    while (!reader.EndOfStream)
        //    {
        //        var line = reader.ReadLine();
        //        var values = line.Split(',');

        //        steps.Add(new Step
        //        {
        //            Time = Convert.ToDouble(values[0]),
        //            Velocity = Convert.ToDouble(values[1]),
        //            IsBraking = values[2] == "1",
        //        });
        //    }

            //foreach (var step in steps)
            //{
            //    Console.WriteLine($"Time-{step.Time},Velocity-{step.Velocity},IsBraking-{step.IsBraking}");
            //}

            //Console.WriteLine($"{steps.Count()}");

        //}

    //}


    /** Simple stub to start our project */
    public class Program
    {
        static RobotApplication _robotApp = new RobotApplication();
        public static void Main()
        {
            while (true)
            {
                _robotApp.Run();
            }
        }
    }
    /**
     * The custom robot application.
     */
    public class RobotApplication
    {
        TalonSRX _talon = new TalonSRX(0);
        bool[] _btns = new bool[10];
        bool[] _btnsLast = new bool[10];
        StringBuilder _sb = new StringBuilder();
        StringBuilder _watchSB = new StringBuilder();
        int _timeToPrint = 0;
        int _timeToColumns = 0;
        const int kTicksPerRotation = 4096;
        bool oneshot = false;

        MotionProfileStatus _motionProfileStatus = new MotionProfileStatus();
        //MotionProfileStatus _trajectoryPos = new MotionProfileStatus();

    public void Run()
        {
            //_talon.SetControlMode(TalonSRX.ControlMode.kVoltage);
                       
            /**define feedback device (CTRE Magnetic Encoder, Absolute Pos. Indexing)*/
            _talon.ConfigSelectedFeedbackSensor(FeedbackDevice.CTRE_MagEncoder_Absolute, 0);
            
            /**set encoder direction*/
            _talon.SetSensorPhase(true);
            
            /**reset sensor position*/
            _talon.SetSelectedSensorPosition(0);

            /**set motor control parameters*/
            _talon.Config_kP(0, 0.80f);
            _talon.Config_kI(0, 0f);
            _talon.Config_kD(0, 0f);
            _talon.Config_kF(0, 0.09724488664269079041176191004297f);
            _talon.SelectProfileSlot(0, 0);
            _talon.ConfigNominalOutputForward(0f, 50);
            _talon.ConfigNominalOutputReverse(0f, 50);
            _talon.ConfigPeakOutputForward(+1.0f, 50);
            _talon.ConfigPeakOutputReverse(-1.0f, 50);
            _talon.ChangeMotionControlFramePeriod(5);
            _talon.ConfigMotionProfileTrajectoryPeriod(0, 50);

            /**set GPIO pins and states*/
          

            //digitalOutKey.Write(true); //sets Output to Logic High

            InputPort digitalInKey = new InputPort(CTRE.HERO.IO.Port5.Pin4,false,Port.ResistorMode.PullDown);
            //OutputPort digitalOutKey = new OutputPort(CTRE.HERO.IO.Port5.Pin4,false);


            bool Ready = false;

            while (!Ready)
            {
                //_sb.Clear();
                //_sb.Append(Ready);
                //Debug.Print(_sb.ToString());

                Ready = digitalInKey.Read();          

                if (Ready)
                {
                    break;
                }
            }

            /* loop forever */
            while (true)
            {
                _talon.GetMotionProfileStatus(_motionProfileStatus);

                bool step = _motionProfileStatus.isLast;
                //int step = _motionProfileStatus.timeDurMs;
                _watchSB.Clear();
                _watchSB.Append(step);
                Debug.Print(_watchSB.ToString());
                //_talon.GetActiveTrajectoryPosition();    

                Drive();

                CTRE.Phoenix.Watchdog.Feed();

                Instrument();

                Thread.Sleep(10);
            }
        }

        void Drive()
        {

            _talon.ProcessMotionProfileBuffer();

            /* configure the motion profile once */
            if (!oneshot)
            {
                /* disable MP to clear IsLast */
                _talon.Set(ControlMode.MotionProfile, 0);
                CTRE.Phoenix.Watchdog.Feed();
                Thread.Sleep(10);
                /* buffer new pts in HERO */
                TrajectoryPoint point = new TrajectoryPoint();
                _talon.ClearMotionProfileHasUnderrun();
                _talon.ClearMotionProfileTrajectories();
                for (uint i = 0; i < MotionProfile.kNumPoints; ++i)

                {
                    point.position = (float)MotionProfile.Points[i][0] * kTicksPerRotation; //convert  from rotations to sensor units
                    point.velocity = (float)MotionProfile.Points[i][1] * kTicksPerRotation / 600;  //convert from RPM to sensor units per 100 ms.
                    point.headingDeg = 0; //not used in this example
                    point.isLastPoint = (i + 1 == MotionProfile.kNumPoints) ? true : false;
                    point.zeroPos = (i == 0) ? true : false;
                    point.profileSlotSelect0 = 0;
                    point.profileSlotSelect1 = 0; //not used in this example
                    point.timeDur = TrajectoryPoint.TrajectoryDuration.TrajectoryDuration_10ms;
                    _talon.PushMotionProfileTrajectory(point);
                }
                /* send the first few pts to Talon */
                for (int i = 0; i < 5; ++i)
                //for (uint i = 0; i < MotionProfile.kNumPoints; ++i)
                {
                    CTRE.Phoenix.Watchdog.Feed();
                    Thread.Sleep(0);
                    _talon.ProcessMotionProfileBuffer();
                }

                /*start MP */
                _talon.Set(ControlMode.MotionProfile, 1);

                oneshot = true;
            }
        }

        void Instrument()
        {
            if (--_timeToColumns <= 0)
            {
                _timeToColumns = 400;
                _sb.Clear();
                _sb.Append("topCnt \t");
                _sb.Append("btmCnt \t");
                _sb.Append("setval \t");
                _sb.Append("HasUndr\t");
                _sb.Append("IsUnder\t");
                _sb.Append(" IsVal \t");
                _sb.Append(" IsLast\t");
                _sb.Append("VelOnly\t");
                _sb.Append(" TargetPos[AndVelocity] \t");
                _sb.Append("Pos[AndVelocity]");
               // Debug.Print(_sb.ToString());
            }

            if (--_timeToPrint <= 0)
            {
                _timeToPrint = 40;

                _sb.Clear();
                _sb.Append(_motionProfileStatus.topBufferCnt);
                _sb.Append("\t\t");
                _sb.Append(_motionProfileStatus.btmBufferCnt);
                _sb.Append("\t\t");
                _sb.Append(_motionProfileStatus.outputEnable);
                _sb.Append("\t\t");
                _sb.Append(_motionProfileStatus.hasUnderrun ? "   1   \t" : "       \t");
                _sb.Append(_motionProfileStatus.isUnderrun ? "   1   \t" : "       \t");
                _sb.Append(_motionProfileStatus.activePointValid ? "   1   \t" : "       \t");

                _sb.Append(_motionProfileStatus.isLast ? "   1   \t" : "       \t");

                _sb.Append(_talon.GetActiveTrajectoryPosition());
                _sb.Append("[");
                _sb.Append(_talon.GetActiveTrajectoryVelocity());
                _sb.Append("]\t");


                _sb.Append("\t\t\t");
                _sb.Append(_talon.GetSelectedSensorPosition(0));
                _sb.Append("[");
                _sb.Append(_talon.GetSelectedSensorVelocity(0));
                _sb.Append("]");
                _sb.Append("\t\t\t\t");
                _sb.Append(_talon.GetClosedLoopError());

               // Debug.Print(_sb.ToString());
            }
        }
    }
}