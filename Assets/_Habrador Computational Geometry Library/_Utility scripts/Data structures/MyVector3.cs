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



        //Operator overloads
        public static MyVector3 operator +(MyVector3 a, MyVector3 b)
        {
            return new MyVector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static MyVector3 operator -(MyVector3 a, MyVector3 b)
        {
            return new MyVector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static MyVector3 operator *(MyVector3 a, float b)
        {
            return new MyVector3(a.x * b, a.y * b, a.z * b);
        }

        public static MyVector3 operator *(float b, MyVector3 a)
        {
            return new MyVector3(a.x * b, a.y * b, a.z * b);
        }

        public static MyVector3 operator -(MyVector3 a)
        {
            return a * -1f;
        }
    }
}
