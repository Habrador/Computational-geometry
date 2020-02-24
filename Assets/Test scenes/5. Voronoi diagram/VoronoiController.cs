using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

public class VoronoiController : MonoBehaviour 
{
    public int seed = 0;

    public float halfMapSize = 10f;

    public float numberOfPoints = 20;



    private void OnDrawGizmos()
    {
        //
        // Generate the random sites
        //
        HashSet<Vector3> randomSites = new HashSet<Vector3>();

        //Generate random numbers with a seed
        Random.InitState(seed);

        float max = halfMapSize;
        float min = -halfMapSize;

        for (int i = 0; i < numberOfPoints; i++)
        {
            float randomX = Random.Range(min, max);
            float randomZ = Random.Range(min, max);

            randomSites.Add(new Vector3(randomX, 0f, randomZ));
        }


        //Points outside of the screen for voronoi which has some cells that are infinite
        float bigSize = halfMapSize * 5f;

        //Star shape which will give a better result when a cell is infinite large
        //When using other shapes, some of the infinite cells misses triangles
        randomSites.Add(new Vector3(0f, 0f, bigSize));
        randomSites.Add(new Vector3(0f, 0f, -bigSize));
        randomSites.Add(new Vector3(bigSize, 0f, 0f));
        randomSites.Add(new Vector3(-bigSize, 0f, 0f));


        //
        // Generate the delaunay for comparison
        //

        //3d to 2d
        HashSet<MyVector2> randomSites_2d = new HashSet<MyVector2>();

        foreach (Vector3 v in randomSites)
        {
            randomSites_2d.Add(v.ToMyVector2());
        }

        HalfEdgeData2 triangleData = _Delaunay.ByFlippingEdges(randomSites_2d, new HalfEdgeData2());

        //From halfedge to triangle
        //HashSet<Triangle> triangles = TransformBetweenDataStructures.TransformFromHalfEdgeToTriangle(triangleData);

        //Mesh delaunayMesh = TransformBetweenDataStructures.ConvertFromTriangleToMeshCompressed(triangles, true);



        //
        // Generate the voronoi diagram
        //
        List<VoronoiCell2> cells = DelaunayToVoronoi.GenerateVoronoiDiagram(randomSites_2d);



        //
        // Debug
        //
        //Display the voronoi diagram
        DisplayVoronoiCells(cells);

        //Display the sites
        DisplayResultsHelper.DisplayPoints(randomSites, 0.1f, Color.black);

        //Display the delaunay triangles
        //DisplayDelaunay(delaunayMesh);
    }



    //Display the voronoi diagram with mesh
    private void DisplayVoronoiCells(List<VoronoiCell2> cells)
    {
        Random.InitState(seed);

        for (int i = 0; i < cells.Count; i++)
        {
            VoronoiCell2 c = cells[i];

            Vector3 p1 = c.sitePos.ToVector3();

            Gizmos.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);

            List<Vector3> vertices = new List<Vector3>();

            List<int> triangles = new List<int>();

            vertices.Add(p1);

            for (int j = 0; j < c.edges.Count; j++)
            {
                Vector3 p3 = c.edges[j].p1.ToVector3();
                Vector3 p2 = c.edges[j].p2.ToVector3();

                vertices.Add(p2);
                vertices.Add(p3);

                triangles.Add(0);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 1);
            }

            Mesh triangleMesh = new Mesh();

            triangleMesh.vertices = vertices.ToArray();

            triangleMesh.triangles = triangles.ToArray();

            triangleMesh.RecalculateNormals();

            Gizmos.DrawMesh(triangleMesh);
        }
    }



    //Display the delaunay triangulation with lines
    private void DisplayDelaunay(Mesh triangulatedMesh)
    {
        int[] meshTriangles = triangulatedMesh.triangles;

        Vector3[] meshVertices = triangulatedMesh.vertices;

        Random.InitState(seed);

        Gizmos.color = Color.black;

        for (int i = 0; i < meshTriangles.Length; i += 3)
        {
            Vector3 p1 = meshVertices[meshTriangles[i + 0]];
            Vector3 p2 = meshVertices[meshTriangles[i + 1]];
            Vector3 p3 = meshVertices[meshTriangles[i + 2]];

            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p1);
        }
    }
}
