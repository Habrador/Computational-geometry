using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;

//Will also generate Delaunay on a sphere
public class VoronoiSphereController : MonoBehaviour
{
    public int seed;

    public int numberOfPoints;

    public float radius;

    //What will be generated
    private Mesh delaunayMesh;

    private HashSet<Vector3> points_Unity;


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

        if (convexHull_normalized != null)
        {
            //To unity mesh
            //UnNormalize
            HalfEdgeData3 convexHull = normalizer.UnNormalize(convexHull_normalized);

            delaunayMesh = convexHull.ConvertToUnityMesh("convex hull aka delaunay triangulation", shareVertices: false, generateNormals: false);
        }
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
            TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors(delaunayMesh, 0);

            //TestAlgorithmsHelpMethods.DisplayMeshEdges(delaunayMesh, Color.black);
        }
    }    
}
