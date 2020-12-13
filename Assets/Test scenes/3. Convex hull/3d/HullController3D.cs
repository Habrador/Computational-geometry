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



    void OnDrawGizmosSelected()
	{
        //Get random points in 3d space
        HashSet<Vector3> points_Unity = TestAlgorithmsHelpMethods.GenerateRandomPoints3D(seed, halfMapSize, numberOfPoints);

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

        HalfEdgeData3 convexHull_normalized = _ConvexHull.Iterative_3D(points_normalized, normalizer);

        timer.Stop();

        Debug.Log($"Generated a 3d convex hull in {timer.ElapsedMilliseconds / 1000f} seconds");



        //
        // Display
        //

        //Points
        TestAlgorithmsHelpMethods.DisplayPoints(points_Unity, 0.01f, Color.black);


        if (convexHull_normalized == null)
        {
            Debug.Log("Convex hull is null");

            return;
        }

        //To unity mesh
        //UnNormalize
        HalfEdgeData3 convexHull = normalizer.UnNormalize(convexHull_normalized);

        Mesh convexHullMesh = convexHull.ConvertToUnityMesh("convex hull", shareVertices: false, generateNormals: false);

        //Hull
        TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors(convexHullMesh, 0);
	}

    

    //
    // Test points
    //
    private HashSet<MyVector3> GetCubeTestPoints()
    {
        HashSet<MyVector3> cube = new HashSet<MyVector3>();

        cube.Add(new MyVector3(0f, 0f, 0f));
        cube.Add(new MyVector3(0f, 1f, 0f));
        cube.Add(new MyVector3(1f, 1f, 0f));
        cube.Add(new MyVector3(1f, 0f, 0f));
        cube.Add(new MyVector3(0f, 0f, 1f));
        cube.Add(new MyVector3(0f, 1f, 1f));
        cube.Add(new MyVector3(1f, 1f, 1f));
        cube.Add(new MyVector3(1f, 0f, 1f));

        return cube;
    }
}
