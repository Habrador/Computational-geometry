using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class GenerateMesh
    {
        //Generate a square grid where each cell has two triangles
        public static HashSet<Triangle2> GenerateGrid(float width, int cells)
        {
            HashSet<Triangle2> grid = MeshGrid.GenerateGrid(width, cells);

            return grid;
        }
    }
}
