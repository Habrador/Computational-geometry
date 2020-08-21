using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

public class VoronoiController : MonoBehaviour 
{
    public int seed = 0;

    public float halfMapSize = 10f;

    public int numberOfPoints = 20;



    private void OnDrawGizmos()
    {
        //
        // Init the sites
        //

        //HashSet<Vector3> sites_3d = GetRandomSites();
        //HashSet<Vector3> sites_3d = GetCustomSites();
        HashSet<Vector3> sites_3d = GetCustomSites2();

        //3d to 2d
        HashSet<MyVector2> sites_2d = new HashSet<MyVector2>();

        foreach (Vector3 v in sites_3d)
        {
            sites_2d.Add(v.ToMyVector2());
        }


        //Normalize
        AABB2 normalizingBox = new AABB2(new List<MyVector2>(sites_2d));

        float dMax = HelpMethods.CalculateDMax(normalizingBox);

        HashSet<MyVector2> randomSites_2d_normalized = HelpMethods.Normalize(sites_2d, normalizingBox, dMax);


        //Generate the voronoi
        List<VoronoiCell2> voronoiCells = _Voronoi.DelaunyToVoronoi(randomSites_2d_normalized);


        //Unnormalize
        voronoiCells = HelpMethods.UnNormalize(voronoiCells, normalizingBox, dMax);


        //Display the voronoi diagram
        DisplayVoronoiCells(voronoiCells);

        //Display the sites
        TestAlgorithmsHelpMethods.DisplayPoints(sites_3d, 0.5f, Color.black);

        //Generate delaunay for comparisons
        GenerateDelaunay(sites_2d);
    }



    private void GenerateDelaunay(HashSet<MyVector2> points_2d)
    {
        //Normalize
        AABB2 normalizingBox = new AABB2(new List<MyVector2>(points_2d));

        float dMax = HelpMethods.CalculateDMax(normalizingBox);

        HashSet<MyVector2> points_2d_normalized = HelpMethods.Normalize(points_2d, normalizingBox, dMax);


        //Generate delaunay
        //HalfEdgeData2 delaunayData = _Delaunay.FlippingEdges(points_2d_normalized, new HalfEdgeData2());
        HalfEdgeData2 delaunayData = _Delaunay.PointByPoint(points_2d_normalized, new HalfEdgeData2());


        //UnNormalize
        HalfEdgeData2 triangleData = HelpMethods.UnNormalize(delaunayData, normalizingBox, dMax);

        //From halfedge to triangle
        HashSet<Triangle2> triangles = _TransformBetweenDataStructures.HalfEdge2ToTriangle2(triangleData);

        //Make sure they have the correct orientation
        triangles = HelpMethods.OrientTrianglesClockwise(triangles);

        //2d to 3d
        HashSet<Triangle3> triangles_3d = new HashSet<Triangle3>();


        int counter = -1;

        foreach (Triangle2 t in triangles)
        {
            counter++;

            //if (counter != 2)
            //{
            //    continue;
            //}
        
            triangles_3d.Add(new Triangle3(t.p1.ToMyVector3(), t.p2.ToMyVector3(), t.p3.ToMyVector3()));

            //Debug.Log($"p1: {t.p1.x} {t.p1.y} p2: {t.p2.x} {t.p2.y} p3: {t.p3.x} {t.p3.y}");

            //MyVector2 circleCenter = _Geometry.CalculateCircleCenter(t.p1, t.p2, t.p3);

            //Debug.Log("Circle center: " + circleCenter.x + " " + circleCenter.y);
        }

        Mesh delaunayMesh = _TransformBetweenDataStructures.Triangle3ToCompressedMesh(triangles_3d);

        //Display the delaunay triangles
        TestAlgorithmsHelpMethods.DisplayMeshEdges(delaunayMesh, Color.black);
    }



    //Display the voronoi diagram with mesh
    private void DisplayVoronoiCells(List<VoronoiCell2> cells)
    {
        Random.InitState(seed);

        for (int i = 0; i < cells.Count; i++)
        {
            //if (i != 0)
            //{
            //    continue;
            //}
        
            VoronoiCell2 cell = cells[i];

            Mesh triangleMesh = _TransformBetweenDataStructures.VoronoiCellToMesh(cell);

            Gizmos.DrawMesh(triangleMesh);
        }
    }



    //
    // Generate points
    //

    //Random
    private HashSet<Vector3> GetRandomSites()
    {
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

        return randomSites;
    }


    //Some points a user sent in that caused a bug
    private HashSet<Vector3> GetCustomSites()
    {
        HashSet<Vector3> sites = new HashSet<Vector3>();

        sites.Add(new Vector3(2.58301f, 0f, -2.07231092f));
        sites.Add(new Vector3(4.260807f, 0f, -0.274704f));
        sites.Add(new Vector3(8.839731f, 0f, -5.794293f));
        sites.Add(new Vector3(-12.11187f, 0f, -3.3448925f));
        sites.Add(new Vector3(8.466095f, 0f, 0.583772f));
        sites.Add(new Vector3(3.597496f, 0f, -3.383692f));
        sites.Add(new Vector3(3.421845f, 0f, -1.1747174f));
        sites.Add(new Vector3(-7.763506f, 0f, -3.487227f));
        sites.Add(new Vector3(-0.6644406f, 0f, -0.723027f));
        sites.Add(new Vector3(7.997253f, 0f, 0.485325f));
        sites.Add(new Vector3(3.234119f, 0f, -5.716683f));
        sites.Add(new Vector3(11.37916f, 0f, 1.904939f));
        sites.Add(new Vector3(11.13493f, 0f, -2.3131549f));
        sites.Add(new Vector3(6.510168f, 0f, 5.292708f));
        sites.Add(new Vector3(-2.473285f, 0f, 3.793113f));
        sites.Add(new Vector3(-8.900781f, 0f, -3.143157f));

        //Points outside of the screen for voronoi which has some cells that are infinite
        float xMax = 12.40375f;
        float xMin = -12.40375f;
        float zMax = 5.945362f;
        float zMin = -6.285754f;


        float xBigSize = (xMax - xMin) * 5f;
        float zBigSize = (zMax - zMin) * 5f;

        sites.Add(new Vector3(0f, 0f, zBigSize));
        sites.Add(new Vector3(0f, 0f, -zBigSize));
        sites.Add(new Vector3(xBigSize, 0f, 0f));
        sites.Add(new Vector3(-xBigSize, 0f, 0f));

        return sites;
    }


    //More points that causes bugs
    private HashSet<Vector3> GetCustomSites2()
    {
        HashSet<Vector3> sites = new HashSet<Vector3>();


        sites.Add(new Vector3(8f, 0f, -7.5f));
        sites.Add(new Vector3(8f, 0f, 2.5f));

        sites.Add(new Vector3(8f, 0f, 5.5f));

        sites.Add(new Vector3(2f, 0f, -7.5f));
        sites.Add(new Vector3(2f, 0f, 2.5f));

        float xMax = 9f;
        float xMin = -9f;
        float zMax = 9f;
        float zMin = -9f;


        float xBigSize = (xMax - xMin) * 5f;
        float zBigSize = (zMax - zMin) * 5f;

        sites.Add(new Vector3(0f, 0f, zBigSize));
        sites.Add(new Vector3(0f, 0f, -zBigSize));
        sites.Add(new Vector3(xBigSize, 0f, 0f));
        sites.Add(new Vector3(-xBigSize, 0f, 0f));

        return sites;
    }
}
