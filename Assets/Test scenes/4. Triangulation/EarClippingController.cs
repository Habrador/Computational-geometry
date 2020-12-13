using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;

public class EarClippingController : MonoBehaviour
{
    //The parent to the points that form the hull (should be ordered counter-clockwise)
    public Transform hullParent;
    //The parent to the points that form a single hole (should be ordered clockwise)
    public List<Transform> holeParents;

    //So we can generate these in a separate method and display them in draw gizmos 
    private HashSet<Triangle2> triangulation;

    //For visualizations
    private int howManyTriangles = 0;
    private List<Mesh> meshes;
    private List<Material> materials;



    public void GenerateTriangulation()
    {
        List<Vector3> hullVertices = GetPointsFromParent(hullParent);

        if (hullVertices == null)
        {
            Debug.Log("We have no points on the hull");

            return;
        }

        //Ear Clipping is a 2d algorithm so convert
        List<MyVector2> hullVertices_2d = hullVertices.Select(p => new MyVector2(p.x, p.z)).ToList();


        //Holes
        List<List<MyVector2>> allHoleVertices_2d = new List<List<MyVector2>>(); 

        foreach (Transform holeParentTrans in holeParents)
        {
            List<Vector3> holeVertices = GetPointsFromParent(holeParentTrans);

            List<MyVector2> holeVertices_2d = null;

            if (holeVertices != null)
            {
                holeVertices_2d = holeVertices.Select(p => new MyVector2(p.x, p.z)).ToList();

                allHoleVertices_2d.Add(holeVertices_2d);
            }
            else
            {
                Debug.Log("A hole has no points");
            }
        }



        //Normalize to range 0-1
        //The holes are always inside this shape, so dont need to take them into account when calculating the normalization values
        Normalizer2 normalizer = new Normalizer2(new List<MyVector2>(hullVertices_2d));

        List<MyVector2> hullVertices_2d_normalized = normalizer.Normalize(hullVertices_2d);

        //Normalize the holes
        List<List<MyVector2>> allHoleVertices_2d_normalized = new List<List<MyVector2>>();

        foreach (List<MyVector2> holeVertices_2d in allHoleVertices_2d)
        {
            List<MyVector2> holeVertices_2d_normalized = normalizer.Normalize(holeVertices_2d);

            allHoleVertices_2d_normalized.Add(holeVertices_2d_normalized);
        }


        //Debug.Log(hullVertices_2d_normalized.Count);

        //Triangulate
        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

        timer.Start();

        triangulation = _EarClipping.Triangulate(hullVertices_2d, allHoleVertices_2d, optimizeTriangles: false);
        //HashSet<Triangle2> triangulation_normalized = EarClipping.Triangulate(hullVertices_2d_normalized, allHoleVertices_2d_normalized);

        //Debug.Log($"Number of triangles from ear clipping: {triangulation_normalized.Count}");

        timer.Stop();

        Debug.Log($"Generated an Ear Clipping triangulation in {timer.ElapsedMilliseconds / 1000f} seconds");


        //Unnormalize
        //triangulation = HelpMethods.UnNormalize(triangulation_normalized, normalizingBox, dMax);
    }



    private void OnDrawGizmos()
    {
        DisplayTriangles();

        DisplayConnectedPoints(hullParent, Color.white);

        foreach (Transform holeParent in holeParents)
        {
            DisplayConnectedPoints(holeParent, Color.white);
        }
    }



    private void DisplayTriangles()
    {
        if (triangulation == null)
        {
            return;
        }


        //Convert from triangle to mesh
        Mesh mesh = _TransformBetweenDataStructures.Triangles2ToMesh(triangulation, false);

        TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors(mesh, 0);
        //TestAlgorithmsHelpMethods.DisplayMesh(mesh, Color.gray);
    }



    private void DisplayConnectedPoints(Transform parentToPoints, Color color)
    {
        List<Vector3> pointsOnHull = GetPointsFromParent(parentToPoints);

        if (pointsOnHull == null)
        {
            Debug.Log("We have no points on the hull");

            return;
        }

        //Debug.Log(pointsOnHull.Count);

        Gizmos.color = color;

        for (int i = 0; i < pointsOnHull.Count; i++)
        {
            Vector3 p1 = pointsOnHull[MathUtility.ClampListIndex(i - 1, pointsOnHull.Count)];
            Vector3 p2 = pointsOnHull[MathUtility.ClampListIndex(i + 0, pointsOnHull.Count)];

            //Direction is important so we should display an arrow show the order of the points
            if (i == 0)
            {
                TestAlgorithmsHelpMethods.DisplayArrow(p1, p2, 0.2f, color);
            }
            else
            {
                Gizmos.DrawLine(p1, p2);
            }

            Gizmos.DrawWireSphere(p1, 0.1f);
        }
    }



    //This makes it easier to move the points by using editor tools than identify which point is which by selecting at its gameobject
    //To make it work you have to select the gameobject this script is attached to
    public List<Transform> GetAllPoints()
    {
        List<Transform> allPoints = new List<Transform>();
    

        //Points on the hull
        List<Transform> childPointsOnHull = GetChildTransformsFromParent(hullParent);

        if (childPointsOnHull != null) allPoints.AddRange(childPointsOnHull);


        //Holes
        foreach (Transform holeParent in holeParents)
        {
            List<Transform> childPointsHole = GetChildTransformsFromParent(holeParent);

            if (childPointsHole != null) allPoints.AddRange(childPointsHole);
        }


        return allPoints;
    }



    //Get all child points to a parent transform
    public List<Vector3> GetPointsFromParent(Transform parentTrans)
    {
        if (parentTrans == null)
        {
            Debug.Log("No parent so cant get children");

            return null;
        }

        //Is not including the parent
        int children = parentTrans.childCount;

        List<Vector3> childrenPositions = new List<Vector3>();

        for (int i = 0; i < children; i++)
        {
            childrenPositions.Add(parentTrans.GetChild(i).position);
        }

        return childrenPositions;
    }

    public List<Transform> GetChildTransformsFromParent(Transform parentTrans)
    {
        if (parentTrans == null)
        {
            Debug.Log("No parent so cant get children");

            return null;
        }

        //Is not including the parent
        int children = parentTrans.childCount;

        List<Transform> childrenTransforms = new List<Transform>();

        for (int i = 0; i < children; i++)
        {
            childrenTransforms.Add(parentTrans.GetChild(i));
        }

        return childrenTransforms;
    }



    //
    // For visualization when pressing Play button
    //

    private void Start()
    {
        GenerateTriangulation();


        //To access standardized methods for visualizations 
        VisualizerController visualizerController = GetComponent<VisualizerController>();

        //Generate the meshes and materials once
        meshes = visualizerController.GenerateTriangulationMesh(triangulation, shouldUnNormalize: false);

        materials = visualizerController.GenerateRandomMaterials(meshes.Count);

        StartCoroutine(DisplayTriangleByTriangle(meshes));
    }


    private void Update()
    {
        for (int i = 0; i < howManyTriangles; i++)
        {
            Vector3 meshPos = Vector3.zero + Vector3.up;

            Graphics.DrawMesh(meshes[i], meshPos, Quaternion.identity, materials[i], 0);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            GetComponent<VisualizerController>().TakeScreenshot("ear-clipping.png");

            Debug.Log("Took screenshot");
        }

        System.GC.Collect();
    }


    private IEnumerator DisplayTriangleByTriangle(List<Mesh> meshes)
    {
        for (int i = 0; i < meshes.Count; i++)
        {  
            yield return new WaitForSeconds(1f);

            howManyTriangles += 1;
        }
    }
}
