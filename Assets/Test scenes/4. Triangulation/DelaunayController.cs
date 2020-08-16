using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

public class DelaunayController : MonoBehaviour 
{
    public int seed = 0;

    public float halfMapSize = 10f;

    public int numberOfPoints = 10;

    //One constraints where the vertices are connected to form the entire constraint
    public List<Vector3> constraints;

    Mesh triangulatedMesh;



    public void GenererateTriangulation()
    {
        //Get the random points
        HashSet<Vector3> points = TestAlgorithmsHelpMethods.GenerateRandomPoints(seed, halfMapSize, numberOfPoints);


        //From 3d to 2d
        HashSet<MyVector2> points_2d = new HashSet<MyVector2>();
        
        foreach (Vector3 v in points)
        {
            points_2d.Add(v.ToMyVector2());
        }

        List<MyVector2> constraints_2d = new List<MyVector2>();

        foreach (Vector3 v in constraints)
        {
            constraints_2d.Add(v.ToMyVector2());
        }

        //Normalize to range 0-1
        //We should use all points, including the constraints
        List<MyVector2> allPoints = new List<MyVector2>();

        allPoints.AddRange(new List<MyVector2>(points_2d));
        allPoints.AddRange(constraints_2d);

        AABB2 normalizingBox = new AABB2(new List<MyVector2>(points_2d));

        float dMax = HelpMethods.CalculateDMax(normalizingBox);

        HashSet<MyVector2> points_2d_normalized = HelpMethods.Normalize(points_2d, normalizingBox, dMax);

        List<MyVector2> constraints_2d_normalized = HelpMethods.Normalize(constraints_2d, normalizingBox, dMax);



        //
        // Generate the triangulation
        //

        //Algorithm 1. Delaunay by triangulate all points with some bad algorithm and then flip edges until we get a delaunay triangulation 
        //HalfEdgeData2 triangleData_normalized = _Delaunay.FlippingEdges(points_2d_normalized, new HalfEdgeData2());


        //Algorithm 2. Delaunay by inserting point-by-point while flipping edges after inserting a single point 
        //HalfEdgeData2 triangleData_normalized = _Delaunay.PointByPoint(points_2d_normalized, new HalfEdgeData2());


        //Algorithm 3. Constrained delaunay
        HalfEdgeData2 triangleData_normalized = _Delaunay.ConstrainedBySloan(points_2d_normalized, constraints_2d_normalized, shouldRemoveTriangles: true, new HalfEdgeData2());



        //UnNormalize
        HalfEdgeData2 triangleData = HelpMethods.UnNormalize(triangleData_normalized, normalizingBox, dMax);

        //From half-edge to triangle
        HashSet<Triangle2> triangles_2d = _TransformBetweenDataStructures.HalfEdge2ToTriangle2(triangleData);

        //From triangulation to mesh

        //Make sure the triangles have the correct orientation
        triangles_2d = HelpMethods.OrientTrianglesClockwise(triangles_2d);

        //From 2d to 3d
        HashSet<Triangle3> triangles_3d = new HashSet<Triangle3>();

        foreach (Triangle2 t in triangles_2d)
        {
            triangles_3d.Add(new Triangle3(t.p1.ToMyVector3(), t.p2.ToMyVector3(), t.p3.ToMyVector3()));
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
    }
}
