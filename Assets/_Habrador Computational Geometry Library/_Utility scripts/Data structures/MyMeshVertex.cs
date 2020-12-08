using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public struct MyMeshVertex
    {
        public MyVector3 pos;
        public MyVector3 normal;
        public MyVector2 uv;

        public MyMeshVertex(MyVector3 pos, MyVector3 normal)
        {
            this.pos = pos;
            this.normal = normal;

            this.uv = default;
        }

        public MyMeshVertex(MyVector3 pos, MyVector3 normal, MyVector2 uv)
        {
            this.pos = pos;
            this.normal = normal;
            this.uv = uv;
        }
    }
}
