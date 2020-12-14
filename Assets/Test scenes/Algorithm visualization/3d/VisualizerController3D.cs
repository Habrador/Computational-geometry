using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;



public class VisualizerController3D : MonoBehaviour
{
    public MeshFilter displayMeshHere;

    public GameObject poinObj;


    private HashSet<GameObject> allPoints = new HashSet<GameObject>();



    void Start()
	{
        poinObj.SetActive(false);
    
        //Get random points in 3d space
        HashSet<Vector3> points_Unity = TestAlgorithmsHelpMethods.GenerateRandomPoints3D(seed: 0, halfCubeSize: 1f, numberOfPoints: 10);

        //Generate points we can display
        foreach (Vector3 p in points_Unity)
        {
            GameObject newPoint = Instantiate(poinObj, p, Quaternion.identity);

            newPoint.SetActive(true);

            allPoints.Add(newPoint);
        }


        displayMeshHere.mesh = null;
    }
}
