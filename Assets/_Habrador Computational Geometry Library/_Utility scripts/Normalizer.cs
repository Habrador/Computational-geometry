using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //To avoid floating point precision issues, it's common to normalize all data to range 0-1
    public class Normalizer2
    {
        private float dMax;

        private AABB2 boundingBox;


        public Normalizer2(List<MyVector2> points)
        {
            this.boundingBox = new AABB2(points);

            this.dMax = CalculateDMax(this.boundingBox);
        }


        //From "A fast algorithm for constructing Delaunay triangulations in the plane" by Sloan
        //boundingBox is the rectangle that covers all original points before normalization
        public float CalculateDMax(AABB2 boundingBox)
        {
            float dMax = Mathf.Max(boundingBox.max.x - boundingBox.min.x, boundingBox.max.y - boundingBox.min.y);

            return dMax;
        }



        //
        // Normalize stuff
        //

        //MyVector2
        public MyVector2 Normalize(MyVector2 point)
        {
            float x = (point.x - boundingBox.min.x) / dMax;
            float y = (point.y - boundingBox.min.y) / dMax;

            MyVector2 pNormalized = new MyVector2(x, y);

            return pNormalized;
        }

        //List<MyVector2>
        public List<MyVector2> Normalize(List<MyVector2> points)
        {
            List<MyVector2> normalizedPoints = new List<MyVector2>();

            foreach (MyVector2 p in points)
            {
                normalizedPoints.Add(Normalize(p));
            }

            return normalizedPoints;
        }

        //HashSet<MyVector2> 
        public HashSet<MyVector2> Normalize(HashSet<MyVector2> points)
        {
            HashSet<MyVector2> normalizedPoints = new HashSet<MyVector2>();

            foreach (MyVector2 p in points)
            {
                normalizedPoints.Add(Normalize(p));
            }

            return normalizedPoints;
        }



        //
        // UnNormalize different stuff
        //

        //MyVector2
        public MyVector2 UnNormalize(MyVector2 point)
        {
            float x = (point.x * dMax) + boundingBox.min.x;
            float y = (point.y * dMax) + boundingBox.min.y;

            MyVector2 pUnNormalized = new MyVector2(x, y);

            return pUnNormalized;
        }

        //List<MyVector2>
        public List<MyVector2> UnNormalize(List<MyVector2> normalized)
        {
            List<MyVector2> unNormalized = new List<MyVector2>();

            foreach (MyVector2 p in normalized)
            {
                MyVector2 pUnNormalized = UnNormalize(p);

                unNormalized.Add(pUnNormalized);
            }

            return unNormalized;
        }

        //HashSet<Triangle2>
        public HashSet<Triangle2> UnNormalize(HashSet<Triangle2> normalized)
        {
            HashSet<Triangle2> unNormalized = new HashSet<Triangle2>();

            foreach (Triangle2 t in normalized)
            {
                MyVector2 p1 = UnNormalize(t.p1);
                MyVector2 p2 = UnNormalize(t.p2);
                MyVector2 p3 = UnNormalize(t.p3);

                Triangle2 tUnNormalized = new Triangle2(p1, p2, p3);

                unNormalized.Add(tUnNormalized);
            }

            return unNormalized;
        }

        //HalfEdgeData2
        public HalfEdgeData2 UnNormalize(HalfEdgeData2 data)
        {
            foreach (HalfEdgeVertex2 v in data.vertices)
            {
                MyVector2 vUnNormalized = UnNormalize(v.position);

                v.position = vUnNormalized;
            }

            return data;
        }

        //List<VoronoiCell2>
        public List<VoronoiCell2> UnNormalize(List<VoronoiCell2> data)
        {
            List<VoronoiCell2> unNormalizedData = new List<VoronoiCell2>();

            foreach (VoronoiCell2 cell in data)
            {
                MyVector2 sitePosUnNormalized = UnNormalize(cell.sitePos);

                VoronoiCell2 cellUnNormalized = new VoronoiCell2(sitePosUnNormalized);

                foreach (VoronoiEdge2 e in cell.edges)
                {
                    MyVector2 p1UnNormalized = UnNormalize(e.p1);
                    MyVector2 p2UnNormalized = UnNormalize(e.p2);

                    VoronoiEdge2 eUnNormalized = new VoronoiEdge2(p1UnNormalized, p2UnNormalized, sitePosUnNormalized);

                    cellUnNormalized.edges.Add(eUnNormalized);
                }

                unNormalizedData.Add(cellUnNormalized);
            }

            return unNormalizedData;
        }
    }
}
