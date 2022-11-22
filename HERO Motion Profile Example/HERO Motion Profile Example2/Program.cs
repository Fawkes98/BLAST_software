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
            _talon.ConfigSelectedFeedbackSensor((FeedbackDevice)TalonFXFeedbackDevice.IntegratedSensor, 0);
            _talon.ConfigIntegratedSensorInitializationStrategy(CTRE.Phoenix.Sensors.SensorInitializationStrategy.BootToZero, 50);



            //set encoder direction
            _talon.SetSensorPhase(true);

            //reset sensor position
            _talon.SetSelectedSensorPosition(0);

            //set motor control parameters
            
            //_talon.Config_kP(0, 0.8f);
            //_talon.Config_kI(0, 0f);
            //_talon.Config_kD(0, 0.0f);
            //_talon.Config_kF(0, 0.0f);

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
                    Thread.Sleep(1000);
                    break;
                }
            }

            //  StopBraking();
            /* loop forever */
            float konstantP = 0.550f;
            float konstantI = 0f;
            float konstantD = 1.4f;
            float konstantF = 0.04f;
            int mode = 0;
            float lAxis = 0;
            
            while (true)
            {
                if (_gamepad.GetButton(5))
                {
                    lAxis = _gamepad.GetAxis(3);
                }
                if(lAxis < 0.05 && lAxis > -0.05)
                {
                    lAxis = 0;
                }

                int maxRPM = 30;//RPM
                int velocity = (int)(lAxis * maxRPM * (float)kTicksPerRotation / 600.0 * 60);

                //Debug.Print("kP:" + konstantP + " | kI:" + konstantI + " | kD:" + konstantD + " | kF:" + konstantF + " | VAL:" + lAxis + " | %:" + _talon.GetMotorOutputPercent() + " | D:" + lAxis * 4000f + " | A:" + _talon.GetSelectedSensorVelocity());

                Debug.Print("" + (lAxis * maxRPM) + "\t" + _talon.GetSelectedSensorVelocity(0) / (float)kTicksPerRotation * 10 + "\t" + _talon.GetMotorOutputPercent());

                if (_talon.GetSelectedSensorVelocity() > velocity);
                {
                    _talon.Set(ControlMode.Velocity, velocity);
                }
                //_talon.Set(ControlMode.PercentOutput, lAxis/2);
                if (_gamepad.GetButton(6))
                {
                    _talon.Set(ControlMode.PercentOutput, 0);
                }

                //_talon.Set(ControlMode.PercentOutput, _gamepad.GetAxis(3) * 0.3f);
                CTRE.Phoenix.Watchdog.Feed();
                _talon.Config_kP(0, konstantP);
                _talon.Config_kI(0, konstantI);
                _talon.Config_kD(0, konstantD);
                _talon.Config_kF(0, konstantF);
                float axis = _gamepad.GetAxis(1);
                if(axis < 0.01 && axis > -0.01)
                {
                    axis = 0;
                }
                if(mode == 0)
                {
                    konstantP += axis * 0.01f;
                }
                else if(mode == 1)
                {
                    konstantI += axis * 0.0001f;
                }
                else if (mode == 2)
                {
                    konstantD += axis * 0.001f;
                }
                float incrementF = (_gamepad.GetAxis(4) - _gamepad.GetAxis(5))/2.0f;
                if(incrementF > 0.01 || incrementF < -0.01)
                    konstantF += incrementF * 0.00001f;
                if (konstantP < 0) konstantP = 0; if (konstantI < 0) konstantI = 0; if (konstantD < 0) konstantD = 0;
                if (_gamepad.GetButton(1))
                {
                    mode = 0;
                    Debug.Print("------------------\n----- P MODE -----\n------------------");
                }
                else if (_gamepad.GetButton(2))
                {
                    mode = 1;
                    Debug.Print("------------------\n----- I MODE -----\n------------------");
                }
                else if (_gamepad.GetButton(3))
                {
                    mode = 2;
                    Debug.Print("------------------\n----- D MODE -----\n------------------");
                }

                brakeSSR.Write(!_gamepad.GetButton(10));


                if (/**digitalInKey.Read() || **/_gamepad.GetButton(4))
                {
                    Debug.Print("Paused due to Green Button:" + digitalInKey.Read() + " or Y:" + _gamepad.GetButton(4));
                    _talon.Set(ControlMode.PercentOutput, 0);
                    Thread.Sleep(1000);
                    while (true)
                    {
                        bool resume = (digitalInKey.Read() || _gamepad.GetButton(4));
                        
                        _talon.Set(ControlMode.PercentOutput, 0);
                        if (resume)
                        {
                            Debug.Print("Unpaused");
                            Thread.Sleep(1000);
                            break;
                        }
                    }
                }
                Thread.Sleep(5);
            }
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