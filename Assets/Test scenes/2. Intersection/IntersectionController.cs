using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

public class IntersectionController : MonoBehaviour 
{
    //Polygon
    public Transform polygonPointsParentTrans;

    //Point
    public Transform pointTrans;

    //Triangles
    public Transform t1_p1_trans;
    public Transform t1_p2_trans;
    public Transform t1_p3_trans;

    public Transform t2_p1_trans;
    public Transform t2_p2_trans;
    public Transform t2_p3_trans;

    //Ray
    public Transform rayTrans;

    //Plane
    public Transform planeTrans;



    void OnDrawGizmos() 
	{
        //PointPolygon();

        //TriangleTriangle();

        //RayPlane();

        LinePlane();

        //PointCircle();

        //LineLine();

        //AABB_AABB();

        //PointTriangle();
    }



    //Is a point intersecting with a triangle?
    private void PointTriangle()
    {
        Vector2 p = pointTrans.position.XZ();

        Vector2 t_p1 = t1_p1_trans.position.XZ();
        Vector2 t_p2 = t1_p2_trans.position.XZ();
        Vector2 t_p3 = t1_p3_trans.position.XZ();

        Triangle2D t = new Triangle2D(t_p1, t_p2, t_p3);

        bool isIntersecting = Intersections.PointTriangle(t, p, includeBorder: true);

        //Display
        Gizmos.color = isIntersecting ? Color.red : Color.white;

        Gizmos.DrawWireSphere(p.XYZ(), 0.1f);

        Gizmos.DrawLine(t.p1.XYZ(), t.p2.XYZ());
        Gizmos.DrawLine(t.p2.XYZ(), t.p3.XYZ());
        Gizmos.DrawLine(t.p3.XYZ(), t.p1.XYZ());
    }



    //Are two lines intersecting?
    private void LineLine()
    {
        Vector2 l1_p1 = t1_p1_trans.position.XZ();
        Vector2 l1_p2 = t1_p2_trans.position.XZ();

        Vector2 l2_p1 = t2_p1_trans.position.XZ();
        Vector2 l2_p2 = t2_p2_trans.position.XZ();

        bool isIntersecting = Intersections.LineLine(l1_p1, l1_p2, l2_p1, l2_p2, shouldIncludeEndPoints: true);

        //Display
        Gizmos.color = isIntersecting ? Color.red : Color.white;

        Gizmos.DrawLine(l1_p1.XYZ(), l1_p2.XYZ());
        Gizmos.DrawLine(l2_p1.XYZ(), l2_p2.XYZ());

        //If they are intersecting we can also get the intersection point
        if (isIntersecting)
        {
            Vector2 intersectionPoint = Intersections.GetLineLineIntersectionPoint(l1_p1, l1_p2, l2_p1, l2_p2);

            Gizmos.DrawWireSphere(intersectionPoint.XYZ(), 0.1f);
        }
    }



    //Are two AABB intersecting?
    private void AABB_AABB()
    {
        Vector2 t1_p1 = t1_p1_trans.position.XZ();
        Vector2 t1_p2 = t1_p2_trans.position.XZ();
        Vector2 t1_p3 = t1_p3_trans.position.XZ();

        Vector2 t2_p1 = t2_p1_trans.position.XZ();
        Vector2 t2_p2 = t2_p2_trans.position.XZ();
        Vector2 t2_p3 = t2_p3_trans.position.XZ();

        Triangle2D t1 = new Triangle2D(t1_p1, t1_p2, t1_p3);
        Triangle2D t2 = new Triangle2D(t2_p1, t2_p2, t2_p3);

        bool isIntersecting = Intersections.AABB_AABB_2D(
            t1.MinX(), t1.MaxX(), t1.MinY(), t1.MaxY(),
            t2.MinX(), t2.MaxX(), t2.MinY(), t2.MaxY());

        Debug.Log("AABB intersecting: " + isIntersecting);

        //Display the rectangles and the vertices we use to make the rectangles
        Vector3 r1_size = new Vector3(t1.MaxX() - t1.MinX(), 0.01f, t1.MaxY() - t1.MinY());
        Vector3 r2_size = new Vector3(t2.MaxX() - t2.MinX(), 0.01f, t2.MaxY() - t2.MinY());

        Vector3 r1_center = new Vector3(t1.MinX() + (r1_size.x * 0.5f), 0f, t1.MinY() + (r1_size.z * 0.5f));
        Vector3 r2_center = new Vector3(t2.MinX() + (r2_size.x * 0.5f), 0f, t2.MinY() + (r2_size.z * 0.5f));

        Gizmos.color = Color.white;

        Gizmos.DrawWireCube(r1_center, r1_size);

        float r = 0.01f;

        Gizmos.DrawWireSphere(t1_p1.XYZ(), r);
        Gizmos.DrawWireSphere(t1_p2.XYZ(), r);
        Gizmos.DrawWireSphere(t1_p3.XYZ(), r);

        Gizmos.color = Color.blue;

        Gizmos.DrawWireCube(r2_center, r2_size);

        Gizmos.DrawWireSphere(t2_p1.XYZ(), r);
        Gizmos.DrawWireSphere(t2_p2.XYZ(), r);
        Gizmos.DrawWireSphere(t2_p3.XYZ(), r);
    }



