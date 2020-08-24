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
        public Color32 c1, c2, c3;

        public Triangle3(MyVector3 p1, MyVector3 p2, MyVector3 p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
            c1 = c2 = c3 = Color.white;
        }
        public Triangle3(MyVector3 p1, MyVector3 p2, MyVector3 p3, Color32 c1, Color32 c2, Color32 c3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
            this.c1 = c1;
            this.c2 = c2;
            this.c3 = c3;
        }

        //Change orientation of triangle from cw -> ccw or ccw -> cw
        public void ChangeOrientation()
        {
            //Swap two vertices
            (p1, p2) = (p2, p1);
        }
    }



    //2d space
    public struct Triangle2
    {
        //Corners
        public MyVector2 p1
        {
            get;
            private set;
        }
        public MyVector2 p2
        {
            get;
            private set;
        }
        public MyVector2 p3
        {
            get;
            private set;
        }

        public Color32 c1, c2, c3;

        public Triangle2(MyVector2 p1, MyVector2 p2, MyVector2 p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
            c1 = c2 = c3 = Color.white;
        }
        public Triangle2(MyVector2 p1, MyVector2 p2, MyVector2 p3, Color32 c1, Color32 c2, Color32 c3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
            this.c1 = c1;
            this.c2 = c2;
            this.c3 = c3;   
        }

        //Change orientation of triangle from cw -> ccw or ccw -> cw
        public void ChangeOrientation()
        {
            //Swap two vertices
            (p1, p2) = (p2, p1);
            (c1, c2) = (c2, c1);
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
