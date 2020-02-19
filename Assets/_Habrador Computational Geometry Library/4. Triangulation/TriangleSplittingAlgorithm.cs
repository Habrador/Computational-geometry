using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Triangulate random points by first generating the convex hull of the points, then triangulate the convex hull
    //and then add the other points one-by-one by splitting the triangle the point is in
    public static class TriangleSplittingAlgorithm
    {
        public static HashSet<Triangle> TriangulatePoints(HashSet<Vector3> points)
        {
            //Step 1. Generate the convex hull - will also remove the points from points list which are not on the hull
            HashSet<Vector2> points_2d = HelpMethods.ConvertListFrom3DTo2D(points);

            List<Vector2> pointsOnConvexHull_2d = _ConvexHull.JarvisMarch(points_2d);

            List<Vector3> pointsOnConvexHull = HelpMethods.ConvertListFrom2DTo3D(pointsOnConvexHull_2d);


            //Step 2. Triangulate the convex hull
            HashSet<Triangle> trianglesInput = _TriangulatePoints.TriangulateConvexHullAlgorithm(pointsOnConvexHull);

            //Convert to hashset to make it faster to remove triangles
            HashSet<Triangle> triangles = new HashSet<Triangle>(trianglesInput);


            //Step 3. From the points we should add, remove those that are already a part of the triangulation
            //We can use these to test the accuracy because these points are on the border
            foreach (Triangle t in triangles)
            {
                points.Remove(t.p1);
                points.Remove(t.p2);
                points.Remove(t.p3);
            }


            //Step 4. Add the remaining points
            foreach (Vector3 currentPoint in points)
            {
                //Which triangle is this point in?
                foreach (Triangle t in triangles)
                {
                    //Dont test if a triangle is on the edge because it's difficult because of floating point precision issues
                    //And most points are not on edges anyway
                    //But if it is very close to the edge, we will still get the correct edge structure when we have run this 
                    //triangulation through a delaunay triangulation algorithm, so the end result will be the same
                    if (Intersections.IsPointInTriangle(t.p1.XZ(), t.p2.XZ(), t.p3.XZ(), currentPoint.XZ(), true))
                    {
                        //Split the triangle into three new triangles

                        //Create 3 new  with correct orientation = clockwise
                        Triangle t1 = new Triangle(t.p1, t.p2, currentPoint);
                        Triangle t2 = new Triangle(t.p2, t.p3, currentPoint);
                        Triangle t3 = new Triangle(t.p3, t.p1, currentPoint);

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
