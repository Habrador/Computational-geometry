using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry.Marching_Squares;

namespace Habrador_Computational_Geometry
{
    //Based on Procedural Cave Generation (E02. Marching Squares): https://www.youtube.com/watch?v=yOgIncKp0BE
    public static class MarchingSquares
    {
        //For the mesh
        public static List<Vector3> vertices;

        public static List<int> triangles;


        //The map consists of 0 or 1, where 1 means solid
        //squareSize is how big each square in the grid is 
        //The map should be created so that 0,0 is negative X and negative Z, which should maybe change in the future?
        //Maybe more general to be nodebased so we dont have to care about the grid, but marching squares is always on a grid
        public static SquareGrid GenerateMesh(int[,] map, float squareSize)
        {
            //Create the data grid to make it easier to triangulate
            SquareGrid squareGrid = new SquareGrid(map, squareSize);

            //Triangulate
            vertices = new List<Vector3>();

            triangles = new List<int>();

            int xLength = squareGrid.squares.GetLength(0);
            int zLength = squareGrid.squares.GetLength(1);

            for (int x = 0; x < xLength; x++)
            {
                for (int z = 0; z < zLength; z++)
                {
                    Square square = squareGrid.squares[x, z];

                    TriangulateSquare(square);
                }
            }

            //Assign the static vertices and triangles to the grid
            squareGrid.triangles = new List<int>(triangles);

            squareGrid.vertices = new List<Vector3>(vertices);

            return squareGrid;
        }



        //Triangulate a square with marching squares
        private static void TriangulateSquare(Square s)
        {
            switch (s.configuration)
            {
                case 0:
                    break;

                //1 point is active
                case 1:
                    MeshFromPoints(s.B, s.BL, s.L);
                    break;
                case 2:
                    MeshFromPoints(s.BR, s.B, s.R);
                    break;
                case 4:
                    MeshFromPoints(s.TR, s.R, s.T);
                    break;
                case 8:
                    MeshFromPoints(s.T, s.L, s.TL);
                    break;

                //2 points are active
                case 3:
                    MeshFromPoints(s.R, s.BR, s.BL, s.L);
                    break;
                case 6:
                    MeshFromPoints(s.BR, s.B, s.T, s.TR);
                    break;
                case 9:
                    MeshFromPoints(s.B, s.BL, s.TL, s.T);
                    break;
                case 12:
                    MeshFromPoints(s.R, s.L, s.TL, s.TR);
                    break;
                case 5:
                    MeshFromPoints(s.R, s.B, s.BL, s.L, s.T, s.TR);
                    break;
                case 10:
                    MeshFromPoints(s.R, s.BR, s.B, s.L, s.TL, s.T);
                    break;

                //3 points are active
                case 7:
                    MeshFromPoints(s.T, s.TR, s.BR, s.BL, s.L);
                    break;
                case 11:
                    MeshFromPoints(s.R, s.BR, s.BL, s.TL, s.T);
                    break;
                case 13:
                    MeshFromPoints(s.R, s.B, s.BL, s.TL, s.TR);
                    break;
                case 14:
                    MeshFromPoints(s.TR, s.BR, s.B, s.L, s.TL);
                    break;

                //4 points active
                case 15:
                    MeshFromPoints(s.TR, s.BR, s.BL, s.BL, s.TL);
                    break;
            }
        }



        //Triangulate unknown number of points
        private static void MeshFromPoints(params Node[] points)
        {
            AssignVertices(points);

            //Similar to triangulating a convex hull if we know the points on the hull
            if (points.Length >= 3)
            {
                CreateTriangle(points[0], points[1], points[2]);
            }
            if (points.Length >= 4)
            {
                CreateTriangle(points[0], points[2], points[3]);
            }
            if (points.Length >= 5)
            {
                CreateTriangle(points[0], points[3], points[4]);
            }
            if (points.Length >= 6)
            {
                CreateTriangle(points[0], points[4], points[5]);
            }
        }



        //Each node has a vertexIndex which we can use to avoid duplicates
        private static void AssignVertices(Node[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].vertexIndex == -1)
                {
                    //Add it
                    vertices.Add(points[i].pos);

                    points[i].vertexIndex = vertices.Count - 1;
                }
            }
        }



        private static void CreateTriangle(Node a, Node b, Node c)
        {
            triangles.Add(a.vertexIndex);
            triangles.Add(b.vertexIndex);
            triangles.Add(c.vertexIndex);
        }
    }
}
