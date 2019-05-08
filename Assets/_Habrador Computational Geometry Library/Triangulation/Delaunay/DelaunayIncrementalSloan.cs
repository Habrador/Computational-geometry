using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //From the report "A fast algorithm for constructing Delaunay triangulations in the plane" by Sloan
    //The basic idea is that we start with one big supertriangle and then add the other points one-by-one while flipping edges
    //The complexity is in worst case O(n^(5/4)) but is usually O(n^1.1). 
    //The average performance is more important because the worst performance is not happening that often
    //TODO:
    // - bins
    public static class DelaunayIncrementalSloan
    {
        public static HalfEdgeData GenerateTriangulation(HashSet<Vector3> inputPoints, HalfEdgeData triangulationData)
        {
            //We need more than 1 point to normalize
            if (inputPoints.Count < 2)
            {
                Debug.Log("Can make a delaunay with sloan with less than 2 points");

                //return null;
            }

            //Debug.Log("Hello");

            //Step 1.Normalize the points to the range(0 - 1), which assumes we have more than 1 point
            //This will lower the floating point precision when unnormalizing again, so we might have to go through
            //all points in the end and make sure they have the correct coordinate
            AABB boundingBox = HelpMethods.GetAABB(new List<Vector2>(HelpMethods.ConvertListFrom3DTo2D(inputPoints)));

            float d_max = Mathf.Max(boundingBox.maxX - boundingBox.minX, boundingBox.maxY - boundingBox.minY);

            HashSet<Vector3> points = new HashSet<Vector3>();

            foreach (Vector3 p in inputPoints)
            {
                Vector3 pNormalized = Vector3.zero;

                pNormalized.x = (p.x - boundingBox.minX) / d_max;
                pNormalized.z = (p.z - boundingBox.minY) / d_max;

                points.Add(pNormalized);
            }

            //Used if we dont want to normalize
            //HashSet<Vector3> points = inputPoints;

            

            //Step 2. Sort the points into bins to make it faster to find which triangle a point is in



            //Step 3. Establish the supertriangle
            //The report argues that the supertriangle should be at (-100, 100) which is way
            //outside of the points which are in the range(0, 1)
            //It's important to save this triangle so we can delete it when we are done
            Triangle superTriangle = new Triangle(new Vector3(-100f, 0f, -100f), new Vector3(100f, 0f, -100f), new Vector3(0f, 0f, 100f));

            //Create the triangulation data with the only triangle we have
            HashSet<Triangle> triangles = new HashSet<Triangle>();

            triangles.Add(superTriangle);

            TransformBetweenDataStructures.TransformFromTriangleToHalfEdge(triangles, triangulationData);

            

            //Step 4. Loop over each point we want to insert and do Steps 5-7
            int missedPoints = 0;
            int flippedEdges = 0;

            foreach (Vector3 p in points)
            {
                //Step 5. Insert the new point in the triangulation
                triangulationData = InsertNewPointInTriangulation(p, triangulationData, ref missedPoints, ref flippedEdges);
            }



            //Step 8. Delete the vertices belonging to the supertriangle
            RemoveSupertriangle(superTriangle, triangulationData);



            //Step 9.Reset the coordinates to their original values because they are currently in the range (0,1)
            foreach (HalfEdgeVertex v in triangulationData.vertices)
            {
                float xUnNormalized = (v.position.x * d_max) + boundingBox.minX;
                float zUnNormalized = (v.position.z * d_max) + boundingBox.minY;

                v.position = new Vector3(xUnNormalized, 0f, zUnNormalized);
            }


            string meshDataString = "Delaunay with sloan created a triangulation with: ";

            meshDataString += "Faces: " + triangulationData.faces.Count;
            meshDataString += " - Vertices: " + triangulationData.vertices.Count;
            meshDataString += " - Edges: " + triangulationData.edges.Count;
            meshDataString += " - Flipped egdes: " + flippedEdges;
            meshDataString += " - Missed points: " + missedPoints;

            Debug.Log(meshDataString);


            return triangulationData;
        }



        //Insert a new point in the triangulation
        //Can be used by other methods
        //Assumes all points are within the existing triangulation
        public static HalfEdgeData InsertNewPointInTriangulation(Vector3 p, HalfEdgeData triangulationData, ref int missedPoints, ref int flippedEdges)
        {
            //Find an existing triangle which encloses p
            HalfEdgeFace f = FindWhichTriangleAPointIsIn(p, null, triangulationData);

            //We couldnt find a triangle maybe because of floating point precision issues
            if (f == null)
            {
                missedPoints += 1;

                return triangulationData;
            }

            //Delete this triangle and form 3 new triangles by connecting p to each of the vertices on the old triangle
            HalfEdgeHelpMethods.SplitTriangleFace(f, p, triangulationData);


            //Step 6. Initialize stack. Place all triangles which are adjacent to the edges opposite p on a LIFO stack
            //The report says we should place triangles, but it's easier to place edges with our data structure 
            Stack<HalfEdge> trianglesToInvestigate = new Stack<HalfEdge>();

            AddTrianglesOppositePToStack(p, trianglesToInvestigate, triangulationData);


            //Step 7. Restore delaunay triangulation
            //While the stack is not empty
            int safety = 0;

            while (trianglesToInvestigate.Count > 0)
            {
                safety += 1;

                if (safety > 10000)
                {
                    Debug.Log("Stuck in infinite loop when restoring delaunay in incremental sloan algorithm");

                    break;
                }

                //Step 7.1. Remove a triangle from the stack
                HalfEdge edgeToTest = trianglesToInvestigate.Pop();

                //Step 7.2. 
                //If p is outside or on the circumcircle for this triangle, we have a delaunay triangle and can return to next loop
                Vector2 a = edgeToTest.v.position.XZ();
                Vector2 b = edgeToTest.prevEdge.v.position.XZ();
                Vector2 c = edgeToTest.nextEdge.v.position.XZ();
                Vector2 p_2d = p.XZ();

                //abc are here counter-clockwise
                if (_Delaunay.ShouldFlipEdgeStable(a, b, c, p_2d))
                {
                    HalfEdgeHelpMethods.FlipTriangleEdge(edgeToTest);

                    //Step 3. Place any triangles which are now opposite p on the stack
                    AddTrianglesOppositePToStack(p, trianglesToInvestigate, triangulationData);

                    flippedEdges += 1;
                }
            }

            return triangulationData;
        }



        //Different methods to find out in which traingle a point is in
        private static HalfEdgeFace FindWhichTriangleAPointIsIn(Vector3 p, HalfEdgeFace startTriangle, HalfEdgeData triangulationData)
        {
            HalfEdgeFace intersectingTriangle = null;

            //Alternative 1. Search through all triangles and use point-in-triangle
            //foreach (HalfEdgeFace f in triangulationData.faces)
            //{
            //    //The corners of this triangle
            //    Vector3 v1 = f.edge.v.position;
            //    Vector3 v2 = f.edge.nextEdge.v.position;
            //    Vector3 v3 = f.edge.nextEdge.nextEdge.v.position;

            //    //Is the point in this triangle?
            //    if (Intersections.IsPointInTriangle(v1.XZ(), v2.XZ(), v3.XZ(), p.XZ(), true))
            //    {
            //        intersectingTriangle = f;

            //        break;
            //    }
            //}


            //Alternative 2. Use a triangulation walk
            //Start at the triangle which was most recently created - this is why we should group the points into bins
            HalfEdgeFace currentTriangle = null;

            //We can feed it a start triangle to sometimes make the algorithm faster
            if (startTriangle != null)
            {
                currentTriangle = startTriangle;
            }
            //Find a random start triangle
            else
            {
                int randomPos = Random.Range(0, triangulationData.faces.Count);

                int i = 0;

                foreach (HalfEdgeFace f in triangulationData.faces)
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


            //Start the triangulation walk to find the intersecting triangle
            int safety = 0;

            while (true)
            {
                safety += 1;

                if (safety > 100000)
                {
                    Debug.Log("Stuck in endless loop when walking in triangulation");

                    break;
                }

                //Is the point intersecting with the current triangle?
                //We need to do 3 tests where each test is using the triangles edges
                //If the point is to the right of all edges, then it's inside the triangle
                HalfEdge e1 = currentTriangle.edge;
                HalfEdge e2 = e1.nextEdge;
                HalfEdge e3 = e2.nextEdge;

                //Check if the point is to the right or on the border of its edges, if so we know its inside this triangle
                //We treat the on-the-border case as if it is inside because the end result is the same
                if (IsPointToTheRightOrOnLine(e1.prevEdge.v.position.XZ(), e1.v.position.XZ(), p.XZ()))
                {
                    if (IsPointToTheRightOrOnLine(e2.prevEdge.v.position.XZ(), e2.v.position.XZ(), p.XZ()))
                    {
                        if (IsPointToTheRightOrOnLine(e3.prevEdge.v.position.XZ(), e3.v.position.XZ(), p.XZ()))
                        {
                            //We have found the triangle the point is in
                            intersectingTriangle = currentTriangle;

                            break;
                        }
                        else
                        {
                            currentTriangle = e3.oppositeEdge.face;
                        }
                    }
                    else
                    {
                        currentTriangle = e2.oppositeEdge.face;
                    }
                }
                //If to the left, move to this triangle and start the search over again
                else
                {
                    currentTriangle = e1.oppositeEdge.face;
                }

            }

            return intersectingTriangle;
        }



        //Is a point to the right or on the line a-b?
        private static bool IsPointToTheRightOrOnLine(Vector2 a, Vector2 b, Vector2 p)
        {
            bool isToTheRight = false;

            int pointPos = Geometry.GetPointPositionInRelationToLine(a, b, p);

            if (pointPos == 0 || pointPos == 1)
            {
                isToTheRight = true;
            }

            return isToTheRight;
        }



        //Find all triangles opposite of p
        private static void AddTrianglesOppositePToStack(Vector3 p, Stack<HalfEdge> trianglesToInvestigate, HalfEdgeData triangulationData)
        {
            //There might be a better way to do this
            //foreach (HalfEdgeVertex v in triangulationData.vertices)
            //{
            //    if (v.position == p)
            //    {
            //        //Each vertex references an half-edge that starts at this point
            //        HalfEdge oppositeEdge = v.edge.nextEdge.oppositeEdge;

            //        //This might happen if we are at the border
            //        //A stack might include duplicates so we have to check for that as well
            //        if (oppositeEdge != null && !trianglesToInvestigate.Contains(oppositeEdge))
            //        {
            //            trianglesToInvestigate.Push(oppositeEdge);
            //        }
            //    }
            //}

            //Find a vertex at position p and then rotate around it to find all opposite edges
            HalfEdgeVertex rotateAroundThis = null;

            foreach (HalfEdgeVertex v in triangulationData.vertices)
            {
                if (v.position == p)
                {
                    rotateAroundThis = v;
                }
            }

            //Which triangle is this vertex a part of, so we know when we have rotated all the way around
            HalfEdgeFace tStart = rotateAroundThis.edge.face;

            HalfEdgeFace tCurrent = null;

            int safety = 0;

            while (tCurrent != tStart)
            {
                safety += 1;

                if (safety > 1000)
                {
                    Debug.Log("Stuck in endless loop when finding opposite edges");

                    break;
                }

                //Try add the edge thats opposite to p if it doesn't exist
                HalfEdge edgeOppositeRotateVertex = rotateAroundThis.edge.nextEdge.oppositeEdge;

                //Null might happen if we are at the border
                //A stack might include duplicates so we have to check for that as well
                if (edgeOppositeRotateVertex != null && !trianglesToInvestigate.Contains(edgeOppositeRotateVertex))
                {
                    trianglesToInvestigate.Push(edgeOppositeRotateVertex);
                }

                //Rotate left - this assumes we can always rotate left so no holes are allowed
                //and neither can we investigate one of the vertices thats a part of the supertriangle
                //which we dont need to worry about because p is never a part of the supertriangle
                rotateAroundThis = rotateAroundThis.edge.oppositeEdge.v;

                //In which triangle are we now?
                tCurrent = rotateAroundThis.edge.face;
            }
        }



        //Remove the supertriangle
        private static void RemoveSupertriangle(Triangle superTriangle, HalfEdgeData triangulationData)
        {
            HashSet<HalfEdgeFace> trianglesToDelete = new HashSet<HalfEdgeFace>();

            foreach (HalfEdgeVertex v in triangulationData.vertices)
            {
                //If the face attached to this vertex already exists, we dont need to check it again
                if (trianglesToDelete.Contains(v.edge.face))
                {
                    continue;
                }
            
                Vector3 v1 = v.position;

                if (v1 == superTriangle.p1 || v1 == superTriangle.p2 || v1 == superTriangle.p3)
                {
                    trianglesToDelete.Add(v.edge.face);
                }
            }

            //Debug.Log("Triangles to delete: " + trianglesToDelete.Count);

            foreach (HalfEdgeFace t in trianglesToDelete)
            {
                HalfEdgeHelpMethods.DeleteTriangle(t, triangulationData, true);
            }
        }
    }
}
