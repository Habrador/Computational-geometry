using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry.Marching_Squares
{
    //The corners in the mesh
    public class Node
    {
        public MyVector2 pos;

        //Index in the mesh which will make it simpler to avoid duplicate vertices
        public int vertexIndex = -1;

        public Node(MyVector2 pos)
        {
            this.pos = pos;
        }
    }
}
