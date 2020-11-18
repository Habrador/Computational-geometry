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
        HashSet<Triangle2> grid = _GenerateMesh.GenerateGrid(width, cells);

        if (grid != null)
        {
            //But this will not display each triangle, so we don't know if the mesh is correct
            //Gizmos.DrawMesh(grid, Vector3.zero, Quaternion.identity);

            //Convert the triangles to a mesh

            //2d to 3d
            HashSet<Triangle3> grid_3d = new HashSet<Triangle3>();

            foreach (Triangle2 t in grid)
            {
                Triangle3 t_3d = new Triangle3(t.p1.ToMyVector3_Yis3D(), t.p2.ToMyVector3_Yis3D(), t.p3.ToMyVector3_Yis3D());

                grid_3d.Add(t_3d);
            }

            //Triangle to mesh
            //Will also test that the triangle->mesh is working
            //Mesh meshGrid = TransformBetweenDataStructures.Triangle3ToCompressedMesh(grid_3d);

            Mesh meshGrid = _TransformBetweenDataStructures.Triangle3ToMesh(grid_3d);

            TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors(meshGrid, 0);
        }
    }
}
