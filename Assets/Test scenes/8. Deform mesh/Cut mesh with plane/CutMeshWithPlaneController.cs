using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

public class CutMeshWithPlaneController : MonoBehaviour
{
    public Transform cutPlaneTrans;

    //Place all new meshes below this go, so we can cut them again
    public Transform meshesToCutParentTrans;



    void Start()
    {
        //Step 1 is to convert all meshes to the half-edge data structure and cache the result, which will improve performance
        List<Transform> transformsToCut = GetChildTransformsWithMeshAttached(meshesToCutParentTrans);

        foreach (Transform childTransToCut in transformsToCut)
        {
            //Only cut active gameobjects
            if (!childTransToCut.gameObject.activeInHierarchy)
            {
                continue;
            }

            //We know a mesh (and thus a mesh filter) is attached so we don't need to check that
            Mesh meshToCut = childTransToCut.GetComponent<MeshFilter>().sharedMesh;

            //Convert from unity mesh to our mesh
            MyMesh myMeshToCut = new MyMesh(meshToCut); 

            //Convert to half-edge data structure
            HalfEdgeData3 halfEdgeMeshData = new HalfEdgeData3(myMeshToCut, HalfEdgeData3.ConnectOppositeEdges.Fast);

            //Don't convert to global space, it's faster to convert the plane to local space
            CutMesh cutMesh = childTransToCut.gameObject.AddComponent<CutMesh>();

            cutMesh.halfEdge3DataStructure = halfEdgeMeshData;
        }
    }

    

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CutMesh();
        }
    }



    private void CutMesh()
    {
        List<Transform> transformsToCut = GetChildTransformsWithMeshAttached(meshesToCutParentTrans);

        //Plane3 cutPlane = new Plane3(cutPlaneTrans.position.ToMyVector3(), cutPlaneTrans.up.ToMyVector3());
        OrientedPlane3 cutPlane = new OrientedPlane3(cutPlaneTrans);
        
        foreach (Transform childTransToCut in transformsToCut)
        {
            //Only cut active gameobjects
            if (!childTransToCut.gameObject.activeInHierarchy)
            {
                continue;
            }

            //Cut the mesh
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

            timer.Start();

            //Should return null (if we couldn't cut the mesh because the mesh didn't intersect with the plane)
            HalfEdgeData3 halfEdgeMeshData = childTransToCut.GetComponent<CutMesh>().halfEdge3DataStructure;

            List<HalfEdgeData3> cutMeshes = CutMeshWithPlane.CutMesh(childTransToCut, halfEdgeMeshData, cutPlane, fillHoles: true);

            timer.Stop();

            Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to cut the mesh");


            if (cutMeshes == null)
            {
                Debug.Log("This mesh couldn't be cut");

                continue;
            }

            Debug.Log($"Number of new meshes after cut: {cutMeshes.Count}");


            //Make sure the new object has the correct transform because it might have been a child to other gameobjects when we found it
            //and these old parent objects might have had some scale, etc
            //This might change in the future but then we would have to locate the correct parent-child, which might be messy?
            Transform oldChildParent = childTransToCut.parent;
            //The transform of the mesh we want to cut will now be in global space, so its scale etc might have changed
            childTransToCut.parent = null;

            timer.Restart();

            //Create new game objects with the new meshes
            foreach (HalfEdgeData3 newHalfEdgeMesh in cutMeshes)
            {
                GameObject newObj = Instantiate(childTransToCut.gameObject);

                newObj.transform.parent = meshesToCutParentTrans;

                //Cache the half-edge data in case we want to cut the mesh again
                childTransToCut.GetComponent<CutMesh>().halfEdge3DataStructure = newHalfEdgeMesh;

                //Convert from half-edge to unity mesh
                MyMesh myMesh = newHalfEdgeMesh.ConvertToMyMesh("Cutted mesh", MyMesh.MeshStyle.HardAndSoftEdges);

                Mesh unityMesh = myMesh.ConvertToUnityMesh(generateNormals: false);

                newObj.GetComponent<MeshFilter>().mesh = unityMesh;
            }

            timer.Stop();

            Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to generate the unity meshes");


            //Hide the original one if we cut the mesh (we didn't reach thid far down if we didnt cut the mesh)
            childTransToCut.gameObject.SetActive(false);

            //And make sure the original mesh is in local space (if it had a parent)
            //We earlier transformed it to global space
            childTransToCut.parent = oldChildParent;
        }
    }



    public List<Transform> GetChildTransformsWithMeshAttached(Transform parentTrans)
    {
        if (parentTrans == null)
        {
            Debug.Log("No parent so cant get children");

            return null;
        }

        //Is not including the parent
        int children = parentTrans.childCount;

        List<Transform> childrenTransforms = new List<Transform>();

        for (int i = 0; i < children; i++)
        {
            Transform thisChild = parentTrans.GetChild(i);

            //Make sure we add the transform to which a mesh filter is attached
            if (thisChild.GetComponent<MeshFilter>() == null)
            {
                //We can't use this directly because we might in the future need the parent to a child with a mesh because it might have some important scripts attached to it
                //And the parent is always thisChild
                MeshFilter[] meshFiltersInChildren = thisChild.GetComponentsInChildren<MeshFilter>(includeInactive: false);

                if (meshFiltersInChildren.Length == 0)
                {
                    Debug.Log("This child has no mesh attached to it");

                    continue;
                }

                foreach (MeshFilter mf in meshFiltersInChildren)
                {
                    childrenTransforms.Add(mf.transform);
                }
            }
            else
            {
                childrenTransforms.Add(thisChild);
            }
        }

        return childrenTransforms;
    }
}
