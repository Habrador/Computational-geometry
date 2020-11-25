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
            MyVector2 hole_MaxX_Vert = verticesHole[0];

            int hole_MaxX_ListPos = 0;

            for (int i = 1; i < verticesHole.Count; i++)
            {
                MyVector2 v = verticesHole[i];

                if (v.x > hole_MaxX_Vert.x)
                {
                    hole_MaxX_Vert = v;

                    hole_MaxX_ListPos = i;
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

            MyVector2 lineStart = hole_MaxX_Vert;
            //Just add some value so we know we are outside
            MyVector2 lineEnd = new MyVector2(hullMaxX + 0.1f, hole_MaxX_Vert.y);


            //Step 3. Do line-line intersection to find the edge on the hull which is the closest to this vertex
            //The first and second point on the hull is defined as edge 0, and so on...
            int closestEdge = -1;

            float minDistanceSqr = Mathf.Infinity;

            MyVector2 intersectionVertex = new MyVector2(-1f, -1f);

            for (int i = 0; i < verticesHull.Count; i++)
            {
                MyVector2 p1_hull = verticesHull[i];
                MyVector2 p2_hull = verticesHull[MathUtility.ClampListIndex(i + 1, verticesHull.Count)];

                bool isIntersecting = _Intersections.LineLine(lineStart, lineEnd, p1_hull, p2_hull, true);

                //Here we can maybe add a check if any of the vertices is on the line

                if (isIntersecting)
                {
                    intersectionVertex = _Intersections.GetLineLineIntersectionPoint(lineStart, lineEnd, p1_hull, p2_hull);

                    float distanceSqr = MyVector2.SqrDistance(lineStart, intersectionVertex);

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
                Debug.Log("Couldn't find a closest edge to hole");

                return;
            }


            //Step 4. The closest edge has two vertices. Pick the one with the highest x-value, which is the vertex
            //that should be visible from the hole
            MyVector2 p1 = verticesHull[closestEdge];
            MyVector2 p2 = verticesHull[MathUtility.ClampListIndex(closestEdge + 1, verticesHull.Count)];

            MyVector2 visibleVertex = p1;

            if (p2.x > visibleVertex.x)
            {
                visibleVertex = p2;
            }


            //Step 4.5. But the hull may intersect with this edge between the point on the hole and the point on the hull, 
            //so the point on the hull might not be visible
            //To be sure, we form a triangle and according to litterature, we check if an reflect vertices are within this triangle
            //If so, one of them will be visible
            Triangle2 t = new Triangle2(hole_MaxX_Vert, intersectionVertex, visibleVertex);

            List<MyVector2> reflectVertices = new List<MyVector2>();

            for (int i = 0; i < verticesHull.Count; i++)
            {
                MyVector2 p_prev = verticesHull[MathUtility.ClampListIndex(i - 1, verticesHull.Count)];

                MyVector2 p = verticesHull[i];
                
                MyVector2 p_next = verticesHull[MathUtility.ClampListIndex(i + 1, verticesHull.Count)];

                if (!EarClipping.IsVertexConvex(p_prev, p, p_next))
                {
                    reflectVertices.Add(p);
                }
            }


            MyVector2 actualVisibleVertex = visibleVertex;

            float minAngle = Mathf.Infinity;

            foreach (MyVector2 v in reflectVertices)
            {
                if (_Intersections.PointTriangle(t, v, includeBorder: true))
                {
                    float angle = MathUtility.AngleBetween(v - hole_MaxX_Vert, v - intersectionVertex);

                    if (angle < minAngle)
                    {
                        minAngle = angle;

                        actualVisibleVertex = v;
                    }
                }
            }

            //Debug.DrawLine(actualVisibleVertex.ToVector3(), Vector3.zero, Color.blue, 2f);

            //Step 5. Modify the vertices list to add the hole at this visibleVertex

            //Reconfigure the hole list to start at the vertex with the largest 

            //Add to back of list
            for (int i = 0; i < hole_MaxX_ListPos; i++)
            {
                verticesHole.Add(verticesHole[i]);
            }

            //Remove those we added to the back of the list
            verticesHole.RemoveRange(0, hole_MaxX_ListPos);

            //Add the two extra vertices we need
            verticesHole.Add(verticesHole[0]);
            verticesHole.Add(actualVisibleVertex);


            //Step 6. Merge the hole with the hull
            int hull_VisibleVertex_ListPos = -100;

            for (int i = 0; i < verticesHull.Count; i++)
            {
                if (actualVisibleVertex.Equals(verticesHull[i]))
                {
                    hull_VisibleVertex_ListPos = i;

                    break;
                }
            }

            if (hull_VisibleVertex_ListPos == -100)
            {
                Debug.Log("Cant find corresponding pos in list");

                return;
            }

            verticesHull.InsertRange(hull_VisibleVertex_ListPos + 1, verticesHole);

            Debug.Log(verticesHull.Count);
        }

    }
}
