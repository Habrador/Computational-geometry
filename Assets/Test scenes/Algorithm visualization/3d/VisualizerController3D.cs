using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;



public class VisualizerController3D : MonoBehaviour
{
    public MeshFilter displayMeshHere;
    public MeshFilter displayOtherMeshHere;

    public GameObject pointObj;

    public GameObject pointActiveObj;


    private HashSet<GameObject> allPoints = new HashSet<GameObject>();

    private Normalizer3 normalizer;



    void Start()
	{
        pointObj.SetActive(false);
        pointActiveObj.SetActive(false);

        //Get random points in 3d space
        HashSet<Vector3> points_Unity = TestAlgorithmsHelpMethods.GenerateRandomPoints3D(seed: 0, halfCubeSize: 1f, numberOfPoints: 10);

        //Generate points we can display
        foreach (Vector3 p in points_Unity)
        {
            GameObject newPoint = Instantiate(pointObj, p, Quaternion.identity);

            newPoint.SetActive(true);

            allPoints.Add(newPoint);
        }


        displayMeshHere.mesh = null;
        displayOtherMeshHere.mesh = null;


        //Standardize the data
        //To MyVector3
        HashSet<MyVector3> points = new HashSet<MyVector3>(points_Unity.Select(x => x.ToMyVector3()));

        //Normalize
        normalizer = new Normalizer3(new List<MyVector3>(points));

        HashSet<MyVector3> points_normalized = normalizer.Normalize(points);


        VisualizeIterativeConvexHull visualizeThisAlgorithm = GetComponent<VisualizeIterativeConvexHull>();

        visualizeThisAlgorithm.StartVisualizer(points_normalized);

    }



    //Display a mesh, which is called from the coroutine when a mesh has changed
    public void DisplayMesh(HalfEdgeData3 meshData)
    {
        //UnNormalize (will modify the original data so we have to normalize when we are finished)
        HalfEdgeData3 meshDataUnNormalized = normalizer.UnNormalize(meshData);

        //Generate a mesh
        Mesh mesh = meshData.ConvertToUnityMesh("Main visualization mesh", shareVertices: false, generateNormals: false);

        displayMeshHere.mesh = mesh;

        //Normalize again
        meshData = normalizer.Normalize(meshDataUnNormalized);
    }
}
