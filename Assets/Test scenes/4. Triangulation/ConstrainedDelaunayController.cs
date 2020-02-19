using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

public class ConstrainedDelaunayController : MonoBehaviour 
{
    public int seed = 0;

    public float halfMapSize = 10f;

    public int numberOfPoints = 10;

    //One obstacle where the vertices are connected to form the entire obstacle
    public List<Vector3> obstacle;

    Mesh triangulatedMesh;



    public void GenererateTriangulation()
    {
        //Add the random points
        HashSet<Vector3> randomPoints = new HashSet<Vector3>();

        Random.InitState(seed);

        for (int i = 0; i < numberOfPoints; i++)
        {
            float randomX = Random.Range(-halfMapSize, halfMapSize);
            float randomZ = Random.Range(-halfMapSize, halfMapSize);

            Vector3 randomPos = new Vector3(randomX, 0f, randomZ);

            randomPoints.Add(randomPos);
        }

        //Generate the triangulation
        HalfEdgeData triangleData = _Delaunay.ConstrainedTriangulationWithSloan(randomPoints, obstacle, true, new HalfEdgeData());

        //From half-edge to triangle
        HashSet<Triangle> triangulation = TransformBetweenDataStructures.TransformFromHalfEdgeToTriangle(triangleData);

        //From triangulation to mesh
        triangulatedMesh = TransformBetweenDataStructures.ConvertFromTriangleToMeshCompressed(triangulation, true);
    }



    private void OnDrawGizmos()
    {
        if (triangulatedMesh != null)
        {
            DebugResults.DisplayMesh(triangulatedMesh, seed);
        }


        //Display the obstacles
        if (obstacle != null)
        {
            //DebugResults.DisplayConnectedPoints(obstacle, Color.black);
        }
    }
}
