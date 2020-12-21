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
        //- Sometimes at the end of a simplification process, the QEM is NaN because the normal of the triangle has length 0 because two vertices are at the same position. This has maybe to do with "mesh inversion." The reports says that you should compare the normal of each neighboring face before and after the contraction. If the normal flips, undo the contraction or penalize it. The temp solution to solve this problem is to set the matrix to zero matrix
        //- The algorithm can also join vertices that are within ||v1 - v2|| < distance, so test to add that. It should merge the hole in the bunny
        //- Maybe there's a faster (and simpler) way by using unique edges instead of double the calculations for an edge going in the opposite direction?
        //- A major bottleneck is finding edges going to a specific vertex. Maybe we could use a lookup table?



        //Merge edges to simplify a mesh
        //Based on reports by Garland and Heckbert, "Surface simplification using quadric error metrics"
        //Is called: "Iterative pair contraction with the Quadric Error Metric (QEM)"
        //- normalizeTriangles is if we want to multiply the QEM with the area of the triangle, which might sometimes give a better result 
        //- normalizer is only needed for debugging
        public static MyMesh SimplifyByMergingEdges(MyMesh originalMesh, int edgesToContract, bool normalizeTriangles = false, Normalizer3 normalizer = null)
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

            //
            // Convert the mesh to half-edge data structure
            //

            //timer.Start();
            HalfEdgeData3 meshData = new HalfEdgeData3(originalMesh);
            //timer.Stop();

            //timer.Start();
            meshData.ConnectAllEdgesFast();
            //timer.Stop();



            //
            // Compute the Q matrices for all the initial vertices
            //

            //Put the result in a lookupm dictionary
            //In the half-edge data structure we have more than one vertex per vertex position if multiple edges are connected to that vertex
            //But we only need to calculate a q matrix for each vertex position
            //This assumes we have no floating point precision issues, which is why the half-edge data stucture should reference positions in a list
            Dictionary<MyVector3, Matrix4x4> qMatrices = new Dictionary<MyVector3, Matrix4x4>();

            HashSet<HalfEdgeVertex3> vertices = meshData.verts;

            //timer.Start();

            //The input to this method is a MyMesh which includes a list of all individual vertices (some might be doubles if we have hard edges)
            //Maybe we can use it? But then we still would have to find the corresponding half-edge vertices...
            //0.142 seconds for the bunny (0.012 for dictionary lookup, 0.024 to calculate the Q matrices, 0.087 to find edges going to vertex)
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
                Matrix4x4 Q = CalculateQMatrix(edgesPointingToThisVertex, normalizeTriangles);
                //timer.Stop();

                qMatrices.Add(v.position, Q);
            }

            //timer.Stop();

         
           
            //
            // Select all valid pairs that can be contracted
            //

            List<HalfEdge3> validPairs = new List<HalfEdge3>(meshData.edges);



            //
            // Compute the cost of contraction for each pair
            //

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



            //
            // Sort all pairs, with the minimum cost pair at the top
            //



            //
            // Start contracting edges
            //

            //For each edge we want to remove
            for (int i = 0; i < edgesToContract; i++)
            {
                //Check that we can simplify the mesh
                //The smallest mesh we can have is a tetrahedron with 4 faces
                if (meshData.faces.Count <= 4)
                {
                    Debug.Log($"Cant contract more than {i} edges");
                
                    break;
                }


                //
                // Remove the pair (v1,v2) of the least cost and contract the pair         
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
                //timer.Stop();
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


                //timer.Start();

                //Get the half-edge we want to contract 
                HalfEdge3 edgeToContract = smallestErrorEdge.halfEdge;

                //Need to save this so we can remove the old Q matrices from the pos-matrix lookup table
                //We may move the edgeToContract when contracting it for optimization purposes???
                Edge3 contractedEdgeEndpoints = new Edge3(edgeToContract.prevEdge.v.position, edgeToContract.v.position);

                //Contract edge
                HashSet<HalfEdge3> edgesPointingToNewVertex = meshData.ContractTriangleHalfEdge(edgeToContract, smallestErrorEdge.mergePosition, timer);

                //timer.Stop();



                //
                // Remove all QEM_edges that belonged to the faces we contracted
                //
                
                //timer.Start();

                //This edge doesnt exist anymore, so remove it from the lookup
                halfEdge_QEM_Lookup.Remove(edgeToContract);

                //Remove the two edges that were a part of the triangle of the edge we contracted               
                RemoveHalfEdgeFromQEMEdges(edgeToContract.nextEdge, QEM_edges, halfEdge_QEM_Lookup);
                RemoveHalfEdgeFromQEMEdges(edgeToContract.nextEdge.nextEdge, QEM_edges, halfEdge_QEM_Lookup);

                //Remove the three edges belonging to the triangle on the opposite side of the edge we contracted
                //If there was an opposite side...
                if (edgeToContract.oppositeEdge != null)
                {
                    HalfEdge3 oppositeEdge = edgeToContract.oppositeEdge;

                    RemoveHalfEdgeFromQEMEdges(oppositeEdge, QEM_edges, halfEdge_QEM_Lookup);
                    RemoveHalfEdgeFromQEMEdges(oppositeEdge.nextEdge, QEM_edges, halfEdge_QEM_Lookup);
                    RemoveHalfEdgeFromQEMEdges(oppositeEdge.nextEdge.nextEdge, QEM_edges, halfEdge_QEM_Lookup);
                }
                //timer.Stop();


                //Remove the edges start and end vertices from the vertex - Q matrix lookup table
                qMatrices.Remove(contractedEdgeEndpoints.p1);
                qMatrices.Remove(contractedEdgeEndpoints.p2);
                //timer.Stop();



                //
                // Update all QEM_edges that is now connected with the new contracted vertex because their errors have changed
                //

                //The contracted position has a new Q matrix
                Matrix4x4 QNew = CalculateQMatrix(edgesPointingToNewVertex, normalizeTriangles);

                //Add the Q matrix to the vertex - Q matrix lookup table
                qMatrices.Add(smallestErrorEdge.mergePosition, QNew);


                //Update the error of the QEM_edges of the edges that pointed to and from one of the two old Q matrices
                //Those edges are the same edges that points to the new vertex and goes from the new vertex
                //timer.Start();
                foreach (HalfEdge3 edgeToV in edgesPointingToNewVertex)
                {
                    //The edge going from the new vertex is the next edge of the edge going to the vertex
                    HalfEdge3 edgeFromV = edgeToV.nextEdge;

                    //To
                    QEM_Edge QEM_edgeToV = halfEdge_QEM_Lookup[edgeToV];

                    Edge3 edgeToV_endPoints = QEM_edgeToV.GetEdgeEndPoints();

                    Matrix4x4 Q1_edgeToV = qMatrices[edgeToV_endPoints.p1];
                    Matrix4x4 Q2_edgeToV = QNew;

                    QEM_edgeToV.UpdateEdge(edgeToV, Q1_edgeToV, Q2_edgeToV);

                    //From
                    QEM_Edge QEM_edgeFromV = halfEdge_QEM_Lookup[edgeFromV];

                    Edge3 edgeFromV_endPoints = QEM_edgeFromV.GetEdgeEndPoints();

                    Matrix4x4 Q1_edgeFromV = QNew;
                    Matrix4x4 Q2_edgeFromV = qMatrices[edgeFromV_endPoints.p2];

                    QEM_edgeFromV.UpdateEdge(edgeFromV, Q1_edgeFromV, Q2_edgeFromV);
                }
                //timer.Stop();
            }


            //Timers: 1.231 to generate the bunny (2400 edge contractions)
            //Init:
            // - 0.1 to convert to half-edge data structure
            // - 0.14 to calculate a Q matrix for each unique vertex
            //Loop (total time):
            // - 0.50 to find smallest QEM error
            // - 0.25 to merge the edges (the bottleneck is where we have to find all edges pointing to a vertex)
            // - 0.02 to remove the data that was destroyed when we contracted an edge
            // - 0.13 to update QEM edges
            Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds");


            //From half-edge to mesh
            MyMesh simplifiedMesh = meshData.ConvertToMyMesh("Simplified mesh", shareVertices: true);


            return simplifiedMesh;
        }



        //Remove a single QEM error edge given the half-edge belonging to that QEM error edge 
        private static void RemoveHalfEdgeFromQEMEdges(HalfEdge3 e, HashSet<QEM_Edge> QEM_edges, Dictionary<HalfEdge3, QEM_Edge> halfEdge_QEM_Lookup)
        {
            QEM_Edge QEM_edge_ToRemove = halfEdge_QEM_Lookup[e];

            QEM_edges.Remove(QEM_edge_ToRemove);

            halfEdge_QEM_Lookup.Remove(e);
        }



        //Calculate the Q matrix for a vertex if we know all edges pointing to the vertex
        private static Matrix4x4 CalculateQMatrix(HashSet<HalfEdge3> edgesPointingToVertex, bool normalizeTriangles)
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
                MyVector3 normal = _Geometry.CalculateTriangleNormal(p1, p2, p3);

                if (float.IsNaN(normal.x) || float.IsNaN(normal.y) || float.IsNaN(normal.z))
                {
                    Debug.LogWarning("This normal has length 0");
                    //TestAlgorithmsHelpMethods.DisplayMyVector3(p1);
                    //TestAlgorithmsHelpMethods.DisplayMyVector3(p2);
                    //TestAlgorithmsHelpMethods.DisplayMyVector3(p3);

                    //Temp solution if the normal is zero
                    Q = Q.Add(Matrix4x4.zero);

                    continue;
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
                //This is only needed if the triangles have very different size
                if (normalizeTriangles)
                {
                    float triangleArea = _Geometry.CalculateTriangleArea(p1, p2, p3);

                    Kp = Kp.Multiply(triangleArea);
                }

                //Q is the sum of all Kp around the vertex
                Q = Q.Add(Kp);
            }


            return Q;
        }
    }
}
