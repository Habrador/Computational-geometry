using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //This will use Unity's Quaternion (which uses Vector3), but input and output will be MyVector3
    //This so we don't have to write our custom Quaternion class
    public struct MyQuaternion
    {
        private Quaternion quaternion;
    
        public MyQuaternion(MyVector3 forward)
        {
            this.quaternion = Quaternion.LookRotation(forward.ToVector3());
        }

        public MyQuaternion(MyVector3 forward, MyVector3 up)
        {
            this.quaternion = Quaternion.LookRotation(forward.ToVector3(), up.ToVector3());
        }

        public MyQuaternion(Quaternion quaternion)
        {
            this.quaternion = quaternion;
        }



        //
        // Quaternion operations
        //

        //Rotate a quaternion some degrees around some axis
        public static MyQuaternion RotateQuaternion(MyQuaternion oldQuaternion, float angleInDegrees, MyVector3 rotationAxis)
        {        
            Quaternion rotationQuaternion = Quaternion.AngleAxis(angleInDegrees, rotationAxis.ToVector3());

            //To rotate a quaternion you just multiply it with the rotation quaternion
            Quaternion newQuaternion = oldQuaternion.quaternion * rotationQuaternion;

            MyQuaternion myNewQuaternion = new MyQuaternion(newQuaternion);

            return myNewQuaternion;
        }



        //
        // Get directions from orientation
        //

        //Forward
        public MyVector3 Forward
        {
            get
            {
                //Multiply the orientation with a direction vector to rotate the direction
                Vector3 forwardDir = quaternion * Vector3.forward;

                return forwardDir.ToMyVector3();
            }
        }

        //Right
        public MyVector3 Right
        {
            get
            {
                //Multiply the orientation with a direction vector to rotate the direction
                Vector3 rightDir = quaternion * Vector3.right;

                return rightDir.ToMyVector3();
            }
        }

        //Up
        public MyVector3 Up
        {
            get
            {
                //Multiply the orientation with a direction vector to rotate the direction
                Vector3 upDir = quaternion * Vector3.up;

                return upDir.ToMyVector3();
            }
        }
    }
}
