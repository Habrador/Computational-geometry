using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Methods related to making holes in Ear Clipping algorithm
    public static class EarClippingHole
    {
        //Merge holes with hull so we get one big list of vertices we can triangulate
        public static void MergeHolesWithHull(List<MyVector2> verticesHull, List<MyVector2> verticesHole)
        {
            //Validate data
            if (verticesHole == null || verticesHole.Count <= 2)
            {
                Debug.Log("The hole doesn't have enough vertices");

                return;
            }


            //Find a vertex in the hole that can also see a vertex in the hull
            //Connect these vertices with two edges, and the hole is now a part of the hull with an invisible seam
            //between the hole and the hull

            //Step 1. Find the vertex in the hole which has the maximum x-value
            MyVector2 holeMaxXVert = verticesHole[0];

            int maxXPosInList = 0;

            for (int i = 1; i < verticesHole.Count; i++)
            {
                MyVector2 v = verticesHole[i];

                if (v.x > holeMaxXVert.x)
                {
                    holeMaxXVert = v;

                    maxXPosInList = i;
                }
            }


            //Step 2. Form a line going from this vertex towards (in x-direction) to a position outside of the hull
            float hullMaxX = verticesHull[0].x;

            for (int i = 1; i < verticesHull.Count; i++)
            {
                MyVector2 v = verticesHull[i];

                if (v.x > hullMaxX)
                {
                    hullMaxX = v.x;
                }
            }

            MyVector2 lineStart = holeMaxXVert;
            MyVector2 lineEnd = new MyVector2(hullMaxX, holeMaxXVert.y);


            //Step 3. Do line-line intersection to find the edge on the hull which is the closest to this vertex
            //The first and second point on the hull is defined as edge 0, and so on...
            int closestEdge = -1;

            float minDistanceSqr = Mathf.Infinity;

            for (int i = 0; i < verticesHull.Count; i++)
            {
                MyVector2 p1_hull = verticesHull[i];
                MyVector2 p2_hull = verticesHull[MathUtility.ClampListIndex(i + 1, verticesHull.Count)];

                bool isIntersecting = _Intersections.LineLine(lineStart, lineEnd, p1_hull, p2_hull, true);

                //Here we can maybe add a check if any of the vertices is on the line

                if (isIntersecting)
                {
                    MyVector2 intersectionPoint = _Intersections.GetLineLineIntersectionPoint(lineStart, lineEnd, p1_hull, p2_hull);

                    float distanceSqr = MyVector2.SqrDistance(lineStart, intersectionPoint);

                    if (distanceSqr < minDistanceSqr)
                    {
                        closestEdge = i;
                        minDistanceSqr = distanceSqr;
                    }
                }
            }

            //This means we couldn't find a closest edge
            if (closestEdge == -1)
            {
                Debug.Log("Couldn't find a closest edge");

                return;
            }


            //Step 4. The closest edge has two vertices. Pick then one with the highest x-value, which is the vertex
            //that should be visible from the hole
            MyVector2 p1 = verticesHull[closestEdge];
            MyVector2 p2 = verticesHull[MathUtility.ClampListIndex(closestEdge + 1, verticesHull.Count)];

            MyVector2 visibleVertex = p1;

            int visibleVertexListPos = closestEdge;

            if (p2.x > visibleVertex.x)
            {
                visibleVertex = p2;

                visibleVertexListPos += 1;
            }


            //Step 5. Modify the vertices list to add the hole at this visibleVertex

            //Reconfigure the hole list to start at the vertex with the largest 

            //Add to back of list
            for (int i = 0; i < maxXPosInList; i++)
            {
                verticesHole.Add(verticesHole[i]);
            }

            //Remove those we added to the back of the list
            verticesHole.RemoveRange(0, maxXPosInList);

            //Add the two extra vertices we need
            verticesHole.Add(verticesHole[0]);
            verticesHole.Add(visibleVertex);


            //Step 6. Merge the hole with the hull
            verticesHull.InsertRange(visibleVertexListPos + 1, verticesHole);

            Debug.Log(verticesHull.Count);
        }

    }
}
