using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Habrador_Computational_Geometry
{
    public static class MeshSimplificationQEM
    {
        //TODO:
        //- Calculate the optimal contraction target v and not just the average between two vertices
        //- Calculate weighted Q matrix by multiplying each Kp matrix with the area of the triangle



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
                HashSet<HalfEdge3> edgesPointingToThisVertex = v.GetEdgesPointingToVertex(meshData);

                Matrix4x4 Q = CalculateQMatrix(edgesPointingToThisVertex);

                qMatrices.Add(v.position, Q);
            }



            //Step 2. Select all valid pairs
            List<HalfEdge3> validPairs = new List<HalfEdge3>(meshData.edges);


            //Step 3. Compute the optimal contraction target v for each valid pair (v1, v2). The error of this target vertex becomes the cost of contracting that pair
            //Assume for simplicity that the contraction target v = (v1 + v2) * 0.5f
            //The error for v1, v2 is given by v^T * (Q1 + Q2) * v 
            //where v = [v.x, v.y, v.z, 1]
            HashSet<QEM_Edge> QEM_edges = new HashSet<QEM_Edge>();

            //HashSet to keep track of unique vertices
            //In the half-edge data structure may have an edge going in the opposite direction with the same 
            //error, so it's a waste of time to sort twice as many edges as needed
            //HashSet<Edge3> uniqueEdges = new HashSet<Edge3>();

            foreach (HalfEdge3 edge in validPairs)
            {            
                MyVector3 p1 = edge.prevEdge.v.position;
                MyVector3 p2 = edge.v.position;

                //Does this edge already exist but in the opposite direction?
                //if (!uniqueEdges.Contains(new Edge3(p2, p1)))
                //{
                //    //It can't exist in this direction because only one edge can go in this direction
                //    uniqueEdges.Add(new Edge3(p1, p2));
                //}

                Matrix4x4 Q1 = qMatrices[p1];
                Matrix4x4 Q2 = qMatrices[p2];

                QEM_Edge qemEdge = new QEM_Edge(edge, Q1, Q2);

                QEM_edges.Add(qemEdge);
            }


            //Step 4. Sort all pairs, with the minimum cost pair at the top
            //Find the QEM edge with the smallest error
            QEM_Edge smallestErrorEdge = null;

            float smallestError = Mathf.Infinity;

            foreach (QEM_Edge QEM_edge in QEM_edges)
            {
                if (QEM_edge.qem < smallestError)
                {
                    smallestError = QEM_edge.qem;

                    smallestErrorEdge = QEM_edge;
                }
            }

            QEM_edges.Remove(smallestErrorEdge);


            //Step 5. Iteratively remove the pair (v1,v2) of the least cost, contract the pair, and update the costs of all valid pairs           

            //Get the half-edge we want to contract 
            HalfEdge3 edgeToContract = smallestErrorEdge.halfEdge;

            //Need to save this so we can remove all old edges that pointed to these vertices
            Edge3 removedEdgeEndpoints = new Edge3(edgeToContract.prevEdge.v.position, edgeToContract.v.position);

            //Contract edge
            meshData.ContractTriangleHalfEdge(edgeToContract, smallestErrorEdge.mergePosition);

            


            //Update all QEM_edges that have changed

            //We need to remove the two edges that were a part of the triangle of the edge we contracted
            //This could become faster if we had a dictionary that saved the half-edge QEM_edge relationship
            //Or maybe we don't need to generate a QEM_edge for all edges, we just need the best one...
            RemoveHalfEdgeFromQEMEdges(edgeToContract.nextEdge, QEM_edges);
            RemoveHalfEdgeFromQEMEdges(edgeToContract.nextEdge.nextEdge, QEM_edges);

            //We need to remove three edges belonging to the triangle on the opposite side of the edge we contracted
            //If there was an opposite side!
            if (edgeToContract.oppositeEdge != null)
            {
                HalfEdge3 oppositeEdge = edgeToContract.oppositeEdge;

                RemoveHalfEdgeFromQEMEdges(oppositeEdge, QEM_edges);
                RemoveHalfEdgeFromQEMEdges(oppositeEdge.nextEdge, QEM_edges);
                RemoveHalfEdgeFromQEMEdges(oppositeEdge.nextEdge.nextEdge, QEM_edges);
            }


            //We have a dictionary with all Q matrices for each vertex position
            
            //Remove the old positions from the Q matrices dictionary
            qMatrices.Remove(removedEdgeEndpoints.p1);
            qMatrices.Remove(removedEdgeEndpoints.p2);

            //Calculate a new Q matrix for the contracted position 

            //To get the edges going to a position we need a vertex from the half-edge data structure
            HalfEdgeVertex3 contractedVertex = null;

            HashSet<HalfEdgeVertex3> verts = meshData.verts;

            foreach (HalfEdgeVertex3 v in verts)
            {
                if (v.position.Equals(smallestErrorEdge.mergePosition))
                {
                    contractedVertex = v;

                    break;
                }
            }

            HashSet<HalfEdge3> edgesPointingToVertex = contractedVertex.GetEdgesPointingToVertex(meshData);

            Matrix4x4 QNew = CalculateQMatrix(edgesPointingToVertex);

            qMatrices.Add(smallestErrorEdge.mergePosition, QNew);


            //Update the QEM_edges of the edges that pointed to and from one of the two old Q matrices
            //Those edges are the same edges that points to the new vertex and goes from the new vertex
            HashSet<HalfEdge3> edgesThatNeedToBeUpdated = new HashSet<HalfEdge3>(edgesPointingToVertex);
            //The edges going from the new vertex is the next edge of the edges going to the vertex
            foreach (HalfEdge3 e in edgesPointingToVertex)
            {
                edgesThatNeedToBeUpdated.Add(e.nextEdge);
            }

            foreach (QEM_Edge this_QEM_edge in QEM_edges)
            {
                if (edgesThatNeedToBeUpdated.Contains(this_QEM_edge.halfEdge))
                {
                    Edge3 endPoints = this_QEM_edge.GetEdgeEndPoints();

                    Matrix4x4 Q1 = qMatrices[endPoints.p1];
                    Matrix4x4 Q2 = qMatrices[endPoints.p2];

                    this_QEM_edge.UpdateEdge(this_QEM_edge.halfEdge, Q1, Q2);

                    edgesThatNeedToBeUpdated.Remove(this_QEM_edge.halfEdge);
                }

                if (edgesThatNeedToBeUpdated.Count == 0)
                {
                    break;
                }
            }


            MyMesh simplifiedMesh = null;

            return simplifiedMesh;
        }







        private static void RemoveHalfEdgeFromQEMEdges(HalfEdge3 e, HashSet<QEM_Edge> QEM_edges)
        {
            foreach (QEM_Edge QEM_edge in QEM_edges)
            {
                if (QEM_edge.halfEdge.Equals(e))
                {
                    QEM_edges.Remove(QEM_edge);

                    //Debug.Log("Removed surplus qem edge");

                    break;
                }
            }
        }



        private static Matrix4x4 CalculateQMatrix(HashSet<HalfEdge3> edgesPointingToVertex)
        {
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

                //To calculate the Kp matrix, we have to define the plane on the form: 
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
                    new Vector4(a * a, a * b, a * c, a * d),
                    new Vector4(a * b, b * b, b * c, b * d),
                    new Vector4(a * c, b * c, c * c, c * d),
                    new Vector4(a * d, b * d, c * d, d * d)
                    );

                //You can multiply this Kp with the area of the triangle to get a weighted-Kp which may improve the result

                //Q is the sum of all Kp around the vertex
                Q = Q.Add(Kp);
            }


            return Q;
        }
    }
}
