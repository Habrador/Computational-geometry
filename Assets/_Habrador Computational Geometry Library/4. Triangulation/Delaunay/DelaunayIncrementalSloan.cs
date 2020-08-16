using System.Collections;
using System.Collections.Generic;
using System.Text;
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
            //We need more than 1 point to 
            if (points.Count < 2)
            {
                Debug.Log("Can make a delaunay with sloan with less than 2 points");

                return null;
            }

            

            //Step 1.Normalize the points to the range(0 - 1), which assumes we have more than 1 point
            //Is not being done here, we assume the points are already normalized



            //Step 2. Sort the points into bins to make it faster to find which triangle a point is in
            //TODO



            //Step 3. Establish the supertriangle
            //The report says that the supertriangle should be at (-100, 100) which is way
            //outside of the points which are in the range(0, 1)
            //So make sure you have NORMALIZED the points
            Triangle2 superTriangle = new Triangle2(new MyVector2(-100f, -100f), new MyVector2(100f, -100f), new MyVector2(0f, 100f));

            //Create the triangulation data with the only triangle we have
            HashSet<Triangle2> triangles = new HashSet<Triangle2>();

            triangles.Add(superTriangle);

            //Change to half-edge data structure
            _TransformBetweenDataStructures.Triangle2ToHalfEdge2(triangles, triangulationData);

            

            //Step 4. Loop over each point we want to insert and do Steps 5-7

            //These are for display purposes only
            int missedPoints = 0;
            int flippedEdges = 0;

            foreach (MyVector2 p in points)
            {
                //Step 5-7
                InsertNewPointInTriangulation(p, triangulationData, ref missedPoints, ref flippedEdges);
            }



            //Step 8. Delete the vertices belonging to the supertriangle
            RemoveSuperTriangle(superTriangle, triangulationData);



            //Step 9.Reset the coordinates to their original values because they are currently in the range (0,1)
            //Is being done outside of this method

            //TODO: replace this with StringBuilder 
            string meshDataString = "Delaunay with sloan created a triangulation with: ";

            meshDataString += "Faces: " + triangulationData.faces.Count;
            meshDataString += " - Vertices: " + triangulationData.vertices.Count;
            meshDataString += " - Edges: " + triangulationData.edges.Count;
            meshDataString += " - Flipped egdes: " + flippedEdges;
            meshDataString += " - Missed points: " + missedPoints;

            Debug.Log(meshDataString);


            return triangulationData;
        }



        //Insert a new point in the triangulation we already have, so we need at least one triangle
        public static void InsertNewPointInTriangulation(MyVector2 p, HalfEdgeData2 triangulationData, ref int missedPoints, ref int flippedEdges)
        {
            //Step 5. Insert the new point in the triangulation
            //Find the existing triangle the point is in
            HalfEdgeFace2 f = PointTriangulationIntersection.TriangulationWalk(p, null, triangulationData);

            //We couldnt find a triangle maybe because the point is not in the triangulation?
            if (f == null)
            {
                missedPoints += 1;
            }

            //Delete this triangle and form 3 new triangles by connecting p to each of the vertices in the old triangle
            HalfEdgeHelpMethods.SplitTriangleFaceAtPoint(f, p, triangulationData);


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

                if (safety > 1000000)
                {
                    Debug.Log("Stuck in infinite loop when restoring delaunay in incremental sloan algorithm");

                    break;
                }

                //Step 7.1. Remove a triangle from the stack
                HalfEdge2 edgeToTest = trianglesToInvestigate.Pop();

                //Step 7.2. Do we need to flip this edge? 
                //If p is outside or on the circumcircle for this triangle, we have a delaunay triangle and can return to next loop
                MyVector2 a = edgeToTest.v.position;
                MyVector2 b = edgeToTest.prevEdge.v.position;
                MyVector2 c = edgeToTest.nextEdge.v.position;
                
                //abc are here counter-clockwise
                if (DelaunayMethods.ShouldFlipEdgeStable(a, b, c, p))
                {
                    HalfEdgeHelpMethods.FlipTriangleEdge(edgeToTest);

                    //Step 7.3. Place any triangles which are now opposite p on the stack
                    AddTrianglesOppositePToStack(p, trianglesToInvestigate, triangulationData);

                    flippedEdges += 1;
                }
            }
        }



        //Find all triangles opposite of vertex p
        //But we will find all edges opposite to p, and from these edges we can find the triangles
        private static void AddTrianglesOppositePToStack(MyVector2 p, Stack<HalfEdge2> trianglesOppositeP, HalfEdgeData2 triangulationData)
        {
            //Find a vertex at position p and then rotate around it, triangle-by-triangle, to find all opposite edges
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

                if (safety > 10000)
                {
                    Debug.Log("Stuck in endless loop when finding opposite edges in Delaunay Sloan");

                    break;
                }

                //The edge opposite to p
                HalfEdge2 edgeOppositeRotateVertex = rotateAroundThis.edge.nextEdge.oppositeEdge;

                //Try to add the edge to the list iof triangles we are interested in 
                //Null might happen if we are at the border
                //A stack might include duplicates so we have to check for that as well
                if (edgeOppositeRotateVertex != null && !trianglesOppositeP.Contains(edgeOppositeRotateVertex))
                {
                    trianglesOppositeP.Push(edgeOppositeRotateVertex);
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
        private static void RemoveSuperTriangle(Triangle2 superTriangle, HalfEdgeData2 triangulationData)
        {
            //The super triangle doesnt exists anymore because we have split it into many new triangles
            //But we can use its vertices to figure out which new triangles (or faces belonging to the triangle) 
            //we should delete
        
            HashSet<HalfEdgeFace2> triangleFacesToDelete = new HashSet<HalfEdgeFace2>();

            //Loop through all vertices belongin to the triangulation
            foreach (HalfEdgeVertex2 v in triangulationData.vertices)
            {
                //If the face attached to this vertex already exists in the list of faces we want to delete
                //Then dont add it again
                if (triangleFacesToDelete.Contains(v.edge.face))
                {
                    continue;
                }

                MyVector2 v1 = v.position;

                //Is this vertex in the triangulation a vertex in the super triangle?
                if (v1.Equals(superTriangle.p1) || v1.Equals(superTriangle.p2) || v1.Equals(superTriangle.p3))
                {
                    triangleFacesToDelete.Add(v.edge.face);
                }
            }

            //Debug.Log("Triangles to delete: " + trianglesToDelete.Count);

            //Delete the new triangles with vertices attached to the super triangle
            foreach (HalfEdgeFace2 f in triangleFacesToDelete)
            {
                HalfEdgeHelpMethods.DeleteTriangleFace(f, triangulationData, shouldSetOppositeToNull: true);
            }
        }
    }
}
