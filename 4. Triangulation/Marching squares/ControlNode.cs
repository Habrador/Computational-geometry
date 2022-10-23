using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry.Marching_Squares
{
    //The corner switches that determines which mesh to pick
    //These are determined by the map we send to the algorithm, where each node can be either 1 or 0
    public class ControlNode : Node
    {
        public bool isActive;

        //Each node needs a reference to two other nodes, which are needed when we generate the mesh
        //These nodes are in the middle between two corners
        public Node above, right;

        //Each node might have a value, which is useful if we want to smooth
        public float value;

        public ControlNode(MyVector2 pos, bool isActive, float squareSize, float value) : base(pos)
        {
            this.isActive = isActive;

            this.above = new Node(base.pos + new MyVector2(0f, 1f) * squareSize * 0.5f);

            this.right = new Node(base.pos + new MyVector2(1f, 0f) * squareSize * 0.5f);

            this.value = value;
        }
    }
}
