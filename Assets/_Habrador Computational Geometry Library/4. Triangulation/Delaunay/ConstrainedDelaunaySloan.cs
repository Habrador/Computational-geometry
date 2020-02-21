using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //From the report "An algorithm for generating constrained delaunay triangulations" by Sloan
    public static class ConstrainedDelaunaySloan
    {
        public static HalfEdgeData2 GenerateTriangulation(HashSet<MyVector2> points, List<MyVector2> constraints, bool shouldRemoveTriangles, HalfEdgeData2 triangleData)
        {
            //Start by generating a delaunay triangulation with all points, including the constraints
            if (constraints != null)
            {
                for (int i = 0; i < constraints.Count; i++)
                {
                    points.Add(constraints[i]);
                }
            }

            //Generate the Delaunay with some algorithm
            //HalfEdgeData triangleData = _Delaunay.TriangulateByFlippingEdges(points);
            _Delaunay.TriangulatePointByPoint(points, triangleData);
            
            //Modify the triangulation by adding the constraints to the delaunay triangulation
            if (constraints != null)
            {
                triangleData = AddConstraints(triangleData, constraints, shouldRemoveTriangles);
            }

            //Debug.Log(triangleData.faces.Count);

            return triangleData;
        }



        //Add the constraints to the delaunay triangulation
        private static HalfEdgeData2 AddConstraints(HalfEdgeData2 triangleData, List<MyVector2> constraints, bool shouldRemoveTriangles)
        {
            //First create a list with all unique edges
            //In the half-edge data structure, we have for each edge, and half edge going in each direction,
            //making it unneccessary to loop through all edges for intersection tests
            //The report suggest we should do a triangle walk, but it will not work if the mesh has holes
            List<HalfEdge2> uniqueEdges = triangleData.GetUniqueEdges();


            //The steps numbering is from the report
            //Step 1. Loop over each constrained edge. For each of these edges, do steps 2-4 
            for (int i = 0; i < constraints.Count; i++)
            {
                //Let each constrained edge be defined by the vertices:
                MyVector2 v_i = constraints[i];
                MyVector2 v_j = constraints[MathUtility.ClampListIndex(i + 1, constraints.Count)];

                //Check if this constraint already exists in the triangulation, if so we are happy and dont need to worry about this edge
                if (IsEdgePartOfTriangulation(uniqueEdges, v_i, v_j))
                {
                    continue;
                }

                //Step 2. Find all edges in the current triangulation that intersects with this constraint
                List<HalfEdge2> intersectingEdges = FindIntersectingEdges(uniqueEdges, v_i, v_j);

                //Step 3. Remove intersecting edges by adding new edges
                List<HalfEdge2> newEdges = RemoveIntersectingEdges(v_i, v_j, intersectingEdges);

                //Step 4. Restore delaunay triangulation (if you want to)
                RestoreDelaunayTriangulation(v_i, v_j, newEdges);
            }

            //Step 5. Remove superfluous triangles (if you need to)
            if (shouldRemoveTriangles)
            {
                RemoveSuperfluousTriangles(triangleData, constraints);
            }

            return triangleData;
        }



        //Remove edges that intersects with a constraint and add new edges
        //The idea here is that all possible triangulations for a set of points can be found 
        //by systematically swapping the diagonal in each convex quadrilateral formed by a pair of triangles
        //So we will test all possible arrangements and will always find a triangulation which includes the constrained edge
        private static List<HalfEdge2> RemoveIntersectingEdges(MyVector2 v_i, MyVector2 v_j, List<HalfEdge2> intersectingEdges)
        {
            List<HalfEdge2> newEdges = new List<HalfEdge2>();

            int safety = 0;

            //While some edges still cross the constrained edge, do steps 3.1 and 3.2
            while (intersectingEdges.Count > 0)
            {
                safety += 1;

                if (safety > 10000)
                {
                    Debug.Log("Stuck in infinite loop when fixing constrained edges");

                    break;
                }

                //Step 3.1. Remove an edge from the list of edges that intersects the constrained edge
                HalfEdge2 e = intersectingEdges[0];

                intersectingEdges.RemoveAt(0);

                //The vertices belonging to the two triangles
                MyVector2 v_k = e.v.position;
                MyVector2 v_l = e.prevEdge.v.position;
                MyVector2 v_third_pos = e.nextEdge.v.position;
                //The vertex belonging to the opposite triangle and isn't shared by the current edge
                MyVector2 v_opposite_pos = e.oppositeEdge.nextEdge.v.position;

                //Step 3.2. If the two triangles that share the edge v_k and v_l do not form a convex quadtrilateral then place
                //the edge back on the list of intersecting edges and go to step 3.1
                if (!Geometry.IsQuadrilateralConvex(v_k, v_l, v_third_pos, v_opposite_pos))
                {
                    intersectingEdges.Add(e);

                    continue;
                }
                else
                {
                    //Flip the edge like we did when we created the delaunay triangulation so use the code from that class
                    HalfEdgeHelpMethods.FlipTriangleEdge(e);

                    //The new diagonal is defined by the vertices
                    MyVector2 v_m = e.v.position;
                    MyVector2 v_n = e.prevEdge.v.position;

                    //If this new diagonal intersects the constrained edge, add it to the list of intersecting edges
                    if (IsEdgeCrossingEdge(v_i, v_j, v_m, v_n))
                    {
                        intersectingEdges.Add(e);
                    }
                    //Place it in the list of newly created edges
                    else
                    {
                        newEdges.Add(e);
                    }
                }
            }

            return newEdges;
        }



        //Try to restore the delaunay triangulation by flipping newly created edges
        //This process is similar to when we created the original delaunay triangulation
        //This step can maybe be skipped if you just want a triangulation and Ive noticed its often not flipping any triangles
        private static void RestoreDelaunayTriangulation(MyVector2 v_i, MyVector2 v_j, List<HalfEdge2> newEdges)
        {
            int safety = 0;

            int flippedEdges = 0;

            //Repeat 4.1 - 4.3 until no further swaps take place
            while (true)
            {
                safety += 1;

                if (safety > 100000)
                {
                    Debug.Log("Stuck in endless loop when delaunay after fixing constrained edges");

                    break;
                }

                bool hasFlippedEdge = false;

                //Step 4.1. Loop over each edge in the list of newly created edges
                for (int j = 0; j < newEdges.Count; j++)
                {
                    HalfEdge2 e = newEdges[j];

                    //Step 4.2. Let the newly created edge be defined by the vertices
                    MyVector2 v_k = e.v.position;
                    MyVector2 v_l = e.prevEdge.v.position;

                    //If this edge is equal to the constrained edge v_i and v_j, then skip to step 4.1
                    //because we are not allowed to flip a constrained edge
                    if ((v_k.Equals(v_i) && v_l.Equals(v_j)) || (v_l.Equals(v_i) && v_k.Equals(v_j)))
                    {
                        continue;
                    }

                    //Step 4.3. If the two triangles that share edge v_k and v_l don't satisfy the delaunay criterion,
                    //so that a vertex of one of the triangles is inside the circumcircle of the other triangle, flip the edge
                    //The third vertex of the triangle belonging to this edge
                    MyVector2 v_third_pos = e.nextEdge.v.position;
                    //The vertice belonging to the triangle on the opposite side of the edge and this vertex is not a part of the edge
                    MyVector2 v_opposite_pos = e.oppositeEdge.nextEdge.v.position;

                    //Test if we should flip this edge
                    //if (_Delaunay.ShouldFlipEdge(v_k.XZ(), v_third_pos.XZ(), v_l.XZ(), v_opposite_pos.XZ()))
                    if (_Delaunay.ShouldFlipEdge(v_l, v_k, v_third_pos, v_opposite_pos))
                    {
                        //Flip the edge
                        hasFlippedEdge = true;

                        HalfEdgeHelpMethods.FlipTriangleEdge(e);

                        flippedEdges += 1;
                    }
                }

                //We have searched through all edges and havent found an edge to flip, so we cant improve anymore
                if (!hasFlippedEdge)
                {
                    Debug.Log("Found a constrained delaunay triangulation in " + flippedEdges + " flips");

                    break;
                }
            }
        }



        //Remove all triangles that are inside the constraint
        //This assumes the vertices in the constraint are ordered clockwise
        private static void RemoveSuperfluousTriangles(HalfEdgeData2 triangleData, List<MyVector2> constraints)
        {
            //This assumes we have at least 3 vertices in the constraint because we cant delete triangles inside a line
            if (constraints.Count < 3)
            {
                return;
            }


            HashSet<HalfEdgeFace2> trianglesToBeDeleted = FindTrianglesWithinConstraint(triangleData, constraints);

            //Delete the triangles
            foreach (HalfEdgeFace2 t in trianglesToBeDeleted)
            {
                HalfEdgeHelpMethods.DeleteTriangle(t, triangleData, true);
            }
        }



        //Find which triangles are within a certain constraint
        //Sometimes we want to delete them and sometimes we want to keep them
        public static HashSet<HalfEdgeFace2> FindTrianglesWithinConstraint(HalfEdgeData2 triangleData, List<MyVector2> constraints)
        {
            if (constraints.Count < 3)
            {
                return null;
            }


            //Create a dictionary with all constraints combinations, which will make it easier to see if an edge is a constraint
            //But its still faster to mark if a constraint is an edge?
            //Dictionary<Vector3, Vector3> constraintsLookup = new Dictionary<Vector3, Vector3>();
            //TODO: is for some reason not working. Most likely because of floating point precision issues
            //When we generate the delaunay, we normalize in the range 0,1 and then unnormalize again
            //When looking up a key in the deictionay we use the key which is the precise measurement
            //Can maybe be fixed by searching thorugh all vertices and make sure they have the same coordinates
            //as the the constraints

            //for (int i = 0; i < constraints.Count; i++)
            //{
            //    //Let each constrained edge be defined by the vertices:
            //    Vector3 v_i = constraints[i];
            //    Vector3 v_j = constraints[MathUtility.ClampListIndex(i + 1, constraints.Count)];

            //    constraintsLookup.Add(v_i, v_j);
            //}


            //Start at a triangle with an edge that shares an edge with the first constraint edge in the list 
            //Since both are clockwise we know we are "inside" of the constraint, so this is a triangle we should delete
            HalfEdgeFace2 borderTriangle = null;

            MyVector2 constrained_p1 = constraints[0];
            MyVector2 constrained_p2 = constraints[1];

            foreach (HalfEdgeFace2 t in triangleData.faces)
            {
                HalfEdge2 e1 = t.edge;
                HalfEdge2 e2 = e1.nextEdge;
                HalfEdge2 e3 = e2.nextEdge;

                //Is any of these edges a constraint?
                if (e1.v.position.Equals(constrained_p2) && e1.prevEdge.v.position.Equals(constrained_p1))
                {
                    borderTriangle = t;

                    break;
                }
                if (e2.v.position.Equals(constrained_p2) && e2.prevEdge.v.position.Equals(constrained_p1))
                {
                    borderTriangle = t;

                    break;
                }
                if (e3.v.position.Equals(constrained_p2) && e3.prevEdge.v.position.Equals(constrained_p1))
                {
                    borderTriangle = t;

                    break;
                }
            }

            if (borderTriangle == null)
            {
                return null;
            }

            //Debug.DrawLine(borderTriangle.edge.v.position, borderTriangle.edge.nextEdge.v.position, Color.white, 2f);
            //Debug.DrawLine(borderTriangle.edge.nextEdge.v.position, borderTriangle.edge.nextEdge.nextEdge.v.position, Color.white, 2f);

            //return;

            //TODO: A better way is to maybe find all triangles on the boundary
            //and then use floodfill to find all triangles that are within the boundary triangles???
            //Another way suggested by a report is to let the user add a point which is in the hole to find a triangle to start with
            //so the user doesnt have to care about the orientation of the hole, or that the constrains are connected

            //Find all triangles within the constraint by using a flood fill algorithm
            //All these triangles should be deleted
            //We can use a hashset, which can determine if an item is in the set in constant time and cant include duplicates
            HashSet<HalfEdgeFace2> trianglesWithinConstraint = new HashSet<HalfEdgeFace2>();

            //We know this triangle should be deleted
            trianglesWithinConstraint.Add(borderTriangle);

            //We are just going to remove the first triangle each loop, so we can use a queue
            Queue<HalfEdgeFace2> trianglesToCheck = new Queue<HalfEdgeFace2>();

            //Start at the triangle we know is within the constraints
            trianglesToCheck.Enqueue(borderTriangle);

            int safety = 0;

            while (true)
            {
                safety += 1;

                if (safety > 100000)
                {
                    Debug.Log("Stuck in infinite loop when deleteing superfluous triangles");

                    break;
                }

                //Stop if we are out of neighbors
                if (trianglesToCheck.Count == 0)
                {
                    break;
                }

                //Pick the first triangle in the list and investigate its neighbors
                HalfEdgeFace2 t = trianglesToCheck.Dequeue();

                HalfEdge2 e1 = t.edge;
                HalfEdge2 e2 = e1.nextEdge;
                HalfEdge2 e3 = e2.nextEdge;

                //If the neighbor is not an outer border meaning no neighbor exists
                //If we have not already visited the neighbor
                //If the edge between the neighbor and this triangle is not a constraint because we are not allowed to cross the constraint
                //Then it's a valid neighbor and we should flood to it
                if (
                    e1.oppositeEdge != null &&
                    !trianglesWithinConstraint.Contains(e1.oppositeEdge.face) &&
                    !IsAnEdgeAConstraint(e1.v.position, e1.prevEdge.v.position, constraints))
                {
                    trianglesToCheck.Enqueue(e1.oppositeEdge.face);

                    trianglesWithinConstraint.Add(e1.oppositeEdge.face);
                }
                if (
                    e2.oppositeEdge != null &&
                    !trianglesWithinConstraint.Contains(e2.oppositeEdge.face) &&
                    !IsAnEdgeAConstraint(e2.v.position, e2.prevEdge.v.position, constraints))
                {
                    trianglesToCheck.Enqueue(e2.oppositeEdge.face);

                    trianglesWithinConstraint.Add(e2.oppositeEdge.face);
                }
                if (
                    e3.oppositeEdge != null &&
                    !trianglesWithinConstraint.Contains(e3.oppositeEdge.face) &&
                    !IsAnEdgeAConstraint(e3.v.position, e3.prevEdge.v.position, constraints))
                {
                    trianglesToCheck.Enqueue(e3.oppositeEdge.face);

                    trianglesWithinConstraint.Add(e3.oppositeEdge.face);
                }
            }

            return trianglesWithinConstraint;
        }



        //Is an edge between p1 and p2 a constraint?
        //private static bool IsAnEdgeAConstraint(Vector3 p1, Vector3 p2, Dictionary<Vector3, Vector3> constraintsLookup)
        private static bool IsAnEdgeAConstraint(MyVector2 p1, MyVector2 p2, List<MyVector2> constraints)
        {
            //if (constraintsLookup.ContainsKey(p1) && constraintsLookup[p1] == p2)
            //{
            //    return true;
            //}
            //if (constraintsLookup.ContainsKey(p2) && constraintsLookup[p2] == p1)
            //{
            //    return true;
            //}

            //Debug.Log("Edge is constraint");
            for (int i = 0; i < constraints.Count; i++)
            {
                MyVector2 c_p1 = constraints[i];
                MyVector2 c_p2 = constraints[MathUtility.ClampListIndex(i + 1, constraints.Count)];

                if ((p1.Equals(c_p1) && p2.Equals(c_p2)) || (p2.Equals(c_p1) && p1.Equals(c_p2)))
                {
                    return true;
                }
            }

            return false;
        }



        //Find all edges of the current triangulation that intersects with the constraint edge between p1 and p2
        private static List<HalfEdge2> FindIntersectingEdges(List<HalfEdge2> uniqueEdges, MyVector2 p1, MyVector2 p2)
        {
            List<HalfEdge2> intersectingEdges = new List<HalfEdge2>();

            //Loop through all edges and see if they are intersecting with the constrained edge
            //An improvement would be to first create a list with unique edges
            for (int i = 0; i < uniqueEdges.Count; i++)
            {
                //The edges the triangle consists of
                HalfEdge2 e = uniqueEdges[i];

                TryAddEdgeToIntersectingEdges(e, p1, p2, intersectingEdges);
            }


            //While the above is working, a faster (but more complicated) way is to do a triangle walk which is suggested in the report
            //This assumes there are no holes in the mesh
            //FindIntersectingEdgesWithTriangleWalk(triangleData, p1, p2, intersectingEdges);



            return intersectingEdges;
        }



        //Find intersecting edges by doing a triangle walk from the start of the constraint we want to test against
        //This assumes there are no holes in the mesh
        private static void FindIntersectingEdgesWithTriangleWalk(HalfEdgeData2 triangleData, MyVector2 p1, MyVector2 p2, List<HalfEdge2> intersectingEdges)
        {
            //Step 1. Begin at a triangle connected to the first vertex in the constraint edge
            HalfEdgeFace2 f = null;

            foreach (HalfEdgeFace2 testFace in triangleData.faces)
            {
                //The edges the triangle consists of
                HalfEdge2 e1 = testFace.edge;
                HalfEdge2 e2 = e1.nextEdge;
                HalfEdge2 e3 = e2.nextEdge;

                //Does one of these edges include the first vertex in the constraint edge
                if (e1.v.position.Equals(p1) || e2.v.position.Equals(p1) || e3.v.position.Equals(p1))
                {
                    f = testFace;

                    break;
                }
            }


            //HalfEdge e1_debug = t.halfEdge;
            //HalfEdge e2_debug = e1_debug.nextEdge;
            //HalfEdge e3_debug = e2_debug.nextEdge;

            //Debug.DrawLine(e1_debug.v.position, e1_debug.prevEdge.v.position, Color.white, 3f);
            //Debug.DrawLine(e2_debug.v.position, e2_debug.prevEdge.v.position, Color.white, 3f);
            //Debug.DrawLine(e3_debug.v.position, e3_debug.prevEdge.v.position, Color.white, 3f);

            //Step2. Walk around p1 until we find a triangle with an edge that intersects with the edge p1-p2
            int safety = 0;

            //This is the last edge on the previous triangle we crossed so we know which way to rotatet
            HalfEdge2 lastEdge = null;

            //When we rotate we might pick the wrong start direction if the edge is on the border, so we can't rotate all the way around
            //If that happens we have to restart and rotate in the other direction
            HalfEdgeFace2 startTriangle = f;

            bool restart = false;

            while (true)
            {
                safety += 1;

                if (safety > 10000)
                {
                    Debug.Log("Stuck in infinite loop when finding the start triangle when finding intersecting edges");

                    break;
                }

                //Check if the current triangle is intersecting with the constraint
                HalfEdge2 e1 = f.edge;
                HalfEdge2 e2 = e1.nextEdge;
                HalfEdge2 e3 = e2.nextEdge;

                //The only edge that can intersect with the constraint is the edge that doesnt include p1, so find it
                HalfEdge2 e_doesnt_include_p1 = null;

                if (!e1.v.position.Equals(p1) && !e1.prevEdge.v.position.Equals(p1))
                {
                    e_doesnt_include_p1 = e1;
                }
                else if (!e2.v.position.Equals(p1) && !e2.prevEdge.v.position.Equals(p1))
                {
                    e_doesnt_include_p1 = e2;
                }
                else
                {
                    e_doesnt_include_p1 = e3;
                }

                //Is the edge that doesn't include p1 intersecting with the constrained edge?
                if (IsEdgeCrossingEdge(e_doesnt_include_p1.v.position, e_doesnt_include_p1.prevEdge.v.position, p1, p2))
                {
                    //We have found the triangle where we should begin the walk
                    break;
                }

                //We have not found the triangle where we should begin the walk, so we should rotate to another triangle which includes p1

                //Find the two edges that include p1 so we can rotate across one of them
                List<HalfEdge2> includes_p1 = new List<HalfEdge2>();

                if (e1 != e_doesnt_include_p1)
                {
                    includes_p1.Add(e1);
                }
                if (e2 != e_doesnt_include_p1)
                {
                    includes_p1.Add(e2);
                }
                if (e3 != e_doesnt_include_p1)
                {
                    includes_p1.Add(e3);
                }

                //This is the first rotation we do from the triangle we found at the start, so we rotate in a direction
                if (lastEdge == null)
                {
                    //But if we are on the border of the triangulation we cant just pick a direction because one of the 
                    //directions might not be valid and end up outside of the triangulation
                    //This problem could be solved if we add a "supertriangle" covering all points

                    lastEdge = includes_p1[0];

                    //Dont go in this direction because then we are outside of the triangulation
                    //Sometimes we may have picked the wrong direction when we rotate from the first triangle 
                    //and rotated around towards a triangle that's at the border, if so we have to restart and rotate
                    //in the other direction
                    if (lastEdge.oppositeEdge == null || restart)
                    {
                        lastEdge = includes_p1[1];
                    }

                    //The triangle we rotate to
                    f = lastEdge.oppositeEdge.face;
                }
                else
                {
                    //Move in the direction that doesnt include the last edge
                    if (includes_p1[0].oppositeEdge != lastEdge)
                    {
                        lastEdge = includes_p1[0];
                    }
                    else
                    {
                        lastEdge = includes_p1[1];
                    }

                    //If we have hit a border edge, we should have rotated in the other direction when we started at the first triangle
                    //So we have to jump back
                    if (lastEdge.oppositeEdge == null)
                    {
                        restart = true;

                        f = startTriangle;

                        lastEdge = null;
                    }
                    else
                    {
                        //The triangle we rotate to
                        f = lastEdge.oppositeEdge.face;
                    }
                }
            }

            //HalfEdge e1_debug = t.halfEdge;
            //HalfEdge e2_debug = e1_debug.nextEdge;
            //HalfEdge e3_debug = e2_debug.nextEdge;

            //Debug.DrawLine(e1_debug.v.position, e1_debug.prevEdge.v.position, Color.white, 1f);
            //Debug.DrawLine(e2_debug.v.position, e2_debug.prevEdge.v.position, Color.white, 1f);
            //Debug.DrawLine(e3_debug.v.position, e3_debug.prevEdge.v.position, Color.white, 1f);


            //Step3. March from one triangle to the next in the general direction of p2
            //This means we always move across the edge of the triangle that intersects with the constraint
            int safety2 = 0;

            lastEdge = null;

            while (true)
            {
                safety2 += 1;

                if (safety2 > 10000)
                {
                    Debug.Log("Stuck in infinite loop when finding intersecting edges");

                    break;
                }

                //The three edges belonging to the current triangle
                HalfEdge2 e1 = f.edge;
                HalfEdge2 e2 = e1.nextEdge;
                HalfEdge2 e3 = e2.nextEdge;

                //Debug.DrawLine(e1.v.position, e1.prevEdge.v.position, Color.white, 1f);
                //Debug.DrawLine(e2.v.position, e2.prevEdge.v.position, Color.white, 1f);
                //Debug.DrawLine(e3.v.position, e3.prevEdge.v.position, Color.white, 1f);

                //Does this triangle include the last vertex on the constraint edge? If so we have found all edges that intersects
                if (e1.v.position.Equals(p2) || e2.v.position.Equals(p2) || e3.v.position.Equals(p2))
                {
                    break;
                }
                //Find which edge that intersects with the constraint
                //More than one edge maight intersect, so we have to check if it's not the edge we are coming from
                else
                {
                    //Save the edge that intersects in case the triangle intersects with two edges
                    if (e1.oppositeEdge != lastEdge && IsEdgeCrossingEdge(e1.v.position, e1.prevEdge.v.position, p1, p2))
                    {
                        lastEdge = e1;
                    }
                    else if (e2.oppositeEdge != lastEdge && IsEdgeCrossingEdge(e2.v.position, e2.prevEdge.v.position, p1, p2))
                    {
                        lastEdge = e2;
                    }
                    else
                    {
                        lastEdge = e3;
                    }

                    //Jump to the next triangle by crossing the edge that intersects with the constraint
                    f = lastEdge.oppositeEdge.face;

                    //Save the intersecting edge
                    intersectingEdges.Add(lastEdge);
                }
            }
        }



        //Check if an edge is intersecting with the constraint edge between p1 and p2
        //If so, add it to the list if the edge doesnt exist in the list
        private static void TryAddEdgeToIntersectingEdges(HalfEdge2 e, MyVector2 p1, MyVector2 p2, List<HalfEdge2> intersectingEdges)
        {
            //The position the edge is going to
            MyVector2 e_p1 = e.v.position;
            //The position the edge is coming from
            MyVector2 e_p2 = e.prevEdge.v.position;

            //Is this edge intersecting with the constraint?
            if (IsEdgeCrossingEdge(e_p1, e_p2, p1, p2))
            {
                //Add it to the list if it isnt already in the list
                for (int i = 0; i < intersectingEdges.Count; i++)
                {
                    //In the half-edge data structure, theres another edge on the opposite side going in the other direction
                    //so we have to check both because we want unique edges
                    if (intersectingEdges[i] == e || intersectingEdges[i].oppositeEdge == e)
                    {
                        //The edge is already in the list
                        return;
                    }
                }

                //The edge is not in the list so add it
                intersectingEdges.Add(e);
            }
        }



        //Is an edge crossing another edge? 
        private static bool IsEdgeCrossingEdge(MyVector2 e1_p1, MyVector2 e1_p2, MyVector2 e2_p1, MyVector2 e2_p2)
        {
            //We will here run into floating point precision issues so we have to be careful
            //To solve that you can first check the end points 
            //and modify the line-line intersection algorithm to include a small epsilon

            //First check if the edges are sharing a point, if so they are not crossing
            if (e1_p1.Equals(e2_p1) || e1_p1.Equals(e2_p2) || e1_p2.Equals(e2_p1) || e1_p2.Equals(e2_p2))
            {
                return false;
            }

            //Then check if the lines are intersecting
            if (!Intersections.LineLine(e1_p1, e1_p2, e2_p1, e2_p2, false))
            {
                return false;
            }

            return true;
        }




        //Is an edge (between p1 and p2) a part of an edge in the triangulation?
        private static bool IsEdgePartOfTriangulation(List<HalfEdge2> uniqueEdges, MyVector2 p1, MyVector2 p2)
        {
            //List<HalfEdge> edges = triangleData.edges;

            for (int i = 0; i < uniqueEdges.Count; i++)
            {
                //The vertices positions of the current triangle
                MyVector2 e_p1 = uniqueEdges[i].v.position;
                MyVector2 e_p2 = uniqueEdges[i].prevEdge.v.position;

                //Check if edge has the same coordinates as the constrained edge
                //We have no idea about direction so we have to check both directions
                if ((e_p1.Equals(p1) && e_p2.Equals(p2)) || (e_p1.Equals(p2) && e_p2.Equals(p1)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
