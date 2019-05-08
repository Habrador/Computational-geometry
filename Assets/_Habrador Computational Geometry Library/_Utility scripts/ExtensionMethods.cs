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
    }
}
