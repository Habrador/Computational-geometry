using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Help class to sort edges
    public struct QEM_Edge
    {
        public MyVector3 v1;
        public MyVector3 v2;

        //Optimal contraction target
        public MyVector3 v;

        //The Quadric Error Metric at this target
        public float qem;
        
        public QEM_Edge(MyVector3 v1, MyVector3 v2, Matrix4x4 Q1, Matrix4x4 Q2)
        {
            this.v1 = v1;
            this.v2 = v2;

            //Compute the optimal contraction target v for the pair (v1, v2)
            //This is the position to which we move v1 and v2 after merging the edge
            //Assume for simplicity that the contraction target v = (v1 + v2) * 0.5f
            //Add the other versions in the future!

            this.v = (v1 + v2) * 0.5f;

            //Compute the Quadric Error Metric at this point v
            //qem = v^T * (Q1 + Q2) * v

            Matrix4x4 Q = Q1.Add(Q2);

            float x = v.x;
            float y = v.y;
            float z = v.z;

            //v^T * Q * v
            //Verify that this is true (was found at bottom in research paper)
            float qemCalculations = 0f;
            qemCalculations += (1f * Q[0, 0] * x * x);
            qemCalculations += (2f * Q[0, 1] * x * y);
            qemCalculations += (2f * Q[0, 2] * x * z);
            qemCalculations += (2f * Q[0, 3] * x);
            qemCalculations += (1f * Q[1, 1] * y * y);
            qemCalculations += (2f * Q[1, 2] * y * z);
            qemCalculations += (2f * Q[1, 3] * y);
            qemCalculations += (1f * Q[2, 2] * z * z);
            qemCalculations += (2f * Q[2, 3] * z);
            qemCalculations += (1f * Q[3, 3]);

            this.qem = qemCalculations;
        }
    }
}
