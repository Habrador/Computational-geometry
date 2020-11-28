using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

public class CutMeshWithPlaneController : MonoBehaviour
{
    public Transform cutPlane;

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
