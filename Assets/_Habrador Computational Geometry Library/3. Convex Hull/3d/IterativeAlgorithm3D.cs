using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Generate a convex hull in 3d space with an iterative algorithm (also known as beneath-beyond)
    //Based on "Computational Geometry in C" by Joseph O'Rourke 
    public static class IterativeAlgorithm3D
    {
        public static HalfEdgeData3 GenerateConvexHull(HashSet<MyVector3> originalPoints)
        {
            HalfEdgeData3 convexHull = new HalfEdgeData3();

            //Step 1. Initialized by making 2 triangles by using three points, so its a flat triangle with a face on each side
            //We could use the ideas from Quickhull to make the start triangle as big as possible
            //We can also remove all vertices that are co-planar with this flat triangle (and are within the triangle)
            //Then find a point which is as far away as possible from this triangle
            //Add it and you have a tetrahedron (triangular pyramid)
            BuildFirstTetrahedron(originalPoints);

            //Step 2. For each other point, test if the point is inside (or on the surface?) of the mesh we have so far
            //If inside, remove it because the point is not on the hull
            //If not inside, see which triangles are visible to the point and remove them
            //Then build new triangles from the edges that have no neighbor to the point

            return convexHull;
        }


        private static void BuildFirstTetrahedron(HashSet<MyVector3> originalPoints)
        {
            List<MyVector3> points = new List<MyVector3>(originalPoints);
        
            //Find all possible combinations of edges between all points
            //TODO: Better to first find the points with smallest and largest x,y,z values
            List<Edge3> pointCombinations = new List<Edge3>();

            for (int i = 0; i < points.Count; i++)
            {
                MyVector3 p1 = points[i];

                for (int j = i + 1; j < points.Count; j++)
                {
                    MyVector3 p2 = points[j];

                    Edge3 e = new Edge3(p1, p2);

                    pointCombinations.Add(e);
                }
            }


            //Find the edge that is the furthest apart
            Edge3 eFurthestApart = pointCombinations[0];

            float maxDistanceBetween = MyVector3.SqrDistance(eFurthestApart.p1, eFurthestApart.p2);

            for (int i = 1; i < pointCombinations.Count; i++)
            {
                Edge3 e = pointCombinations[i];

                float distanceBetween = MyVector3.SqrDistance(e.p1, e.p2);

                if (distanceBetween > maxDistanceBetween)
                {
                    maxDistanceBetween = distanceBetween;

                    eFurthestApart = e;
                }
            }

            Debug.DrawLine(eFurthestApart.p1.ToVector3(), eFurthestApart.p2.ToVector3(), Color.white, 1f);

            //Remove the two points we found         
            originalPoints.Remove(eFurthestApart.p1);
            originalPoints.Remove(eFurthestApart.p2);


            //Find a point which is the furthest apart from this edge
            points = new List<MyVector3>(originalPoints);

            MyVector3 pointFurthestAway = points[0];

            MyVector3 closestPoint = _Geometry.GetClosestPointOnLine(eFurthestApart, pointFurthestAway, withinSegment: false);

            float maxDistSqr = MyVector3.SqrDistance(pointFurthestAway, closestPoint);

            for (int i = 1; i < points.Count; i++)
            {
                MyVector3 thisPoint = points[i];

                closestPoint = _Geometry.GetClosestPointOnLine(eFurthestApart, pointFurthestAway, withinSegment: false);

                float distSqr = MyVector3.SqrDistance(thisPoint, closestPoint);

                if (distSqr > maxDistSqr)
                {
                    maxDistSqr = distSqr;

                    pointFurthestAway = thisPoint;
                }
            }


            Debug.DrawLine(eFurthestApart.p1.ToVector3(), pointFurthestAway.ToVector3(), Color.white, 1f);
            Debug.DrawLine(eFurthestApart.p2.ToVector3(), pointFurthestAway.ToVector3(), Color.white, 1f);
        }
    }
}
