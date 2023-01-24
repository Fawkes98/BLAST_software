//Select the green highlighted cells and paste into  a csv file.  			
//No need to copy the blank lines at the bottom.  			
//This can be pasted into an array for direct use in C++/Java.			
//       Position (rotations)	Velocity (RPM)	Duration (ms)	
namespace HERO_Motion_Profile_Example
{
    public class MotionProfile
    {
        public const uint kNumPoints = 12;
        // Time (ms),	Velocity (RPM)

        public static int[] timeArray = new int[] //ms
        {
            0,
            5000,
            20000,
            25000,
            40000,
            45000,
            60000,
            65000,
            80000,
            85000,
            100000,
            105000

        };

        public static double[] velocityArray = new double[] //rpm
        {
            0,
            24.420,
            24.420,
            34.536,
            34.536,
            42.298,
            42.298,
            48.841,
            48.841,
            54.606,
            54.606,
            0
        };
        public static bool[] brakeFlag = new bool[]
        {
            false
        };
    }
}