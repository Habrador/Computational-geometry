using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Methods for display half-edge data
    public static class DebugHalfEdge
    {
        //Transform is to transform a point to global space, which can be null 
        public static void DisplayEdgesWithNoOpposite(HashSet<HalfEdge3> edges, Transform trans, Color color, Normalizer3 normalizer = null, float timer = 20f)
        {
            foreach (HalfEdge3 e in edges)
            {
                if (e.oppositeEdge != null)
                {
                    continue;
                }

                MyVector3 my_p1 = e.v.position;
                MyVector3 my_p2 = e.prevEdge.v.position;

                if (normalizer != null)
                {
                    my_p1 = normalizer.UnNormalize(my_p1);
                    my_p2 = normalizer.UnNormalize(my_p2);
                }

                Vector3 p1 = my_p1.ToVector3();
                Vector3 p2 = my_p2.ToVector3();

                //Local to global space
                if (trans != null)
                {
                    p1 = trans.TransformPoint(p1);
                    p2 = trans.TransformPoint(p2);
                }

                Debug.DrawLine(p1, p2, color, timer);
            }
        }



        public static void DisplayEdges(HashSet<HalfEdge3> edges, Transform trans, Color color, Normalizer3 normalizer = null, float timer = 20f)
        {
            foreach (HalfEdge3 e in edges)
            {
                MyVector3 my_p1 = e.v.position;
                MyVector3 my_p2 = e.prevEdge.v.position;

                if (normalizer != null)
                {
                    my_p1 = normalizer.UnNormalize(my_p1);
                    my_p2 = normalizer.UnNormalize(my_p2);
                }

                Vector3 p1 = my_p1.ToVector3();
                Vector3 p2 = my_p2.ToVector3();

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
