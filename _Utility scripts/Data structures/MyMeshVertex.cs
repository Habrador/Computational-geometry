using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public struct MyMeshVertex
    {
        public MyVector3 position;
        public MyVector3 normal;
        public MyVector2 uv;

        public MyMeshVertex(MyVector3 position, MyVector3 normal)
        {
            this.position = position;
            this.normal = normal;

            this.uv = default;
        }

        public MyMeshVertex(MyVector3 position, MyVector3 normal, MyVector2 uv)
        {
            this.position = position;
            this.normal = normal;
            this.uv = uv;
        }
    }
}
