using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //3D
    public class Ray3
    {
        public MyVector3 origin;

        public MyVector3 dir;


        public Ray3(MyVector3 origin, MyVector3 dir)
        {
            this.origin = origin;

            this.dir = dir;
        }
    }



    //2D
    public class Ray2
    {
        public MyVector2 origin;

        public MyVector2 dir;


        public Ray2(MyVector2 origin, MyVector2 dir)
        {
            this.origin = origin;

            this.dir = dir;
        }
    }
}
