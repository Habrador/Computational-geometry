using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //And edge between two vertices in 2d space
    public class Edge2
    {
        public MyVector2 p1;
        public MyVector2 p2;

        //Is this edge intersecting with another edge?
        public bool isIntersecting = false;

        public Edge2(MyVector2 p1, MyVector2 p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }
    }



    //And edge between two vertices in 3d space
    public struct Edge3
    {
        public MyVector3 p1;
        public MyVector3 p2;

        //Is this edge intersecting with another edge?
        //public bool isIntersecting = false;

        public Edge3(MyVector3 p1, MyVector3 p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }
    }
}
