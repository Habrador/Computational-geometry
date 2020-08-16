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

    //public enum InfrontOnBack
    //{
    //    Infront, On, Back
    //}

    public static class _Geometry
    {
        //
        // Calculate the center of circle in 2d space given three coordinates (Complicated version)
        //
        //http://paulbourke.net/geometry/circlesphere/
        public static MyVector2 CalculateCircleCenter_Alternative2(MyVector2 a, MyVector2 b, MyVector2 c)
        {
            //Important note from the source: 
            //"If either line is vertical then the corresponding slope is infinite. This can be solved by simply rearranging the order of the points so that vertical lines do not occur."
            //We get a division by 0 if b.x = a.x and/or c.x = b.x when calculating ma and mb
            //Combinations:
            //abc
            //acb
            //bac
            //bca
            //cab
            //cba
            if (!IsPerpendicular(a, b, c))
            {
                return GetCircleCenter(a, b, c);
            }
            else if (!IsPerpendicular(a, c, b))
            {
                return GetCircleCenter(a, c, b);
            }
            else if (!IsPerpendicular(b, a, c))
            {
                return GetCircleCenter(b, a, c);
            }
            else if (!IsPerpendicular(b, c, a))
            {
                return GetCircleCenter(b, c, a);
            }
            else if (!IsPerpendicular(c, b, a))
            {
                return GetCircleCenter(c, b, a);
            }
            else if (!IsPerpendicular(c, a, b))
            {
                return GetCircleCenter(c, a, b);
            }
            else
            {
                Debug.LogWarning("Cant calculate circle center because all points are on same line");

                return new MyVector2(-100f, -100f);
            }
        }


        //Is connected with the above if we know a circle center can be calculated
        private static MyVector2 GetCircleCenter(MyVector2 a, MyVector2 b, MyVector2 c)
        {
            float yDelta_a = b.y - a.y;
            float xDelta_a = b.x - a.x;
            float yDelta_b = c.y - b.y;
            float xDelta_b = c.x - b.x;

            float tolerance = 0.00001f;

            //Check whether the lines are pependicular and parallel to x-y axis
            //This is a special case and we have to calculate the circle center in another way
            if (Mathf.Abs(xDelta_a) <= tolerance && Mathf.Abs(yDelta_b) <= tolerance)
            {
                //Debug.Log("The points are pependicular and parallel to x-y axis");
                
                float center_special_X = 0.5f * (b.x + c.x);
                float center_special_Y = 0.5f * (a.y + b.y);

                MyVector2 center_special = new MyVector2(center_special_X, center_special_Y);

                return center_special;
            }

            //This assumes that we have tested that b.x != a.x and c.x != b.x
            float ma = (b.y - a.y) / (b.x - a.x);
            float mb = (c.y - b.y) / (c.x - b.x);

            float centerX = (ma * mb * (a.y - c.y) + mb * (a.x + b.x) - ma * (b.x + c.x)) / (2 * (mb - ma));

            float centerY = (-1f / ma) * (centerX - (a.x + b.x) / 2f) + (a.y + b.y) / 2f;

            MyVector2 center = new MyVector2(centerX, centerY);

            return center;
        }


        //Is connected with the above to avoid division by 0
        private static bool IsPerpendicular(MyVector2 p1, MyVector2 p2, MyVector2 p3)
        {
            float yDelta_a = p2.y - p1.y;
            float xDelta_a = p2.x - p1.x;
            float yDelta_b = p3.y - p2.y;
            float xDelta_b = p3.x - p2.x;

            float tolerance = 0.00001f;

            //Check whether the line of the two points are vertical
            if (Mathf.Abs(xDelta_a) <= tolerance && Mathf.Abs(yDelta_b) <= tolerance)
            {
                //Debug.Log("The points are pependicular and parallel to x-y axis");
                return false;
            }
            
            if (Mathf.Abs(yDelta_a) <= tolerance)
            {
                //Debug.Log("A line of two point are perpendicular to x-axis 1");
                return true;
            }
            else if (Mathf.Abs(yDelta_b) <= tolerance)
            {
                //Debug.Log("A line of two point are perpendicular to x-axis 2");
                return true;
            }
            else if (Mathf.Abs(xDelta_a) <= tolerance)
            {
                //Debug.Log("A line of two point are perpendicular to y-axis 1");
                return true;
            }
            else if (Mathf.Abs(xDelta_b) <= tolerance)
            {
                //Debug.Log("A line of two point are perpendicular to y-axis 2");
                return true;
            }
            else
            {
                return false;
            }
        }



        //
        // Calculate the center of circle in 2d space given three coordinates - Simple version
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

            float one_divided_by_4_A = 1f / (4f * A);

            float x = a.x + one_divided_by_4_A * ((Y_2 * L_10_square) - (Y_1 * L_20_square));
            float y = a.y + one_divided_by_4_A * ((X_1 * L_20_square) - (X_2 * L_10_square));

            MyVector2 center = new MyVector2(x, y);

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
        // Is a point to the left, to the right, or on a plane
        //
        //https://gamedevelopment.tutsplus.com/tutorials/understanding-sutherland-hodgman-clipping-for-physics-engines--gamedev-11917
        //Notice that the plane normal doesnt have to be normalized
        //public static float DistanceFromPointToPlane(Vector3 planeNormal, Vector3 planePos, Vector3 pointPos)
        //{
        //    //Positive distance denotes that the point p is on the front side of the plane 
        //    //Negative means it's on the back side
        //    float distance = Vector3.Dot(planeNormal, pointPos - planePos);

        //    return distance;
        //}

        public static float DistanceFromPointToPlane(MyVector2 planeNormal, MyVector2 planePos, MyVector2 pointPos)
        {
            //Positive distance denotes that the point p is on the front side of the plane 
            //Negative means it's on the back side
            float distance = MyVector2.Dot(planeNormal, pointPos - planePos);

            return distance;
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
        // Find the closest point on a line segment from a point
        //
        //From https://www.youtube.com/watch?v=KHuI9bXZS74
        //Maybe better version https://stackoverflow.com/questions/3120357/get-closest-point-to-a-line
        public static MyVector2 GetClosestPointOnLineSegment(MyVector2 a, MyVector2 b, MyVector2 p)
        {
            MyVector2 a_p = p - a;
            MyVector2 a_b = b - a;

            //This is using vector projections???

            //Square magnitude of AB vector
            float sqrMagnitudeAB = MyVector2.SqrMagnitude(a_b);

            //The DOT product of a_p and a_b  
            float ABAPproduct = MyVector2.Dot(a_p, a_b);

            //The normalized "distance" from a to the closest point  
            float distance = ABAPproduct / sqrMagnitudeAB;

            //This point may not be on the line segment, if so return one of the end points
            //Check if P projection is over vectorAB     
            if (distance < 0)
            {
                return a;
            }
            else if (distance > 1)
            {
                return b;
            }
            else
            {
                return a + a_b * distance;
            }
        }



        //Calculate the angle between the vectors if we are going from p1-p2-p3
        //Return +180 if "small" or -180 if "large"
        //public static float CalculateAngleBetweenVectors(MyVector2 p1, MyVector2 p2, MyVector2 p3)
        //{
        //    MyVector2 from = p1 - p2;

        //    MyVector2 to = p3 - p2;

        //    float angle = Vector2.SignedAngle(from, to);

        //    return angle;
        //}



        //Create a supertriangle that contains all other points
        //According to the book "Geometric tools for computer graphics" a reasonably sized triangle
        //is one that contains a circle that contains the axis-aligned bounding rectangle of the points 
        //Is currently not used anywhere because our points are normalized to the range 0-1
        //and then we can make a supertriangle by just setting its size to 100
        public static Triangle2 GenerateSupertriangle(HashSet<MyVector2> points)
        {
            //Step 1. Create a AABB around the points
            AABB2 aabb = new AABB2(new List<MyVector2>(points));

            MyVector2 TL = new MyVector2(aabb.minX, aabb.maxY);
            MyVector2 TR = new MyVector2(aabb.maxX, aabb.maxY);
            MyVector2 BR = new MyVector2(aabb.maxX, aabb.minY);


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
    }
}
