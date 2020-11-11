using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //This struct should provide a Transform (position and orientation) suitable for curves like Bezier
    public struct InterpolationTransform
    {
        public MyVector3 position;
        
        public MyQuaternion orientation;

        public InterpolationTransform(MyVector3 position, MyQuaternion orientation)
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
        public static MyQuaternion GetOrientationByUsingUpRef(MyVector3 tangent, MyVector3 upRef)
        {
            MyVector3 biNormal = MyVector3.Normalize(MyVector3.Cross(upRef, tangent));

            MyVector3 normal = MyVector3.Normalize(MyVector3.Cross(tangent, biNormal));

            //Quaternion orientation = Quaternion.LookRotation(tangent.ToVector3(), normal.ToVector3());

            MyQuaternion orientation = new MyQuaternion(tangent, normal);

            return orientation;
        }



        //
        // Get directions from orientation
        //

        //Forward
        public MyVector3 Forward => orientation.Forward;
        public MyVector3 Right   => orientation.Right;
        public MyVector3 Up      => orientation.Up;

        //public MyVector3 Forward
        //{
        //    get
        //    {
        //        return orientation.Forward;
        //    }
        //}

        //Right
        //public MyVector3 Right
        //{
        //    get
        //    {
        //        return orientation.Right;
        //    }
        //}

        ////Up
        //public MyVector3 Up
        //{
        //    get
        //    {
        //        return orientation.Up;
        //    }
        //}
    }
}
