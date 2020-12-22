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
        StartCoroutine(QEMLoop(halfEdgeMeshData, maxEdgesToContract, maxError, normalizeTriangles));
    }


    private IEnumerator QEMLoop(HalfEdgeData3 halfEdgeMeshData, int maxEdgesToContract, float maxError, bool normalizeTriangles = false)
    {

        //PAUSE FOR VISUALIZATION
        //Display what we have so far
        controller.DisplayMeshMain(halfEdgeMeshData.faces);

        yield return new WaitForSeconds(10f);
    }
}
