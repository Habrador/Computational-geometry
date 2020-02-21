using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

public class DelaunayController : MonoBehaviour 
{
    public int seed = 0;

    public float halfMapSize = 10f;

    public int numberOfPoints = 10;

    private Mesh triangulatedMesh;

    private HashSet<Vector3> randomPoints;

    public bool displayVertices = false;



    public void GenererateTriangulation()
    {
        //Add the random points
        randomPoints = new HashSet<Vector3>();

        Random.InitState(seed);

        for (int i = 0; i < numberOfPoints; i++)
        {
            float randomX = Random.Range(-halfMapSize, halfMapSize);
            float randomZ = Random.Range(-halfMapSize, halfMapSize);

            Vector3 randomPos = new Vector3(randomX, 0f, randomZ);

            randomPoints.Add(randomPos);
        }


        //Generate the triangulation

        //From 3d to 2d
        HashSet<MyVector2> randomPoints_2d = new HashSet<MyVector2>();

        foreach (Vector3 v in randomPoints)
        {
            randomPoints_2d.Add(v.ToMyVector2());
        }


        //Algorithm 1
        //HalfEdgeData triangleData = _Delaunay.TriangulateByFlippingEdges(randomPoints);

        //Algorithm 2
        HalfEdgeData2 triangleData = _Delaunay.TriangulatePointByPoint(randomPoints_2d, new HalfEdgeData2());


        //From half-edge to triangle
        HashSet<Triangle2> triangles_2d = TransformBetweenDataStructures.TransformFromHalfEdgeToTriangle(triangleData);

        //Convert to mesh

        //Make sure the triangles have the correct orientation
        HelpMethods.OrientTrianglesClockwise(triangles_2d);

        //From 2d to 3d
        HashSet<Triangle3> triangulation_3d = new HashSet<Triangle3>();

        foreach (Triangle2 t in triangles_2d)
        {
            triangulation_3d.Add(new Triangle3(t.p1.ToMyVector3(), t.p2.ToMyVector3(), t.p3.ToMyVector3()));
        }

        triangulatedMesh = TransformBetweenDataStructures.ConvertFromTriangleToMeshCompressed(triangulation_3d);
    }



    private void OnDrawGizmos()
    {
        //Display the triangulation with random colors
        DebugResults.DisplayMesh(triangulatedMesh, seed);

        //Display the points
        if (displayVertices)
        {
            DebugResults.DisplayPoints(randomPoints, 0.2f, Color.black);
        }
    }
}
