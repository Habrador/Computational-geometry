using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class MathUtility
    {
        //The value we use to avoid floating point precision issues
        //http://sandervanrossen.blogspot.com/2009/12/realtime-csg-part-1.html
        public const float EPSILON = 0.00001f;



        //Remap value from range 1 to range 2
        public static float Remap(float value, float r1_low, float r1_high, float r2_low, float r2_high)
        {
            float remappedValue = r2_low + (value - r1_low) * ((r2_high - r2_low) / (r1_high - r1_low));

            return remappedValue;
        }



        //Clamp list indices
        //Will even work if index is larger/smaller than listSize, so can loop multiple times
        public static int ClampListIndex(int index, int listSize)
        {
            index = ((index % listSize) + listSize) % listSize;

            return index;
        }



        // Returns the determinant of the 2x2 matrix defined as
        // | x1 x2 |
        // | y1 y2 |
        public static float Det2(float x1, float x2, float y1, float y2)
        {
            return (x1 * y2 - y1 * x2);
        }



        //Calculate an angle measured in 360 degrees
        //Vector3.Angle is measured in 180 degrees
        //From should be Vector3.forward if you measure y angle, and to is the direction
        public static float CalculateAngle(Vector3 from, Vector3 to)
        {
            return Quaternion.FromToRotation(from, to).eulerAngles.y;
        }



        //Add value to average
        //http://www.bennadel.com/blog/1627-create-a-running-average-without-storing-individual-values.htm
        //count - how many values does the average consist of
        public static float AddValueToAverage(float oldAverage, float valueToAdd, float count)
        {
            float newAverage = ((oldAverage * count) + valueToAdd) / (count + 1f);

            return newAverage;
        }



        //Round a value to nearest int value determined by roundValue
        //So if roundValue is 5, we round 11 to 10 because we want to go in steps of 5
        //such as 0, 5, 10, 15
        public static int RoundValue(float value, float roundValue)
        {
            int roundedValue = (int)(Mathf.Round(value / roundValue) * roundValue);

            return roundedValue;
        }



        //Lerp exponentially between 2 values
        public static float Eerp(float a, float b, float t)
        {
            float exponentiallyLerpedValue = a * Mathf.Pow(b / a, t);

            return exponentiallyLerpedValue;
        }
    }
}
