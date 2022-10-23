using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry.MeshAlgorithms
{
    //Generate a square grid where each cell consists of two triangles
    //The coordinate system starts in the middle
    public static class Grid
    {
        //The grid is always a square
        //witdh - the width of the entire chunk
        //cells - the number of cells in one row
        public static HashSet<Triangle2> GenerateGrid(float width, int cells)
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
            List<MyVector2> vertices = new List<MyVector2>();

            for (int i = 0; i < verticesInOneRow; i++)
            {
                for (int j = 0; j < verticesInOneRow; j++)
                {
                    MyVector2 vertexPos = new MyVector2(-halfWidth + i * cellWidth, -halfWidth + j * cellWidth);

                    vertices.Add(vertexPos);
                }
            }


            //Generate triangles by using the 1d list as if it was 2d
            //List<int> triangles = new List<int>();

            HashSet<Triangle2> triangles = new HashSet<Triangle2>();

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
                        int BL_pos = ConvertArrayPos(verticesInOneRow, i - 1, j - 1);
                        int BR_pos = ConvertArrayPos(verticesInOneRow, i - 0, j - 1);
                        int TL_pos = ConvertArrayPos(verticesInOneRow, i - 1, j - 0);
                        int TR_pos = ConvertArrayPos(verticesInOneRow, i - 0, j - 0);

                        MyVector2 BL = vertices[BL_pos];
                        MyVector2 BR = vertices[BR_pos];
                        MyVector2 TL = vertices[TL_pos];
                        MyVector2 TR = vertices[TR_pos];

                        //Triangle 1
                        //triangles.Add(TR);
                        //triangles.Add(BL);
                        //triangles.Add(TL);

                        //Triangle 2
                        //triangles.Add(TR);
                        //triangles.Add(BR);
                        //triangles.Add(BL);

                        Triangle2 t1 = new Triangle2(TR, BL, TL);
                        Triangle2 t2 = new Triangle2(TR, BR, BL);

                        triangles.Add(t1);
                        triangles.Add(t2);
                    }
                }
            }


            //Generate the mesh
            //Mesh mesh = new Mesh();

            //mesh.name = "Grid";

            //mesh.vertices = vertices.ToArray();
            //mesh.triangles = triangles.ToArray();

            //mesh.RecalculateBounds();
            //mesh.RecalculateNormals();


            return triangles;
        }



        //Convert from 2d array to 1d array
        private static int ConvertArrayPos(int width, int row, int col)
        {
            int arrayPos = width * row + col;

            return arrayPos;
        }
    }
}
