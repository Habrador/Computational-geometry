using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;

//Generate a 3d voronoi diagram on a sphere
//This will use the delaunay triangulation on a sphere, which is the same as the convex hull of the sphere
public class VoronoiSphereController : MonoBehaviour
{
    public int seed;

    public int numberOfPoints;

    public float radius;

    //What will be generated
    private Mesh delaunayMesh;

    private HashSet<Vector3> points_Unity;

    //Each cell has its own mesh
    private HashSet<Mesh> voronoiCellsMeshes;



    //Generates points, delaunay triangulation, and voronoi diagram
    public void Generate()
    {
        if (radius <= 0f)
        {
            radius = 0.01f;
        }
        if (numberOfPoints < 4)
        {
            numberOfPoints = 4;
        }

        //Get random points in 3d space
        points_Unity = TestAlgorithmsHelpMethods.GenerateRandomPointsOnSphere(seed, radius, numberOfPoints);

        //To MyVector3
        HashSet<MyVector3> points = new HashSet<MyVector3>(points_Unity.Select(x => x.ToMyVector3()));

        //Normalize
        Normalizer3 normalizer = new Normalizer3(new List<MyVector3>(points));

        HashSet<MyVector3> points_normalized = normalizer.Normalize(points);


        //Generate the convex hull, which is the same as the Delaunay triangulation of points on the sphere

        //Iterative algorithm
        HalfEdgeData3 convexHull_normalized = _ConvexHull.Iterative_3D(points_normalized, normalizer);
        //HalfEdgeData3 convexHull_normalized = null;

        //Generate the voronoi diagram from the delaunay triangulation
        if (convexHull_normalized != null)
        {
            HashSet<VoronoiCell3> voronoiCells_normalized = _Voronoi.Delaunay3DToVoronoi(convexHull_normalized);

            if (voronoiCells_normalized != null)
            {
                //Unnormalize
                HashSet<VoronoiCell3> voronoiCells = normalizer.UnNormalize(voronoiCells_normalized);

                //Generate a mesh for each separate cell
                voronoiCellsMeshes = GenerateVoronoiCellsMeshes(voronoiCells);
            }
        }


        if (convexHull_normalized != null)
        {
            //To unity mesh
            //UnNormalize
            HalfEdgeData3 convexHull = normalizer.UnNormalize(convexHull_normalized);

            delaunayMesh = convexHull.ConvertToUnityMesh("convex hull aka delaunay triangulation", shareVertices: false, generateNormals: false);
        }
    }



    private HashSet<Mesh> GenerateVoronoiCellsMeshes(HashSet<VoronoiCell3> voronoiCells)
    {
        HashSet<Mesh> meshes = new HashSet<Mesh>();

        foreach (VoronoiCell3 cell in voronoiCells)
        {
            List<Vector3> verts = new List<Vector3>();

            List<int> triangles = new List<int>();
        
            List<VoronoiEdge3> edges = cell.edges;
            
            //TODO: Update so they share vertices
            foreach(VoronoiEdge3 e in edges)
            {
                //Build a triangle with this edge and the voronoi site which is sort of the center
                verts.Add(e.p1.ToVector3());
                verts.Add(e.p2.ToVector3());
                verts.Add(e.sitePos.ToVector3());

                int triangleCounter = triangles.Count;

                triangles.Add(triangleCounter + 0);
                triangles.Add(triangleCounter + 1);
                triangles.Add(triangleCounter + 2);
            }

            Mesh mesh = new Mesh();

            mesh.SetVertices(verts);
            mesh.SetTriangles(triangles, 0);

            mesh.RecalculateNormals();

            meshes.Add(mesh);
        }

        return meshes;
    }



    void OnDrawGizmosSelected()
	{
        //Points
        if (points_Unity != null)
        {
            //TestAlgorithmsHelpMethods.DisplayPoints(points_Unity, 0.01f, Color.black);
        }

       
        //Hull = delaunay triangulation
        if (delaunayMesh != null)
        {
            //TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors(delaunayMesh, 0);

            //TestAlgorithmsHelpMethods.DisplayMeshEdges(delaunayMesh, Color.black);
        }


        //Voronoi cells
        if (voronoiCellsMeshes != null)
        {
            Random.InitState(seed);
        
            foreach (Mesh mesh in voronoiCellsMeshes)
            {
                Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);

                TestAlgorithmsHelpMethods.DisplayMesh(mesh, color);
            }
        }
    }    
}
