using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Triangulate random points by first generating the convex hull of the points, then triangulate the convex hull
    //and then add the other points one-by-one by splitting the triangle the point is in
    public static class TriangleSplittingAlgorithm
    {
        public static HashSet<Triangle2> TriangulatePoints(HashSet<MyVector2> points)
        {
            //Step 1. Generate the convex hull - will also remove the points from points list which are not on the hull
            List<MyVector2> pointsOnConvexHull = _ConvexHull.JarvisMarch(points);


            //Step 2. Triangulate the convex hull
            HashSet<Triangle2> triangles = _TriangulatePoints.TriangulateConvexHullAlgorithm(pointsOnConvexHull);


            //Step 3. From the points we should add, remove those that are already a part of the triangulation
            foreach (Triangle2 t in triangles)
            {
                points.Remove(t.p1);
                points.Remove(t.p2);
                points.Remove(t.p3);
            }


            //Step 4. Add the remaining points while splitting the triangles they end up in
            foreach (MyVector2 currentPoint in points)
            {
                //Which triangle is this point in?
                foreach (Triangle2 t in triangles)
                {
                    if (Intersections.PointTriangle(t, currentPoint, true))
                    {
                        //Split the triangle into three new triangles

                        //Create 3 new  with correct orientation = clockwise
                        Triangle2 t1 = new Triangle2(t.p1, t.p2, currentPoint);
                        Triangle2 t2 = new Triangle2(t.p2, t.p3, currentPoint);
                        Triangle2 t3 = new Triangle2(t.p3, t.p1, currentPoint);

                        //Remove the old triangle
                        triangles.Remove(t);

                        //Add the new triangles
                        triangles.Add(t1);
                        triangles.Add(t2);
                        triangles.Add(t3);

                        break;
                    }
                }
            }

       

            return triangles;
        }
    }
}
