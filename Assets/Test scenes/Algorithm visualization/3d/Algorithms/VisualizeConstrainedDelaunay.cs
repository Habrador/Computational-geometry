using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;


public class VisualizeConstrainedDelaunay : MonoBehaviour
{
    private VisualizerController3D visualizeController;

    //Constraints by using children to a parent, which we have to drag
    //Should be sorted counter-clock-wise
    public Transform hullConstraintParent;
    //Should be sorted clock-wise
    public List<Transform> holeConstraintParents;



    void Start()
	{
        visualizeController = GetComponent<VisualizerController3D>();

        //Get the random points
        HashSet<Vector2> randomPoints = TestAlgorithmsHelpMethods.GenerateRandomPoints2D(0, halfSquareSize: 6f, numberOfPoints: 25);

        //To MyVector2
        HashSet<MyVector2> randomPoints_2d = new HashSet<MyVector2>(randomPoints.Select(x => x.ToMyVector2()));

        //Hull
        List<Vector3> hullPoints = TestAlgorithmsHelpMethods.GetPointsFromParent(hullConstraintParent);

        List<MyVector2> hullPoints_2d = hullPoints.Select(x => x.ToMyVector2()).ToList(); ;

        //Holes
        HashSet<List<MyVector2>> allHolePoints_2d = new HashSet<List<MyVector2>>();

        foreach (Transform holeParent in holeConstraintParents)
        {
            List<Vector3> holePoints = TestAlgorithmsHelpMethods.GetPointsFromParent(holeParent);

            if (holePoints != null)
            {
                List<MyVector2> holePoints_2d = holePoints.Select(x => x.ToMyVector2()).ToList();

                allHolePoints_2d.Add(holePoints_2d);
            }
        }


        //Normalize to range 0-1
        //We should use all points, including the constraints because the hole may be outside of the random points
        List<MyVector2> allPoints = new List<MyVector2>();

        allPoints.AddRange(randomPoints_2d);

        allPoints.AddRange(hullPoints_2d);

        foreach (List<MyVector2> hole in allHolePoints_2d)
        {
            allPoints.AddRange(hole);
        }

        Normalizer2 normalizer = new Normalizer2(allPoints);

        HashSet<MyVector2> randomPoints_2d_normalized = normalizer.Normalize(randomPoints_2d);

        List<MyVector2> hullPoints_2d_normalized = normalizer.Normalize(hullPoints_2d);

        HashSet<List<MyVector2>> allHolePoints_2d_normalized = new HashSet<List<MyVector2>>();

        foreach (List<MyVector2> hole in allHolePoints_2d)
        {
            List<MyVector2> hole_normalized = normalizer.Normalize(hole);

            allHolePoints_2d_normalized.Add(hole_normalized);
        }



        StartCoroutine(GenerateConstrainedDelaunayLoop(randomPoints_2d_normalized, hullPoints_2d_normalized, allHolePoints_2d_normalized, shouldRemoveTriangles: true, new HalfEdgeData2(), normalizer));
    }



    private IEnumerator GenerateConstrainedDelaunayLoop(HashSet<MyVector2> points, List<MyVector2> hull, HashSet<List<MyVector2>> holes, bool shouldRemoveTriangles, HalfEdgeData2 triangleData, Normalizer2 normalizer)
    {
        //Start by generating a delaunay triangulation with all points, including the constraints
        HashSet<MyVector2> allPoints = new HashSet<MyVector2>();

        if (points != null)
        {
            allPoints.UnionWith(points);
        }

        if (hull != null)
        {
            allPoints.UnionWith(hull);
        }

        if (holes != null)
        {
            foreach (List<MyVector2> hole in holes)
            {
                allPoints.UnionWith(hole);
            }
        }


        

        //Generate the Delaunay triangulation with some algorithm

        //triangleData = _Delaunay.FlippingEdges(allPoints);
        triangleData = _Delaunay.PointByPoint(allPoints, triangleData);


        //
        // PAUSE AND VISUALIZE
        //

        visualizeController.DisplayMeshMain(triangleData, normalizer);

        yield return new WaitForSeconds(3f);


        
        //Modify the triangulation by adding the constraints to the delaunay triangulation
        yield return StartCoroutine(AddConstraints(triangleData, hull, shouldRemoveTriangles, normalizer));

        foreach (List<MyVector2> hole in holes)
        {
            yield return StartCoroutine(AddConstraints(triangleData, hole, shouldRemoveTriangles, normalizer));
        }
    }


