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

    public static class Geometry
    {
        //
        // Calculate the center of circle in 2d space given three coordinates
        //
        //http://paulbourke.net/geometry/circlesphere/
        public static MyVector2 CalculateCircleCenter(MyVector2 a, MyVector2 b, MyVector2 c)
        {
            float ma = (b.y - a.y) / (b.x - a.x);
            float mb = (c.y - b.y) / (c.x - b.x);

            float centerX = (ma * mb * (a.y - c.y) + mb * (a.x + b.x) - ma * (b.x + c.x)) / (2 * (mb - ma));

            float centerY = (-1f / ma) * (centerX - (a.x + b.x) / 2f) + (a.y + b.y) / 2f;

            MyVector2 center = new MyVector2(centerX, centerY);

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
        private static float GetPointInRelationToVectorValue(MyVector2 a, MyVector2 b, MyVector2 p)
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
        //Returns -1 if to the left, 0 if on the border, 1 if to the right
        public static LeftOnRight IsPoint_Left_On_Right_OfVector(MyVector2 a, MyVector2 b, MyVector2 p)
        {
            float relationValue = GetPointInRelationToVectorValue(a, b, p);

            //To avoid floating point precision issues we can add a small value
            float epsilon = MathUtility.EPSILON;

            //To the right
            if (relationValue < 0f - epsilon)
            {
                return LeftOnRight.Right;
            }
            //To the left
            else if (relationValue > 0f + epsilon)
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
            bool abc = Geometry.IsTriangleOrientedClockwise(a, b, c);
            bool abd = Geometry.IsTriangleOrientedClockwise(a, b, d);
            bool bcd = Geometry.IsTriangleOrientedClockwise(b, c, d);
            bool cad = Geometry.IsTriangleOrientedClockwise(c, a, d);

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
            AABB aabb = HelpMethods.GetAABB(new List<MyVector2>(points));

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
    }
}
