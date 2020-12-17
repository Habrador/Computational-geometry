using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

public class OtherController : MonoBehaviour 
{
    public Transform pointATrans;
    public Transform pointBTrans;
    public Transform pointCTrans;
    public Transform pointDTrans;
	
	
	
	void OnDrawGizmos() 
	{
        MyVector2 a = pointATrans.position.ToMyVector2();
        MyVector2 b = pointBTrans.position.ToMyVector2();
        MyVector2 c = pointCTrans.position.ToMyVector2();
        MyVector2 d = pointDTrans.position.ToMyVector2();

        MyVector3 a_3d = pointATrans.position.ToMyVector3();
        MyVector3 b_3d = pointBTrans.position.ToMyVector3();
        MyVector3 c_3d = pointCTrans.position.ToMyVector3();
        MyVector3 d_3d = pointDTrans.position.ToMyVector3();

        //PointInRelationToVector(a, b, c);

        //IsTriangleOrientedClockwise(a, b, c);

        //IsQuadrilateralConvex(a, b, c, d);

        //PointInRelationToPlane(a, b, c);

        //IsPointBetweenPoints(a, b, c);

        //ClosestPointOnLineSegment(a, b, c);

        //PassedWaypoint(a, b, c);

        //CenterOfCircle(a, b, c);

        //AngleBetweenVectors(a, b, c);

        AngleBetweenVectors3D(a_3d, b_3d, c_3d);

        Gizmos.DrawWireSphere(pointATrans.position, 0.1f);
        Gizmos.DrawWireSphere(pointBTrans.position, 0.1f);
        Gizmos.DrawWireSphere(pointCTrans.position, 0.1f);
        //Gizmos.DrawWireSphere((pointATrans.position + pointBTrans.position) * 0.5f, 0.1f);
    }



    //Calculate the angle between Vector a and b both originating from c
    private void AngleBetweenVectors(MyVector2 a, MyVector2 b, MyVector2 c)
    {
        Gizmos.DrawLine(pointCTrans.position, pointATrans.position);
        Gizmos.DrawLine(pointCTrans.position, pointBTrans.position);

        MyVector2 from = a - c;
        MyVector2 to = b - c;

        float angle = MathUtility.AngleFromToCCW(from, to);

        Debug.Log(angle * Mathf.Rad2Deg);
    }



    //Calculate the angle between Vector a and b both originating from c
    private void AngleBetweenVectors3D(MyVector3 a, MyVector3 b, MyVector3 c)
    {
        Gizmos.DrawLine(pointCTrans.position, pointATrans.position);
        Gizmos.DrawLine(pointCTrans.position, pointBTrans.position);

        MyVector3 from = a - c;
        MyVector3 to = b - c;

        float angle = MathUtility.AngleFromToCCW(from, to, Vector3.forward.ToMyVector3());

        Debug.Log(angle * Mathf.Rad2Deg);
    }



    private void CenterOfCircle(MyVector2 a, MyVector2 b, MyVector2 c)
    {
        MyVector2 center = _Geometry.CalculateCircleCenter(a, b, c);
        //MyVector2 center = _Geometry.CalculateCircleCenter_Alternative2(a, b, c);

        //Debug.Log(center.x + " " + center.y);

        float radius = Vector3.Magnitude(a.ToVector3() - center.ToVector3());

        Gizmos.DrawWireSphere(center.ToVector3(), radius);
    }



    private void PassedWaypoint(MyVector2 wp1, MyVector2 wp2, MyVector2 testPoint)
    {
        bool hasPassed = _Geometry.HasPassedWaypoint(wp1, wp2, testPoint);

        Debug.Log(hasPassed);


        //Diplay
        TestAlgorithmsHelpMethods.DisplayArrow(wp1.ToVector3(), wp2.ToVector3(), 0.5f, Color.white);

        Gizmos.color = hasPassed ? Color.red : Color.black;

        Gizmos.DrawWireSphere(testPoint.ToVector3(), 0.5f);
    }



    private void ClosestPointOnLineSegment(MyVector2 a, MyVector2 b, MyVector2 testPoint)
    {
        MyVector2 closestPoint = _Geometry.GetClosestPointOnLine(new Edge2(a, b), testPoint, withinSegment: true);


        //Diplay
        Gizmos.color = Color.white;

        Gizmos.DrawLine(a.ToVector3(), b.ToVector3());

        Gizmos.DrawWireSphere(testPoint.ToVector3(), 0.1f);

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(closestPoint.ToVector3(), 0.1f);
    }



