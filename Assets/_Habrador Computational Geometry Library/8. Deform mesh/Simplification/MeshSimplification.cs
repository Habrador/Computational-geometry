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
            Dictionary<MyVector3, Matrix4x4> qMatrices = new Dictionary<MyVector3, Matrix4x4>();

            HashSet<HalfEdgeVertex3> vertices = meshData.verts;

            foreach (HalfEdgeVertex3 v in vertices)
            {
                //Have we already calculated a Q matrix for this vertex?
                //Remember that we have multiple vertices at the same position in the half-edge data structure
                if (qMatrices.ContainsKey(v.position))
                {
                    continue;
                }

                //Calculate the Q matrix for this vertex

                //Find all triangles meeting at this vertex
                HashSet<HalfEdge3> edgesPointingToVertex = v.GetEdgesPointingToVertex();

                Matrix4x4 Q = Matrix4x4.zero;

                //Calculate a Kp matrix for each triangle attached to this vertex and add it to the sumOfKp 
                foreach (HalfEdge3 e in edgesPointingToVertex)
                {
                    //To calculate the Kp matric we need all vertices
                    MyVector3 p1 = e.v.position;
                    MyVector3 p2 = e.nextEdge.v.position;
                    MyVector3 p3 = e.nextEdge.nextEdge.v.position;

                    //...and a normal
                    MyVector3 normal = _Geometry.CalculateNormal(p1, p2, p3);

                    //To calculate the Kp matrix, we have to define the plane by the equation 
                    //ax + by + cz + d = 0 where a^2 + b^2 + c^2 = 1
                    //a, b, c are given by the normal: 
                    float a = normal.x;
                    float b = normal.y;
                    float c = normal.z;

                    //To calculate d we just use one of the points on the plane (in the triangle)
                    //d = -(ax + by + cz)
                    float d = -(a * p1.x + b * p1.y + c * p1.z);

                    //This built-in matrix is initialized by giving it columns
                    Matrix4x4 Kp = new Matrix4x4(
                        new Vector4(a*a, a*b, a*c, a*d),
                        new Vector4(a*b, b*b, b*c, b*d),
                        new Vector4(a*c, b*c, c*c, c*d),
                        new Vector4(a*d, b*d, c*d, d*d)
                        );

                    //So Q is the sum of all Kp around the vertex
                    Q = Q.Add(Kp);
                }

                qMatrices.Add(v.position, Q);
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
