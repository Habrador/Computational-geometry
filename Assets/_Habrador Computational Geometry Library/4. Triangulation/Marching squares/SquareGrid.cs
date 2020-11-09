using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry.Marching_Squares
{
    //Will hold an entire marching squares grid, so we can create multiple of these with different data
    public class SquareGrid
    {
        public Square[,] squares;

        //For creatig a Unity mesh
        public List<Vector3> vertices;

        public List<int> triangles;


        //For displaying the contour (the edge of the mesh)
        //public List<>


        public SquareGrid(int[,] map, float squareSize)
        {
            //Init
            int nodeCountX = map.GetLength(0);
            int nodeCountZ = map.GetLength(1);

            float mapWidthX = nodeCountX * squareSize;
            float mapWidthZ = nodeCountZ * squareSize;


            //Step 1. Create the control nodes which have a position and a state if they are active or not
            //These can be reused between squares, so better to create them once from a memory perspective
            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountZ];

            for (int x = 0; x < nodeCountX; x++)
            {
                for (int z = 0; z < nodeCountZ; z++)
                {
                    //Center the map around (0,0)
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
                    ControlNode BL = controlNodes[x + 0, z + 0];
                    ControlNode BR = controlNodes[x + 1, z + 0];
                    ControlNode TR = controlNodes[x + 1, z + 1];
                    ControlNode TL = controlNodes[x + 0, z + 1];

                    squares[x, z] = new Square(TL, TR, BR, BL);
                }
            }
        }
    }
}
