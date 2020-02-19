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
        //List<Triangle> triangles = null;


        //Alt 1 - sort the points and then add triangles by checking which edge is visible to that point
        //List<Triangle> triangles = _TriangulatePoints.IncrementalTriangulation(points);

        //Alt 2 - triangulate the convex polygon, and the add points by splitting the triangles into 3 new triangles
        HashSet<Triangle> triangles = _TriangulatePoints.TriangleSplitting(points);

        //Alt 3 - triangulate the convex hull of the algorithm
        //List<Triangle> triangles = TriangulatePointsAlgorithms.TriangulateConvexHull(points);


        //List<Vector3> hull = HullAlgorithms.JarvisMarch(points);

        //Debug.Log(hull.Count);

        //for (int i = 1; i < hull.Count; i++)
        //{
        //    Debug.DrawLine(hull[i - 1], hull[i], Color.white, 1f);
        //}

        //DebugHalfEdge(triangles);


        //print("Triangles " + triangles.Count);


        
        if (triangles != null)
        {
            //Make sure the triangles have the correct orientation
            HelpMethods.OrientTrianglesClockwise(triangles);

            triangulatedMesh = TransformBetweenDataStructures.ConvertFromTriangleToMeshCompressed(triangles, true);
        }
    }



    private void OnDrawGizmos()
    {
        if (triangulatedMesh != null)
        {
            //Display the triangles with a random color
            DebugResults.DisplayMesh(triangulatedMesh, seed);

            //Display the points
            DebugResults.DisplayPoints(originalPoints, 0.2f, Color.black);
        }
    }
}