    //
    // Add the constraints to the delaunay triangulation
    //

    //timer is for debugging
    private IEnumerator AddConstraints(HalfEdgeData2 triangleData, List<MyVector2> constraints, bool shouldRemoveTriangles, Normalizer2 normalizer, System.Diagnostics.Stopwatch timer = null)
    {
        //Validate the data
        if (constraints == null)
        {
            yield return null;
        }



        //
        // PAUSE AND VISUALIZE
        //

        //Show the constraint with a line mesh
        HashSet<Triangle2> lineTriangles = _GenerateMesh.ConnectedLineSegments(constraints, width: 0.01f, isConnected: true);

        //UnNormalized and to half-edge 3 (also move each vertex up a little or will intersect with the underlying mesh)
        HalfEdgeData3 lineData = new HalfEdgeData3();

        foreach (Triangle2 t in lineTriangles)
        {
            MyVector2 p1 = t.p1;
            MyVector2 p2 = t.p2;
            MyVector2 p3 = t.p3;

            p1 = normalizer.UnNormalize(p1);
            p2 = normalizer.UnNormalize(p2);
            p3 = normalizer.UnNormalize(p3);

            lineData.AddTriangle(p1.ToMyVector3_Yis3D(0.1f), p2.ToMyVector3_Yis3D(0.1f), p3.ToMyVector3_Yis3D(0.1f));
        }

        visualizeController.DisplayMeshOtherUnNormalized(lineData.faces);

        yield return new WaitForSeconds(2f);




        //Get a list with all edges
        //This is faster than first searching for unique edges
        //The report suggest we should do a triangle walk, but it will not work if the mesh has holes
        //The mesh has holes because we remove triangles while adding constraints one-by-one
        //so maybe better to remove triangles after we added all constraints...
        HashSet<HalfEdge2> edges = triangleData.edges;


        //The steps numbering is from the report
        //Step 1. Loop over each constrained edge. For each of these edges, do steps 2-4 
        for (int i = 0; i < constraints.Count; i++)
        {
            //Let each constrained edge be defined by the vertices:
            MyVector2 c_p1 = constraints[i];
            MyVector2 c_p2 = constraints[MathUtility.ClampListIndex(i + 1, constraints.Count)];

            //Check if this constraint already exists in the triangulation, 
            //if so we are happy and dont need to worry about this edge
            //timer.Start();
            if (IsEdgeInListOfEdges(edges, c_p1, c_p2))
            {
                continue;
            }
            //timer.Stop();

            //Step 2. Find all edges in the current triangulation that intersects with this constraint
            //Is returning unique edges only, so not one edge going in the opposite direction
            //timer.Start();
            Queue<HalfEdge2> intersectingEdges = FindIntersectingEdges_BruteForce(edges, c_p1, c_p2);
            //timer.Stop();

            //Debug.Log("Intersecting edges: " + intersectingEdges.Count);

            //Step 3. Remove intersecting edges by flipping triangles
            //This takes 0 seconds so is not bottleneck
            //timer.Start();
            List<HalfEdge2> newEdges = new List<HalfEdge2>();

            yield return StartCoroutine(RemoveIntersectingEdges(c_p1, c_p2, intersectingEdges, newEdges, triangleData, normalizer));
            //timer.Stop();

            //Step 4. Try to restore delaunay triangulation 
            //Because we have constraints we will never get a delaunay triangulation
            //This takes 0 seconds so is not bottleneck
            //timer.Start();
            yield return StartCoroutine(RestoreDelaunayTriangulation(c_p1, c_p2, newEdges, triangleData, normalizer));
            //timer.Stop();
        }

        //Step 5. Remove superfluous triangles, such as the triangles "inside" the constraints  
        if (shouldRemoveTriangles)
        {
            //timer.Start();
            yield return StartCoroutine(RemoveSuperfluousTriangles(triangleData, constraints, normalizer));
            //timer.Stop();
        }

        //return triangleData;



        //
        // PAUSE AND VISUALIZE
        //

        visualizeController.HideMeshOther();

        yield return new WaitForSeconds(2f);
    }



