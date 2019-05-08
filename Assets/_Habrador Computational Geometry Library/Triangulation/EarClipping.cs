using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Habrador_Computational_Geometry
{
    //NOT NEEDED BECAUSE CAN DO THE SAME WITH CONSTRAINED DELAUNAY SO BETTER TO USE JUST ONE ALGORITHM
    public class EarClipping
    {
        //
        // Triangulate a concave hull
        //
        //This assumes that we have a hull and now we want to triangulate it
        //The points on the hull should be ordered counter-clockwise
        //This alorithm is called ear clipping and it's O(n*n) Another common algorithm is dividing it into trapezoids and it's O(n log n)
        //One can maybe do it in O(n) time but no such version is known
        //Assumes we have at least 3 points
        //The returned triangles have the correct orientation, so we dont need to test if all triangles have the correct
        //orientation when making a mesh
        //public static List<TriangleOld> TriangulateConcavePolygon(List<Vector3> points)
        //{
        //    //Debug.Log();

        //    //The list with triangles the method returns
        //    List<TriangleOld> triangles = new List<TriangleOld>();

        //    //If we just have three points, then we dont have to do all calculations
        //    if (points.Count == 3)
        //    {
        //        triangles.Add(new TriangleOld(points[0], points[2], points[1]));

        //        return triangles;
        //    }



        //    //Step 1. Store the vertices in a list and we also need to know the next and prev vertex
        //    List<Vertex> vertices = new List<Vertex>();

        //    for (int i = 0; i < points.Count; i++)
        //    {
        //        vertices.Add(new Vertex(points[i]));
        //    }

        //    //Find the next and previous vertex
        //    for (int i = 0; i < vertices.Count; i++)
        //    {
        //        int nextPos = MathUtility.ClampListIndex(i + 1, vertices.Count);

        //        int prevPos = MathUtility.ClampListIndex(i - 1, vertices.Count);

        //        vertices[i].prevVertex = vertices[prevPos];

        //        vertices[i].nextVertex = vertices[nextPos];
        //    }



        //    //Step 2. Find the reflex (concave) and convex vertices, and ear vertices
        //    for (int i = 0; i < vertices.Count; i++)
        //    {
        //        CheckIfReflexOrConvex(vertices[i]);
        //    }

        //    //Have to find the ears after we have found if the vertex is reflex or convex 
        //    //Do we really need an ear list???
        //    List<Vertex> earVertices = new List<Vertex>();

        //    for (int i = 0; i < vertices.Count; i++)
        //    {
        //        IsVertexEar(vertices[i], vertices, earVertices);
        //    }



        //    //Step 3. Triangulate!
        //    int safety = 0;

        //    while (true)
        //    {
        //        //This means we have just one triangle left
        //        if (vertices.Count == 3)
        //        {
        //            //The final triangle
        //            triangles.Add(new TriangleOld(vertices[0], vertices[0].prevVertex, vertices[0].nextVertex));
        //            //triangles.Add(new Triangle(vertices[0], vertices[2], vertices[1]));

        //            break;
        //        }

        //        //Make a triangle of the first ear
        //        Vertex earVertex = earVertices[0];

        //        Vertex earVertexPrev = earVertex.prevVertex;
        //        Vertex earVertexNext = earVertex.nextVertex;

        //        TriangleOld newTriangle = new TriangleOld(earVertex, earVertexPrev, earVertexNext);

        //        triangles.Add(newTriangle);

        //        //Remove the vertex from the lists
        //        earVertices.Remove(earVertex);

        //        vertices.Remove(earVertex);

        //        //Update the previous vertex and next vertex
        //        earVertexPrev.nextVertex = earVertexNext;
        //        earVertexNext.prevVertex = earVertexPrev;

        //        //...see if we have found a new ear by investigating the two vertices that was part of the ear
        //        CheckIfReflexOrConvex(earVertexPrev);
        //        CheckIfReflexOrConvex(earVertexNext);

        //        earVertices.Remove(earVertexPrev);
        //        earVertices.Remove(earVertexNext);

        //        IsVertexEar(earVertexPrev, vertices, earVertices);
        //        IsVertexEar(earVertexNext, vertices, earVertices);



        //        safety += 1;

        //        if (safety > 100000)
        //        {
        //            Debug.Log("Stuck in endless loop when triangulating concave polygon");

        //            //for (int i = 0; i < vertices.Count; i++)
        //            //{
        //            //    Gizmos.DrawWireSphere(vertices[i].position, 1f);
        //            //}

        //            //for (int i = 0; i < reflexVertices.Count; i++)
        //            //{
        //            //    Gizmos.DrawWireSphere(reflexVertices[i].position, 1f);
        //            //}

        //            //for (int i = 0; i < earVertices.Count; i++)
        //            //{
        //            //    Gizmos.DrawWireSphere(earVertices[i].position, 1f);
        //            //}

        //            break;
        //        }
        //    }

        //    //Debug.Log(triangles.Count);

        //    return triangles;
        //}



        ////Check if a vertex if reflex or convex, and add to appropriate list
        //private static void CheckIfReflexOrConvex(Vertex v)
        //{
        //    v.isReflex = false;
        //    v.isConvex = false;

        //    //This is a reflex vertex if its triangle is oriented clockwise
        //    Vector2 a = v.prevVertex.GetPos2D_XZ();
        //    Vector2 b = v.GetPos2D_XZ();
        //    Vector2 c = v.nextVertex.GetPos2D_XZ();

        //    if (Geometry.IsTriangleOrientedClockwise(a, b, c))
        //    {
        //        v.isReflex = true;
        //    }
        //    else
        //    {
        //        v.isConvex = true;
        //    }
        //}



        ////Check if a vertex is an ear
        ////We will check vertices that are part of the triangle belonging to v, maybe change that in the future???
        //private static void IsVertexEar(Vertex v, List<Vertex> vertices, List<Vertex> earVertices)
        //{
        //    //A reflex vertex cant be an ear!
        //    if (v.isReflex)
        //    {
        //        return;
        //    }

        //    //This triangle to check point in triangle
        //    Vector2 a = v.prevVertex.GetPos2D_XZ();
        //    Vector2 b = v.GetPos2D_XZ();
        //    Vector2 c = v.nextVertex.GetPos2D_XZ();

        //    bool hasPointInside = false;

        //    for (int i = 0; i < vertices.Count; i++)
        //    {
        //        //We only need to check if a reflex vertex is inside of the triangle
        //        if (vertices[i].isReflex)
        //        {
        //            Vector2 p = vertices[i].GetPos2D_XZ();

        //            //This means inside and not on the hull
        //            if (Intersections.IsPointInTriangle(a, b, c, p))
        //            {
        //                hasPointInside = true;

        //                break;
        //            }
        //        }
        //    }

        //    if (!hasPointInside)
        //    {
        //        earVertices.Add(v);
        //    }
        //}
    }
}
