using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Habrador_Computational_Geometry
{
    //Sort the points along one axis. The first 3 points form a triangle. Consider the next point and connect it with all
    //previously connected edges which are visible to the point. 
    //We assume an edge is visible if the center of the edge is visible to the point.
    public static class IncrementalTriangulationAlgorithm
    {
        public static List<Triangle2> TriangulatePoints(List<MyVector2> points)
        {
            List<Triangle2> triangles = new List<Triangle2>();

            //Sort the points along x-axis
            //OrderBy is always soring in ascending order - use OrderByDescending to get in the other order
            points = points.OrderBy(n => n.x).ToList();

            //The first 3 vertices are always forming a triangle
            Triangle2 newTriangle = new Triangle2(points[0], points[1], points[2]);

            triangles.Add(newTriangle);

            //All edges that form the triangles, so we have something to test against
            List<Edge2> edges = new List<Edge2>();

            edges.Add(new Edge2(newTriangle.p1, newTriangle.p2));
            edges.Add(new Edge2(newTriangle.p2, newTriangle.p3));
            edges.Add(new Edge2(newTriangle.p3, newTriangle.p1));

            //Add the other triangles one by one
            //Starts at 3 because we have already added 0,1,2
            for (int i = 3; i < points.Count; i++)
            {
                MyVector2 currentPoint = points[i];

                //The edges we add this loop or we will get stuck in an endless loop
                List<Edge2> newEdges = new List<Edge2>();

                //Is this edge visible? We only need to check if the midpoint of the edge is visible 
                for (int j = 0; j < edges.Count; j++)
                {
                    Edge2 currentEdge = edges[j];

                    MyVector2 midPoint = (currentEdge.p1 + currentEdge.p2) * 0.5f;

                    Edge2 visibilityLine = new Edge2(currentPoint, midPoint);

                    //Check if the visibility line is intersecting with any of the other edges
                    bool canSeeEdge = true;

                    for (int k = 0; k < edges.Count; k++)
                    {
                        //Dont compare the visibility line with the edge we are drawing the line from because they are intersecting
                        if (k == j)
                        {
                            continue;
                        }

                        if (AreEdgesIntersecting(visibilityLine, edges[k]))
                        {
                            canSeeEdge = false;

                            break;
                        }
                    }

                    //This is a valid triangle
                    if (canSeeEdge)
                    {
                        Edge2 edgeToPoint1 = new Edge2(currentEdge.p1, currentPoint);
                        Edge2 edgeToPoint2 = new Edge2(currentEdge.p2, currentPoint);

                        newEdges.Add(edgeToPoint1);
                        newEdges.Add(edgeToPoint2);

                        Triangle2 newTri = new Triangle2(edgeToPoint1.p1, edgeToPoint1.p2, edgeToPoint2.p1);

                        triangles.Add(newTri);
                    }
                }


                for (int j = 0; j < newEdges.Count; j++)
                {
                    edges.Add(newEdges[j]);
                }
            }


            return triangles;
        }



        private static bool AreEdgesIntersecting(Edge2 e1, Edge2 e2)
        {
            MyVector2 l1_p1 = e1.p1;
            MyVector2 l1_p2 = e1.p2;

            MyVector2 l2_p1 = e2.p1;
            MyVector2 l2_p2 = e2.p2;

            bool isIntersecting = Intersections.LineLine(l1_p1, l1_p2, l2_p1, l2_p2, true);

            return isIntersecting;
        }
    }
}
