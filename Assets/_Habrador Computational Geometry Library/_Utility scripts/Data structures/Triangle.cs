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
            //Swap two vertices
            (p1, p2) = (p2, p1);
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
            //Swap two vertices
            (p1, p2) = (p2, p1);
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


        //Find the opposite edge to a vertex
        public Edge2 FindOppositeEdgeToVertex(MyVector2 p)
        {
            if (p.Equals(p1))
            {
                return new Edge2(p2, p3);
            }
            else if (p.Equals(p2))
            {
                return new Edge2(p3, p1);
            }
            else
            {
                return new Edge2(p1, p2);
            }
        }


        //Check if an edge is a part of this triangle
        public bool IsEdgePartOfTriangle(Edge2 e)
        {
            if ((e.p1.Equals(p1) && e.p2.Equals(p2)) || (e.p1.Equals(p2) && e.p2.Equals(p1)))
            {
                return true;
            }
            if ((e.p1.Equals(p2) && e.p2.Equals(p3)) || (e.p1.Equals(p3) && e.p2.Equals(p2)))
            {
                return true;
            }
            if ((e.p1.Equals(p3) && e.p2.Equals(p1)) || (e.p1.Equals(p1) && e.p2.Equals(p3)))
            {
                return true;
            }

            return false;
        }


        //Find the vertex which is not an edge
        public MyVector2 GetVertexWhichIsNotPartOfEdge(Edge2 e)
        {
            if (!p1.Equals(e.p1) && !p1.Equals(e.p2))
            {
                return p1;
            }
            if (!p2.Equals(e.p1) && !p2.Equals(e.p2))
            {
                return p2;
            }

            return p3;
        }
    }
}
