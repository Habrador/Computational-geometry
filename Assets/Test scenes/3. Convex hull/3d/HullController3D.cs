using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;


//Generates a convex hull in 3d space from a set of points
public class HullController3D : MonoBehaviour
{
    public int numberOfPoints = 10;

    public float halfMapSize = 1f;

    public int seed;

    public bool removeUnwantedTriangles;

    public MeshFilter constructHullFromThisMesh;

    //Display the hull on this go
    public MeshFilter meshFilter;



    //Called from editor script
    public void GenerateHull()
	{
        //Get random points in 3d space
        HashSet<Vector3> points_Unity = TestAlgorithmsHelpMethods.GenerateRandomPoints3D(seed, halfMapSize, numberOfPoints);

        //HashSet<Vector3> points_Unity = GetCubeTestPoints();

        //Points from a mesh
        /*
        Transform meshTrans = constructHullFromThisMesh.transform;

        List<Vector3> vertices = new List<Vector3>(constructHullFromThisMesh.sharedMesh.vertices);

        //Local to global space
        List<Vector3> verticesGlobal = vertices.Select(x => meshTrans.TransformPoint(x)).ToList();

        HashSet<Vector3> points_Unity = new HashSet<Vector3>(verticesGlobal);
        */

        //To stress-test these algorithms, generate points on a sphere because all of those should be on the hull
        //HashSet<Vector3> points_Unity = TestAlgorithmsHelpMethods.GenerateRandomPointsOnSphere(seed, radius: 1f, numberOfPoints);

        //To MyVector3
        HashSet<MyVector3> points = new HashSet<MyVector3>(points_Unity.Select(x => x.ToMyVector3()));

        //Normalize
        Normalizer3 normalizer = new Normalizer3(new List<MyVector3>(points));

        HashSet<MyVector3> points_normalized = normalizer.Normalize(points);



        //
        // Generate the convex hull
        //

        //Algorithm 1. Iterative algorithm

        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

        timer.Start();

        HalfEdgeData3 convexHull_normalized = _ConvexHull.Iterative_3D(points_normalized, removeUnwantedTriangles, normalizer);

        timer.Stop();

        Debug.Log($"Generated a 3d convex hull in {timer.ElapsedMilliseconds / 1000f} seconds with {convexHull_normalized.faces.Count} triangles");



        //
        // Display
        //

        //Points
        //TestAlgorithmsHelpMethods.DisplayPoints(points_Unity, 0.01f, Color.black);


        //Hull mesh
        if (convexHull_normalized != null)
        {
            HalfEdgeData3 convexHull = normalizer.UnNormalize(convexHull_normalized);

            MyMesh myMesh = convexHull.ConvertToMyMesh("convex hull", MyMesh.MeshStyle.HardEdges);

            //To unity mesh
            Mesh convexHullMesh = myMesh.ConvertToUnityMesh(generateNormals: false, myMesh.meshName);

            //Using gizmos to display mesh in 3d space gives a bad result
            //TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors(convexHullMesh, 0);

            //Better to add it to a gameobject
            //Use Shaded Wireframe to see the triangles
            meshFilter.mesh = convexHullMesh;

            //Points on the hull
            //These are shining thorugh the mesh
            //TestAlgorithmsHelpMethods.DisplayMeshCorners(convexHullMesh, 0.01f, Color.black);
        }
    }





    //
    // Test points
    //
    private HashSet<Vector3> GetCubeTestPoints()
    {
        HashSet<Vector3> cube = new HashSet<Vector3>();

        cube.Add(new Vector3(0f, 0f, 0f));
        cube.Add(new Vector3(0f, 1f, 0f));
        cube.Add(new Vector3(1f, 1f, 0f));
        cube.Add(new Vector3(1f, 0f, 0f));
        cube.Add(new Vector3(0f, 0f, 1f));
        cube.Add(new Vector3(0f, 1f, 1f));
        cube.Add(new Vector3(1f, 1f, 1f));
        cube.Add(new Vector3(1f, 0f, 1f));

        //Split sides
        cube.Add(new Vector3(1f, 0.5f, 1f));

        return cube;
    }
}
