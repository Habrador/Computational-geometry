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
        //List<Vector3> points = GenerateRandomPoints(seed, mapSize, numberOfPoints);

        //Points from a plane mesh
        HashSet<Vector3> points = GeneratePointsFromPlane(planeTrans);



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



    //Generate random points within a specified square size
    private List<Vector3> GenerateRandomPoints(int seed, float squareSize, int totalPoints)
    {
        List<Vector3> randomPoints = new List<Vector3>();

        //Generate random numbers with a seed
        Random.InitState(seed);

        float max = squareSize;
        float min = -squareSize;

        for (int i = 0; i < totalPoints; i++)
        {
            float randomX = Random.Range(min, max);
            float randomZ = Random.Range(min, max);

            randomPoints.Add(new Vector3(randomX, 0f, randomZ));
        }

        return randomPoints;
    }



    //Find all vertices of a plane
    private HashSet<Vector3> GeneratePointsFromPlane(Transform planeTrans)
    {
        HashSet<Vector3> points = new HashSet<Vector3>();

        Mesh mesh = planeTrans.GetComponent<MeshFilter>().sharedMesh;

        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPos = planeTrans.TransformPoint(vertices[i]);

            points.Add(worldPos);
        }

        return points;
    }
}
