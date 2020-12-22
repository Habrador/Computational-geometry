using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;


//Visualize mesh simplification by using QEM
public class VisualizeMergeEdgesQEM : MonoBehaviour
{

    private VisualizerController3D controller;


    public void StartVisualizer(HalfEdgeData3 halfEdgeMeshData, int maxEdgesToContract, float maxError, bool normalizeTriangles = false)
    {
        controller = GetComponent<VisualizerController3D>();


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
            Matrix4x4 Q = MeshSimplification_QEM.CalculateQMatrix(edgesPointingToThisVertex, normalizeTriangles);
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


        //Main visualization algorithm coroutine
        StartCoroutine(QEMLoop(halfEdgeMeshData, sorted_QEM_edges, qMatrices, halfEdge_QEM_Lookup, maxEdgesToContract, maxError, normalizeTriangles));
    }


    private IEnumerator QEMLoop(HalfEdgeData3 halfEdgeMeshData, Heap<QEM_Edge> sorted_QEM_edges, Dictionary<MyVector3, Matrix4x4> qMatrices, Dictionary<HalfEdge3, QEM_Edge> halfEdge_QEM_Lookup, int maxEdgesToContract, float maxError, bool normalizeTriangles = false)
    {

        //PAUSE FOR VISUALIZATION
        //Display what we have so far
        controller.DisplayMeshMain(halfEdgeMeshData.faces);

        controller.displayStuffUI.text = "Triangles: " + halfEdgeMeshData.faces.Count.ToString();

        yield return new WaitForSeconds(5f);


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
            HashSet<HalfEdge3> edgesPointingToNewVertex = halfEdgeMeshData.ContractTriangleHalfEdge(edgeToContract, smallestErrorEdge.mergePosition);

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
            Matrix4x4 QNew = MeshSimplification_QEM.CalculateQMatrix(edgesPointingToNewVertex, normalizeTriangles);

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



            //PAUSE FOR VISUALIZATION
            //Display what we have so far
            controller.DisplayMeshMain(halfEdgeMeshData.faces);

            controller.displayStuffUI.text = "Triangles: " + halfEdgeMeshData.faces.Count.ToString();

            yield return new WaitForSeconds(0.02f);
        }
    }
}
