using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

public class DynamicConstrainedDelaunayController : MonoBehaviour 
{
    public int seed = 0;

    public float halfMapSize = 10f;

    public int numberOfPoints = 10;

    //One obstacle where the vertices are connected to form the entire obstacle
    [HideInInspector]
    public List<Vector3> obstacle;

    //The obstacles that are in the scene so we can remove them
    [HideInInspector]
    public List<Obstacle> obstaclesInTheScene;

    //The list of all constrained edges in the scene
    [HideInInspector]
    public List<Edge> allConstrainedEdges;

    private Mesh triangulatedMesh;

    private HalfEdgeData triangleData;



    //Generate a delaunay triangulation before we start adding/removing edges
    public void GenererateInitialTriangulation()
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

        //Generate the initial triangulation
        triangleData = _Delaunay.ConstrainedTriangulationWithSloan(randomPoints, null, false, new HalfEdgeData());

        //From half-edge to triangle
        HashSet<Triangle> triangulation = TransformBetweenDataStructures.TransformFromHalfEdgeToTriangle(triangleData);

        //From triangulation to mesh
        triangulatedMesh = TransformBetweenDataStructures.ConvertFromTriangleToMeshCompressed(triangulation, true);
    }



    private void OnDrawGizmos()
    {
        if (triangulatedMesh != null)
        {
            DebugResults.DisplayMesh(triangulatedMesh, false, seed, Color.gray);

            DebugResults.DisplayMeshSides(triangulatedMesh, Color.gray * 0.2f);
        }


        //Display where we want to add/remove an obstacle
        if (obstacle != null)
        {
            DebugResults.DisplayConnectedPoints(obstacle, Color.black);
        }

        //Display the obstacles
        if (obstaclesInTheScene != null)
        {
            for (int i = 0; i < obstaclesInTheScene.Count; i++)
            {
                DebugResults.DisplayConnectedPoints(obstaclesInTheScene[i].obstacle, Color.red);
            }
        }
    }



    [System.Serializable]
    public struct Obstacle
    {
        public List<Vector3> obstacle;

        public Obstacle(List<Vector3> obstacle)
        {
            this.obstacle = obstacle;
        }
    }

}
