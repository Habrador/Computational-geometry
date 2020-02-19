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
        Mesh grid = GenerateMesh.GenerateGrid(width, cells);

        if (grid != null)
        {
            //But this will not display each triangle, so we don't know if the mesh is correct
            //Gizmos.DrawMesh(grid, Vector3.zero, Quaternion.identity);

            DebugResults.DisplayMesh(grid, 0);
        }
    }
}
