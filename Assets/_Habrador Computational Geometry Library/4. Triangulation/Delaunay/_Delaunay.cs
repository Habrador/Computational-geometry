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

        //Alternative 1. Triangulate with some algorithm - then flip edges until we have a delaunay triangulation
        //Is actually not simple beacuse it requires a convex hull algorithm, and a triangulate-points-algorithm
        //so it depends on other algorithms
        public static HalfEdgeData2 ByFlippingEdges(HashSet<MyVector2> points, HalfEdgeData2 triangleData)
        {
            triangleData = DelaunayFlipEdges.GenerateTriangulation(points, triangleData);

            return triangleData;
        }


        //Alternative 2. Start with one triangle covering all points - then insert the points one-by-one while flipping edges
        //Requires just this algorithm and is not dependent on other algorithms
        public static HalfEdgeData2 PointByPoint(HashSet<MyVector2> points, HalfEdgeData2 triangleData)
        {
            //From the report "A fast algorithm for constructing Delaunay triangulations in the plane" by Sloan
            triangleData = DelaunayIncrementalSloan.GenerateTriangulation(points, triangleData);

            return triangleData;
        }



        //
        // Constrained Delaunay
        //

        //Alternative 1. From the report "An algorithm for generating constrained delaunay triangulations" by Sloan
        //Start with a delaunay triangulation of all points, including the constraints
        //Then flip edges to make sure the constrains are in the triangulation
        //Then remove the unwanted triangles within the constraints if we want to
        public static HalfEdgeData2 ConstrainedBySloan(HashSet<MyVector2> sites, List<MyVector2> constraints, bool shouldRemoveTriangles, HalfEdgeData2 triangleData)
        {
            ConstrainedDelaunaySloan.GenerateTriangulation(sites, constraints, shouldRemoveTriangles, triangleData);

            return triangleData;
        }
    }
}
