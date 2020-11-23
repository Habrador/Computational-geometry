using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;

public class EarClippingController : MonoBehaviour
{
    public Transform hullParent;



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

        HashSet<Triangle2> triangles = EarClipping.Triangulate(pointsOnHull_2d);

        //Display
        if (triangles != null)
        {
            Debug.Log("Number of triangles from ear clipping: " + triangles.Count);

            //Convert from triangle to mesh
            Mesh mesh = _TransformBetweenDataStructures.Triangles2ToMesh(triangles, false);

            TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors(mesh, 0);
        }
    }



    private void OnDrawGizmos()
    {
        DisplayHull();    
    }



    private void DisplayHull()
    {
        List<Vector3> pointsOnHull = GetPointsFromParent(hullParent);

        if (pointsOnHull == null)
        {
            Debug.Log("We have no points on the hull");

            return;
        }

        //Debug.Log(pointsOnHull.Count);

        Gizmos.color = Color.white;

       for (int i = 0; i < pointsOnHull.Count; i++)
       {
            Vector3 p1 = pointsOnHull[MathUtility.ClampListIndex(i - 1, pointsOnHull.Count)];
            Vector3 p2 = pointsOnHull[MathUtility.ClampListIndex(i + 0, pointsOnHull.Count)];

            Gizmos.DrawLine(p1, p2);

            Gizmos.DrawWireSphere(p1, 0.1f);
       }
    }



    //This makes it easier to move the points by using editor tools than identify which point is which by selecting at its gameobject
    //To make it work you have to select the gameobject this script is attached to
    public List<Transform> GetAllPoints()
    {
        List<Transform> allPoints = new List<Transform>();
    

        //Points on the hull
        List<Transform> pointsOnHull = GetTransformsFromParent(hullParent);

        if (pointsOnHull != null)
        {
            allPoints.AddRange(pointsOnHull);
        }


        //Holes

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

    public List<Transform> GetTransformsFromParent(Transform parentTrans)
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
