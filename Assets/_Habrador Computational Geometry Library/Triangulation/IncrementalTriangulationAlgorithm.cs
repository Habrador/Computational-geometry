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
        public static List<Triangle> TriangulatePoints(List<Vector3> points)
        {
            List<Triangle> triangles = new List<Triangle>();

            //Sort the points along x-axis
            //OrderBy is always soring in ascending order - use OrderByDescending to get in the other order
            points = points.OrderBy(n => n.x).ToList();

            //The first 3 vertices are always forming a triangle
            Triangle newTriangle = new Triangle(points[0], points[1], points[2]);

            triangles.Add(newTriangle);

            //All edges that form the triangles, so we have something to test against
            List<Edge> edges = new List<Edge>();

            edges.Add(new Edge(newTriangle.p1, newTriangle.p2));
            edges.Add(new Edge(newTriangle.p2, newTriangle.p3));
            edges.Add(new Edge(newTriangle.p3, newTriangle.p1));

            //Add the other triangles one by one
            //Starts at 3 because we have already added 0,1,2
            for (int i = 3; i < points.Count; i++)
            {
                Vector3 currentPoint = points[i];

                //The edges we add this loop or we will get stuck in an endless loop
                List<Edge> newEdges = new List<Edge>();

                //Is this edge visible? We only need to check if the midpoint of the edge is visible 
                for (int j = 0; j < edges.Count; j++)
                {
                    Edge currentEdge = edges[j];

                    Vector3 midPoint = (currentEdge.p1 + currentEdge.p2) / 2f;

                    Edge visibilityLine = new Edge(currentPoint, midPoint);

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
                        Edge edgeToPoint1 = new Edge(currentEdge.p1, currentPoint);
                        Edge edgeToPoint2 = new Edge(currentEdge.p2, currentPoint);

                        newEdges.Add(edgeToPoint1);
                        newEdges.Add(edgeToPoint2);

                        Triangle newTri = new Triangle(edgeToPoint1.p1, edgeToPoint1.p2, edgeToPoint2.p1);

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



        private static bool AreEdgesIntersecting(Edge edge1, Edge edge2)
        {
            Vector2 l1_p1 = edge1.p1.XZ();
            Vector2 l1_p2 = edge1.p2.XZ();

            Vector2 l2_p1 = edge2.p1.XZ();
            Vector2 l2_p2 = edge2.p2.XZ();

            bool isIntersecting = Intersections.AreLinesIntersecting(l1_p1, l1_p2, l2_p1, l2_p2, true);

            return isIntersecting;
        }
    }
}
