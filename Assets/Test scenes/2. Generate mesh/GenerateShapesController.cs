using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

public class GenerateShapesController : MonoBehaviour
{
    public Transform pointATrans;
    public Transform pointBTrans;
    public Transform pointCTrans;
    public Transform pointDTrans;



    private void OnDrawGizmos()
    {
        Vector3 pA_3d = pointATrans.position;
        Vector3 pB_3d = pointBTrans.position;
        Vector3 pC_3d = pointCTrans.position;
        Vector3 pD_3d = pointDTrans.position;

        MyVector2 pA = pA_3d.ToMyVector2();
        MyVector2 pB = pB_3d.ToMyVector2();
        MyVector2 pC = pC_3d.ToMyVector2();
        MyVector2 pD = pD_3d.ToMyVector2();


        //CircleMesh(pA);

        //CircleMeshHollow(pB);

        //LineSegmemt(pA, pB);

        ConnectedLines(pA, pB, pC, pD);

        //Arrow(pA, pB);
    }



    private void Arrow(MyVector2 pA, MyVector2 pB)
    {
        HashSet<Triangle2> triangles = _GenerateMesh.Arrow(pA, pB, lineWidth: 0.2f, arrowSize: 0.6f);

        if (triangles == null)
        {
            return;
        }

        Mesh mesh = _TransformBetweenDataStructures.Triangles2ToMesh(triangles, useCompressedMesh: false);

        //Display
        TestAlgorithmsHelpMethods.DisplayMesh(mesh, Color.white);
    }



    private void ConnectedLines(MyVector2 pA, MyVector2 pB, MyVector2 pC, MyVector2 pD)
    {
        List<MyVector2> lines = new List<MyVector2>();

        lines.Add(pA);
        lines.Add(pB);
        lines.Add(pC);
        lines.Add(pD);

        HashSet<Triangle2> triangles = _GenerateMesh.ConnectedLineSegments(lines, 0.5f, isConnected: true);

        Mesh mesh = _TransformBetweenDataStructures.Triangles2ToMesh(triangles, useCompressedMesh: false);

        //Display
        TestAlgorithmsHelpMethods.DisplayMesh(mesh, Color.white);
    }



    private void LineSegmemt(MyVector2 pA, MyVector2 pB)
    {
        HashSet<Triangle2> triangles = _GenerateMesh.LineSegment(pA, pB, 0.2f);

        Mesh mesh = _TransformBetweenDataStructures.Triangles2ToMesh(triangles, useCompressedMesh: false);

        //Display
        TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors(mesh, 0);
    }



    private void CircleMesh(MyVector2 pA)
    {
        HashSet<Triangle2> triangles = _GenerateMesh.Circle(pA, radius: 1.6f, resolution: 30);

        Mesh mesh = _TransformBetweenDataStructures.Triangles2ToMesh(triangles, useCompressedMesh: false);

        //Display
        //TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors(mesh, 0);
        TestAlgorithmsHelpMethods.DisplayMesh(mesh, Color.white);
    }



    private void CircleMeshHollow(MyVector2 pA)
    {
        HashSet<Triangle2> triangles = _GenerateMesh.CircleHollow(pA, innerRadius: 3f, resolution: 30, width: 1f);

        Mesh mesh = _TransformBetweenDataStructures.Triangles2ToMesh(triangles, useCompressedMesh: false);

        //Display
        //TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors(mesh, 0);
        TestAlgorithmsHelpMethods.DisplayMesh(mesh, Color.white);
    }
}
