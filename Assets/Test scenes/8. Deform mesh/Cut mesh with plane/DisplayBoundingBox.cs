using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

//Attach this to go with mesh and it should display its bounding box
public class DisplayBoundingBox : MonoBehaviour
{

    void OnDrawGizmosSelected()
	{
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        if (meshFilter == null)
        {
            Debug.Log("You need a mesh filter");

            return;
        }

        Mesh mesh = meshFilter.sharedMesh;

        if (mesh == null)
        {
            Debug.Log("You need a mesh");

            return;
        }

        //mesh.bounds is in local space
        DisplayMeshBoundingBox(mesh);


        //Renderer.bounds in in world space, so this becomes AABB
        MeshRenderer mr = GetComponent<MeshRenderer>();

        if (mr == null)
        {
            Debug.Log("You need a mesh renderer");

            return;
        }

        DisplayMeshAABB(mr);
    }


    //Renderer.bounds are AABB in world space
    private void DisplayMeshAABB(MeshRenderer mr)
    {
        Bounds bounds = mr.bounds;

        //Vector3 halfSize = bounds.extents;

        //Vector3 top = bounds.center + Vector3.up * halfSize.y;
        //Vector3 bottom = bounds.center - Vector3.up * halfSize.y;


        //Vector3 topFR = top + Vector3.forward * halfSize.z + Vector3.right * halfSize.x;
        //Vector3 topFL = top + Vector3.forward * halfSize.z + Vector3.left * halfSize.x;
        //Vector3 topBR = top - Vector3.forward * halfSize.z + Vector3.right * halfSize.x;
        //Vector3 topBL = top - Vector3.forward * halfSize.z + Vector3.left * halfSize.x;

        //Vector3 bottomFR = bottom + Vector3.forward * halfSize.z + Vector3.right * halfSize.x;
        //Vector3 bottomFL = bottom + Vector3.forward * halfSize.z + Vector3.left * halfSize.x;
        //Vector3 bottomBR = bottom - Vector3.forward * halfSize.z + Vector3.right * halfSize.x;
        //Vector3 bottomBL = bottom - Vector3.forward * halfSize.z + Vector3.left * halfSize.x;


        AABB3 aabb = new AABB3(bounds);


        Gizmos.color = Color.black;

        List<Edge3> edges = aabb.GetEdges();

        foreach (Edge3 e in edges)
        {
            Gizmos.DrawLine(e.p1.ToVector3(), e.p2.ToVector3());
        }
    }



    //Mesh.Bounds are AABB in local space
    //Is taking rotation into account
    private void DisplayMeshBoundingBox(Mesh mesh)
    {
        Bounds bounds = mesh.bounds;

        Vector3 halfSize = bounds.extents;

        Vector3 xVec = Vector3.right * halfSize.x;
        Vector3 yVec = Vector3.up * halfSize.y;
        Vector3 zVec = Vector3.forward * halfSize.z;

        Vector3 top = bounds.center + yVec;
        Vector3 bottom = bounds.center - yVec;

        Vector3 topFR = top + zVec + xVec;
        Vector3 topFL = top + zVec - xVec;
        Vector3 topBR = top - zVec + xVec;
        Vector3 topBL = top - zVec - xVec;

        Vector3 bottomFR = bottom + zVec + xVec;
        Vector3 bottomFL = bottom + zVec - xVec;
        Vector3 bottomBR = bottom - zVec + xVec;
        Vector3 bottomBL = bottom - zVec - xVec;


        //Local to world space
        topFR = transform.TransformPoint(topFR);
        topFL = transform.TransformPoint(topFL);
        topBR = transform.TransformPoint(topBR);
        topBL = transform.TransformPoint(topBL);

        bottomFR = transform.TransformPoint(bottomFR);
        bottomFL = transform.TransformPoint(bottomFL);
        bottomBR = transform.TransformPoint(bottomBR);
        bottomBL = transform.TransformPoint(bottomBL);


        Gizmos.color = Color.black;

        Gizmos.DrawWireSphere(topFR, 0.1f);
        Gizmos.DrawWireSphere(bottomFR, 0.1f);



        //Generate the AABB 
        List<MyVector3> points = new List<MyVector3>()
        {
            topFR.ToMyVector3(),
            topFL.ToMyVector3(),
            topBR.ToMyVector3(),
            topBL.ToMyVector3(),

            bottomFR.ToMyVector3(),
            bottomFL.ToMyVector3(),
            bottomBR.ToMyVector3(),
            bottomBL.ToMyVector3(),
        };

        //This aabb should be the same as if we had used meshrender.bounds
        AABB3 aabb = new AABB3(points);

        Gizmos.color = Color.blue;

        List<Edge3> edges = aabb.GetEdges();

        foreach (Edge3 e in edges)
        {
            Gizmos.DrawLine(e.p1.ToVector3(), e.p2.ToVector3());
        }
    }
}
