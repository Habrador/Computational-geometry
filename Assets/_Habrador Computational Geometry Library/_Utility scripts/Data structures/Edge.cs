using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //And edge between two vertices
    public class Edge
    {
        public Vector3 p1;
        public Vector3 p2;

        //Is this edge intersecting with another edge?
        public bool isIntersecting = false;

        public Edge(Vector3 p1, Vector3 p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }

        //Flip edge
        //public void FlipEdge()
        //{
        //    Vector3 temp = v1;

        //    v1 = v2;

        //    v2 = temp;
        //}
    }
}
