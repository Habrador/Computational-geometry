using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

//Help methods for displaying curves
public static class DisplayInterpolation
{
    //Display interpolated values
    public static void DisplayCurve(List<Vector3> values, bool useRandomColor)
    {
        DisplayCurve(values, useRandomColor: true, Color.white);
    }

    public static void DisplayCurve(List<Vector3> values, Color color)
    {
        DisplayCurve(values, useRandomColor: false, color);
    }

    public static void DisplayCurve(List<Vector3> values, bool useRandomColor, Color color)
    {
        //Draw lines
        Random.InitState(0);

        for (int i = 1; i < values.Count; i++)
        {
            if (!useRandomColor)
            {
                Gizmos.color = color;
            }
            else
            {
                Gizmos.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            }

            Gizmos.DrawLine(values[i - 1], values[i]);
        }


        //Draw points
        Gizmos.color = Color.black;

        for (int i = 0; i < values.Count; i++)
        {
            Gizmos.DrawWireSphere(values[i], 0.05f);
        }
    }



    //Display handle points
    public static void DisplayHandle(Vector3 handlePos, Vector3 curvePos)
    {
        Gizmos.color = Color.white;

        Gizmos.DrawLine(handlePos, curvePos);

        Gizmos.DrawWireSphere(handlePos, 0.2f);
    }



    //Display rays
    public static void DisplayDirections(List<Vector3> startPos, List<Vector3> rayDir, float rayLength, Color color)
    {
        Gizmos.color = color;
    
        for (int i = 0; i < startPos.Count; i++)
        {
            Gizmos.DrawRay(startPos[i], rayDir[i] * rayLength);
        }
    }



    //Display orientations
    public static void DisplayOrientations(List<InterpolationTransform> orientations, float rayLength)
    {
        foreach (InterpolationTransform orientation in orientations)
        {
            Gizmos.color = Color.blue;

            Gizmos.DrawRay(orientation.position.ToVector3(), orientation.Forward.ToVector3() * rayLength);

            Gizmos.color = Color.red;

            Gizmos.DrawRay(orientation.position.ToVector3(), orientation.Right.ToVector3() * rayLength);

            Gizmos.color = Color.green;

            Gizmos.DrawRay(orientation.position.ToVector3(), orientation.Up.ToVector3() * rayLength);
        }
    }
}
