using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Generate a convex hull in 3d space with an iterative algorithm (also known as beneath-beyond)
    //Based on "Computational Geometry in C" by Joseph O'Rourke 
    public static class IterativeHullAlgorithm3D
    {
        public static HalfEdgeData3 GenerateConvexHull(HashSet<MyVector3> originalPoints)
        {
            HalfEdgeData3 convexHull = new HalfEdgeData3();

            //Step 1. Init by making a tetrahedron (triangular pyramid)
            BuildFirstTetrahedron(originalPoints, convexHull);

            //Step 2. For each other point, test if the point is inside (or on the surface?) of the mesh we have so far
            //If inside, remove it because the point is not on the hull
            //If not inside, see which triangles are visible to the point and remove them
            //Then build new triangles from the edges that have no neighbor to the point

            return convexHull;
        }



        //Initialize by making 2 triangles by using three points, so its a flat triangle with a face on each side
        //We could use the ideas from Quickhull to make the start triangle as big as possible
        //Then find a point which is the furthest away as possible from these triangles
        //Add that point and you have a tetrahedron (triangular pyramid)
        private static void BuildFirstTetrahedron(HashSet<MyVector3> points, HalfEdgeData3 convexHull)
        {
            //Of all points, find the two points that are furthes away from each other
            Edge3 eFurthestApart = FindEdgeFurthestApart(points);

            //Remove the two points we found         
            points.Remove(eFurthestApart.p1);
            points.Remove(eFurthestApart.p2);


            //Find a point which is the furthest away from this edge
            MyVector3 pointFurthestAway = FindPointFurthestFromEdge(eFurthestApart, points);

            //Remove the point
            points.Remove(pointFurthestAway);

            Debug.DrawLine(eFurthestApart.p1.ToVector3(), eFurthestApart.p2.ToVector3(), Color.white, 1f);
            Debug.DrawLine(eFurthestApart.p1.ToVector3(), pointFurthestAway.ToVector3(), Color.blue, 1f);
            Debug.DrawLine(eFurthestApart.p2.ToVector3(), pointFurthestAway.ToVector3(), Color.blue, 1f);


            //Now we can build two triangles
            //It doesnt matter how we build these triangles as long as they are opposite
            //But the normal matters, so make sure it is calculated so the triangles are ordered clockwise while the normal is pointing out
            MyVector3 p1 = eFurthestApart.p1;
            MyVector3 p2 = eFurthestApart.p2;
            MyVector3 p3 = pointFurthestAway;

            //convexHull.AddTriangle(p1, p2, p3);
            //convexHull.AddTriangle(p1, p3, p2);

            //Debug.Log(convexHull.faces.Count);
            /*
            foreach (HalfEdgeFace3 f in convexHull.faces)
            {
                Vector3 p1_test = f.edge.v.position.ToVector3();
                Vector3 p2_test = f.edge.nextEdge.v.position.ToVector3();
                Vector3 p3_test = f.edge.nextEdge.nextEdge.v.position.ToVector3();

                Vector3 normal = f.edge.v.normal.ToVector3();

                TestAlgorithmsHelpMethods.DebugDrawTriangle(p1_test, p2_test, p3_test, normal * 0.5f, Color.white, Color.red);

                //To test the the triangle is clock-wise
                TestAlgorithmsHelpMethods.DebugDrawCircle(p1_test, 0.1f, Color.red);
                TestAlgorithmsHelpMethods.DebugDrawCircle(p2_test, 0.2f, Color.blue);
            }
            */
        }



        //From a list of points, find the two points that are furthest away from each other
        private static Edge3 FindEdgeFurthestApart(HashSet<MyVector3> pointsHashSet)
        {
            List<MyVector3> points = new List<MyVector3>(pointsHashSet);
        

            //Find all possible combinations of edges between all points
            //TODO: Better to first find the points on the hull???
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

            //Init by picking the first edge
            Edge3 eFurthestApart = pointCombinations[0];

            float maxDistanceBetween = MyVector3.SqrDistance(eFurthestApart.p1, eFurthestApart.p2);

            //Try to find a better edge
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

            return eFurthestApart;
        }



        //Given an edge and a list of points, find the point furthest away from the edge
        private static MyVector3 FindPointFurthestFromEdge(Edge3 edge, HashSet<MyVector3> pointsHashSet)
        {
            List<MyVector3> points = new List<MyVector3>(pointsHashSet);

            //Init with the first point
            MyVector3 pointFurthestAway = points[0];

            MyVector3 closestPointOnLine = _Geometry.GetClosestPointOnLine(edge, pointFurthestAway, withinSegment: false);

            float maxDistSqr = MyVector3.SqrDistance(pointFurthestAway, closestPointOnLine);

            //Try to find a better point
            for (int i = 1; i < points.Count; i++)
            {
                MyVector3 thisPoint = points[i];

                //TODO make sure that thisPoint is NOT colinear with the edge because then we wont be able to build a triangle

                closestPointOnLine = _Geometry.GetClosestPointOnLine(edge, thisPoint, withinSegment: false);

                float distSqr = MyVector3.SqrDistance(thisPoint, closestPointOnLine);

                if (distSqr > maxDistSqr)
                {
                    maxDistSqr = distSqr;

                    pointFurthestAway = thisPoint;
                }
            }


            return pointFurthestAway;
        }
    }
}
