using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //This struct should provide a Transform (position and orientation) suitable for curves like Bezier in 3d space
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

        //You can read about these methods:
        //https://pomax.github.io/bezierinfo/#pointvectors3d
        //Game Programming Gems 2: The Parallel Transport Frame 

        //Just pick an "up" reference vector
        //From "Unite 2015 - A coder's guide to spline-based procedural geometry" https://www.youtube.com/watch?v=o9RK6O2kOKo
        //Is not going to work if we have loops, but should work if you make "2d" roads like in cities skylines
        public static MyQuaternion GetOrientationByUsingUpRef(MyVector3 tangent, MyVector3 upRef)
        {
            tangent = MyVector3.Normalize(tangent);
        
            MyVector3 biNormal = MyVector3.Normalize(MyVector3.Cross(upRef, tangent));

            MyVector3 normal = MyVector3.Normalize(MyVector3.Cross(tangent, biNormal));

            MyQuaternion orientation = new MyQuaternion(tangent, normal);

            return orientation;
        }

        //"Frenet normal"
        //Works in many cases (but does super bizarre things in some others
        public static MyQuaternion GetOrientationByUsingFrenetNormal(MyVector3 tangent, MyVector3 secondDerivativeVec)
        {
            MyVector3 a = MyVector3.Normalize(tangent);

            //What a next point's tangent would be if the curve stopped changing at our point and just had the same derivative and second derivative from that point on
            MyVector3 b = MyVector3.Normalize(a + secondDerivativeVec);

            //A vector that we use as the "axis of rotation" for turning the tangent a quarter circle to get the normal
            MyVector3 r = MyVector3.Normalize(MyVector3.Cross(b, a));

            //The normal vector should be perpendicular to the plane that the tangent and the axis of rotation lie in
            MyVector3 normal = MyVector3.Normalize(MyVector3.Cross(r, a));

            MyQuaternion orientation = new MyQuaternion(tangent, normal);

            return orientation;
        }

        //"Rotation Minimising Frame" (also known as "parallel transport frame" or "Bishop frame")
        //Has to be computed for the entire curve, so we can't do it for a single point on the curve
        //public static MyQuaternion GetOrientationByUsingFrame(MyVector3 tangent)
        //{
            
        //}



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
