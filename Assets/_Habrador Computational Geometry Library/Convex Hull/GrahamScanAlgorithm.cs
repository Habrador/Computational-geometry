//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//public static class GrahamScanAlgorithm
//{
//    public static List<Vertex> GetConvexHull(List<Vertex> points)
//    {
//        //The list with points on the convex hull
//        List<Vertex> convexHull = new List<Vertex>();

       

//        //Step 1 - Find the vertice with the smallest x coordinate
//        //If several have the same x coordinate, find the one with the smallest z
//        //Init with just the first in the list
//        Vector3 smallestValue = points[0].position;
//        int smallestIndex = 0;

//        //Check if we can find a smaller value
//        for (int i = 1; i < points.Count; i++)
//        {
//            Vertex point = points[i];

//            if (point.position.x < smallestValue.x || (point.position.x == smallestValue.x && point.position.z < smallestValue.z))
//            {
//                smallestValue = point.position;

//                smallestIndex = i;
//            }
//        }

//        //Gizmos.DrawWireSphere(points[smallestIndex].position, 0.5f);

//        //return null;

//        //Remove the smallest coordinate from the list and add it to the list 
//        //with convex hull vertices because this vertex is on the convex hull
//        convexHull.Add(points[smallestIndex]);

//        points.RemoveAt(smallestIndex);



//        //Step 2 - Sort the vertices based on angle to start vertex
//        Vector3 firstPoint = convexHull[0].position;

//        //Important to assign the points to points
//        //The first point in this list is now as "below" as possible to the first point, so we will make the convex hull counter-clockwise
//        //If the have the same angle, order by distance???
//        //points = points.OrderBy(n => Mathf.Atan2(n.position.z - firstPoint.z, n.position.x - firstPoint.x)).ThenBy(n => Vector3.SqrMagnitude(n.position - firstPoint)).ToList();
//        points = points.OrderBy(n => Mathf.Atan2(n.position.z - firstPoint.z, n.position.x - firstPoint.x)).ToList();

//        Gizmos.DrawWireSphere(points[0].position, 0.5f);
//        Gizmos.DrawWireSphere(points[points.Count - 1].position, 0.5f);

//        //Test the sorting by returning points instead of convexHull
//        //return points;

//        //Debug
//        //for (int i = 0; i < points.Count; i++)
//        //{
//        //    Gizmos.DrawLine(firstPoint, points[i].position);
//        //}
//        //Gizmos.DrawLine(firstPoint, points[points.Count - 1].position);



//        //Step 3 - The vertex with the smallest angle is also on the convex hull so add it, so now we have two points on the convex hull!
//        convexHull.Add(points[0]);



//        //Step 4 - The main algorithm to find the convex hull
//        //Start with the first two points from our sorted list of points, then add the next point in the list. 
//        //Test the last three points to see if they are clockwise. If they are continue to add points to the hull. 
//        //Whenever, the last three points are counter clockwise, remove them from the hull.

//        int m = 2;
//        for (int i = 1; i < points.Count; i++)
//        {
//            //Get the vertices of the current triangle abc
//            Vertex a = convexHull[m - 2];
//            Vertex b = convexHull[m - 1];

//            Vertex c = points[i];

//            //Is this a clockwise or a counter-clockwise triangle ?
//            if (!IsTriangleOrientedClockwise(a, b, c))
//            {
//                convexHull.Add(c);

//                m += 1;
//            }
//            else
//            {
//                //May need to back track several steps in case we messed up at an earlier point
//                int safety = 0;
//                while (IsTriangleOrientedClockwise(a, b, c) && safety < 10000)
//                {
//                    //Is this costly?? We are removing from the end, an alternative is to usevertex.previousVertex
//                    convexHull.RemoveAt(convexHull.Count - 1);

//                    m -= 1;

//                    a = convexHull[m - 2];
//                    b = convexHull[m - 1];

//                    c = points[i];

//                    safety += 1;

//                    if (safety > 1000)
//                    {
//                        break;
//                    }
//                }

//                i -= 1;
//            }
//        }



//        return convexHull;
//    }



//    //Help method to go from vertex to vector2
//    private static bool IsTriangleOrientedClockwise(Vertex a, Vertex b, Vertex c)
//    {
//        Vector2 a2D = new Vector2(a.position.x, a.position.z);
//        Vector2 b2D = new Vector2(b.position.x, b.position.z);
//        Vector2 c2D = new Vector2(c.position.x, c.position.z);

//        return Geometry.IsTriangleOrientedClockwise(a2D, b2D, c2D);
//    }
//}
