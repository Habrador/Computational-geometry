using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;

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


        //Generate the convex hull

        //Iterative algorithm
        HalfEdgeData3 convexHull = _ConvexHull.Iterative_3D(points);


        //Display the points
        TestAlgorithmsHelpMethods.DisplayPoints(points_Unity, 0.01f, Color.black);
	}

    
    
}
