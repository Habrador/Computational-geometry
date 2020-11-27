using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Line in 2d space
    public struct Line2
    {
        public MyVector2 p1;
        public MyVector2 p2;

        public Line2(MyVector2 p1, MyVector2 p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }
    }
}
