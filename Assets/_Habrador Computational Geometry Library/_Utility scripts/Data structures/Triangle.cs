using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //3d space
    public struct Triangle3
    {
        //Corners
        public MyVector3 p1;
        public MyVector3 p2;
        public MyVector3 p3;

        public Triangle3(MyVector3 p1, MyVector3 p2, MyVector3 p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        //Change orientation of triangle from cw -> ccw or ccw -> cw
        public void ChangeOrientation()
        {
            MyVector3 temp = this.p1;

            this.p1 = this.p2;

            this.p2 = temp;
        }
    }



    //2d space
    public struct Triangle2
    {
        //Corners
        public MyVector2 p1;
        public MyVector2 p2;
        public MyVector2 p3;

        public Triangle2(MyVector2 p1, MyVector2 p2, MyVector2 p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        //Change orientation of triangle from cw -> ccw or ccw -> cw
        public void ChangeOrientation()
        {
            MyVector2 temp = this.p1;

            this.p1 = this.p2;

            this.p2 = temp;
        }

        //Find the max and min coordinates, which is useful when doing AABB intersections
        public float MinX()
        {
            return Mathf.Min(p1.x, Mathf.Min(p2.x, p3.x));
        }

        public float MaxX()
        {
            return Mathf.Max(p1.x, Mathf.Max(p2.x, p3.x));
        }

        public float MinY()
        {
            return Mathf.Min(p1.y, Mathf.Min(p2.y, p3.y));
        }

        public float MaxY()
        {
            return Mathf.Max(p1.y, Mathf.Max(p2.y, p3.y));
        }
    }
}
