using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Just a triangle with three corners
    public struct Triangle
    {
        //Corners
        public Vector3 p1;
        public Vector3 p2;
        public Vector3 p3;

        public Triangle(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        //Change orientation of triangle from cw -> ccw or ccw -> cw
        public void ChangeOrientation()
        {
            Vector3 temp = this.p1;

            this.p1 = this.p2;

            this.p2 = temp;
        }
    }

    //2d space
    public struct Triangle2D
    {
        //Corners
        public Vector2 p1;
        public Vector2 p2;
        public Vector2 p3;

        public Triangle2D(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        //Change orientation of triangle from cw -> ccw or ccw -> cw
        public void ChangeOrientation()
        {
            Vector2 temp = this.p1;

            this.p1 = this.p2;

            this.p2 = temp;
        }
    }
}
