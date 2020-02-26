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
        // Triangulate points on convex hull and points inside of convex hull
        //

        //Triangulate random points by: 
        //1. Generating the convex hull of the points
        //2. Triangulate the convex hull
        //3. Add the other points one-by-one while splitting the triangle the point is in
        public static HashSet<Triangle2> TriangleSplitting(HashSet<MyVector2> points)
        {
            return TriangleSplittingAlgorithm.TriangulatePoints(points);
        }



        //Triangulate random points by: 
        //1. Sort the points along one axis. The first 3 points form a triangle 
        //2. Consider the next point and connect it with all previously connected points which are visible to the point
        //3. Do 2 until we are out of points to add
        //Is not working with colinear points
        public static HashSet<Triangle2> IncrementalTriangulation(HashSet<MyVector2> points)
        {
            return IncrementalTriangulationAlgorithm.TriangulatePoints(points);
        }



        //
        // Triangulate points on convex hull
        //

        //Input should always be a list with the points on the convex hull sorted in clockwise or counter-clockwise order
        public static HashSet<Triangle2> PointsOnConvexHull(List<MyVector2> pointsOnConvexHull)
        {
            HashSet<Triangle2> triangles = TriangulateConvexHull.GetTriangles(pointsOnConvexHull);

            return triangles;
        }

        //Provide a point which is inside of the convex hull to make it easier to triangulate colinear points
        //And the triangles should be more "even", which can also be useful
        public static HashSet<Triangle2> PointsOnConvexHull(List<MyVector2> pointsOnConvexHull, MyVector2 insidePoint)
        {
            HashSet<Triangle2> triangles = TriangulateConvexHull.GetTriangles(pointsOnConvexHull, insidePoint);

            return triangles;
        }
    }
}
