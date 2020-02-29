using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

public class TriangulatePointsController : MonoBehaviour 
{
    public int seed;
    //How many points do we want to triangulate?
    public int numberOfPoints = 25;
    //The size of the map
    public float mapSize = 20f;
    //A plane to test the triangulation algorithms for edge cases
    public Transform planeTrans;

    private Mesh triangulatedMesh;

    private HashSet<Vector3> points;

    private List<MyVector2> pointsOnHull;


    public void TriangulateThePoints()
    {
        if (pointsOnHull != null)
        {
            pointsOnHull.Clear();
        }

        //
        // Get points to triangulate
        //

        //Random points
        points = TestAlgorithmsHelpMethods.GenerateRandomPoints(seed, mapSize, numberOfPoints);

        //Points from a plane mesh to test colinear points
        //points = TestAlgorithmsHelpMethods.GeneratePointsFromPlane(planeTrans);



        //
        // Prepare the points
        //

        //3d to 2d
        HashSet<MyVector2> points_2d = new HashSet<MyVector2>();

        foreach (Vector3 v in points)
        {
            points_2d.Add(v.ToMyVector2());
        }

        //Normalize to range 0-1
        AABB normalizingBox = HelpMethods.GetAABB(new List<MyVector2>(points_2d));

        float dMax = HelpMethods.CalculateDMax(normalizingBox);

        HashSet<MyVector2> points_2d_normalized = HelpMethods.Normalize(points_2d, normalizingBox, dMax);



        //
        // Triangulate points on convex hull and points inside of convex hull
        //

        //Method 1 - sort the points and then add triangles by checking which edge is visible to that point
        HashSet<Triangle2> triangles_2d_normalized = _TriangulatePoints.VisibleEdgesTriangulation(points_2d_normalized);

        //Method 2 - triangulate the convex polygon, then add the rest of the points one-by-one
        //The old triangle the point ends up in is split into tree new triangles
        //HashSet<Triangle2> triangles_2d_normalized = _TriangulatePoints.TriangleSplitting(points_2d_normalized);


        //
        // Triangulate points on convex hull
        //

        //First find the convex hull of the points
        //This means that we first need to find the points on the convex hull
        //List<MyVector2> pointsOnHull_normalized = _ConvexHull.JarvisMarch(points_2d_normalized);

        //Method 1. Find the colinear points while triangulating the hull
        //HashSet<Triangle2> triangles_2d_normalized = _TriangulatePoints.PointsOnConvexHull(pointsOnHull_normalized);

        //Method 2. Add a point inside of the convex hull to deal with colinear points
        //MyVector2 insidePoint = HelpMethods.NormalizePoint(planeTrans.position.ToMyVector2(), normalizingBox, dMax);

        //HashSet<Triangle2> triangles_2d_normalized = _TriangulatePoints.PointsOnConvexHull(pointsOnHull_normalized, insidePoint);



        //
        // Display
        //
        Debug.Log("Number of triangles: " + triangles_2d_normalized.Count);
        /*
        if (pointsOnHull_normalized != null)
        {
            pointsOnHull = HelpMethods.UnNormalize(pointsOnHull_normalized, normalizingBox, dMax);
        }
        */
        if (triangles_2d_normalized != null)
        {
            //Unnormalized the triangles
            HashSet<Triangle2> triangles_2d = HelpMethods.UnNormalize(triangles_2d_normalized, normalizingBox, dMax);
        
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
    }



    private void OnDrawGizmos()
    {
        if (triangulatedMesh != null)
        {
            //Display the triangles with a random color
            TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors(triangulatedMesh, seed);

            //Display the points
            TestAlgorithmsHelpMethods.DisplayPoints(points, 0.1f, Color.black);

            //Display the points on the hull
            if (pointsOnHull != null && pointsOnHull.Count > 0)
            {
                HashSet<Vector3> pointsOnHull_3d = new HashSet<Vector3>();

                foreach (MyVector2 p in pointsOnHull)
                {
                    pointsOnHull_3d.Add(p.ToVector3());
                }
            
                TestAlgorithmsHelpMethods.DisplayPoints(pointsOnHull_3d, 0.3f, Color.black);
            }
        }
    }
}
