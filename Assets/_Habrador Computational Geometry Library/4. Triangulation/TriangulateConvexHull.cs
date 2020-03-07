using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class TriangulateConvexHull
    {
        //
        // Algorithm 1
        //

        //If you have points on a convex hull, sorted one after each other 
        //If you have colinear points, it will ignore some of them but still triangulate the entire area
        //Colinear points are not changing the shape
        public static HashSet<Triangle2> GetTriangles(List<MyVector2> pointsOnHull, bool addColinearPoints)
        {
            //If we hadnt have to deal with colinear points, this algorithm would be really simple:
            HashSet<Triangle2> triangles = new HashSet<Triangle2>();

            //This vertex will be a vertex in all triangles
            MyVector2 a = pointsOnHull[0];

            //And then we just loop through the other edges to make all triangles
            for (int i = 1; i < pointsOnHull.Count; i++)
            {
                MyVector2 b = pointsOnHull[i];
                MyVector2 c = pointsOnHull[MathUtility.ClampListIndex(i + 1, pointsOnHull.Count)];

                //Is this a valid triangle?
                //If a, b, c are on the same line, the triangle has no area and we can't add it
                LeftOnRight pointRelation = _Geometry.IsPoint_Left_On_Right_OfVector(a, b, c);

                if (pointRelation == LeftOnRight.On)
                {
                    continue;
                }

                triangles.Add(new Triangle2(a, b, c));
            }


            //Add the missing colinear points by splitting triangles
            //Step 2.1. We have now triangulated the entire convex polygon, but if we have colinear points
            //some of those points were not added
            //We can add them by splitting triangles

            //Find colinear points
            if (addColinearPoints)
            {
                HashSet<MyVector2> colinearPoints = new HashSet<MyVector2>(pointsOnHull);

                //Remove points that are in the triangulation from the points on the convex hull
                //and we can see which where not added = the colinear points
                foreach (Triangle2 t in triangles)
                {
                    colinearPoints.Remove(t.p1);
                    colinearPoints.Remove(t.p2);
                    colinearPoints.Remove(t.p3);
                }

                //Debug.Log("Colinear points: " + colinearPoints.Count);

                //Go through all colinear points and find which edge they should split
                //On the border we only need to split one edge because this edge has no neighbors
                foreach (MyVector2 p in colinearPoints)
                {
                    foreach (Triangle2 t in triangles)
                    {
                        //Is this point in the triangle
                        if (_Intersections.PointTriangle(t, p, includeBorder: true))
                        {
                            SplitTriangleEdge(t, p, triangles);

                            break;
                        }
                    }
                }
            }


            return triangles;
        }

        //Help method to split triangle edge
        private static void SplitTriangleEdge(Triangle2 t, MyVector2 p, HashSet<Triangle2> triangles)
        {
            MyVector2 a = t.p1;
            MyVector2 b = t.p2;
            MyVector2 c = t.p3;

            //Which edge should we split?
            if (_Geometry.IsPoint_Left_On_Right_OfVector(a, b, p) == LeftOnRight.On)
            {
                Triangle2 t1_new = new Triangle2(a, c, p);
                Triangle2 t2_new = new Triangle2(b, c, p);

                triangles.Remove(t);

                triangles.Add(t1_new);
                triangles.Add(t2_new);
            }
            else if (_Geometry.IsPoint_Left_On_Right_OfVector(b, c, p) == LeftOnRight.On)
            {
                Triangle2 t1_new = new Triangle2(b, a, p);
                Triangle2 t2_new = new Triangle2(c, a, p);

                triangles.Remove(t);

                triangles.Add(t1_new);
                triangles.Add(t2_new);
            }
            else if (_Geometry.IsPoint_Left_On_Right_OfVector(c, a, p) == LeftOnRight.On)
            {
                Triangle2 t1_new = new Triangle2(c, b, p);
                Triangle2 t2_new = new Triangle2(a, b, p);

                triangles.Remove(t);

                triangles.Add(t1_new);
                triangles.Add(t2_new);
            }
        }



        //
        // Algorithm 2
        //

        //Provide a point which is inside of the convex hull to make it easier to triangulate colinear points
        //And the triangles should be more "even", which can also be useful
        public static HashSet<Triangle2> GetTriangles(List<MyVector2> points, MyVector2 pointInside)
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
