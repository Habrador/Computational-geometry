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
        public static HalfEdgeData2 GenerateTriangulation(HashSet<MyVector2> points, HalfEdgeData2 triangulationData)
        {
            //We need more than 1 point to normalize
            if (points.Count < 2)
            {
                Debug.Log("Can make a delaunay with sloan with less than 2 points");

                //return null;
            }

            //Debug.Log("Hello");

            //Step 1.Normalize the points to the range(0 - 1), which assumes we have more than 1 point
            //This will lower the floating point precision when unnormalizing again, so we might have to go through
            //all points in the end and make sure they have the correct coordinate
            //Better to normalize and unnormalize outside of this method to make it more standardized
            //AABB boundingBox = HelpMethods.GetAABB(new List<MyVector2>(inputPoints));

            //float dMax = HelpMethods.CalculateDMax(boundingBox);

            //HashSet<MyVector2> points = new HashSet<MyVector2>();

            //foreach (MyVector2 p in inputPoints)
            //{
            //    points.Add(HelpMethods.NomalizePoint(p, boundingBox, dMax));
            //}



            //Step 2. Sort the points into bins to make it faster to find which triangle a point is in
            //TODO



            //Step 3. Establish the supertriangle
            //The report argues that the supertriangle should be at (-100, 100) which is way
            //outside of the points which are in the range(0, 1)
            //It's important to save this triangle so we can delete it when we are done
            Triangle2 superTriangle = new Triangle2(new MyVector2(-100f, -100f), new MyVector2(100f, -100f), new MyVector2(0f, 100f));

            //Create the triangulation data with the only triangle we have
            HashSet<Triangle2> triangles = new HashSet<Triangle2>();

            triangles.Add(superTriangle);

            TransformBetweenDataStructures.Triangle2ToHalfEdge2(triangles, triangulationData);

            

            //Step 4. Loop over each point we want to insert and do Steps 5-7
            int missedPoints = 0;
            int flippedEdges = 0;

            foreach (MyVector2 p in points)
            {
                //Step 5. Insert the new point in the triangulation
                triangulationData = InsertNewPointInTriangulation(p, triangulationData, ref missedPoints, ref flippedEdges);
            }



            //Step 8. Delete the vertices belonging to the supertriangle
            RemoveSupertriangle(superTriangle, triangulationData);



            //Step 9.Reset the coordinates to their original values because they are currently in the range (0,1)
            //foreach (HalfEdgeVertex2 v in triangulationData.vertices)
            //{
            //    MyVector2 vUnnNormalized = HelpMethods.UnNomalizePoint(v.position, boundingBox, dMax);

            //    v.position = vUnnNormalized;
            //}


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
        public static HalfEdgeData2 InsertNewPointInTriangulation(MyVector2 p, HalfEdgeData2 triangulationData, ref int missedPoints, ref int flippedEdges)
        {
            //Find an existing triangle which encloses p
            HalfEdgeFace2 f = FindWhichTriangleAPointIsIn(p, null, triangulationData);

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
            Stack<HalfEdge2> trianglesToInvestigate = new Stack<HalfEdge2>();

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
                HalfEdge2 edgeToTest = trianglesToInvestigate.Pop();

                //Step 7.2. 
                //If p is outside or on the circumcircle for this triangle, we have a delaunay triangle and can return to next loop
                MyVector2 a = edgeToTest.v.position;
                MyVector2 b = edgeToTest.prevEdge.v.position;
                MyVector2 c = edgeToTest.nextEdge.v.position;
                

                //abc are here counter-clockwise
                if (DelaunayMethods.ShouldFlipEdgeStable(a, b, c, p))
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
        private static HalfEdgeFace2 FindWhichTriangleAPointIsIn(MyVector2 p, HalfEdgeFace2 startTriangle, HalfEdgeData2 triangulationData)
        {
            HalfEdgeFace2 intersectingTriangle = null;

            //Alternative 1. Search through all triangles and use point-in-triangle
            /*
            foreach (HalfEdgeFace2 f in triangulationData.faces)
            {
                //The corners of this triangle
                MyVector2 v1 = f.edge.v.position;
                MyVector2 v2 = f.edge.nextEdge.v.position;
                MyVector2 v3 = f.edge.nextEdge.nextEdge.v.position;

                Triangle2 t = new Triangle2(v1, v2, v3);

                //Is the point in this triangle?
                if (Intersections.PointTriangle(t, p, true))
                {
                    intersectingTriangle = f;

                    break;
                }
            }
            */
            
            //Alternative 2. Use a triangulation walk
            //Start at the triangle which was most recently created - this is why we should group the points into bins
            HalfEdgeFace2 currentTriangle = null;

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
                HalfEdge2 e1 = currentTriangle.edge;
                HalfEdge2 e2 = e1.nextEdge;
                HalfEdge2 e3 = e2.nextEdge;

                //Check if the point is to the right or on the border of its edges, if so we know its inside this triangle
                //We treat the on-the-border case as if it is inside because the end result is the same
                if (IsPointToTheRightOrOnLine(e1.prevEdge.v.position, e1.v.position, p))
                {
                    if (IsPointToTheRightOrOnLine(e2.prevEdge.v.position, e2.v.position, p))
                    {
                        if (IsPointToTheRightOrOnLine(e3.prevEdge.v.position, e3.v.position, p))
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
        private static bool IsPointToTheRightOrOnLine(MyVector2 a, MyVector2 b, MyVector2 p)
        {
            bool isToTheRight = false;

            LeftOnRight pointPos = Geometry.IsPoint_Left_On_Right_OfVector(a, b, p);

            if (pointPos == LeftOnRight.Right || pointPos == LeftOnRight.On)
            {
                isToTheRight = true;
            }

            return isToTheRight;
        }



        //Find all triangles opposite of p
        private static void AddTrianglesOppositePToStack(MyVector2 p, Stack<HalfEdge2> trianglesToInvestigate, HalfEdgeData2 triangulationData)
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
            HalfEdgeVertex2 rotateAroundThis = null;

            foreach (HalfEdgeVertex2 v in triangulationData.vertices)
            {
                if (v.position.Equals(p))
                {
                    rotateAroundThis = v;
                }
            }

            //Which triangle is this vertex a part of, so we know when we have rotated all the way around
            HalfEdgeFace2 tStart = rotateAroundThis.edge.face;

            HalfEdgeFace2 tCurrent = null;

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
                HalfEdge2 edgeOppositeRotateVertex = rotateAroundThis.edge.nextEdge.oppositeEdge;

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
        private static void RemoveSupertriangle(Triangle2 superTriangle, HalfEdgeData2 triangulationData)
        {
            HashSet<HalfEdgeFace2> trianglesToDelete = new HashSet<HalfEdgeFace2>();

            foreach (HalfEdgeVertex2 v in triangulationData.vertices)
            {
                //If the face attached to this vertex already exists, we dont need to check it again
                if (trianglesToDelete.Contains(v.edge.face))
                {
                    continue;
                }

                MyVector2 v1 = v.position;

                if (v1.Equals(superTriangle.p1) || v1.Equals(superTriangle.p2) || v1.Equals(superTriangle.p3))
                {
                    trianglesToDelete.Add(v.edge.face);
                }
            }

            //Debug.Log("Triangles to delete: " + trianglesToDelete.Count);

            foreach (HalfEdgeFace2 t in trianglesToDelete)
            {
                HalfEdgeHelpMethods.DeleteTriangle(t, triangulationData, true);
            }
        }
    }
}
