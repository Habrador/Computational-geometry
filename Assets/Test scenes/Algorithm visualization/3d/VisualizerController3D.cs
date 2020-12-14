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

    public SpinAroundCamera cameraScript;

    private HashSet<GameObject> allPoints = new HashSet<GameObject>();

    public Normalizer3 normalizer;

    public HashSet<HalfEdgeFace3> meshData;


    void Start()
	{
        pointObj.SetActive(false);
        pointActiveObj.SetActive(false);

        //Get random points in 3d space
        //HashSet<Vector3> points_Unity = TestAlgorithmsHelpMethods.GenerateRandomPoints3D(seed: 0, halfCubeSize: 1f, numberOfPoints: 50);

        //Get random points on a sphere
        HashSet<Vector3> points_Unity = TestAlgorithmsHelpMethods.GenerateRandomPointsOnSphere(seed: 0, radius: 1f, numberOfPoints: 50);

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



    private void OnDrawGizmos()
    {
        if (meshData == null)
        {
            return;
        }

        //Display the mesh with lines
        Gizmos.color = Color.black;

        foreach (HalfEdgeFace3 f in meshData)
        {
            Vector3 p1 = normalizer.UnNormalize(f.edge.v.position).ToVector3();
            Vector3 p2 = normalizer.UnNormalize(f.edge.nextEdge.v.position).ToVector3();
            Vector3 p3 = normalizer.UnNormalize(f.edge.prevEdge.v.position).ToVector3();

            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p1);
        }

        ////Debug.Log("hello");
    }



    //Display a mesh, which is called from the coroutine when a mesh has changed
    public void DisplayMesh(HashSet<HalfEdgeFace3> meshDataUnNormalized, MeshFilter mf)
    {
        //Generate a mesh
        Mesh mesh = HalfEdgeData3.ConvertToUnityMesh("Main visualization mesh", meshDataUnNormalized);

        mf.mesh = mesh;
    }

    public void DisplayMeshMain(HashSet<HalfEdgeFace3> meshData)
    {
        //UnNormalize (will modify the original data so we have to normalize when we are finished)
        HashSet<HalfEdgeFace3> meshDataUnNormalized = normalizer.UnNormalize(meshData);

        this.meshData = meshDataUnNormalized;

        DisplayMesh(meshDataUnNormalized, displayMeshHere);

        //Normalize again
        meshData = normalizer.Normalize(meshDataUnNormalized);
    }

    public void DisplayMeshOther(HashSet<HalfEdgeFace3> meshData)
    {
        //UnNormalize (will modify the original data so we have to normalize when we are finished)
        HashSet<HalfEdgeFace3> meshDataUnNormalized = normalizer.UnNormalize(meshData);

        DisplayMesh(meshDataUnNormalized, displayOtherMeshHere);

        //Normalize again
        meshData = normalizer.Normalize(meshDataUnNormalized);
    }


    //Display active point
    public void DisplayActivePoint(MyVector3 pos)
    {
        Vector3 pos_unNormalized = normalizer.UnNormalize(pos).ToVector3();
    
        pointActiveObj.SetActive(true);

        pointActiveObj.transform.position = pos_unNormalized;
    }

    //Hide active point
    public void HideActivePoint()
    {
        pointActiveObj.SetActive(false);
    }

    //Hide visible point
    public void HideVisiblePoint(MyVector3 pos)
    {
        Vector3 pos_unNormalized = normalizer.UnNormalize(pos).ToVector3();

        foreach (GameObject go in allPoints)
        {
            if (!go.activeInHierarchy)
            {
                continue;
            }

            if (Mathf.Abs(Vector3.Magnitude(pos_unNormalized - go.transform.position)) < 0.0001f)
            {
                go.SetActive(false);

                break;
            }
        }
    }

    //Hide all visible points that are in some collection
    public void HideAllVisiblePoints(HashSet<HalfEdgeVertex3> verts)
    {
        foreach (HalfEdgeVertex3 v in verts)
        {
            HideVisiblePoint(v.position);
        }
    }
}
