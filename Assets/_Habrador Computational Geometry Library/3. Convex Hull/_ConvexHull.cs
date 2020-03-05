using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class _ConvexHull
    {
        //2d space
    
        //Algorithm 1. Jarvis March - slow but simple
        public static List<MyVector2> JarvisMarch(HashSet<MyVector2> points)
        {
            List<MyVector2> pointsList = new List<MyVector2>(points);

            if (!CanFormConvexHull(pointsList))
            {
                return null;
            }
        
            //Has to return a list and not hashset because the points have an order coming after each other
            List<MyVector2> pointsOnHull = JarvisMarchAlgorithm.GenerateConvexHull(pointsList);

            return pointsOnHull;
        }


        //Algorithm 2. Quickhull
        public static List<MyVector2> Quickhull(HashSet<MyVector2> points, bool includeColinearPoints)
        {
            List<MyVector2> pointsList = new List<MyVector2>(points);

            if (!CanFormConvexHull(pointsList))
            {
                return null;
            }

            //Has to return a list and not hashset because the points have an order coming after each other
            List<MyVector2> pointsOnHull = QuickhullAlgorithm.GenerateConvexHull(pointsList, includeColinearPoints);

            return pointsOnHull;
        }



        //
        // Algorithms that test if we can form a convex hull
        //
        private static bool CanFormConvexHull(List<MyVector2> points)
        {
            //First test of we can form a convex hull

            //If fewer points, then we cant create a convex hull
            if (points.Count < 3)
            {
                Debug.Log("Too few points co calculate a convex hull");

                return false;
            }

            //Find the bounding box of the points
            //If the spread is close to 0, then they are all at the same position, and we cant create a hull
            AABB2 box = new AABB2(points);

            if (Mathf.Abs(box.maxX - box.minX) < MathUtility.EPSILON || Mathf.Abs(box.maxY - box.minY) < MathUtility.EPSILON)
            {
                Debug.Log("The points cant form a convex hull");

                return false;
            }

            return true;
        }
    }
}
