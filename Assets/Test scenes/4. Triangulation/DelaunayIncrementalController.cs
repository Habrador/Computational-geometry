using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

public class DelaunayIncrementalController : MonoBehaviour
{
    Mesh triangulatedMesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    public Rect bounds;
    AABB2 normalizingBox; // Rect in Habrador? what's the difference to Rect??
    float dMax;
    HalfEdgeData2 delaunayData_normalized;

    void Start()
    {
        InitializeMeshComponents();

        normalizingBox = new AABB2(bounds.xMin, bounds.xMax, bounds.yMin, bounds.yMax);
        dMax = HelpMethods.CalculateDMax(normalizingBox);

        // Triangle2 superTriangle = new Triangle2(new MyVector2(-10f, -10f), new MyVector2(10f, -10f), new MyVector2(0f, 10f));
        float rightNormalized = (normalizingBox.maxX - normalizingBox.minX) / dMax;
        float topNormalized = (normalizingBox.maxY - normalizingBox.minY) / dMax;
        Triangle2 quadTri1 = new Triangle2(new MyVector2(0f, 0f), new MyVector2(rightNormalized, 0f), new MyVector2(0f, topNormalized));
        Triangle2 quadTri2 = new Triangle2(new MyVector2(rightNormalized, 0f), new MyVector2(rightNormalized, topNormalized), new MyVector2(0f, topNormalized));
        

        //Create the triangulation data with a quad
        HashSet<Triangle2> triangles_normalized = new HashSet<Triangle2>();
        triangles_normalized.Add(quadTri1);
        triangles_normalized.Add(quadTri2);

        //Change to half-edge data structure
        delaunayData_normalized = new HalfEdgeData2();
        _TransformBetweenDataStructures.Triangle2ToHalfEdge2(triangles_normalized, delaunayData_normalized);
    }

    void InitializeMeshComponents()
    {
        meshFilter = GetComponent<MeshFilter>();
        if(meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        if(meshFilter.mesh == null)
        {
            meshFilter.mesh = new Mesh();
        }
        triangulatedMesh = meshFilter.mesh;

        meshRenderer = GetComponent<MeshRenderer>();
        if(meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
    }

    [ContextMenu("Add random point")]
    void AddRandomPoint()
    {
        //These are for display purposes only
        int missedPoints = 0;
        int flippedEdges = 0;

        // random value within bounds, normalized
        float x = (normalizingBox.maxX - normalizingBox.minX) * Random.value / dMax;
        float y = (normalizingBox.maxY - normalizingBox.minY) * Random.value / dMax;
        DelaunayIncrementalSloan.InsertNewPointInTriangulation(new MyVector2(x, y), delaunayData_normalized, ref missedPoints, ref flippedEdges);
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.Space))
        {
            AddRandomPoint();
        }

        //From half-edge to triangle
        HashSet<Triangle2> triangles_2d_normalized = _TransformBetweenDataStructures.HalfEdge2ToTriangle2(delaunayData_normalized);

        //From 2d to 3d
        HashSet<Triangle3> triangles_3d = new HashSet<Triangle3>();

        foreach (Triangle2 t in triangles_2d_normalized)
        {
            //UnNormalize here
            triangles_3d.Add(new Triangle3(HelpMethods.UnNormalize(t.p1, normalizingBox, dMax).ToMyVector3(), HelpMethods.UnNormalize(t.p2, normalizingBox, dMax).ToMyVector3(), HelpMethods.UnNormalize(t.p3, normalizingBox, dMax).ToMyVector3()));
        }

        triangulatedMesh = _TransformBetweenDataStructures.Triangle3ToCompressedMesh(triangles_3d, triangulatedMesh); 
    }

}
