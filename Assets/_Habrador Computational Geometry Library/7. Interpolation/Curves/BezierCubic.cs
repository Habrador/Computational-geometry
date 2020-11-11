using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A collection of classes to make the methods more general
namespace Habrador_Computational_Geometry
{
    //Bezier with two handles
    public class BezierCubic : _Curve
    {
        //Start and end points
        public MyVector3 posA;
        public MyVector3 posB;
        //Handles connected to the start and end points
        public MyVector3 handleA;
        public MyVector3 handleB;


        public BezierCubic(MyVector3 posA, MyVector3 posB, MyVector3 handleA, MyVector3 handleB)
        {
            this.posA = posA;
            this.posB = posB;

            this.handleA = handleA;
            this.handleB = handleB;
        }


        //
        // Position and forward dir
        //

        //Get interpolated position on the curve at point t
        public override MyVector3 GetPosition(float t)
        {
            MyVector3 interpolatedValue = GetPosition(posA, posB, handleA, handleB, t);

            return interpolatedValue;
        }

        public static MyVector3 GetPosition(MyVector3 posA, MyVector3 posB, MyVector3 handlePosA, MyVector3 handlePosB, float t)
        {
            //MyVector3 interpolation_1 = BezierLinear(posA, handlePosA, t);
            //MyVector3 interpolation_2 = BezierLinear(handlePosA, handlePosB, t);
            //MyVector3 interpolation_3 = BezierLinear(handlePosB, posB, t);

            //MyVector3 interpolation_1_2 = BezierLinear(interpolation_1, interpolation_2, t);
            //MyVector3 interpolation_2_3 = BezierLinear(interpolation_2, interpolation_3, t);

            //Above can be simplified if we are utilizing the quadratic bezier
            MyVector3 interpolation_1_2 = BezierQuadratic.GetPosition(posA, handlePosB, handlePosA, t);
            MyVector3 interpolation_2_3 = BezierQuadratic.GetPosition(handlePosA, posB, handlePosB, t);

            MyVector3 finalInterpolation = BezierLinear.GetPosition(interpolation_1_2, interpolation_2_3, t);

            return finalInterpolation;
        }


        //Get the forward direction at a point on the Bezier Cubic
        //This direction is always tangent to the curve
        public static MyVector3 GetForwardDir(MyVector3 posA, MyVector3 posB, MyVector3 handlePosA, MyVector3 handlePosB, float t)
        {
            //Same as when we calculate t
            MyVector3 interpolation_1_2 = BezierQuadratic.GetPosition(posA, handlePosB, handlePosA, t);
            MyVector3 interpolation_2_3 = BezierQuadratic.GetPosition(handlePosA, posB, handlePosB, t);

            MyVector3 forwardDir = MyVector3.Normalize(interpolation_2_3 - interpolation_1_2);

            return forwardDir;
        }



        ////Get interpolated tangent at point t
        //public MyVector3 GetInterpolatedTangent(float t)
        //{
        //    //Same as when we calculate t
        //    MyVector3 interpolation_1_2 = _Interpolation.BezierQuadratic(posA, handleB, handleA, t);
        //    MyVector3 interpolation_2_3 = _Interpolation.BezierQuadratic(posA, posB, handleB, t);

        //    //This direction is always tangent to the curve
        //    MyVector3 forwardDir = MyVector3.Normalize(interpolation_2_3 - interpolation_1_2);

        //    return forwardDir;
        //}



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



        //Actual derivative at point t
        public float ExactDerivative(float t)
        {
            MyVector3 A = posA;
            MyVector3 B = handleA;
            MyVector3 C = handleB;
            MyVector3 D = posB;

            //Layer 1
            //(1-t)A + tB = A - At + Bt
            //(1-t)B + tC = B - Bt + Ct
            //(1-t)C + tD = C - Ct + Dt

            //Layer 2
            //(1-t)(A - At + Bt) + t(B - Bt + Ct) = A - At + Bt - At + At^2 - Bt^2 + Bt - Bt^2 + Ct^2 = A - 2At + 2Bt + At^2 - 2Bt^2 + Ct^2 
            //(1-t)(B - Bt + Ct) + t(C - Ct + Dt) = B - Bt + Ct - Bt + Bt^2 - Ct^2 + Ct - Ct^2 + Dt^2 = B - 2Bt + 2Ct + Bt^2 - 2Ct^2 + Dt^2

            //Layer 3
            //(1-t)(A - 2At + 2Bt + At^2 - 2Bt^2 + Ct^2) + t(B - 2Bt + 2Ct + Bt^2 - 2Ct^2 + Dt^2)
            //A - 2At + 2Bt + At^2 - 2Bt^2 + Ct^2 - At + 2At^2 - 2Bt^2 - At^3 + 2Bt^3 - Ct^3 + Bt - 2Bt^2 + 2Ct^2 + Bt^3 - 2Ct^3 + Dt^3
            //A - 3At + 3Bt + 3At^2 - 6Bt^2 + 3Ct^2 - At^3 + 3Bt^3 - 3Ct^3 + Dt^3
            //A - 3t(A - B) + t^2(3A - 6B + 3C) + t^3(-A + 3B - 3C + D)
            //A - 3t(A - B) + t^2(3(A - 2B + C)) + t^3(-(A - 3(B - C) - D)

            //The derivative: -3(A - B) + 2t(3(A - 2B + C)) + 3t^2(-(A - 3(B - C) - D)
            //-3(A - B) + t(6(A - 2B + C)) + t^2(-3(A - 3(B - C) - D)

            MyVector3 derivativeVector = t * t * (-3f * (A - 3f * (B - C) - D));

            derivativeVector += t * (6f * (A - 2f * B + C));

            derivativeVector += -3f * (A - B);


            float derivative = MyVector3.Magnitude(derivativeVector);


            return derivative;
        }



        //
        // Get a Transform (includes position and orientation) at point t
        //
        public InterpolationTransform GetTransform(float t)
        {
            //The position on the curve at point t
            MyVector3 pos = GetPosition(t);

            //This forward direction (tangent) on the curve at point t
            MyVector3 forwardDir = GetForwardDir(posA, posB, handleA, handleB, t);


            //The position and the tangent are easy to find, what's difficult to find is the normal because a line doesn't have a single normal

            //To get the normal in 2d, we can just flip two coordinates in the forward vector
            //MyVector3 tangent = new MyVector3(-forwardDir.z, 0f, forwardDir.x);


            //In 3d there are multiple alternatives

            //Alternative 1

            //A simple way to get the other directions is to use LookRotation with just forward dir as parameter
            //Then the up direction will always be the world up direction, and it calculates the right direction 
            //This idea is not working for all possible curve orientations
            Quaternion orientation = Quaternion.LookRotation(forwardDir.ToVector3());

            //This is the same as providing a reference vector which is up
            //Quaternion orientation = InterpolationTransform.GetOrientationByUsingUpRef(forwardDir, Vector3.up.ToMyVector3());


            InterpolationTransform trans = new InterpolationTransform(pos, orientation);

            return trans;
        }
    }
}
