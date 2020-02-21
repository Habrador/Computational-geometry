using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Unity loves to automatically cast beween Vector2 and Vector3
    //Because theres no way to stop it, its better to use a custom struct 
    public struct MyVector3
    {
        public float x;
        public float y;
        public float z;

        public MyVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}
