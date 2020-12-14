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
    //Should be bigger so we can display it above the non-active point
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
    public void DisplayMesh(HashSet<HalfEdgeFace3> meshData, MeshFilter mf)
    {
        //UnNormalize (will modify the original data so we have to normalize when we are finished)
        HashSet<HalfEdgeFace3> meshDataUnNormalized = normalizer.UnNormalize(meshData);

        //Generate a mesh
        Mesh mesh = HalfEdgeData3.ConvertToUnityMesh("Main visualization mesh", meshDataUnNormalized);

        mf.mesh = mesh;

        //Normalize again
        meshData = normalizer.Normalize(meshDataUnNormalized);
    }

    public void DisplayMeshMain(HashSet<HalfEdgeFace3> meshData)
    {
        DisplayMesh(meshData, displayMeshHere);
    }

    public void DisplayMeshOther(HashSet<HalfEdgeFace3> meshData)
    {
        DisplayMesh(meshData, displayOtherMeshHere);
    }


    //Display active point
    public void DisplayActivePoint(MyVector3 pos)
    {
        Vector3 pos_unNormalized = normalizer.UnNormalize(pos).ToVector3();
    
        pointActiveObj.SetActive(true);

        pointActiveObj.transform.position = pos_unNormalized;
    }
}