    //Is a point intersecting with a circle?
    private void PointCircle()
    {
        Vector2 testPoint = pointTrans.position.XZ();

        Vector2 circlePointA = t1_p1_trans.position.XZ();
        Vector2 circlePointB = t1_p2_trans.position.XZ();
        Vector2 circlePointC = t1_p3_trans.position.XZ();

        //Is a point in a circle determines by three other points
        float isPointInCircle = Intersections.PointCircle(circlePointA, circlePointB, circlePointC, testPoint);

        print(isPointInCircle);


        //Display the circle
        Gizmos.color = Color.white;
        
        Vector2 centerOfCicle = Geometry.CalculateCircleCenter(circlePointA, circlePointB, circlePointC);

        float radius = Vector2.Distance(centerOfCicle, circlePointA);

        Gizmos.DrawWireSphere(new Vector3(centerOfCicle.x, 0f, centerOfCicle.y), radius);

        //Display the points
        float pointRadius = 0.2f;

        Gizmos.color = Color.blue;

        Gizmos.DrawWireSphere(pointTrans.position, pointRadius);

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(t1_p1_trans.position, pointRadius);
        Gizmos.DrawWireSphere(t1_p2_trans.position, pointRadius);
        Gizmos.DrawWireSphere(t1_p3_trans.position, pointRadius);
    }



    //Is a line intersecting with a plane?
    private void LinePlane()
    {
        Vector3 planeNormal = planeTrans.forward;

        Vector3 planePos = planeTrans.position;

        Vector3 line_p1 = t1_p1_trans.position;

        Vector3 line_p2 = t1_p2_trans.position;


        //2d space
        Vector2 planeNormal_2d = planeNormal.XZ();

        Vector2 planePos_2d = planePos.XZ();

        Vector3 line_p1_2d = line_p1.XZ();

        Vector3 line_p2_2d = line_p2.XZ();


        bool isIntersecting = Intersections.LinePlane(planePos_2d, planeNormal_2d, line_p1_2d, line_p2_2d);


        //Debug
        Gizmos.color = Color.blue;

        Vector3 planeDir = new Vector3(planeNormal_2d.y, 0f, -planeNormal_2d.x);

        //Draw the plane which is just a long line
        float infinite = 100f;

        Gizmos.DrawRay(planePos, planeDir * infinite);
        Gizmos.DrawRay(planePos, -planeDir * infinite);

        //Draw the plane normal
        Gizmos.DrawLine(planePos, planePos + planeNormal * 1f);


        //Line
        Gizmos.color = Color.white;

        Gizmos.DrawWireSphere(line_p1, 0.1f);
        Gizmos.DrawWireSphere(line_p2, 0.1f);

        if (isIntersecting)
        {
            Gizmos.color = Color.red;

            Vector2 intersectionPoint = Intersections.GetLinePlaneIntersectionCoordinate(planePos_2d, planeNormal_2d, line_p1_2d, line_p2_2d);

            Gizmos.DrawWireSphere(intersectionPoint.XYZ(), 0.2f);
        }

        Gizmos.DrawLine(line_p1, line_p2);
    }



