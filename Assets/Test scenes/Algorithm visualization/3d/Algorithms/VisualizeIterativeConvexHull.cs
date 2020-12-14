using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

public class VisualizeIterativeConvexHull : MonoBehaviour
{
    private VisualizerController3D controller;


    //These points should be normalized
    public void StartVisualizer(HashSet<MyVector3> points)
    {
        controller = GetComponent<VisualizerController3D>();

        HalfEdgeData3 convexHull = new HalfEdgeData3();

        //Generate the first tertahedron
        IterativeHullAlgorithm3D.BuildFirstTetrahedron(points, convexHull);

        //Main visualization algorithm
        StartCoroutine(GenerateHull(points, convexHull));
    }



    private IEnumerator GenerateHull(HashSet<MyVector3> points, HalfEdgeData3 convexHull)
    {
        //Display what we have so far
        controller.DisplayMesh(convexHull);

        yield return new WaitForSeconds(10f);

        Debug.Log("Waited 10 seconds");
    
        yield return null;
    }
}
