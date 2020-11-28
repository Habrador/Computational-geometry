using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalsController : MonoBehaviour
{
    public Transform decalTrans;

    public Camera thisCamera;



    void Start()
	{
        
	}

    

    void Update()
    {
        PlaceDecal();
    }



    //Fire a ray from the mouse pos to place the decals in the scene  
    private void PlaceDecal()
    {
        if (thisCamera == null)
        {
            Debug.Log("You need a camera");

            return;
        }
        if (decalTrans == null)
        {
            Debug.Log("You need a decal");

            return;
        }


        RaycastHit hit;
        
        Ray ray = thisCamera.ScreenPointToRay(Input.mousePosition);

        //Make sure the decal doesn't have a collider or the ray will hit it!

        if (Physics.Raycast(ray, out hit))
        {
            //Place the decal at the position where we hit
            decalTrans.position = hit.point;

            //The forward of a built-in quad is inverted
            decalTrans.forward = -hit.normal;

            //Debug.Log(hit.point);

            //The object that was hit
            //Transform objectHit = hit.transform;

            DeformDecal(decalTrans);
        }
    }


    //Based on:
    // - Game Programming Gems 2 (p. 395): Applying decals to arbitrary surfaces 
    private void DeformDecal(Transform decalTrans)
    {
        Vector3 P = decalTrans.position;
        //If we are using the built-in quad, the orientation becomes:
        Vector3 N = -decalTrans.forward;
        Vector3 T = decalTrans.right;
        Vector3 B = decalTrans.up;


    }
}
