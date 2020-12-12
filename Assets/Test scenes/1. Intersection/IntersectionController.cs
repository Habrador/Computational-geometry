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

        //PointCircle();

        //LineLine();

        //AABB_AABB();

        //PointTriangle();

        //PlanePlane();

        //RayPlane();

        LinePlane();
    }



    //Is a plane intersecting with a plane
    private void PlanePlane()
    {
        Vector3 planeNormal_1 = planeTrans.forward;

        Vector3 planePos_1 = planeTrans.position;

        Vector3 planeNormal_2 = rayTrans.forward;

        Vector3 planePos_2 = rayTrans.position;

        //3d to 2d
        MyVector2 normal_1 = planeNormal_1.ToMyVector2();
        MyVector2 normal_2 = planeNormal_2.ToMyVector2();

        MyVector2 pos1 = planePos_1.ToMyVector2();
        MyVector2 pos2 = planePos_2.ToMyVector2();

        Plane2 plane_1 = new Plane2(pos1, normal_1);
        Plane2 plane_2 = new Plane2(pos2, normal_2);

        //Intersections
        bool isIntersecting = _Intersections.PlanePlane(plane_1, plane_2);

        Debug.Log("Are planes intersecting: " + isIntersecting);


        //Display
        //if (isIntersecting)
        //{
        //    MyVector2 intersectionPoint = Intersections.GetPlanePlaneIntersectionPoint(pos1, normal_1, pos2, normal_2);

        //    Gizmos.DrawWireSphere(intersectionPoint.ToVector3(), 0.2f);
        //}

        //Color planeColor = isIntersecting ? Color.red : Color.white;

        //TestAlgorithmsHelpMethods.DrawPlane(pos1, normal_1, planeColor);
        //TestAlgorithmsHelpMethods.DrawPlane(pos2, normal_2, planeColor);


        
        //Display with mesh
        TestAlgorithmsHelpMethods.DisplayPlaneMesh(pos1, normal_1, 0.5f, Color.blue);
        TestAlgorithmsHelpMethods.DisplayPlaneMesh(pos2, normal_2, 0.5f, Color.blue);

        if (isIntersecting)
        {
            MyVector2 intersectionPoint = _Intersections.GetPlanePlaneIntersectionPoint(plane_1, plane_2);

            TestAlgorithmsHelpMethods.DisplayCircleMesh(intersectionPoint, 1f, 20, Color.red);
        }
    }


    //Is a point intersecting with a triangle?
    private void PointTriangle()
    {
        MyVector2 p = pointTrans.position.ToMyVector2();

        MyVector2 t_p1 = t1_p1_trans.position.ToMyVector2();
        MyVector2 t_p2 = t1_p2_trans.position.ToMyVector2();
        MyVector2 t_p3 = t1_p3_trans.position.ToMyVector2();

        Triangle2 t = new Triangle2(t_p1, t_p2, t_p3);

        bool isIntersecting = _Intersections.PointTriangle(t, p, includeBorder: true);

        //Display
        //Gizmos.color = isIntersecting ? Color.red : Color.white;

        //Gizmos.DrawWireSphere(p.ToVector3(), 0.1f);

        //Gizmos.DrawLine(t.p1.ToVector3(), t.p2.ToVector3());
        //Gizmos.DrawLine(t.p2.ToVector3(), t.p3.ToVector3());
        //Gizmos.DrawLine(t.p3.ToVector3(), t.p1.ToVector3());


        //With mesh to better see what's going on
        //Triangle
        TestAlgorithmsHelpMethods.DisplayTriangleMesh(t_p1, t_p2, t_p3, Color.white);

        //Point
        Color pointColor = isIntersecting ? Color.red : Color.white;

        TestAlgorithmsHelpMethods.DisplayCircleMesh(p, 1f, 20, pointColor);
    }



    //Are two lines intersecting?
    private void LineLine()
    {
        MyVector2 l1_p1 = t1_p1_trans.position.ToMyVector2();
        MyVector2 l1_p2 = t1_p2_trans.position.ToMyVector2();

        MyVector2 l2_p1 = t2_p1_trans.position.ToMyVector2();
        MyVector2 l2_p2 = t2_p2_trans.position.ToMyVector2();

        Edge2 l1 = new Edge2(l1_p1, l1_p2);
        Edge2 l2 = new Edge2(l2_p1, l2_p2);

        bool isIntersecting = _Intersections.LineLine(l1, l2, includeEndPoints: true);

        //Display

        //Gizmos.DrawLine(l1_p1.ToVector3(), l1_p2.ToVector3());
        //Gizmos.DrawLine(l2_p1.ToVector3(), l2_p2.ToVector3());

        //if (isIntersecting)
        //{
        //    MyVector2 intersectionPoint = Intersections.GetLineLineIntersectionPoint(l1_p1, l1_p2, l2_p1, l2_p2);

        //    //Gizmos.color = Color.red;

        //    //Gizmos.DrawSphere(intersectionPoint.ToVector3(), 1f);
        //}


        //With mesh

        //Line
        TestAlgorithmsHelpMethods.DisplayLineMesh(l1_p1, l1_p2, 0.5f, Color.white);
        TestAlgorithmsHelpMethods.DisplayLineMesh(l2_p1, l2_p2, 0.5f, Color.white);

        //If they are intersecting we can also get the intersection point
        if (isIntersecting)
        {
            MyVector2 intersectionPoint = _Intersections.GetLineLineIntersectionPoint(l1, l2);

            TestAlgorithmsHelpMethods.DisplayCircleMesh(intersectionPoint, 1f, 20, Color.red);
        }
    }



    //Are two AABB intersecting?
    private void AABB_AABB()
    {
        MyVector2 t1_p1 = t1_p1_trans.position.ToMyVector2();
        MyVector2 t1_p2 = t1_p2_trans.position.ToMyVector2();
        MyVector2 t1_p3 = t1_p3_trans.position.ToMyVector2();

        MyVector2 t2_p1 = t2_p1_trans.position.ToMyVector2();
        MyVector2 t2_p2 = t2_p2_trans.position.ToMyVector2();
        MyVector2 t2_p3 = t2_p3_trans.position.ToMyVector2();

        AABB2 r1 = new AABB2(new List<MyVector2>() { t1_p1, t1_p2, t1_p3 });
        AABB2 r2 = new AABB2(new List<MyVector2>() { t2_p1, t2_p2, t2_p3 });

        bool isIntersecting = _Intersections.AABB_AABB(r1, r2);

        Debug.Log("AABB intersecting: " + isIntersecting);

        //Display the rectangles and the vertices we use to make the rectangles
        Vector3 r1_size = new Vector3(r1.max.x - r1.min.x, 0.01f, r1.max.y - r1.min.y);
        Vector3 r2_size = new Vector3(r2.max.x - r2.min.x, 0.01f, r2.max.y - r2.min.y);

        Vector3 r1_center = new Vector3(r1.min.x + (r1_size.x * 0.5f), 0f, r1.min.y + (r1_size.z * 0.5f));
        Vector3 r2_center = new Vector3(r2.min.x + (r2_size.x * 0.5f), 0f, r2.min.y + (r2_size.z * 0.5f));

        Gizmos.color = Color.white;

        Gizmos.DrawCube(r1_center, r1_size);

        //float r = 0.1f;

        //Gizmos.DrawWireSphere(t1_p1.ToVector3(), r);
        //Gizmos.DrawWireSphere(t1_p2.ToVector3(), r);
        //Gizmos.DrawWireSphere(t1_p3.ToVector3(), r);

        Gizmos.color = isIntersecting ? Color.red : Color.white;
        

        Gizmos.DrawCube(r2_center, r2_size);

        //Gizmos.DrawWireSphere(t2_p1.ToVector3(), r);
        //Gizmos.DrawWireSphere(t2_p2.ToVector3(), r);
        //Gizmos.DrawWireSphere(t2_p3.ToVector3(), r);
    }



    //Is a point intersecting with a circle?
    private void PointCircle()
    {
        MyVector2 testPoint = pointTrans.position.ToMyVector2();

        MyVector2 circlePointA = t1_p1_trans.position.ToMyVector2();
        MyVector2 circlePointB = t1_p2_trans.position.ToMyVector2();
        MyVector2 circlePointC = t1_p3_trans.position.ToMyVector2();

        //Is a point in a circle determines by three other points
        IntersectionCases intersectionCases = _Intersections.PointCircle(circlePointA, circlePointB, circlePointC, testPoint);

        //print(isPointInCircle);


        //Display the circle
        //if (intersectionCases == IntersectionCases.NoIntersection)
        //{
        //    Gizmos.color = Color.white;
        //}
        //if (intersectionCases == IntersectionCases.IsInside)
        //{
        //    Gizmos.color = Color.red;
        //}
        //if (intersectionCases == IntersectionCases.IsOnEdge)
        //{
        //    Gizmos.color = Color.blue;
        //}


        MyVector2 centerOfCicle = _Geometry.CalculateCircleCenter(circlePointA, circlePointB, circlePointC);

        float radius = MyVector2.Distance(centerOfCicle, circlePointA);

        //Gizmos.DrawWireSphere(centerOfCicle.ToVector3(), radius);

        ////Display the points
        //float pointRadius = 0.2f;

        //Gizmos.DrawWireSphere(pointTrans.position, pointRadius);

        //Gizmos.DrawWireSphere(t1_p1_trans.position, pointRadius);
        //Gizmos.DrawWireSphere(t1_p2_trans.position, pointRadius);
        //Gizmos.DrawWireSphere(t1_p3_trans.position, pointRadius);


        //With mesh
        //Big circle
        TestAlgorithmsHelpMethods.DisplayCircleMesh(centerOfCicle, radius, 60, Color.white);

        //Small circle
        Color circleColor = (intersectionCases == IntersectionCases.IsInside) ? Color.red : Color.white;

        TestAlgorithmsHelpMethods.DisplayCircleMesh(testPoint, 1f, 20, circleColor);
    }



    //Is a line intersecting with a plane?
    private void LinePlane()
    {
        Vector3 planeNormal = planeTrans.forward;

        Vector3 planePos = planeTrans.position;

        Vector3 line_p1 = t1_p1_trans.position;

        Vector3 line_p2 = t1_p2_trans.position;


        //2d space
        MyVector2 planeNormal_2d = planeNormal.ToMyVector2();

        MyVector2 planePos_2d = planePos.ToMyVector2();

        MyVector2 line_p1_2d = line_p1.ToMyVector2();

        MyVector2 line_p2_2d = line_p2.ToMyVector2();

        Plane2 plane = new Plane2(planePos_2d, planeNormal_2d);

        Edge2 line = new Edge2(line_p1_2d, line_p2_2d);

        bool isIntersecting = _Intersections.LinePlane(plane, line);


        //Debug
        //TestAlgorithmsHelpMethods.DrawPlane(planePos_2d, planeNormal_2d, Color.blue);

        ////Line
        //Gizmos.color = Color.white;

        //Gizmos.DrawWireSphere(line_p1, 0.1f);
        //Gizmos.DrawWireSphere(line_p2, 0.1f);

        //if (isIntersecting)
        //{
        //    Gizmos.color = Color.red;

        //    MyVector2 intersectionPoint = Intersections.GetLinePlaneIntersectionPoint(planePos_2d, planeNormal_2d, line_p1_2d, line_p2_2d);

        //    Gizmos.DrawWireSphere(intersectionPoint.ToVector3(), 0.2f);
        //}

        //Gizmos.DrawLine(line_p1, line_p2);


        //Display with mesh
        //Plane
        TestAlgorithmsHelpMethods.DisplayPlaneMesh(planePos_2d, planeNormal_2d, 0.5f, Color.blue);

        //Line
        TestAlgorithmsHelpMethods.DisplayLineMesh(line_p1_2d, line_p2_2d, 0.5f, Color.white);

        if (isIntersecting)
        {
            MyVector2 intersectionPoint = _Intersections.GetLinePlaneIntersectionPoint(plane, line);

            TestAlgorithmsHelpMethods.DisplayCircleMesh(intersectionPoint, 1f, 20, Color.red);
        }
    }



    //Is a ray intersecting with a plane?
    private void RayPlane()
    {
        Vector3 planeNormal = planeTrans.forward;

        Vector3 planePos = planeTrans.position;

        Vector3 rayPos = rayTrans.position;

        Vector3 rayDir = rayTrans.forward;


        //2d space
        Plane2 plane_2d = new Plane2(planePos.ToMyVector2(), planeNormal.ToMyVector2());

        Ray2 ray_2d = new Ray2(rayPos.ToMyVector2(), rayDir.ToMyVector2());

        //Might as well test the distance from the point to the plane as well
        float distance = _Geometry.GetSignedDistanceFromPointToPlane(ray_2d.origin, plane_2d);

        Debug.Log(distance);

        bool isIntersecting = _Intersections.RayPlane(plane_2d, ray_2d);


        //Debug
        Gizmos.color = Color.blue;

        TestAlgorithmsHelpMethods.DrawPlane(plane_2d.pos, plane_2d.normal, Color.blue);


        //Ray
        //Gizmos.color = Color.white;

        //Gizmos.DrawWireSphere(rayPos, 0.1f);

        //if (isIntersecting)
        //{
        //    Gizmos.color = Color.red;

        //    MyVector2 intersectionPoint = Intersections.GetRayPlaneIntersectionPoint(planePos_2d, planeNormal_2d, rayPos_2d, rayDir_2d);

        //    Gizmos.DrawWireSphere(intersectionPoint.ToVector3(), 0.2f);
        //}

        //Gizmos.DrawRay(rayPos, rayDir * 100f);


        
        //Display with mesh
        //Plane
        TestAlgorithmsHelpMethods.DisplayPlaneMesh(plane_2d.pos, plane_2d.normal, 0.5f, Color.blue);

        //Ray
        TestAlgorithmsHelpMethods.DisplayArrowMesh(ray_2d.origin, ray_2d.origin + ray_2d.dir * 6f, 0.5f, 0.5f + 0.5f, Color.white);

        if (isIntersecting)
        {
            MyVector2 intersectionPoint = _Intersections.GetRayPlaneIntersectionPoint(plane_2d, ray_2d);

            TestAlgorithmsHelpMethods.DisplayCircleMesh(intersectionPoint, 1f, 20, Color.red);
        }
    }



    //Is a triangle intersecting with a triangle?
    private void TriangleTriangle()
    {
        //3d to 2d
        Triangle2 t1 = new Triangle2(
            t1_p1_trans.transform.position.ToMyVector2(), 
            t1_p2_trans.transform.position.ToMyVector2(), 
            t1_p3_trans.transform.position.ToMyVector2());
        
        Triangle2 t2 = new Triangle2(
            t2_p1_trans.transform.position.ToMyVector2(), 
            t2_p2_trans.transform.position.ToMyVector2(), 
            t2_p3_trans.transform.position.ToMyVector2());

        bool isIntersecting = _Intersections.TriangleTriangle(t1, t2, do_AABB_test: false);



        //Display
        //Color color = isIntersecting ? Color.red : Color.white;

        //TestAlgorithmsHelpMethods.DisplayTriangle(t1.p1.ToVector3(), t1.p2.ToVector3(), t1.p3.ToVector3(), color);
        //TestAlgorithmsHelpMethods.DisplayTriangle(t2.p1.ToVector3(), t2.p2.ToVector3(), t2.p3.ToVector3(), color);


        //With mesh to better see what's going on
        TestAlgorithmsHelpMethods.DisplayTriangleMesh(t1.p1, t1.p2, t1.p3, Color.white);

        Color meshColor = isIntersecting ? Color.red : Color.white;

        TestAlgorithmsHelpMethods.DisplayTriangleMesh(t2.p1, t2.p2, t2.p3, meshColor);
    }



    //Is a point intersecting with a polygon
    private void PointPolygon()
    {
        List<Vector3> polygonPoints = GetVerticesFromParent(polygonPointsParentTrans);

        //To 2d
        List<MyVector2> polygonPoints_2d = new List<MyVector2>();

        for (int i = 0; i < polygonPoints.Count; i++)
        {
            polygonPoints_2d.Add(polygonPoints[i].ToMyVector2());
        }

        Vector3 testPoint = pointTrans.position;

        //Is the point inside the polygon
        bool isIntersecting = _Intersections.PointPolygon(polygonPoints_2d, testPoint.ToMyVector2());

        //Display
        //Gizmos.color = isIntersecting ? Color.red : Color.white;

        //for (int i = 0; i < polygonPoints.Count; i++)
        //{
        //    int iPlusOne = MathUtility.ClampListIndex(i + 1, polygonPoints.Count);

        //    Gizmos.DrawLine(polygonPoints[i], polygonPoints[iPlusOne]);
        //}

        //Gizmos.DrawWireSphere(testPoint, 0.1f);


        //With mesh to better see what's going on
        //Line
        TestAlgorithmsHelpMethods.DisplayConnectedLinesMesh(polygonPoints_2d, 0.5f, Color.white);

        //Point
        Color circleColor = isIntersecting ? Color.red : Color.white;

        TestAlgorithmsHelpMethods.DisplayCircleMesh(testPoint.ToMyVector2(), 1f, 20, circleColor);
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
