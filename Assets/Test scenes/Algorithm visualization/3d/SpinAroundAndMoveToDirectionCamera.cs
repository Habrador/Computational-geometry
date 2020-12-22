using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;



//Rotate a camera around origo while moving to a certain part of the mesh we want to look at
public class SpinAroundAndMoveToDirectionCamera : MonoBehaviour
{
    private float camRotationSpeed = 2f;

    private float maxCamMoveVerticalSpeed = 1f;

    //How far will the camera move above/below zero
    private float maxMinHeight = 2f;

    //private bool shouldMoveUp = true;

    private Vector3 wantedDirection = Vector3.zero;

    private float wantedHeight = 0f;
    

    void LateUpdate()
    {
        //transform.position = Vector3.zero;

        //Rotate around
        //transform.Translate(Vector3.right * Time.deltaTime * camRotationSpeed);
        //This is resulting in less stutter
        //transform.RotateAround(Vector3.zero, Vector3.up, -Time.deltaTime * camRotationSpeed);
        //transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0f, 90f, 0f), Time.deltaTime * camRotationSpeed);

        //Debug.Log(wantedDirection);

        if (wantedDirection.Equals(Vector3.zero))
        {
            transform.RotateAround(Vector3.zero, Vector3.up, -Time.deltaTime * camRotationSpeed * 10f);

            //Debug.Log("hello");
        }
        else
        {
            //We are basically comparing vectors, so to make sure the vectors are the same we have to cheat
            transform.LookAt(new Vector3(0f, transform.position.y, 0f));

            //Quaternion currentCameraRotation = transform.rotation;
            Quaternion currentCameraRotation = Quaternion.LookRotation(transform.forward, Vector3.up);

            Quaternion futureCameraRotation = Quaternion.LookRotation(wantedDirection, Vector3.up);

            float angleDelta = Quaternion.Angle(currentCameraRotation, futureCameraRotation);

            //Debug.Log(transform.forward + " " + wantedDirection);

            transform.RotateAround(Vector3.zero, Vector3.up, angleDelta * camRotationSpeed * Time.deltaTime);
        }
        

        //Move up/down
        //camVerticalSpeed should be smaller the closer we are to a turning point to make it smoother
        float camHeight = Mathf.Abs(transform.position.y);

        //float camVerticalSpeed = _Interpolation.Sinerp(maxCamMoveVerticalSpeed, maxCamMoveVerticalSpeed * 0.99f, camHeight / wantedHeight);

        //Debug.Log(camSpeed);

        float camVerticalSpeed = maxCamMoveVerticalSpeed;

        if (wantedHeight > transform.position.y)
        {
            transform.Translate(Vector3.up * Time.deltaTime * camVerticalSpeed, Space.World);

            if (wantedHeight < transform.position.y)
            {
                transform.position = new Vector3(transform.position.x, wantedHeight, transform.position.z);
            }
        }
        else if (wantedHeight < transform.position.y)
        {
            transform.Translate(-Vector3.up * Time.deltaTime * camVerticalSpeed, Space.World);

            if (wantedHeight > transform.position.y)
            {
                transform.position = new Vector3(transform.position.x, wantedHeight, transform.position.z);
            }
        }
        

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



    public void SetWantedDirection(Vector3 dir)
    {
        this.wantedDirection = dir;
    }
    public void SetWantedHeight(float y)
    {
        this.wantedHeight = y;
    }
}
