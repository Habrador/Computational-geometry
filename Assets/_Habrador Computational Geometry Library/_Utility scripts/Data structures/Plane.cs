using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public class Plane
    {
        public Vector3 pos;

        public Vector3 normal;


        public Plane(Vector3 pos, Vector3 normal)
        {
            this.pos = pos;

            this.normal = normal;
        }
    }



    public class Plane2D
    {
        public Vector2 pos;

        public Vector2 normal;


        public Plane2D(Vector2 pos, Vector2 normal)
        {
            this.pos = pos;

            this.normal = normal;
        }
    }
}
