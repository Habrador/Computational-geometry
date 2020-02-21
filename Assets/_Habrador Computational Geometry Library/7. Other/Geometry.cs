using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class Geometry
    {
        //Calculate the center of circle in 2d space given three coordinates
        //http://paulbourke.net/geometry/circlesphere/
        public static MyVector2 CalculateCircleCenter(MyVector2 p1, MyVector2 p2, MyVector2 p3)
        {
            float ma = (p2.y - p1.y) / (p2.x - p1.x);
            float mb = (p3.y - p2.y) / (p3.x - p2.x);

            float centerX = (ma * mb * (p1.y - p3.y) + mb * (p1.x + p2.x) - ma * (p2.x + p3.x)) / (2 * (mb - ma));

            float centerY = (-1 / ma) * (centerX - (p1.x + p2.x) / 2) + (p1.y + p2.y) / 2;

            MyVector2 center = new MyVector2(centerX, centerY);

            return center;
        }



        //Is a triangle in 2d space oriented clockwise or counter-clockwise
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



        ////Does a point p lie to the left or to the right of a vector going from a to b
        ////https://gamedev.stackexchange.com/questions/71328/how-can-i-add-and-subtract-convex-polygons
        //public static bool IsAPointLeftOfVector(Vector2 a, Vector2 b, Vector2 p)
        //{
        //    //To avoid floating point precision issues we can add a small value
        //    float epsilon = MathUtility.EPSILON;

        //    float determinant = (a.x - p.x) * (b.y - p.y) - (a.y - p.y) * (b.x - p.x);

        //    bool isToLeft = true;

        //    if (determinant < 0f - epsilon)
        //    {
        //        isToLeft = false;
        //    }

        //    return isToLeft;
        //}


        //Use this if we might en up on the line, which has a low probability in a game, but may happen in some cases
        //Where is c in relation to a-b
        //Returns -1 if to the left, 0 if on the border, 1 if to the right
        public static int GetPointPositionInRelationToLine(MyVector2 a, MyVector2 b, MyVector2 p)
        {
            //To avoid floating point precision issues we can add a small value
            float epsilon = MathUtility.EPSILON;

            float determinant = (a.x - p.x) * (b.y - p.y) - (a.y - p.y) * (b.x - p.x);

            // < 0 -> to the right
            if (determinant < 0f - epsilon)
            {
                return 1;
            }
            // > 0 -> to the left
            else if (determinant > 0f + epsilon)
            {
                return -1;
            }
            // = 0 -> on the line
            else
            {
                return 0;
            }
        }



        //Is a point to the left, to the right, or on a plane
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



        //Is a quadrilateral convex? Assume no 3 points are colinear and the shape doesnt look like an hourglass
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
            //Another maybe more understandable way is point in triangle?
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



        //Is a point c between point a and b (we assume all 3 are on the same line)
        public static bool IsPointBetweenPoints(MyVector2 a, MyVector2 b, MyVector2 c)
        {
            bool isBetween = false;

            //Entire line segment
            MyVector2 ab = b - a;
            //The intersection and the first point
            MyVector2 ac = c - a;

            //Need to check 2 things: 
            //1. If the vectors are pointing in the same direction = if the dot product is positive
            //2. If the length of the vector between the intersection and the first point is smaller than the entire line
            if (MyVector2.Dot(ab, ac) > 0f && MyVector2.SqrMagnitude(ab) >= MyVector2.SqrMagnitude(ac))
            {
                isBetween = true;
            }

            return isBetween;
        }



        //Find the closest point on a line segment from a point
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
    }
}
