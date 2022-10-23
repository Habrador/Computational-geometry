using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Which triangle in a triangulation is a point intersecting with?
    public static class PointTriangulationIntersection
    {
        //
        // Alternative 1. Search through all triangles and use point-in-triangle
        //

        //Simple but slow
        public static HalfEdgeFace2 BruteForce(MyVector2 p, HalfEdgeData2 triangulationData)
        {
            HalfEdgeFace2 intersectingTriangle = null;

            foreach (HalfEdgeFace2 f in triangulationData.faces)
            {
                //The corners of this triangle
                MyVector2 v1 = f.edge.v.position;
                MyVector2 v2 = f.edge.nextEdge.v.position;
                MyVector2 v3 = f.edge.nextEdge.nextEdge.v.position;

                Triangle2 t = new Triangle2(v1, v2, v3);

                //Is the point in this triangle?
                if (_Intersections.PointTriangle(t, p, true))
                {
                    intersectingTriangle = f;

                    break;
                }
            }

            return intersectingTriangle;
        }



        //
        // Alternative 2. Triangulation walk
        //
        
        //Fast but a little more complicated to understand
        //We can also give it a list, which should be empty so we can display the triangulation walk
        public static HalfEdgeFace2 TriangulationWalk(MyVector2 p, HalfEdgeFace2 startTriangle, HalfEdgeData2 triangulationData, List<HalfEdgeFace2> visitedTriangles = null)
        {
            HalfEdgeFace2 intersectingTriangle = null;


            //If we have a triangle to start in which may speed up the algorithm
            HalfEdgeFace2 currentTriangle = null;

            //We can feed it a start triangle to sometimes make the algorithm faster
            if (startTriangle != null)
            {
                currentTriangle = startTriangle;
            }
            //Find a random start triangle which is faster than starting at the first triangle?
            else
            {
                int randomPos = Random.Range(0, triangulationData.faces.Count);

                int i = 0;

                //faces are stored in a hashset so we have to loop through them while counting
                //to find the start triangle
                foreach (HalfEdgeFace2 f in triangulationData.faces)
                {
                    if (i == randomPos)
                    {
                        currentTriangle = f;

                        break;
                    }

                    i += 1;
                }
            }

            if (currentTriangle == null)
            {
                Debug.Log("Couldnt find start triangle when walking in triangulation");

                return null;
            }

            if (visitedTriangles != null)
            {
                visitedTriangles.Add(currentTriangle);
            }
            


            //Start the triangulation walk to find the intersecting triangle
            int safety = 0;

            while (true)
            {
                safety += 1;

                if (safety > 1000000)
                {
                    Debug.Log("Stuck in endless loop when walking in triangulation");

                    break;
                }

                //Is the point intersecting with the current triangle?
                //We need to do 3 tests where each test is using the triangles edges
                //If the point is to the right of all edges, then it's inside the triangle
                //If the point is to the left we jump to that triangle instead
                HalfEdge2 e1 = currentTriangle.edge;
                HalfEdge2 e2 = e1.nextEdge;
                HalfEdge2 e3 = e2.nextEdge;


                //Test 1
                if (IsPointToTheRightOrOnLine(e1.prevEdge.v.position, e1.v.position, p))
                {
                    //Test 2
                    if (IsPointToTheRightOrOnLine(e2.prevEdge.v.position, e2.v.position, p))
                    {
                        //Test 3
                        if (IsPointToTheRightOrOnLine(e3.prevEdge.v.position, e3.v.position, p))
                        {
                            //We have found the triangle the point is in
                            intersectingTriangle = currentTriangle;

                            break;
                        }
                        //If to the left, move to this triangle
                        else
                        {
                            currentTriangle = e3.oppositeEdge.face;
                        }
                    }
                    //If to the left, move to this triangle
                    else
                    {
                        currentTriangle = e2.oppositeEdge.face;
                    }
                }
                //If to the left, move to this triangle
                else
                {
                    currentTriangle = e1.oppositeEdge.face;
                }


                if (visitedTriangles != null)
                {
                    visitedTriangles.Add(currentTriangle);
                }
            }


            //Add the last triangle if we found it
            if (visitedTriangles != null && intersectingTriangle != null)
            {
                visitedTriangles.Add(intersectingTriangle);
            }

            return intersectingTriangle;
        }

        //Help method to make code smaller
        //Is p to the right or on the line a-b
        private static bool IsPointToTheRightOrOnLine(MyVector2 a, MyVector2 b, MyVector2 p)
        {
            bool isToTheRight = false;

            LeftOnRight pointPos = _Geometry.IsPoint_Left_On_Right_OfVector(a, b, p);

            if (pointPos == LeftOnRight.Right || pointPos == LeftOnRight.On)
            {
                isToTheRight = true;
            }

            return isToTheRight;
        }
    }
}