    //
    // Remove the edges that intersects with a constraint by flipping triangles
    //

    //The idea here is that all possible triangulations for a set of points can be found 
    //by systematically swapping the diagonal in each convex quadrilateral formed by a pair of triangles
    //So we will test all possible arrangements and will always find a triangulation which includes the constrained edge
    private IEnumerator RemoveIntersectingEdges(MyVector2 v_i, MyVector2 v_j, Queue<HalfEdge2> intersectingEdges, List<HalfEdge2> newEdges, HalfEdgeData2 triangleData, Normalizer2 normalizer)
    {
        int safety = 0;

        //While some edges still cross the constrained edge, do steps 3.1 and 3.2
        while (intersectingEdges.Count > 0)
        {
            safety += 1;

            if (safety > 100000)
            {
                Debug.Log("Stuck in infinite loop when fixing constrained edges");

                break;
            }

            //Step 3.1. Remove an edge from the list of edges that intersects the constrained edge
            HalfEdge2 e = intersectingEdges.Dequeue();

            //The vertices belonging to the two triangles
            MyVector2 v_k = e.v.position;
            MyVector2 v_l = e.prevEdge.v.position;
            MyVector2 v_3rd = e.nextEdge.v.position;
            //The vertex belonging to the opposite triangle and isn't shared by the current edge
            MyVector2 v_opposite_pos = e.oppositeEdge.nextEdge.v.position;

            //Step 3.2. If the two triangles don't form a convex quadtrilateral
            //place the edge back on the list of intersecting edges (because this edge cant be flipped) 
            //and go to step 3.1
            if (!_Geometry.IsQuadrilateralConvex(v_k, v_l, v_3rd, v_opposite_pos))
            {
                intersectingEdges.Enqueue(e);

                continue;
            }
            else
            {
                //Flip the edge like we did when we created the delaunay triangulation
                HalfEdgeHelpMethods.FlipTriangleEdge(e);


                //
                // PAUSE AND VISUALIZE
                //

                visualizeController.DisplayMeshMain(triangleData, normalizer);

                yield return new WaitForSeconds(0.5f);


                //The new diagonal is defined by the vertices
                MyVector2 v_m = e.v.position;
                MyVector2 v_n = e.prevEdge.v.position;

                //If this new diagonal intersects with the constrained edge, add it to the list of intersecting edges
                if (IsEdgeCrossingEdge(v_i, v_j, v_m, v_n))
                {
                    intersectingEdges.Enqueue(e);
                }
                //Place it in the list of newly created edges
                else
                {
                    newEdges.Add(e);
                }
            }
        }
    }



    //
    // Try to restore the delaunay triangulation by flipping newly created edges
    //

    //This process is similar to when we created the original delaunay triangulation
    //This step can maybe be skipped if you just want a triangulation and Ive noticed its often not flipping any triangles
    private IEnumerator RestoreDelaunayTriangulation(MyVector2 c_p1, MyVector2 c_p2, List<HalfEdge2> newEdges, HalfEdgeData2 triangleData, Normalizer2 normalizer)
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

