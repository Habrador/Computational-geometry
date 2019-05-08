using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class TriangulateConvexHull
    {
        //If you have points on a convex hull, sorted one after each other
        public static HashSet<Triangle> GetTriangles(List<Vector3> points)
        {    
            //Triangulate the convex hull
            HashSet<Triangle> triangles = new HashSet<Triangle>();

            //This vertex is always a part of each triangler
            Vector3 a = points[0];

            for (int i = 2; i < points.Count; i++)
            {
                Vector3 b = points[i];
                Vector3 c = points[i - 1];

                triangles.Add(new Triangle(a, b, c));
            }

            return triangles;
        }
    }
}
