using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;

public class EarClippingController : MonoBehaviour
{
    //The parent to the points that form the hull (should be ordered counter-clockwise)
    public Transform hullParent;
    //The parent to the points that form a single hole (should be ordered clockwise)
    public Transform holeParent_1;

    //So we can generate these in a separate method and display them in draw gizmos 
    private HashSet<Triangle2> triangulation;



    public void GenerateTriangulation()
    {
        List<Vector3> pointsOnHull = GetPointsFromParent(hullParent);

        if (pointsOnHull == null)
        {
            Debug.Log("We have no points on the hull");

            return;
        }

        //Ear Clipping is a 2d algorithm so convert
        List<MyVector2> pointsOnHull_2d = pointsOnHull.Select(p => new MyVector2(p.x, p.z)).ToList();


        //Holes
        List<Vector3> pointsHole_1 = GetPointsFromParent(holeParent_1);

        List<MyVector2> pointsHole_2d = null;

        if (pointsHole_1 != null)
        {
            pointsHole_2d = pointsHole_1.Select(p => new MyVector2(p.x, p.z)).ToList();
        }
        else
        {
            Debug.Log("A hole has no points");
        }


        //Triangulate
        triangulation = EarClipping.Triangulate(pointsOnHull_2d, pointsHole_2d);

        Debug.Log($"Number of triangles from ear clipping: {triangulation.Count}");
    }



    private void OnDrawGizmos()
    {
        DisplayTriangles();

        DisplayConnectedPoints(hullParent, Color.white);
        
        DisplayConnectedPoints(holeParent_1, Color.white);
    }



    private void DisplayTriangles()
    {
        if (triangulation == null)
        {
            return;
        }


        //Convert from triangle to mesh
        Mesh mesh = _TransformBetweenDataStructures.Triangles2ToMesh(triangulation, false);

        TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors(mesh, 0);
    }



    private void DisplayConnectedPoints(Transform parentToPoints, Color color)
    {
        List<Vector3> pointsOnHull = GetPointsFromParent(parentToPoints);

        if (pointsOnHull == null)
        {
            Debug.Log("We have no points on the hull");

            return;
        }

        //Debug.Log(pointsOnHull.Count);

        Gizmos.color = color;

        for (int i = 0; i < pointsOnHull.Count; i++)
        {
            Vector3 p1 = pointsOnHull[MathUtility.ClampListIndex(i - 1, pointsOnHull.Count)];
            Vector3 p2 = pointsOnHull[MathUtility.ClampListIndex(i + 0, pointsOnHull.Count)];

            //Direction is important so we should display an arrow show the order of the points
            if (i == 0)
            {
                TestAlgorithmsHelpMethods.DisplayArrow(p1, p2, 0.2f, color);
            }
            else
            {
                Gizmos.DrawLine(p1, p2);
            }

            Gizmos.DrawWireSphere(p1, 0.1f);
        }
    }



    //This makes it easier to move the points by using editor tools than identify which point is which by selecting at its gameobject
    //To make it work you have to select the gameobject this script is attached to
    public List<Transform> GetAllPoints()
    {
        List<Transform> allPoints = new List<Transform>();
    

        //Points on the hull
        List<Transform> childPointsOnHull = GetChildTransformsFromParent(hullParent);

        if (childPointsOnHull != null) allPoints.AddRange(childPointsOnHull);


        //Holes
        List<Transform> childPointsHole_1 = GetChildTransformsFromParent(holeParent_1);

        if (childPointsHole_1 != null) allPoints.AddRange(childPointsHole_1);
        

        return allPoints;
    }



    //Get all child points to a parent transform
    public List<Vector3> GetPointsFromParent(Transform parentTrans)
    {
        if (parentTrans == null)
        {
            Debug.Log("No parent so cant get children");

            return null;
        }

        //Is not including the parent
        int children = parentTrans.childCount;

        List<Vector3> childrenPositions = new List<Vector3>();

        for (int i = 0; i < children; i++)
        {
            childrenPositions.Add(parentTrans.GetChild(i).position);
        }

        return childrenPositions;
    }

    public List<Transform> GetChildTransformsFromParent(Transform parentTrans)
    {
        if (parentTrans == null)
        {
            Debug.Log("No parent so cant get children");

            return null;
        }

        //Is not including the parent
        int children = parentTrans.childCount;

        List<Transform> childrenTransforms = new List<Transform>();

        for (int i = 0; i < children; i++)
        {
            childrenTransforms.Add(parentTrans.GetChild(i));
        }

        return childrenTransforms;
    }
}
