using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;

//Generate a 3d voronoi diagram on a sphere
//This will use the delaunay triangulation on a sphere, which is the same as the convex hull of the sphere
//TODO:
// - Add some optimization to make each voronoi region as flat as possible
public class VoronoiSphereController : MonoBehaviour
{
    public int seed;

    public int numberOfPoints;

    public float radius;

    //To display voronoi cells with vertex colors
    public MeshFilter meshFilter;


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


        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();


        //
        // Generate the convex hull, which is the same as the Delaunay triangulation of points on the sphere
        //

        //Iterative algorithm
        timer.Start();

        HalfEdgeData3 convexHull_normalized = _ConvexHull.Iterative_3D(points_normalized, removeUnwantedTriangles: false, normalizer);

        timer.Stop();

        Debug.Log($"Generated a 3d convex hull in {timer.ElapsedMilliseconds / 1000f} seconds");

        if (convexHull_normalized == null)
        {
            return;
        }



        //
        // Generate the voronoi diagram from the delaunay triangulation
        //
        timer.Restart();

        HashSet<VoronoiCell3> voronoiCells_normalized = _Voronoi.Delaunay3DToVoronoi(convexHull_normalized);

        timer.Stop();

        Debug.Log($"Generated a 3d voronoi diagram in {timer.ElapsedMilliseconds / 1000f} seconds");

        if (voronoiCells_normalized == null)
        {
            return;
        }


        //
        // Display
        //
        
        //Delaunay
        HalfEdgeData3 convexHull = normalizer.UnNormalize(convexHull_normalized);

        MyMesh myMesh = convexHull.ConvertToMyMesh("convex hull aka delaunay triangulation", MyMesh.MeshStyle.HardEdges);

        delaunayMesh = myMesh.ConvertToUnityMesh(generateNormals: false);


        //Voronoi
        HashSet<VoronoiCell3> voronoiCells = normalizer.UnNormalize(voronoiCells_normalized);

        //Generate a mesh for each separate cell
        voronoiCellsMeshes = GenerateVoronoiCellsMeshes(voronoiCells);

        //Generate a single mesh for all cells where each vertex has a color belonging to that cell
        //Now we can display the mesh with an unlit shader where each vertex is associated with a color belonging to that cell
        //The problem is that the voronoi cell is not a flat surface on the mesh
        //But it looks flat if we are using an unlit shader
        Mesh oneMesh = GenerateAndDisplaySingleMesh(voronoiCellsMeshes);

        if (meshFilter != null)
        {
            meshFilter.mesh = oneMesh;
        }
    }



    //Make it a single mesh
    private Mesh GenerateAndDisplaySingleMesh(HashSet<Mesh> meshes)
    {
        Mesh voronoiCellsMesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Color> vertexColors = new List<Color>();
        List<Vector3> normals = new List<Vector3>();

        foreach (Mesh mesh in meshes)
        {
            int numberOfVerticesBefore = vertices.Count;
        
            vertices.AddRange(mesh.vertices);
            vertexColors.AddRange(mesh.colors);
            normals.AddRange(mesh.normals);

            //Triangles are not the same
            int[] oldTriangles = mesh.triangles;

            for (int i = 0; i < oldTriangles.Length; i++)
            {
                triangles.Add(oldTriangles[i] + numberOfVerticesBefore);
            }
        }

        Mesh oneMesh = new Mesh();

        oneMesh.SetVertices(vertices);
        oneMesh.SetTriangles(triangles, 0);
        oneMesh.SetNormals(normals);
        oneMesh.SetColors(vertexColors);

        return oneMesh;
    }



    //Generate a single mesh for each voronoi cell
    //Each vertex belonging to a cell gets its a color associated with that cell 
    private HashSet<Mesh> GenerateVoronoiCellsMeshes(HashSet<VoronoiCell3> voronoiCells)
    {
        HashSet<Mesh> meshes = new HashSet<Mesh>();

        foreach (VoronoiCell3 cell in voronoiCells)
        {
            List<Vector3> vertices = new List<Vector3>();

            List<int> triangles = new List<int>();

            List<Vector3> normals = new List<Vector3>();
        
            List<VoronoiEdge3> edges = cell.edges;

            //This is the center of the cell
            //To build the mesh, we just add triangles from the edges to the site pos
            MyVector3 sitePos = cell.sitePos;

            //In 3d space, the corners in the voronoi cell are not on the plane, so shading becomes bad
            //Shading improves if we calculate an average site pos by looking at each corner in the cell
            MyVector3 averageSitePos = default;

            for (int i = 0; i < edges.Count; i++)
            {
                averageSitePos += edges[i].p1;
            }

            averageSitePos = averageSitePos * (1f / edges.Count);

            vertices.Add(averageSitePos.ToVector3());

            //VoronoiEdge3 e0 = edges[0];

            //Vector3 normal = Vector3.Cross(e0.p2.ToVector3() - e0.p1.ToVector3(), e0.sitePos.ToVector3() - e0.p1.ToVector3()).normalized;

            //normals.Add(normal);


            //Another way to get a nicer looking surface is to use a vertex color
            //and then use a shader set to non-lit
            Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);

            List<Color> vertexColors = new List<Color>();

            vertexColors.Add(color);

            foreach (VoronoiEdge3 e in edges)
            {
                //Build a triangle with this edge and the voronoi site which is sort of the center
                vertices.Add(e.p2.ToVector3());
                vertices.Add(e.p1.ToVector3());
                //verts.Add(e.sitePos.ToVector3());

                //normals.Add(normal);
                //normals.Add(normal);

                vertexColors.Add(color);
                vertexColors.Add(color);

                int triangleCounter = triangles.Count;

                triangles.Add(0);
                triangles.Add(vertices.Count - 1);
                triangles.Add(vertices.Count - 2);
            }

            Mesh mesh = new Mesh();

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            //mesh.SetNormals(normals);

            mesh.SetColors(vertexColors);

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
            //Random.InitState(seed);
        
            //foreach (Mesh mesh in voronoiCellsMeshes)
            //{
            //    Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);

            //    TestAlgorithmsHelpMethods.DisplayMesh(mesh, color);
            //}
        }
    }    
}
