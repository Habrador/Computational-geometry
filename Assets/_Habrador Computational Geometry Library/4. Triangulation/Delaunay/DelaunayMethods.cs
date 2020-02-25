using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Methods for all delaunay algorithms
    public static class DelaunayMethods
    {
        //Test if we should flip an edge
        //a, b, c belongs to the triangle and d is the point on the other triangle
        //a-c is the edge, which is important so we can flip it, by making the edge b-d
        public static bool ShouldFlipEdge(MyVector2 a, MyVector2 b, MyVector2 c, MyVector2 d)
        {
            bool shouldFlipEdge = false;

            //Use the circle test to test if we need to flip this edge
            //We should flip if d is inside a circle formed by a, b, c
            IntersectionCases intersectionCases = Intersections.PointCircle(a, b, c, d);

            if (intersectionCases == IntersectionCases.IsInside)
            {
                //Are these the two triangles forming a convex quadrilateral? Otherwise the edge cant be flipped
                if (Geometry.IsQuadrilateralConvex(a, b, c, d))
                {
                    //If the new triangle after a flip is not better, then dont flip
                    //This will also stop the algorithm from ending up in an endless loop
                    IntersectionCases intersectionCases2 = Intersections.PointCircle(b, c, d, a);

                    if (intersectionCases2 == IntersectionCases.IsOnEdge || intersectionCases2 == IntersectionCases.IsInside)
                    {
                        shouldFlipEdge = false;
                    }
                    else
                    {
                        shouldFlipEdge = true;
                    }
                }
            }

            return shouldFlipEdge;
        }



        //From "A fast algortihm for generating constrained delaunay..."
        //Is numerically stable
        //v1, v2 should belong to the edge we ant to flip
        //v1, v2, v3 are counter-clockwise
        //Is this also checking if the edge can be swapped
        public static bool ShouldFlipEdgeStable(MyVector2 v1, MyVector2 v2, MyVector2 v3, MyVector2 vp)
        {
            float x_13 = v1.x - v3.x;
            float x_23 = v2.x - v3.x;
            float x_1p = v1.x - vp.x;
            float x_2p = v2.x - vp.x;

            float y_13 = v1.y - v3.y;
            float y_23 = v2.y - v3.y;
            float y_1p = v1.y - vp.y;
            float y_2p = v2.y - vp.y;

            float cos_a = x_13 * x_23 + y_13 * y_23;
            float cos_b = x_2p * x_1p + y_2p * y_1p;

            if (cos_a >= 0f && cos_b >= 0f)
            {
                return false;
            }
            if (cos_a < 0f && cos_b < 0)
            {
                return true;
            }

            float sin_ab = (x_13 * y_23 - x_23 * y_13) * cos_b + (x_2p * y_1p - x_1p * y_2p) * cos_a;

            if (sin_ab < 0)
            {
                return true;
            }

            return false;
        }



        //Create a supertriangle that contains all other points
        //According to the book "Geometric tools for computer graphics" a reasonably sized triangle
        //is one that contains a circle that contains the axis-aligned bounding rectangle of the points 
        public static Triangle2 GetSupertriangle(HashSet<MyVector2> points)
        {
            //Step 1. Create a AABB around the points
            float maxX = float.MinValue;
            float minX = float.MaxValue;
            float maxY = float.MinValue;
            float minY = float.MaxValue;

            foreach (MyVector2 pos in points)
            {
                if (pos.x > maxX)
                {
                    maxX = pos.x;
                }
                else if (pos.x < minX)
                {
                    minX = pos.x;
                }

                if (pos.y > maxY)
                {
                    maxY = pos.y;
                }
                else if (pos.y < minY)
                {
                    minY = pos.y;
                }
            }

            MyVector2 TL = new MyVector2(minX, maxY);
            MyVector2 TR = new MyVector2(maxX, maxY);
            MyVector2 BR = new MyVector2(maxX, minY);

            //Debug AABB
            //Gizmos.DrawLine(TL, TR);
            //Gizmos.DrawLine(TR, BR);
            //Gizmos.DrawLine(BR, BL);
            //Gizmos.DrawLine(BL, TL);



            //Step2. Find the inscribed circle - the smallest circle that surrounds the AABB
            MyVector2 circleCenter = (TL + BR) * 0.5f;

            float circleRadius = MyVector2.Magnitude(circleCenter - TR);

            //Debug circle
            //Gizmos.DrawWireSphere(circleCenter, circleRadius);



            //Step 3. Create the smallest triangle that surrounds the circle
            //All edges of this triangle have the same length
            float halfSideLenghth = circleRadius / Mathf.Tan(30f * Mathf.Deg2Rad);

            //The center position of the bottom-edge
            MyVector2 tri_B = new MyVector2(circleCenter.x, circleCenter.y - circleRadius);

            MyVector2 tri_BL = new MyVector2(tri_B.x - halfSideLenghth, tri_B.y);
            MyVector2 tri_BR = new MyVector2(tri_B.x + halfSideLenghth, tri_B.y);

            //The height from the bottom edge to the top vertex
            float triangleHeight = halfSideLenghth * Mathf.Tan(60f * Mathf.Deg2Rad);

            MyVector2 tri_T = new MyVector2(circleCenter.x, tri_B.y + triangleHeight);

            //Debug
            //Gizmos.DrawLine(tri_BL, tri_BR);
            //Gizmos.DrawLine(tri_BL, tri_T);
            //Gizmos.DrawLine(tri_BR, tri_T);

            Triangle2 superTriangle = new Triangle2(tri_BR, tri_BL, tri_T);

            return superTriangle;
        }
    }
}
