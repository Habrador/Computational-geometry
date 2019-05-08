using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TriangulatePointsController))]
public class TriangulatePointsEditor : Editor
{
    private TriangulatePointsController triangulatePoints;



    private void OnEnable()
    {
        triangulatePoints = target as TriangulatePointsController;

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
    }



    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        //Update when changing value in inspector
        if (base.DrawDefaultInspector())
        {
            triangulatePoints.TriangulateThePoints();

            EditorUtility.SetDirty(target);
        }
        
        if (GUILayout.Button("Triangulate points"))
        {
            triangulatePoints.TriangulateThePoints();

            EditorUtility.SetDirty(target);
        }
    }
}
