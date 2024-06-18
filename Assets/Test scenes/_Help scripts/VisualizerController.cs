using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using UnityEngine.UI;

public class VisualizerController : MonoBehaviour
{
    public int seed = 0;

    //Data related to the random points we want to do operations on
    //public float halfMapSize = 10f;

    //public int numberOfPoints = 10;

    //Time to pause to visualize something
    public float pauseTime = 0.25f;

    //To display how many flips we need
    //public Text flipText;

    //To make it faster to unnormalize the data when we display it
    private Normalizer2 normalizer;
    


    //Used for visualizing purposes

    //Triangles that has different colors
    [System.NonSerialized]
    public List<Mesh> multiColoredMeshes = new List<Mesh>();
    [System.NonSerialized]
    public List<Material> multiColoredMeshesMaterials = new List<Material>();

    //Triangles with a single color
    [System.NonSerialized]
    public List<Mesh> blackMeshes = new List<Mesh>();
    private Material blackMaterial;

    [System.NonSerialized]
    public bool shouldDisplayColoredMesh = true;

    //Display a single point with draw gizmos
    [System.NonSerialized]
    public MyVector2 activePoint;

    //Connected points
    [System.NonSerialized]
    public List<MyVector2> connectedPoints;


    /*
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

        normalizingBox = new AABB2(new List<MyVector2>(points_2d));

        dMax = HelpMethods.CalculateDMax(normalizingBox);

        HashSet<MyVector2> points_2d_normalized = HelpMethods.Normalize(points_2d, normalizingBox, dMax);


        //Visualization 1. Delaunay flip edges
        //DelaunayFlipEdgesVisual flipEdges = GetComponent<DelaunayFlipEdgesVisual>();

        //if (flipEdges != null)
        //{
        //    flipEdges.StartVisualizer(points_2d_normalized, new HalfEdgeData2());
        //}


        //Visualization 2. Delaunay point-by-point
        //DelaunayPointByPointVisual pointByPoint = GetComponent<DelaunayPointByPointVisual>();

        //if (pointByPoint != null)
        //{
        //    pointByPoint.StartVisualizer(points_2d_normalized, new HalfEdgeData2());
        //}


        //Visualization 3. Triangulate with visible edges
        //VisibleEdgeVisualizer visibleEdge = GetComponent<VisibleEdgeVisualizer>();

        //if (visibleEdge)
        //{
        //    visibleEdge.StartVisualization(points_2d_normalized);
        //}


        //Visualization 4. Gift wrapping
        GiftWrappingVisualizer giftWrapping = GetComponent<GiftWrappingVisualizer>();

        if (giftWrapping)
        {
            giftWrapping.InitVisualization(points_2d_normalized);
        }
    }
    */


    //Display what we want to display
    //private void Update()
    //{
    //    //Triangulation with random color
    //    if (multiColoredMeshes != null && multiColoredMeshesMaterials != null && shouldDisplayColoredMesh)
    //    {
    //        for (int i = 0; i < multiColoredMeshes.Count; i++)
    //        {
    //            //Display it
    //            Vector3 meshPos = Vector3.zero;

    //            Graphics.DrawMesh(multiColoredMeshes[i], meshPos, Quaternion.identity, multiColoredMeshesMaterials[i], 0);
    //        }
    //    }


    //    //Black meshes
    //    if (blackMeshes != null && blackMaterial != null)
    //    {
    //        for (int i = 0; i < blackMeshes.Count; i++)
    //        {
    //            //Display it (draw the black mehes above)
    //            Vector3 meshPos = Vector3.zero + Vector3.up;

    //            Graphics.DrawMesh(blackMeshes[i], meshPos, Quaternion.identity, blackMaterial, 0);
    //        }
    //    }


    //    System.GC.Collect();
    //}



    //TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors(displayMesh, seed);
    //Is adding memory each time we run it in playmode, which could maybe 
    //have been solved by destroying the meshes we create each update???
    //But DrawMesh is similar
    private void OnDrawGizmos()
    {
        if (multiColoredMeshes != null)
        {
            foreach (Mesh m in multiColoredMeshes)
            {
                TestAlgorithmsHelpMethods.DisplayMeshEdges(m, Color.black);
            }
        }


        //This point is set to be outside if its not active
        Gizmos.color = Color.white;

        //Gizmos.DrawWireSphere(activePoint.ToVector3(), 0.3f);


        //Connected points
        if (connectedPoints != null)
        {
            Gizmos.color = Color.white;

            for (int i = 0; i < connectedPoints.Count; i++)
            {
                //Show the circle
                Gizmos.DrawWireSphere(connectedPoints[i].ToVector3(), 0.2f);

                //Line to previous point
                if (i > 0)
                {
                    Gizmos.DrawLine(connectedPoints[i].ToVector3(), connectedPoints[i - 1].ToVector3());
                }
            }
        }
    }



    //
    // Reset methods
    //

    public void ResetBlackMeshes()
    {
        blackMeshes.Clear();

        Resources.UnloadUnusedAssets();
    }

    public void ResetMultiColoredMeshes()
    {
        //Generate the triangle meshes
        multiColoredMeshes.Clear();
        //Materials is not a constant because in some algorithms we add triangles
        //Could maybe in that case just add a new material?
        multiColoredMeshesMaterials.Clear();

        //This line is important or unity will run into memory problems
        //This line will remove the mesh and material we just cleared from memory
        Resources.UnloadUnusedAssets();
    }



