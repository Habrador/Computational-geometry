using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Interpolation between values with different algorithms
    public static class _Interpolation
    {  
        //Linear interpolation - if t is constant then the step between each value is constant
        public static float Lerp(float a, float b, float t)
        {
            t = Mathf.Clamp01(t);
        
            //Same as Mathf.Lerp(a, b, t);
            float interpolatedValue = (1f - t) * a + t * b;

            return interpolatedValue;
        }

        //3d
        public static MyVector3 Lerp(MyVector3 a, MyVector3 b, float t)
        {
            t = Mathf.Clamp01(t);

            //Same as Mathf.Lerp(a, b, t);
            MyVector3 interpolatedValue = (1f - t) * a + t * b;

            return interpolatedValue;
        }


        //Ease out interpolation - the values get smaller and smaller
        //https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
        public static float Sinerp(float a, float b, float t)
        {
            t = Mathf.Clamp01(t);

            float newT = Mathf.Sin(t * Mathf.PI * 0.5f);

            float interpolatedValue = Lerp(a, b, newT);

            return interpolatedValue;
        }


        //Ease in interpolation - the values get larger and larger
        //https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
        public static float Coserp(float a, float b, float t)
        {
            t = Mathf.Clamp01(t);

            float newT = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);

            float interpolatedValue = Lerp(a, b, newT);

            return interpolatedValue;
        }


        //Exponential 
        //https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
        public static float Eerp(float a, float b, float t)
        {
            t = Mathf.Clamp01(t);

            float newT = t * t;

            float interpolatedValue = Lerp(a, b, newT);

            return interpolatedValue;
        }


        //Smoothstep - values get smaller and smaller in the beginning AND in the end
        //https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
        public static float Smoothsteperp(float a, float b, float t)
        {
            t = Mathf.Clamp01(t);

            float newT = t * t * (3f - 2f * t);

            float interpolatedValue = Lerp(a, b, newT);

            return interpolatedValue;
        }


        //Smootherstep - values get smaller and smaller in the beginning AND in the end but a little smoother than the above
        //https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
        public static float Smoothersteperp(float a, float b, float t)
        {
            t = Mathf.Clamp01(t);

            float newT = t = t * t * t * (t * (6f * t - 15f) + 10f);

            float interpolatedValue = Lerp(a, b, newT);

            return interpolatedValue;
        }


        //If you want to have more control of the curve you can use the idea from Quadtratic Bezier, but in just one dimension
        //handleA and handleB are the values that control the shape of the curve
        //https://www.youtube.com/watch?v=S2fz4BS2J3Y
        public static float CubicBezierErp(float valueA, float valueB, float handleA, float handleB, float t)
        {
            float a = Lerp(valueA, handleA, t);
            float b = Lerp(handleA, handleB, t);
            float c = Lerp(handleB, valueB, t);

            float a_b = Lerp(a, b, t);
            float b_c = Lerp(b, c, t);

            float interpolatedValue = Lerp(a_b, b_c, t);

            return interpolatedValue;
        }



        //Exponential interpolation between 2 values
        //Which breaks at a = 0, so better to use something else
        //From https://twitter.com/freyaholmer/status/1068293398073929728
        //public static float Eerp(float a, float b, float t)
        //{
        //    float interpolatedValue = a * Mathf.Pow(b / a, t);

        //    return interpolatedValue;
        //}
    }
}
