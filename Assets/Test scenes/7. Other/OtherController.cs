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

        //PointInRelationToVector(a, b, c);

        //IsTriangleOrientedClockwise(a, b, c);

        //IsQuadrilateralConvex(a, b, c, d);

        //PointInRelationToPlane(a, b, c);

        //IsPointBetweenPoints(a, b, c);

        ClosestPointOnLineSegment(a, b, c);
    }



    private void ClosestPointOnLineSegment(MyVector2 a, MyVector2 b, MyVector2 testPoint)
    {
        MyVector2 closestPoint = Geometry.GetClosestPointOnLineSegment(a, b, testPoint);

        //Diplay
        Gizmos.color = Color.white;

        Gizmos.DrawLine(a.ToVector3(), b.ToVector3());

        Gizmos.DrawWireSphere(testPoint.ToVector3(), 0.1f);

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(closestPoint.ToVector3(), 0.1f);
    }



    private void IsPointBetweenPoints(MyVector2 a, MyVector2 b, MyVector2 testPoint)
    {
        bool isBetween = Geometry.IsPointBetweenPoints(a, b, testPoint);

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

        //Positive if infront, negative if behind
        float distanceToPlane = Geometry.DistanceFromPointToPlane(planeNormal, planePos, testPoint);

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
        bool isConvex = Geometry.IsQuadrilateralConvex(a, b, c, d);

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
        bool isOrientedClockwise = Geometry.IsTriangleOrientedClockwise(a, b, c);

        Debug.Log("Is oriented clockwise: " + isOrientedClockwise);


        //Display the triangle
        Gizmos.color = Color.white;

        Gizmos.DrawLine(a.ToVector3(), b.ToVector3());
        Gizmos.DrawLine(b.ToVector3(), c.ToVector3());
        Gizmos.DrawLine(c.ToVector3(), a.ToVector3());

        //Arrows showing the direction of the triangle
        float arrowSize = 0.1f;

        DebugResultsHelper.DrawArrow(a.ToVector3(), b.ToVector3(), arrowSize, Color.white);
        DebugResultsHelper.DrawArrow(b.ToVector3(), c.ToVector3(), arrowSize, Color.white);
        DebugResultsHelper.DrawArrow(c.ToVector3(), a.ToVector3(), arrowSize, Color.white);
    }



    //Is a point to the left or to the right of a vector going from a to b
    private void PointInRelationToVector(MyVector2 a, MyVector2 b, MyVector2 p)
    {
        //bool isToLeft = Geometry.IsPointLeftOfVector(a, b, p);

        //Debug.Log("Is to left: " + isToLeft);

        LeftOnRight value = Geometry.IsPoint_Left_On_Right_OfVector(a, b, p);

        if (value == LeftOnRight.Left) { Debug.Log("Left"); }
        if (value == LeftOnRight.On) { Debug.Log("On"); }
        if (value == LeftOnRight.Right) { Debug.Log("Right"); }

        //Display
        Vector3 a_3d = a.ToVector3();
        Vector3 b_3d = b.ToVector3();

        Gizmos.DrawLine(a_3d, b_3d);

        float arrowSize = 0.1f;

        DebugResultsHelper.DrawArrow(a_3d, b_3d, arrowSize, Color.white);

        Gizmos.DrawWireSphere(p.ToVector3(), 0.1f);
    }
}
