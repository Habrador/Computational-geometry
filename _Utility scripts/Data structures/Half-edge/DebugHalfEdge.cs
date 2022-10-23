using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Methods for display half-edge data
    public static class DebugHalfEdge
    {
        //Transform is to transform a point to global space, which can be null 
        public static void DisplayEdgesWithNoOpposite(HashSet<HalfEdge3> edges, Transform trans, Color color, float timer = 20f)
        {
            foreach (HalfEdge3 e in edges)
            {
                if (e.oppositeEdge != null)
                {
                    continue;
                }

                Vector3 p1 = e.v.position.ToVector3();
                Vector3 p2 = e.prevEdge.v.position.ToVector3();

                //Local to global space
                if (trans != null)
                {
                    p1 = trans.TransformPoint(p1);
                    p2 = trans.TransformPoint(p2);
                }

                Debug.DrawLine(p1, p2, color, timer);
            }
        }
    }
}
