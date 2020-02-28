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

        //CircleMeshHollow(pA);

        //LineSegmemt(pA, pB);

        //ConnectedLines(pA, pB, pC, pD);

        Arrow(pA, pB);
    }



    private void Arrow(MyVector2 pA, MyVector2 pB)
    {
        HashSet<Triangle2> triangles = GenerateMesh.Arrow(pA, pB, lineWidth: 0.2f, arrowSize: 0.6f);

        if (triangles == null)
        {
            return;
        }

        Mesh mesh = Triangles2ToMesh(triangles);

        //Display
        TestAlgorithmsHelpMethods.DisplayMesh(mesh, 0, Color.white);
    }



    private void ConnectedLines(MyVector2 pA, MyVector2 pB, MyVector2 pC, MyVector2 pD)
    {
        List<MyVector2> lines = new List<MyVector2>();

        lines.Add(pA);
        lines.Add(pB);
        lines.Add(pC);
        lines.Add(pD);

        HashSet<Triangle2> triangles = GenerateMesh.ConnectedLineSegments(lines, 0.5f, isConnected: true);

        Mesh mesh = Triangles2ToMesh(triangles);

        //Display
        TestAlgorithmsHelpMethods.DisplayMesh(mesh, 0, Color.white);
    }



    private void LineSegmemt(MyVector2 pA, MyVector2 pB)
    {
        HashSet<Triangle2> triangles = GenerateMesh.GenerateLineSegment(pA, pB, 0.2f);

        Mesh mesh = Triangles2ToMesh(triangles);

        //Display
        TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors(mesh, 0);
    }



    private void CircleMesh(MyVector2 pA)
    {
        HashSet<Triangle2> triangles = GenerateMesh.GenerateCircle(pA, radius: 1.6f, resolution: 10);

        Mesh mesh = Triangles2ToMesh(triangles);

        //Display
        TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors(mesh, 0);
    }



    private void CircleMeshHollow(MyVector2 pA)
    {
        HashSet<Triangle2> triangles = GenerateMesh.GenerateCircleHollow(pA, radius: 1.6f, resolution: 10, width: 1f);

        Mesh mesh = Triangles2ToMesh(triangles);

        //Display
        TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors(mesh, 0);
    }



    //Help method to just convert triangles to mesh
    private Mesh Triangles2ToMesh(HashSet<Triangle2> triangles)
    {
        Debug.Log(triangles.Count);

        //2d to 3d
        HashSet<Triangle3> triangles_3d = new HashSet<Triangle3>();

        foreach (Triangle2 t in triangles)
        {
            triangles_3d.Add(new Triangle3(t.p1.ToMyVector3(), t.p2.ToMyVector3(), t.p3.ToMyVector3()));
        }

        //To mesh
        Mesh mesh = TransformBetweenDataStructures.Triangle3ToMesh(triangles_3d);

        return mesh;
    }
}
