using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VoronoiSphereController))]
public class VoronoiSphereControllerEditor : Editor
{
    private VoronoiSphereController triangulatePoints;



    private void OnEnable()
    {
        triangulatePoints = target as VoronoiSphereController;

        //Hide the main GOs move/rot/scale handle
        Tools.hidden = true;
    }



    private void OnDisable()
    {
        //Un-hide the main GOs move/ rot / scale handle
        Tools.hidden = false;
    }



    private void OnSceneGUI()
    {
        //So you we cant click on anything else in the scene
        HandleUtility.AddDefaultControl(0);

        /*
        //Move the points
        List<Transform> transforms = triangulatePoints.GetAllPoints();

        if (transforms != null)
        {
            for (int i = 0; i < transforms.Count; i++)
            {
                Vector3 newPos = MovePoint(transforms[i].position);

                transforms[i].position = newPos;
            }
        }
        */
    }


    /*
    private Vector3 MovePoint(Vector3 pos)
    {
        if (Tools.current == Tool.Move)
        {
            //Check if we have moved the point
            EditorGUI.BeginChangeCheck();

            //Get the new position and display it
            pos = Handles.PositionHandle(pos, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                //Save the new value
                EditorUtility.SetDirty(target);

                triangulatePoints.GenerateTriangulation();
            }
        }

        return pos;
    }
    */


    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        //Update when changing value in inspector
        if (base.DrawDefaultInspector())
        {
            triangulatePoints.Generate();

            EditorUtility.SetDirty(target);
        }

        if (GUILayout.Button("Triangulate points"))
        {
            triangulatePoints.Generate();

            //Will not work because the classes in the triangle is not set to serializable 
            EditorUtility.SetDirty(target);
        }
    }
}
