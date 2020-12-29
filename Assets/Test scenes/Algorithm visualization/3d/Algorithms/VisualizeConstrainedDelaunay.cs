using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;


public class VisualizeConstrainedDelaunay : MonoBehaviour
{
    private VisualizerController3D controller;

    //Constraints by using children to a parent, which we have to drag
    //Should be sorted counter-clock-wise
    public Transform hullConstraintParent;
    //Should be sorted clock-wise
    public List<Transform> holeConstraintParents;



    void Start()
	{
        controller = GetComponent<VisualizerController3D>();


        //Hull
        List<Vector3> hullPoints = TestAlgorithmsHelpMethods.GetPointsFromParent(hullConstraintParent);

        List<MyVector2> hullPoints_2d = hullPoints.Select(x => x.ToMyVector2()).ToList(); ;

        //Holes
        HashSet<List<MyVector2>> allHolePoints_2d = new HashSet<List<MyVector2>>();

        foreach (Transform holeParent in holeConstraintParents)
        {
            List<Vector3> holePoints = TestAlgorithmsHelpMethods.GetPointsFromParent(holeParent);

            if (holePoints != null)
            {
                List<MyVector2> holePoints_2d = holePoints.Select(x => x.ToMyVector2()).ToList();

                allHolePoints_2d.Add(holePoints_2d);
            }
        }


        //Normalize to range 0-1
        //We should use all points, including the constraints because the hole may be outside of the random points
        List<MyVector2> allPoints = new List<MyVector2>();

        //allPoints.AddRange(randomPoints_2d);

        allPoints.AddRange(hullPoints_2d);

        foreach (List<MyVector2> hole in allHolePoints_2d)
        {
            allPoints.AddRange(hole);
        }

        Normalizer2 normalizer = new Normalizer2(allPoints);

        List<MyVector2> hullPoints_2d_normalized = normalizer.Normalize(hullPoints_2d);

        HashSet<List<MyVector2>> allHolePoints_2d_normalized = new HashSet<List<MyVector2>>();

        foreach (List<MyVector2> hole in allHolePoints_2d)
        {
            List<MyVector2> hole_normalized = normalizer.Normalize(hole);

            allHolePoints_2d_normalized.Add(hole_normalized);
        }



        StartCoroutine(GenerateConstrainedDelaunayLoop(null, hullPoints_2d_normalized, allHolePoints_2d_normalized, shouldRemoveTriangles: true, new HalfEdgeData2(), normalizer));
    }



    private IEnumerator GenerateConstrainedDelaunayLoop(HashSet<MyVector2> points, List<MyVector2> hull, HashSet<List<MyVector2>> holes, bool shouldRemoveTriangles, HalfEdgeData2 triangleData, Normalizer2 normalizer)
    {
        //Start by generating a delaunay triangulation with all points, including the constraints
        HashSet<MyVector2> allPoints = new HashSet<MyVector2>();

        if (points != null)
        {
            allPoints.UnionWith(points);
        }

        if (hull != null)
        {
            allPoints.UnionWith(hull);
        }

        if (holes != null)
        {
            foreach (List<MyVector2> hole in holes)
            {
                allPoints.UnionWith(hole);
            }
        }


        

        //Generate the Delaunay triangulation with some algorithm

        //triangleData = _Delaunay.FlippingEdges(allPoints);
        triangleData = _Delaunay.PointByPoint(allPoints, triangleData);


        //
        // PAUSE AND VISUALIZE
        //

        controller.DisplayMeshMain(triangleData, normalizer);

        yield return new WaitForSeconds(5f);


        /*
        //Modify the triangulation by adding the constraints to the delaunay triangulation
        triangleData = AddConstraints(triangleData, hull, shouldRemoveTriangles, timer);

        foreach (List<MyVector2> hole in holes)
        {
            triangleData = AddConstraints(triangleData, hole, shouldRemoveTriangles, timer);
        }
        */
        
    }
}
