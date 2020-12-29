using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;
using UnityEngine.UI;



public class VisualizerController3D : MonoBehaviour
{
    public MeshFilter meshInput;
    public MeshFilter displayMeshHere;
    public MeshFilter displayOtherMeshHere;

    public GameObject pointObj;
    //Should be bigger so we can display it above the non-active point
    public GameObject pointActiveObj;

    //GUI
    public TMPro.TextMeshProUGUI displayStuffUI;

    public SpinAroundAndMoveToDirectionCamera cameraScript;

    private HashSet<GameObject> allPoints = new HashSet<GameObject>();

    public Normalizer3 normalizer;

    public HashSet<HalfEdgeFace3> meshData;



    void Awake()
	{
        pointObj.SetActive(false);
        pointActiveObj.SetActive(false); 

        displayMeshHere.mesh = null;
        displayOtherMeshHere.mesh = null;

        //StartConvexHull();

        //StartMeshSimplification();
    }



    private void StartMeshSimplification()
    {
        Mesh meshToSimplify = meshInput.sharedMesh;

        meshInput.transform.gameObject.SetActive(false);


        //
        // Change data structure and normalize
        //

        //Mesh -> MyMesh
        MyMesh myMeshToSimplify = new MyMesh(meshToSimplify);

        //From local to global space
        myMeshToSimplify.vertices = myMeshToSimplify.vertices.Select(x => meshInput.transform.TransformPoint(x.ToVector3()).ToMyVector3()).ToList();

        //Normalize to 0-1
        //this.normalizer = new Normalizer3(myMeshToSimplify.vertices);

        //We only need to normalize the vertices
        //myMeshToSimplify.vertices = normalizer.Normalize(myMeshToSimplify.vertices);

        HalfEdgeData3 myMeshToSimplify_HalfEdge = new HalfEdgeData3(myMeshToSimplify, HalfEdgeData3.ConnectOppositeEdges.Fast);


        //Start
        VisualizeMergeEdgesQEM visualizeThisAlgorithm = GetComponent<VisualizeMergeEdgesQEM>();

        visualizeThisAlgorithm.StartVisualizer(myMeshToSimplify_HalfEdge, maxEdgesToContract: 2450, maxError: Mathf.Infinity);
    }



    private void StartConvexHull()
    {
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

        //return; 

        //Display the mesh with lines
        Gizmos.color = Color.black;

        foreach (HalfEdgeFace3 f in meshData)
        {
            //Vector3 p1 = normalizer.UnNormalize(f.edge.v.position).ToVector3();
            //Vector3 p2 = normalizer.UnNormalize(f.edge.nextEdge.v.position).ToVector3();
            //Vector3 p3 = normalizer.UnNormalize(f.edge.prevEdge.v.position).ToVector3();

            Vector3 p1 = f.edge.v.position.ToVector3();
            Vector3 p2 = f.edge.nextEdge.v.position.ToVector3();
            Vector3 p3 = f.edge.prevEdge.v.position.ToVector3();


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
        MyMesh myMesh = HalfEdgeData3.ConvertToMyMesh("Main visualization mesh", meshDataUnNormalized, MyMesh.MeshStyle.HardEdges);

        Mesh mesh = myMesh.ConvertToUnityMesh(generateNormals: true);

        mf.mesh = mesh;

        //Debug.Log(mesh.triangles.Length);
    }

    public void DisplayMeshMain(HashSet<HalfEdgeFace3> meshData)
    {
        //UnNormalize (will modify the original data so we have to normalize when we are finished)
        //HashSet<HalfEdgeFace3> meshDataUnNormalized = normalizer.UnNormalize(meshData);

        HashSet<HalfEdgeFace3> meshDataUnNormalized = meshData;

        this.meshData = meshDataUnNormalized;

        DisplayMesh(meshDataUnNormalized, displayMeshHere);

        //Normalize again
        //meshData = normalizer.Normalize(meshDataUnNormalized);
    }

    public void DisplayMeshMain(HalfEdgeData2 meshData, Normalizer2 normalizer)
    {
        //UnNormalize and to 3d
        HalfEdgeData3 meshDataUnNormalized_3d = new HalfEdgeData3();


        //We dont want to modify the original data
        //HalfEdgeData2 meshDataUnNormalized = normalizer.UnNormalize(meshData);

        HashSet<HalfEdgeFace2> faces_2d = meshData.faces;

        foreach (HalfEdgeFace2 f in faces_2d)
        {
            MyVector2 p1 = f.edge.v.position;
            MyVector2 p2 = f.edge.nextEdge.v.position;
            MyVector2 p3 = f.edge.nextEdge.nextEdge.v.position;

            p1 = normalizer.UnNormalize(p1);
            p2 = normalizer.UnNormalize(p2);
            p3 = normalizer.UnNormalize(p3);

            meshDataUnNormalized_3d.AddTriangle(p1.ToMyVector3_Yis3D(), p2.ToMyVector3_Yis3D(), p3.ToMyVector3_Yis3D());
        }

        this.meshData = meshDataUnNormalized_3d.faces;

        DisplayMesh(meshDataUnNormalized_3d.faces, displayMeshHere);
        
        //Normalize again
        //meshData = normalizer.Normalize(meshDataUnNormalized);
    }

    public void DisplayMeshOther(HashSet<HalfEdgeFace3> meshData)
    {
        //UnNormalize (will modify the original data so we have to normalize when we are finished)
        HashSet<HalfEdgeFace3> meshDataUnNormalized = normalizer.UnNormalize(meshData);

        DisplayMesh(meshDataUnNormalized, displayOtherMeshHere);

        //Normalize again
        meshData = normalizer.Normalize(meshDataUnNormalized);
    }

    public void DisplayMeshOtherUnNormalized(HashSet<HalfEdgeFace3> meshDataUnNormalized)
    {
        displayOtherMeshHere.gameObject.SetActive(true);

        DisplayMesh(meshDataUnNormalized, displayOtherMeshHere);
    }

    public void HideMeshOther()
    {
        displayOtherMeshHere.gameObject.SetActive(false);
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
