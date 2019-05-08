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
        //Algorithm 1
        //HalfEdgeData triangleData = _Delaunay.TriangulateByFlippingEdges(randomPoints);

        //Algorithm 2
        HalfEdgeData triangleData = _Delaunay.TriangulatePointByPoint(randomPoints, new HalfEdgeData());


        //From half-edge to triangle
        HashSet<Triangle> triangulation = TransformBetweenDataStructures.TransformFromHalfEdgeToTriangle(triangleData);

        //Convert to mesh
        triangulatedMesh = TransformBetweenDataStructures.ConvertFromTriangleToMeshCompressed(triangulation, true);
    }



    private void OnDrawGizmos()
    {
        //Display the triangulation with random colors
        DebugResults.DisplayMesh(triangulatedMesh, true, seed, Color.black);

        //Display the points
        if (displayVertices)
        {
            DebugResults.DisplayPoints(randomPoints, 0.2f, Color.black);
        }
    }
}
