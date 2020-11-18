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
        if (profile == null)
        {
            Debug.Log("You need to assign a mesh profile");

            return;
        }

        if (transforms == null || transforms.Count <= 1)
        {
            Debug.Log("You need more transforms");

            return;
        }

        float profileScale = 0.25f;

        //Test that the profile is correct
        //InterpolationTransform testTrans = transforms[1];

        //DisplayMeshProfile(profile, testTrans, profileScale);

        //Vertices
        List<Vector3> vertices = new List<Vector3>();

        //Normals
        List<Vector3> normals = new List<Vector3>(); 

        for (int step = 0; step < transforms.Count; step++)
        {
            InterpolationTransform thisTransform = transforms[step];
        
            for (int i = 0; i < profile.vertices.Length; i++)
            {
                MyVector2 localPos2d = profile.vertices[i].point;

                MyVector3 localPos = new MyVector3(localPos2d.x, localPos2d.y, 0f);

                MyVector3 pos = thisTransform.LocalToWorld_Pos(localPos * profileScale);

                vertices.Add(pos.ToVector3());


                //Normals
                MyVector2 localNormal2d = profile.vertices[i].normal;

                MyVector3 localNormal = new MyVector3(localNormal2d.x, localNormal2d.y, 0f);

                MyVector3 normal = thisTransform.LocalToWorld_Dir(localNormal);

                normals.Add(normal.ToVector3());
            }
        }

        //Triangles
        List<int> triangles = new List<int>();

        //We connect the first transform with the next transform, ignoring the last transform because it doesnt have a next
        for (int step = 0; step < transforms.Count - 1; step++)
        {
            //The index where this profile starts in the list of all vertices in the entire mesh
            int profileIndexThis = step * profile.vertices.Length;
            //The index where the next profile starts
            int profileIndexNext = (step + 1) * profile.vertices.Length;

            //Each line has 2 points 
            for (int line = 0; line < profile.lineIndices.Length; line+=2)
            {
                int lineIndexA = profile.lineIndices[line];
                int lineIndexB = profile.lineIndices[line + 1];

                //Now we can identify the vertex we need in the list of all vertices in the entire mesh
                //The current profile
                int thisA = profileIndexThis + lineIndexA;
                int thisB = profileIndexThis + lineIndexB;
                //The next profile
                int nextA = profileIndexNext + lineIndexA;
                int nextB = profileIndexNext + lineIndexB;

                //Build two triangles
                triangles.Add(thisA);
                triangles.Add(nextA);
                triangles.Add(nextB);

                triangles.Add(thisB);
                triangles.Add(thisA);
                triangles.Add(nextB);
            }
        }

        Mesh mesh = new Mesh();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();

        //mesh.RecalculateNormals();

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

        for (int i = 0; i < profile.lineIndices.Length; i += 2)
        {
            Vector3 pos_1 = positions_3d[profile.lineIndices[i]];
            Vector3 pos_2 = positions_3d[profile.lineIndices[i + 1]];

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
