using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class MeshSimplification
    {
        //Merge edges to simplify a mesh
        //Based on reports by Garland and Heckbert
        //Uses quadric error metrics (QEM)
        //Normalizer is only needed for debugging
        public static MyMesh SimplifyByMergingEdges(MyMesh originalMesh, Normalizer3 normalizer = null)
        {
            MyMesh simplifiedMesh = null;

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

            //Convert to half-edge data structure (takes 0.01 seconds for the bunny)
            //timer.Start();
            HalfEdgeData3 meshData = new HalfEdgeData3(originalMesh);
            //timer.Stop();

            //Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to generate the basic half edge data structure");

            //timer.Reset();

            //timer.Start();
            meshData.ConnectAllEdgesFast();
            //timer.Stop();

            //Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to connect all opposite edges");

            //Measure which way is the best to connect all edges





            return simplifiedMesh;
        }
    }
}