    //
    // Generate meshes
    //

    //Generate list of meshes from the Half-edge data structure
    public List<Mesh> GenerateTriangulationMesh(HalfEdgeData2 triangleData, bool shouldUnNormalize)
    {
        //From half-edge to triangle
        HashSet<Triangle2> triangles_2d = new HashSet<Triangle2>();

        foreach (HalfEdgeFace2 f in triangleData.faces)
        {
            //Each face has in this case three edges
            MyVector2 p1 = f.edge.v.position;
            MyVector2 p2 = f.edge.nextEdge.v.position;
            MyVector2 p3 = f.edge.nextEdge.nextEdge.v.position;

            Triangle2 t = new Triangle2(p1, p2, p3);

            triangles_2d.Add(t);
        }

        List<Mesh> meshes = GenerateTriangulationMesh(triangles_2d, shouldUnNormalize);

        return meshes;
    }


    //Generate list of meshes from the Triangle2 data structure
    public List<Mesh> GenerateTriangulationMesh(HashSet<Triangle2> triangleData, bool shouldUnNormalize)
    {
        //Unnormalize
        HashSet<Triangle2> triangles_2d = new HashSet<Triangle2>();

        if (shouldUnNormalize)
        {
            foreach (Triangle2 t in triangleData)
            {
                //Each face has in this case three edges
                MyVector2 p1 = t.p1;
                MyVector2 p2 = t.p2;
                MyVector2 p3 = t.p3;

                //Unnormalize the point
                p1 = normalizer.UnNormalize(p1);
                p2 = normalizer.UnNormalize(p2);
                p3 = normalizer.UnNormalize(p3);

                Triangle2 t_unnormalized = new Triangle2(p1, p2, p3);

                triangles_2d.Add(t_unnormalized);
            }
        }
        else
        {
            triangles_2d = triangleData;
        }


        //Make sure the triangles have the correct orientation
        //triangles_2d = HelpMethods.OrientTrianglesClockwise(triangles_2d);


        //From 2d to mesh in one step if we want one single mesh
        //Mesh displayMesh = _TransformBetweenDataStructures.Triangles2ToMesh(triangles_2d, useCompressedMesh: false);


        //Generate the meshes from triangles
        List<Mesh> meshes = new List<Mesh>();

        foreach (Triangle2 t in triangles_2d)
        {
            Mesh triangleMesh = Triangle2ToMesh(t);

            meshes.Add(triangleMesh);
        }


        return meshes;
    }



    //Generate circle meshes for delaunay based in 3 points in a triangle, where d is the opposite vertex
    public HashSet<Triangle2> GenerateDelaunayCircleTriangles(MyVector2 a, MyVector2 b, MyVector2 c, MyVector2 d)
    {
        //Unnormalize the points
        a = UnNormalize(a);
        b = UnNormalize(b);
        c = UnNormalize(c);
        d = UnNormalize(d);


        //Generate the triangles

        //Big circle
        MyVector2 center = _Geometry.CalculateCircleCenter(a, b, c);

        float radius = MyVector2.Distance(center, a);
        
        HashSet<Triangle2> allTriangles = _GenerateMesh.CircleHollow(center, radius, 100, 0.1f);


        //Circles showing the 4 points
        float circleMeshRadius = 0.3f;

        HashSet<Triangle2> circle_a = _GenerateMesh.Circle(a, circleMeshRadius, 10);
        HashSet<Triangle2> circle_b = _GenerateMesh.Circle(b, circleMeshRadius, 10);
        HashSet<Triangle2> circle_c = _GenerateMesh.Circle(c, circleMeshRadius, 10);
        HashSet<Triangle2> circle_d = _GenerateMesh.Circle(d, circleMeshRadius, 10);

        //Similar to List's add range
        allTriangles.UnionWith(circle_a);
        allTriangles.UnionWith(circle_b);
        allTriangles.UnionWith(circle_c);
        allTriangles.UnionWith(circle_d);


        return allTriangles;
    }



    //From Triangle2 to mesh where height is option to avoid z-fighting
    //But we can also determine height on DrawMesh()
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



    //
    // Other
    //

    public MyVector2 UnNormalize(MyVector2 p)
    {
        return normalizer.UnNormalize(p);
    }



    public void SetActivePoint(MyVector2 p)
    {
        activePoint = normalizer.UnNormalize(p);
    }



    //Generate a list with random materials
    public List<Material> GenerateRandomMaterials(int howMany)
    {
        List<Material> materials = new List<Material>();


        Random.InitState(seed);

        for (int i = 0; i < howMany; i++)
        {
            //Color the triangle
            Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);

            //float grayScale = Random.Range(0f, 1f);

            //Color color = new Color(grayScale, grayScale, grayScale, 1f);


            Material mat = new Material(Shader.Find("Unlit/Color"));

            mat.color = color;


            materials.Add(mat);
        }

        return materials;
    }



    //Take a screenshot
    public void TakeScreenshot(string name)
    {
        string path = "c:\\Nerladdat\\Temp\\" + name;
    
        ScreenCapture.CaptureScreenshot(path);
    }
}
