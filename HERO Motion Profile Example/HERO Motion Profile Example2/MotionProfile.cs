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
            25000,
            30000,
            35000,
            1000000,
        };

        public static double[] velocityArray = new double[] //rpm
        {
            0,
            20,
            20,
            0,
            0,
            0
        };

        public static int[] brakeTime = new int[] //ms //these numbers are for the next segment of the motion profile
        {
            0,
            0,
            500,
            0,
            0, 
            0
        };

        public static short[] brakeCount = new short[] //count //these numbers are for the next segment of the motion profile
        {
            0,
            0,
            4,
            0,
            0,
            0
        };
        public static bool[] brakeFlag = new bool[]
        {
            false
        };
    }
}