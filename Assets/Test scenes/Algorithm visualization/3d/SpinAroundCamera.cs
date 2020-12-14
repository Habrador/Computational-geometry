using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;



//Rotate a camera around origo while moving it up and down to show entire mesh
public class SpinAroundCamera : MonoBehaviour
{
    private float camRotationSpeed = 2f;

    private float maxCamMoveVerticalSpeed = 1f;

    //How far will the camera move above/below zero
    private float maxMinHeight = 2f;

    private bool shouldMoveUp = true;

    public Vector3 wantedDirection = Vector3.zero;

    Quaternion currentCameraRotation;

    void LateUpdate()
    {
        //transform.position = Vector3.zero;

        //Rotate around
        //transform.Translate(Vector3.right * Time.deltaTime * camRotationSpeed);
        //This is resulting in less stutter
        //transform.RotateAround(Vector3.zero, Vector3.up, -Time.deltaTime * camRotationSpeed);
        //transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0f, 90f, 0f), Time.deltaTime * camRotationSpeed);

        if (wantedDirection == Vector3.zero)
        {
            transform.RotateAround(Vector3.zero, Vector3.up, -Time.deltaTime * camRotationSpeed);
        }
        else
        {
            Quaternion currentCameraRotation = transform.rotation;

            Quaternion futureCameraRotation = Quaternion.LookRotation(wantedDirection, Vector3.up);

            float angleDelta = Quaternion.Angle(currentCameraRotation, futureCameraRotation);

            Debug.Log(angleDelta);

            transform.RotateAround(Vector3.zero, Vector3.up, angleDelta * camRotationSpeed * Time.deltaTime);
        }
        

        //Move up/down
        //camVerticalSpeed should be smaller the closer we are to a turning point to make it smoother
        //float camHeight = Mathf.Abs(transform.position.y);

        //float camVerticalSpeed = _Interpolation.Sinerp(maxCamMoveVerticalSpeed, maxCamMoveVerticalSpeed * 0.99f, camHeight / maxMinHeight);

        //Debug.Log(camSpeed);

        //if (shouldMoveUp)
        //{
        //    transform.Translate(Vector3.up * Time.deltaTime * camVerticalSpeed, Space.World);
        //}
        //else
        //{
        //    transform.Translate(-Vector3.up * Time.deltaTime * camVerticalSpeed, Space.World);
        //}

        ////Change move up/down direction
        //if (transform.position.y > maxMinHeight && shouldMoveUp)
        //{
        //    shouldMoveUp = false;
        //}
        //if (transform.position.y < -maxMinHeight && !shouldMoveUp)
        //{
        //    shouldMoveUp = true;
        //}

        //transform.Translate(Vector3.forward * 3f);


        //Look at center, which will rotate the camera
        transform.LookAt(Vector3.zero);
    }
}
