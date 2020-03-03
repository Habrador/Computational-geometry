using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using UnityEngine.UI;

public class VisualizerController : MonoBehaviour
{
    public int seed = 0;

    //Data related to the random points we want to do operations on
    public float halfMapSize = 10f;

    public int numberOfPoints = 10;

    //Time to pause to visualize something
    public float pauseTime = 0.25f;

    //To display how many flips we need
    public Text flipText;

    //To make it faster to unnormalize the data when we display it 
    private AABB normalizingBox;

    private float dMax;


    //Used for visualizing purposes

    //Triangles and materials so we can display them 
    private List<Mesh> triangleMeshes = new List<Mesh>();
    private List<Material> triangleMaterials = new List<Material>();

    //Black meshes
    private List<Mesh> blackMeshes = new List<Mesh>();

    private Material blackMaterial;

    [System.NonSerialized]
    public bool shouldDisplayColoredMesh = true;

    [System.NonSerialized]
    public MyVector2 activePoint;



    private void Start()
    {
        //Init GUI
        flipText.text = "Flipped edges: " + 0;

        //Create the material we use to display meshes with a single color
        blackMaterial = new Material(Shader.Find("Unlit/Color"));

        blackMaterial.color = Color.black;

        //Set active point to be outside of screen
        activePoint = new MyVector2(-10000f, -10000f);

        //Generate the points we want to triangulate
        HashSet<Vector3> points = TestAlgorithmsHelpMethods.GenerateRandomPoints(seed, halfMapSize, numberOfPoints);

        //From 3d to 2d
        HashSet<MyVector2> points_2d = new HashSet<MyVector2>();

        foreach (Vector3 v in points)
        {
            points_2d.Add(v.ToMyVector2());
        }


        //Normalize to range 0-1
        //We should use all points, including the constraints
        List<MyVector2> allPoints = new List<MyVector2>();

        allPoints.AddRange(new List<MyVector2>(points_2d));

        normalizingBox = HelpMethods.GetAABB(new List<MyVector2>(points_2d));

        dMax = HelpMethods.CalculateDMax(normalizingBox);

        HashSet<MyVector2> points_2d_normalized = HelpMethods.Normalize(points_2d, normalizingBox, dMax);


        //Run delaunay with some algorithm
        //DelaunayFlipEdgesVisual flipEdges = GetComponent<DelaunayFlipEdgesVisual>();

        //if (flipEdges != null)
        //{
        //    flipEdges.StartVisualizer(points_2d_normalized, new HalfEdgeData2());
        //}

        //DelaunayPointByPointVisual pointByPoint = GetComponent<DelaunayPointByPointVisual>();

        //if (pointByPoint != null)
        //{
        //    pointByPoint.StartVisualizer(points_2d_normalized, new HalfEdgeData2());
        //}

        //Triangulate with visible edges
        VisibleEdgeVisualizer visibleEdge = GetComponent<VisibleEdgeVisualizer>();

        if (visibleEdge)
        {
            visibleEdge.StartVisualization(points_2d_normalized);
        }
    }



    //Display what we want to display
    private void Update()
    {
        //Triangulation with random color
        if (triangleMeshes != null && triangleMaterials != null && shouldDisplayColoredMesh)
        {
            for (int i = 0; i < triangleMeshes.Count; i++)
            {
                //Display it
                Graphics.DrawMesh(triangleMeshes[i], Vector3.zero, Quaternion.identity, triangleMaterials[i], 0);
            }
        }


        //Black meshes
        if (blackMeshes != null && blackMaterial != null)
        {
            for (int i = 0; i < blackMeshes.Count; i++)
            {
                //Display it
                Graphics.DrawMesh(blackMeshes[i], Vector3.zero, Quaternion.identity, blackMaterial, 0);
            }
        }


        System.GC.Collect();
    }



    //Generate the mesh from the half-edge data structure, which is called when we have flipped an edge
    public void GenerateTriangulationMesh(HalfEdgeData2 triangleData_normalized)
    {
        //From half-edge to triangle
        HashSet<Triangle2> triangles_2d = new HashSet<Triangle2>();

        foreach (HalfEdgeFace2 f in triangleData_normalized.faces)
        {
            //Each face has in this case three edges
            MyVector2 p1 = f.edge.v.position;
            MyVector2 p2 = f.edge.nextEdge.v.position;
            MyVector2 p3 = f.edge.nextEdge.nextEdge.v.position;

            Triangle2 t = new Triangle2(p1, p2, p3);

            triangles_2d.Add(t);
        }

        GenerateTriangulationMesh(triangles_2d);
    }
    
    
    
