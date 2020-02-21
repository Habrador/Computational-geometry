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
        

        //From 3d to 2d
        HashSet<MyVector2> randomPoints_2d = new HashSet<MyVector2>();
        
        foreach (Vector3 v in randomPoints)
        {
            randomPoints_2d.Add(v.ToMyVector2());
        }

        List<MyVector2> obstacle_2d = new List<MyVector2>();

        foreach (Vector3 v in obstacle)
        {
            obstacle_2d.Add(v.ToMyVector2());
        }


        //Generate the triangulation
        HalfEdgeData2 triangleData = _Delaunay.ConstrainedTriangulationWithSloan(randomPoints_2d, obstacle_2d, true, new HalfEdgeData2());

        //From half-edge to triangle
        HashSet<Triangle2> triangles_2d = TransformBetweenDataStructures.TransformFromHalfEdgeToTriangle(triangleData);

        //From triangulation to mesh
        
        //Make sure the triangles have the correct orientation
        HelpMethods.OrientTrianglesClockwise(triangles_2d);

        //From 2d to 3d
        HashSet<Triangle3> triangles_3d = new HashSet<Triangle3>();

        foreach (Triangle2 t in triangles_2d)
        {
            triangles_3d.Add(new Triangle3(t.p1.ToMyVector3(), t.p2.ToMyVector3(), t.p3.ToMyVector3()));
        }

        triangulatedMesh = TransformBetweenDataStructures.ConvertFromTriangleToMeshCompressed(triangles_3d);
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
