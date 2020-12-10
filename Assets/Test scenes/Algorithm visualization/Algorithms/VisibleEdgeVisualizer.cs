using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;



//Visualizes the visible-edge triangulation algorithm
public class VisibleEdgeVisualizer : MonoBehaviour
{
    VisualizerController controller;



    public void StartVisualization(HashSet<MyVector2> points)
    {
        controller = GetComponent<VisualizerController>();

        //Start by showing all points we want to triangulate
        ShowPoints(points);

        StartCoroutine(RunVisualization(points));
    }



    private IEnumerator RunVisualization(HashSet<MyVector2> points)
    {

        //Step 0. Init the triangles we will return
        HashSet<Triangle2> triangles = new HashSet<Triangle2>();



        //Step 1. Sort the points
        List<MyVector2> sortedPoints = new List<MyVector2>(points);

        //OrderBy is always soring in ascending order - use OrderByDescending to get in the other order
        //sortedPoints = sortedPoints.OrderBy(n => n.x).ToList();

        //If we have colinear points we have to sort in both x and y
        sortedPoints = sortedPoints.OrderBy(n => n.x).ThenBy(n => n.y).ToList();



        //Step 2. Create the first triangle so we can start the algorithm because we need edges to look at
        //and see if they are visible

        //Pick the first two points in the sorted list - These are always a part of the first triangle
        MyVector2 p1 = sortedPoints[0];
        MyVector2 p2 = sortedPoints[1];

        //Remove them
        sortedPoints.RemoveAt(0);
        sortedPoints.RemoveAt(0);

        //The problem is the third point
        //If we have colinear points, then the third point in the sorted list is not always a valid point
        //to form a triangle because then it will be flat
        //So we have to look for a better point
        for (int i = 0; i < sortedPoints.Count; i++)
        {
            //We have found a non-colinear point
            LeftOnRight pointRelation = _Geometry.IsPoint_Left_On_Right_OfVector(p1, p2, sortedPoints[i]);

            if (pointRelation == LeftOnRight.Left || pointRelation == LeftOnRight.Right)
            {
                MyVector2 p3 = sortedPoints[i];

                //Remove this point
                sortedPoints.RemoveAt(i);

                //Build the first triangle
                Triangle2 newTriangle = new Triangle2(p1, p2, p3);

                triangles.Add(newTriangle);

                break;
            }
        }

        ////If we have finished search and not found a triangle, that means that all points
        ////are colinear and we cant form any triangles
        //if (triangles.Count == 0)
        //{
        //    Debug.Log("All points you want to triangulate a co-linear");

        //    return null;
        //}



        //Show the triangulation
        ShowTriangles(triangles);

        yield return new WaitForSeconds(controller.pauseTime);



        //Step 3. Add the other points one-by-one

        //For each point we add we have to calculate a convex hull of the previous points
        //An optimization is to not use all points in the triangulation
        //to calculate the hull because many of them might be inside of the hull
        //So we will use the previous points on the hull and add the point we added last iteration
        //to generate the new convex hull

        //First we need to init the convex hull
        HashSet<MyVector2> triangulatePoints = new HashSet<MyVector2>();

        foreach (Triangle2 t in triangles)
        {
            triangulatePoints.Add(t.p1);
            triangulatePoints.Add(t.p2);
            triangulatePoints.Add(t.p3);
        }

        //Calculate the first convex hull
        List<MyVector2> pointsOnHull = _ConvexHull.JarvisMarch_2D(triangulatePoints);

        //Add the other points one-by-one
        foreach (MyVector2 pointToAdd in sortedPoints)
        {
            bool couldFormTriangle = false;


            //Show which point we draw triangles to
            controller.SetActivePoint(pointToAdd);


            //Loop through all edges in the convex hull
            for (int j = 0; j < pointsOnHull.Count; j++)
            {
                MyVector2 hull_p1 = pointsOnHull[j];
                MyVector2 hull_p2 = pointsOnHull[MathUtility.ClampListIndex(j + 1, pointsOnHull.Count)];

                //First we have to check if the points are colinear, then we cant form a triangle
                LeftOnRight pointRelation = _Geometry.IsPoint_Left_On_Right_OfVector(hull_p1, hull_p2, pointToAdd);

                if (pointRelation == LeftOnRight.On)
                {
                    continue;
                }

                //If this triangle is clockwise, then we can see the edge
                //so we should create a new triangle with this edge and the point
                if (_Geometry.IsTriangleOrientedClockwise(hull_p1, hull_p2, pointToAdd))
                {
                    triangles.Add(new Triangle2(hull_p1, hull_p2, pointToAdd));

                    couldFormTriangle = true;


                    //Show the triangulation
                    ShowTriangles(triangles);

                    yield return new WaitForSeconds(controller.pauseTime);
                }
            }

            //Add the point to the list of points on the hull
            //Will re-generate the hull by using these points so dont worry that the 
            //list is not valid anymore
            if (couldFormTriangle)
            {
                pointsOnHull.Add(pointToAdd);

                //Find the convex hull of the current triangulation
                //It generates a counter-clockwise convex hull
                pointsOnHull = _ConvexHull.JarvisMarch_2D(new HashSet<MyVector2>(pointsOnHull));
            }
            else
            {
                Debug.Log("This point could not form any triangles " + pointToAdd.x + " " + pointToAdd.y);
            }
        }



        yield return null;
    }



    //
    // Display stuff
    //

    //Show points
    private void ShowPoints(HashSet<MyVector2> points)
    {
        controller.ResetBlackMeshes();


        HashSet<Triangle2> triangles = new HashSet<Triangle2>();

        foreach (MyVector2 p in points)
        {
            MyVector2 point = controller.UnNormalize(p);
        
            HashSet<Triangle2> circleTriangles = _GenerateMesh.Circle(point, 0.1f, 10);

            triangles.UnionWith(circleTriangles);
        }


        //Will unnormalize
        List<Mesh> meshes = controller.GenerateTriangulationMesh(triangles, shouldUnNormalize: false);

        controller.blackMeshes = meshes;
    }


    //Show triangles
    private void ShowTriangles(HashSet<Triangle2> triangles)
    {
        controller.ResetMultiColoredMeshes();

        List<Mesh> meshes = controller.GenerateTriangulationMesh(triangles, shouldUnNormalize: true);

        List<Material> materials = controller.GenerateRandomMaterials(triangles.Count);

        controller.multiColoredMeshes = meshes;
        controller.multiColoredMeshesMaterials = materials;
    }
}
