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
        //- Sometimes at the end of a simplification process, the QEM is NaN because the normal of the triangle has length 0 because two vertices are at the same position. This has maybe to do with "mesh inversion." The reports says that you should compare the normal of each neighboring face before and after the contraction. If the normal flips, undo the contraction or penalize it 
        //- The algorithm can also join vertices that are within ||v1 - v2|| < distance, so add that!
        //- Maybe there's a faster way by using unique edges instead of double the calculations for an edge going in the opposite direction?



        //Merge edges to simplify a mesh
        //Based on reports by Garland and Heckbert, "Surface simplification using quadric error metrics"
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

            //timer.Start();

            //The input to this method is a MyMesh which includes a list of all individual vertices (some might be doubles if we have hard edges)
            //Maybe we can use it? But then we still would have to find the corresponding half-edge vertices...

            foreach (HalfEdgeVertex3 v in vertices)
            {
                //Have we already calculated a Q matrix for this vertex?
                //Remember that we have multiple vertices at the same position in the half-edge data structure
                //timer.Start();
                if (qMatrices.ContainsKey(v.position))
                {
                    continue;
                }
                //timer.Stop();

                //Calculate the Q matrix for this vertex
                //Find all edges meeting at this vertex
                //Maybe we can speed up by saving all vertices which can't be rotated around, while removing edges of those that can, which will result in fewer edges to search through when using the brute force approach?
                //timer.Start();
                HashSet<HalfEdge3> edgesPointingToThisVertex = v.GetEdgesPointingToVertex(meshData);
                //timer.Stop();

                //timer.Start();
                Matrix4x4 Q = CalculateQMatrix(edgesPointingToThisVertex);
                //timer.Stop();

                qMatrices.Add(v.position, Q);
            }

            //timer.Stop();

            //0.142 seconds for the bunny (0.012 for dictionary lookup, 0.024 to calculate the Q matrices, 0.087 to find edges going to vertex)
            //Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds");


            //Step 2. Select all valid pairs
            List<HalfEdge3> validPairs = new List<HalfEdge3>(meshData.edges);


            //Step 3. Compute the cost of contraction for each pair
            HashSet<QEM_Edge> QEM_edges = new HashSet<QEM_Edge>();

            //We need a lookup table to faster remove and update QEM_edges
            Dictionary<HalfEdge3, QEM_Edge> halfEdge_QEM_Lookup = new Dictionary<HalfEdge3, QEM_Edge>();

            foreach (HalfEdge3 halfEdge in validPairs)
            {            
                MyVector3 p1 = halfEdge.prevEdge.v.position;
                MyVector3 p2 = halfEdge.v.position;

                Matrix4x4 Q1 = qMatrices[p1];
                Matrix4x4 Q2 = qMatrices[p2];

                QEM_Edge QEM_edge = new QEM_Edge(halfEdge, Q1, Q2);

                QEM_edges.Add(QEM_edge);

                halfEdge_QEM_Lookup.Add(halfEdge, QEM_edge);
            }



            //For each edge we want to remove
            for (int i = 0; i < 2400; i++)
            {
                //Check that we can simplify the mesh
                if (meshData.faces.Count <= 4)
                {
                    break;
                }


                //
                // Step 4. Sort all pairs, with the minimum cost pair at the top
                //
                
                //Find the QEM edge with the smallest error
                //timer.Start();
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
                //timer.Stop();

                if (smallestErrorEdge == null)
                {
                    Debug.LogWarning("Cant find a smallest QEM edge");

                    Debug.Log($"Number of QEM_edges: {QEM_edges.Count}");

                    foreach (QEM_Edge QEM_edge in QEM_edges)
                    {
                        Debug.Log(QEM_edge.qem);
                    }
                
                    Debug.Log($"Faces: {meshData.faces.Count} Edges: {meshData.edges.Count} Verts: {meshData.verts.Count}");

                    break;
                }



                //
                // Step 5. Remove the pair (v1,v2) of the least cost, contract the pair, and update the costs of all valid pairs           
                //

                //Get the half-edge we want to contract 
                HalfEdge3 edgeToContract = smallestErrorEdge.halfEdge;

                //Need to save this so we can remove all old edges that pointed to these vertices
                Edge3 removedEdgeEndpoints = new Edge3(edgeToContract.prevEdge.v.position, edgeToContract.v.position);

                //Contract edge
                HashSet<HalfEdge3> edgesPointingToNewVertex = meshData.ContractTriangleHalfEdge(edgeToContract, smallestErrorEdge.mergePosition);


                //
                // Remove all QEM_edges that belonged to the faces we contracted
                //
                
                //timer.Start();

                //This edge doesnt exist anymore, so remove it from the lookup
                halfEdge_QEM_Lookup.Remove(edgeToContract);

                //We need to remove the two edges that were a part of the triangle of the edge we contracted
                //This could become faster if we had a dictionary that saved the half-edge QEM_edge relationship
                //Or maybe we don't need to generate a QEM_edge for all edges, we just need the best one...
               
                RemoveHalfEdgeFromQEMEdges(edgeToContract.nextEdge, QEM_edges, halfEdge_QEM_Lookup);
                RemoveHalfEdgeFromQEMEdges(edgeToContract.nextEdge.nextEdge, QEM_edges, halfEdge_QEM_Lookup);

                //We need to remove three edges belonging to the triangle on the opposite side of the edge we contracted
                //If there was an opposite side!
                if (edgeToContract.oppositeEdge != null)
                {
                    HalfEdge3 oppositeEdge = edgeToContract.oppositeEdge;

                    RemoveHalfEdgeFromQEMEdges(oppositeEdge, QEM_edges, halfEdge_QEM_Lookup);
                    RemoveHalfEdgeFromQEMEdges(oppositeEdge.nextEdge, QEM_edges, halfEdge_QEM_Lookup);
                    RemoveHalfEdgeFromQEMEdges(oppositeEdge.nextEdge.nextEdge, QEM_edges, halfEdge_QEM_Lookup);
                }
                //timer.Stop();


                //We have a dictionary with all Q matrices for each vertex position

                //Remove the old positions from the Q matrices dictionary
                qMatrices.Remove(removedEdgeEndpoints.p1);
                qMatrices.Remove(removedEdgeEndpoints.p2);
                //timer.Stop();


                //
                // Update all QEM_edges that is connected with the new contracted vertex
                //

                //TestAlgorithmsHelpMethods.DisplayMyVector3(contractedVertex.position);
                //TestAlgorithmsHelpMethods.DisplayMyVector3(smallestErrorEdge.mergePosition);

                Matrix4x4 QNew = CalculateQMatrix(edgesPointingToNewVertex);

                //Add the Q matrix to the vertex - Q matrix lookup table
                qMatrices.Add(smallestErrorEdge.mergePosition, QNew);


                //Update the QEM_edges of the edges that pointed to and from one of the two old Q matrices
                //Those edges are the same edges that points to the new vertex and goes from the new vertex
                timer.Start();
                foreach (HalfEdge3 edgeToV in edgesPointingToNewVertex)
                {
                    QEM_Edge QEM_edgeToV = halfEdge_QEM_Lookup[edgeToV];

                    Edge3 edgeToV_endPoints = QEM_edgeToV.GetEdgeEndPoints();

                    Matrix4x4 Q1_edgeToV = qMatrices[edgeToV_endPoints.p1];
                    Matrix4x4 Q2_edgeToV = QNew;

                    QEM_edgeToV.UpdateEdge(edgeToV, Q1_edgeToV, Q2_edgeToV);


                    //The edge going from the new vertex is the next edge of the edge going to the vertex
                    HalfEdge3 edgeFromV = edgeToV.nextEdge;

                    QEM_Edge QEM_edgeFromV = halfEdge_QEM_Lookup[edgeFromV];

                    Edge3 edgeFromV_endPoints = QEM_edgeFromV.GetEdgeEndPoints();

                    Matrix4x4 Q1_edgeFromV = QNew;
                    Matrix4x4 Q2_edgeFromV = qMatrices[edgeFromV_endPoints.p2];

                    QEM_edgeFromV.UpdateEdge(edgeFromV, Q1_edgeFromV, Q2_edgeFromV);
                }
                timer.Stop();
            }


            //Timers: 1.231 to generate the bunny (2400 edge merges)
            // - 0.01 to fonvert to half-edge data structure
            // - 0.1 to connect all opposite edges in the half-edge data structure
            // - 0.142 to calculate a Q matrix for each unique vertex
            // - 0.544 to find smallest QEM error, would be faster if we used a heap?
            // - 0.015 to remove the edge we contract from the data structure (including the edges we removed after contracting an edge)
            // - 0.128 to update QEM edges
            Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds");


            //From half-edge to mesh
            MyMesh simplifiedMesh = meshData.ConvertToMyMesh("Simplified mesh", shareVertices: true);

            return simplifiedMesh;
        }



        //Remove a single QEM error edge given the half-edge belonging to that QEM error edge
        //This would have been faster if we connected the QEM-edges like the half-edge data structure
        //The QEM edge could also be a child class to half-edge class and we would get all that for free 
        private static void RemoveHalfEdgeFromQEMEdges(HalfEdge3 e, HashSet<QEM_Edge> QEM_edges, Dictionary<HalfEdge3, QEM_Edge> halfEdge_QEM_Lookup)
        {
            QEM_Edge QEM_edge_ToRemove = halfEdge_QEM_Lookup[e];

            QEM_edges.Remove(QEM_edge_ToRemove);

            halfEdge_QEM_Lookup.Remove(e);

            //foreach (QEM_Edge QEM_edge in QEM_edges)
            //{
            //    if (QEM_edge.halfEdge.Equals(e))
            //    {
            //        QEM_edges.Remove(QEM_edge);

            //        //Debug.Log("Removed surplus qem edge");

            //        break;
            //    }
            //}
        }



        //Calculate the Q matrix for a vertex if we know all edges pointing to the vertex
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

                if (float.IsNaN(normal.x) || float.IsNaN(normal.y) || float.IsNaN(normal.z))
                {
                    Debug.LogWarning("This normal has length 0");
                    //TestAlgorithmsHelpMethods.DisplayMyVector3(p1);
                    //TestAlgorithmsHelpMethods.DisplayMyVector3(p2);
                    //TestAlgorithmsHelpMethods.DisplayMyVector3(p3);
                }

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
