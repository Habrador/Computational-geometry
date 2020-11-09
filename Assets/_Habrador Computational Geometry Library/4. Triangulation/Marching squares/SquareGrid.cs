using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry.Marching_Squares
{
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
}
