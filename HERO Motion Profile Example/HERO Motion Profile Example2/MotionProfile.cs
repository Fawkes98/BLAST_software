//Select the green highlighted cells and paste into  a csv file.  			
//No need to copy the blank lines at the bottom.  			
//This can be pasted into an array for direct use in C++/Java.			
//       Position (rotations)	Velocity (RPM)	Duration (ms)	
namespace HERO_Motion_Profile_Example
{
    public class MotionProfile
    {
        public const uint kNumPoints = 39;
        // Time (ms),	Velocity (RPM)

        public static int[] timeArray = new int[] //ms
        {
            0,
            4000,
            14220,
            17760,
            20310,
            22050,
            22990,
            24590,
            27120,
            29260,
            32660,
            43070,
            44940,
            46500,
            60230,
            65550,
            72340,
            78400,
            83210,
            91570,
            122200,
            151210,
            159680,
            164120,
            171480,
            259370,
            277600,
            287910,
            343400,
            418570,
            475810,
            498990,
            528760,
            534190,
            539990,
            543350,
            600000,
            610000,
            800000
        };

        public static double[] velocityArray = new double[] //rpm
        {
            0.0 ,
            1.71887 ,
            1.74752 ,
            4.96277 ,
            17.22693 ,
            12.71966 ,
            14.81096 ,
            11.88887 ,
            18.56383 ,
            15.71814 ,
            17.45611 ,
            18.64023 ,
            19.99623 ,
            19.06995 ,
            21.0944 ,
            14.72502 ,
            15.86138 ,
            23.29073 ,
            24.02603 ,
            25.47752 ,
            31.66547 ,
            39.06617 ,
            42.35113 ,
            38.31178 ,
            1.73797 ,
            1.73797 ,
            5.77732 ,
            7.44845 ,
            14.1998 ,
            23.29837 ,
            32.47716 ,
            36.73614 ,
            44.0891 ,
            45.30186 ,
            1.89076 ,
            1.72842 ,
            1.72842 ,
            0.0 ,
            0,0
        };
        public static bool[] brakeFlag = new bool[]
        {
            false
        };
    }
}