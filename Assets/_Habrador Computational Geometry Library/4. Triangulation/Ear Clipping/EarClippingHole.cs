using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Habrador_Computational_Geometry
{
    //Methods related to making holes in Ear Clipping algorithm
    public static class EarClippingHole
    {
        //Merge holes with hull so we get one big list of vertices we can triangulate
        public static void MergeHolesWithHull(List<MyVector2> verticesHull, List<List<MyVector2>> allHoleVertices)
        {
            //Validate
            if (allHoleVertices == null || allHoleVertices.Count == 0)
            {
                return;
            }
        
        
            //Change data structure
            List<ConnectedVertices> holes = new List<ConnectedVertices>();

            foreach (List<MyVector2> hole in allHoleVertices)
            {
                //Validate data
                if (hole == null || hole.Count <= 2)
                {
                    Debug.Log("The hole doesn't have enough vertices");

                    continue;
                }

                ConnectedVertices connectedVerts = new ConnectedVertices(hole);

                holes.Add(connectedVerts);
            }


            //Sort the holes by their max x-value, from highest to lowest
            holes = holes.OrderByDescending(o => o.maxX_Vert.x).ToList();

            foreach (ConnectedVertices hole in holes)
            {
                MergeHoleWithHull(verticesHull, hole);
            }
        }



        //Merge a single hole with the hull
        //Basic idea is to find a vertex in the hole that can also see a vertex in the hull
        //Connect these vertices with two edges, and the hole is now a part of the hull with an invisible seam
        //between the hole and the hull
        private static void MergeHoleWithHull(List<MyVector2> verticesHull, ConnectedVertices hole)
        {
            //Step 1. Find the vertex in the hole which has the maximum x-value
            //Has already been done when we created the data structure


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

            MyVector2 lineStart = hole.maxX_Vert;
            //Just add some value so we know we are outside
            MyVector2 lineEnd = new MyVector2(hullMaxX + 0.1f, hole.maxX_Vert.y);


            //Step 3. Do line-line intersection to find the edge on the hull which is the closest to this vertex
            //The first and second point on the hull is defined as edge 0, and so on...
            int closestEdge = -1;

            float minDistanceSqr = Mathf.Infinity;

            MyVector2 intersectionVertex = new MyVector2(-1f, -1f);

            for (int i = 0; i < verticesHull.Count; i++)
            {
                MyVector2 p1_hull = verticesHull[i];
                MyVector2 p2_hull = verticesHull[MathUtility.ClampListIndex(i + 1, verticesHull.Count)];

                //We dont need to check this line if its to the left of the point on the hole
                //If so they cant intersect
                if (p1_hull.x < hole.maxX_Vert.x && p2_hull.x < hole.maxX_Vert.x)
                {
                    continue;
                }

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
            Triangle2 t = new Triangle2(hole.maxX_Vert, intersectionVertex, visibleVertex);

            List<MyVector2> reflectVertices = new List<MyVector2>();

            for (int i = 0; i < verticesHull.Count; i++)
            {
                MyVector2 p = verticesHull[i];

                //We dont need to check this vertex if its to the left of the point on the hull
                //because that vertex can't be within the triangle
                if (p.x < hole.maxX_Vert.x)
                {
                    continue;
                }

                MyVector2 p_prev = verticesHull[MathUtility.ClampListIndex(i - 1, verticesHull.Count)];

                MyVector2 p_next = verticesHull[MathUtility.ClampListIndex(i + 1, verticesHull.Count)];

                //Here we have to ignore colinear points, which need to be reflect when triangulating, but are giving an error here
                if (!EarClipping.IsVertexConvex(p_prev, p, p_next, isColinearPointsConcave: false))
                {
                    reflectVertices.Add(p);
                }
            }



            float minAngle = Mathf.Infinity;

            float minDistSqr = Mathf.Infinity;

            foreach (MyVector2 v in reflectVertices)
            {
                if (_Intersections.PointTriangle(t, v, includeBorder: true))
                {
                    float angle = MathUtility.AngleBetween(intersectionVertex - hole.maxX_Vert, v - hole.maxX_Vert);

                    //Debug.DrawLine(v.ToVector3(1f), hole.maxX_Vert.ToVector3(1f), Color.blue, 2f);

                    //Debug.DrawLine(intersectionVertex.ToVector3(1f), hole.maxX_Vert.ToVector3(1f), Color.black, 2f);

                    //TestAlgorithmsHelpMethods.DebugDrawCircle(v.ToVector3(1f), 0.3f, Color.blue);

                    //Debug.Log(angle * Mathf.Rad2Deg);

                    if (angle < minAngle)
                    {
                        minAngle = angle;

                        visibleVertex = v;

                        //We also need to calculate this in case a future point has the same angle
                        minDistSqr = MyVector2.SqrDistance(v, hole.maxX_Vert);

                        //Debug.Log(minDistanceSqr);

                        //TestAlgorithmsHelpMethods.DebugDrawCircle(v.ToVector3(1f), 0.3f, Color.green);
                    }
                    //If the angle is the same, then pick the vertex which is the closest to the point on the hull
                    else if (Mathf.Abs(angle - minAngle) < MathUtility.EPSILON)
                    {
                        float distSqr = MyVector2.SqrDistance(v, hole.maxX_Vert);


                        //Debug.Log(minDistanceSqr);


                        if (distSqr < minDistSqr)
                        {
                            visibleVertex = v;

                            minDistSqr = distSqr;

                            //TestAlgorithmsHelpMethods.DebugDrawCircle(v.ToVector3(1f), 0.3f, Color.red);

                            //Debug.Log(distSqr);
                        }

                        //Debug.Log("Hello");
                    }
                }
            }

            Debug.DrawLine(visibleVertex.ToVector3(1f), hole.maxX_Vert.ToVector3(1f), Color.red, 2f);

            TestAlgorithmsHelpMethods.DebugDrawCircle(visibleVertex.ToVector3(1f), 0.3f, Color.red);

            //Step 5. Modify the vertices list to add the hole at this visibleVertex

            //Reconfigure the hole list to start at the vertex with the largest 

            //Add to back of list
            for (int i = 0; i < hole.maxX_ListPos; i++)
            {
                hole.vertices.Add(hole.vertices[i]);
            }

            //Remove those we added to the back of the list
            hole.vertices.RemoveRange(0, hole.maxX_ListPos);

            //Add the two extra vertices we need
            hole.vertices.Add(hole.vertices[0]);
            hole.vertices.Add(visibleVertex);


            //Step 6. Merge the hole with the hull
            int hull_VisibleVertex_ListPos = -100;

            for (int i = 0; i < verticesHull.Count; i++)
            {
                if (visibleVertex.Equals(verticesHull[i]))
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

            verticesHull.InsertRange(hull_VisibleVertex_ListPos + 1, hole.vertices);

            Debug.Log($"Number of vertices on the hull after adding a hole: {verticesHull.Count}");
        }

    }
}
