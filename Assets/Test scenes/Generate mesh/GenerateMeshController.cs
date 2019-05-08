using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

public class GenerateMeshController : MonoBehaviour 
{
    public float width;

    public int cells;

    private void OnDrawGizmos()
    {
        Mesh chunk = GenerateMesh.GenerateChunk(width, cells);

        if (chunk != null)
        {
            Gizmos.DrawMesh(chunk, Vector3.zero, Quaternion.identity);
        }
    }
}
