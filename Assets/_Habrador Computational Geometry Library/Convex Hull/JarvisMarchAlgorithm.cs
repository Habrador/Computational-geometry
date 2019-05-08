using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Generate a counter-clockwise convex hull with the jarvis march algorithm (gift wrapping)
    //The basic idea is that we first find a point we know is on the convex hull, then the next 
    //point is always to the right of all other points
    //The algorithm is O(n*n) but is often faster if the number of points on the hull is fewer than all points
    //In that case the algorithm will be O(h * n)
    //Is more robust than other algorithms because it will handle colinear points with ease
    public static class JarvisMarchAlgorithm
    {
        public static List<Vector2> GetConvexHull(HashSet<Vector2> inputPoints)
        {
            List<Vector2> points = new List<Vector2>(inputPoints);
        
            //If fewer points, then we cant create a convex hull
            if (points.Count < 3)
            {
                Debug.Log("Too few points co calculate a convex hull");
            
                return null;
            }

            //Find the bounding box of the points
            //If the spread is close to 0, then they are all at the same position, and we cant create a hull
            AABB box = HelpMethods.GetAABB(points);

            if (Mathf.Abs(box.maxX - box.minX) < MathUtility.EPSILON && Mathf.Abs(box.maxY - box.minY) < MathUtility.EPSILON)
            {
                Debug.Log("The points cant form a convex hull");

                return null;
            }



            //Hashset results in infinite loop for some reason when using First() to select the first element
            //HashSet<Vector2> points = new HashSet<Vector2>(pointsList);

            //The list with points on the convex hull
            List<Vector2> convexHull = new List<Vector2>();

            //Step 1. Find the vertex with the smallest x coordinate
            //If several have the same x coordinate, find the one with the smallest z
            Vector2 startPos = points[0];

            for (int i = 1; i < points.Count; i++)
            {
                Vector2 testPos = points[i];

                //Because of precision issues, we use Mathf.Approximately to test if the x positions are the same
                if (testPos.x < startPos.x || (testPos.x == startPos.x && testPos.y < startPos.y))
                {
                    startPos = points[i];
                }
            }

            //This vertex is always on the convex hull
            convexHull.Add(startPos);

            //But we can't remove it from the list of all points because we need it to stop the algorithm
            //points.Remove(startPos);



            //Step 2. Loop to generate the convex hull
            Vector2 currentPoint = convexHull[0];

            int counter = 0;

            while (true)
            {
                //We might have colinear points, so we need a list to save all points added this iteration
                List<Vector2> pointsToAddToTheHull = new List<Vector2>();


                //Pick next point randomly
                Vector2 nextPoint = points[Random.Range(0, points.Count)];

                //If we are coming from the first point on the convex hull
                //then we are not allowed to pick it as next point, so we have to try again
                if (nextPoint == convexHull[0] && currentPoint == convexHull[0])
                {
                    counter += 1;
                
                    continue;
                }

                //This point is assumed to be on the convex hull
                pointsToAddToTheHull.Add(nextPoint);

                //But this randomly selected point might not be the best next point, so we have to see if we can improve
                //by finding a point that is more to the right
                //We also have to check if this point has colinear points
                for (int i = 0; i < points.Count; i++)
                {
                    Vector2 point = points[i];
                
                    //Dont test the point we picked randomly
                    //Or the point we are coming from which might happen when we move from the first point on the hull
                    if (point == nextPoint || point == currentPoint)
                    {
                        //counter += 1;    

                        continue;
                    }
                   
                    Vector2 testPoint = point;

                    //Where is the test point in relation to the line a-b
                    //  1 -> to the right
                    //  0 -> on the line
                    // -1 -> to the left
                    int relation = Geometry.GetPointPositionInRelationToLine(currentPoint, nextPoint, testPoint);

                    //The test point is on the line, so we have found a colinear point
                    if (relation == 0)
                    {
                        pointsToAddToTheHull.Add(testPoint);
                    }
                    //To the right = better point, so pick it as next point on the convex hull
                    else if (relation == 1)
                    {
                        nextPoint = testPoint;

                        //Clear colinear points because they belonged to the old point which was worse
                        pointsToAddToTheHull.Clear();

                        pointsToAddToTheHull.Add(nextPoint);
                    }
                    //To the left = worse point so do nothing
                }



                //Sort this list, so we can add the colinear points in correct order
                pointsToAddToTheHull = pointsToAddToTheHull.OrderBy(n => Vector2.SqrMagnitude(n - currentPoint)).ToList();

                convexHull.AddRange(pointsToAddToTheHull);

                //The next current point is the last point in this list
                currentPoint = pointsToAddToTheHull[pointsToAddToTheHull.Count - 1];

                //Remove the points that are now on the convex hull, which should speed up the algorithm
                for (int i = 0; i < pointsToAddToTheHull.Count; i++)
                {
                    points.Remove(pointsToAddToTheHull[i]);
                }



                //Have we found the first point on the hull? If so we have completed the hull
                if (currentPoint.Equals(convexHull[0]))
                {
                    //Then remove it because it is the same as the first point, and we want a convex hull with no duplicates
                    convexHull.RemoveAt(convexHull.Count - 1);

                    break;
                }


                //Safety
                if (counter > 10000)
                {
                    Debug.Log("Stuck in endless loop when generating convex hull with jarvis march");

                    break;
                }

                counter += 1;
            }

            

            return convexHull;
        }
    }
}
