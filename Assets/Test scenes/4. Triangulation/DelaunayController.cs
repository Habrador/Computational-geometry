using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;

public class DelaunayController : MonoBehaviour 
{
    public int seed = 0;

    public float halfMapSize = 1f;

    public int numberOfPoints = 20;


    //Constraints

    //One constraints where the vertices are connected to form the entire constraint
    public List<Vector3> constraints;

    //Constraints by using children to a parent, which we have to drag
    //Should be sorted counter-clock-wise
    public Transform hullConstraintParent;
    //Should be sorted clock-wise
    public List<Transform> holeConstraintParents;


    //The mesh so we can generate when we press a button and display it in DrawGizmos
    Mesh triangulatedMesh;


    public void GenerateTriangulation()
    {
        
        //Get the random points
        //HashSet<Vector2> randomPoints = TestAlgorithmsHelpMethods.GenerateRandomPoints2D(seed, halfMapSize, numberOfPoints);

        //To MyVector2
        //HashSet<MyVector2> randomPoints_2d = new HashSet<MyVector2>(randomPoints.Select(x => x.ToMyVector2()));

        /*
        List<MyVector2> constraints_2d = constraints.Select(x => x.ToMyVector2()).ToList();

        //Normalize to range 0-1
        //We should use all points, including the constraints because the hole may be outside of the random points
        List<MyVector2> allPoints = new List<MyVector2>();

        allPoints.AddRange(new List<MyVector2>(points_2d));
        allPoints.AddRange(constraints_2d);

        AABB2 normalizingBox = new AABB2(new List<MyVector2>(points_2d));

        float dMax = HelpMethods.CalculateDMax(normalizingBox);

        HashSet<MyVector2> points_2d_normalized = HelpMethods.Normalize(points_2d, normalizingBox, dMax);

        List<MyVector2> constraints_2d_normalized = HelpMethods.Normalize(constraints_2d, normalizingBox, dMax);
        */


        //Hull
        List<Vector3> hullPoints = TestAlgorithmsHelpMethods.GetPointsFromParent(hullConstraintParent);

        List<MyVector2> hullPoints_2d = hullPoints.Select(x => x.ToMyVector2()).ToList(); ;

        //Holes
        HashSet<List<MyVector2>> allHolePoints_2d = new HashSet<List<MyVector2>>();

        foreach (Transform holeParent in holeConstraintParents)
        {
            List<Vector3> holePoints = TestAlgorithmsHelpMethods.GetPointsFromParent(holeParent);

            if (holePoints != null)
            {
                List<MyVector2> holePoints_2d = holePoints.Select(x => x.ToMyVector2()).ToList();

                allHolePoints_2d.Add(holePoints_2d);
            }
        }


        //Normalize to range 0-1
        //We should use all points, including the constraints because the hole may be outside of the random points
        List<MyVector2> allPoints = new List<MyVector2>();

        //allPoints.AddRange(randomPoints_2d);

        allPoints.AddRange(hullPoints_2d);

        foreach (List<MyVector2> hole in allHolePoints_2d)
        {
            allPoints.AddRange(hole);
        }

        Normalizer2 normalizer = new Normalizer2(allPoints);

        List<MyVector2> hullPoints_2d_normalized = normalizer.Normalize(hullPoints_2d);

        HashSet<List<MyVector2>> allHolePoints_2d_normalized = new HashSet<List<MyVector2>>();

        foreach (List<MyVector2> hole in allHolePoints_2d)
        {
            List<MyVector2> hole_normalized = normalizer.Normalize(hole);

            allHolePoints_2d_normalized.Add(hole_normalized);
        }



        //
        // Generate the triangulation
        //

        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

        timer.Start();

        //Algorithm 1. Delaunay by triangulate all points with some bad algorithm and then flip edges until we get a delaunay triangulation 
        //HalfEdgeData2 triangleData_normalized = _Delaunay.FlippingEdges(points_2d_normalized, new HalfEdgeData2());


        //Algorithm 2. Delaunay by inserting point-by-point while flipping edges after inserting a single point 
        //HalfEdgeData2 triangleData_normalized = _Delaunay.PointByPoint(points_2d_normalized, new HalfEdgeData2());


        //Algorithm 3. Constrained delaunay
        HalfEdgeData2 triangleData_normalized = _Delaunay.ConstrainedBySloan(null, hullPoints_2d_normalized, allHolePoints_2d_normalized, shouldRemoveTriangles: true, new HalfEdgeData2());

        timer.Stop();

        Debug.Log($"Generated a delaunay triangulation in {timer.ElapsedMilliseconds / 1000f} seconds");


        //UnNormalize
        HalfEdgeData2 triangleData = normalizer.UnNormalize(triangleData_normalized);

        //From half-edge to triangle
        HashSet<Triangle2> triangles_2d = _TransformBetweenDataStructures.HalfEdge2ToTriangle2(triangleData);

        //From triangulation to mesh

        //Make sure the triangles have the correct orientation
        triangles_2d = HelpMethods.OrientTrianglesClockwise(triangles_2d);

        //From 2d to 3d
        HashSet<Triangle3<MyVector3>> triangles_3d = new HashSet<Triangle3<MyVector3>>();

        foreach (Triangle2 t in triangles_2d)
        {
            triangles_3d.Add(new Triangle3<MyVector3>(t.p1.ToMyVector3_Yis3D(), t.p2.ToMyVector3_Yis3D(), t.p3.ToMyVector3_Yis3D()));
        }

        triangulatedMesh = _TransformBetweenDataStructures.Triangle3ToCompressedMesh(triangles_3d);
    }



    private void OnDrawGizmos()
    {
        if (triangulatedMesh != null)
        {
            TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors(triangulatedMesh, seed);
        }


        //Display the obstacles
        if (constraints != null)
        {
            //DebugResults.DisplayConnectedPoints(obstacle, Color.black);
        }


        //Display drag constraints
        DisplayDragConstraints();
    }



    private void DisplayDragConstraints()
    {
        if (hullConstraintParent != null)
        {
            List<Vector3> points = TestAlgorithmsHelpMethods.GetPointsFromParent(hullConstraintParent);

            TestAlgorithmsHelpMethods.DisplayConnectedPoints(points, Color.white, true);
        }

        if (holeConstraintParents != null)
        {
            foreach (Transform holeParent in holeConstraintParents)
            {
                List<Vector3> points = TestAlgorithmsHelpMethods.GetPointsFromParent(holeParent);

                TestAlgorithmsHelpMethods.DisplayConnectedPoints(points, Color.white, true);
            }
        }
    }
}
