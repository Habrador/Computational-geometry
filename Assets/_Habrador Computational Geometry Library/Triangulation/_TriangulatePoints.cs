using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Triangulate random points and hulls (both convex and concave)
    //Delaunay is not a part of this triangulation section
    public static class _TriangulatePoints
    {
        //
        // Points
        //

        //Triangulate random points by first generating the convex hull of the points, then triangulate the convex hull
        //and then add the other points and split the triangle the point is in
        public static HashSet<Triangle> TriangleSplitting(HashSet<Vector3> points)
        {
            return TriangleSplittingAlgorithm.TriangulatePoints(points);
        }



        //Sort the points along one axis. The first 3 points form a triangle. Consider the next point and connect it with all
        //previously connected points which are visible to the point. An edge is visible if the center of the edge is visible to the point
        public static List<Triangle> IncrementalTriangulation(List<Vector3> points)
        {
            return IncrementalTriangulationAlgorithm.TriangulatePoints(points);
        }



        //
        // Hull
        //

        //Triangulate a convex hull
        public static HashSet<Triangle> TriangulateConvexHullAlgorithm(List<Vector3> pointsOnConvexHull)
        {
            HashSet<Triangle> triangles = TriangulateConvexHull.GetTriangles(pointsOnConvexHull);

            return triangles;
        }

        //Triangulate a concave hull with ear clipping
    }
}
