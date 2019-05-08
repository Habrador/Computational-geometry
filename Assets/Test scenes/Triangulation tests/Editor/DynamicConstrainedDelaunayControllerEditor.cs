using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DynamicConstrainedDelaunayController))]
public class DynamicConstrainedDelaunayControllerEditor : Editor
{
    private DynamicConstrainedDelaunayController triangulatePoints;



    private void OnEnable()
    {
        triangulatePoints = target as DynamicConstrainedDelaunayController;

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

        //Move the obstacle points
        List<Vector3> obstacle = triangulatePoints.obstacle;

        if (obstacle != null)
        {
            for (int i = 0; i < obstacle.Count; i++)
            {
                Vector3 newPos = MovePoint(obstacle[i]);

                obstacle[i] = newPos;
            }
        }
    }



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

                //triangulatePoints.GenererateTriangulation();
            }
        }

        return pos;
    }



    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        //Update when changing value in inspector
        //if (base.DrawDefaultInspector())
        //{
        //    triangulatePoints.GenererateInitialTriangulation();

        //    EditorUtility.SetDirty(target);
        //}

        if (GUILayout.Button("Reset"))
        {
            triangulatePoints.GenererateInitialTriangulation();

            EditorUtility.SetDirty(target);
        }

        if (GUILayout.Button("Add obstacle"))
        {
            //triangulatePoints.GenererateTriangulation();

            EditorUtility.SetDirty(target);
        }

        if (GUILayout.Button("Remove obstacle"))
        {
            //triangulatePoints.GenererateTriangulation();

            EditorUtility.SetDirty(target);
        }
    }
}