                //If this edge is equal to the constrained edge, then skip to step 4.1
                //because we are not allowed to flip the constrained edge
                if ((v_k.Equals(c_p1) && v_l.Equals(c_p2)) || (v_l.Equals(c_p1) && v_k.Equals(c_p2)))
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
                if (DelaunayMethods.ShouldFlipEdge(v_l, v_k, v_third_pos, v_opposite_pos))
                {
                    //Flip the edge
                    hasFlippedEdge = true;

                    HalfEdgeHelpMethods.FlipTriangleEdge(e);

                    flippedEdges += 1;


                    //
                    // PAUSE AND VISUALIZE
                    //

                    visualizeController.DisplayMeshMain(triangleData, normalizer);

                    yield return new WaitForSeconds(0.5f);
                }
            }

            //We have searched through all edges and havent found an edge to flip, so we cant improve anymore
            if (!hasFlippedEdge)
            {
                //Debug.Log("Found a constrained delaunay triangulation in " + flippedEdges + " flips");

                break;
            }
        }
    }



    //
    // Remove all triangles that are inside the constraint
    //

    //This assumes the vertices in the constraint are ordered clockwise
    private IEnumerator RemoveSuperfluousTriangles(HalfEdgeData2 triangleData, List<MyVector2> constraints, Normalizer2 normalizer)
    {
        //This assumes we have at least 3 vertices in the constraint because we cant delete triangles inside a line
        if (constraints.Count < 3)
        {
            yield return null;
        }

        HashSet<HalfEdgeFace2> trianglesToBeDeleted = FindTrianglesWithinConstraint(triangleData, constraints);

        if (trianglesToBeDeleted == null)
        {
            Debug.Log("There are no triangles to delete");

            yield return null;
        }

        //Delete the triangles
        foreach (HalfEdgeFace2 t in trianglesToBeDeleted)
        {
            HalfEdgeHelpMethods.DeleteTriangleFace(t, triangleData, true);


            //
            // PAUSE AND VISUALIZE
            //

            visualizeController.DisplayMeshMain(triangleData, normalizer);

            yield return new WaitForSeconds(0.5f);
        }
    }



    //
    // Find which triangles are within a constraint
    //

    public static HashSet<HalfEdgeFace2> FindTrianglesWithinConstraint(HalfEdgeData2 triangleData, List<MyVector2> constraints)
    {
        HashSet<HalfEdgeFace2> trianglesToDelete = new HashSet<HalfEdgeFace2>();

        //Store the triangles we flood fill in this queue
        Queue<HalfEdgeFace2> trianglesToCheck = new Queue<HalfEdgeFace2>();


        //Step 1. Find all half-edges in the current triangulation which are constraint
        //Maybe faster to find all constraintEdges for ALL constraints because we are doing this per hole and hull
        //We have to find ALL because some triangles are not connected and will thus be missed if we find just a single start-triangle
        //Is also needed when flood-filling so we dont jump over a constraint
        HashSet<HalfEdge2> constraintEdges = FindAllConstraintEdges(constraints, triangleData);

        //Each edge is associated with a face which should be deleted
        foreach (HalfEdge2 e in constraintEdges)
        {
            if (!trianglesToCheck.Contains(e.face))
            {
                trianglesToCheck.Enqueue(e.face);
            }
        }


        //Step 2. Find the rest of the triangles within the constraint by using a flood-fill algorithm
        int safety = 0;

        List<HalfEdge2> edgesToCheck = new List<HalfEdge2>();

        while (true)
        {
            safety += 1;

            if (safety > 100000)
            {
                Debug.Log("Stuck in infinite loop when looking for triangles within constraint");

                break;
            }

            //Stop if we are out of neighbors
            if (trianglesToCheck.Count == 0)
            {
                break;
            }

            //Pick the first triangle in the list and investigate its neighbors
            HalfEdgeFace2 t = trianglesToCheck.Dequeue();

            //Add it for deletion
            trianglesToDelete.Add(t);

            //Investigate the triangles on the opposite sides of these edges
            edgesToCheck.Clear();

            edgesToCheck.Add(t.edge);
            edgesToCheck.Add(t.edge.nextEdge);
            edgesToCheck.Add(t.edge.nextEdge.nextEdge);

            //A triangle is a neighbor within the constraint if:
            //- The neighbor is not an outer border meaning no neighbor exists
            //- If we have not already visited the neighbor
            //- If the edge between the neighbor and this triangle is not a constraint
            foreach (HalfEdge2 e in edgesToCheck)
            {
                //No neighbor exists
                if (e.oppositeEdge == null)
                {
                    continue;
                }

                HalfEdgeFace2 neighbor = e.oppositeEdge.face;

                //We have already visited this neighbor
                if (trianglesToDelete.Contains(neighbor) || trianglesToCheck.Contains(neighbor))
                {
                    continue;
                }

                //This edge is a constraint and we can't jump across constraints 
                if (constraintEdges.Contains(e))
                {
                    continue;
                }

                trianglesToCheck.Enqueue(neighbor);
            }
        }

        return trianglesToDelete;
    }



    //Find a triangle which has an edge going from p1 to p2
    //private static HalfEdgeFace2 FindTriangleWithEdge(MyVector2 p1, MyVector2 p2, HalfEdgeData2 triangleData)
    //{
    //    HashSet<HalfEdge2> edges = triangleData.edges;

    //    foreach (HalfEdge2 e in edges)
    //    {
    //        //An edge is going TO a vertex
    //        MyVector2 e_p1 = e.prevEdge.v.position;
    //        MyVector2 e_p2 = e.v.position;

    //        if (e_p1.Equals(p1) && e_p2.Equals(p2))
    //        {
    //            return e.face;
    //        }
    //    }

    //    return null;
    //}



    //Find all half-edges that are constraint
    private static HashSet<HalfEdge2> FindAllConstraintEdges(List<MyVector2> constraints, HalfEdgeData2 triangleData)
    {
        HashSet<HalfEdge2> constrainEdges = new HashSet<HalfEdge2>();


        //Create a new set with all constrains, and as we discover new constraints, we delete constrains, which will make searching faster
        //A constraint can only exist once!
        HashSet<Edge2> constraintsEdges = new HashSet<Edge2>();

        for (int i = 0; i < constraints.Count; i++)
        {
            MyVector2 c_p1 = constraints[i];
            MyVector2 c_p2 = constraints[MathUtility.ClampListIndex(i + 1, constraints.Count)];

            constraintsEdges.Add(new Edge2(c_p1, c_p2));
        }


        //All edges we have to search
        HashSet<HalfEdge2> edges = triangleData.edges;

        foreach (HalfEdge2 e in edges)
        {
            //An edge is going TO a vertex
            MyVector2 e_p1 = e.prevEdge.v.position;
            MyVector2 e_p2 = e.v.position;

            //Is this edge a constraint?
            foreach (Edge2 c_edge in constraintsEdges)
            {
                if (e_p1.Equals(c_edge.p1) && e_p2.Equals(c_edge.p2))
                {
                    constrainEdges.Add(e);

                    constraintsEdges.Remove(c_edge);

                    //Move on to the next edge
                    break;
                }
            }

            //We have found all constraint, so don't need to search anymore
            if (constraintsEdges.Count == 0)
            {
                break;
            }
        }

        return constrainEdges;
    }



    //
    // Find edges that intersect with a constraint
    //

    //Method 1. Brute force by testing all unique edges
    //Find all edges of the current triangulation that intersects with the constraint edge between p1 and p2
    private static Queue<HalfEdge2> FindIntersectingEdges_BruteForce(HashSet<HalfEdge2> edges, MyVector2 c_p1, MyVector2 c_p2)
    {
        //Should be in a queue because we will later plop the first in the queue and add edges in the back of the queue 
        Queue<HalfEdge2> intersectingEdges = new Queue<HalfEdge2>();

        //We also need to make sure that we are only adding unique edges to the queue
        //In the half-edge data structure we have an edge going in the opposite direction
        //and we only need to add an edge going in one direction
        HashSet<Edge2> edgesInQueue = new HashSet<Edge2>();

        //Loop through all edges and see if they are intersecting with the constrained edge
        foreach (HalfEdge2 e in edges)
        {
            //The position the edge is going to
            MyVector2 e_p2 = e.v.position;
            //The position the edge is coming from
            MyVector2 e_p1 = e.prevEdge.v.position;

            //Has this edge been added, but in the opposite direction?
            if (edgesInQueue.Contains(new Edge2(e_p2, e_p1)))
            {
                continue;
            }

            //Is this edge intersecting with the constraint?
            if (IsEdgeCrossingEdge(e_p1, e_p2, c_p1, c_p2))
            {
                //If so add it to the queue of edges
                intersectingEdges.Enqueue(e);

                edgesInQueue.Add(new Edge2(e_p1, e_p2));
            }
        }

        return intersectingEdges;
    }



    //Method 2. Triangulation walk
    //This assumes there are no holes in the mesh
    //And that we have a super-triangle around the triangulation
    private static void FindIntersectingEdges_TriangleWalk(HalfEdgeData2 triangleData, MyVector2 c_p1, MyVector2 c_p2, List<HalfEdge2> intersectingEdges)
    {
        //Step 1. Begin at a triangle connected to the constraint edges's vertex c_p1
        HalfEdgeFace2 f = null;

        foreach (HalfEdgeFace2 testFace in triangleData.faces)
        {
            //The edges the triangle consists of
            HalfEdge2 e1 = testFace.edge;
            HalfEdge2 e2 = e1.nextEdge;
            HalfEdge2 e3 = e2.nextEdge;

            //Does one of these edges include the first vertex in the constraint edge
            if (e1.v.position.Equals(c_p1) || e2.v.position.Equals(c_p1) || e3.v.position.Equals(c_p1))
            {
                f = testFace;

                break;
            }
        }



        //Step2. Walk around p1 until we find a triangle with an edge that intersects with the edge p1-p2


        //Step3. March from one triangle to the next in the general direction of p2

    }



    //
    // Edge stuff
    //

    //Are two edges the same edge?
    private static bool AreTwoEdgesTheSame(MyVector2 e1_p1, MyVector2 e1_p2, MyVector2 e2_p1, MyVector2 e2_p2)
    {
        //Is e1_p1 part of this constraint?
        if ((e1_p1.Equals(e2_p1) || e1_p1.Equals(e2_p2)))
        {
            //Is e1_p2 part of this constraint?
            if ((e1_p2.Equals(e2_p1) || e1_p2.Equals(e2_p2)))
            {
                return true;
            }
        }

        return false;
    }



    //Is an edge (between p1 and p2) in a list with edges
    private static bool IsEdgeInListOfEdges(HashSet<HalfEdge2> edges, MyVector2 p1, MyVector2 p2)
    {
        foreach (HalfEdge2 e in edges)
        {
            //The vertices positions of the current triangle
            MyVector2 e_p2 = e.v.position;
            MyVector2 e_p1 = e.prevEdge.v.position;

            //Check if edge has the same coordinates as the constrained edge
            //We have no idea about direction so we have to check both directions
            //This is fast because we only need to test one coordinate and if that 
            //coordinate doesn't match the edges can't be the same
            //We can't use a dictionary because we flip edges constantly so it would have to change?
            if (AreTwoEdgesTheSame(p1, p2, e_p1, e_p2))
            {
                return true;
            }
        }

        return false;
    }



    //Is an edge between p1 and p2 a constraint?
    //private static bool IsEdgeAConstraint(MyVector2 p1, MyVector2 p2, List<MyVector2> constraints)
    //{
    //    for (int i = 0; i < constraints.Count; i++)
    //    {
    //        MyVector2 c_p1 = constraints[i];
    //        MyVector2 c_p2 = constraints[MathUtility.ClampListIndex(i + 1, constraints.Count)];

    //        if (AreTwoEdgesTheSame(p1, p2, c_p1, c_p2))
    //        {
    //            return true;
    //        }
    //    }

    //    return false;
    //}



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
        if (!_Intersections.LineLine(new Edge2(e1_p1, e1_p2), new Edge2(e2_p1, e2_p2), includeEndPoints: false))
        {
            return false;
        }

        return true;
    }
}
