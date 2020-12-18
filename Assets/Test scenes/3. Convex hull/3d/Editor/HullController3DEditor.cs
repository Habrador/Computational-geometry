using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HullController3D))]
public class HullController3DEditor : Editor
{
    private HullController3D hullGenerator;



    private void OnEnable()
    {
        hullGenerator = target as HullController3D;

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
            hullGenerator.GenerateHull();

            EditorUtility.SetDirty(target);
        }

        if (GUILayout.Button("Generate hull"))
        {
            hullGenerator.GenerateHull();

            //Will not work because the classes in the triangle is not set to serializable 
            EditorUtility.SetDirty(target);
        }
    }
}
