using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Interpolation between points with different algorithms
    //These are in 3d, if you want them in 2d just set a coordinate to 0
    public static class _Interpolation
    {
        //
        // Bezier Curves
        //

        //https://en.wikipedia.org/wiki/B%C3%A9zier_curve

        //Linear bezier - straight line
        public static MyVector3 BezierLinear(MyVector3 a, MyVector3 b, float t)
        {
            float lerpX = Lerp(a.x, b.x, t);
            float lerpY = Lerp(a.y, b.y, t);
            float lerpZ = Lerp(a.z, b.z, t);

            MyVector3 interpolatedPos = new MyVector3(lerpX, lerpY, lerpZ);

            return interpolatedPos;
        }


        //Quadratic bezier - one handle
        public static MyVector3 BezierQuadratic(MyVector3 posA, MyVector3 posB, MyVector3 handlePos, float t)
        {
            MyVector3 interpolation_posA_handlePos = BezierLinear(posA, handlePos, t);
            MyVector3 interpolation_handlePos_posB = BezierLinear(handlePos, posB, t);

            MyVector3 finalInterpolation = BezierLinear(interpolation_posA_handlePos, interpolation_handlePos_posB, t);

            return finalInterpolation;
        }

        //Get the forward direction do the Bezier Quadratic
        //This direction is always tangent to the curve
        public static MyVector3 BezierQuadraticForwardDir(MyVector3 posA, MyVector3 posB, MyVector3 handlePos, float t)
        {
            //Same as when we calculate t
            MyVector3 interpolation_posA_handlePos = BezierLinear(posA, handlePos, t);
            MyVector3 interpolation_handlePos_posB = BezierLinear(handlePos, posB, t);

            MyVector3 forwardDir = MyVector3.Normalize(interpolation_handlePos_posB - interpolation_posA_handlePos);

            return forwardDir;
        }


        //Cubic bezier - two handles
        public static MyVector3 BezierCubic(MyVector3 posA, MyVector3 posB, MyVector3 handlePosA, MyVector3 handlePosB, float t)
        {
            //MyVector3 interpolation_1 = BezierLinear(posA, handlePosA, t);
            //MyVector3 interpolation_2 = BezierLinear(handlePosA, handlePosB, t);
            //MyVector3 interpolation_3 = BezierLinear(handlePosB, posB, t);

            //MyVector3 interpolation_1_2 = BezierLinear(interpolation_1, interpolation_2, t);
            //MyVector3 interpolation_2_3 = BezierLinear(interpolation_2, interpolation_3, t);

            //Above can be simplified if we are utilizing the quadratic bezier
            MyVector3 interpolation_1_2 = BezierQuadratic(posA, handlePosB, handlePosA, t);
            MyVector3 interpolation_2_3 = BezierQuadratic(handlePosA, posB, handlePosB, t);

            MyVector3 finalInterpolation = BezierLinear(interpolation_1_2, interpolation_2_3, t);

            return finalInterpolation;
        }

        //Get the position and the orientation at t
        public static InterpolationTransform BezierCubicTransform(MyVector3 posA, MyVector3 posB, MyVector3 handlePosA, MyVector3 handlePosB, float t)
        {
            //Same as when we calculate t
            MyVector3 interpolation_1_2 = BezierQuadratic(posA, handlePosB, handlePosA, t);
            MyVector3 interpolation_2_3 = BezierQuadratic(handlePosA, posB, handlePosB, t);

            MyVector3 finalInterpolation = BezierLinear(interpolation_1_2, interpolation_2_3, t);

            //This direction is always tangent to the curve
            MyVector3 forwardDir = MyVector3.Normalize(interpolation_2_3 - interpolation_1_2);

            //A simple way to get the other directions is to use LookRotation with just forward dir as parameter
            //Then the up direction will always be the world up direction, and it calculates the right direction 
            Quaternion orientation = Quaternion.LookRotation(forwardDir.ToVector3());


            InterpolationTransform trans = new InterpolationTransform(finalInterpolation, orientation);

            return trans;
        }



        //
        // Catmull-Rom curve
        //

        //The curve has a start and end point (p1 and p2), and the shape is determined also by two handle points (p0 and p3)
        //The difference from the bezier case is that you can make it so the curve is going through these handle points
        //So if you have a set of points and want a smooth path between these points, you don't have to bother with handles
        //to determine the shape of the curve
        //http://www.iquilezles.org/www/articles/minispline/minispline.htm
        public static MyVector3 CatmullRom(MyVector3 p0, MyVector3 p1, MyVector3 p2, MyVector3 p3, float t)
        {
            //The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
            MyVector3 a = 2f * p1;
            MyVector3 b = p2 - p0;
            MyVector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
            MyVector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

            //The cubic polynomial: a + b * t + c * t^2 + d * t^3
            MyVector3 interpolatedPos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

            return interpolatedPos;
        }



        //
        // Interpolate between values
        //

        //Linear interpolation between 2 values
        public static float Lerp(float a, float b, float t)
        {
            //Same as Mathf.Lerp(a, b, t);
            float interpolatedValue = (1f - t) * a + t * b;

            return interpolatedValue;
        }



        //Exponential interpolation between 2 values
        //Which breaks at a = 0, so better to feed a "exponential t" to Lerp
        //From https://twitter.com/freyaholmer/status/1068293398073929728
        //public static float Eerp(float a, float b, float t)
        //{
        //    float interpolatedValue = a * Mathf.Pow(b / a, t);

        //    return interpolatedValue;
        //}
    }
}
