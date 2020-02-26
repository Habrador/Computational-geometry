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
            List<Triangle2> triangles = new List<Triangle2>();

            //This vertex will be a vertex in all triangles (except for some of those that forms colinear points)
            MyVector2 a = points[0];

            List<MyVector2> colinearPoints = new List<MyVector2>();

            //And then we just loop through the other edges to make all triangles
            for (int i = 2; i < points.Count; i++)
            {
                MyVector2 b = points[i - 1];
                MyVector2 c = points[i];

                //If we hadnt had colinear points this would have been the last line
                //triangles.Add(new Triangle2(a, b, c));

                //But we can always make a triangle if the corners of the triangle are co-linear
                //Then the triangle will be flat. So we have to check if b is one the line between a and c
                LeftOnRight orientation = Geometry.IsPoint_Left_On_Right_OfVector(a, c, b);

                if (orientation == LeftOnRight.On)
                {
                    colinearPoints.Add(b);

                    continue;
                }
                else
                {
                    //First check if we have colinear points that have to be added
                    if (colinearPoints.Count > 0)
                    {
                        //First add the colinear points
                        for (int j = 0; j < colinearPoints.Count; j++)
                        {
                            if (j == 0)
                            {
                                triangles.Add(new Triangle2(a, colinearPoints[j], c));
                            }
                            else
                            {
                                triangles.Add(new Triangle2(colinearPoints[j - 1], colinearPoints[j], c));
                            }
                        }

                        //Add the last triangle
                        triangles.Add(new Triangle2(colinearPoints[colinearPoints.Count - 1], b, c));

                        colinearPoints.Clear();
                    }
                    else
                    {
                        triangles.Add(new Triangle2(a, b, c));
                    }
                }
            }

            //We might still have colinear points to add
            if (colinearPoints.Count > 0)
            {
                //Remove the last triangle because it's not valid anymore
                Triangle2 lastTriangle = triangles[triangles.Count - 1];

                triangles.RemoveAt(triangles.Count - 1);

                //We also have to add the last point on the hull if its colinear
                MyVector2 lastOnHull = points[points.Count - 1];
                MyVector2 lastColinear = colinearPoints[colinearPoints.Count - 1];

                LeftOnRight orientation = Geometry.IsPoint_Left_On_Right_OfVector(a, lastColinear, lastOnHull);

                if (orientation == LeftOnRight.On)
                {
                    colinearPoints.Add(lastOnHull);
                }

                //Add the colinear points

                //First we have to identify our new a - the point we will anchor all new triangles to
                //This new a is part of the triangle we removed
                MyVector2 newA = lastTriangle.p2;

                //We also add the first point on the hull to colinear points to make it easier to build triangles
                colinearPoints.Add(a);

                for (int i = 1; i < colinearPoints.Count; i++)
                {
                    MyVector2 b = colinearPoints[i - 1];
                    MyVector2 c = colinearPoints[i];

                    triangles.Add(new Triangle2(newA, b, c));

                    //Debug.DrawLine(colinearPoints[i].ToVector3(), Vector3.zero, Color.white, 3f);
                }
            }


            HashSet<Triangle2> finalTriangles = new HashSet<Triangle2>(triangles);

            return finalTriangles;
        }



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
