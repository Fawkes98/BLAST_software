/* BOARDS Launch Acceleration Simulation Tool (BLAST) Motion Control Software (C# Component)
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
        double[] PID = new double[] { 0.8, 0, 0.02 };

        TalonFX _talon = new TalonFX(0);
        bool[] _btns = new bool[10];
        bool[] _btnsLast = new bool[10];
        StringBuilder _sb = new StringBuilder();
        //StringBuilder _watchSB = new StringBuilder();
        int _timeToPrint = 0;
        int _timeToColumns = 0;
        const int kTicksPerRotation = 4096;
        bool oneshot = false;
        bool[] brakeFlag = new bool[HERO_Motion_Profile_Example.MotionProfile.kNumPoints];

        private GameController _gamepad = new GameController(UsbHostDevice.GetInstance(0));

        OutputPort brakeSSR = new OutputPort(CTRE.HERO.IO.Port5.Pin5, false);
        bool brakeToggle = false;

        MotionProfileStatus _motionProfileStatus = new MotionProfileStatus();
        //MotionProfileStatus _trajectoryPos = new MotionProfileStatus();

        public void Run()
        {
            UsbHostDevice.GetInstance(0).SetSelectableXInputFilter(UsbHostDevice.SelectableXInputFilter.XInputDevices);
            //_talon.SetControlMode(TalonFX.ControlMode.kVoltage);

            _talon.ConfigFactoryDefault();
            
            /**define feedback device (CTRE Magnetic Encoder, Absolute Pos. Indexing)*/
            _talon.ConfigSelectedFeedbackSensor(FeedbackDevice.CTRE_MagEncoder_Absolute, 1);
            
            
            
            //set encoder direction
            _talon.SetSensorPhase(true);

            //reset sensor position
            _talon.SetSelectedSensorPosition(0);

            //set motor control parameters
            
            _talon.Config_kP(0, 0.8f);
            _talon.Config_kI(0, 0f);
            _talon.Config_kD(0, 0.0f);
            _talon.Config_kF(0, 0.09724488664269079041176191004297f);

            //_talon.Config_kP(1, 0f);
            //_talon.Config_kI(1, 0f);
            //_talon.Config_kD(1, 0f);
            //_talon.Config_kF(1, 0f);

            _talon.SelectProfileSlot(0, 0);
            _talon.ConfigNominalOutputForward(0f, 50);
            _talon.ConfigNominalOutputReverse(0f, 50);
            _talon.ConfigPeakOutputForward(+1.0f, 50);
            _talon.ConfigPeakOutputReverse(-1.0f, 50);
            _talon.ChangeMotionControlFramePeriod(5);
            _talon.ConfigMotionProfileTrajectoryPeriod(0, 50);
            
            //set GPIO pins and states

            //digitalOutKey.Write(true); //sets Output to Logic High

            InputPort digitalInKey = new InputPort(CTRE.HERO.IO.Port5.Pin4, false, Port.ResistorMode.PullDown);
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
                    if (_gamepad.GetButton(1))
                    {
                        Debug.Print("BUTTON");
                    }
                    break;
                }
            }

            //  StopBraking();
            /* loop forever */
            while (true)
            {
                _talon.GetMotionProfileStatus(_motionProfileStatus);

                bool step = _motionProfileStatus.isLast;

                //int step = _motionProfileStatus.timeDurMs;

                /** Printing status for debug*/
                //_watchSB.Clear();
                //_watchSB.Append(step);
                //Debug.Print(_watchSB.ToString());

                _talon.GetActiveTrajectoryPosition();

                Drive();

                //  ConfigureBrake( );

                CTRE.Phoenix.Watchdog.Feed();

                Instrument();

                if (_gamepad.GetButton(1))
                {
                    Debug.Print("BUTTON");
                }

                Thread.Sleep(5);
            }
        }

        void Drive()
        {

            _talon.ProcessMotionProfileBuffer();

            /* configure the motion profile once */
            if (!oneshot)
            {
                Debug.Print("Initializing Motion Profile - BLAST Lab");
                // StopBraking();//before starting, stop braking
                /* disable MP to clear IsLast */
                _talon.Set(ControlMode.MotionProfile, 0);
                CTRE.Phoenix.Watchdog.Feed();
                Thread.Sleep(10);

                /* buffer new pts in HERO */
                TrajectoryPoint point = new TrajectoryPoint();
                _talon.ClearMotionProfileHasUnderrun();
                _talon.ClearMotionProfileTrajectories();
                for (uint i = 0; i < HERO_Motion_Profile_Example.MotionProfile.kNumPoints; ++i)
                {
                    point.position = (float)HERO_Motion_Profile_Example.MotionProfile.PointsPosition[i] * (float)kTicksPerRotation; //convert  from rotations to sensor units
                    point.velocity = (float)HERO_Motion_Profile_Example.MotionProfile.PointsVelocity[i] * (float)kTicksPerRotation / 600.0;  //convert from RPM to sensor units per 100 ms 
                    point.headingDeg = 0; //not used in this example
                    point.isLastPoint = (i + 1 == HERO_Motion_Profile_Example.MotionProfile.kNumPoints) ? true : false;
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
                    Thread.Sleep(5);
                    _talon.ProcessMotionProfileBuffer();
                }

                /*start MP */
                _talon.Set(ControlMode.MotionProfile, 1);

                oneshot = true;
                
            }
            //Debug.Print("Falcon CUR:"+ _talon.GetOutputCurrent() + "\tFalcon VEL:"+ _talon.GetSelectedSensorVelocity() + "\tFalcon POS:" + _talon.GetSelectedSensorPosition());
            //Debug.Print("GamepadB0: " + _gamepad.GetButton(0));
            Debug.Print("" + _talon.GetActiveTrajectoryVelocity(1) + ", " + _talon.GetSelectedSensorVelocity(1));
        }

        void Instrument()
        {
            if (--_timeToColumns <= 0)
            {
                _timeToColumns = 100;
                _sb.Clear();
                _sb.Append("topCnt \t");
                _sb.Append("btmCnt \t");
                _sb.Append("setval \t");
                _sb.Append("HasUndr\t");
                _sb.Append("IsUnder\t");
                _sb.Append(" IsVal \t");
                _sb.Append(" IsLast\t");
                //_sb.Append("VelOnly\t");
                _sb.Append(" TargetPos[AndVelocity] \t");
                _sb.Append("Pos[AndVelocity]\t");
                _sb.Append("ClosedLoopError");
                //Debug.Print(_sb.ToString());
                brakeToggle = !brakeToggle;
                //brakeSSR.Write(brakeToggle);
            }

            if (--_timeToPrint <= 0)
            {
                _timeToPrint = 10;

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

                _sb.Append(_talon.GetActiveTrajectoryPosition(1) / 4096.0);
                _sb.Append("[");
                _sb.Append(_talon.GetActiveTrajectoryVelocity(1) / 4096.0);
                _sb.Append("]\t");


                _sb.Append("\t\t\t");
                _sb.Append(_talon.GetSelectedSensorPosition(1)/ 4096.0);
                _sb.Append("[");
                _sb.Append(_talon.GetSelectedSensorVelocity(1)/ 4096.0);
                _sb.Append("]");
                _sb.Append("\t\t\t\t");
                _sb.Append(_talon.GetClosedLoopError());

                //Debug.Print(_sb.ToString());
            }
        }

        public void ConfigureBrake()
        {
            int target = _talon.GetClosedLoopTarget();

            Debug.Print(target.ToString() + " is the target");

            return;

        }

        
        public void StartBraking()
        {
            Debug.Print("HALT");
            //here's where we actually cause the braking to occur

            //brakeSSR.Write(false); WHEN YOU ENABLE THIS, REMEMBER TO MAKE MOTOR COAST DURING THIS PART
        }

        public void StopBraking()
        {
            Debug.Print("DO NOT HALT");
            //here's where we remove the brake;

            //brakeSSR.Write(true); WHEN YOU ENABLE THIS, REMEMBER TO MAKE MOTOR COAST DURING THIS PART
        }


    }
}