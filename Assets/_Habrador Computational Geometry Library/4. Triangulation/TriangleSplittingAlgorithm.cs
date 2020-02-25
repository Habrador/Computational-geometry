using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Triangulate random points by: 
    //1. Generating the convex hull of the points
    //2. Triangulate the convex hull
    //3. Add the other points one-by-one while splitting the triangle the point is in
    public static class TriangleSplittingAlgorithm
    {
        public static HashSet<Triangle2> TriangulatePoints(HashSet<MyVector2> points)
        {
            //Step 1. Generate the convex hull - will also remove the points from points list which are not on the hull
            List<MyVector2> pointsOnConvexHull = _ConvexHull.JarvisMarch(points);


            //Step 2. Triangulate the convex hull
            HashSet<Triangle2> triangles = _TriangulatePoints.PointsOnConvexHull(pointsOnConvexHull);


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
                    if (Intersections.PointTriangle(t, currentPoint, includeBorder: true))
                    {
                        //Split the triangle into three new triangles
                        //We ignore if it ends up on the edge of a triangle
                        //If that happens we should split the edge
                        //But it will most likely not end up exactly on the edge because of floating point precision issues
                        //And we are most likely going to run a Delaunay algorithm on this "bad" triangulation
                        //so it doesn't matter anyway

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
