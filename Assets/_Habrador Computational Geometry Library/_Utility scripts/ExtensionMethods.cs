using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class ExtensionMethods
    {
        //
        // Vectors
        //
    
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


        //Convert between vectors: myvector3 to vector3, etc

        //Vector3 - MyVector2
        public static MyVector2 ToMyVector2(this Vector3 v)
        {
            return new MyVector2(v.x, v.z);
        }

        //Vector2 - MyVector2
        public static MyVector2 ToMyVector2(this Vector2 v)
        {
            return new MyVector2(v.x, v.y);
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

        //MyVector2 -> Vector2
        public static Vector2 ToVector2(this MyVector2 v)
        {
            return new Vector2(v.x, v.y);
        }

        //MyVector2 -> MyVector3 (2d x is 3d x, 2d y is 3d z)
        public static MyVector3 ToMyVector3_Yis3D(this MyVector2 v, float yPos = 0f)
        {
            return new MyVector3(v.x, yPos, v.y);
        }

        //MyVector3 -> MyVector2
        public static MyVector2 ToMyVector2(this MyVector3 v)
        {
            return new MyVector2(v.x, v.z);
        }



        //
        // HashSet
        //

        //Get first best value in a hashset and remove it
        public static T FakePop<T>(this HashSet<T> hashSet)
        {
            T firstBestT = default;
        
            foreach (T thisT in hashSet)
            {
                firstBestT = thisT;

                break;
            }

            hashSet.Remove(firstBestT);

            return firstBestT;
        }

        //Get a ref to the first best value in a hashset
        public static T FakePeek<T>(this HashSet<T> hashSet)
        {
            T firstBestT = default;

            foreach (T thisT in hashSet)
            {
                firstBestT = thisT;

                break;
            }

            return firstBestT;
        }



        //
        // Matrix4x4
        //

        //Add b to a
        //Operator overloads dont work unless they are in the Matrix4x4 class 
        public static Matrix4x4 Add(this Matrix4x4 a, Matrix4x4 b)
        {
            //Can access element in matric by [row, column]

            //Matrix addition is just adding element by element
            return new Matrix4x4(
                new Vector4(a[0, 0] + b[0, 0], a[1, 0] + b[1, 0], a[2, 0] + b[2, 0], a[3, 0] + b[3, 0]),
                new Vector4(a[0, 1] + b[0, 1], a[1, 1] + b[1, 1], a[2, 1] + b[2, 1], a[3, 1] + b[3, 1]),
                new Vector4(a[0, 2] + b[0, 2], a[1, 2] + b[1, 2], a[2, 2] + b[2, 2], a[3, 2] + b[3, 2]),
                new Vector4(a[0, 3] + b[0, 3], a[1, 3] + b[1, 3], a[2, 3] + b[2, 3], a[3, 3] + b[3, 3])
            );
        }

        //Multiplay matrix with a
        public static Matrix4x4 Multiply(this Matrix4x4 a, float b)
        {
            //Can access element in matric by [row, column]

            //Matrix multiplication is just multiplying each element by b
            return new Matrix4x4(
                new Vector4(a[0, 0] * b, a[1, 0] * b, a[2, 0] * b, a[3, 0] * b),
                new Vector4(a[0, 1] * b, a[1, 1] * b, a[2, 1] * b, a[3, 1] * b),
                new Vector4(a[0, 2] * b, a[1, 2] * b, a[2, 2] * b, a[3, 2] * b),
                new Vector4(a[0, 3] * b, a[1, 3] * b, a[2, 3] * b, a[3, 3] * b)
            );
        }
    }
}
