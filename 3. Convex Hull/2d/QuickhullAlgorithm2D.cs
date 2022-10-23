using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //https://en.wikipedia.org/wiki/Quickhull
    //"Implementing Quickhull" pdf from Valve by Dirk Gregorious
    public static class QuickhullAlgorithm2D
    {
        //Used for debugging so we can see what's going on
        //public static List<MyVector2> GenerateConvexHull(List<MyVector2> originalPoints, bool includeColinearPoints, AABB normalizingbox, float dMax)


        public static List<MyVector2> GenerateConvexHull(List<MyVector2> originalPoints, bool includeColinearPoints)
        {
            List<MyVector2> pointsOnConvexHull = new List<MyVector2>();


            //Step 1. 
            //Find the extreme points along each axis
            //This is similar to AABB but we need both x and y coordinates at each extreme point
            MyVector2 maxX = originalPoints[0];
            MyVector2 minX = originalPoints[0];
            MyVector2 maxY = originalPoints[0];
            MyVector2 minY = originalPoints[0];

            for (int i = 1; i < originalPoints.Count; i++)
            {
                MyVector2 p = originalPoints[i];

                if (p.x > maxX.x)
                {
                    maxX = p;
                }
                if (p.x < minX.x)
                {
                    minX = p;
                }

                if (p.y > maxY.y)
                {
                    maxY = p;
                }
                if (p.y < minY.y)
                {
                    minY = p;
                }
            }


            //Step 2. 
            //From the 4 extreme points, choose the pair that's furthest appart
            //These two are the first two points on the hull
            List<MyVector2> extremePoints = new List<MyVector2>() { maxX, minX, maxY, minY };

            //Just pick some points as start value
            MyVector2 p1 = maxX;
            MyVector2 p2 = minX;

            //Can use sqr because we are not interested in the exact distance
            float maxDistanceSqr = -Mathf.Infinity;

            //Loop through all points and compare them with each other
            for (int i = 0; i < extremePoints.Count; i++)
            {
                MyVector2 p1_test = extremePoints[i];
            
                for (int j = i + 1; j < extremePoints.Count; j++)
                {                
                    MyVector2 p2_test = extremePoints[j];

                    float distSqr = MyVector2.SqrDistance(p1_test, p2_test);

                    if (distSqr > maxDistanceSqr)
                    {
                        maxDistanceSqr = distSqr;

                        p1 = p1_test;
                        p2 = p2_test;
                    }
                }
            }

            //Convert the list to hashset to easier remove points which are on the hull or are inside of the hull
            HashSet<MyVector2> pointsToAdd = new HashSet<MyVector2>(originalPoints);

            //Remove the first 2 points on the hull
            pointsToAdd.Remove(p1);
            pointsToAdd.Remove(p2);


            //Step 3. 
            //Find the third point on the hull, by finding the point which is the furthest
            //from the line between p1 and p2
            MyVector2 p3 = FindPointFurthestFromEdge(p1, p2, pointsToAdd);
           
            //Remove it from the points we want to add
            pointsToAdd.Remove(p3);


            //Step 4. Form the intitial triangle 

            //Make sure the hull is oriented counter-clockwise
            Triangle2 tStart = new Triangle2(p1, p2, p3);

            if (_Geometry.IsTriangleOrientedClockwise(tStart.p1, tStart.p2, tStart.p3))
            {
                tStart.ChangeOrientation();
            }
            
            //New p1-p3
            p1 = tStart.p1;
            p2 = tStart.p2;
            p3 = tStart.p3;

            //pointsOnConvexHull.Add(p1);
            //pointsOnConvexHull.Add(p2);
            //pointsOnConvexHull.Add(p3);

            //Remove the points that we now know are within the hull triangle
            RemovePointsWithinTriangle(tStart, pointsToAdd);


            //Step 5. 
            //Associate the rest of the points to their closest edge
            HashSet<MyVector2> edge_p1p2_points = new HashSet<MyVector2>();
            HashSet<MyVector2> edge_p2p3_points = new HashSet<MyVector2>();
            HashSet<MyVector2> edge_p3p1_points = new HashSet<MyVector2>();

            foreach (MyVector2 p in pointsToAdd)
            {
                //p1 p2
                LeftOnRight pointRelation1 = _Geometry.IsPoint_Left_On_Right_OfVector(p1, p2, p);

                if (pointRelation1 == LeftOnRight.On || pointRelation1 == LeftOnRight.Right)
                {
                    edge_p1p2_points.Add(p);

                    continue;
                }

                //p2 p3
                LeftOnRight pointRelation2 = _Geometry.IsPoint_Left_On_Right_OfVector(p2, p3, p);

                if (pointRelation2 == LeftOnRight.On || pointRelation2 == LeftOnRight.Right)
                {
                    edge_p2p3_points.Add(p);

                    continue;
                }

                //p3 p1
                //If the point hasnt been added yet, we know it belong to this edge
                edge_p3p1_points.Add(p);
            }


            //Step 6
            //For each edge, find the point furthest away and create a new triangle
            //and repeat the above steps by finding which points are inside of the hull
            //and which points are outside and belong to a new edge
            
            //Will automatically ignore the last point on this sub-hull to avoid doubles 
            List<MyVector2> pointsOnHUll_p1p2 = CreateSubConvexHUll(p1, p2, edge_p1p2_points);

            List<MyVector2> pointsOnHUll_p2p3 = CreateSubConvexHUll(p2, p3, edge_p2p3_points);

            List<MyVector2> pointsOnHUll_p3p1 = CreateSubConvexHUll(p3, p1, edge_p3p1_points);


            //Create the final hull by combing the points
            pointsOnConvexHull.Clear();

            pointsOnConvexHull.AddRange(pointsOnHUll_p1p2);
            pointsOnConvexHull.AddRange(pointsOnHUll_p2p3);
            pointsOnConvexHull.AddRange(pointsOnHUll_p3p1);



            //Step 7. Add colinear points
            //I think the easiest way to add colinear points is to add them when the algorithm is finished
            if (includeColinearPoints)
            {
                pointsOnConvexHull = AddColinearPoints(pointsOnConvexHull, originalPoints);
            }


            //Debug.Log("Points on hull: " + pointsOnConvexHull.Count);


            return pointsOnConvexHull;
        }



        //Add colinear points to the convex hull
        private static List<MyVector2> AddColinearPoints(List<MyVector2> pointsOnConvexHull, List<MyVector2> points)
        {
            List<MyVector2> pointsOnConvexHull_IncludingColinear = new List<MyVector2>();
            
            //From the original points we dont have to remove the points that are on the convex hull
            //because they will be added anyway

            //Loop through all edges
            for (int i = 0; i < pointsOnConvexHull.Count; i++)
            {
                MyVector2 p1 = pointsOnConvexHull[i];
                MyVector2 p2 = pointsOnConvexHull[MathUtility.ClampListIndex(i + 1, pointsOnConvexHull.Count)];

                List<MyVector2> colinearPoints = new List<MyVector2>();

                foreach (MyVector2 p in points)
                {
                    LeftOnRight pointRelation = _Geometry.IsPoint_Left_On_Right_OfVector(p1, p2, p);

                    if (pointRelation == LeftOnRight.On)
                    {
                        colinearPoints.Add(p);
                    }
                }

                //Sort the colinear points so the are added on the correct order from p1 to p2
                colinearPoints = colinearPoints.OrderBy(n => MyVector2.SqrMagnitude(n - p1)).ToList();

                //Remove the last colinear point to avoid doubles
                colinearPoints.RemoveAt(colinearPoints.Count - 1);

                pointsOnConvexHull_IncludingColinear.AddRange(colinearPoints);
            }


            return pointsOnConvexHull_IncludingColinear;
        }



        //Split an edge and create a new sub-convex hull
        private static List<MyVector2> CreateSubConvexHUll(MyVector2 p1, MyVector2 p3, HashSet<MyVector2> pointsToAdd)
        {
            if (pointsToAdd.Count == 0)
            {
                //Never return the last point so we avoid doubles on the convex hull
                return new List<MyVector2>() { p1 };
            }
        
        
            //Find the point which is furthest from an edge
            MyVector2 p2 = FindPointFurthestFromEdge(p1, p3, pointsToAdd);

            //This point is also on the hull
            pointsToAdd.Remove(p2);

            //Remove points within this sub-hull triangle
            RemovePointsWithinTriangle(new Triangle2(p1, p2, p3), pointsToAdd);

            //No more points to add
            if (pointsToAdd.Count == 0)
            {
                //Never return the last point so we avoid doubles on the convex hull
                return new List<MyVector2>() { p1, p2 };
            }
            //If we still have points to add, we have to split the edges again
            else
            {
                //As before, find the points outside of each edge
                HashSet<MyVector2> edge_p1p2_points = new HashSet<MyVector2>();
                HashSet<MyVector2> edge_p2p3_points = new HashSet<MyVector2>();

                foreach (MyVector2 p in pointsToAdd)
                {
                    //p1 p2
                    LeftOnRight pointRelation1 = _Geometry.IsPoint_Left_On_Right_OfVector(p1, p2, p);

                    if (pointRelation1 == LeftOnRight.On || pointRelation1 == LeftOnRight.Right)
                    {
                        edge_p1p2_points.Add(p);

                        continue;
                    }

                    //p2 p3
                    //If the point hasnt been added yet, we know it belong to this edge
                    edge_p2p3_points.Add(p);
                }


                //Split the edge again
                List<MyVector2> pointsOnHUll_p1p2 = CreateSubConvexHUll(p1, p2, edge_p1p2_points);
                List<MyVector2> pointsOnHUll_p2p3 = CreateSubConvexHUll(p2, p3, edge_p2p3_points);


                //Combine the list
                List<MyVector2> pointsOnHull = pointsOnHUll_p1p2;

                pointsOnHull.AddRange(pointsOnHUll_p2p3);


                return pointsOnHull;
            }
        }



        //Remove from points from hashset that are within a triangle
        private static void RemovePointsWithinTriangle(Triangle2 t, HashSet<MyVector2> points)
        {
            HashSet<MyVector2> pointsToRemove = new HashSet<MyVector2>();

            foreach (MyVector2 p in points)
            {
                if (_Intersections.PointTriangle(t, p, includeBorder: true))
                {
                    pointsToRemove.Add(p);
                }
            }

            foreach (MyVector2 p in pointsToRemove)
            {
                points.Remove(p);
            }
        }



        //Find the point which is furthest away from an edge
        private static MyVector2 FindPointFurthestFromEdge(MyVector2 p1, MyVector2 p2, HashSet<MyVector2> points)
        {
            //Just init the third point
            MyVector2 p3 = new MyVector2(0f, 0f);

            //Set max distance to something small
            float maxDistanceToEdge = -Mathf.Infinity;

            //The direction of the edge so we can create the normal (doesnt matter in which way it points)
            MyVector2 edgeDir = p2 - p1;

            //We dont need to normalize this normal 
            MyVector2 edgeNormal = new MyVector2(edgeDir.y, -edgeDir.x);

            //Find the actual third point
            foreach (MyVector2 p in points)
            {
                //The distance between this point and the edge is the same as the distance between
                //the point and the plane
                Plane2 plane = new Plane2(p1, edgeNormal);

                float distanceToEdge = _Geometry.GetSignedDistanceFromPointToPlane(p, plane);

                //The distance can be negative if we are behind the plane
                //and because we just picked a normal out of nowhere, we have to make sure
                //the distance is positive
                if (distanceToEdge < 0f)
                {
                    distanceToEdge *= -1f;
                }

                //This point is better
                if (distanceToEdge > maxDistanceToEdge)
                {
                    maxDistanceToEdge = distanceToEdge;

                    p3 = p;
                }
            }

            return p3;
        }



        //For debugging
        private static void DisplayPoints(HashSet<MyVector2> points, Normalizer2 normalizer)
        {
            foreach (MyVector2 p in points)
            {
                MyVector2 pUnNormalize = normalizer.UnNormalize(p);
            
                Debug.DrawLine(pUnNormalize.ToVector3(), Vector3.zero, Color.blue, 3f);
            }
        }
    }
}
