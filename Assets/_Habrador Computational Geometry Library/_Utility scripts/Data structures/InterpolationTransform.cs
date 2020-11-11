using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //This struct should provide a Transform (position and orientation) suitable for curves like Bezier
    public struct InterpolationTransform
    {
        public MyVector3 position;
        //Will be messy because position is using MyVector3 and orientation is using Vector3
        //But it's easier than writing a custom Quaternion class
        public Quaternion orientation;

        public InterpolationTransform(MyVector3 position, Quaternion orientation)
        {
            this.position = position;
            this.orientation = orientation;
        }



        //
        // Calculate orientation by using different methods
        //

        //If we have a forward (tangent) and an "up" reference vector
        //So this is not going to work if we have loops
        //From "Unite 2015 - A coder's guide to spline-based procedural geometry" https://www.youtube.com/watch?v=o9RK6O2kOKo
        public static Quaternion GetOrientationByUsingUpRef(MyVector3 tangent, MyVector3 upRef)
        {
            MyVector3 biNormal = MyVector3.Normalize(MyVector3.Cross(upRef, tangent));

            MyVector3 normal = MyVector3.Normalize(MyVector3.Cross(tangent, biNormal));

            Quaternion orientation = Quaternion.LookRotation(tangent.ToVector3(), normal.ToVector3());

            return orientation;
        }



        //
        // Get directions from orientation
        //

        //Forward
        public MyVector3 Forward
        {
            get
            {
                //Multiply the orientation with a direction vector to rotate the direction
                Vector3 forwardDir = orientation * Vector3.forward;

                return forwardDir.ToMyVector3();
            }
        }

        //Right
        public MyVector3 Right
        {
            get
            {
                //Multiply the orientation with a direction vector to rotate the direction
                Vector3 rightDir = orientation * Vector3.right;

                return rightDir.ToMyVector3();
            }
        }

        //Up
        public MyVector3 Up
        {
            get
            {
                //Multiply the orientation with a direction vector to rotate the direction
                Vector3 upDir = orientation * Vector3.up;

                return upDir.ToMyVector3();
            }
        }
    }
}
