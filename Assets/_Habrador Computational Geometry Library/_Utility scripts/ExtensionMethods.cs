using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class ExtensionMethods
    {
        //3d -> 2d
        public static Vector2 XZ(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        //2d -> 3d where default y is 0f
        public static Vector3 XYZ(this Vector2 v, float yPos = 0f)
        {
            return new Vector3(v.x, yPos, v.y);
        }


        //
        // Convert between myvector3, vector3, etc
        //

        //Vector3 - MyVector2
        public static MyVector2 ToMyVector2(this Vector3 v)
        {
            return new MyVector2(v.x, v.z);
        }

        //Vector3 -> MyVector3
        public static MyVector3 ToMyVector3(this Vector3 v)
        {
            return new MyVector3(v.x, v.y, v.z);
        }

        //MyVector3 -> Vector3
        public static Vector3 ToVector3(this MyVector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        //MyVector2 -> Vector3
        public static Vector3 ToVector3(this MyVector2 v, float yPos = 0f)
        {
            return new Vector3(v.x, yPos, v.y);
        }

        //MyVector2 -> MyVector3
        public static MyVector3 ToMyVector3(this MyVector2 v, float yPos = 0f)
        {
            return new MyVector3(v.x, yPos, v.y);
        }

        //MyVector3 -> MyVector2
        public static MyVector2 ToMyVector2(this MyVector3 v)
        {
            return new MyVector2(v.x, v.z);
        }
    }
}
