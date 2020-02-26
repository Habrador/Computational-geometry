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
        public static float CalculateDMax(AABB boundingBox)
        {
            float dMax = Mathf.Max(boundingBox.maxX - boundingBox.minX, boundingBox.maxY - boundingBox.minY);

            return dMax;
        }


        //Normalize stuff

        //MyVector2
        public static MyVector2 Normalize(MyVector2 point, AABB boundingBox, float dMax)
        {
            float x = (point.x - boundingBox.minX) / dMax;
            float y = (point.y - boundingBox.minY) / dMax;

            MyVector2 pNormalized = new MyVector2(x, y);

            return pNormalized;
        }

        //List<MyVector2>
        public static List<MyVector2> Normalize(List<MyVector2> points, AABB boundingBox, float dMax)
        {
            List<MyVector2> normalizedPoints = new List<MyVector2>();

            foreach (MyVector2 p in points)
            {
                normalizedPoints.Add(Normalize(p, boundingBox, dMax));
            }

            return normalizedPoints;
        }

        //HashSet<MyVector2> 
        public static HashSet<MyVector2> Normalize(HashSet<MyVector2> points, AABB boundingBox, float dMax)
        {
            HashSet<MyVector2> normalizedPoints = new HashSet<MyVector2>();

            foreach (MyVector2 p in points)
            {
                normalizedPoints.Add(Normalize(p, boundingBox, dMax));
            }

            return normalizedPoints;
        }


        //UnNormalize different stuff

        //MyVector2
        public static MyVector2 UnNormalize(MyVector2 point, AABB boundingBox, float dMax)
        {
            float x = (point.x * dMax) + boundingBox.minX;
            float y = (point.y * dMax) + boundingBox.minY;

            MyVector2 pUnNormalized = new MyVector2(x, y);

            return pUnNormalized;
        }

        //List<MyVector2>
        public static List<MyVector2> UnNormalize(List<MyVector2> normalized, AABB aabb, float dMax)
        {
            List<MyVector2> unNormalized = new List<MyVector2>();

            foreach (MyVector2 p in normalized)
            {
                MyVector2 pUnNormalized = UnNormalize(p, aabb, dMax);

                unNormalized.Add(pUnNormalized);
            }

            return unNormalized;
        }

        //HashSet<Triangle2>
        public static HashSet<Triangle2> UnNormalize(HashSet<Triangle2> normalized, AABB aabb, float dMax)
        {
            HashSet<Triangle2> unNormalized = new HashSet<Triangle2>();

            foreach (Triangle2 t in normalized)
            {
                MyVector2 p1 = HelpMethods.UnNormalize(t.p1, aabb, dMax);
                MyVector2 p2 = HelpMethods.UnNormalize(t.p2, aabb, dMax);
                MyVector2 p3 = HelpMethods.UnNormalize(t.p3, aabb, dMax);

                Triangle2 tUnNormalized = new Triangle2(p1, p2, p3);

                unNormalized.Add(tUnNormalized);
            }

            return unNormalized;
        }

        //HalfEdgeData2
        public static HalfEdgeData2 UnNormalize(HalfEdgeData2 data, AABB aabb, float dMax)
        {
            foreach (HalfEdgeVertex2 v in data.vertices)
            {
                MyVector2 vUnnNormalized = HelpMethods.UnNormalize(v.position, aabb, dMax);

                v.position = vUnnNormalized;
            }

            return data;
        }
    }
}
