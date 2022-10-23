using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry.Marching_Squares
{
    //Will hold an entire marching squares grid, so we can create multiple of these with different data
    //Will also hold other useful information such as contour edges
    public class SquareGrid
    {
        public Square[,] squares;

        //For creating a Unity mesh
        public List<MyVector2> vertices;

        public List<int> triangles;


        //For displaying the contour (the edge of the mesh)
        public List<Edge2> contourEdges;



        public SquareGrid(float[,] map, float squareSize)
        {
            //Init
            int nodeCountX = map.GetLength(0);
            int nodeCountZ = map.GetLength(1);

            float halfMapWidthX = nodeCountX * squareSize * 0.5f;
            float halfMapWidthZ = nodeCountZ * squareSize * 0.5f;

            float halfSquareSize = squareSize * 0.5f;


            //Step 1. Create the control nodes which have a position and a state if they are active or not
            //These can be reused between squares, so better to create them once from a memory perspective
            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountZ];

            for (int x = 0; x < nodeCountX; x++)
            {
                for (int z = 0; z < nodeCountZ; z++)
                {
                    //Center the map around (0,0)
                    float xPos = -halfMapWidthX + x * squareSize + halfSquareSize;
                    float zPos = -halfMapWidthZ + z * squareSize + halfSquareSize;

                    MyVector2 pos = new MyVector2(xPos, zPos);

                    bool isActive = map[x, z] >= 1f - Mathf.Epsilon;

                    controlNodes[x, z] = new ControlNode(pos, isActive, squareSize, map[x, z]);
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



        //Marching squares is a 2d algorithm, so we need a height to display it in 3d
        public Mesh GenerateUnityMesh(float meshHeight)
        {
            Mesh mesh = new Mesh();

            //Convert from 2d to 3d
            Vector3[] meshVertices = new Vector3[vertices.Count];

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 v = vertices[i].ToVector3(meshHeight);

                meshVertices[i] = v;
            }

            mesh.vertices = meshVertices;

            mesh.triangles = triangles.ToArray();

            mesh.RecalculateNormals();

            return mesh;
        }
    }
}
