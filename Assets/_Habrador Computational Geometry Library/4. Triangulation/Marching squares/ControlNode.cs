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

        public ControlNode(Vector3 pos, bool isActive, float squareSize) : base(pos)
        {
            this.isActive = isActive;

            this.above = new Node(base.pos + Vector3.forward * squareSize * 0.5f);

            this.right = new Node(base.pos + Vector3.right * squareSize * 0.5f);
        }
    }
}
