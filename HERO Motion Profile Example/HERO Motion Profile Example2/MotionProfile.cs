//Select the green highlighted cells and paste into  a csv file.  			
//No need to copy the blank lines at the bottom.  			
//This can be pasted into an array for direct use in C++/Java.			
//       Position (rotations)	Velocity (RPM)	Duration (ms)	
namespace HERO_Motion_Profile_Example
{
    public class MotionProfile
    {
        public const uint kNumPoints = 10;
        // Time (ms),	Velocity (RPM)

        public static int[] timeArray = new int[] //ms
        {
            0,
            5000,
            8000,
            10000,
            13000,
            15000,
            19000,
            21000,
            23000,
            100000

        };

        public static double[] velocityArray = new double[] //rpm
        {
            0,
            15,
            10,
            20,
            21,
            30,
            0,
            5,
            0,
            0
        };
        public static bool[] brakeFlag = new bool[]
        {
            false
        };
    }
}