    //Is a ray intersecting with a plane?
    private void RayPlane()
    {
        Vector3 planeNormal = planeTrans.forward;

        Vector3 planePos = planeTrans.position;

        Vector3 rayPos = rayTrans.position;

        Vector3 rayDir = rayTrans.forward;


        //2d space
        Vector2 planeNormal_2d = planeNormal.XZ();

        Vector2 planePos_2d = planePos.XZ();

        Vector2 rayPos_2d = rayPos.XZ();

        Vector2 rayDir_2d = rayDir.XZ();


        //Might as well test the distance from the point to the plane as well
        float distance = Geometry.DistanceFromPointToPlane(planeNormal_2d, planePos_2d, rayPos_2d);

        Debug.Log(distance);

        bool isIntersecting = Intersections.RayPlane(planePos_2d, planeNormal_2d, rayPos_2d, rayDir_2d);


        //Debug
        Gizmos.color = Color.blue;

        Vector3 planeDir = new Vector3(planeNormal_2d.y, 0f, -planeNormal_2d.x);

        //Draw the plane which is just a long line
        float infinite = 100f;

        Gizmos.DrawRay(planePos, planeDir * infinite);
        Gizmos.DrawRay(planePos, -planeDir * infinite);
        
        //Draw the plane normal
        Gizmos.DrawLine(planePos, planePos + planeNormal * 1f);

        
        //Ray
        Gizmos.color = Color.white;

        Gizmos.DrawWireSphere(rayPos, 0.1f);

        if (isIntersecting)
        {
            Gizmos.color = Color.red;

            Vector2 intersectionPoint = Intersections.GetRayPlaneIntersectionCoordinate(planePos_2d, planeNormal_2d, rayPos_2d, rayDir_2d);

            Gizmos.DrawWireSphere(intersectionPoint.XYZ(), 0.2f);
        }

        Gizmos.DrawRay(rayPos, rayDir * infinite);
    }



    //Is a triangle intersecting with a triangle?
    private void TriangleTriangle()
    {
        Triangle t1 = new Triangle(t1_p1_trans.position, t1_p2_trans.position, t1_p3_trans.position);
        Triangle t2 = new Triangle(t2_p1_trans.position, t2_p2_trans.position, t2_p3_trans.position);

        //3d to 2d
        Triangle2D t1_2d = new Triangle2D(t1.p1.XZ(), t1.p2.XZ(), t1.p3.XZ());
        Triangle2D t2_2d = new Triangle2D(t2.p1.XZ(), t2.p2.XZ(), t2.p3.XZ());

        bool isIntersecting = Intersections.TriangleTriangle2D(t1_2d, t2_2d, do_AABB_test: false);



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



    //Is a point intersecting with a polygon
    private void PointPolygon()
    {
        List<Vector3> polygonPoints = GetVerticesFromParent(polygonPointsParentTrans);

        //To 2d
        List<Vector2> polygonPoints_2d = new List<Vector2>();

        for (int i = 0; i < polygonPoints.Count; i++)
        {
            polygonPoints_2d.Add(polygonPoints[i].XZ());
        }

        Vector2 testPoint = pointTrans.position.XZ();

        //Is the point inside the polygon
        bool isIntersecting = Intersections.PointPolygon(polygonPoints_2d, testPoint);

        //Display
        Gizmos.color = isIntersecting ? Color.red : Color.white;

        for (int i = 0; i < polygonPoints.Count; i++)
        {
            int iPlusOne = MathUtility.ClampListIndex(i + 1, polygonPoints.Count);

            Gizmos.DrawLine(polygonPoints[i], polygonPoints[iPlusOne]);
        }

        Gizmos.DrawWireSphere(testPoint.XYZ(), 0.1f);
    }



    //
    // Help methods
    //

    //Get child vertex positions from parent trans
    private List<Vector3> GetVerticesFromParent(Transform parent)
    {
        int childCount = parent.childCount;

        List<Vector3> children = new List<Vector3>();

        for (int i = 0; i < childCount; i++)
        {
            children.Add(parent.GetChild(i).position);
        }

        return children;
    }
}
