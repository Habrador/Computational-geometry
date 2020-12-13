using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSphere : MonoBehaviour
{
    public float turnSpeed = 0.5f;



    void Start()
	{
        
	}

    
    void Update()
    {
        transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime);
    }
}
