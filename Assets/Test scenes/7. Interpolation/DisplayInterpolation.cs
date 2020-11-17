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
        DisplayCurve(values, useRandomColor: true, Color.white, true);
    }

    public static void DisplayCurve(List<Vector3> values, Color color)
    {
        DisplayCurve(values, useRandomColor: false, color, true);
    }

    //Display curve with high resolution, which is useful if we want to compare the actual curve with a curve split into steps
    //Unity has built-in Handles.DrawBezier but doesn't exist for other curve types
    public static void DisplayCurve(_Curve curve, Color color)
    {
        int steps = 200;

        List<MyVector3> positionsOnCurve = InterpolationHelpMethods.SplitCurve(curve, steps, tEnd: 1f);

        List<Vector3> positionsOnCurveStandardized = positionsOnCurve.ConvertAll(x => x.ToVector3()); 

        DisplayCurve(positionsOnCurveStandardized, false, color, false);
    }

    public static void DisplayCurve(List<Vector3> values, bool useRandomColor, Color color, bool drawPoints)
    {
        //Draw lines
        Random.InitState(0);

        for (int i = 1; i < values.Count; i++)
        {
            Gizmos.color = !useRandomColor ? color: new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

            Gizmos.DrawLine(values[i - 1], values[i]);
        }


        //Draw each position with a circle
        if (drawPoints)
        {
            Gizmos.color = Color.black;

            for (int i = 0; i < values.Count; i++)
            {
                Gizmos.DrawWireSphere(values[i], 0.05f);
            }
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
        //Same colors as Unitys coordinate system
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



    //Display mesh extruded along a curve
    public static void DisplayMesh()
    {

    }
}
