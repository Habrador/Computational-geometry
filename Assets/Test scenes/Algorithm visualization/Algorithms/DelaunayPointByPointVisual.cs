using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;



//Visualizes delaunay triangulation algorithm "point-by-point" in steps
public class DelaunayPointByPointVisual : MonoBehaviour
{
    VisualizerController controller;



    public void StartVisualizer(HashSet<MyVector2> points, HalfEdgeData2 triangulationData)
    {
        controller = GetComponent<VisualizerController>();


        //Step 3. Establish the supertriangle
        //The report says that the supertriangle should be at (-100, 100) which is way
        //outside of the points which are in the range(0, 1)
        Triangle2 superTriangle = new Triangle2(new MyVector2(-100f, -100f), new MyVector2(100f, -100f), new MyVector2(0f, 100f));

        //Create the triangulation data with the only triangle we have
        HashSet<Triangle2> triangles = new HashSet<Triangle2>();

        triangles.Add(superTriangle);

        //Change to half-edge data structure
        _TransformBetweenDataStructures.Triangle2ToHalfEdge2(triangles, triangulationData);



        //Start the visualization
        StartCoroutine(InsertPoints(points, triangulationData, superTriangle));
    }



    IEnumerator InsertPoints(HashSet<MyVector2> points, HalfEdgeData2 triangulationData, Triangle2 superTriangle)
    {
        //VISUALZ
        ShowTriangles(triangulationData);

        //VISUALZ - dont show the colored mesh until its finished because its flickering
        controller.shouldDisplayColoredMesh = false;

        yield return new WaitForSeconds(controller.pauseTime);


        //Step 4. Loop over each point we want to insert and do Steps 5-7

        //These are for display purposes only
        int missedPoints = 0;
        int flippedEdges = 0;

        foreach (MyVector2 p in points)
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


            //VISUALZ
            //Display the point as a black circle
            ShowCircle(p);

            yield return new WaitForSeconds(controller.pauseTime);

            ShowTriangles(triangulationData);

            yield return new WaitForSeconds(controller.pauseTime);


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

                    //VISUALZ
                    controller.flipText.text = "Flipped edges: " + flippedEdges;

                    ShowTriangles(triangulationData);

                    yield return new WaitForSeconds(controller.pauseTime);
                }
            }
        }

        
        //Dont show the last point we added
        controller.ResetBlackMeshes();


        //Step 8. Delete the vertices belonging to the supertriangle
        StartCoroutine(RemoveSuperTriangle(superTriangle, triangulationData));

        yield return null;
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
    IEnumerator RemoveSuperTriangle(Triangle2 superTriangle, HalfEdgeData2 triangulationData)
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


            //VISUALZ
            ShowTriangles(triangulationData);

            yield return new WaitForSeconds(controller.pauseTime);
        }


        //VISUALZ - show the colored mesh when its finished
        controller.shouldDisplayColoredMesh = true;

        yield return null;
    }



    //
    // Visualz
    //

    //Show triangles
    private void ShowTriangles(HalfEdgeData2 triangles)
    {
        controller.ResetMultiColoredMeshes();

        List<Mesh> meshes = controller.GenerateTriangulationMesh(triangles, shouldUnNormalize: true);

        List<Material> materials = controller.GenerateRandomMaterials(meshes.Count);

        controller.multiColoredMeshes = meshes;
        controller.multiColoredMeshesMaterials = materials;
    }

    //Show current active point where we split triangle
    private void ShowCircle(MyVector2 p)
    {
        controller.ResetBlackMeshes();

        HashSet<Triangle2> triangles = _GenerateMesh.Circle(controller.UnNormalize(p), 0.2f, 10);

        List<Mesh> meshes = controller.GenerateTriangulationMesh(triangles, shouldUnNormalize: false);

        controller.blackMeshes = meshes;
    }
}
