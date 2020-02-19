using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class Intersections
    {
        //
        // Are two lines intersecting?
        //
        //http://thirdpartyninjas.com/blog/2008/10/07/line-segment-intersection/
        public static bool AreLinesIntersecting(Vector2 l1_p1, Vector2 l1_p2, Vector2 l2_p1, Vector2 l2_p2, bool shouldIncludeEndPoints)
        {
            //To avoid floating point precision issues we can add a small value
            float epsilon = MathUtility.EPSILON;

            bool isIntersecting = false;

            float denominator = (l2_p2.y - l2_p1.y) * (l1_p2.x - l1_p1.x) - (l2_p2.x - l2_p1.x) * (l1_p2.y - l1_p1.y);

            //Make sure the denominator is > 0, if so the lines are parallel
            if (denominator != 0f)
            {
                float u_a = ((l2_p2.x - l2_p1.x) * (l1_p1.y - l2_p1.y) - (l2_p2.y - l2_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;
                float u_b = ((l1_p2.x - l1_p1.x) * (l1_p1.y - l2_p1.y) - (l1_p2.y - l1_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;

                //Are the line segments intersecting if the end points are the same
                if (shouldIncludeEndPoints)
                {
                    //Is intersecting if u_a and u_b are between 0 and 1 or exactly 0 or 1
                    if (u_a >= 0f + epsilon && u_a <= 1f - epsilon && u_b >= 0f + epsilon && u_b <= 1f - epsilon)
                    {
                        isIntersecting = true;
                    }
                }
                else
                {
                    //Is intersecting if u_a and u_b are between 0 and 1
                    if (u_a > 0f + epsilon && u_a < 1f - epsilon && u_b > 0f + epsilon && u_b < 1f - epsilon)
                    {
                        isIntersecting = true;
                    }
                }

            }

            return isIntersecting;
        }



        //Whats the coordinate of an intersection point between two lines in 2d space if we know they are intersecting
        //http://thirdpartyninjas.com/blog/2008/10/07/line-segment-intersection/
        public static Vector2 GetLineLineIntersectionPoint(Vector2 l1_p1, Vector2 l1_p2, Vector2 l2_p1, Vector2 l2_p2)
        {
            float denominator = (l2_p2.y - l2_p1.y) * (l1_p2.x - l1_p1.x) - (l2_p2.x - l2_p1.x) * (l1_p2.y - l1_p1.y);

            float u_a = ((l2_p2.x - l2_p1.x) * (l1_p1.y - l2_p1.y) - (l2_p2.y - l2_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;

            Vector2 intersectionPoint = l1_p1 + u_a * (l1_p2 - l1_p1);

            return intersectionPoint;
        }



        //
        // Ray-plane intersection in 3d space, but can be used in 2d space as well if one of the coordinates is the same
        //
        //Assume the vectors are normalized, and that the plane normal is looking towards the point
        //http://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-plane-and-ray-disk-intersection
        public static bool AreRayPlaneIntersecting(Vector3 planePos, Vector3 planeNormal, Vector3 rayStart, Vector3 rayDir)
        {
            //To avoid floating point precision issues we can add a small value
            float epsilon = MathUtility.EPSILON;

            bool areIntersecting = false;

            float denominator = Vector3.Dot(-planeNormal, rayDir);

            //Debug.Log(denominator);

            if (denominator > epsilon)
            {
                Vector3 vecBetween = planePos - rayStart;

                float t = Vector3.Dot(vecBetween, -planeNormal) / denominator;

                //Debug.Log(t);

                if (t >= 0f)
                {
                    areIntersecting = true;
                }
            }

            return areIntersecting;
        }



        //Get the coordinate if we know a ray-plane is intersecting
        public static Vector3 GetRayPlaneIntersectionCoordinate(Vector3 planePos, Vector3 planeNormal, Vector3 rayStart, Vector3 rayDir)
        {
            float denominator = Vector3.Dot(-planeNormal, rayDir);

            Vector3 vecBetween = planePos - rayStart;

            float t = Vector3.Dot(vecBetween, -planeNormal) / denominator;

            Vector3 intersectionPoint = rayStart + rayDir * t;

            return intersectionPoint;
        }

        //2d space
        public static Vector2 GetRayPlaneIntersectionCoordinate(Vector2 planePos, Vector2 planeNormal, Vector2 rayStart, Vector2 rayDir)
        {
            float denominator = Vector2.Dot(-planeNormal, rayDir);

            Vector2 vecBetween = planePos - rayStart;

            float t = Vector2.Dot(vecBetween, -planeNormal) / denominator;

            Vector2 intersectionPoint = rayStart + rayDir * t;

            return intersectionPoint;
        }



        //
        // Line-plane intersection
        //
        //Is a line intersecting with a plane?
        public static bool AreLinePlaneIntersecting(Vector3 planeNormal, Vector3 planePos, Vector3 linePos1, Vector3 linePos2)
        {
            //To avoid floating point precision issues we can add a small value
            float epsilon = MathUtility.EPSILON;

            bool areIntersecting = false;

            Vector3 lineDir = (linePos1 - linePos2).normalized;

            float denominator = Vector3.Dot(-planeNormal, lineDir);

            //Debug.Log(denominator);

            //No intersection if the line and plane are parallell
            if (denominator > epsilon || denominator < -epsilon)
            {
                Vector3 vecBetween = planePos - linePos1;

                float t = Vector3.Dot(vecBetween, -planeNormal) / denominator;

                Vector3 intersectionPoint = linePos1 + lineDir * t;

                //Gizmos.DrawWireSphere(intersectionPoint, 0.5f);

                if (Geometry.IsPointBetweenPoints(linePos1, linePos2, intersectionPoint))
                {
                    areIntersecting = true;
                }
            }

            return areIntersecting;
        }



        //We know a line plane is intersecting and now we want the coordinate of intersection
        public static Vector3 GetLinePlaneIntersectionCoordinate(Vector3 planeNormal, Vector3 planePos, Vector3 linePos1, Vector3 linePos2)
        {
            Vector3 vecBetween = planePos - linePos1;

            Vector3 lineDir = (linePos1 - linePos2).normalized;

            float denominator = Vector3.Dot(-planeNormal, lineDir);

            float t = Vector3.Dot(vecBetween, -planeNormal) / denominator;

            Vector3 intersectionPoint = linePos1 + lineDir * t;

            return intersectionPoint;
        }



        //
        // Is a point inside a triangle?
        //
        //From http://totologic.blogspot.se/2014/01/accurate-point-in-triangle-test.html
        //p is the testpoint, and the other points are corners in the triangle
        //-1 if outside, 0 if on the border, 1 if inside the triangle
        public static bool IsPointInTriangle(Triangle2D t, Vector2 p, bool includeBorder)
        {
            //To avoid floating point precision issues we can add a small value
            float epsilon = MathUtility.EPSILON;

            float zero = 0f - epsilon;
            float one = 1f + epsilon;

            //Based on Barycentric coordinates
            float denominator = ((t.p2.y - t.p3.y) * (t.p1.x - t.p3.x) + (t.p3.x - t.p2.x) * (t.p1.y - t.p3.y));

            float a = ((t.p2.y - t.p3.y) * (p.x - t.p3.x) + (t.p3.x - t.p2.x) * (p.y - t.p3.y)) / denominator;
            float b = ((t.p3.y - t.p1.y) * (p.x - t.p3.x) + (t.p1.x - t.p3.x) * (p.y - t.p3.y)) / denominator;
            float c = 1 - a - b;

            bool isWithinTriangle = false;

            if (includeBorder)
            {
                //The point is within the triangle or on the border
                if (a >= zero && a <= one && b >= zero && b <= one && c >= zero && c <= one)
                {
                    isWithinTriangle = true;
                }
            }
            else
            {
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
        public static bool IsTriangleInsideTriangle(Triangle2D t1, Triangle2D t2)
        {
            bool isWithin = false;

            if (
                IsPointInTriangle(t2, t1.p1, false) &&
                IsPointInTriangle(t2, t1.p2, false) &&
                IsPointInTriangle(t2, t1.p3, false))
            {
                isWithin = true;
            }

            return isWithin;
        }



        //
        // Are two Axis-aligned-bounding-box intersecting?
        //
        //r1_minX - the smallest x-coordinate of all corners belonging to rectangle 1
        public static bool AreAABBIntersecting(
            float r1_minX, float r1_maxX, float r1_minY, float r1_maxY,
            float r2_minX, float r2_maxX, float r2_minY, float r2_maxY)
        {
            //If the min of one box in one dimension is greater than the max of another box then the boxes are not intersecting
            //They have to intersect in 2 dimensions. We have to test if box 1 is to the left or box 2 and vice versa
            bool isIntersecting = true;

            //X axis
            if (r1_minX > r2_maxX)
            {
                isIntersecting = false;
            }
            else if (r2_minX > r1_maxX)
            {
                isIntersecting = false;
            }
            //Y axis
            else if (r1_minY > r2_maxY)
            {
                isIntersecting = false;
            }
            else if (r2_minY > r1_maxY)
            {
                isIntersecting = false;
            }

            return isIntersecting;
        }



        //
        // Is a point inside a polygon?
        //
        //The list describing the polygon has to be sorted either clockwise or counter-clockwise because we have to identify its edges
        public static bool IsPointInPolygon(List<Vector2> polygonPoints, Vector2 point)
        {
            //Step 1. Find a point outside of the polygon
            //Pick a point with a x position larger than the polygons max x position, which is always outside
            Vector2 maxXPosVertex = polygonPoints[0];

            for (int i = 1; i < polygonPoints.Count; i++)
            {
                if (polygonPoints[i].x > maxXPosVertex.x)
                {
                    maxXPosVertex = polygonPoints[i];
                }
            }

            //The point should be outside so just pick a number to move it outside
            Vector2 pointOutside = maxXPosVertex + new Vector2(10f, 0f);


            //Step 2. Create an edge between the point we want to test with the point thats outside
            Vector2 l1_p1 = point;
            Vector2 l1_p2 = pointOutside;


            //Step 3. Find out how many edges of the polygon this edge is intersecting
            int numberOfIntersections = 0;

            for (int i = 0; i < polygonPoints.Count; i++)
            {
                //Line 2
                Vector2 l2_p1 = polygonPoints[i];

                int iPlusOne = MathUtility.ClampListIndex(i + 1, polygonPoints.Count);

                Vector2 l2_p2 = polygonPoints[iPlusOne];

                //Are the lines intersecting?
                if (AreLinesIntersecting(l1_p1, l1_p2, l2_p1, l2_p2, true))
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
        // Are two triangles intersecting in 2d space
        //
        public static bool AreTrianglesIntersecting2D(Triangle2D t1, Triangle2D t2, bool do_AABB_test)
        {
            bool isIntersecting = false;

            //Step 0. AABB intersection which may speed up the algorithm if the triangles are far apart
            if (do_AABB_test)
            {
                //Rectangle that covers t1 
                float r1_minX = Mathf.Min(t1.p1.x, Mathf.Min(t1.p2.x, t1.p3.x));
                float r1_maxX = Mathf.Max(t1.p1.x, Mathf.Max(t1.p2.x, t1.p3.x));

                float r1_minY = Mathf.Min(t1.p1.y, Mathf.Min(t1.p2.y, t1.p3.y));
                float r1_maxY = Mathf.Max(t1.p1.y, Mathf.Max(t1.p2.y, t1.p3.y));

                //Rectangle that covers t2
                float r2_minX = Mathf.Min(t2.p1.x, Mathf.Min(t2.p2.x, t2.p3.x));
                float r2_maxX = Mathf.Max(t2.p1.x, Mathf.Max(t2.p2.x, t2.p3.x));

                float r2_minY = Mathf.Min(t2.p1.y, Mathf.Min(t2.p2.y, t2.p3.y));
                float r2_maxY = Mathf.Max(t2.p1.y, Mathf.Max(t2.p2.y, t2.p3.y));

                if (!AreAABBIntersecting(r1_minX, r1_maxX, r1_minY, r1_maxY, r2_minX, r2_maxX, r2_minY, r2_maxY))
                {
                    return false;
                }
            }


            //Step 1. Line-line instersection

            //Line 1 of t1 against all lines of t2
            if (
                AreLinesIntersecting(t1.p1, t1.p2, t2.p1, t2.p2, true) ||
                AreLinesIntersecting(t1.p1, t1.p2, t2.p2, t2.p3, true) ||
                AreLinesIntersecting(t1.p1, t1.p2, t2.p3, t2.p1, true)
            )
            {
                isIntersecting = true;
            }
            //Line 2 of t1 against all lines of t2
            else if (
                AreLinesIntersecting(t1.p2, t1.p3, t2.p1, t2.p2, true) ||
                AreLinesIntersecting(t1.p2, t1.p3, t2.p2, t2.p3, true) ||
                AreLinesIntersecting(t1.p2, t1.p3, t2.p3, t2.p1, true)
            )
            {
                isIntersecting = true;
            }
            //Line 3 of t1 against all lines of t2
            else if (
                AreLinesIntersecting(t1.p3, t1.p1, t2.p1, t2.p2, true) ||
                AreLinesIntersecting(t1.p3, t1.p1, t2.p2, t2.p3, true) ||
                AreLinesIntersecting(t1.p3, t1.p1, t2.p3, t2.p1, true)
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
            if (IsPointInTriangle(t2, t1.p1, true) || IsPointInTriangle(t1, t2.p1, true))
            {
                isIntersecting = true;
            }


            return isIntersecting;
        }
    }
}
