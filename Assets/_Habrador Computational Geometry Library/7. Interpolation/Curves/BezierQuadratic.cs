using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A collection of classes to make the methods more general
namespace Habrador_Computational_Geometry
{
    //Bezier with one handle
    public class BezierQuadratic : _Curve
    {
        //Start and end point
        public MyVector3 posA;
        public MyVector3 posB;
        //Handle connected to start and end points
        public MyVector3 handle;


        public BezierQuadratic(MyVector3 posA, MyVector3 posB, MyVector3 handle)
        {
            this.posA = posA;
            this.posB = posB;
            
            this.handle = handle;
        }


        //
        // Position and forward dir
        //

        //Get interpolated position at point t
        public override MyVector3 GetPosition(float t)
        {
            MyVector3 interpolatedValue = GetPosition(posA, posB, handle, t);

            return interpolatedValue;
        }

        public static MyVector3 GetPosition(MyVector3 posA, MyVector3 posB, MyVector3 handlePos, float t)
        {
            MyVector3 interpolation_posA_handlePos = BezierLinear.GetPosition(posA, handlePos, t);
            MyVector3 interpolation_handlePos_posB = BezierLinear.GetPosition(handlePos, posB, t);

            MyVector3 finalInterpolation = BezierLinear.GetPosition(interpolation_posA_handlePos, interpolation_handlePos_posB, t);

            return finalInterpolation;
        }


        //Get the forward direction at a point on the Bezier Quadratic
        //This direction is always tangent to the curve
        public static MyVector3 GetForwardDir(MyVector3 posA, MyVector3 posB, MyVector3 handlePos, float t)
        {
            //Same as when we calculate t
            MyVector3 interpolation_posA_handlePos = BezierLinear.GetPosition(posA, handlePos, t);
            MyVector3 interpolation_handlePos_posB = BezierLinear.GetPosition(handlePos, posB, t);

            MyVector3 forwardDir = MyVector3.Normalize(interpolation_handlePos_posB - interpolation_posA_handlePos);

            return forwardDir;
        }



        //
        // Derivative
        //

        public override float CalculateDerivative(float t)
        {
            //Choose how to calculate the derivative
            //float derivative = InterpolationHelpMethods.EstimateDerivative(this, t);

            float derivative = ExactDerivative(t);

            return derivative;
        }



        //Derivative at point t
        public float ExactDerivative(float t)
        {
            MyVector3 A = posA;
            MyVector3 B = handle;
            MyVector3 C = posB;

            //Layer 1
            //(1-t)A + tB = A - At + Bt
            //(1-t)B + tC = B - Bt + Ct

            //Layer 2
            //(1-t)(A - At + Bt) + t(B - Bt + Ct)
            //A - At + Bt - At + At^2 - Bt^2 + Bt - Bt^2 + Ct^2
            //A - 2At + 2Bt + At^2 - 2Bt^2 + Ct^2 
            //A - t(2(A - B)) + t^2(A - 2B + C)

            //Derivative: -(2(A - B)) + t(2(A - 2B + C))

            MyVector3 derivativeVector = t * (2f * (A - 2f * B + C));

            derivativeVector += -2f * (A - B);


            float derivative = MyVector3.Magnitude(derivativeVector);


            return derivative;
        }
    }
}
