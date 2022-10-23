using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Help class to sort edges
    //Will also calculate at what position this edge should merge
    public class QEM_Edge : IHeapItem<QEM_Edge>
    {
        public HalfEdge3 halfEdge;

        //Optimal contraction target position
        public MyVector3 mergePosition;

        //The Quadric Error Metric at the merge position
        public float qem;



        public QEM_Edge(HalfEdge3 halfEdge, Matrix4x4 Q1, Matrix4x4 Q2)
        {
            UpdateEdge(halfEdge, Q1, Q2);
        }



        public void UpdateEdge(HalfEdge3 halfEdge, Matrix4x4 Q1, Matrix4x4 Q2)
        {
            this.halfEdge = halfEdge;

            //Compute the optimal contraction target v for the pair (v1, v2) and the qem at this position
            CalculateMergePositionANDqem(this.halfEdge, Q1, Q2);
        }



        //Compute the optimal contraction target v for the pair (v1, v2)
        private void CalculateMergePositionANDqem(HalfEdge3 e, Matrix4x4 Q1, Matrix4x4 Q2)
        {
            //The basic idea is that we want to find a position on the edge that minimizes the error

            //Assume for simplicity that the contraction target are v = (v1 + v2) * 0.5f, or v = v1, or v = v2
            //Add the other versions in the future!
            //Adding just 3 alternatives instead of using just the average position, adds 0.2 seconds for the bunny (2400 iterations)
            //But the result is slightly better
            MyVector3 p1 = e.prevEdge.v.position;
            MyVector3 p2 = e.v.position;

            MyVector3 v1 = p1;
            MyVector3 v2 = p2;
            MyVector3 v3 = (p1 + p2) * 0.5f;

            //Compute the Quadric Error Metric at each point v
            float qem1 = CalculateQEM(v1, Q1, Q2);
            float qem2 = CalculateQEM(v2, Q1, Q2);
            float qem3 = CalculateQEM(v3, Q1, Q2);
           
            //Find which vertex minimized the qem
            if (qem1 < qem2 && qem1 < qem3)
            {
                this.mergePosition = v1;

                this.qem = qem1;
            }
            else if (qem2 < qem1 && qem2 < qem3)
            {
                this.mergePosition = v2;

                this.qem = qem2;
            }
            else
            {
                this.mergePosition = v3;

                this.qem = qem3;
            }
        }



        //Compute the Quadric Error Metric at point v
        //The error for v1, v2 is given by v^T * (Q1 + Q2) * v 
        //where v = [v.x, v.y, v.z, 1] is the optimal contraction target position
        private float CalculateQEM(MyVector3 v, Matrix4x4 Q1, Matrix4x4 Q2)
        {
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

            float qem = qemCalculations;

            return qem;
        }



        //Get the positions where this edge starts and end
        public Edge3 GetEdgeEndPoints()
        {
            MyVector3 p1 = this.halfEdge.prevEdge.v.position;
            MyVector3 p2 = this.halfEdge.v.position;

            Edge3 e = new Edge3(p1, p2);

            return e;
        }



        //
        // For the heap
        //

        private int heapIndex;

        //To be able to sort the items in the heap
        public int HeapIndex
        {
            get { return heapIndex; }
            set { this.heapIndex = value; }
        }

        //To be able to sort items in the heap
        //https://docs.microsoft.com/en-us/previous-versions/windows/silverlight/dotnet-windows-silverlight/74z9b11e(v=vs.95)?redirectedfrom=MSDN
        public int CompareTo(QEM_Edge other)
        {
            //Compare
            int compare = qem.CompareTo(other.qem);

            //We want to return 1 if the item has a higher priority than then item we are comparing it with has
            //meaning that qem < other.qem
            //But CompareTo is return 1 of qem > other.qem, so we have to return the negative
            return -compare;
        }
    }
}
