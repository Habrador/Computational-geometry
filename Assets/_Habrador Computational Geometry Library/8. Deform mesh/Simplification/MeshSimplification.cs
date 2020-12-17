using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class MeshSimplification
    {
        //Merge edges to simplify a mesh
        //Based on reports by Garland and Heckbert
        //Is called: "Iterative pair contraction with the Quadric Error Metric (QEM)"
        //Normalizer is only needed for debugging
        public static MyMesh SimplifyByMergingEdges(MyMesh originalMesh, Normalizer3 normalizer = null)
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

            //Convert to half-edge data structure (takes 0.01 seconds for the bunny)
            //timer.Start();
            HalfEdgeData3 meshData = new HalfEdgeData3(originalMesh);
            //timer.Stop();

            //Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to generate the basic half edge data structure");

            //timer.Reset();

            //timer.Start();
            //Takes 0.1 seconds for the bunny
            meshData.ConnectAllEdgesFast();
            //timer.Stop();

            //Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to connect all opposite edges");


            //The simplification algorithm starts here


            //Step 1. Compute the Q matrices for all the initial vertices

            //Put the result in a dictionary
            //In the half-edge data structure we have more than one vertex per vertex position if multiple edges are connected to that vertex
            //But we only need to calculate a q matrix for each vertex position
            //This assumes we have no floating point precision issues, which is why the half-edge data stucture should reference positions in a list
            Dictionary<MyVector3, float> qMatrices = new Dictionary<MyVector3, float>();

            HashSet<HalfEdgeVertex3> vertices = meshData.verts;

            foreach (HalfEdgeVertex3 v in vertices)
            {
                //Have we already calculated a Q matrix for this vertex?
                if (qMatrices.ContainsKey(v.position))
                {
                    continue;
                }
            }



            //Step 2. Select all valid pairs

            //Step 3. Compute the optimal contraction target for each valid pair (v1, v2). The error of this target vertex becomes the cost of contracting that pair

            //Step 4. Sort all pairs, with the minimum cost pair at the top

            //Step 5. Iteratively remove the pair (v1, v2) of the least cost, contract the pair, and update the costs of all valid pairs involving just v1?



            MyMesh simplifiedMesh = null;

            return simplifiedMesh;
        }
    }
}
