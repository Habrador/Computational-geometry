using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Habrador_Computational_Geometry
{
    //Methods related to making holes in Ear Clipping algorithm
    public static class EarClippingHoleMethods
    {
        //Merge holes with hull so we get one big list of vertices we can triangulate
        //We merge by creating "bridges" between the holes and the hull
        public static List<MyVector2> MergeHolesWithHull(List<MyVector2> verticesHull, List<List<MyVector2>> allHoleVertices)
        {
            //Validate
            if (allHoleVertices == null || allHoleVertices.Count == 0)
            {
                return null;
            }


            //Change data structure
            EarClippingPolygon hull = new EarClippingPolygon(new Polygon2(verticesHull));

            List<EarClippingPolygon> holes = new List<EarClippingPolygon>();

            int counter = 1;

            foreach (List<MyVector2> hole in allHoleVertices)
            {
                //Validate data
                if (hole == null || hole.Count <= 2)
                {
                    Debug.Log("The hole doesn't have enough vertices");

                    continue;
                }

                EarClippingPolygon holePolygon = new EarClippingPolygon(new Polygon2(hole));

                holes.Add(holePolygon);

                holePolygon.id = counter;

                counter++;
            }


            //Sort the holes by their max x-value coordinate, from highest to lowest
            holes = holes.OrderByDescending(o => o.maxX_Vert.x).ToList();


            //Merge the holes with the hull so we get a hull with seams that we can triangulate like a hull without holes
            foreach (EarClippingPolygon hole in holes)
            {
                MergeHoleWithHull(hull, hole);
            }


            return hull.Vertices;
        }



        //Merge a single hole with the hull
        //Basic idea is to find a vertex in the hole that can also see a vertex in the hull
        //Connect these vertices with two edges, and the hole is now a part of the hull with an invisible seam
        //between the hole and the hull
        private static void MergeHoleWithHull(EarClippingPolygon hull, EarClippingPolygon hole)
        {
            //Step 1. Find the vertex in the hole which has the maximum x-value
            //Has already been done when we created the data structure


            //Step 2. Form a line going from this vertex towards (in x-direction) to a position outside of the hull
            MyVector2 lineStart = hole.maxX_Vert;
            //Just add some value so we know we are outside
            MyVector2 lineEnd = new MyVector2(hull.maxX_Vert.x + 0.01f, hole.maxX_Vert.y);
            //Important to add lineStart first because we will need it later
            Edge2 line_hole_to_outside = new Edge2(lineStart, lineEnd);


            //Step 3. Find a vertex on the hull which is visible to the point on the hole with max x pos
            //The first and second point on the hull is defined as edge 0, and so on...
            int closestEdge = -1;

            MyVector2 visibleVertex;

            FindVisibleVertexOnHUll(hull, hole, line_hole_to_outside, out closestEdge, out visibleVertex);

            //This means we couldn't find a closest edge
            if (closestEdge == -1)
            {
                Debug.Log("Couldn't find a closest edge to hole");

                return;
            }


            //Step 4. Modify the hull vertices so we get an edge from the hull to the hole, around the hole, and back to the hull

            //First reconfigure the hole list to start at the vertex with the largest x pos
            //[a, b, c, d, e] and c is the one with the largest x pos, we get:
            //[a, b, c, d, e, a, b]
            //[c, d, e, a, b]
            //We also need two extra vertices, one from the hole and one from the hull
            //If p is the visible vertex, we get:
            //[c, d, e, a, b, c, p]

            //Maybe more efficient if we turn the list into a queue?

            //Add to back of list
            for (int i = 0; i < hole.maxX_ListPos; i++)
            {
                hole.Vertices.Add(hole.Vertices[i]);
            }

            //Remove those we added to the back of the list
            hole.Vertices.RemoveRange(0, hole.maxX_ListPos);

            //Add the two extra vertices we need
            hole.Vertices.Add(hole.Vertices[0]);
            hole.Vertices.Add(visibleVertex);


            //Merge the hole with the hull
            List<MyVector2> verticesHull = hull.Vertices;

            //Find where we should insert the hole
            int visibleVertex_ListPos = hull.GetLastListPos(visibleVertex);

            if (visibleVertex_ListPos == -1)
            {
                Debug.Log("Cant find corresponding pos in list");

                return;
            }

            //Insert the hole after the visible vertex
            verticesHull.InsertRange(visibleVertex_ListPos + 1, hole.Vertices);

            //Debug.Log($"Number of vertices on the hull after adding a hole: {verticesHull.Count}");
        }



        //Find a vertex on the hull that should be visible from the hole
        private static void FindVisibleVertexOnHUll(EarClippingPolygon hull, EarClippingPolygon hole, Edge2 line_hole_to_outside, out int closestEdge, out MyVector2 visibleVertex)
        {
            //The first and second point on the hull is defined as edge 0, and so on...
            closestEdge = -1;
            //The vertex that should be visible to the hole (which is the max of the line that's intersecting with the line)
            visibleVertex = new MyVector2(-1f, -1f);


            //Do line-line intersection to find intersectionVertex which is the point of intersection that's the closest to the hole
            MyVector2 intersectionVertex = new MyVector2(-1f, -1f);

            float minDistanceSqr = Mathf.Infinity;

            List<MyVector2> verticesHull = hull.Vertices;

            for (int i = 0; i < verticesHull.Count; i++)
            {
                MyVector2 p1_hull = verticesHull[i];
                MyVector2 p2_hull = verticesHull[MathUtility.ClampListIndex(i + 1, verticesHull.Count)];

                //We dont need to check this line if it's to the left of the point on the hole
                //If so they cant intersect
                if (p1_hull.x < hole.maxX_Vert.x && p2_hull.x < hole.maxX_Vert.x)
                {
                    continue;
                }

                Edge2 line_hull = new Edge2(p1_hull, p2_hull);

                bool isIntersecting = _Intersections.LineLine(line_hole_to_outside, line_hull, includeEndPoints: true);

                //Here we can maybe add a check if any of the vertices is on the line???

                if (isIntersecting)
                {
                    MyVector2 testIntersectionVertex = _Intersections.GetLineLineIntersectionPoint(line_hole_to_outside, line_hull);

                    //if (hole.id == 3) TestAlgorithmsHelpMethods.DebugDrawCircle(testIntersectionVertex.ToVector3(), 0.2f, Color.green);

                    float distanceSqr = MyVector2.SqrDistance(line_hole_to_outside.p1, testIntersectionVertex);

                    if (distanceSqr < minDistanceSqr)
                    {
                        closestEdge = i;
                        minDistanceSqr = distanceSqr;

                        intersectionVertex = testIntersectionVertex;
                    }
                }
            }

            //This means we couldn't find a closest edge
            if (closestEdge == -1)
            {
                Debug.Log("Couldn't find a closest edge to hole");

                return;
            }


            //But we can't connect the hole with this intersection point, so we need to find a vertex which is visible from the hole
            //The closest edge has two vertices. Pick the one with the highest x-value, which is the vertex
            //that should be visible from the hole
            MyVector2 p1 = hull.Vertices[closestEdge];
            MyVector2 p2 = hull.Vertices[MathUtility.ClampListIndex(closestEdge + 1, hull.Vertices.Count)];

            visibleVertex = p1;

            //They are the same so pick the one that is the closest
            if (Mathf.Abs(p1.x - p2.x) < MathUtility.EPSILON)
            {
                float hole_p1 = MyVector2.SqrDistance(hole.maxX_Vert, p1);
                float hole_p2 = MyVector2.SqrDistance(hole.maxX_Vert, p2);

                visibleVertex = hole_p1 < hole_p2 ? p1 : p2;
            }
            else if (p2.x > visibleVertex.x)
            {
                visibleVertex = p2;
            }

            if (hole.id == 3)
            {
                //TestAlgorithmsHelpMethods.DebugDrawCircle(intersectionVertex.ToVector3(), 0.4f, Color.black);
                //TestAlgorithmsHelpMethods.DebugDrawCircle(visibleVertex.ToVector3(), 0.4f, Color.green);
            }

            //But the hull may still intersect with this edge between the point on the hole and the visible point on the hull, 
            //so the visible point on the hull might not be visible after all
            //So we might have to find a new point which is visible
            FindActualVisibleVertexOnHull(hull, hole, intersectionVertex, ref visibleVertex);
        }



        //The hull may still intersect with the edge between the point on the hole and the "visible" point on the hull, 
        //so the point on the hull might not be visible, so we should try to find a better point
        private static void FindActualVisibleVertexOnHull(EarClippingPolygon hull, EarClippingPolygon hole, MyVector2 intersectionVertex, ref MyVector2 visibleVertex)
        {
            //Form a triangle
            Triangle2 t = new Triangle2(hole.maxX_Vert, intersectionVertex, visibleVertex);

            //According to litterature, we check if a reflect vertex is within this triangle
            //If so, one of them is a better visible vertex on the hull
            List<MyVector2> reflectVertices = FindReflectVertices(hull, hole);

            //Pick the reflect vertex with the smallest angle 
            //The angle is measure from the point on the hole towards:
            //- intersection point on the hull
            //- reflect vertex
            float minAngle = Mathf.Infinity;

            //If more than one reflect vertex have the same angle then pick the one closest to the point on the hole
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
                    }
                }
            }

            //Will show how the holes are connected with the hull
            //Debug.DrawLine(visibleVertex.ToVector3(1f), hole.maxX_Vert.ToVector3(1f), Color.red, 5f);

            //TestAlgorithmsHelpMethods.DebugDrawCircle(visibleVertex.ToVector3(1f), 0.3f, Color.red);
            //TestAlgorithmsHelpMethods.DebugDrawCircle(hole.maxX_Vert.ToVector3(1f), 0.3f, Color.red);
        }



        //Find reflect vertices (that also have a higher x pos than the hole)
        private static List<MyVector2> FindReflectVertices(EarClippingPolygon hull, EarClippingPolygon hole)
        {
            List<MyVector2> reflectVertices = new List<MyVector2>();


            List<MyVector2> verticesHull = hull.Vertices;

            for (int i = 0; i < verticesHull.Count; i++)
            {
                MyVector2 p = verticesHull[i];

                //We dont need to check this vertex if it's to the left of the point on the hull
                //because that vertex can't be visible
                if (p.x < hole.maxX_Vert.x)
                {
                    continue;
                }

                MyVector2 p_prev = verticesHull[MathUtility.ClampListIndex(i - 1, verticesHull.Count)];

                MyVector2 p_next = verticesHull[MathUtility.ClampListIndex(i + 1, verticesHull.Count)];

                if (!_EarClipping.IsVertexConvex(p_prev, p, p_next))
                {
                    reflectVertices.Add(p);
                }
            }


            return reflectVertices;
        }
    }
}
