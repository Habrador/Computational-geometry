using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PoissonDiscSampling))]
public class PoissonDiscSamplingEditor : Editor
{
    private PoissonDiscSampling poissonDiscSampling;



    private void OnEnable()
    {
        poissonDiscSampling = target as PoissonDiscSampling;

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
            poissonDiscSampling.RunAlgorithm();
        }

        if (GUILayout.Button("Run algorithm"))
        {
            poissonDiscSampling.RunAlgorithm();

            EditorUtility.SetDirty(target);
        }
    }
}
