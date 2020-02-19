using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

public class IntersectionController : MonoBehaviour 
{
    public List<Transform> polygonPointsTrans;

    public Transform pointTrans;

    public Transform t1_p1_trans;
    public Transform t1_p2_trans;
    public Transform t1_p3_trans;

    public Transform t2_p1_trans;
    public Transform t2_p2_trans;
    public Transform t2_p3_trans;

    public Transform rayTrans;
    public Transform planeTrans;

    public Transform lineP1Trans;
    public Transform lineP2Trans;



    void OnDrawGizmos() 
	{
        //Point in polygon
        //PointInPolygon();


        //Triangle-triangle
        //TriangleTriangle();


        //Ray-plane
        //RayPlane();


        //Line-plane
        LinePlane();
    }



    private void LinePlane()
    {
        Vector3 planeNormal = planeTrans.forward;

        Vector3 planePos = planeTrans.position;

        Vector3 linePos1 = lineP1Trans.position;

        Vector3 linePos2 = lineP2Trans.position;

        //2d
        planeNormal.y = 0f;
        planePos.y = 0f;
        linePos1.y = 0f;
        linePos2.y = 0f;


        //Debug
        Gizmos.color = Color.blue;

        Vector3 planeDir = new Vector3(planeNormal.z, 0f, -planeNormal.x);

        Gizmos.DrawRay(planePos, planeDir * 100f);
        Gizmos.DrawRay(planePos, -planeDir * 100f);

        Gizmos.DrawLine(planePos, planePos + planeNormal * 1f);

        //Line
        Gizmos.color = Color.white;

        Gizmos.DrawWireSphere(linePos1, 0.1f);
        Gizmos.DrawWireSphere(linePos2, 0.1f);

        if (Intersections.AreLinePlaneIntersecting(planeNormal, planePos, linePos1, linePos2))
        {
            Gizmos.color = Color.red;
        }

        Gizmos.DrawLine(linePos1, linePos2);
    }



    private void RayPlane()
    {
        Vector3 planeNormal = planeTrans.forward;

        Vector3 planePos = planeTrans.position;

        Vector3 rayPos = rayTrans.position;

        Vector3 rayDir = rayTrans.forward;

        //2d
        planeNormal.y = 0f;
        planePos.y = 0f;
        rayPos.y = 0f;
        rayDir.y = 0f;



        //Is the point to the left or to the right of the plane
        float distance = Geometry.DistanceFromPointToPlane(planeNormal, planePos, rayPos);

        Debug.Log(distance);

        bool areIntersecting = Intersections.AreRayPlaneIntersecting(planePos, planeNormal, rayPos, rayDir);


        //Debug
        Gizmos.color = Color.blue;

        Vector3 planeDir = new Vector3(planeNormal.z, 0f, -planeNormal.x);

        Gizmos.DrawRay(planePos, planeDir * 100f);
        Gizmos.DrawRay(planePos, -planeDir * 100f);

        Gizmos.DrawLine(planePos, planePos + planeNormal * 1f);

        //Point
        Gizmos.color = Color.white;

        Gizmos.DrawWireSphere(rayPos, 0.1f);

        ////if (areIntersecting)
        //{
        //    Gizmos.color = Color.red;

        //    Vector3 intersectionPoint = Intersections.GetRayPlaneIntersectionCoordinate(planePos, planeNormal, rayPos, rayDir);

        //    Gizmos.DrawWireSphere(intersectionPoint, 0.2f);
        //}

        //Gizmos.DrawRay(rayPos, rayDir * 100f);


    }



    private void TriangleTriangle()
    {
        Triangle t1 = new Triangle(t1_p1_trans.position, t1_p2_trans.position, t1_p3_trans.position);
        Triangle t2 = new Triangle(t2_p1_trans.position, t2_p2_trans.position, t2_p3_trans.position);

        //3d to 2d
        Triangle2D t1_2d = new Triangle2D(t1.p1.XZ(), t1.p2.XZ(), t1.p3.XZ());
        Triangle2D t2_2d = new Triangle2D(t2.p1.XZ(), t2.p2.XZ(), t2.p3.XZ());

        bool isIntersecting = Intersections.AreTrianglesIntersecting2D(t1_2d, t2_2d, false);



        //Display
        Gizmos.color = Color.white;

        if (isIntersecting)
        {
            Gizmos.color = Color.red;
        }

        Gizmos.DrawLine(t1.p1, t1.p2);
        Gizmos.DrawLine(t1.p2, t1.p3);
        Gizmos.DrawLine(t1.p3, t1.p1);

        Gizmos.DrawLine(t2.p1, t2.p2);
        Gizmos.DrawLine(t2.p2, t2.p3);
        Gizmos.DrawLine(t2.p3, t2.p1);
    }



    private void PointInPolygon()
    {
        List<Vector2> polygonPoints = new List<Vector2>();

        for (int i = 0; i < polygonPointsTrans.Count; i++)
        {
            polygonPoints.Add(new Vector2(polygonPointsTrans[i].position.x, polygonPointsTrans[i].position.z));
        }

        Vector2 testPoint = new Vector2(pointTrans.position.x, pointTrans.position.z);



        //Is the point inside the polygon
        bool isInside = Intersections.IsPointInPolygon(polygonPoints, testPoint);



        //Display
        Gizmos.color = Color.white;

        for (int i = 0; i < polygonPoints.Count; i++)
        {
            int iPlusOne = MathUtility.ClampListIndex(i + 1, polygonPoints.Count);

            Gizmos.DrawLine(polygonPoints[i], polygonPoints[iPlusOne]);
        }

        if (isInside)
        {
            Gizmos.color = Color.blue;
        }

        Gizmos.DrawWireSphere(testPoint, 0.1f);
    }
	
}
