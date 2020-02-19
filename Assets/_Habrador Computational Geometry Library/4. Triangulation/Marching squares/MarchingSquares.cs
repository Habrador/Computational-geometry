using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingSquares
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



    //Will hold an entire marching squares grid, so we can create multiple of these with different data
    public class SquareGrid
    {
        public Square[,] squares;

        public List<Vector3> vertices;

        public List<int> triangles;


        public SquareGrid(int[,] map, float squareSize)
        {
            //Init
            //Its more general to say we dont know if the map is a square
            int nodeCountX = map.GetLength(0);
            int nodeCountZ = map.GetLength(1);

            float mapWidthX = nodeCountX * squareSize;
            float mapWidthZ = nodeCountZ * squareSize;


            //Step 1. First create the control nodes
            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountZ];

            for (int x = 0; x < nodeCountX; x++)
            {
                for (int z = 0; z < nodeCountZ; z++)
                {
                    float xPos = -mapWidthX * 0.5f + x * squareSize + squareSize * 0.5f;
                    float zPos = -mapWidthZ * 0.5f + z * squareSize + squareSize * 0.5f;

                    Vector3 pos = new Vector3(xPos, 0f, zPos);

                    bool isActive = map[x, z] == 1;

                    controlNodes[x, z] = new ControlNode(pos, isActive, squareSize);
                }
            }


            //Step 2. Create the squares which consists of 4 control nodes, so there will be 1 less square than nodes
            squares = new Square[nodeCountX - 1, nodeCountZ - 1];

            for (int x = 0; x < nodeCountX - 1; x++)
            {
                for (int z = 0; z < nodeCountZ - 1; z++)
                {
                    //The control nodes were created from BL
                    squares[x, z] = new Square(
                        controlNodes[x + 0, z + 1],
                        controlNodes[x + 1, z + 1],
                        controlNodes[x + 1, z + 0],
                        controlNodes[x + 0, z + 0]
                        );
                }
            }
        }
    }



    //A square with control nodes in the corners and nodes on the edges between the corners
    //We need all 8 to determine the position and size of the mesh as this square
    public class Square 
    {
        //The nodes that determines the size of the mesh
        public ControlNode TL, TR, BL, BR;
        //The midpoint nodes that are between the control nodes that determines the corners of the mesh
        public Node L, T, R, B;
        //The marching square configuration for this square (16 possibilities)
        public int configuration = 0;

        public Square(ControlNode TL, ControlNode TR, ControlNode BR, ControlNode BL)
        {
            this.TL = TL;
            this.TR = TR;
            this.BL = BL;
            this.BR = BR;

            this.L = BL.above;
            this.T = TL.right;
            this.R = BR.above;
            this.B = BL.right;

            if (TL.isActive)
            {
                configuration += 8;
            }
            if (TR.isActive)
            {
                configuration += 4;
            }
            if (BL.isActive)
            {
                configuration += 1;
            }
            if (BR.isActive)
            {
                configuration += 2;
            }
        }
    }



    //The corners in the mesh
    public class Node
    {
        public Vector3 pos;
        //Index in the mesh
        public int vertexIndex = -1;

        public Node(Vector3 pos)
        {
            this.pos = pos;
        }
    }



    //The corner switches that determines which mesh to pick
    //These are determined by the map we send to the algorithm, where each node can be either 1 or 0
    public class ControlNode : Node
    {
        public bool isActive;

        //Each switch needs a reference to two nodes that determines the position of the mesh
        public Node above, right;

        public ControlNode(Vector3 pos, bool isActive, float squareSize) : base(pos)
        {
            this.isActive = isActive;

            this.above = new Node(base.pos + Vector3.forward * squareSize * 0.5f);

            this.right = new Node(base.pos + Vector3.right * squareSize * 0.5f);
        }
    }
	
	
}
