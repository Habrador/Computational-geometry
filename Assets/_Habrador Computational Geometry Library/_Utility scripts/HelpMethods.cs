using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Standardized methods that are the same for all
    public static class HelpMethods
    {
        //
        // Orient triangles so they have the correct orientation
        //
        public static HashSet<Triangle2> OrientTrianglesClockwise(HashSet<Triangle2> triangles)
        {
            //Convert to list or we will no be able to update the orientation
            List<Triangle2> trianglesList = new List<Triangle2>(triangles);

            for (int i = 0; i < trianglesList.Count; i++)
            {
                Triangle2 t = trianglesList[i];

                if (!Geometry.IsTriangleOrientedClockwise(t.p1, t.p2, t.p3))
                {
                    t.ChangeOrientation();

                    trianglesList[i] = t;

                    //Debug.Log("Changed orientation");
                }
            }

            //Back to hashset
            triangles = new HashSet<Triangle2>(trianglesList);

            return triangles;
        }



        //
        // Calculate the AABB of a set of points (in 2d)
        //
        public static AABB GetAABB(List<MyVector2> points)
        {
            MyVector2 p1 = points[0];

            float minX = p1.x;
            float maxX = p1.x;
            float minY = p1.y;
            float maxY = p1.y;

            for (int i = 1; i < points.Count; i++)
            {
                MyVector2 p = points[i];

                if (p.x < minX)
                {
                    minX = p.x;
                }
                else if (p.x > maxX)
                {
                    maxX = p.x;
                }

                if (p.y < minY)
                {
                    minY = p.y;
                }
                else if (p.y > maxY)
                {
                    maxY = p.y;
                }
            }

            AABB box = new AABB(minX, maxX, minY, maxY);

            return box;
        }



        //
        // Normalize points to the range (0 -> 1)
        //
        //From "A fast algorithm for constructing Delaunay triangulations in the plane" by Sloan
        //boundingBox is the rectangle that covers all original points before normalization
        public static MyVector2 NomalizePoint(MyVector2 point, AABB boundingBox, float dMax)
        {
            float x = (point.x - boundingBox.minX) / dMax;
            float y = (point.y - boundingBox.minY) / dMax;

            MyVector2 pNormalized = new MyVector2(x, y);

            return pNormalized;
        }

        public static MyVector2 UnNomalizePoint(MyVector2 point, AABB boundingBox, float dMax)
        {
            float x = (point.x * dMax) + boundingBox.minX;
            float y = (point.y * dMax) + boundingBox.minY;

            MyVector2 pUnNormalized = new MyVector2(x, y);

            return pUnNormalized;
        }

        public static float CalculateDMax(AABB boundingBox)
        {
            float dMax = Mathf.Max(boundingBox.maxX - boundingBox.minX, boundingBox.maxY - boundingBox.minY);

            return dMax;
        }
    }
}
