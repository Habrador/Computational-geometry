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
        List<Transform> transformsToCut = GetChildTransformsFromParent(meshesToCutParentTrans);

        Plane3 cutPlane = new Plane3(cutPlaneTrans.position.ToMyVector3(), cutPlaneTrans.up.ToMyVector3());
        
        foreach (Transform child in transformsToCut)
        {
            Mesh meshToCut = child.GetComponent<MeshFilter>().mesh;
        
            if (meshToCut == null)
            {
                Debug.Log("There's no mesh to cut");

                continue;
            }

            //Should return null (if we couldn't cut the mesh because the mesh didn't intersect with the plane)
            List<Mesh> cutMeshes = CutMeshWithPlane.CutMesh(meshToCut, cutPlane);

            if (cutMeshes == null)
            {
                Debug.Log("This mesh couldn't be cut");

                continue;
            }

            Debug.Log(cutMeshes.Count);


            //Create new game objects with the new meshes
            foreach (Mesh newMesh in cutMeshes)
            {
                GameObject newObj = Instantiate(child.gameObject);

                newObj.transform.position = child.position;
                newObj.transform.rotation = child.rotation;

                newObj.transform.parent = meshesToCutParentTrans;

                newObj.GetComponent<MeshFilter>().mesh = newMesh;
            }
        }
    }



    public List<Transform> GetChildTransformsFromParent(Transform parentTrans)
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
            childrenTransforms.Add(parentTrans.GetChild(i));
        }

        return childrenTransforms;
    }
}
