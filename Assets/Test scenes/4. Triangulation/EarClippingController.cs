using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

public class EarClippingController : MonoBehaviour
{
    public Transform hullParent;



    public void GenerateTriangulation()
    {
        
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
            return;
        }

        // Debug.Log(pointsOnHull.Count);

        Gizmos.color = Color.white;

       for (int i = 0; i < pointsOnHull.Count; i++)
       {
            Vector3 p1 = pointsOnHull[MathUtility.ClampListIndex(i - 1, pointsOnHull.Count)];
            Vector3 p2 = pointsOnHull[MathUtility.ClampListIndex(i + 0, pointsOnHull.Count)];

            Gizmos.DrawLine(p1, p2);

            Gizmos.DrawWireSphere(p1, 0.1f);
       }
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
}
