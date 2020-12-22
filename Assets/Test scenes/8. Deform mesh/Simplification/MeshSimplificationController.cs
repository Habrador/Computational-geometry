using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;

public class MeshSimplificationController : MonoBehaviour
{
    public MeshFilter meshFilterToSimplify;

    public MeshFilter meshFilterToShowSimplifiedMesh;


    public void SimplifyMesh()
    {
        //Has to be sharedMesh if we are using Editor tools
        Mesh meshToSimplify = meshFilterToSimplify.sharedMesh;


        //
        // Change data structure and normalize
        //

        //Mesh -> MyMesh
        MyMesh myMeshToSimplify = new MyMesh(meshToSimplify); 

        //Normalize to 0-1
        Normalizer3 normalizer = new Normalizer3(myMeshToSimplify.vertices);

        //We only need to normalize the vertices
        myMeshToSimplify.vertices = normalizer.Normalize(myMeshToSimplify.vertices);

        HalfEdgeData3 myMeshToSimplify_HalfEdge = new HalfEdgeData3(myMeshToSimplify, HalfEdgeData3.ConnectOppositeEdges.Fast);



        //
        // Simplify
        //

        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

        timer.Start();

        HalfEdgeData3 mySimplifiedMesh_HalfEdge = MeshSimplification_QEM.Simplify(myMeshToSimplify_HalfEdge, maxEdgesToContract: 2400, maxError: Mathf.Infinity, normalizeTriangles: true);

        timer.Stop();

        Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to simplify the mesh");



        //
        // Change data structure and un-normalize
        //

        timer.Reset();
        timer.Start();

        //From half-edge to mesh
        MyMesh mySimplifiedMesh = mySimplifiedMesh_HalfEdge.ConvertToMyMesh("Simplified mesh", MyMesh.MeshStyle.HardEdges);

        //Un-Normalize
        mySimplifiedMesh.vertices = normalizer.UnNormalize(mySimplifiedMesh.vertices);

        //Convert to global space
        Transform trans = meshFilterToSimplify.transform;

        mySimplifiedMesh.vertices = mySimplifiedMesh.vertices.Select(x => trans.TransformPoint(x.ToVector3()).ToMyVector3()).ToList();

        //Convert to mesh
        Mesh unitySimplifiedMesh = mySimplifiedMesh.ConvertToUnityMesh(generateNormals: true, meshName: "simplified mesh");

        //Attach to new game object
        meshFilterToShowSimplifiedMesh.mesh = unitySimplifiedMesh;

        timer.Stop();

        Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to finalize the mesh after simplifying");
    }
}
