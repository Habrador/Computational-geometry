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
        public MyVector3 handlePos;


        public BezierQuadratic(MyVector3 posA, MyVector3 posB, MyVector3 handlePos)
        {
            this.posA = posA;
            this.posB = posB;
            
            this.handlePos = handlePos;
        }


        //
        // Position at point t
        //

        public override MyVector3 GetPosition(float t)
        {
            MyVector3 interpolatedValue = GetPosition(posA, posB, handlePos, t);

            return interpolatedValue;
        }

        //Uses de Casteljau's algorithm 
        public static MyVector3 GetPosition(MyVector3 posA, MyVector3 posB, MyVector3 handlePos, float t)
        {
            //MyVector3 interpolation_posA_handlePos = BezierLinear.GetPosition(posA, handlePos, t);
            //MyVector3 interpolation_handlePos_posB = BezierLinear.GetPosition(handlePos, posB, t);

            //MyVector3 finalInterpolation = BezierLinear.GetPosition(interpolation_posA_handlePos, interpolation_handlePos_posB, t);

            //Above can be simplified by putting it into one big equation
            //Layer 1
            //(1-t)A + tB = A - At + Bt
            //(1-t)B + tC = B - Bt + Ct

            //Layer 2
            //(1-t)(A - At + Bt) + t(B - Bt + Ct)
            //A - At + Bt - At + At^2 - Bt^2 + Bt - Bt^2 + Ct^2
            //A - 2At + 2Bt + At^2 - 2Bt^2 + Ct^2 
            //A - t(2(A - B)) + t^2(A - 2B + C)

            MyVector3 A = posA;
            MyVector3 B = handlePos;
            MyVector3 C = posB;

            MyVector3 finalInterpolation = A;

            finalInterpolation += -t * (2f * (A - B));

            //t^2 -> quadratic 
            finalInterpolation += Mathf.Pow(t, 2f) * (A - 2f * B + C);

            return finalInterpolation;
        }



        //
        // Forward direction (tangent) at point t
        //

        public static MyVector3 GetForwardDir(MyVector3 posA, MyVector3 posB, MyVector3 handlePos, float t)
        {
            //Alternative 1
            //Same as when we calculate position from t
            //MyVector3 interpolation_posA_handlePos = BezierLinear.GetPosition(posA, handlePos, t);
            //MyVector3 interpolation_handlePos_posB = BezierLinear.GetPosition(handlePos, posB, t);

            //MyVector3 forwardDir = MyVector3.Normalize(interpolation_handlePos_posB - interpolation_posA_handlePos);

            //Alternative 2
            //The forward dir is also the derivative vector
            MyVector3 forwardDir = MyVector3.Normalize(DerivativeVec(posA, posB, handlePos, t));

            return forwardDir;
        }

        public MyVector3 GetForwardDir(float t)
        {
            MyVector3 forwardDir = GetForwardDir(posA, posB, handlePos, t);

            return forwardDir;
        }



        //
        // Derivative at point t
        //

        public override float GetDerivative(float t)
        {
            //Alternative 1. Estimated
            //float derivative = InterpolationHelpMethods.EstimateDerivative(this, t);

            //Alternative 2. Exact
            MyVector3 derivativeVec = DerivativeVec(posA, posB, handlePos, t);

            float derivative = MyVector3.Magnitude(derivativeVec);

            return derivative;
        }

        public static MyVector3 DerivativeVec(MyVector3 posA, MyVector3 posB, MyVector3 handlePos, float t)
        {
            MyVector3 A = posA;
            MyVector3 B = handlePos;
            MyVector3 C = posB;

            //The derivative of the equation we use when finding position along the curve at t: 
            //-(2(A - B)) + t(2(A - 2B + C))

            MyVector3 derivativeVector = -(2f * (A - B));

            derivativeVector += t * (2f * (A - 2f * B + C));

            return derivativeVector;
        }
    }
}
