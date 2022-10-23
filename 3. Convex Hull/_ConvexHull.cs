using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class _ConvexHull
    {
        //
        // 2d space
        //

        //Jarvis March - slow but simple
        public static List<MyVector2> JarvisMarch_2D(HashSet<MyVector2> points)
        {
            List<MyVector2> pointsList = new List<MyVector2>(points);

            if (!CanFormConvexHull_2d(pointsList))
            {
                return null;
            }
        
            //Has to return a list and not hashset because the points have an order coming after each other
            List<MyVector2> pointsOnHull = JarvisMarchAlgorithm2D.GenerateConvexHull(pointsList);

            return pointsOnHull;
        }


        //Quickhull
        public static List<MyVector2> Quickhull_2D(HashSet<MyVector2> points, bool includeColinearPoints)
        {
            List<MyVector2> pointsList = new List<MyVector2>(points);

            if (!CanFormConvexHull_2d(pointsList))
            {
                return null;
            }

            //Has to return a list and not hashset because the points have an order coming after each other
            List<MyVector2> pointsOnHull = QuickhullAlgorithm2D.GenerateConvexHull(pointsList, includeColinearPoints);

            return pointsOnHull;
        }



        //
        // 3d space
        //

        //Iterative
        //Normalizer is only needed for debugging
        //removeUnwantedTriangles means that we will try to improve the quality of the triangles in the hull
        public static HalfEdgeData3 Iterative_3D(HashSet<MyVector3> points, bool removeUnwantedTriangles, Normalizer3 normalizer = null)
        {
            List<MyVector3> pointsList = new List<MyVector3>(points);

            if (!CanFormConvexHull_3d(pointsList))
            {
                return null;
            }

            HalfEdgeData3 convexHull = IterativeHullAlgorithm3D.GenerateConvexHull(points, removeUnwantedTriangles, normalizer);

            return convexHull;
        }



        //
        // Algorithms that test if we can form a convex hull
        //
        private static bool CanFormConvexHull_2d(List<MyVector2> points)
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
            AABB2 rectangle = new AABB2(points);

            if (!rectangle.IsRectangleARectangle())
            {
                Debug.Log("The points cant form a convex hull");

                return false;
            }

            return true;
        }

        private static bool CanFormConvexHull_3d(List<MyVector3> points)
        {
            //First test of we can form a convex hull

            //If fewer points, then we cant create a convex hull in 3d space
            if (points.Count < 4)
            {
                Debug.Log("Too few points co calculate a convex hull");

                return false;
            }

            //Find the bounding box of the points
            //If the spread is close to 0, then they are all at the same position, and we cant create a hull
            AABB3 box = new AABB3(points);

            if (!box.IsBoxABox())
            {
                Debug.Log("The points cant form a convex hull");

                return false;
            }

            return true;
        }
    }
}
