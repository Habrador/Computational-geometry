using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MarchingSquaresController))]
public class MarchingSquaresControllerEditor : Editor
{
    private MarchingSquaresController triangulatePoints;



    private void OnEnable()
    {
        triangulatePoints = target as MarchingSquaresController;

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

        //Autoupdate if we have changed a value in the editor, such as mapSize
        if (DrawDefaultInspector())
        {
            triangulatePoints.GenerateMap();

            EditorUtility.SetDirty(target);
        }

        if (GUILayout.Button("Triangulate points"))
        {
            triangulatePoints.GenerateMap();

            EditorUtility.SetDirty(target);
        }
    }
}
