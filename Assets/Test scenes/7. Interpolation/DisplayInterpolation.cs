using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;

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

    public static void DisplayCurve(List<Vector3> points, bool useRandomColor, Color color, bool drawPoints)
    {
        //Draw lines
        Random.InitState(0);

        for (int i = 1; i < points.Count; i++)
        {
            Gizmos.color = !useRandomColor ? color: new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

            Gizmos.DrawLine(points[i - 1], points[i]);
        }


        //Draw each position with a circle
        if (drawPoints)
        {
            DisplayPoints(points);
        }
    }
    


    //Display points
    public static void DisplayPoints(List<Vector3> points)
    {
        Gizmos.color = Color.black;

        for (int i = 0; i < points.Count; i++)
        {
            Gizmos.DrawWireSphere(points[i], 0.05f);
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
    public static void DisplayExtrudedMesh(List<InterpolationTransform> transforms, MeshProfile profile)
    {
        Mesh mesh = ExtrudeMeshAlongCurve.GenerateMesh(transforms, profile, 0.25f);

        if (mesh == null)
        {
            return;
        }

        //Gizmos.DrawMesh(mesh);
        Gizmos.DrawWireMesh(mesh);
        //TestAlgorithmsHelpMethods.DisplayMesh(mesh, Color.green);
        //TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors(mesh, 0);

        //Graphics.DrawMeshNow(mesh, Vector3.zero, Quaternion.identity);
    }



    //Display a MeshProfile at a certain InterpolationTransform
    public static void DisplayMeshProfile(MeshProfile profile, InterpolationTransform transform, float profileScale)
    {
        //Display the points

        //Convert all vertices from 2d to 3d in global space
        //List<MyVector3> positions_3d = profile.vertices.Select(s =>
        //    testTrans.LocalToWorld(new MyVector3(s.point.x, s.point.y, 0f) * profileScale)
        //).ToList();

        List<Vector3> positions_3d = new List<Vector3>();

        foreach (Vertex v in profile.vertices)
        {
            MyVector2 localPos2d = v.point;

            MyVector3 localPos = new MyVector3(localPos2d.x, localPos2d.y, 0f);

            MyVector3 pos = transform.LocalToWorld_Pos(localPos * profileScale);

            positions_3d.Add(pos.ToVector3());
        }

        DisplayPoints(positions_3d);


        //Display how the points are connected with lines
        Gizmos.color = Color.white;

        for (int i = 0; i < profile.lineIndices.Length; i++)
        {
            Vector3 pos_1 = positions_3d[profile.lineIndices[i].x];
            Vector3 pos_2 = positions_3d[profile.lineIndices[i].y];

            Gizmos.DrawLine(pos_1, pos_2);
        }


        //Display normals at each point
        Gizmos.color = Color.blue;

        //Convert all normals from 2d to 3d in global space
        List<Vector3> normals_3d = new List<Vector3>();

        foreach (Vertex v in profile.vertices)
        {
            MyVector2 normal2d = v.normal;

            MyVector3 normal = new MyVector3(normal2d.x, normal2d.y, 0f);

            MyVector3 worldNormal = transform.LocalToWorld_Dir(normal);

            normals_3d.Add(worldNormal.ToVector3());
        }

        DisplayDirections(positions_3d, normals_3d, 0.5f, Color.magenta);
    }
}
