using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Help enum in case we need to return something else than a bool
    public enum LeftOnRight
    {
        Left, On, Right
    }
    public enum OutsideOnInside
    {
        Outside, On, Inside
    }

    

    public static class _Geometry
    {
        //
        // Calculate the center of circle in 2d space given three coordinates
        //
        //From the book "Geometric Tools for Computer Graphics"
        public static MyVector2 CalculateCircleCenter(MyVector2 a, MyVector2 b, MyVector2 c)
        {
            //Make sure the triangle a-b-c is counterclockwise
            if (!IsTriangleOrientedClockwise(a, b, c))
            {
                //Swap two vertices to change orientation
                (a, b) = (b, a);

                //Debug.Log("Swapped vertices");
            }


            //The area of the triangle
            float X_1 = b.x - a.x;
            float X_2 = c.x - a.x;
            float Y_1 = b.y - a.y;
            float Y_2 = c.y - a.y;

            float A = 0.5f * MathUtility.Det2(X_1, Y_1, X_2, Y_2);

            //Debug.Log(A);


            //The center coordinates:
            //float L_10 = MyVector2.Magnitude(b - a);
            //float L_20 = MyVector2.Magnitude(c - a);

            //float L_10_square = L_10 * L_10;
            //float L_20_square = L_20 * L_20;

            float L_10_square = MyVector2.SqrMagnitude(b - a);
            float L_20_square = MyVector2.SqrMagnitude(c - a);

            float one_divided_by_4A = 1f / (4f * A);

            float x = a.x + one_divided_by_4A * ((Y_2 * L_10_square) - (Y_1 * L_20_square));
            float y = a.y + one_divided_by_4A * ((X_1 * L_20_square) - (X_2 * L_10_square));

            MyVector2 center = new MyVector2(x, y);

            return center;
        }



        //
        // Calculate the center of circle in 3d space given three coordinates
        //
        //From https://gamedev.stackexchange.com/questions/60630/how-do-i-find-the-circumcenter-of-a-triangle-in-3d
        public static MyVector3 CalculateCircleCenter(MyVector3 a, MyVector3 b, MyVector3 c)
        {
            MyVector3 ac = c - a;
            MyVector3 ab = b - a;
            MyVector3 abXac = MyVector3.Cross(ab, ac);

            //This is the vector from a to the circumsphere center
            MyVector3 toCircumsphereCenter = MyVector3.Cross(abXac, ab) * Mathf.Pow(MyVector3.Magnitude(ac), 2f);

            toCircumsphereCenter += MyVector3.Cross(ac, abXac) * Mathf.Pow(MyVector3.Magnitude(ab), 2f);

            toCircumsphereCenter *= (1f / (2f * Mathf.Pow(MyVector3.Magnitude(abXac), 2f)));
            
            float circumsphereRadius = MyVector3.Magnitude(toCircumsphereCenter);

            //The circumsphere center becomes
            MyVector3 ccs = a + toCircumsphereCenter;

            return ccs;
        }



        //
        // Calculate the center of a triangle in 3d space
        //
        public static MyVector3 CalculateTriangleCenter(MyVector3 p1, MyVector3 p2, MyVector3 p3)
        {
            MyVector3 center = (p1 + p2 + p3) * (1f / 3f);

            return center;
        }



        //
        // Is a triangle in 2d space oriented clockwise or counter-clockwise
        //
        //https://math.stackexchange.com/questions/1324179/how-to-tell-if-3-connected-points-are-connected-clockwise-or-counter-clockwise
        //https://en.wikipedia.org/wiki/Curve_orientation
        public static bool IsTriangleOrientedClockwise(MyVector2 p1, MyVector2 p2, MyVector2 p3)
        {
            bool isClockWise = true;

            float determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;

            if (determinant > 0f)
            {
                isClockWise = false;
            }

            return isClockWise;
        }



        //
        // Does a point p lie to the left, to the right, or on a vector going from a to b
        //
        //https://gamedev.stackexchange.com/questions/71328/how-can-i-add-and-subtract-convex-polygons
        public static float GetPointInRelationToVectorValue(MyVector2 a, MyVector2 b, MyVector2 p)
        {
            float x1 = a.x - p.x;
            float x2 = a.y - p.y;
            float y1 = b.x - p.x;
            float y2 = b.y - p.y;

            float determinant = MathUtility.Det2(x1, x2, y1, y2);

            return determinant;
        }

        public static bool IsPointLeftOfVector(MyVector2 a, MyVector2 b, MyVector2 p)
        {
            float relationValue = GetPointInRelationToVectorValue(a, b, p);

            bool isToLeft = true;

            //to avoid floating point precision issues we can add a small value
            float epsilon = MathUtility.EPSILON;

            if (relationValue < 0f - epsilon)
            {
                isToLeft = false;
            }

            return isToLeft;
        }

        //Same as above but we want to figure out if we are on the vector
        //Use this if we might en up on the line, which has a low probability in a game, but may happen in some cases
        //Where is c in relation to a-b
        public static LeftOnRight IsPoint_Left_On_Right_OfVector(MyVector2 a, MyVector2 b, MyVector2 p)
        {
            float relationValue = GetPointInRelationToVectorValue(a, b, p);

            //To avoid floating point precision issues we can add a small value
            float epsilon = MathUtility.EPSILON;

            //To the right
            if (relationValue < -epsilon)
            {
                return LeftOnRight.Right;
            }
            //To the left
            else if (relationValue > epsilon)
            {
                return LeftOnRight.Left;
            }
            //= 0 -> on the line
            else
            {
                return LeftOnRight.On;
            }
        }



        //
        // Point-plane relations
        //
        //https://gamedevelopment.tutsplus.com/tutorials/understanding-sutherland-hodgman-clipping-for-physics-engines--gamedev-11917
        //Notice that the plane normal doesnt have to be normalized

        //The signed distance from a point to a plane
        //- Positive distance denotes that the point p is outside the plane (in the direction of the plane normal)
        //- Negative means it's inside

        //3d
        public static float GetSignedDistanceFromPointToPlane(MyVector3 pointPos, Plane3 plane)
        {
            float distance = MyVector3.Dot(plane.normal, pointPos - plane.pos);

            return distance;
        }

        //2d
        public static float GetSignedDistanceFromPointToPlane(MyVector2 pointPos, Plane2 plane)
        {
            float distance = MyVector2.Dot(plane.normal, pointPos - plane.pos);

            return distance;
        }


        //Relations of a point to a plane

        //3d
        //Outside means in the planes normal direction
        public static bool IsPointOutsidePlane(MyVector3 pointPos, Plane3 plane) 
        {
            float distance = GetSignedDistanceFromPointToPlane(pointPos, plane);

            //To avoid floating point precision issues we can add a small value
            float epsilon = MathUtility.EPSILON;

            if (distance > 0f + epsilon)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //3d
        public static OutsideOnInside IsPoint_Outside_On_Inside_Plane(MyVector3 pointPos, Plane3 plane)
        {
            float distance = GetSignedDistanceFromPointToPlane(pointPos, plane);

            //To avoid floating point precision issues we can add a small value
            float epsilon = MathUtility.EPSILON;

            if (distance > 0f + epsilon)
            {
                return OutsideOnInside.Outside;
            }
            else if (distance < 0f - epsilon)
            {
                return OutsideOnInside.Inside;
            }
            else
            {
                return OutsideOnInside.On;
            }
        }



        //
        // Is a quadrilateral convex? Assume no 3 points are colinear and the shape doesnt look like an hourglass
        //
        //A quadrilateral is a polygon with four edges (or sides) and four vertices or corners
        public static bool IsQuadrilateralConvex(MyVector2 a, MyVector2 b, MyVector2 c, MyVector2 d)
        {
            bool isConvex = false;

            //Convex if the convex hull includes all 4 points - will require just 4 determinant operations
            //In this case we dont kneed to know the order of the points, which is better
            //We could split it up into triangles, but still messy because of interior/exterior angles
            //Another version is if we know the edge between the triangles that form a quadrilateral
            //then we could measure the 4 angles of the edge, add them together (2 and 2) to get the interior angle
            //But it will still require 8 magnitude operations which is slow
            //From: https://stackoverflow.com/questions/2122305/convex-hull-of-4-points
            bool abc = _Geometry.IsTriangleOrientedClockwise(a, b, c);
            bool abd = _Geometry.IsTriangleOrientedClockwise(a, b, d);
            bool bcd = _Geometry.IsTriangleOrientedClockwise(b, c, d);
            bool cad = _Geometry.IsTriangleOrientedClockwise(c, a, d);

            if (abc && abd && bcd & !cad)
            {
                isConvex = true;
            }
            else if (abc && abd && !bcd & cad)
            {
                isConvex = true;
            }
            else if (abc && !abd && bcd & cad)
            {
                isConvex = true;
            }
            //The opposite sign, which makes everything inverted
            else if (!abc && !abd && !bcd & cad)
            {
                isConvex = true;
            }
            else if (!abc && !abd && bcd & !cad)
            {
                isConvex = true;
            }
            else if (!abc && abd && !bcd & !cad)
            {
                isConvex = true;
            }


            return isConvex;
        }



        //
        // Is a point p between point a and b (we assume all 3 are on the same line)
        //
        public static bool IsPointBetweenPoints(MyVector2 a, MyVector2 b, MyVector2 p)
        {
            bool isBetween = false;

            //Entire line segment
            MyVector2 ab = b - a;
            //The intersection and the first point
            MyVector2 ap = p - a;

            //Need to check 2 things: 
            //1. If the vectors are pointing in the same direction = if the dot product is positive
            //2. If the length of the vector between the intersection and the first point is smaller than the entire line
            if (MyVector2.Dot(ab, ap) > 0f && MyVector2.SqrMagnitude(ab) >= MyVector2.SqrMagnitude(ap))
            {
                isBetween = true;
            }

            return isBetween;
        }



        //
        // Line-point calculations
        //
        //From https://stackoverflow.com/questions/3120357/get-closest-point-to-a-line
        //and https://www.youtube.com/watch?v=_ENEsV_kNx8
        public static MyVector2 GetClosestPointOnLine(Edge2 e, MyVector2 p, bool withinSegment)
        {
            MyVector2 a = e.p1;
            MyVector2 b = e.p2;

            //Assume the line goes from a to b
            MyVector2 ab = b - a;
            //Vector from "start" of the line to the point outside of line
            MyVector2 ap = p - a;

            //Scalar projection https://en.wikipedia.org/wiki/Scalar_projection
            //The scalar projection is a scalar, equal to the length of the orthogonal projection of ap on ab, with a negative sign if the projection has an opposite direction with respect to ab.
            //scalarProjection = Dot(ap, ab) / Magnitude(ab) where the magnitude of ab is the distance between a and b
            //If ab is normalized, we get scalarProjection = Dot(ap, ab)

            //The distance from a to q (the closes point on the line):
            //float aq_distance = MyVector2.Dot(ap, ab) / MyVector2.Magnitude(ab);

            //To get the closest point on the line:
            //MyVector2 q = a + MyVector2.Normalize(ab) * aq_distance;


            //Can we do better?
            //Magnitude is defined as: Mathf.Sqrt((ab * ab))
            //Normalization is defined as (ab / magnitude(ab))
            //We get: q = a + (ab / magnitude(ab)) * (1 / magnitude(ab)) * dot(ap, ab)
            //Ignore the q and the dot and we get: (ab / Mathf.Sqrt((ab * ab))) * (1 / Mathf.Sqrt((ab * ab))) = ab / (ab * ab)
            //So we can use the square magnitude of ab and then we don't need to normalize ab (to get q), so we save two square roots, which is good because square root is a slow operation

            //The normalized "distance" from a to the closest point, so between 0 and 1 if we are within the line segment
            float distance = MyVector2.Dot(ap, ab) / MyVector2.SqrMagnitude(ab);

            //This point may not be on the line segment, if so return one of the end points
            float epsilon = MathUtility.EPSILON;
            
            if (withinSegment && distance < 0f - epsilon)
            {
                return a;
            }
            else if (withinSegment && distance > 1f + epsilon)
            {
                return b;
            }
            else
            {
                //This works because a_b is not normalized and distance is [0,1] if distance is within ab
                return a + ab * distance;
            }
        }

        //3d
        //Same math as in 2d case
        public static MyVector3 GetClosestPointOnLine(Edge3 e, MyVector3 p, bool withinSegment)
        {
            MyVector3 a = e.p1;
            MyVector3 b = e.p2;

            //Assume the line goes from a to b
            MyVector3 ab = b - a;
            //Vector from start of the line to the point outside of line
            MyVector3 ap = p - a;

            //The normalized "distance" from a to the closest point, so [0,1] if we are within the line segment
            float distance = MyVector3.Dot(ap, ab) / MyVector3.SqrMagnitude(ab);


            ///This point may not be on the line segment, if so return one of the end points
            float epsilon = MathUtility.EPSILON;

            if (withinSegment && distance < 0f - epsilon)
            {
                return a;
            }
            else if (withinSegment && distance > 1f + epsilon)
            {
                return b;
            }
            else
            {
                //This works because a_b is not normalized and distance is [0,1] if distance is within ab
                return a + ab * distance;
            }
        }



        //Create a supertriangle that contains all other points
        //According to the book "Geometric tools for computer graphics" a reasonably sized triangle
        //is one that contains a circle that contains the axis-aligned bounding rectangle of the points 
        public static Triangle2 GenerateSupertriangle(HashSet<MyVector2> points)
        {
            //Step 1. Create a AABB around the points
            AABB2 aabb = new AABB2(new List<MyVector2>(points));

            MyVector2 TL = new MyVector2(aabb.min.x, aabb.max.y);
            MyVector2 TR = new MyVector2(aabb.max.x, aabb.max.y);
            MyVector2 BR = new MyVector2(aabb.max.x, aabb.min.y);


            //Step2. Find the inscribed circle - the smallest circle that surrounds the AABB
            MyVector2 circleCenter = (TL + BR) * 0.5f;

            float circleRadius = MyVector2.Magnitude(circleCenter - TR);


            //Step 3. Create the smallest triangle that surrounds the circle
            //All edges of this triangle have the same length
            float halfSideLenghth = circleRadius / Mathf.Tan(30f * Mathf.Deg2Rad);

            //The center position of the bottom-edge
            MyVector2 t_B = new MyVector2(circleCenter.x, circleCenter.y - circleRadius);

            MyVector2 t_BL = new MyVector2(t_B.x - halfSideLenghth, t_B.y);
            MyVector2 t_BR = new MyVector2(t_B.x + halfSideLenghth, t_B.y);

            //The height from the bottom edge to the top vertex
            float triangleHeight = halfSideLenghth * Mathf.Tan(60f * Mathf.Deg2Rad);

            MyVector2 t_T = new MyVector2(circleCenter.x, t_B.y + triangleHeight);

            
            //The final triangle
            Triangle2 superTriangle = new Triangle2(t_BR, t_BL, t_T);

            return superTriangle;
        }



        //
        // If p is going from p1 to p2, has it passed p2?
        //
        //This is very useful if we are moving between waypoints and want to know if we have passed
        //waypoint b
        public static bool HasPassedWaypoint(MyVector2 wp1, MyVector2 wp2, MyVector2 p)
        {
            //The vector between the character and the waypoint we are going from
            MyVector2 a = p - wp1;

            //The vector between the waypoints
            MyVector2 b = wp2 - wp1;

            //Vector projection from https://en.wikipedia.org/wiki/Vector_projection
            //To know if we have passed the upcoming waypoint we need to find out how much of b is a1
            //a1 = (a.b / |b|^2) * b
            //a1 = progress * b -> progress = a1 / b -> progress = (a.b / |b|^2)
            float progress = (a.x * b.x + a.y * b.y) / (b.x * b.x + b.y * b.y);

            //If progress is above 1 we know we have passed the waypoint
            if (progress > 1.0f + MathUtility.EPSILON)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        //
        // Calculate the normal of a clock-wise oriented triangle in 3d space
        //
        public static MyVector3 CalculateTriangleNormal(MyVector3 p1, MyVector3 p2, MyVector3 p3, bool shouldNormalize = true)
        {
            MyVector3 normal = MyVector3.Cross(p3 - p2, p1 - p2);

            if (shouldNormalize)
            {
                normal = MyVector3.Normalize(normal);
            }

            return normal;
        }



        //
        // Calculate the area of a triangle in 3d space
        //
        //https://math.stackexchange.com/questions/128991/how-to-calculate-the-area-of-a-3d-triangle
        public static float CalculateTriangleArea(MyVector3 p1, MyVector3 p2, MyVector3 p3)
        {
            MyVector3 normal = CalculateTriangleNormal(p1, p2, p3, shouldNormalize: false);

            float parallelogramArea = MyVector3.Magnitude(normal);

            float triangleArea = parallelogramArea * 0.5f;

            return triangleArea;
        }
    }
}
