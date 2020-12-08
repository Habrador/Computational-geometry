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

        DisplayMeshRendererAABB(mr);
    }


    //Renderer.bounds are AABB in world space
    private void DisplayMeshRendererAABB(MeshRenderer mr)
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
    //Is taking rotation into account, so we get an oriented bounding box
    private void DisplayMeshBoundingBox(Mesh mesh)
    {
        Box orientedBB = new Box(mesh, transform);


        //Display corners
        //HashSet<MyVector3> corners = orientedBB.GetCorners();
        
        //Gizmos.color = Color.blue;

        //foreach (MyVector3 v in corners)
        //{
        //    Gizmos.DrawWireSphere(v.ToVector3(), 0.1f);
        //}
        


        //Generate the AABB which should give the same result as when using the MeshRenderer
        //AABB3 aabb = new AABB3(new List<MyVector3>(corners));



        //Display the edges
        List<Edge3> edges = orientedBB.GetEdges();

        Gizmos.color = Color.blue;

        foreach (Edge3 e in edges)
        {
            Gizmos.DrawLine(e.p1.ToVector3(), e.p2.ToVector3());
        }
    }
}