    private void IsPointBetweenPoints(MyVector2 a, MyVector2 b, MyVector2 testPoint)
    {
        bool isBetween = _Geometry.IsPointBetweenPoints(a, b, testPoint);

        Debug.Log("Is between: " + isBetween);


        //Diplay
        Gizmos.color = Color.white;

        Gizmos.DrawLine(a.ToVector3(), b.ToVector3());

        Gizmos.DrawWireSphere(testPoint.ToVector3(), 0.1f);
    }



    private void PointInRelationToPlane(MyVector2 a, MyVector2 b, MyVector2 testPoint)
    {
        MyVector2 planeDir = b - a;

        MyVector2 planeNormal = MyVector2.Normalize(new MyVector2(planeDir.y, -planeDir.x));

        MyVector2 planePos = a + planeDir * 0.5f;

        Plane2 plane = new Plane2(planePos, planeNormal);

        //Positive if infront, negative if behind
        float distanceToPlane = _Geometry.GetSignedDistanceFromPointToPlane(testPoint, plane);

        Debug.Log("Distance: " + distanceToPlane);

        
        //Display
        
        //Plane
        Gizmos.color = Color.blue;

        Gizmos.DrawLine(a.ToVector3(), b.ToVector3());
        //Plane normal
        Gizmos.DrawLine(planePos.ToVector3(), planePos.ToVector3() + planeNormal.ToVector3() * 0.1f);

        //Point
        Gizmos.color = Color.white;

        Gizmos.DrawWireSphere(testPoint.ToVector3(), 0.1f);
    }



    //Is a quadtrilateral convex
    private void IsQuadrilateralConvex(MyVector2 a, MyVector2 b, MyVector2 c, MyVector2 d)
    {
        bool isConvex = _Geometry.IsQuadrilateralConvex(a, b, c, d);

        Debug.Log("Is convex " + isConvex);

        //Display the quadrilateral
        Gizmos.color = Color.white;

        Gizmos.DrawLine(a.ToVector3(), b.ToVector3());
        Gizmos.DrawLine(b.ToVector3(), c.ToVector3());
        Gizmos.DrawLine(c.ToVector3(), d.ToVector3());
        Gizmos.DrawLine(d.ToVector3(), a.ToVector3());
    }



    //Is a triangle oriented clockwise
    private void IsTriangleOrientedClockwise(MyVector2 a, MyVector2 b, MyVector2 c)
    {
        Triangle2 t = new Triangle2(a, b, c);

        bool isOrientedClockwise = _Geometry.IsTriangleOrientedClockwise(t.p1, t.p2, t.p3);

        Debug.Log("Is oriented clockwise: " + isOrientedClockwise);

        //We can also test if changing orientation is working
        t.ChangeOrientation();

        bool isOrientedClockwiseAfterRotation = _Geometry.IsTriangleOrientedClockwise(t.p1, t.p2, t.p3);

        Debug.Log("Is oriented clockwise after changing orientation: " + isOrientedClockwiseAfterRotation);


        //Display the triangle
        Gizmos.color = Color.white;

        Gizmos.DrawLine(a.ToVector3(), b.ToVector3());
        Gizmos.DrawLine(b.ToVector3(), c.ToVector3());
        Gizmos.DrawLine(c.ToVector3(), a.ToVector3());

        //Arrows showing the direction of the triangle
        float arrowSize = 0.1f;

        TestAlgorithmsHelpMethods.DisplayArrow(a.ToVector3(), b.ToVector3(), arrowSize, Color.white);
        TestAlgorithmsHelpMethods.DisplayArrow(b.ToVector3(), c.ToVector3(), arrowSize, Color.white);
        TestAlgorithmsHelpMethods.DisplayArrow(c.ToVector3(), a.ToVector3(), arrowSize, Color.white);
    }



    //Is a point to the left or to the right of a vector going from a to b
    private void PointInRelationToVector(MyVector2 a, MyVector2 b, MyVector2 p)
    {
        //bool isToLeft = Geometry.IsPointLeftOfVector(a, b, p);

        //Debug.Log("Is to left: " + isToLeft);

        LeftOnRight value = _Geometry.IsPoint_Left_On_Right_OfVector(a, b, p);

        if (value == LeftOnRight.Left) { Debug.Log("Left"); }
        if (value == LeftOnRight.On) { Debug.Log("On"); }
        if (value == LeftOnRight.Right) { Debug.Log("Right"); }

        //Display
        Vector3 a_3d = a.ToVector3();
        Vector3 b_3d = b.ToVector3();

        Gizmos.DrawLine(a_3d, b_3d);

        float arrowSize = 0.1f;

        TestAlgorithmsHelpMethods.DisplayArrow(a_3d, b_3d, arrowSize, Color.white);

        Gizmos.DrawWireSphere(p.ToVector3(), 0.1f);
    }
}
