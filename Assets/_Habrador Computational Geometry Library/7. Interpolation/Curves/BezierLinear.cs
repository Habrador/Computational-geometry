using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A collection of classes to make the methods more general
namespace Habrador_Computational_Geometry
{
    //Bezier with zero handles
    public class BezierLinear : _Curve
    {
        //Start and end point
        public MyVector3 posA;
        public MyVector3 posB;

        public BezierLinear(MyVector3 posA, MyVector3 posB)
        {
            this.posA = posA;
            this.posB = posB;
        }



        //
        // Position
        //

        public override MyVector3 GetPosition(float t)
        {
            MyVector3 interpolatedPos = GetPosition(posA, posB, t);

            return interpolatedPos;
        }

        //Linear bezier - straight line
        //3d
        public static MyVector3 GetPosition(MyVector3 a, MyVector3 b, float t)
        {
            t = Mathf.Clamp01(t);

            //float lerpX = _Interpolation.Lerp(a.x, b.x, t);
            //float lerpY = _Interpolation.Lerp(a.y, b.y, t);
            //float lerpZ = _Interpolation.Lerp(a.z, b.z, t);

            //MyVector3 interpolatedPos = new MyVector3(lerpX, lerpY, lerpZ);

            //Above is same as
            //(1-t)A + tB = A - At + Bt

            MyVector3 interpolatedPos = a - a * t + b * t;

            return interpolatedPos;
        }

        //2d
        public static MyVector2 GetPosition(MyVector2 a, MyVector2 b, float t)
        {
            t = Mathf.Clamp01(t);

            //float lerpX = _Interpolation.Lerp(a.x, b.x, t);
            //float lerpY = _Interpolation.Lerp(a.y, b.y, t);

            //MyVector2 interpolatedPos = new MyVector2(lerpX, lerpY);

            //Above is same as
            //(1-t)A + tB = A - At + Bt

            MyVector2 interpolatedPos = a - a * t + b * t;

            return interpolatedPos;
        }



        //
        // Derivative
        //

        public override float GetDerivative(float t)
        {
            MyVector3 derivativeVec = GetDerivativeVec(posA, posB);

            float derivative = MyVector3.Magnitude(derivativeVec);

            return derivative;
        }

        public static MyVector3 GetDerivativeVec(MyVector3 posA, MyVector3 posB)
        {
            //Pos: A - At + Bt
            //Derivative: -A + B

            MyVector3 derivativeVec = -posA + posB;

            return derivativeVec;
        }

        public override MyVector3 GetSecondDerivativeVec(float t)
        {
            throw new System.NotImplementedException();
        }


        //
        // Tangent
        //

        public override MyVector3 GetTangent(float t)
        {
            throw new System.NotImplementedException();
        }

        
    }
}
