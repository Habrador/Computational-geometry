using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Create a delaunay triangulation by flipping edges
    //Simple but slow
    public class DelaunayFlipEdges
    {
        public static HalfEdgeData2 GenerateTriangulation(HashSet<MyVector2> points, HalfEdgeData2 triangleData)
        {
            //Step 1. Triangulate the points with some algorithm. The result is a convex triangulation
            //HashSet<Triangle2> triangles = _TriangulatePoints.VisibleEdgesTriangulation(points);
            HashSet<Triangle2> triangles = _TriangulatePoints.TriangleSplitting(points, addColinearPoints: true);

            //Step 2. Change the data structure from triangle to half-edge to make it easier to flip edges
            triangleData = _TransformBetweenDataStructures.Triangle2ToHalfEdge2(triangles, triangleData);

            //Step 3. Flip edges until we have a delaunay triangulation
            FlipEdges(triangleData);

            return triangleData;
        }



        //Flip edges until we get a delaunay triangulation
        private static void FlipEdges(HalfEdgeData2 triangleData)
        {
            //The edges we want to flip
            HashSet<HalfEdge2> edges = triangleData.edges;

            //To avoid getting stuck in infinite loop
            int safety = 0;

            //Count how many edges we have flipped, which may be interesting to display
            int flippedEdges = 0;

            while (true)
            {
                safety += 1;

                if (safety > 100000)
                {
                    Debug.Log("Stuck in endless loop when flipping edges to get a Delaunay triangulation");

                    break;
                }

                bool hasFlippedEdge = false;

                //Search through all edges to see if we can flip an edge
                foreach (HalfEdge2 thisEdge in edges)
                {
                    //Is this edge sharing an edge with another triangle, otherwise its a border, and then we cant flip the edge
                    if (thisEdge.oppositeEdge == null)
                    {
                        continue;
                    }

                    //The positions of the vertices belonging to the two triangles that we might flip
                    //a-c should be the edge that we might flip
                    MyVector2 a = thisEdge.v.position;
                    MyVector2 b = thisEdge.nextEdge.v.position;
                    MyVector2 c = thisEdge.nextEdge.nextEdge.v.position;
                    MyVector2 d = thisEdge.oppositeEdge.nextEdge.v.position;

                    //Test if we should flip this edge
                    if (DelaunayMethods.ShouldFlipEdge(a, b, c, d))
                    {
                        flippedEdges += 1;

                        hasFlippedEdge = true;

                        HalfEdgeHelpMethods.FlipTriangleEdge(thisEdge);
                    }

                }

                //We have searched through all edges and havent found an edge to flip, so we have a Delaunay triangulation!
                if (!hasFlippedEdge)
                {
                    Debug.Log("Found a delaunay triangulation in " + flippedEdges + " flips");

                    break;
                }
            }
        }
    }
}
