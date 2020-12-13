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
            List<Mesh> cutMeshes = CutMeshWithPlane.CutMesh(childTransToCut, cutPlane);

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
            //The transform will now be in global space
            childTransToCut.parent = null;


            //Create new game objects with the new meshes
            foreach (Mesh newMesh in cutMeshes)
            {
                GameObject newObj = Instantiate(childTransToCut.gameObject);

                newObj.transform.parent = meshesToCutParentTrans;

                newObj.GetComponent<MeshFilter>().mesh = newMesh;
            }


            //Hide the original one
            childTransToCut.gameObject.SetActive(false);

            //And make sure the original one has the previous parent
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
