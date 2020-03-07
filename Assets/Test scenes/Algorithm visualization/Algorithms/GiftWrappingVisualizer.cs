using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;

public class GiftWrappingVisualizer : MonoBehaviour
{
    VisualizerController controller;



    public void InitVisualization(HashSet<MyVector2> points)
    {
        controller = GetComponent<VisualizerController>();    
    
        //VISUALIZE
        //Generate meshes for all points 
        ShowAllPoints(points);

        StartCoroutine(RunAlgorithm(new List<MyVector2>(points)));
    }



    private IEnumerator RunAlgorithm(List<MyVector2> points)
    {
        //The list with points on the convex hull
        List<MyVector2> pointsOnConvexHull = new List<MyVector2>();


        //Step 0. Normalize the data to range [0, 1] or everything will break at larger sizes :(
        //Make sure the data is already normalized!!!



        //Step 1. Find the vertex with the smallest x coordinate
        //If several points have the same x coordinate, find the one with the smallest y
        MyVector2 startPos = points[0];

        for (int i = 1; i < points.Count; i++)
        {
            MyVector2 testPos = points[i];

            //Because of precision issues, we use a small value to test if they are the same
            if (testPos.x < startPos.x || ((Mathf.Abs(testPos.x - startPos.x) < MathUtility.EPSILON && testPos.y < startPos.y)))
            {
                startPos = points[i];
            }
        }

        //This vertex is always on the convex hull
        pointsOnConvexHull.Add(startPos);


        //VISUALIZE
        ShowHull(pointsOnConvexHull, null);

        yield return new WaitForSeconds(controller.pauseTime);


        //But we can't remove it from the list of all points because we need it to stop the algorithm
        //points.Remove(startPos);



        //Step 2. Loop to find the other points on the hull
        MyVector2 previousPoint = pointsOnConvexHull[0];

        int counter = 0;

        while (true)
        {
            //We might have colinear points, so we need a list to save all points added this iteration
            List<MyVector2> pointsToAddToTheHull = new List<MyVector2>();


            //Pick next point randomly
            MyVector2 nextPoint = points[Random.Range(0, points.Count)];

            //If we are coming from the first point on the convex hull
            //then we are not allowed to pick it as next point, so we have to try again
            if (previousPoint.Equals(pointsOnConvexHull[0]) && nextPoint.Equals(pointsOnConvexHull[0]))
            {
                counter += 1;

                continue;
            }


            //VISUALIZE
            ShowHull(pointsOnConvexHull, new List<MyVector2>() { nextPoint });
            //ShowActivePoint(nextPoint);

            yield return new WaitForSeconds(controller.pauseTime);

            //This point is assumed to be on the convex hull
            pointsToAddToTheHull.Add(nextPoint);


            //But this randomly selected point might not be the best next point, so we have to see if we can improve
            //by finding a point that is more to the right
            //We also have to check if this point has colinear points if it happens to be on the hull
            for (int i = 0; i < points.Count; i++)
            {
                MyVector2 testPoint = points[i];

                //Dont test the point we picked randomly
                //Or the point we are coming from which might happen when we move from the first point on the hull
                if (testPoint.Equals(nextPoint) || testPoint.Equals(previousPoint))
                {
                    continue;
                }


                //VISUALIZE
                //ShowHull(pointsOnConvexHull, new List<MyVector2>() { testPoint });
                ShowActivePoint(testPoint);

                yield return new WaitForSeconds(controller.pauseTime);


                //Where is the test point in relation to the line between the point we are coming from
                //which we know is on the hull, and the point we think is on the hull
                LeftOnRight pointRelation = _Geometry.IsPoint_Left_On_Right_OfVector(previousPoint, nextPoint, testPoint);

                //The test point is on the line, so we have found a colinear point
                if (pointRelation == LeftOnRight.On)
                {
                    pointsToAddToTheHull.Add(testPoint);
                }
                //To the right = better point, so pick it as next point we want to test if it is on the hull
                else if (pointRelation == LeftOnRight.Right)
                {
                    nextPoint = testPoint;

                    //Clear colinear points because they belonged to the old point which was worse
                    pointsToAddToTheHull.Clear();

                    pointsToAddToTheHull.Add(nextPoint);

                    //We dont have to start over because the previous points weve gone through were worse


                    //VISUALIZE
                    ShowHull(pointsOnConvexHull, pointsToAddToTheHull);

                    yield return new WaitForSeconds(controller.pauseTime);
                }
                //To the left = worse point so do nothing
            }



            //Sort this list, so we can add the colinear points in correct order
            pointsToAddToTheHull = pointsToAddToTheHull.OrderBy(n => MyVector2.SqrMagnitude(n - previousPoint)).ToList();

            pointsOnConvexHull.AddRange(pointsToAddToTheHull);

            //Remove the points that are now on the convex hull, which should speed up the algorithm
            //Or will it be slower because it also takes some time to remove points?
            for (int i = 0; i < pointsToAddToTheHull.Count; i++)
            {
                points.Remove(pointsToAddToTheHull[i]);
            }


            //The point we are coming from in the next iteration
            previousPoint = pointsOnConvexHull[pointsOnConvexHull.Count - 1];


            //Have we found the first point on the hull? If so we have completed the hull
            if (previousPoint.Equals(pointsOnConvexHull[0]))
            {
                //Then remove it because it is the same as the first point, and we want a convex hull with no duplicates
                pointsOnConvexHull.RemoveAt(pointsOnConvexHull.Count - 1);

                //Stop the loop!
                break;
            }


            //Safety
            if (counter > 100000)
            {
                Debug.Log("Stuck in endless loop when generating convex hull with jarvis march");

                break;
            }

            counter += 1;
        }



        //Dont forget to unnormalize the points!

        //VISUALIZE
        ShowHull(pointsOnConvexHull, new List<MyVector2>(){pointsOnConvexHull[0]});

        yield return null;
    }



    //
    // Visualize stuff
    //

    private void ShowAllPoints(HashSet<MyVector2> points)
    {
        HashSet<Triangle2> triangles = new HashSet<Triangle2>();

        foreach (MyVector2 p in points)
        {
            HashSet<Triangle2> tCircle = _GenerateMesh.Circle(controller.UnNormalize(p), 0.1f, 10);

            triangles.UnionWith(tCircle);
        }

        //To mesh
        List<Mesh> meshes = controller.GenerateTriangulationMesh(triangles, shouldUnNormalize: false);

        controller.ResetBlackMeshes();

        controller.blackMeshes = meshes;

        //Debug.Log(meshes.Count);
    }



    //Testpoint is in a list so we can null it
    private void ShowHull(List<MyVector2> pointsOnHUll, List<MyVector2> testPoint)
    {
        //Normalize
        List<MyVector2> connectedPoints = new List<MyVector2>();

        if (pointsOnHUll != null)
        {
            foreach (MyVector2 p in pointsOnHUll)
            {
                connectedPoints.Add(controller.UnNormalize(p));
            }
        }

        if (testPoint != null)
        {
            foreach (MyVector2 p in testPoint)
            {
                connectedPoints.Add(controller.UnNormalize(p));
            }
        }

        controller.connectedPoints = connectedPoints;
    }



    //Show active point we are testing if its more to the right
    private void ShowActivePoint(MyVector2 p)
    {
        controller.activePoint = controller.UnNormalize(p);
    }
}
