using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Important that input is hashset because cant handle duplicates 
namespace Habrador_Computational_Geometry
{
    public static class _Delaunay
    {
        //
        // Delaunay
        //

        //Algorithm 1. Triangulate the points with some algorithm - then flip edges until we have a delaunay triangulation
        public static HalfEdgeData2 FlippingEdges(HashSet<MyVector2> points, HalfEdgeData2 triangleData)
        {
            triangleData = DelaunayFlipEdges.GenerateTriangulation(points, triangleData);

            return triangleData;
        }


        //Algorithm 2. Start with one triangle covering all points - then insert the points one-by-one while flipping edges
        //From the report "A fast algorithm for constructing Delaunay triangulations in the plane" by Sloan
        public static HalfEdgeData2 PointByPoint(HashSet<MyVector2> points, HalfEdgeData2 triangleData)
        {
            triangleData = DelaunayIncrementalSloan.GenerateTriangulation(points, triangleData);

            return triangleData;
        }



        //
        // Constrained Delaunay
        //

        //Algorithm 1. From the report "An algorithm for generating constrained delaunay triangulations" by Sloan
        //Start with a delaunay triangulation of all points, including the constraints
        //Then flip edges to make sure the constrains are in the triangulation
        //Then remove the unwanted triangles within the constraints (if we want to)
        // - sites: just some points
        // Constraints:
        // - hull: remove all triangles outside of the hull, should be ordered counter-clock-wise
        // - holes: remove all triangles within the holes, should be ordered clock-wise 
        public static HalfEdgeData2 ConstrainedBySloan(HashSet<MyVector2> points, List<MyVector2> hull, HashSet<List<MyVector2>> holes, bool shouldRemoveTriangles, HalfEdgeData2 triangleData)
        {
            ConstrainedDelaunaySloan.GenerateTriangulation(points, hull, holes, shouldRemoveTriangles, triangleData);

            return triangleData;
        }



        //
        // Dynamic constrained delaunay
        //

        //TODO
    }
}