    public void GenerateTriangulationMesh(HashSet<Triangle2> triangleData_unnormalized)
    {
        //Unnormalize
        HashSet<Triangle2> triangles_2d = new HashSet<Triangle2>();

        foreach (Triangle2 t in triangleData_unnormalized)
        {
            //Each face has in this case three edges
            MyVector2 p1 = t.p1;
            MyVector2 p2 = t.p2;
            MyVector2 p3 = t.p3;

            //Unnormalize the point
            p1 = HelpMethods.UnNormalize(p1, normalizingBox, dMax);
            p2 = HelpMethods.UnNormalize(p2, normalizingBox, dMax);
            p3 = HelpMethods.UnNormalize(p3, normalizingBox, dMax);

            Triangle2 t_unnormalized = new Triangle2(p1, p2, p3);

            triangles_2d.Add(t_unnormalized);
        }


        //Make sure the triangles have the correct orientation
        //triangles_2d = HelpMethods.OrientTrianglesClockwise(triangles_2d);


        //From 2d to mesh in one step
        //Mesh displayMesh = _TransformBetweenDataStructures.Triangles2ToMesh(triangles_2d, useCompressedMesh: false);


        //Generate the triangle meshes
        triangleMeshes.Clear();
        //Materials is not a constant because in some algorithms we add triangles
        //Could maybe in that case just add a new material?
        triangleMaterials.Clear();

        //This line is important or unity will run into memory problems
        //This line will remove the mesh and material we just cleared from memory
        Resources.UnloadUnusedAssets();


        Random.InitState(seed);

        foreach (Triangle2 t in triangles_2d)
        {
            Mesh triangleMesh = Triangle2ToMesh(t);


            //Color the triangle
            Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);

            //float grayScale = Random.Range(0f, 1f);

            //Color color = new Color(grayScale, grayScale, grayScale, 1f);


            Material mat = new Material(Shader.Find("Unlit/Color"));

            mat.color = color;


            //Add them to the lists
            triangleMeshes.Add(triangleMesh);

            triangleMaterials.Add(mat);
        }
    }


    //Generate just a circle mesh
    public void GenerateCircleMesh(MyVector2 a, bool shouldResetAllMeshes)
    {
        if (shouldResetAllMeshes)
        {
            ClearBlackMeshes();
        }

        a = HelpMethods.UnNormalize(a, normalizingBox, dMax);

        HashSet<Triangle2> circle_a = GenerateMesh.Circle(a, 0.2f, 10);

        //Generate meshes
        foreach (Triangle2 t in circle_a)
        {
            Mesh triangleMesh = Triangle2ToMesh(t, 0.1f);

            blackMeshes.Add(triangleMesh);
        }
    }


    //Generate circle meshes for delaunay based in 3 points in a triangle, where d is the opposite vertex
    public void GenerateDelaunayCircleMeshes(MyVector2 a, MyVector2 b, MyVector2 c, MyVector2 d)
    {
        //Remove all old meshes
        ClearBlackMeshes();


        //Unnormalize the points
        a = HelpMethods.UnNormalize(a, normalizingBox, dMax);
        b = HelpMethods.UnNormalize(b, normalizingBox, dMax);
        c = HelpMethods.UnNormalize(c, normalizingBox, dMax);
        d = HelpMethods.UnNormalize(d, normalizingBox, dMax);


        //Generate the triangles

        //Big circle
        MyVector2 center = Geometry.CalculateCircleCenter(a, b, c);

        float radius = MyVector2.Distance(center, a);
        
        HashSet<Triangle2> allTriangles = GenerateMesh.CircleHollow(center, radius, 100, 0.1f);


        //Circles showing the 4 points
        float circleMeshRadius = 0.3f;

        HashSet<Triangle2> circle_a = GenerateMesh.Circle(a, circleMeshRadius, 10);
        HashSet<Triangle2> circle_b = GenerateMesh.Circle(b, circleMeshRadius, 10);
        HashSet<Triangle2> circle_c = GenerateMesh.Circle(c, circleMeshRadius, 10);
        HashSet<Triangle2> circle_d = GenerateMesh.Circle(d, circleMeshRadius, 10);

        //Similar to List's add range
        allTriangles.UnionWith(circle_a);
        allTriangles.UnionWith(circle_b);
        allTriangles.UnionWith(circle_c);
        allTriangles.UnionWith(circle_d);


        //Active edge is a-c
        HashSet<Triangle2> activeEdgeMesh = GenerateMesh.LineSegment(a, c, 0.2f);

        allTriangles.UnionWith(activeEdgeMesh);


        //Generate meshes
        foreach (Triangle2 t in allTriangles)
        {
            Mesh triangleMesh = Triangle2ToMesh(t, 0.1f);

            blackMeshes.Add(triangleMesh);
        }
    }

    public void ClearBlackMeshes()
    {
        blackMeshes.Clear();

        Resources.UnloadUnusedAssets();
    }


    public void SetActivePoint(MyVector2 p)
    {
        activePoint = HelpMethods.UnNormalize(p, normalizingBox, dMax);
    }



    //From Triangle2 to mesh where height is option to avoid z-fighting
    private Mesh Triangle2ToMesh(Triangle2 t, float meshHeight = 0f)
    {
        //Make a single mesh triangle
        Vector3 p1 = t.p1.ToVector3(meshHeight);
        Vector3 p2 = t.p2.ToVector3(meshHeight);
        Vector3 p3 = t.p3.ToVector3(meshHeight);

        Mesh triangleMesh = new Mesh();

        triangleMesh.vertices = new Vector3[] { p1, p2, p3 };

        triangleMesh.triangles = new int[] { 0, 1, 2 };

        triangleMesh.RecalculateNormals();

        return triangleMesh;
    }



    //TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors(displayMesh, seed);
    //Is adding memory each time we run it in playmode, which could maybe 
    //have been solved by destroying the meshes we create each update???
    //But DrawMesh is similar
    private void OnDrawGizmos()
    {
        if (triangleMeshes != null)
        {
            foreach (Mesh m in triangleMeshes)
            {
                TestAlgorithmsHelpMethods.DisplayMeshEdges(m, Color.black);
            }


            Gizmos.color = Color.white;

            Gizmos.DrawWireSphere(activePoint.ToVector3(), 0.3f);
        }
    }
}
