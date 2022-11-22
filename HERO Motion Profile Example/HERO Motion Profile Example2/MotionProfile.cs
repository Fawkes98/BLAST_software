//Select the green highlighted cells and paste into  a csv file.  			
//No need to copy the blank lines at the bottom.  			
//This can be pasted into an array for direct use in C++/Java.			
//       Position (rotations)	Velocity (RPM)	Duration (ms)	
namespace HERO_Motion_Profile_Example
{
    public class MotionProfile
    {
        public const uint kNumPoints = 6;
        // Time (ms),	Velocity (RPM)

        public static int[] timeArray = new int[] //ms
        {
            0,
            10000,
            20000,
            30000,
            40000,
            60000

        };

        public static double[] velocityArray = new double[] //rpm
        {
            0,
            30,
            30,
            60,
            60,
            0
        };
        public static bool[] brakeFlag = new bool[]
        {
            false
        };
    }
}