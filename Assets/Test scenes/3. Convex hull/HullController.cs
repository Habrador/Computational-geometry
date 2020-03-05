using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

public class HullController : MonoBehaviour 
{
    //The plane with colinear points to stress-test the algorithms
    public Transform planeTrans;

    public float mapSize;

    public int numberOfPoints;

    public int seed;



    private void OnDrawGizmos()
    {    
        //
        // Generate the points we are going to find the convex hull from
        //

        //Random points
        //HashSet<Vector3> points = TestAlgorithmsHelpMethods.GenerateRandomPoints(seed, mapSize, numberOfPoints);

        //Points from a plane mesh
        HashSet<Vector3> points = TestAlgorithmsHelpMethods.GeneratePointsFromPlane(planeTrans);


        //
        // Prepare the points
        //

        //From 3d to 2d
        HashSet<MyVector2> points_2d = new HashSet<MyVector2>();

        foreach (Vector3 v in points)
        {
            points_2d.Add(v.ToMyVector2());
        }

        //Normalize to range 0-1
        AABB2 normalizingBox = new AABB2(new List<MyVector2>(points_2d));

        float dMax = HelpMethods.CalculateDMax(normalizingBox);

        HashSet<MyVector2> points_2d_normalized = HelpMethods.Normalize(points_2d, normalizingBox, dMax);



        //
        // Generate the convex hull
        //



        //Algorithm 1. Jarvis March - slow but simple
        //List<MyVector2> pointsOnConvexHull_2d_normalized = _ConvexHull.JarvisMarch(points_2d_normalized);


        //Algorithm 2. Quickhull
        //List<MyVector2> pointsOnConvexHull_2d_normalized = _ConvexHull.Quickhull(points_2d_normalized, includeColinearPoints: true, normalizingBox, dMax);
        List<MyVector2> pointsOnConvexHull_2d_normalized = _ConvexHull.Quickhull(points_2d_normalized, includeColinearPoints: true);

        if (pointsOnConvexHull_2d_normalized == null)
        {
            Debug.Log("Couldnt find a convex hull");
        }



        //
        // Display 
        //

        //Display points on the hull and lines between the points
        if (pointsOnConvexHull_2d_normalized != null)
        {
            //UnNormalize
            List<MyVector2> pointsOnConvexHull_2d = HelpMethods.UnNormalize(pointsOnConvexHull_2d_normalized, normalizingBox, dMax);

            //From 2d to 3d
            List<Vector3> pointsOnConvexHull = new List<Vector3>();

            foreach (MyVector2 v in pointsOnConvexHull_2d)
            {
                pointsOnConvexHull.Add(v.ToVector3());
            }

            //print(pointsOnConvexHull.Count);

            for (int i = 0; i < pointsOnConvexHull.Count; i++)
            {
                int i_minus_one = MathUtility.ClampListIndex(i - 1, pointsOnConvexHull.Count);

                Gizmos.DrawLine(pointsOnConvexHull[i_minus_one], pointsOnConvexHull[i]);
            }

            float size = 0.1f;
            for (int i = 1; i < pointsOnConvexHull.Count; i++)
            {
                Gizmos.DrawWireSphere(pointsOnConvexHull[i], size);

                //So we can see in which order they were added
                size += 0.01f;
            }
        }

        //Display all the original points
        foreach (Vector3 p in points)
        {
            Gizmos.DrawSphere(p, 0.1f);
        }
    }



    //Time an algorithm
    private float TimeJarvis(HashSet<MyVector2> points)
    {
        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

        stopWatch.Start();
        
        List<MyVector2> pointsOnConvexHull_2d_normalized = _ConvexHull.JarvisMarch(points);
        
        stopWatch.Stop();

        float timeJarvis = stopWatch.ElapsedMilliseconds;

        return timeJarvis;
    }

}
