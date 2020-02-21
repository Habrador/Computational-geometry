using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //3D
    public class Plane3
    {
        public MyVector3 pos;

        public MyVector3 normal;


        public Plane3(MyVector3 pos, MyVector3 normal)
        {
            this.pos = pos;

            this.normal = normal;
        }
    }



    //2D
    public class Plane2
    {
        public MyVector2 pos;

        public MyVector2 normal;


        public Plane2(MyVector2 pos, MyVector2 normal)
        {
            this.pos = pos;

            this.normal = normal;
        }
    }
}
