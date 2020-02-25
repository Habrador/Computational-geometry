using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class TriangulateConvexHull
    {
        //If you have points on a convex hull, sorted one after each other
        //This algorithm is only working if we have no-colinear points on the hull
        public static HashSet<Triangle2> GetTrianglesNoColinearPoints(List<MyVector2> points)
        {    
            HashSet<Triangle2> triangles = new HashSet<Triangle2>();

            //This vertex will be a vertex in all triangles
            MyVector2 a = points[0];

            //And then we just loop through the other edges to make all triangles
            for (int i = 2; i < points.Count; i++)
            {
                MyVector2 b = points[i];
                MyVector2 c = points[i - 1];

                triangles.Add(new Triangle2(a, b, c));
            }

            return triangles;
        }



        //If we have colinear points, we have to give it a point inside of the hull
        public static HashSet<Triangle2> GetTrianglesColinearPoints(List<MyVector2> points, MyVector2 pointInside)
        {
            HashSet<Triangle2> triangles = new HashSet<Triangle2>();

            //This vertex will be a vertex in all triangles
            MyVector2 a = pointInside;

            //And then we just loop through the other edges to make all triangles
            for (int i = 0; i < points.Count; i++)
            {
                MyVector2 b = points[i];
                MyVector2 c = points[MathUtility.ClampListIndex(i + 1, points.Count)];
                
                triangles.Add(new Triangle2(a, b, c));
            }

            return triangles;
        }
    }
}
