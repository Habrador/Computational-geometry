using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

public class UtilityController : MonoBehaviour 
{
    public Transform pointATrans;
    public Transform pointBTrans;
    public Transform pointCTrans;
    public Transform pointDTrans;


    void Start() 
	{
	    
	}
	
	
	
	void OnDrawGizmos() 
	{
        Vector2 a = new Vector2(pointATrans.position.x, pointATrans.position.z);
        Vector2 b = new Vector2(pointBTrans.position.x, pointBTrans.position.z);
        Vector2 c = new Vector2(pointCTrans.position.x, pointCTrans.position.z);
        Vector2 d = new Vector2(pointDTrans.position.x, pointDTrans.position.z);


        //Is a point to the left or to the right of a vector
        //bool isToLeft = Orientations.IsAPointLeftOfVector(a, b, c);


        //Is a point in a circle determines by three other points
        //float isPointInCircle = Orientations.IsPointInsideOutsideOrOnCircle(a, b, c, d);

        //print(isPointInCircle);

        //Vector2 centerOfCicle = Geometry.CarculateCircleCenter(a, b, c);

        //float radius = Vector2.Distance(centerOfCicle, a);

        //Gizmos.DrawWireSphere(new Vector3(centerOfCicle.x, 0f, centerOfCicle.y), radius);


        //Line-line intersection
        //Gizmos.DrawLine(pointATrans.position, pointBTrans.position);
        //Gizmos.DrawLine(pointCTrans.position, pointDTrans.position);

        //if (Intersections.AreLinesIntersecting(a, b, c, d, false))
        //{
        //    //Vector2 intersectionPoint = Intersections.GetIntersectionCoordinate(a, b, c, d);

        //    //Gizmos.DrawWireSphere(new Vector3(intersectionPoint.x, 0f, intersectionPoint.y), 0.5f);

        //    print("Are intersecting");
        //}
        //else
        //{
        //    print("Are not intersecting");
        //}


        //point in triangle
        //Gizmos.DrawLine(pointATrans.position, pointBTrans.position);
        //Gizmos.DrawLine(pointBTrans.position, pointCTrans.position);
        //Gizmos.DrawLine(pointCTrans.position, pointATrans.position);

        //if (Intersections.IsPointInTriangle(a, b, c, d))
        //{
        //    Gizmos.DrawWireSphere(pointDTrans.position, 1f);
        //}


        //Clockwise
        //print(Geometry.IsTriangleOrientedClockwise(a, b, c));


        //Change triangle orientation
        //Triangle triangle = new Triangle(new Vertex(pointATrans.position), new Vertex(pointBTrans.position), new Vertex(pointCTrans.position));

        //print(Geometry.IsTriangleOrientedClockwise(triangle.v1.position, triangle.v2.position, triangle.v3.position));

        //triangle.ChangeOrientation();

        //print(Geometry.IsTriangleOrientedClockwise(triangle.v1.position, triangle.v2.position, triangle.v3.position));


        //Is a simple quadrilateral (something with 4 sides) convex?
        //Debug.Log(Geometry.IsQuadrilateralConvex(a, b, c, d));

        //Gizmos.DrawLine(pointATrans.position, pointBTrans.position);
        //Gizmos.DrawLine(pointBTrans.position, pointCTrans.position);
        //Gizmos.DrawLine(pointCTrans.position, pointDTrans.position);
        //Gizmos.DrawLine(pointDTrans.position, pointATrans.position);


        //The closest point on a line sigement from a point
        //Vector2 closestPoint = Geometry.GetClosestPointOnLineSegment(a, b, c);

        //Display the line a-b
        //Gizmos.DrawLine(pointATrans.position, pointBTrans.position);

        //Display the point
        //Gizmos.DrawWireSphere(new Vector3(closestPoint.x, 0f, closestPoint.y), 1f);



        //Angle between vectors
        //float anglebetween = Geometry.CalculateAngleBetweenVectors(a, b, c);

        //Debug.Log(anglebetween);

        Gizmos.DrawLine(pointATrans.position, pointBTrans.position);
        Gizmos.DrawLine(pointBTrans.position, pointCTrans.position);

        Vector3 A = pointATrans.position;
        Vector3 B = pointBTrans.position;
        Vector3 C = pointCTrans.position;
        //Vector3 D = pointDTrans.position;

        Vector3 ab = B - A;
        Vector3 bc = C - B;

        Vector3 normal_1 = new Vector3(ab.z, 0f, -ab.x).normalized;
        Vector3 normal_2 = new Vector3(bc.z, 0f, -bc.x).normalized;

        Gizmos.DrawRay((A + B) * 0.5f, normal_1);
        Gizmos.DrawRay((B + C) * 0.5f, normal_2);

        float angle = Geometry.CalculateAngleBetweenVectors(a, b, c);

        //Vector2 vec1 = a - b;
        //Vector2 vec2 = c - b;

        //float dot = vec1.x * vec2.x + vec1.y * vec2.y;

        //float det = vec1.x * vec2.y - vec1.y * vec2.x;

        ////float angle = Mathf.Atan2(det, dot) * Mathf.Rad2Deg;


        ////float angle = Quaternion.FromToRotation(Vector3.up, to - from).eulerAngles.z;

        //Vector3 from = A - B;
        //Vector3 to = C - B;

        //float angle = Vector3.SignedAngle(from, to, Vector3.up);

        Debug.Log(angle);
    }
}
