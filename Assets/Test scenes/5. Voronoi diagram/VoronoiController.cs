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

        //3d to 2d
        HashSet<MyVector2> randomSites_2d = new HashSet<MyVector2>();

        foreach (Vector3 v in randomSites)
        {
            randomSites_2d.Add(v.ToMyVector2());
        }


        //Generate the voronoi
        List<VoronoiCell2> voronoiCells = _Voronoi.DelaunyToVoronoi(randomSites_2d);

        //Display the voronoi diagram
        DisplayVoronoiCells(voronoiCells);

        //Display the sites
        TestAlgorithmsHelpMethods.DisplayPoints(randomSites, 0.1f, Color.black);


        //Generate delaunay for comparisons
        GenerateDelaunay(randomSites_2d);
    }



    private void GenerateDelaunay(HashSet<MyVector2> points)
    {
        HalfEdgeData2 delaunayData = _Delaunay.FlippingEdges(points, new HalfEdgeData2());

        //From halfedge to triangle
        HashSet<Triangle2> triangles = TransformBetweenDataStructures.HalfEdge2ToTriangle2(delaunayData);

        //Make sure they have the correct orientation
        triangles = HelpMethods.OrientTrianglesClockwise(triangles);

        //2d to 3d
        HashSet<Triangle3> triangles_3d = new HashSet<Triangle3>();

        foreach (Triangle2 t in triangles)
        {
            triangles_3d.Add(new Triangle3(t.p1.ToMyVector3(), t.p2.ToMyVector3(), t.p3.ToMyVector3()));
        }

        Mesh delaunayMesh = TransformBetweenDataStructures.Triangle3ToCompressedMesh(triangles_3d);

        //Display the delaunay triangles
        TestAlgorithmsHelpMethods.DisplayMeshEdges(delaunayMesh, Color.black);
    }



    //Display the voronoi diagram with mesh
    private void DisplayVoronoiCells(List<VoronoiCell2> cells)
    {
        Random.InitState(seed);

        for (int i = 0; i < cells.Count; i++)
        {
            VoronoiCell2 cell = cells[i];

            Vector3 p1 = cell.sitePos.ToVector3();

            Gizmos.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);

            List<Vector3> vertices = new List<Vector3>();

            List<int> triangles = new List<int>();

            vertices.Add(p1);

            for (int j = 0; j < cell.edges.Count; j++)
            {
                Vector3 p3 = cell.edges[j].p1.ToVector3();
                Vector3 p2 = cell.edges[j].p2.ToVector3();

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
}
