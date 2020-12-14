using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;



//Rotate a camera around origo while moving it up and down to show entire mesh
public class SpinAroundCamera : MonoBehaviour
{
    private float camRotationSpeed = 3f;

    private float maxCamMoveVerticalSpeed = 1f;

    //How far will the camera move above/below zero
    private float maxMinHeight = 2f;

    private bool shouldMoveUp = true;



    void LateUpdate()
    {
        //Rotate around
        transform.Translate(Vector3.right * Time.deltaTime * camRotationSpeed);


        //Move up/down
        //camVerticalSpeed should be smaller the closer we are to a turning point to make it smoother
        float camHeight = Mathf.Abs(transform.position.y);

        float camVerticalSpeed = _Interpolation.Sinerp(maxCamMoveVerticalSpeed, maxCamMoveVerticalSpeed * 0.9f, camHeight / maxMinHeight);

        //Debug.Log(camSpeed);

        if (shouldMoveUp)
        {
            transform.Translate(Vector3.up * Time.deltaTime * camVerticalSpeed, Space.World);
        }
        else
        {
            transform.Translate(-Vector3.up * Time.deltaTime * camVerticalSpeed, Space.World);
        }

        //Change move up/down direction
        if (transform.position.y > maxMinHeight && shouldMoveUp)
        {
            shouldMoveUp = false;
        }
        if (transform.position.y < -maxMinHeight && !shouldMoveUp)
        {
            shouldMoveUp = true;
        }


        //Look at center, which will rotate the camera
        transform.LookAt(Vector3.zero);
    }
}
