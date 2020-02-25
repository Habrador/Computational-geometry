using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Habrador_Computational_Geometry
{
    //Triangulate random points by: 
    //1. Sort the points along one axis. The first 3 points form a triangle 
    //2. Consider the next point and connect it with all previously connected points which are visible to the point
    //3. Do 2 until we are out of points to add
    //Is not working with colinear points
    public static class IncrementalTriangulationAlgorithm
    {
        public static HashSet<Triangle2> TriangulatePoints(HashSet<MyVector2> pointsHashset)
        {
            //Alt 1. Use the convex hull of the current triangulation to determine if edge is visible from point
            HashSet<Triangle2> triangulation = TriangulatePointsConvexHull(pointsHashset);

            //Alt 2. Use edge from point to center of edge to test if edge is visible
            //Is muuuuuuch slower than convex hull because we have to test edges against all other edges

            return triangulation;
        }



        //We assume an edge is visible from a point if the triangle (formed by travering edges in the convex hull
        //of the existing triangulation) form a clockwise triangle with the point
        //https://stackoverflow.com/questions/8898255/check-whether-a-point-is-visible-from-a-face-of-a-2d-convex-hull
        private static HashSet<Triangle2> TriangulatePointsConvexHull(HashSet<MyVector2> pointsHashset)
        {
            HashSet<Triangle2> triangles = new HashSet<Triangle2>();

            //The points we will add
            List<MyVector2> originalPoints = new List<MyVector2>(pointsHashset);


            //Step 1. Sort the points along x-axis
            //OrderBy is always soring in ascending order - use OrderByDescending to get in the other order
            //originalPoints = originalPoints.OrderBy(n => n.x).ThenBy(n => n.y).ToList();
            originalPoints = originalPoints.OrderBy(n => n.x).ToList();


            //Step 2. Create the first triangle so we can start the algorithm

            //Assumes the convex hull algorithm sorts in the same way in x and y directions as we do above
            MyVector2 p1Start = originalPoints[0];
            MyVector2 p2Start = originalPoints[1];
            MyVector2 p3Start = originalPoints[2];

            //Theese points for the first triangle
            Triangle2 newTriangle = new Triangle2(p1Start, p2Start, p3Start);

            triangles.Add(newTriangle);


            //All points that form the triangles
            HashSet<MyVector2> triangulatedPoints = new HashSet<MyVector2>();

            triangulatedPoints.Add(p1Start);
            triangulatedPoints.Add(p2Start);
            triangulatedPoints.Add(p3Start);

            
            //Step 3. Add the other points one-by-one
            //Add the other points one by one
            //Starts at 3 because we have already added 0,1,2
            for (int i = 3; i < originalPoints.Count; i++)
            {
                MyVector2 pointToAdd = originalPoints[i];

                //Find the convex hull of the current triangulation
                //It generates a counter-clockwise convex hull
                List<MyVector2> pointsOnHull = _ConvexHull.JarvisMarch(triangulatedPoints);

                if (pointsOnHull == null)
                {
                    Debug.Log("No points on hull when triangulating");

                    continue;
                }

                bool couldFormTriangle = false;

                //Loop through all edges in the convex hull
                for (int j = 0; j < pointsOnHull.Count; j++)
                {
                    MyVector2 p1 = pointsOnHull[j];
                    MyVector2 p2 = pointsOnHull[MathUtility.ClampListIndex(j + 1, pointsOnHull.Count)];

                    //If this triangle is clockwise, then we can see the edge
                    //so we should create a new triangle with this edge and the point
                    if (Geometry.IsTriangleOrientedClockwise(p1, p2, pointToAdd))
                    {
                        triangles.Add(new Triangle2(p1, p2, pointToAdd));

                        couldFormTriangle = true;
                    }
                }

                //Add the point to the list of all points in the current triangulation
                //if the point could form a triangle
                if (couldFormTriangle)
                {
                    triangulatedPoints.Add(pointToAdd);
                }
            }
            


            return triangles;
        }
    }
}
