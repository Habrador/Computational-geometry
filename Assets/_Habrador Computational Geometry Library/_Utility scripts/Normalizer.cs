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
        // UnNormalize stuff
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

        //HashSet<VoronoiCell2>
        public HashSet<VoronoiCell2> UnNormalize(HashSet<VoronoiCell2> data)
        {
            HashSet<VoronoiCell2> unNormalizedData = new HashSet<VoronoiCell2>();

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



    public class Normalizer3
    {
        private float dMax;

        private AABB3 boundingBox;


        public Normalizer3(List<MyVector3> points)
        {
            this.boundingBox = new AABB3(points);

            this.dMax = CalculateDMax(this.boundingBox);
        }


        //From "A fast algorithm for constructing Delaunay triangulations in the plane" by Sloan
        //boundingBox is the rectangle that covers all original points before normalization
        public float CalculateDMax(AABB3 aabb)
        {
            float dMax = Mathf.Max(aabb.max.x - aabb.min.x, Mathf.Max(aabb.max.y - aabb.min.y, aabb.max.z - aabb.min.z));

            return dMax;
        }



        //
        // Normalize stuff
        //

        //MyVector3
        public MyVector3 Normalize(MyVector3 point)
        {
            float x = (point.x - boundingBox.min.x) / dMax;
            float y = (point.y - boundingBox.min.y) / dMax;
            float z = (point.z - boundingBox.min.z) / dMax;

            MyVector3 pNormalized = new MyVector3(x, y, z);

            return pNormalized;
        }


        //List<MyVector3>
        public List<MyVector3> Normalize(List<MyVector3> points)
        {
            List<MyVector3> normalizedPoints = new List<MyVector3>();

            foreach (MyVector3 p in points)
            {
                normalizedPoints.Add(Normalize(p));
            }

            return normalizedPoints;
        }


        //HashSet<MyVector3> 
        public HashSet<MyVector3> Normalize(HashSet<MyVector3> points)
        {
            HashSet<MyVector3> normalizedPoints = new HashSet<MyVector3>();

            foreach (MyVector3 p in points)
            {
                normalizedPoints.Add(Normalize(p));
            }

            return normalizedPoints;
        }


        //HalfEdgeData3
        public HalfEdgeData3 Normalize(HalfEdgeData3 data)
        {
            foreach (HalfEdgeVertex3 v in data.verts)
            {
                MyVector3 vNormalized = Normalize(v.position);

                v.position = vNormalized;
            }

            return data;
        }


        //HashSet<HalfEdgeFace3>
        public HashSet<HalfEdgeFace3> Normalize(HashSet<HalfEdgeFace3> data)
        {
            foreach (HalfEdgeFace3 f in data)
            {
                //TODO: This will generate a new list for each face, so maybe better to put the code from the method here
                List<HalfEdge3> edges = f.GetEdges();

                if (edges == null)
                {
                    continue;
                }

                foreach (HalfEdge3 e in edges)
                {
                    HalfEdgeVertex3 v = e.v;

                    v.position = Normalize(v.position);
                }
            }

            return data;
        }



        //
        // UnNormalize stuff
        //

        //MyVector3
        public MyVector3 UnNormalize(MyVector3 point)
        {
            float x = (point.x * dMax) + boundingBox.min.x;
            float y = (point.y * dMax) + boundingBox.min.y;
            float z = (point.z * dMax) + boundingBox.min.z;

            MyVector3 pUnNormalized = new MyVector3(x, y, z);

            return pUnNormalized;
        }


        //List<MyVector3>
        public List<MyVector3> UnNormalize(List<MyVector3> normalized)
        {
            List<MyVector3> unNormalized = new List<MyVector3>();

            foreach (MyVector3 p in normalized)
            {
                MyVector3 pUnNormalized = UnNormalize(p);

                unNormalized.Add(pUnNormalized);
            }

            return unNormalized;
        }


        //HashSet<Triangle3>
        public HashSet<Triangle3> UnNormalize(HashSet<Triangle3> normalized)
        {
            HashSet<Triangle3> unNormalized = new HashSet<Triangle3>();

            foreach (Triangle3 t in normalized)
            {
                MyVector3 p1 = UnNormalize(t.p1);
                MyVector3 p2 = UnNormalize(t.p2);
                MyVector3 p3 = UnNormalize(t.p3);

                Triangle3 tUnNormalized = new Triangle3(p1, p2, p3);

                unNormalized.Add(tUnNormalized);
            }

            return unNormalized;
        }


        //HalfEdgeData3
        public HalfEdgeData3 UnNormalize(HalfEdgeData3 data)
        {
            foreach (HalfEdgeVertex3 v in data.verts)
            {
                MyVector3 vUnNormalized = UnNormalize(v.position);

                v.position = vUnNormalized;
            }

            return data;
        }


        //HashSet<HalfEdgeFace3>
        public HashSet<HalfEdgeFace3> UnNormalize(HashSet<HalfEdgeFace3> data)
        {
            foreach (HalfEdgeFace3 f in data)
            {
                //TODO: This will generate a new list for each face, so maybe better to put the code from the method here
                List<HalfEdge3> edges = f.GetEdges();

                if (edges == null)
                {
                    continue;
                }

                foreach (HalfEdge3 e in edges)
                {
                    HalfEdgeVertex3 v = e.v;

                    v.position = UnNormalize(v.position);
                }
            }

            return data;
        }


        //HashSet<VoronoiCell3>
        public HashSet<VoronoiCell3> UnNormalize(HashSet<VoronoiCell3> data)
        {
            HashSet<VoronoiCell3> unNormalizedData = new HashSet<VoronoiCell3>();

            foreach (VoronoiCell3 cell in data)
            {
                MyVector3 sitePosUnNormalized = UnNormalize(cell.sitePos);

                VoronoiCell3 cellUnNormalized = new VoronoiCell3(sitePosUnNormalized);

                foreach (VoronoiEdge3 e in cell.edges)
                {
                    MyVector3 p1UnNormalized = UnNormalize(e.p1);
                    MyVector3 p2UnNormalized = UnNormalize(e.p2);

                    VoronoiEdge3 eUnNormalized = new VoronoiEdge3(p1UnNormalized, p2UnNormalized, sitePosUnNormalized);

                    cellUnNormalized.edges.Add(eUnNormalized);
                }

                unNormalizedData.Add(cellUnNormalized);
            }

            return unNormalizedData;
        }
        
    }
}
