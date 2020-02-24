using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Triangulate random points and hulls (both convex and concave)
    //Delaunay is not part of this section
    public static class _TriangulatePoints
    {
        //
        // Points
        //

        //Triangulate random points by first generating the convex hull of the points, then triangulate the convex hull
        //and then add the other points one-by-one while splitting the triangle the point is in
        public static HashSet<Triangle2> TriangleSplitting(HashSet<MyVector2> points)
        {
            return TriangleSplittingAlgorithm.TriangulatePoints(points);
        }



        //Sort the points along one axis. The first 3 points form a triangle. Consider the next point and connect it with all
        //previously connected points which are visible to the point. An edge is visible if the center of the edge is visible to the point
        public static HashSet<Triangle2> IncrementalTriangulation(HashSet<MyVector2> points)
        {
            return IncrementalTriangulationAlgorithm.TriangulatePoints(points);
        }



        //
        // Hull
        //

        //Triangulate a convex hull
        //Input should always be a list with the points on the convex hull sorted in clockwise or counter-clockwise order
        //or the algorithm will not work
        public static HashSet<Triangle2> PointsOnConvexHull(List<MyVector2> pointsOnConvexHull)
        {
            HashSet<Triangle2> triangles = TriangulateConvexHull.GetTriangles(pointsOnConvexHull);

            return triangles;
        }
    }
}
