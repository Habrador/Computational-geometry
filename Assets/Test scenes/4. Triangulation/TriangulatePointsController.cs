using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

public class TriangulatePointsController : MonoBehaviour 
{
    public int seed;
    //How many points do we want to triangulate?
    public int numberOfPoints = 25;
    //The size of the map
    public float mapSize = 20f;

    private Mesh triangulatedMesh;

    private HashSet<Vector3> originalPoints;


    public void TriangulateThePoints()
    {
        //
        //Generate the random points
        //
        HashSet<Vector3> points = new HashSet<Vector3>();

        //Generate random numbers with a seed
        Random.InitState(seed);

        float max = mapSize / 2f;
        float min = -mapSize / 2f;

        for (int i = 0; i < numberOfPoints; i++)
        {
            float randomX = Random.Range(min, max);
            float randomZ = Random.Range(min, max);

            points.Add(new Vector3(randomX, 0f, randomZ));
        }

        //Copy the list so we can display the original points because the algorithms might modify the list with points
        originalPoints = new HashSet<Vector3>(points);



        //Triangulate

        //3d to 2d
        HashSet<MyVector2> points_2d = new HashSet<MyVector2>();

        foreach (Vector3 v in originalPoints)
        {
            points_2d.Add(v.ToMyVector2());
        }

        //List<Triangle> triangles = null;


        //Alt 1 - sort the points and then add triangles by checking which edge is visible to that point
        //List<Triangle2> triangles_2d = _TriangulatePoints.IncrementalTriangulation(points_2d);

        //Alt 2 - triangulate the convex polygon, and the add points by splitting the triangles into 3 new triangles
        HashSet<Triangle2> triangles_2d = _TriangulatePoints.TriangleSplitting(points_2d);

        //Alt 3 - triangulate the convex hull of the algorithm
        //List<Triangle2> triangles_2d = TriangulatePointsAlgorithms.TriangulateConvexHull(points_2d);


        //List<Vector3> hull = HullAlgorithms.JarvisMarch(points);

        //Debug.Log(hull.Count);

        //for (int i = 1; i < hull.Count; i++)
        //{
        //    Debug.DrawLine(hull[i - 1], hull[i], Color.white, 1f);
        //}

        //DebugHalfEdge(triangles);


        //print("Triangles " + triangles.Count);


        
        if (triangles_2d != null)
        {
            //Make sure the triangles have the correct orientation
            HelpMethods.OrientTrianglesClockwise(triangles_2d);

            //From 2d to 3d
            HashSet<Triangle3> triangles_3d = new HashSet<Triangle3>();

            foreach (Triangle2 t in triangles_2d)
            {
                triangles_3d.Add(new Triangle3(t.p1.ToMyVector3(), t.p2.ToMyVector3(), t.p3.ToMyVector3()));
            }

            triangulatedMesh = TransformBetweenDataStructures.Triangle3ToCompressedMesh(triangles_3d);
        }
    }



    private void OnDrawGizmos()
    {
        if (triangulatedMesh != null)
        {
            //Display the triangles with a random color
            DebugResultsHelper.DisplayMesh(triangulatedMesh, seed);

            //Display the points
            DebugResultsHelper.DisplayPoints(originalPoints, 0.2f, Color.black);
        }
    }
}
