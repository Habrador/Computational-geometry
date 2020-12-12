using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Help enum, to make it easier to deal with three cases: intersecting inside, intersecting on edge, not intersecting 
    //If we have two cases we can just return a bool
    public enum IntersectionCases
    {
        IsInside,
        IsOnEdge,
        NoIntersection
    }

    public static class _Intersections
    {
        //
        // Are two lines intersecting?
        //
        //http://thirdpartyninjas.com/blog/2008/10/07/line-segment-intersection/
        //Notice that there are more than one way to test if two line segments are intersecting
        //but this is the fastest according to https://www.habrador.com/tutorials/math/5-line-line-intersection/
        public static bool LineLine(Edge2 a, Edge2 b, bool includeEndPoints)
        {
            //To avoid floating point precision issues we can use a small value
            float epsilon = MathUtility.EPSILON;

            bool isIntersecting = false;

            float denominator = (b.p2.y - b.p1.y) * (a.p2.x - a.p1.x) - (b.p2.x - b.p1.x) * (a.p2.y - a.p1.y);

            //Make sure the denominator is != 0 (or the lines are parallel)
            if (denominator > 0f + epsilon || denominator < 0f - epsilon)
            {
                float u_a = ((b.p2.x - b.p1.x) * (a.p1.y - b.p1.y) - (b.p2.y - b.p1.y) * (a.p1.x - b.p1.x)) / denominator;
                float u_b = ((a.p2.x - a.p1.x) * (a.p1.y - b.p1.y) - (a.p2.y - a.p1.y) * (a.p1.x - b.p1.x)) / denominator;

                //Are the line segments intersecting if the end points are the same
                if (includeEndPoints)
                {
                    //The only difference between endpoints not included is the =, which will never happen so we have to subtract 0 by epsilon
                    float zero = 0f - epsilon;
                    float one = 1f + epsilon;

                    //Are intersecting if u_a and u_b are between 0 and 1 or exactly 0 or 1
                    if (u_a >= zero && u_a <= one && u_b >= zero && u_b <= one)
                    {
                        isIntersecting = true;
                    }
                }
                else
                {
                    float zero = 0f + epsilon;
                    float one = 1f - epsilon;

                    //Are intersecting if u_a and u_b are between 0 and 1
                    if (u_a > zero && u_a < one && u_b > zero && u_b < one)
                    {
                        isIntersecting = true;
                    }
                }

            }

            return isIntersecting;
        }



        //Whats the coordinate of the intersection point between two lines in 2d space if we know they are intersecting
        //http://thirdpartyninjas.com/blog/2008/10/07/line-segment-intersection/        
        public static MyVector2 GetLineLineIntersectionPoint(Edge2 a, Edge2 b)
        {
            float denominator = (b.p2.y - b.p1.y) * (a.p2.x - a.p1.x) - (b.p2.x - b.p1.x) * (a.p2.y - a.p1.y);

            float u_a = ((b.p2.x - b.p1.x) * (a.p1.y - b.p1.y) - (b.p2.y - b.p1.y) * (a.p1.x - b.p1.x)) / denominator;

            MyVector2 intersectionPoint = a.p1 + u_a * (a.p2 - a.p1);

            return intersectionPoint;
        }



        //
        // Line, plane, ray intersection with plane
        //

        //This is a useful method to find the intersection coordinate if we know we are intersecting
        //Is used for ray-plane, line-plane, plane-plane
        //2d
        private static MyVector2 GetIntersectionCoordinate(Plane2 plane, Ray2 ray)
        {
            float denominator = MyVector2.Dot(-plane.normal, ray.dir);

            MyVector2 vecBetween = plane.pos - ray.origin;

            float t = MyVector2.Dot(vecBetween, -plane.normal) / denominator;

            MyVector2 intersectionPoint = ray.origin + ray.dir * t;

            return intersectionPoint;
        }
        
        //3d
        private static MyVector3 GetIntersectionCoordinate(Plane3 plane, Ray3 ray)
        {
            float denominator = MyVector3.Dot(-plane.normal, ray.dir);

            MyVector3 vecBetween = plane.pos - ray.origin;

            float t = MyVector3.Dot(vecBetween, -plane.normal) / denominator;

            MyVector3 intersectionPoint = ray.origin + ray.dir * t;

            return intersectionPoint;
        }



        //
        // Ray-plane intersection
        // http://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-plane-and-ray-disk-intersection
        //

        //2d
        public static bool RayPlane(Plane2 plane, Ray2 ray)
        {
            //To avoid floating point precision issues we can add a small value
            float epsilon = MathUtility.EPSILON;

            bool areIntersecting = false;

            float denominator = MyVector2.Dot(plane.normal * -1f, ray.dir);

            //Debug.Log(denominator);

            //The ray has to point at the surface of the plane
            //The surface of the plane is determined by the normal
            if (denominator > epsilon)
            {
                //Now we have to figur out of the ray starts "inside" of the plane
                //meaning on the other side of the normal
                //If so it can't hit the plane
                MyVector2 vecBetween = plane.pos - ray.origin;

                float t = MyVector2.Dot(vecBetween, plane.normal * -1f) / denominator;

                //Debug.Log(t);

                if (t >= 0f)
                {
                    areIntersecting = true;
                }
            }

            return areIntersecting;
        }

        //Get the coordinate if we know a ray-plane is intersecting
        //2d
        public static MyVector2 GetRayPlaneIntersectionPoint(Plane2 plane, Ray2 ray)
        {
            MyVector2 intersectionPoint = GetIntersectionCoordinate(plane, ray);

            return intersectionPoint;
        }



        //
        // Line-plane intersection
        //

        //2d
        public static bool LinePlane(Plane2 plane, Edge2 line)
        {
            //To avoid floating point precision issues we can add a small value
            float epsilon = MathUtility.EPSILON;

            bool areIntersecting = false;

            MyVector2 lineDir = MyVector2.Normalize(line.p1 - line.p2);

            float denominator = MyVector2.Dot(-plane.normal, lineDir);

            //Debug.Log(denominator);

            //No intersection if the line and plane are perpendicular
            if (denominator > epsilon || denominator < -epsilon)
            {
                MyVector2 vecBetween = plane.pos - line.p1;

                float t = MyVector2.Dot(vecBetween, -plane.normal) / denominator;

                MyVector2 intersectionPoint = line.p1 + lineDir * t;

                //Gizmos.DrawWireSphere(intersectionPoint, 0.5f);

                if (_Geometry.IsPointBetweenPoints(line.p1, line.p2, intersectionPoint))
                {
                    areIntersecting = true;
                }
            }

            return areIntersecting;
        }

        //We know a line plane is intersecting and now we want the coordinate of intersection
        //2d
        public static MyVector2 GetLinePlaneIntersectionPoint(Plane2 plane, Edge2 line)
        {
            MyVector2 lineDir = MyVector2.Normalize(line.p1 - line.p2);

            Ray2 ray = new Ray2(line.p1, lineDir);

            MyVector2 intersectionPoint = GetIntersectionCoordinate(plane, ray);

            return intersectionPoint;
        }

        //3d
        public static MyVector3 GetLinePlaneIntersectionPoint(Plane3 plane, Edge3 line)
        {
            MyVector3 lineDir = MyVector3.Normalize(line.p1 - line.p2);

            Ray3 ray = new Ray3(line.p1, lineDir);

            MyVector3 intersectionPoint = GetIntersectionCoordinate(plane, ray);

            return intersectionPoint;
        }



        //
        // Plane-plane intersection
        //
        //2d
        public static bool PlanePlane(Plane2 plane_1, Plane2 plane_2)
        {
            bool areIntersecting = false;

            float dot = MyVector2.Dot(plane_1.normal, plane_2.normal);

            //Debug.Log(dot);

            //No intersection if the planes are parallell
            //The are parallell if the dot product is 1 or -1

            //To avoid floating point precision issues we can add a small value
            float one = 1f - MathUtility.EPSILON;

            if (dot < one && dot > -one)
            {
                areIntersecting = true;
            }

            return areIntersecting;
        }

        //If we know two planes are intersecting, what's the point of intersection?
        //2d
        public static MyVector2 GetPlanePlaneIntersectionPoint(Plane2 plane_1, Plane2 plane_2)
        {
            MyVector2 lineDir = MyVector2.Normalize(new MyVector2(plane_2.normal.y, -plane_2.normal.x));

            Ray2 ray = new Ray2(plane_2.pos, lineDir);

            MyVector2 intersectionPoint = GetIntersectionCoordinate(plane_1, ray);

            return intersectionPoint;
        }



        //
        // Is a point inside a triangle?
        //
        //From http://totologic.blogspot.se/2014/01/accurate-point-in-triangle-test.html
        public static bool PointTriangle(Triangle2 t, MyVector2 p, bool includeBorder)
        {
            //To avoid floating point precision issues we can add a small value
            float epsilon = MathUtility.EPSILON;

            //Based on Barycentric coordinates
            float denominator = ((t.p2.y - t.p3.y) * (t.p1.x - t.p3.x) + (t.p3.x - t.p2.x) * (t.p1.y - t.p3.y));

            float a = ((t.p2.y - t.p3.y) * (p.x - t.p3.x) + (t.p3.x - t.p2.x) * (p.y - t.p3.y)) / denominator;
            float b = ((t.p3.y - t.p1.y) * (p.x - t.p3.x) + (t.p1.x - t.p3.x) * (p.y - t.p3.y)) / denominator;
            float c = 1 - a - b;

            bool isWithinTriangle = false;

            if (includeBorder)
            {
                float zero = 0f - epsilon;
                float one = 1f + epsilon;

                //The point is within the triangle or on the border
                if (a >= zero && a <= one && b >= zero && b <= one && c >= zero && c <= one)
                {
                    isWithinTriangle = true;
                }
            }
            else
            {
                float zero = 0f + epsilon;
                float one = 1f - epsilon;

                //The point is within the triangle
                if (a > zero && a < one && b > zero && b < one && c > zero && c < one)
                {
                    isWithinTriangle = true;
                }
            }

            return isWithinTriangle;
        }


        //Is a point inside, outside, or on the border of a triangle
        //-1 if outside, 0 if on the border, 1 if inside the triangle
        //BROKEN use if the point is to the left or right of all edges in the triangle
        //public static int IsPointInOutsideOnTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
        //{
        //    //To avoid floating point precision issues we can add a small value
        //    float epsilon = MathUtility.EPSILON;

        //    float zero = 0f + epsilon;
        //    float one = 1f - epsilon;

        //    //Based on Barycentric coordinates
        //    float denominator = ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));

        //    float a = ((p2.y - p3.y) * (p.x - p3.x) + (p3.x - p2.x) * (p.y - p3.y)) / denominator;
        //    float b = ((p3.y - p1.y) * (p.x - p3.x) + (p1.x - p3.x) * (p.y - p3.y)) / denominator;
        //    float c = 1 - a - b;

        //    int returnValue = -1;

        //    //The point is on the border, meaning exactly 0 or 1 in a world with no floating precision issues

        //    //The point is on or within the triangle
        //    if (a >= zero && a <= one && b >= zero && b <= one && c >= zero && c <= one)
        //    {
        //        returnValue = 1;
        //    }

        //    return returnValue;
        //}



        //
        // Is a triangle inside a triangle
        //
        //Is triangle 1 inside triangle 2?
        public static bool IsTriangleInsideTriangle(Triangle2 t1, Triangle2 t2)
        {
            bool isWithin = false;

            //Test if each vertex is inside the triangle
            if (
                PointTriangle(t2, t1.p1, false) &&
                PointTriangle(t2, t1.p2, false) &&
                PointTriangle(t2, t1.p3, false))
            {
                isWithin = true;
            }

            return isWithin;
        }



        //
        // Are two Axis-aligned-bounding-box (boxes are here rectangles) intersecting?
        //
        
        //2d
        public static bool AABB_AABB(AABB2 r1, AABB2 r2)
        {
            //If the min of one box in one dimension is greater than the max of another box then the boxes are not intersecting
            //They have to intersect in 2 dimensions. We have to test if box 1 is to the left or box 2 and vice versa
            bool isIntersecting = true;

            //X axis
            ///r1_minX - the smallest x-coordinate of all corners belonging to rectangle 1
            if (r1.min.x > r2.max.x)
            {
                isIntersecting = false;
            }
            else if (r2.min.x > r1.max.x)
            {
                isIntersecting = false;
            }
            //Y axis
            else if (r1.min.y > r2.max.y)
            {
                isIntersecting = false;
            }
            else if (r2.min.y > r1.max.y)
            {
                isIntersecting = false;
            }

            return isIntersecting;
        }



        //
        // Is a point intersecting with a circle?
        //
        //Is a point d inside, outside or on the same circle where a, b, c are all on the circle's edge
        public static IntersectionCases PointCircle(MyVector2 a, MyVector2 b, MyVector2 c, MyVector2 testPoint)
        {
            //Center of circle
            MyVector2 circleCenter = _Geometry.CalculateCircleCenter(a, b, c);

            //The radius sqr of the circle
            float radiusSqr = MyVector2.SqrDistance(a, circleCenter);

            //The distance sqr from the point to the circle center
            float distPointCenterSqr = MyVector2.SqrDistance(testPoint, circleCenter);
            
            //Add/remove a small value becuse we will never be exactly on the edge because of floating point precision issues
            //Mutiply epsilon by two because we are using sqr root???
            if (distPointCenterSqr < radiusSqr - MathUtility.EPSILON * 2f)
            {
                return IntersectionCases.IsInside;
            }
            else if (distPointCenterSqr > radiusSqr + MathUtility.EPSILON * 2f)
            {
                return IntersectionCases.NoIntersection;
            }
            else
            {
                return IntersectionCases.IsOnEdge;
            }
        }



        //
        // Is a point intersecting with a polygon?
        //
        //The list describing the polygon has to be sorted either clockwise or counter-clockwise because we have to identify its edges
        //TODO: May sometimes fail because of floating point precision issues
        public static bool PointPolygon(List<MyVector2> polygonPoints, MyVector2 point)
        {
            //Step 1. Find a point outside of the polygon
            //Pick a point with a x position larger than the polygons max x position, which is always outside
            MyVector2 maxXPosVertex = polygonPoints[0];

            for (int i = 1; i < polygonPoints.Count; i++)
            {
                if (polygonPoints[i].x > maxXPosVertex.x)
                {
                    maxXPosVertex = polygonPoints[i];
                }
            }

            //The point should be outside so just pick a number to move it outside
            //Should also move it up a little to minimize floating point precision issues
            //This is where it fails if this line is exactly on a vertex
            MyVector2 pointOutside = maxXPosVertex + new MyVector2(1f, 0.01f);


            //Step 2. Create an edge between the point we want to test with the point thats outside
            MyVector2 l1_p1 = point;
            MyVector2 l1_p2 = pointOutside;

            //Debug.DrawLine(l1_p1.XYZ(), l1_p2.XYZ());


            //Step 3. Find out how many edges of the polygon this edge is intersecting with
            int numberOfIntersections = 0;

            for (int i = 0; i < polygonPoints.Count; i++)
            {
                //Line 2
                MyVector2 l2_p1 = polygonPoints[i];

                int iPlusOne = MathUtility.ClampListIndex(i + 1, polygonPoints.Count);

                MyVector2 l2_p2 = polygonPoints[iPlusOne];

                //Are the lines intersecting?
                if (_Intersections.LineLine(new Edge2(l1_p1, l1_p2), new Edge2(l2_p1, l2_p2), includeEndPoints: true))
                {
                    numberOfIntersections += 1;
                }
            }


            //Step 4. Is the point inside or outside?
            bool isInside = true;

            //The point is outside the polygon if number of intersections is even or 0
            if (numberOfIntersections == 0 || numberOfIntersections % 2 == 0)
            {
                isInside = false;
            }

            return isInside;
        }



        //
        // Is a point within a convex hull?
        //

        //If the point is on the hull it's "inside"
        public static bool PointWithinConvexHull(MyVector3 point, HalfEdgeData3 convexHull)
        {
            bool isInside = true;

            float epsilon = MathUtility.EPSILON;

            //We know a point is within the hull if the point is inside all planes formed by the faces of the hull
            foreach (HalfEdgeFace3 triangle in convexHull.faces)
            {
                //Build a plane
                Plane3 plane = new Plane3(triangle.edge.v.position, triangle.edge.v.normal);

                //Find the distance to the plane from the point
                //The distance is negative if the point is inside the plane
                float distance = _Geometry.GetSignedDistanceFromPointToPlane(point, plane);

                //This point is outside, which means we don't need to test more planes
                if (distance > 0f + epsilon)
                {
                    isInside = false;

                    break;
                }
            }

            return isInside;
        }



        //
        // Are two triangles intersecting?
        //

        //2d
        public static bool TriangleTriangle(Triangle2 t1, Triangle2 t2, bool do_AABB_test)
        {
            bool isIntersecting = false;

            //Step 0. AABB intersection which may speed up the algorithm if the triangles are far apart
            if (do_AABB_test)
            {
                //Rectangle that covers t1 
                AABB2 r1 = new AABB2(t1.MinX(), t1.MaxX(), t1.MinY(), t1.MaxY());

                //Rectangle that covers t2
                AABB2 r2 = new AABB2(t2.MinX(), t2.MaxX(), t2.MinY(), t2.MaxY());

                if (!AABB_AABB(r1, r2))
                {
                    return false;
                }
            }


            //Step 1. Line-line instersection

            Edge2 t1_e1 = new Edge2(t1.p1, t1.p2);
            Edge2 t1_e2 = new Edge2(t1.p2, t1.p3);
            Edge2 t1_e3 = new Edge2(t1.p3, t1.p1);

            Edge2 t2_e1 = new Edge2(t2.p1, t2.p2);
            Edge2 t2_e2 = new Edge2(t2.p2, t2.p3);
            Edge2 t2_e3 = new Edge2(t2.p3, t2.p1);

            //Line 1 of t1 against all lines of t2
            if (
                LineLine(t1_e1, t2_e1, true) ||
                LineLine(t1_e1, t2_e2, true) ||
                LineLine(t1_e1, t2_e3, true)
            )
            {
                isIntersecting = true;
            }
            //Line 2 of t1 against all lines of t2
            else if (
                LineLine(t1_e2, t2_e1, true) ||
                LineLine(t1_e2, t2_e2, true) ||
                LineLine(t1_e2, t2_e3, true)
            )
            {
                isIntersecting = true;
            }
            //Line 3 of t1 against all lines of t2
            else if (
                LineLine(t1_e3, t2_e1, true) ||
                LineLine(t1_e3, t2_e2, true) ||
                LineLine(t1_e3, t2_e3, true)
            )
            {
                isIntersecting = true;
            }

            //Now we can return if we are intersecting so we dont need to spend time testing something else
            if (isIntersecting)
            {
                return isIntersecting;
            }


            //Step 2. Point-in-triangle intersection
            //We only need to test one corner from each triangle
            //If this point is not in the triangle, then the other points can't be in the triangle, because if this point is outside
            //and another point is inside, then the line between them would have been covered by step 1: line-line intersections test
            if (PointTriangle(t2, t1.p1, true) || PointTriangle(t1, t2.p1, true))
            {
                isIntersecting = true;
            }


            return isIntersecting;
        }
    }
}
