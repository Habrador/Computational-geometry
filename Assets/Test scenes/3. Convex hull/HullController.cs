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
        // Generate the convex hull
        //

        //From 3d to 2d
        HashSet<MyVector2> points_2d = new HashSet<MyVector2>();

        foreach (Vector3 v in points)
        {
            points_2d.Add(v.ToMyVector2());
        }

        //Algorithm 1. Jarvis March - slow but simple
        List<MyVector2> pointsOnConvexHull_2d = _ConvexHull.JarvisMarch(points_2d);

        if (pointsOnConvexHull_2d == null)
        {
            Debug.Log("Couldnt find a convex hull");
        }

        

        //
        // Display 
        //

        //Display points on the hull and lines between the points
        if (pointsOnConvexHull_2d != null)
        {
            //From 2d to 3d
            List<Vector3> pointsOnConvexHull = new List<Vector3>();

            foreach (MyVector2 v in pointsOnConvexHull_2d)
            {
                pointsOnConvexHull.Add(v.ToVector3());
            }

            //print(pointsOnConvexHull.Count);

            for (int i = 1; i < pointsOnConvexHull.Count; i++)
            {
                Gizmos.DrawLine(pointsOnConvexHull[i - 1], pointsOnConvexHull[i]);
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
}
