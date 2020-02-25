using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class TriangulateConvexHull
    {
        //If you have points on a convex hull, sorted one after each other
        public static HashSet<Triangle2> GetTriangles(List<MyVector2> points)
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
    }
}
