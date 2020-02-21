using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Generate a square grid where each cell consists of two triangles
    //The coordinate system starts in the middle
    public static class GridMesh
    {
        //The grid is always a square
        //witdh - the width of the entire chunk
        //cells - the number of cells in one row
        public static Mesh GenerateGrid(float width, int cells)
        {
            //We cant have a grid with 0 cells
            if (cells <= 0)
            {
                Debug.Log("The grid needs at least one cell");
            
                return null;
            }

            //The width has to be greater than 0
            if (width <= 0f)
            {
                Debug.Log("The grid needs a positive width");

                return null;
            }



            //The number of vertices in one row is always cells + 1
            int verticesInOneRow = cells + 1;

            //The width of one cell
            float cellWidth = width / (float)cells;

            //What's the half width of the grid?
            float halfWidth = width * 0.5f;

            //Generate vertices
            List<Vector3> vertices = new List<Vector3>();

            for (int i = 0; i < verticesInOneRow; i++)
            {
                for (int j = 0; j < verticesInOneRow; j++)
                {
                    Vector3 vertexPos = new Vector3(-halfWidth + i * cellWidth, 0f, -halfWidth + j * cellWidth);

                    vertices.Add(vertexPos);
                }
            }


            //Generate triangles by using the 1d list as if it was 2d
            List<int> triangles = new List<int>();

            for (int i = 0; i < verticesInOneRow; i++)
            {
                for (int j = 0; j < verticesInOneRow; j++)
                {
                    //We cant build triangles from the first row/column
                    if (i == 0 || j == 0)
                    {
                        continue;
                    }
                    else
                    {
                        //Four vertices
                        int BL = ConvertArrayPos(verticesInOneRow, i - 1, j - 1);
                        int BR = ConvertArrayPos(verticesInOneRow, i - 0, j - 1);
                        int TL = ConvertArrayPos(verticesInOneRow, i - 1, j - 0);
                        int TR = ConvertArrayPos(verticesInOneRow, i - 0, j - 0);

                        //Triangle 1
                        triangles.Add(TR);
                        triangles.Add(BL);
                        triangles.Add(TL);

                        //Triangle 2
                        triangles.Add(TR);
                        triangles.Add(BR);
                        triangles.Add(BL);
                    }
                }
            }


            //Generate the mesh
            Mesh mesh = new Mesh();

            mesh.name = "Grid";

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();


            return mesh;
        }



        //Convert from 2d array to 1d array
        private static int ConvertArrayPos(int width, int row, int col)
        {
            int arrayPos = width * row + col;

            return arrayPos;
        }
    }
}
