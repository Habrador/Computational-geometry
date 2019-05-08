using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Boolean operations on triangles in 2d space with a constrained delaunay algorithm
public class CutTrianglesController : MonoBehaviour 
{
    public GameObject mesh_1_Obj;
    public GameObject mesh_2_Obj;

    //Needed if we want random colors on the triangles when displaying them
    public int seed;



    void OnDrawGizmos() 
	{
        Mesh mesh1_old = mesh_1_Obj.GetComponent<MeshFilter>().sharedMesh;
        Mesh mesh2_old = mesh_2_Obj.GetComponent<MeshFilter>().sharedMesh;

        //Clone the data so we always start with a clean mesh
        Mesh mesh1 = CloneMesh(mesh1_old);
        Mesh mesh2 = CloneMesh(mesh2_old);


        //From local to global
        mesh1.vertices = TransformArrayFromLocalToGlobal(mesh1.vertices, mesh_1_Obj.transform);
        mesh2.vertices = TransformArrayFromLocalToGlobal(mesh2.vertices, mesh_2_Obj.transform);


        //Cut the mesh by using a constrained delaunay algorithm
        //Mesh outputMesh = Boolean2DTriangles.CutTriangles(mesh1, mesh2);
        //Mesh outputMesh = BooleanClippingAlgorithm.CutTriangles(mesh1, mesh2);


        //From global to local
        //outputMesh.vertices = TransformArrayFromLocalToGlobal(outputMesh.vertices, mesh_1_Obj.transform);


        //Display the mesh
        //Hide the original mesh which we cant modify without breaking everything
        mesh_1_Obj.GetComponent<MeshRenderer>().enabled = false;

        //if (outputMesh != null)
        //{
        //    DisplayMesh(outputMesh);
        //}
    }



    //Clone a mesh
    private Mesh CloneMesh(Mesh oldMesh)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = oldMesh.vertices.Clone() as Vector3[];

        int[] triangles = oldMesh.triangles.Clone() as int[];

        mesh.vertices = vertices;

        mesh.triangles = triangles;

        return mesh;
    }



    //Transform all vertices from local to global
    private Vector3[] TransformArrayFromLocalToGlobal(Vector3[] array, Transform trans)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = trans.TransformPoint(array[i]);
        }

        return array;
    }



    //Transform all vertices from global to local
    private Vector3[] TransformArrayFromGlobalToLocal(Vector3[] array, Transform trans)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = trans.InverseTransformPoint(array[i]);
        }

        return array;
    }



    //Display mesh for debugging
    private void DisplayMesh(Mesh mesh)
    {
        //The vertices are in global space
        Vector3[] vertices = mesh.vertices;

        Debug.Log("Vertices: " + vertices.Length + " Triangles: " + mesh.triangles.Length / 3);

        int[] triangles = mesh.triangles;

        float sphereSize = 0.1f;

        //Show vertices
        Gizmos.color = Color.black;

        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], sphereSize);
        }

        //Show triangles
        Random.InitState(seed);
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v1 = vertices[triangles[i + 0]];
            Vector3 v2 = vertices[triangles[i + 1]];
            Vector3 v3 = vertices[triangles[i + 2]];

            Mesh triangleMesh = new Mesh();

            triangleMesh.vertices = new Vector3[] { v1, v2, v3 };
            triangleMesh.triangles = new int[] { 0, 1, 2 };

            triangleMesh.RecalculateNormals();

            //Give it random color
            Gizmos.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);

            Gizmos.DrawMesh(triangleMesh);
        }
    }
}
