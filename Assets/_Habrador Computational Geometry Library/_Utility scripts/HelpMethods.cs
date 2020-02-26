using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Standardized methods that are the same for all
    public static class HelpMethods
    {
        //Orient triangles so they have the correct orientation
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



        //Calculate the AABB of a set of points
        public static AABB GetAABB(List<MyVector2> points)
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            for (int i = 0; i < points.Count; i++)
            {
                MyVector2 p = points[i];

                if (p.x < minX)
                {
                    minX = p.x;
                }
                if (p.x > maxX)
                {
                    maxX = p.x;
                }

                if (p.y < minY)
                {
                    minY = p.y;
                }
                if (p.y > maxY)
                {
                    maxY = p.y;
                }
            }

            AABB box = new AABB(minX, maxX, minY, maxY);

            return box;
        }



        //Normalize the points to the range(0 - 1)
        //From "A fast algorithm for constructing Delaunay triangulations in the plane" by Sloan
        public static void NormalizePoints(List<MyVector2> points, out List<MyVector2> normalizedPoints, out float d_max, out AABB boundingBox)
        {
            boundingBox = HelpMethods.GetAABB(points);

            d_max = Mathf.Max(boundingBox.maxX - boundingBox.minX, boundingBox.maxY - boundingBox.minY);

            normalizedPoints = new List<MyVector2>();

            //Normalize
            foreach (MyVector2 p in points)
            {
                float x = (p.x - boundingBox.minX) / d_max;
                float y = (p.y - boundingBox.minY) / d_max;

                MyVector2 pNormalized = new MyVector2(x, y);

                normalizedPoints.Add(pNormalized);
            }
        }
    }
}
