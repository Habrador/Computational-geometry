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

                if (!_Geometry.IsTriangleOrientedClockwise(t.p1, t.p2, t.p3))
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
        // Normalize points to the range (0 -> 1)
        //
        //From "A fast algorithm for constructing Delaunay triangulations in the plane" by Sloan
        //boundingBox is the rectangle that covers all original points before normalization
        public static float CalculateDMax(AABB2 boundingBox)
        {
            float dMax = Mathf.Max(boundingBox.maxX - boundingBox.minX, boundingBox.maxY - boundingBox.minY);

            return dMax;
        }


        //Normalize stuff

        //MyVector2
        public static MyVector2 Normalize(MyVector2 point, AABB2 boundingBox, float dMax)
        {
            float x = (point.x - boundingBox.minX) / dMax;
            float y = (point.y - boundingBox.minY) / dMax;

            MyVector2 pNormalized = new MyVector2(x, y);

            return pNormalized;
        }

        //List<MyVector2>
        public static List<MyVector2> Normalize(List<MyVector2> points, AABB2 boundingBox, float dMax)
        {
            List<MyVector2> normalizedPoints = new List<MyVector2>();

            foreach (MyVector2 p in points)
            {
                normalizedPoints.Add(Normalize(p, boundingBox, dMax));
            }

            return normalizedPoints;
        }

        //HashSet<MyVector2> 
        public static HashSet<MyVector2> Normalize(HashSet<MyVector2> points, AABB2 boundingBox, float dMax)
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
        public static MyVector2 UnNormalize(MyVector2 point, AABB2 boundingBox, float dMax)
        {
            float x = (point.x * dMax) + boundingBox.minX;
            float y = (point.y * dMax) + boundingBox.minY;

            MyVector2 pUnNormalized = new MyVector2(x, y);

            return pUnNormalized;
        }

        //List<MyVector2>
        public static List<MyVector2> UnNormalize(List<MyVector2> normalized, AABB2 aabb, float dMax)
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
        public static HashSet<Triangle2> UnNormalize(HashSet<Triangle2> normalized, AABB2 aabb, float dMax)
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
        public static HalfEdgeData2 UnNormalize(HalfEdgeData2 data, AABB2 aabb, float dMax)
        {
            foreach (HalfEdgeVertex2 v in data.vertices)
            {
                MyVector2 vUnNormalized = HelpMethods.UnNormalize(v.position, aabb, dMax);

                v.position = vUnNormalized;
            }

            return data;
        }

        //List<VoronoiCell2>
        public static List<VoronoiCell2> UnNormalize(List<VoronoiCell2> data, AABB2 aabb, float dMax)
        {
            List<VoronoiCell2> unNormalizedData = new List<VoronoiCell2>();

            foreach (VoronoiCell2 cell in data)
            {
                MyVector2 sitePosUnNormalized = HelpMethods.UnNormalize(cell.sitePos, aabb, dMax);

                VoronoiCell2 cellUnNormalized = new VoronoiCell2(sitePosUnNormalized);

                foreach (VoronoiEdge2 e in cell.edges)
                {
                    MyVector2 p1UnNormalized = HelpMethods.UnNormalize(e.p1, aabb, dMax);
                    MyVector2 p2UnNormalized = HelpMethods.UnNormalize(e.p2, aabb, dMax);

                    VoronoiEdge2 eUnNormalized = new VoronoiEdge2(p1UnNormalized, p2UnNormalized, sitePosUnNormalized);

                    cellUnNormalized.edges.Add(eUnNormalized);
                }

                unNormalizedData.Add(cellUnNormalized);
            }

            return unNormalizedData;
        }
    }
}
