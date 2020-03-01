using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using UnityEngine.UI;

public class DelaunayVisualizerController : MonoBehaviour
{
    public int seed = 0;

    //Data related to the points we want to triangulate
    public float halfMapSize = 10f;

    public int numberOfPoints = 10;

    //To display how many flips we need
    public Text flipText;

    //To make it faster to unnormalize the data when we display it 
    private AABB normalizingBox;

    private float dMax;

    //Time to pause when we have made a flip
    public float timeBetweenFlip = 0.25f;

    //Triangles and materials so we can display them 
    private List<Mesh> triangleMeshes = new List<Mesh>();
    private List<Material> triangleMaterials = new List<Material>();



    private void Start()
    {
        //Init GUI
        flipText.text = "Flipped edges: " + 0;

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
        FlipEdgesVisual flipEdges = GetComponent<FlipEdgesVisual>();

        if (flipEdges != null)
        {
            flipEdges.Delaunay_FlipEdges_Visualizer(points_2d_normalized, new HalfEdgeData2());
        }
    }



    //Display the triangles
    private void Update()
    {
        if (triangleMeshes == null || triangleMaterials == null)
        {
            Debug.Log("No materials and/or no meshes to display");
        
            return;
        }


        for (int i = 0; i < triangleMeshes.Count; i++)
        {
            //Display it
            Graphics.DrawMesh(triangleMeshes[i], Vector3.zero, Quaternion.identity, triangleMaterials[i], 0);
        }

        System.GC.Collect();
    }



    //Generate the mesh from the half-edge data structure, which is called when we have flipped an edge
    public void GenerateMesh(HalfEdgeData2 triangleData_normalized)
    {
        //From half-edge to triangle while unnormalizing
        HashSet<Triangle2> triangles_2d = new HashSet<Triangle2>();

        foreach (HalfEdgeFace2 f in triangleData_normalized.faces)
        {
            //Each face has in this case three edges
            MyVector2 p1 = f.edge.v.position;
            MyVector2 p2 = f.edge.nextEdge.v.position;
            MyVector2 p3 = f.edge.nextEdge.nextEdge.v.position;

            //Unnormalize the point
            p1 = HelpMethods.UnNormalize(p1, normalizingBox, dMax);
            p2 = HelpMethods.UnNormalize(p2, normalizingBox, dMax);
            p3 = HelpMethods.UnNormalize(p3, normalizingBox, dMax);

            Triangle2 t = new Triangle2(p1, p2, p3);

            triangles_2d.Add(t);
        }


        //Make sure the triangles have the correct orientation
        //triangles_2d = HelpMethods.OrientTrianglesClockwise(triangles_2d);


        //From 2d to mesh in one step
        //Mesh displayMesh = _TransformBetweenDataStructures.Triangles2ToMesh(triangles_2d, useCompressedMesh: false);


        //Generate the triangle meshes
        GenerateTriangleMeshes(triangles_2d);
    }



    //Generates triangles and materials for each triangle
    private void GenerateTriangleMeshes(HashSet<Triangle2> triangles_2d)
    {
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
            //Make a single mesh triangle
            Vector3 p1 = t.p1.ToVector3();
            Vector3 p2 = t.p2.ToVector3();
            Vector3 p3 = t.p3.ToVector3();

            Mesh triangleMesh = new Mesh();

            triangleMesh.vertices = new Vector3[] { p1, p2, p3 };

            triangleMesh.triangles = new int[] { 0, 1, 2 };

            triangleMesh.RecalculateNormals();


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



    //Is adding memory each time we run it in playmode, which could maybe 
    //have been solved by destroying the meshes we create each update???
    //But DrawMesh is similar
    //private void OnDrawGizmos()
    //{
    //    //if (triangleData_normalized == null || triangleData_normalized.faces == null)
    //    //{
    //    //    return;
    //    //}

    //    //Debug.Log("Testing");

    //    //Display the triangulation 

    //    if (displayMesh == null)
    //    {
    //        return;
    //    }


    //    //Display the mesh
    //    TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors(displayMesh, seed);
    //}
}
