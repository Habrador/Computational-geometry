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
        public static HalfEdgeData TriangulateByFlippingEdges(HashSet<Vector3> points, HalfEdgeData triangleData)
        {
            triangleData = DelaunayFlipEdges.GenerateTriangulation(points, triangleData);

            return triangleData;
        }


        //Alternative 2. Start with one triangle covering all points - then insert the points one-by-one while flipping edges
        //Requires just this algorithm and is not dependent on other algorithms
        public static HalfEdgeData TriangulatePointByPoint(HashSet<Vector3> points, HalfEdgeData triangleData)
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
        public static HalfEdgeData ConstrainedTriangulationWithSloan(HashSet<Vector3> sites, List<Vector3> obstacles, bool shouldRemoveTriangles, HalfEdgeData triangleData)
        {
            ConstrainedDelaunaySloan.GenerateTriangulation(sites, obstacles, shouldRemoveTriangles, triangleData);

            return triangleData;
        }



        //
        // Dynamic Constrained Delaunay
        //

        //Add constraints
        public static HalfEdgeData AddConstraintToConstrainedDelaunay(HalfEdgeData triangleData, Edge constraintToAdd, List<Edge> allConstraints)
        {        
            triangleData = DynamicConstrainedDelaunay.AddConstraint(triangleData, constraintToAdd, allConstraints);

            return triangleData;
        }

        //Remove constraints
        public static HalfEdgeData RemoveConstraintFromConstrainedDelaunay(HalfEdgeData triangleData, Edge constraintToRemove, List<Edge> allConstraints)
        {
            triangleData = DynamicConstrainedDelaunay.AddConstraint(triangleData, constraintToRemove, allConstraints);

            return triangleData;
        }


        //
        // Methods for all algorithms
        //

        //Test if we should flip an edge
        //a, b, c belongs to t1 and d is the point on the other triangle
        //a-c is the edge, which is important so we can flip it, by making the edge b-d
        public static bool ShouldFlipEdge(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            bool shouldFlipEdge = false;

            //Use the circle test to test if we need to flip this edge
            //We should flip if d is inside a circle formed by a, b, c
            float circleTestValue = Geometry.IsPointInsideOutsideOrOnCircle(a, b, c, d);

            if (circleTestValue < 0f)
            {
                //Are these the two triangles forming a convex quadrilateral? Otherwise the edge cant be flipped
                if (Geometry.IsQuadrilateralConvex(a, b, c, d))
                {
                    //If the new triangle after a flip is not better, then dont flip
                    //This will also stop the algoritm from ending up in an endless loop
                    if (Geometry.IsPointInsideOutsideOrOnCircle(b, c, d, a) <= circleTestValue)
                    {
                        shouldFlipEdge = false;
                    }
                    else
                    {
                        shouldFlipEdge = true;
                    }
                }
            }

            return shouldFlipEdge;
        }



        //From "A fast algortihm for generating constrained delaunay..."
        //Is numerically stable
        //v1, v2 should belong to the edge we ant to flip
        //v1, v2, v3 are counter-clockwise
        //Is this also checking if the edge can be swapped
        public static bool ShouldFlipEdgeStable(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 vp)
        {
            float x_13 = v1.x - v3.x;
            float x_23 = v2.x - v3.x;
            float x_1p = v1.x - vp.x;
            float x_2p = v2.x - vp.x;

            float y_13 = v1.y - v3.y;
            float y_23 = v2.y - v3.y;
            float y_1p = v1.y - vp.y;
            float y_2p = v2.y - vp.y;

            float cos_a = x_13 * x_23 + y_13 * y_23;
            float cos_b = x_2p * x_1p + y_2p * y_1p;

            if (cos_a >= 0f && cos_b >= 0f)
            {
                return false;
            }
            if (cos_a < 0f && cos_b < 0)
            {
                return true;
            }

            float sin_ab = (x_13 * y_23 - x_23 * y_13) * cos_b + (x_2p * y_1p - x_1p * y_2p) * cos_a;

            if (sin_ab < 0)
            {
                return true;
            }

            return false;
        }



        //Create a supertriangle that contains all other points
        //According to the book "Geometric tools for computer graphics" a reasonably sized triangle
        //is one that contains a circle that contains the axis-aligned bounding rectangle of the points 
        public static Triangle GetSupertriangle(HashSet<Vector3> points)
        {
            //Step 1. Create a AABB around the points
            float maxX = float.MinValue;
            float minX = float.MaxValue;
            float maxZ = float.MinValue;
            float minZ = float.MaxValue;

            foreach (Vector3 pos in points)
            {
                if (pos.x > maxX)
                {
                    maxX = pos.x;
                }
                else if (pos.x < minX)
                {
                    minX = pos.x;
                }

                if (pos.z > maxZ)
                {
                    maxZ = pos.z;
                }
                else if (pos.z < minZ)
                {
                    minZ = pos.z;
                }
            }

            Vector3 TL = new Vector3(minX, 0f, maxZ);
            Vector3 TR = new Vector3(maxX, 0f, maxZ);
            //Vector3 BL = new Vector3(minX, 0f, minZ);
            Vector3 BR = new Vector3(maxX, 0f, minZ);

            //Debug AABB
            //Gizmos.DrawLine(TL, TR);
            //Gizmos.DrawLine(TR, BR);
            //Gizmos.DrawLine(BR, BL);
            //Gizmos.DrawLine(BL, TL);



            //Step2. Find the inscribed circle - the smallest circle that surrounds the AABB
            Vector3 circleCenter = (TL + BR) * 0.5f;

            float circleRadius = (circleCenter - TR).magnitude;

            //Debug circle
            //Gizmos.DrawWireSphere(circleCenter, circleRadius);



            //Step 3. Create the smallest triangle that surrounds the circle
            //All edges of this triangle have the same length
            float halfSideLenghth = circleRadius / Mathf.Tan(30f * Mathf.Deg2Rad);

            //The center position of the bottom-edge
            Vector3 tri_B = new Vector3(circleCenter.x, 0f, circleCenter.z - circleRadius);

            Vector3 tri_BL = new Vector3(tri_B.x - halfSideLenghth, 0f, tri_B.z);
            Vector3 tri_BR = new Vector3(tri_B.x + halfSideLenghth, 0f, tri_B.z);

            //The height from the bottom edge to the top vertex
            float triangleHeight = halfSideLenghth * Mathf.Tan(60f * Mathf.Deg2Rad);

            Vector3 tri_T = new Vector3(circleCenter.x, 0f, tri_B.z + triangleHeight);

            //Debug
            //Gizmos.DrawLine(tri_BL, tri_BR);
            //Gizmos.DrawLine(tri_BL, tri_T);
            //Gizmos.DrawLine(tri_BR, tri_T);

            Triangle superTriangle = new Triangle(tri_BR, tri_BL, tri_T);

            return superTriangle;
        }
    }
}
