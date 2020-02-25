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



    public void TriangulateThePoints()
    {
        //Get points to triangulate

        //Random points
        points = TestAlgorithmsHelpMethods.GenerateRandomPoints(seed, mapSize, numberOfPoints);

        //Points from a plane mesh to test colinear points
        //points = TestAlgorithmsHelpMethods.GeneratePointsFromPlane(planeTrans);

        //3d to 2d
        HashSet<MyVector2> points_2d = new HashSet<MyVector2>();

        foreach (Vector3 v in points)
        {
            points_2d.Add(v.ToMyVector2());
        }


        //
        // Triangulate the points
        //

        //Method 1 - sort the points and then add triangles by checking which edge is visible to that point
        HashSet<Triangle2> triangles_2d = _TriangulatePoints.IncrementalTriangulation(points_2d);

        //Method 2 - triangulate the convex polygon, then add the rest of the points one-by-one
        //The old triangle the point ends up in is split into tree new triangles
        //HashSet<Triangle2> triangles_2d = _TriangulatePoints.TriangleSplitting(points_2d);

        //Method 3 - triangulate the convex hull of the points
        //This means that we first need to find the points on the convex hull
        //List<MyVector2> pointsOnHull = _ConvexHull.JarvisMarch(points_2d); 

        //No colinear points
        //HashSet<Triangle2> triangles_2d = _TriangulatePoints.PointsOnConvexHull(pointsOnHull);

        //Colinear points
        //HashSet<Triangle2> triangles_2d = _TriangulatePoints.PointsOnConvexHull(pointsOnHull, planeTrans.position.ToMyVector2());



        //Display
        Debug.Log("Number of triangles: " + triangles_2d.Count);

        if (triangles_2d != null)
        {
            //Make sure the triangles have the correct orientation
            triangles_2d = HelpMethods.OrientTrianglesClockwise(triangles_2d);

            //From 2d to 3d
            HashSet<Triangle3> triangles_3d = new HashSet<Triangle3>();

            foreach (Triangle2 t in triangles_2d)
            {
                triangles_3d.Add(new Triangle3(t.p1.ToMyVector3(), t.p2.ToMyVector3(), t.p3.ToMyVector3()));
            }

            triangulatedMesh = TransformBetweenDataStructures.Triangle3ToCompressedMesh(triangles_3d);
        }
    }



    private void OnDrawGizmos()
    {
        if (triangulatedMesh != null)
        {
            //Display the triangles with a random color
            TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors(triangulatedMesh, seed);

            //Display the points
            TestAlgorithmsHelpMethods.DisplayPoints(points, 0.2f, Color.black);
        }
    }
}
