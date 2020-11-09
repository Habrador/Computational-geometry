using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry.Marching_Squares
{
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
}
