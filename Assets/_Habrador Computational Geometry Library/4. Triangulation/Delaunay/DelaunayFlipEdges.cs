using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Create a delaunay triangulation by flipping edges
    //Simple but time consuming
    public class DelaunayFlipEdges
    {
        public static HalfEdgeData GenerateTriangulation(HashSet<Vector3> points, HalfEdgeData triangleData)
        {
            //Step 1. Triangulate the points with some algorithm
            //List<Triangle> triangles = TriangulatePoints.IncrementalTriangulation(points);
            HashSet<Triangle> triangles = _TriangulatePoints.TriangleSplitting(points);

            //Step 2. Change the structure from triangle to half-edge to make it faster to flip edges
            triangleData = TransformBetweenDataStructures.TransformFromTriangleToHalfEdge(triangles, triangleData);

            //Step 3. Flip edges until we have a delaunay triangulation
            FlipEdges(triangleData);

            return triangleData;
        }



        //Flip edges until we get a delaunay triangulation (or something close to it)
        private static void FlipEdges(HalfEdgeData triangleData)
        {
            //The edges we want to flip
            HashSet<HalfEdge> edges = triangleData.edges;


            int safety = 0;

            int flippedEdges = 0;

            while (true)
            {
                safety += 1;

                if (safety > 100000)
                {
                    Debug.Log("Stuck in endless loop when flipping edges");

                    break;
                }

                bool hasFlippedEdge = false;

                //Search through all edges to see if we can flip an edge
                foreach (HalfEdge thisEdge in edges)
                {
                    //Is this edge sharing an edge with another triangle, otherwise its a border, and then we cant flip the edge
                    if (thisEdge.oppositeEdge == null)
                    {
                        continue;
                    }

                    //The positions in 2d space of the vertices belonging to the two triangles that we might flip
                    //a-c should be the edge that we might flip
                    Vector2 aPos = thisEdge.v.position.XZ();
                    Vector2 bPos = thisEdge.nextEdge.v.position.XZ();
                    Vector2 cPos = thisEdge.nextEdge.nextEdge.v.position.XZ();
                    Vector2 dPos = thisEdge.oppositeEdge.nextEdge.v.position.XZ();

                    //Test if we should flip this edge
                    if (_Delaunay.ShouldFlipEdge(aPos, bPos, cPos, dPos))
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
