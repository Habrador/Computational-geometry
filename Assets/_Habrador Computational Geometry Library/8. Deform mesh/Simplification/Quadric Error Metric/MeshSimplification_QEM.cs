using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Habrador_Computational_Geometry
{
    public static class MeshSimplification_QEM
    {
        //TODO:
        //- Calculate the optimal contraction target v and not just the average between two vertices or one of the two endpoints
        //- Sometimes at the end of a simplification process, the QEM is NaN because the normal of the triangle has length 0 because two vertices are at the same position. This has maybe to do with "mesh inversion." The reports says that you should compare the normal of each neighboring face before and after the contraction. If the normal flips, undo the contraction or penalize it. The temp solution to solve this problem is to set the matrix to zero matrix if the normal is NaN
        //- The algorithm can also join vertices that are within ||v1 - v2|| < distance, so test to add that. It should merge the hole in the bunny
        //- Maybe there's a faster (and simpler) way by using unique edges instead of double the calculations for an edge going in the opposite direction?
        //- A major bottleneck is finding edges going to a specific vertex. The problem is that if there are holes in the mesh, we can't just rotate around the vertex to find the edges - we have to search through ALL edges. In the regular mesh structure, we have a list of all vertices, so moving a vertex would be fast if we moved it in that list, so all edges should reference a vertex in a list?
        //- Is edgesToContract the correct way to stop the algorithm? Maybe it should be number of vertices in the final mesh?
        //- Visualize the error by using some color scale.
        //- Some times when we contract an edge we end up with invalid triangles, such as triangles with area 0. Are all these automatically removed if we weigh each error with triangle area? Or do we need to check that the triangulation is valid after contracting an edge?



        /// <summary>
        /// Merge edges to simplify a mesh
        /// Based on reports by Garland and Heckbert, "Surface simplification using quadric error metrics"
        /// Is called: "Iterative pair contraction with the Quadric Error Metric (QEM)"
        /// </summary>
        /// <param name="halfEdgeMeshData">Original mesh</param>
        /// <param name="maxEdgesToContract">How many edges do we want to merge (the algorithm stops if it can't merge more edges)</param>
        /// <param name="maxError">Stop merging edges if the error is bigger than the maxError, which will prevent the algorithm from changing the shape of the mesh</param>
        /// <param name="normalizeTriangles">Sometimes the quality improves if we take triangle area into account when calculating ther error</param>
        /// <param name="normalizer">Is only needed for debugging</param>
        /// <returns>The simplified mesh</returns>
        /// If you set edgesToContract to max value, then it will continue until it cant merge any more edges or the maxError is reached
        /// If you set maxError to max value, then it will continue to merge edges until it cant merge or max edgesToContract is reached 
        public static HalfEdgeData3 Simplify(HalfEdgeData3 halfEdgeMeshData, int maxEdgesToContract, float maxError, bool normalizeTriangles = false, Normalizer3 normalizer = null)
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();


            //
            // Compute the Q matrices for all the initial vertices
            //

            //Put the result in a lookup dictionary
            //This assumes we have no floating point precision issues, so vertices at the same position have to be at the same position
            Dictionary<MyVector3, Matrix4x4> qMatrices = new Dictionary<MyVector3, Matrix4x4>();

            HashSet<HalfEdgeVertex3> vertices = halfEdgeMeshData.verts;

            //timer.Start();

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

                //timer.Start();
                //Find all edges meeting at this vertex
                HashSet<HalfEdge3> edgesPointingToThisVertex = v.GetEdgesPointingToVertex(halfEdgeMeshData);
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

            List<HalfEdge3> validPairs = new List<HalfEdge3>(halfEdgeMeshData.edges);



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

            //The fastest way to keep the data sorted is to use a heap
            Heap<QEM_Edge> sorted_QEM_edges = new Heap<QEM_Edge>(QEM_edges.Count);

            foreach (QEM_Edge e in QEM_edges)
            {
                sorted_QEM_edges.Add(e);
            }



            //
            // Start contracting edges
            //

            //For each edge we want to remove
            for (int i = 0; i < maxEdgesToContract; i++)
            {
                //Check that we can simplify the mesh
                //The smallest mesh we can have is a tetrahedron with 4 faces, itherwise we get a flat triangle
                if (halfEdgeMeshData.faces.Count <= 4)
                {
                    Debug.Log($"Cant contract more than {i} edges");
                
                    break;
                }


                //
                // Remove the pair (v1,v2) of the least cost and contract the pair         
                //

                //timer.Start();

                QEM_Edge smallestErrorEdge = sorted_QEM_edges.RemoveFirst();

                //This means an edge in this face has already been contracted
                //We are never removing edges from the heap after contracting and edges, 
                //so we do it this way for now, which is maybe better?
                if (smallestErrorEdge.halfEdge.face == null)
                {
                    //This edge wasn't contracted so don't add it to iteration
                    i -= 1;
                
                    continue;
                }

                if (smallestErrorEdge.qem > maxError)
                {
                    Debug.Log($"Cant contract more than {i} edges because reached max error");

                    break;
                }

                //timer.Stop();


                //timer.Start();

                //Get the half-edge we want to contract 
                HalfEdge3 edgeToContract = smallestErrorEdge.halfEdge;

                //Need to save the endpoints so we can remove the old Q matrices from the pos-matrix lookup table
                Edge3 contractedEdgeEndpoints = new Edge3(edgeToContract.prevEdge.v.position, edgeToContract.v.position);

                //Contract edge
                HashSet<HalfEdge3> edgesPointingToNewVertex = halfEdgeMeshData.ContractTriangleHalfEdge(edgeToContract, smallestErrorEdge.mergePosition, timer);

                //timer.Stop();



                //
                // Remove all QEM_edges that belonged to the faces we contracted
                //

                //This is not needed if we check if an edge in the triangle has already been contracted

                /*
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
                */

                //Remove the edges start and end vertices from the pos-matrix lookup table
                qMatrices.Remove(contractedEdgeEndpoints.p1);
                qMatrices.Remove(contractedEdgeEndpoints.p2);
                //timer.Stop();



                //
                // Update all QEM_edges that is now connected with the new contracted vertex because their errors have changed
                //

                //The contracted position has a new Q matrix
                Matrix4x4 QNew = CalculateQMatrix(edgesPointingToNewVertex, normalizeTriangles);

                //Add the Q matrix to the pos-matrix lookup table
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

                    sorted_QEM_edges.UpdateItem(QEM_edgeToV);


                    //From
                    QEM_Edge QEM_edgeFromV = halfEdge_QEM_Lookup[edgeFromV];

                    Edge3 edgeFromV_endPoints = QEM_edgeFromV.GetEdgeEndPoints();

                    Matrix4x4 Q1_edgeFromV = QNew;
                    Matrix4x4 Q2_edgeFromV = qMatrices[edgeFromV_endPoints.p2];

                    QEM_edgeFromV.UpdateEdge(edgeFromV, Q1_edgeFromV, Q2_edgeFromV);

                    sorted_QEM_edges.UpdateItem(QEM_edgeFromV);
                }
                //timer.Stop();
            }


            //Timers: 0.78 to generate the simplified bunny (2400 edge contractions) (normalizing triangles is 0.05 seconds slower)
            //Init:
            // - 0.1 to convert to half-edge data structure
            // - 0.14 to calculate a Q matrix for each unique vertex
            //Loop (total time):
            // - 0.04 to find smallest QEM error
            // - 0.25 to merge the edges (the bottleneck is where we have to find all edges pointing to a vertex)
            // - 0.02 to remove the data that was destroyed when we contracted an edge
            // - 0.13 to update QEM edges
            //Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to measure whatever we measured");


            return halfEdgeMeshData;
        }



        //Calculate the Q matrix for a vertex if we know all edges pointing to the vertex
        public static Matrix4x4 CalculateQMatrix(HashSet<HalfEdge3> edgesPointingToVertex, bool normalizeTriangles)
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
