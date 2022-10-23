using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Generate a convex hull in 3d space with an iterative algorithm (also known as beneath-beyond)
    //Is very similar to Quickhull
    //Based on "Computational Geometry in C" by Joseph O'Rourke
    //and "Implementing Quickhull" pdf from Valve by Dirk Gregorious
    public static class IterativeHullAlgorithm3D
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="points">The points from which we want to build the convex hull</param>
        /// <param name="removeUnwantedTriangles">At the end of the algorithm, try to remove triangles from the hull that we dont want, 
        //such as needles where one edge is much shorter than the other edges in the triangle</param>
        /// <param name="normalizer">Is only needed for debugging</param>
        /// <returns></returns>
        public static HalfEdgeData3 GenerateConvexHull(HashSet<MyVector3> points, bool removeUnwantedTriangles, Normalizer3 normalizer = null)
        {
            HalfEdgeData3 convexHull = new HalfEdgeData3();

            //Step 1. Init by making a tetrahedron (triangular pyramid) and remove all points within the tetrahedron
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

            //timer.Start();

            BuildFirstTetrahedron(points, convexHull);

            //timer.Stop();

            //Debug.Log($"Testrahedron {timer.ElapsedMilliseconds/1000f}");

            //Debug.Log(convexHull.faces.Count);

            //return convexHull;

            //Step 2. For each other point: 
            // -If the point is within the hull constrcuted so far, remove it
            // - Otherwise, see which triangles are visible to the point and remove them
            //   Then build new triangles from the edges that have no neighbor to the point

            List<MyVector3> pointsToAdd = new List<MyVector3>(points);

            int removedPointsCounter = 0;

            //int debugCounter = 0;

            foreach (MyVector3 p in pointsToAdd)
            {
                //Is this point within the tetrahedron
                bool isWithinHull = _Intersections.PointWithinConvexHull(p, convexHull);

                if (isWithinHull)
                {
                    points.Remove(p);

                    removedPointsCounter += 1;

                    continue;
                }


                //Find visible triangles and edges on the border between the visible and invisible triangles
                HashSet<HalfEdgeFace3> visibleTriangles = null;
                HashSet<HalfEdge3> borderEdges = null;

                FindVisibleTrianglesAndBorderEdgesFromPoint(p, convexHull, out visibleTriangles, out borderEdges);
                

                //Remove all visible triangles
                foreach (HalfEdgeFace3 triangle in visibleTriangles)
                {
                    convexHull.DeleteFace(triangle);
                }


                //Make new triangle by connecting all edges on the border with the point 
                //Debug.Log($"Number of border edges: {borderEdges.Count}");
                //int debugStop = 11;

                //Save all ned edges so we can connect them with an opposite edge
                //To make it faster you can use the ideas in the Valve paper to get a sorted list of newEdges
                HashSet<HalfEdge3> newEdges = new HashSet<HalfEdge3>(); 

                foreach(HalfEdge3 borderEdge in borderEdges)
                {
                    //Each edge is point TO a vertex
                    MyVector3 p1 = borderEdge.prevEdge.v.position;
                    MyVector3 p2 = borderEdge.v.position;

                    /*
                    if (debugCounter > debugStop)
                    {
                        Debug.DrawLine(normalizer.UnNormalize(p1).ToVector3(), normalizer.UnNormalize(p2).ToVector3(), Color.white, 2f);

                        Debug.DrawLine(normalizer.UnNormalize(p1).ToVector3(), normalizer.UnNormalize(p).ToVector3(), Color.gray, 2f);
                        Debug.DrawLine(normalizer.UnNormalize(p2).ToVector3(), normalizer.UnNormalize(p).ToVector3(), Color.gray, 2f);

                        convexHull.AddTriangle(p2, p1, p);
                    }
                    else
                    {
                        //Debug.Log(borderEdge.face);

                        convexHull.AddTriangle(p2, p1, p);
                    }
                    */

                    //The border edge belongs to a triangle which is invisible
                    //Because triangles are oriented clockwise, we have to add the vertices in the other direction
                    //to build a new triangle with the point
                    HalfEdgeFace3 newTriangle = convexHull.AddTriangle(p2, p1, p);

                    //Connect the new triangle with the opposite edge on the border
                    //When we create the face we give it a reference edge which goes to p2
                    //So the edge we want to connect is the next edge
                    HalfEdge3 edgeToConnect = newTriangle.edge.nextEdge;

                    edgeToConnect.oppositeEdge = borderEdge;
                    borderEdge.oppositeEdge = edgeToConnect;

                    //Two edges are still not connected, so save those
                    HalfEdge3 e1 = newTriangle.edge;
                    //HalfEdge3 e2 = newTriangle.edge.nextEdge;
                    HalfEdge3 e3 = newTriangle.edge.nextEdge.nextEdge;
                    
                    newEdges.Add(e1);
                    //newEdges.Add(e2);
                    newEdges.Add(e3);
                }

                //timer.Start();
                //Two edges in each triangle are still not connected with an opposite edge
                foreach (HalfEdge3 e in newEdges)
                {
                    if (e.oppositeEdge != null)
                    {
                        continue;
                    }

                    convexHull.TryFindOppositeEdge(e, newEdges);
                }
                //timer.Stop();


                //Connect all new triangles and the triangles on the border, 
                //so each edge has an opposite edge or flood filling will be impossible
                //timer.Start();
                //convexHull.ConnectAllEdges();
                //timer.Stop();


                //if (debugCounter > debugStop)
                //{
                //    break;
                //}

                //debugCounter += 1;
            }

            //Debug.Log($"Connect half-edges took {timer.ElapsedMilliseconds/1000f} seconds");

            Debug.Log($"Removed {removedPointsCounter} points during the construction of the hull because they were inside the hull");


            //
            // Clean up 
            //

            //Merge concave edges according to the paper


            //Remove unwanted triangles, such as slivers and needles
            //Which is maybe not needed because when you add a Unity convex mesh collider to the result of this algorithm, there are still slivers
            //Unity's mesh collider is also using quads and not just triangles
            //But if you add enough points, so you end up with many points on the hull you can see that Unitys convex mesh collider is not capturing all points, so they must be using some simplification algorithm

            //Run the hull through the mesh simplification algorithm
            if (removeUnwantedTriangles)
            {
                convexHull = MeshSimplification_QEM.Simplify(convexHull, maxEdgesToContract: int.MaxValue, maxError: 0.0001f, normalizeTriangles: true);
            }
                
            
            return convexHull;
        }



        //Find all visible triangles from a point
        //Also find edges on the border between invisible and visible triangles
        public static void FindVisibleTrianglesAndBorderEdgesFromPoint(MyVector3 p, HalfEdgeData3 convexHull, out HashSet<HalfEdgeFace3> visibleTriangles, out HashSet<HalfEdge3> borderEdges)
        {
            //Flood-fill from the visible triangle to find all other visible triangles
            //When you cross an edge from a visible triangle to an invisible triangle, 
            //save the edge because thhose edge should be used to build triangles with the point
            //These edges should belong to the triangle which is not visible
            borderEdges = new HashSet<HalfEdge3>();

            //Store all visible triangles here so we can't visit triangles multiple times
            visibleTriangles = new HashSet<HalfEdgeFace3>();


            //Start the flood-fill by finding a triangle which is visible from the point
            //A triangle is visible if the point is outside the plane formed at the triangles
            //Another sources is using the signed volume of a tetrahedron formed by the triangle and the point
            HalfEdgeFace3 visibleTriangle = FindVisibleTriangleFromPoint(p, convexHull.faces);

            //If we didn't find a visible triangle, we have some kind of edge case and should move on for now
            if (visibleTriangle == null)
            {
                Debug.LogWarning("Couldn't find a visible triangle so will ignore the point");

                return;
            }


            //The queue which we will use when flood-filling
            Queue<HalfEdgeFace3> trianglesToFloodFrom = new Queue<HalfEdgeFace3>();

            //Add the first triangle to init the flood-fill 
            trianglesToFloodFrom.Enqueue(visibleTriangle);

            List<HalfEdge3> edgesToCross = new List<HalfEdge3>();

            int safety = 0;

            while (true)
            {
                //We have visited all visible triangles
                if (trianglesToFloodFrom.Count == 0)
                {
                    break;
                }

                HalfEdgeFace3 triangleToFloodFrom = trianglesToFloodFrom.Dequeue();

                //This triangle is always visible and should be deleted
                visibleTriangles.Add(triangleToFloodFrom);

                //Investigate bordering triangles
                edgesToCross.Clear();

                edgesToCross.Add(triangleToFloodFrom.edge);
                edgesToCross.Add(triangleToFloodFrom.edge.nextEdge);
                edgesToCross.Add(triangleToFloodFrom.edge.nextEdge.nextEdge);

                //Jump from this triangle to a bordering triangle
                foreach (HalfEdge3 edgeToCross in edgesToCross)
                {
                    HalfEdge3 oppositeEdge = edgeToCross.oppositeEdge;

                    if (oppositeEdge == null)
                    {
                        Debug.LogWarning("Found an opposite edge which is null");

                        break;
                    }

                    HalfEdgeFace3 oppositeTriangle = oppositeEdge.face;

                    //Have we visited this triangle before (only test visible triangles)?
                    if (trianglesToFloodFrom.Contains(oppositeTriangle) || visibleTriangles.Contains(oppositeTriangle))
                    {
                        continue;
                    }

                    //Check if this triangle is visible
                    //A triangle is visible from a point the point is outside of a plane formed with the triangles position and normal 
                    Plane3 plane = new Plane3(oppositeTriangle.edge.v.position, oppositeTriangle.edge.v.normal);

                    bool isPointOutsidePlane = _Geometry.IsPointOutsidePlane(p, plane);

                    //This triangle is visible so save it so we can flood from it
                    if (isPointOutsidePlane)
                    {
                        trianglesToFloodFrom.Enqueue(oppositeTriangle);
                    }
                    //This triangle is invisible. Since we only flood from visible triangles, 
                    //it means we crossed from a visible triangle to an invisible triangle, so save the crossing edge
                    else
                    {
                        borderEdges.Add(oppositeEdge);
                    }
                }


                safety += 1;

                if (safety > 50000)
                {
                    Debug.Log("Stuck in infinite loop when flood-filling visible triangles");

                    break;
                }
            }
        }



        //Find a visible triangle from a point
        private static HalfEdgeFace3 FindVisibleTriangleFromPoint(MyVector3 p, HashSet<HalfEdgeFace3> triangles)
        {
            HalfEdgeFace3 visibleTriangle = null;

            foreach (HalfEdgeFace3 triangle in triangles)
            {
                //A triangle is visible from a point the point is outside of a plane formed with the triangles position and normal 
                Plane3 plane = new Plane3(triangle.edge.v.position, triangle.edge.v.normal);

                bool isPointOutsidePlane = _Geometry.IsPointOutsidePlane(p, plane);

                //We have found a triangle which is visible from the point and should be removed
                if (isPointOutsidePlane)
                {
                    visibleTriangle = triangle;

                    break;
                }
            }

            return visibleTriangle;
        }



        //Initialize by making 2 triangles by using three points, so its a flat triangle with a face on each side
        //We could use the ideas from Quickhull to make the start triangle as big as possible
        //Then find a point which is the furthest away as possible from these triangles
        //Add that point and you have a tetrahedron (triangular pyramid)
        public static void BuildFirstTetrahedron(HashSet<MyVector3> points, HalfEdgeData3 convexHull)
        {
            //Of all points, find the two points that are furthes away from each other
            Edge3 eFurthestApart = FindEdgeFurthestApart(points);

            //Remove the two points we found         
            points.Remove(eFurthestApart.p1);
            points.Remove(eFurthestApart.p2);


            //Find a point which is the furthest away from this edge
            //TODO: Is this point also on the AABB? So we don't have to search all remaining points...
            MyVector3 pointFurthestAway = FindPointFurthestFromEdge(eFurthestApart, points);

            //Remove the point
            points.Remove(pointFurthestAway);


            //Display the triangle
            //Debug.DrawLine(eFurthestApart.p1.ToVector3(), eFurthestApart.p2.ToVector3(), Color.white, 1f);
            //Debug.DrawLine(eFurthestApart.p1.ToVector3(), pointFurthestAway.ToVector3(), Color.blue, 1f);
            //Debug.DrawLine(eFurthestApart.p2.ToVector3(), pointFurthestAway.ToVector3(), Color.blue, 1f);


            //Now we can build two triangles
            //It doesnt matter how we build these triangles as long as they are opposite
            //But the normal matters, so make sure it is calculated so the triangles are ordered clock-wise while the normal is pointing out
            MyVector3 p1 = eFurthestApart.p1;
            MyVector3 p2 = eFurthestApart.p2;
            MyVector3 p3 = pointFurthestAway;

            convexHull.AddTriangle(p1, p2, p3);
            convexHull.AddTriangle(p1, p3, p2);

            //Debug.Log(convexHull.faces.Count);
            /*
            foreach (HalfEdgeFace3 f in convexHull.faces)
            {
                TestAlgorithmsHelpMethods.DebugDrawTriangle(f, Color.white, Color.red);
            }
            */

            //Find the point which is furthest away from the triangle (this point cant be co-planar)
            List<HalfEdgeFace3> triangles = new List<HalfEdgeFace3>(convexHull.faces);

            //Just pick one of the triangles
            HalfEdgeFace3 triangle = triangles[0];

            //Build a plane
            Plane3 plane = new Plane3(triangle.edge.v.position, triangle.edge.v.normal);

            //Find the point furthest away from the plane
            MyVector3 p4 = FindPointFurthestAwayFromPlane(points, plane);

            //Remove the point
            points.Remove(p4);

            //Debug.DrawLine(p1.ToVector3(), p4.ToVector3(), Color.green, 1f);
            //Debug.DrawLine(p2.ToVector3(), p4.ToVector3(), Color.green, 1f);
            //Debug.DrawLine(p3.ToVector3(), p4.ToVector3(), Color.green, 1f);

            //Now we have to remove one of the triangles == the triangle the point is outside of
            HalfEdgeFace3 triangleToRemove = triangles[0];
            HalfEdgeFace3 triangleToKeep = triangles[1];

            //This means the point is inside the triangle-plane, so we have to switch
            //We used triangle #0 to generate the plane
            if (_Geometry.GetSignedDistanceFromPointToPlane(p4, plane) < 0f)
            {
                triangleToRemove = triangles[1];
                triangleToKeep = triangles[0];
            }

            //Delete the triangle 
            convexHull.DeleteFace(triangleToRemove);

            //Build three new triangles

            //The triangle we keep is ordered clock-wise:
            MyVector3 p1_opposite = triangleToKeep.edge.v.position;
            MyVector3 p2_opposite = triangleToKeep.edge.nextEdge.v.position;
            MyVector3 p3_opposite = triangleToKeep.edge.nextEdge.nextEdge.v.position;

            //But we are looking at it from the back-side, 
            //so we add those vertices counter-clock-wise to make the new triangles clock-wise
            convexHull.AddTriangle(p1_opposite, p3_opposite, p4);
            convexHull.AddTriangle(p3_opposite, p2_opposite, p4);
            convexHull.AddTriangle(p2_opposite, p1_opposite, p4);

            //Make sure all opposite edges are connected
            convexHull.ConnectAllEdgesSlow();

            //Debug.Log(convexHull.faces.Count);

            //Display what weve got so far
            //foreach (HalfEdgeFace3 f in convexHull.faces)
            //{
            //    TestAlgorithmsHelpMethods.DebugDrawTriangle(f, Color.white, Color.red);
            //}

            /*
            //Now we might as well remove all the points that are within the tetrahedron because they are not on the hull
            //But this is slow if we have many points and none of them are inside
            HashSet<MyVector3> pointsToRemove = new HashSet<MyVector3>();

            foreach (MyVector3 p in points)
            {
                bool isWithinConvexHull = _Intersections.PointWithinConvexHull(p, convexHull);

                if (isWithinConvexHull)
                {
                    pointsToRemove.Add(p);
                }
            }

            Debug.Log($"Removed {pointsToRemove.Count} points because they were within the tetrahedron");

            foreach (MyVector3 p in pointsToRemove)
            {
                points.Remove(p);
            }
            */
        }


       
        //Given points and a plane, find the point furthest away from the plane
        private static MyVector3 FindPointFurthestAwayFromPlane(HashSet<MyVector3> points, Plane3 plane)
        {
            //Cant init by picking the first point in a list because it might be co-planar
            MyVector3 bestPoint = default;

            float bestDistance = -Mathf.Infinity;

            foreach (MyVector3 p in points)
            {
                float distance = _Geometry.GetSignedDistanceFromPointToPlane(p, plane);

                //Make sure the point is not co-planar
                float epsilon = MathUtility.EPSILON;

                //If distance is around 0
                if (distance > -epsilon && distance < epsilon)
                {
                    continue;
                }

                //Make sure distance is positive
                if (distance < 0f) distance *= -1f;

                if (distance > bestDistance)
                {
                    bestDistance = distance;

                    bestPoint = p;
                }
            }

            return bestPoint;
        }



        //From a list of points, find the two points that are furthest away from each other
        private static Edge3 FindEdgeFurthestApart(HashSet<MyVector3> pointsHashSet)
        {
            List<MyVector3> points = new List<MyVector3>(pointsHashSet);


            //Instead of using all points, find the points on the AABB
            MyVector3 maxX = points[0]; //Cant use default because default doesnt exist and might be a min point
            MyVector3 minX = points[0];
            MyVector3 maxY = points[0];
            MyVector3 minY = points[0];
            MyVector3 maxZ = points[0];
            MyVector3 minZ = points[0];

            for (int i = 1; i < points.Count; i++)
            {
                MyVector3 p = points[i];
            
                if (p.x > maxX.x)
                {
                    maxX = p;
                }
                if (p.x < minX.x)
                {
                    minX = p;
                }

                if (p.y > maxY.y)
                {
                    maxY = p;
                }
                if (p.y < minY.y)
                {
                    minY = p;
                }

                if (p.z > maxZ.z)
                {
                    maxZ = p;
                }
                if (p.z < minZ.z)
                {
                    minZ = p;
                }
            }

            //Some of these might be the same point (like minZ and minY)
            //But we have earlier check that the points have a width greater than 0, so we should get the points we need
            HashSet<MyVector3> extremePointsHashSet = new HashSet<MyVector3>();

            extremePointsHashSet.Add(maxX);
            extremePointsHashSet.Add(minX);
            extremePointsHashSet.Add(maxY);
            extremePointsHashSet.Add(minY);
            extremePointsHashSet.Add(maxZ);
            extremePointsHashSet.Add(minZ);

            points = new List<MyVector3>(extremePointsHashSet);


            //Find all possible combinations of edges between all points
            List<Edge3> pointCombinations = new List<Edge3>();

            for (int i = 0; i < points.Count; i++)
            {
                MyVector3 p1 = points[i];

                for (int j = i + 1; j < points.Count; j++)
                {
                    MyVector3 p2 = points[j];

                    Edge3 e = new Edge3(p1, p2);

                    pointCombinations.Add(e);
                }
            }


            //Find the edge that is the furthest apart

            //Init by picking the first edge
            Edge3 eFurthestApart = pointCombinations[0];

            float maxDistanceBetween = MyVector3.SqrDistance(eFurthestApart.p1, eFurthestApart.p2);

            //Try to find a better edge
            for (int i = 1; i < pointCombinations.Count; i++)
            {
                Edge3 e = pointCombinations[i];

                float distanceBetween = MyVector3.SqrDistance(e.p1, e.p2);

                if (distanceBetween > maxDistanceBetween)
                {
                    maxDistanceBetween = distanceBetween;

                    eFurthestApart = e;
                }
            }

            return eFurthestApart;
        }



        //Given an edge and a list of points, find the point furthest away from the edge
        private static MyVector3 FindPointFurthestFromEdge(Edge3 edge, HashSet<MyVector3> pointsHashSet)
        {
            List<MyVector3> points = new List<MyVector3>(pointsHashSet);

            //Init with the first point
            MyVector3 pointFurthestAway = points[0];

            MyVector3 closestPointOnLine = _Geometry.GetClosestPointOnLine(edge, pointFurthestAway, withinSegment: false);

            float maxDistSqr = MyVector3.SqrDistance(pointFurthestAway, closestPointOnLine);

            //Try to find a better point
            for (int i = 1; i < points.Count; i++)
            {
                MyVector3 thisPoint = points[i];

                //TODO make sure that thisPoint is NOT colinear with the edge because then we wont be able to build a triangle

                closestPointOnLine = _Geometry.GetClosestPointOnLine(edge, thisPoint, withinSegment: false);

                float distSqr = MyVector3.SqrDistance(thisPoint, closestPointOnLine);

                if (distSqr > maxDistSqr)
                {
                    maxDistSqr = distSqr;

                    pointFurthestAway = thisPoint;
                }
            }


            return pointFurthestAway;
        }
    }
}
