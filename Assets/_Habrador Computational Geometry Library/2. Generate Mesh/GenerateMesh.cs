using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class GenerateMesh
    {
        //Generate a square grid where each cell has two triangles
        public static Mesh GenerateGrid(float width, int cells)
        {
            Mesh grid = GridMesh.GenerateGrid(width, cells);

            return grid;
        }
    }
}
