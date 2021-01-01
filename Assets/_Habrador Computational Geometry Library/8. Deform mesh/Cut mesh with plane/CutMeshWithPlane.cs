using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Cut a meth with a plane
    //TODO:
    //- Remove small edges on the cut edge to get a better triangulation by measuring the length of each edge. This should also fix problem with ugly normals. They are also causing trouble when we identify hole-edges, so sometimes we get small triangles as separate meshes
    //- Normalize the data to 0-1 to avoid floating point precision issues
    //- Is failing if the mesh we cut has holes in it at the bottom, and the mesh intersects with one of those holes. But that's not a problem because then we can't fill the hole anyway! Maybe we can fix that by finding a better way to identify the different holes
    //- Can we use DOTS/GPU/threads to improve performance? Several sub-algorithms can be done in parallell

    //- Time measurements for optimizations (bunny):
    //- AABB-plane test: 0.005
    //- Separate meshes into outside/inside plane: 0.01
    //- Connect opposite edges: 0.004
    //- Remove small edges: 
    //- Identify and fill holes: 0.015
    //- Find mesh islands: 0.012
    //- Connect hole with mesh: 0.001
    public static class CutMeshWithPlane 
    {
        //Should return null if the mesh couldn't be cut because it doesn't intersect with the plane
        //Cant handle sub-meshes, but they should be avoided anyway because of performance reasons!
        //Otherwise it should return the new meshes
        //meshTrans is needed so we can transform the cut plane to the mesh's local space 
        //halfEdgeMeshData should thus be in local space
        public static List<HalfEdgeData3> CutMesh(Transform meshTrans, HalfEdgeData3 halfEdgeMeshData, OrientedPlane3 orientedCutPlaneGlobal, bool fillHoles)
        {
            //
            // Validate the input data
            //

            if (meshTrans == null)
            {
                Debug.Log("There's no transform to cut");

                return null;
            }

            if (halfEdgeMeshData == null)
            {
                Debug.Log("There's no mesh to cut");

                return null;
            }

            //Used only for finding optimization problems
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();



            //
            // Check if the AABB of the mesh is intersecting with the plane. Otherwise we can't cut the mesh, so its a waste of time
            //
            timer.Start();

            //The plane with just a normal
            Plane3 cutPlaneGlobal = orientedCutPlaneGlobal.Plane3;

            bool isIntersecting = IsMeshAABBIntersectingWithPlane(meshTrans, cutPlaneGlobal);

            if (!isIntersecting)
            {
                Debug.Log("This mesh's AABB didn't intersect with the plane, so we couldn't cut it.");
            
                return null;
            }

            Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to do the AABB-plane intersection test");



            //
            // Separate the old mesh into two new meshes (or just one if the mesh is not intersecting with the plane)
            //
            timer.Restart();

            //The two meshes we might end up with after the cut
            //One is "outside" of the plane and another is "inside" the plane
            HalfEdgeData3 newMeshO = new HalfEdgeData3();
            HalfEdgeData3 newMeshI = new HalfEdgeData3();

            //Save the new edges we add when cutting triangles that intersects with the plane
            //Needs to be edges so we can later connect them with each other to fill the hole
            //And to remove small triangles
            //We only need to save the outside edges, because we can identify the inside edges because each edge has an opposite edge
            HashSet<HalfEdge3> cutEdgesO = new HashSet<HalfEdge3>();

            //Transform the plane from global space to local space of the mesh
            //which is faster than transforming the mesh from local space to global space 
            MyVector3 planePosLocal = meshTrans.InverseTransformPoint(cutPlaneGlobal.pos.ToVector3()).ToMyVector3();
            MyVector3 planeNormalLocal = meshTrans.InverseTransformDirection(cutPlaneGlobal.normal.ToVector3()).ToMyVector3();

            Plane3 cutPlaneLocal = new Plane3(planePosLocal, planeNormalLocal);
            
            //This new meshes might have islands (and thus be not connected) but we check for that later
            SeparateMeshWithPlane(halfEdgeMeshData, newMeshO, newMeshI, cutPlaneLocal, cutEdgesO);

            Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to separate the meshes");

            //Generate new meshes only if the old mesh intersected with the plane
            if (newMeshO.faces.Count == 0 || newMeshI.faces.Count == 0)
            {
                Debug.Log("This mesh didn't intersect with the plane, so we couldn't cut it.");
            
                return null;
            }



            //
            // Find opposite edge to each edge
            //
            timer.Restart();

            //Most edges should already have an opposite edge, but we need to connected some of the new triangles edges with each other
            //This can maybe be improved because we know which edges have no connection and we could just search through those
            newMeshO.ConnectAllEdgesFast();
            newMeshI.ConnectAllEdgesFast();

            Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to connect the opposite edges");


            //Display all edges which have no opposite for debugging
            //Remember that this will NOT display the holes because the hole-edges are connected across the border
            //DebugHalfEdge.DisplayEdgesWithNoOpposite(newMeshO.edges, meshTrans, Color.white);
            //DebugHalfEdge.DisplayEdgesWithNoOpposite(newMeshI.edges, meshTrans, Color.white);

            //This will display the hole(s)
            //DebugHalfEdge.DisplayEdges(cutEdgesO, meshTrans, Color.white);



            //
            // Remove small triangles at the seam where we did the cut
            //
            timer.Restart();

            //The small edges may cause shading issues and the fewer edges we have the faster it will take to fill the holes
            //RemoveSmallTriangles(F_Mesh, newEdges);

            Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to remove small edges");



            //
            // Identify all holes and fill the holes
            //
            HashSet<CutMeshHole> allHoles = null;

            if (fillHoles)
            {
                timer.Restart();

                allHoles = FillHoles(cutEdgesO, orientedCutPlaneGlobal, meshTrans, planeNormalLocal);

                Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to identify and fill holes");
            }



            //
            // Separate the meshes (they are still connected in the half-edge data structure at the cut edge)
            //
            
            foreach (HalfEdge3 e in cutEdgesO)
            {
                if (e.oppositeEdge != null)
                {
                    HalfEdge3 eOpposite = e.oppositeEdge;

                    //Remove the connection
                    e.oppositeEdge = null;
                    eOpposite.oppositeEdge = null;
                }
            }



            //
            // Split each mesh into separate meshes if the original mesh is not connected, meaning it has islands
            //
            timer.Restart();

            HashSet<HalfEdgeData3> newMeshesO = SeparateMeshIslands(newMeshO);
            HashSet<HalfEdgeData3> newMeshesI = SeparateMeshIslands(newMeshI);

            Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to find mesh islands");



            //
            // Connect each hole mesh with respective mesh
            //
            if (fillHoles)
            {
                timer.Restart();

                //It should be faster to do this after identifying each mesh island 
                //because that process requires flood-filling which is slower the more triangles each mesh has
                AddHolesToMeshes(newMeshesO, newMeshesI, allHoles);

                Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to match hole with mesh");
            }



            //
            // Combine the inside and outside meshes
            //

            List<HalfEdgeData3> allNewMeshes = new List<HalfEdgeData3>();

            allNewMeshes.AddRange(newMeshesO);
            allNewMeshes.AddRange(newMeshesI);

            

            return allNewMeshes;
        }



        //
        // Separates a mesh by a plane
        //
        private static void SeparateMeshWithPlane(HalfEdgeData3 halfEdgeMeshData, HalfEdgeData3 newMeshO, HalfEdgeData3 newMeshI, Plane3 cutPlaneLocal, HashSet<HalfEdge3> cutEdgesO)
        {
            //Loop through all triangles in the mesh
            HashSet<HalfEdgeFace3> triangles = halfEdgeMeshData.faces;

            foreach (HalfEdgeFace3 triangle in triangles)
            {
                //The verts in this triangles
                HalfEdgeVertex3 v1 = triangle.edge.v;
                HalfEdgeVertex3 v2 = triangle.edge.nextEdge.v;
                HalfEdgeVertex3 v3 = triangle.edge.nextEdge.nextEdge.v;

                //Check on which side of the plane these vertices are
                bool is_p1_outside = _Geometry.IsPointOutsidePlane(v1.position, cutPlaneLocal);
                bool is_p2_outside = _Geometry.IsPointOutsidePlane(v2.position, cutPlaneLocal);
                bool is_p3_outside = _Geometry.IsPointOutsidePlane(v3.position, cutPlaneLocal);


                //Build triangles belonging to respective mesh

                //All are outside the plane (no cut needed)
                if (is_p1_outside && is_p2_outside && is_p3_outside)
                {
                    newMeshO.AddTriangle(triangle, findOppositeEdge: false);
                }
                //All are inside the plane (no cut needed)
                else if (!is_p1_outside && !is_p2_outside && !is_p3_outside)
                {
                    newMeshI.AddTriangle(triangle, findOppositeEdge: false);
                }
                //The vertices are on different sides of the plane, so we need to cut the triangle into 3 new triangles
                else
                {
                    //We get 6 cases where each vertex is on its own outside or inside the plane


                    //p1 is outside
                    if (is_p1_outside && !is_p2_outside && !is_p3_outside)
                    {
                        CutTriangleOneOutside(v1, v2, v3, newMeshO, newMeshI, cutEdgesO, cutPlaneLocal);
                    }
                    //p1 is inside
                    else if (!is_p1_outside && is_p2_outside && is_p3_outside)
                    {
                        CutTriangleTwoOutside(v2, v3, v1, newMeshO, newMeshI, cutEdgesO, cutPlaneLocal);
                    }

                    //p2 is outside
                    else if (!is_p1_outside && is_p2_outside && !is_p3_outside)
                    {
                        CutTriangleOneOutside(v2, v3, v1, newMeshO, newMeshI, cutEdgesO, cutPlaneLocal);
                    }
                    //p2 is inside
                    else if (is_p1_outside && !is_p2_outside && is_p3_outside)
                    {
                        CutTriangleTwoOutside(v3, v1, v2, newMeshO, newMeshI, cutEdgesO, cutPlaneLocal);
                    }

                    //p3 is outside
                    else if (!is_p1_outside && !is_p2_outside && is_p3_outside)
                    {
                        CutTriangleOneOutside(v3, v1, v2, newMeshO, newMeshI, cutEdgesO, cutPlaneLocal);
                    }
                    //p3 is inside
                    else if (is_p1_outside && is_p2_outside && !is_p3_outside)
                    {
                        CutTriangleTwoOutside(v1, v2, v3, newMeshO, newMeshI, cutEdgesO, cutPlaneLocal);
                    }

                    //Something is strange if we end up here...
                    else
                    {
                        Debug.Log("No case was gound where we split triangle into 3 new triangles");
                    }
                }
            }
        }



        //We have holes and we have meshes with empty holes, this will pair them together
        private static void AddHolesToMeshes(HashSet<HalfEdgeData3> newMeshesO, HashSet<HalfEdgeData3> newMeshesI, HashSet<CutMeshHole> allHoles)
        {
            //This may happen if the original mesh has holes in it and the plane intersects one of those holes. 
            //Then we can't identify the hole as closed and we can't repair the hole anyway 
            if (allHoles == null)
            {
                return;
            }
        
            foreach (CutMeshHole hole in allHoles)
            {
                HalfEdge3 holeEdgeI = hole.holeEdgeI;
                HalfEdge3 holeEdgeO = hole.holeEdgeO;

                //Outside
                foreach (HalfEdgeData3 mesh in newMeshesO)
                {
                    //This edge was generated when we created the infrastructure
                    //So we just have to check if it's a part of the mesh
                    if (mesh.edges.Contains(holeEdgeO))
                    {
                        mesh.MergeMesh(hole.holeMeshO);
                    }
                }

                //Inside
                foreach (HalfEdgeData3 mesh in newMeshesI)
                {
                    if (mesh.edges.Contains(holeEdgeI))
                    {
                        mesh.MergeMesh(hole.holeMeshI);
                    }
                }
            }
        }



        //
        // Separate a mesh by its islands (if it has islands)
        //

        private static HashSet<HalfEdgeData3> SeparateMeshIslands(HalfEdgeData3 meshData)
        {
            //System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            
            HashSet<HalfEdgeData3> meshIslands = new HashSet<HalfEdgeData3>();

            HashSet<HalfEdgeFace3> allFaces = meshData.faces;
            

            //Separate by flood-filling

            //Reset the bool (which might be true if we have done operations on this mesh before)
            //This takes 0 seconds
            foreach (HalfEdgeFace3 f in allFaces)
            {
                f.hasVisisted = false;
            }
            
            //Faces belonging to a separate island
            HashSet<HalfEdgeFace3> facesOnThisIsland = new HashSet<HalfEdgeFace3>();

            //Faces we havent flooded from yet
            Queue<HalfEdgeFace3> facesToFloodFrom = new Queue<HalfEdgeFace3>();

            //Add a first face to the queue to start the flooding
            HalfEdgeFace3 firstFace = allFaces.FakePop();

            facesToFloodFrom.Enqueue(firstFace);

            int numberOfIslands = 0;

            //Help list to make it flooding from triangle easier 
            List<HalfEdge3> edges = new List<HalfEdge3>();

            int safety = 0;
            
            while (true)
            {
                //If the queue is empty, it means we have flooded this island
                if (facesToFloodFrom.Count == 0)
                {
                    numberOfIslands += 1;
                    
                    //Generate the new half-edge data structure from the faces that belong to this island
                    HalfEdgeData3 meshIsland = HalfEdgeData3.GenerateHalfEdgeDataFromFaces(facesOnThisIsland);
                   
                    meshIslands.Add(meshIsland);

                    //We still have triangles to visit, so they must be on a new island, so restart
                    if (allFaces.Count > 0)
                    {
                        facesOnThisIsland = new HashSet<HalfEdgeFace3>();

                        //Add a first face to the queue
                        firstFace = allFaces.FakePop();

                        facesToFloodFrom.Enqueue(firstFace);
                    }
                    //No more triangles to visit, so we have identified all islands! 
                    else
                    {
                        Debug.Log($"This mesh has {numberOfIslands} islands");
                    
                        break;
                    }
                }

                //Pick a triangle to flood from 
                HalfEdgeFace3 f = facesToFloodFrom.Dequeue();

                f.hasVisisted = true;

                facesOnThisIsland.Add(f);

                //Remove from the original mesh so we can identify that we have found all islands
                allFaces.Remove(f);
                
                //Find neighboring triangles 
                edges.Clear();

                edges.Add(f.edge);
                edges.Add(f.edge.nextEdge);
                edges.Add(f.edge.nextEdge.nextEdge);
                
                foreach (HalfEdge3 e in edges)
                {
                    //We cant flood across this edge if it doesn't have a neighbor
                    if (e.oppositeEdge != null)
                    {
                        HalfEdgeFace3 fNeighbor = e.oppositeEdge.face;

                        //If we haven't visited this triangle before...
                        if (!fNeighbor.hasVisisted)
                        {
                            facesToFloodFrom.Enqueue(fNeighbor);

                            fNeighbor.hasVisisted = true;
                        }

                        //This is 0.02 seconds slower for the bunny
                        //if (!facesOnThisIsland.Contains(fNeighbor) && !facesToFloodFrom.Contains(fNeighbor))
                        //{
                        //    //...we will flood from it in the near future
                        //    facesToFloodFrom.Enqueue(fNeighbor);
                        //}
                    } 
                }
                

                safety += 1;

                if (safety > 50000)
                {
                    Debug.Log("Stuck in infinite loop when generating mesh islands");

                    break;
                }
            }

            
            
            //Debug.Log($"Whatever we timed took {timer.ElapsedMilliseconds / 1000f} seconds");

            return meshIslands;
        }



        //
        // Identify holes and fill holes with mesh
        //

        //Fill the hole (or holes) in the mesh
        private static HashSet<CutMeshHole> FillHoles(HashSet<HalfEdge3> holeEdgesO, OrientedPlane3 orientedCutPlane, Transform meshTrans, MyVector3 planeNormalLocal)
        {
            if (holeEdgesO == null)
            {
                Debug.Log("This mesh has no hole");

                return null;
            }


            //Find all separate holes
            HashSet<List<HalfEdge3>> allHoles = IdentifySeparateHoles(holeEdgesO);
            
            if (allHoles.Count == 0)
            {
                Debug.LogWarning("Couldn't identify any holes even though we have hole edges");

                return null;
            }

            //Debug
            //foreach (List<HalfEdge3> hole in allHoles)
            //{
            //    DebugHalfEdge.DisplayEdges(new HashSet<HalfEdge3>(hole), meshTrans, Color.white);
            //}


            //Fill the hole with a mesh
            HashSet<CutMeshHole> allHoleMeshes = new HashSet<CutMeshHole>();

            foreach (List<HalfEdge3> hole in allHoles)
            {
                HalfEdgeData3 holeMeshI = new HalfEdgeData3();
                HalfEdgeData3 holeMeshO = new HalfEdgeData3();

                //Transform vertices to local position of the cut plane to make it easier to triangulate with Ear Clipping
                //Ear CLipping wants vertices in 2d
                List<MyVector2> sortedVertices_2D = new List<MyVector2>();

                Transform planeTrans = orientedCutPlane.planeTrans;

                foreach (HalfEdge3 e in hole)
                {
                    MyVector3 pMeshSpace = e.v.position;

                    //Mesh space to Global space
                    Vector3 pGlobalSpace = meshTrans.TransformPoint(pMeshSpace.ToVector3());

                    //Global space to Plane space
                    Vector3 pPlaneSpace = planeTrans.InverseTransformPoint(pGlobalSpace);

                    //Y is normal direction so should be 0
                    MyVector2 p2D = new MyVector2(pPlaneSpace.x, pPlaneSpace.z);

                    sortedVertices_2D.Add(p2D);
                }


                //Triangulate with Ear Clipping

                //Need to reverse to standardize for the Ear Elipping algorithm
                sortedVertices_2D.Reverse();

                HashSet<Triangle2> triangles = _EarClipping.Triangulate(sortedVertices_2D, null, optimizeTriangles: false);

                //Debug.Log($"Number of triangles from Ear Clipping: {triangles.Count}");

                //Transform triangles to mesh space and half-edge data structure
                foreach (Triangle2 t in triangles)
                {
                    //2d to 3d space
                    Vector3 p1 = new Vector3(t.p1.x, 0f, t.p1.y);
                    Vector3 p2 = new Vector3(t.p2.x, 0f, t.p2.y);
                    Vector3 p3 = new Vector3(t.p3.x, 0f, t.p3.y);

                    //Plane space to Global space
                    Vector3 p1Global = planeTrans.TransformPoint(p1);
                    Vector3 p2Global = planeTrans.TransformPoint(p2);
                    Vector3 p3Global = planeTrans.TransformPoint(p3);

                    //Global space to Mesh space
                    Vector3 p1Mesh = meshTrans.InverseTransformPoint(p1Global);
                    Vector3 p2Mesh = meshTrans.InverseTransformPoint(p2Global);
                    Vector3 p3Mesh = meshTrans.InverseTransformPoint(p3Global);

                    //For inside mesh
                    MyMeshVertex v1_I = new MyMeshVertex(p1Mesh.ToMyVector3(), planeNormalLocal);
                    MyMeshVertex v2_I = new MyMeshVertex(p2Mesh.ToMyVector3(), planeNormalLocal);
                    MyMeshVertex v3_I = new MyMeshVertex(p3Mesh.ToMyVector3(), planeNormalLocal);

                    //For outside mesh
                    MyMeshVertex v1_O = new MyMeshVertex(p1Mesh.ToMyVector3(), -planeNormalLocal);
                    MyMeshVertex v2_O = new MyMeshVertex(p2Mesh.ToMyVector3(), -planeNormalLocal);
                    MyMeshVertex v3_O = new MyMeshVertex(p3Mesh.ToMyVector3(), -planeNormalLocal);

                    //Now we can finally add this triangle to the half-edge data structure
                    holeMeshI.AddTriangle(v1_I, v2_I, v3_I);
                    holeMeshO.AddTriangle(v1_O, v3_O, v2_O);
                }

                //Connect the opposite edges
                holeMeshI.ConnectAllEdgesFast();
                holeMeshO.ConnectAllEdgesFast();

                //We also need to save an edge belonging to the mesh to easier merge mesh with hole
                //The hole edges were generated by using edges in the outside mesh
                HalfEdge3 holeEdgeO = hole[0];
                HalfEdge3 holeEdgeI = holeEdgeO.oppositeEdge;

                CutMeshHole newHole = new CutMeshHole(holeMeshI, holeMeshO, holeEdgeI, holeEdgeO);

                allHoleMeshes.Add(newHole);
            }

            return allHoleMeshes;
        }



        //We might end up with multiple holes, so we need to identify all of them
        //Input is just a list of all edges that form the hole(s)
        //The output list is sorted so we can walk around the hole (if there was no empty hole in the mesh we cut)
        //Should return a list of half-edges because makes it faster to identify hole-mesh
        private static HashSet<List<HalfEdge3>> IdentifySeparateHoles(HashSet<HalfEdge3> cutEdgesOriginal)
        {
            HashSet<List<HalfEdge3>> allHoles = new HashSet<List<HalfEdge3>>();

            //Alternatively create a linked list, which should identify all holes automatically?
            //Which is better because this is currently failing because the hole may not be connected around if there's an empty hole in the mesh
            //Should return a list of half-edges because makes ut faster to identify hole-mesh
            //Can maybe borrow the half-edge data structure and set the opposite edge to the edge in the mesh...

            //Clone the list with cut edges because we need it to be intact
            HashSet<HalfEdge3> cutEdges = new HashSet<HalfEdge3>(cutEdgesOriginal);

            //Faster to just pick a start edge
            HalfEdge3 startEdge = cutEdges.FakePop();

            //Add it back so we can stop the algorithm
            cutEdges.Add(startEdge);


            List<HalfEdge3> sortedHoleEdges = new List<HalfEdge3>() { startEdge };
            //The first edge is needed to stop the algorithm, so don't remove it!
            //cutEdges.Remove(startEdge);


            //Then we can use the cutEdges to find our way around the hole's edge
            //We cant use the half-edge data structure because multiple edges may start at a vertex
            int safety = 0;

            while (true)
            {
                //Find an edge that starts at the last sorted cut edge
                HalfEdge3 nextEdge = null;

                MyVector3 lastPos = sortedHoleEdges[sortedHoleEdges.Count - 1].v.position;

                foreach (HalfEdge3 e in cutEdges)
                {
                    //A half-edge points to a vertex, so we want and edge going from the last vertex
                    if (e.prevEdge.v.position.Equals(lastPos))
                    {
                        nextEdge = e;

                        cutEdges.Remove(nextEdge);

                        break;
                    }
                }


                if (nextEdge == null)
                {
                    Debug.Log("Could not find a next edge when filling the hole");

                    break;
                }
                //The hole is back where it started
                else if (nextEdge == startEdge)
                {
                    //Debug.Log($"Number of edges to fill this hole: {sortedHoleEdges.Count}");

                    allHoles.Add(sortedHoleEdges);

                    //We have another hole
                    if (cutEdges.Count > 0)
                    {
                        startEdge = cutEdges.FakePop();

                        //Add it back so we can stop the algorithm
                        cutEdges.Add(startEdge);

                        //Start over with a new list
                        sortedHoleEdges = new List<HalfEdge3>() { startEdge };
                    }
                    //No more holes
                    else
                    {
                        Debug.Log($"The mesh has {allHoles.Count} holes");
                    
                        break;
                    }
                }
                else
                {
                    sortedHoleEdges.Add(nextEdge);
                }



                safety += 1;

                if (safety > 20000)
                {
                    Debug.Log("Stuck in infinite loop when finding connected cut edges");

                    //Debug.Log(sortedHoleEdges.Count);

                    break;
                }
            }

            return allHoles;
        }



        //
        // Cut triangle into 3 triangles
        //

        //Cut a triangle where one vertex is outside and the other vertices are inside
        //Make sure they are sorted clockwise: O1-I1-I2
        //O means that this vertex is outside of the plane
        private static void CutTriangleOneOutside(HalfEdgeVertex3 O1, HalfEdgeVertex3 I1, HalfEdgeVertex3 I2, HalfEdgeData3 newMeshO, HalfEdgeData3 newMeshI, HashSet<HalfEdge3> newEdgesO, Plane3 cutPlane)
        {
            //
            // Find where we should cut this triangle and the normal at those positions 
            //

            //Cut the triangle by using edge-plane intersection
            //Triangles in Unity are ordered clockwise, so form edges that intersects with the plane
            //The edges should always go from outside to inside, or we may end up with floating point precision issues
            Edge3 e_O1I1 = new Edge3(O1.position, I1.position);
            //Edge3 e_I1I2 = new Edge3(I1, I2); //Not needed because never intersects with the plane
            Edge3 e_I2O1 = new Edge3(O1.position, I2.position);

            //The positions of the intersection vertices
            MyVector3 pos_O1I1 = _Intersections.GetLinePlaneIntersectionPoint(cutPlane, e_O1I1);
            MyVector3 pos_I2O1 = _Intersections.GetLinePlaneIntersectionPoint(cutPlane, e_I2O1);

            //The normals of the intersection vertices
            float percentageBetween_O1I1 = MyVector3.Distance(O1.position, pos_O1I1) / MyVector3.Distance(O1.position, I1.position);
            float percentageBetween_I2O1 = MyVector3.Distance(I2.position, pos_I2O1) / MyVector3.Distance(I2.position, O1.position);

            MyVector3 normal_O1I1 = _Interpolation.Lerp(O1.normal, I1.normal, percentageBetween_O1I1);
            MyVector3 normal_I2O1 = _Interpolation.Lerp(I2.normal, O1.normal, percentageBetween_I2O1);

            //MyVector3 normal_F1B1 = Vector3.Slerp(F1.normal.ToVector3(), B1.normal.ToVector3(), percentageBetween_F1B1).ToMyVector3();
            //MyVector3 normal_B2F1 = Vector3.Slerp(B2.normal.ToVector3(), F1.normal.ToVector3(), percentageBetween_B2F1).ToMyVector3();

            normal_O1I1 = MyVector3.Normalize(normal_O1I1);
            normal_I2O1 = MyVector3.Normalize(normal_I2O1);

            

            //
            // Form 3 new triangles
            //

            //All vertices in the polygon (clockwise order) from which we will build three triangles
            MyMeshVertex vO1 = new MyMeshVertex(O1.position, O1.normal);
            MyMeshVertex vO1I1 = new MyMeshVertex(pos_O1I1, normal_O1I1);
            MyMeshVertex vI1 = new MyMeshVertex(I1.position, I1.normal);
            MyMeshVertex vI2 = new MyMeshVertex(I2.position, I2.normal);
            MyMeshVertex vI2O1 = new MyMeshVertex(pos_I2O1, normal_I2O1);

            //The 3 original half-edges (a vertex points to an edge going from it)
            HalfEdge3 halfEdge_O1I1 = O1.edge;
            HalfEdge3 halfEdge_I1I2 = I1.edge; //This edge will keep it's opposite edge because we don't cut it
            HalfEdge3 halfEdge_I2O1 = I1.edge;

            //New outside triangle
            HalfEdgeFace3 triangleO_1 = newMeshO.AddTriangle(vO1, vO1I1, vI2O1);

            //New inside triangles
            HalfEdgeFace3 triangleI_1 = newMeshI.AddTriangle(vO1I1, vI1, vI2);

            HalfEdgeFace3 triangleI_2 = newMeshI.AddTriangle(vO1I1, vI2, vI2O1);


            //
            // Connect whatever we can connect
            //

            //From the outside triangle, we have to save the cut edge
            //AddNewTriangleToMesh(v1, v2, v3) methods returns a triangle which references and edge point TO v1
            //So the cutEdge should be
            HalfEdge3 outerCutEdge = triangleO_1.edge.nextEdge.nextEdge;

            //Save it
            newEdgesO.Add(outerCutEdge);

            //We also need to connect the new meshes with each other by using the cut edge
            //This will make it simpler to add hole meshes and merge small edges
            HalfEdge3 innerCutEdge = triangleI_2.edge;

            innerCutEdge.oppositeEdge = outerCutEdge;
            outerCutEdge.oppositeEdge = innerCutEdge;

            //We also have an edge that didnt change because it wasn't cut I1-I2
            //So make sure its opposite edges are connected in the correct way
            HalfEdge3 newHalfEdge_I1I2 = triangleI_1.edge.nextEdge.nextEdge;

            //If the original edge going in this direction has an opposite edge
            if (halfEdge_I1I2.oppositeEdge != null)
            {
                newHalfEdge_I1I2.oppositeEdge = halfEdge_I1I2.oppositeEdge;

                halfEdge_I1I2.oppositeEdge.oppositeEdge = newHalfEdge_I1I2;
            }
        }



        //Cut a triangle where two vertices are inside and the other vertex is outside
        //Make sure they are sorted clockwise: O1-O2-I1
        //O means that this vertex is outside the plane
        private static void CutTriangleTwoOutside(HalfEdgeVertex3 O1, HalfEdgeVertex3 O2, HalfEdgeVertex3 I1, HalfEdgeData3 newMeshO, HalfEdgeData3 newMeshI, HashSet<HalfEdge3> newEdgesO, Plane3 cutPlane)
        {
            //
            // Find where we should cut this triangle and the normal at those positions 
            //

            //Cut the triangle by using edge-plane intersection
            //The edges should always go from outside to inside, or we may end up with floating point precision issues
            //Triangles in Unity are ordered clockwise, so form edges that intersects with the plane:
            Edge3 e_O2I1 = new Edge3(O2.position, I1.position);
            //Edge3 e_F1F2 = new Edge3(F1, F2); //Not needed because never intersects with the plane
            Edge3 e_I1O1 = new Edge3(O1.position, I1.position);

            //The positions of the intersection vertices
            MyVector3 pos_O2I1 = _Intersections.GetLinePlaneIntersectionPoint(cutPlane, e_O2I1);
            MyVector3 pos_I1O1 = _Intersections.GetLinePlaneIntersectionPoint(cutPlane, e_I1O1);

            //The normals of the intersection vertices
            float percentageBetween_O2I1 = MyVector3.Distance(O2.position, pos_O2I1) / MyVector3.Distance(O2.position, I1.position);
            float percentageBetween_I1O1 = MyVector3.Distance(I1.position, pos_I1O1) / MyVector3.Distance(I1.position, O1.position);

            MyVector3 normal_O2I1 = _Interpolation.Lerp(O2.normal, I1.normal, percentageBetween_O2I1);
            MyVector3 normal_I1O1 = _Interpolation.Lerp(I1.normal, O1.normal, percentageBetween_I1O1);

            //MyVector3 normal_F2B1 = Vector3.Slerp(F2.normal.ToVector3(), B1.normal.ToVector3(), percentageBetween_F2B1).ToMyVector3();
            //MyVector3 normal_B1F1 = Vector3.Slerp(B1.normal.ToVector3(), F1.normal.ToVector3(), percentageBetween_B1F1).ToMyVector3();

            normal_O2I1 = MyVector3.Normalize(normal_O2I1);
            normal_I1O1 = MyVector3.Normalize(normal_I1O1);

            

            //
            // Form 3 new triangles
            //

            //All vertices in the polygon (clockwise order) from which we will build three triangles
            MyMeshVertex vO1 = new MyMeshVertex(O1.position, O1.normal);
            MyMeshVertex vO2 = new MyMeshVertex(O2.position, O2.normal);
            MyMeshVertex vO2I1 = new MyMeshVertex(pos_O2I1, normal_O2I1);
            MyMeshVertex vI1 = new MyMeshVertex(I1.position, I1.normal);
            MyMeshVertex v_I1O1 = new MyMeshVertex(pos_I1O1, normal_I1O1);


            //The 3 original half-edges (a vertex points to an edge going from it)
            HalfEdge3 halfEdge_O1O2 = O1.edge; //This edge will keep it's opposite edge because we don't cut it
            HalfEdge3 halfEdge_O2I1 = O2.edge; 
            HalfEdge3 halfEdge_I1O1 = I1.edge;


            //New outside triangles
            HalfEdgeFace3 triangleO_1 = newMeshO.AddTriangle(vO1, vO2, vO2I1);

            HalfEdgeFace3 triangleO_2 = newMeshO.AddTriangle(vO1, vO2I1, v_I1O1);

            //New inside triangle
            HalfEdgeFace3 triangleI_1 = newMeshI.AddTriangle(vO2I1, vI1, v_I1O1);



            //
            // Connect whatever we can connect
            //

            //From the outside triangle, we have to save the cut edge
            //AddNewTriangleToMesh(v1, v2, v3) methods returns a triangle which references and edge point TO v1
            //So the cutEdge should be
            HalfEdge3 outerCutEdge = triangleO_2.edge.nextEdge.nextEdge;

            //Save it
            newEdgesO.Add(outerCutEdge);

            //We also need to connect the new meshes with each other by using the cut edge
            //This will make it simpler to add hole meshes and merge small edges
            HalfEdge3 innerCutEdge = triangleI_1.edge;

            innerCutEdge.oppositeEdge = outerCutEdge;
            outerCutEdge.oppositeEdge = innerCutEdge;

            //We also have an edge that didnt change because it wasn't cut I1-I2
            //So make sure its opposite edges are connected in the correct way
            HalfEdge3 newHalfEdge_O1O2 = triangleO_1.edge.nextEdge;

            //If the original edge going in this direction has an opposite edge
            if (halfEdge_O1O2.oppositeEdge != null)
            {
                newHalfEdge_O1O2.oppositeEdge = halfEdge_O1O2.oppositeEdge;

                halfEdge_O1O2.oppositeEdge.oppositeEdge = newHalfEdge_O1O2;
            }
        }



        //
        // Speed up calculations by first checking if the mesh's AABB is intersecting with the plane
        //

        private static bool IsMeshAABBIntersectingWithPlane(Transform meshTrans, Plane3 cutPlaneGlobal)
        {
            //To get the AABB in world space we can use the mesh renderer
            MeshRenderer mr = meshTrans.GetComponent<MeshRenderer>();

            if (mr == null)
            {
                Debug.Log("A mesh renderer is not attached so we can speed up the performance :(");

                //So we have to return true because we don't know

                return true;
            }


            AABB3 aabb = new AABB3(mr.bounds);

            //The corners of this box 
            HashSet<MyVector3> corners = aabb.GetCorners();

            if (corners != null && corners.Count > 1)
            {
                //The points are in world space so use the plane in world space
                if (ArePointsOnOneSideOfPlane(new List<MyVector3>(corners), cutPlaneGlobal))
                {
                    Debug.Log("This mesh can't be cut because its AABB doesnt intersect with the plane");

                    return false;
                }
            }

            return true;
        }



        //Is a list of points on one side of a plane?
        public static bool ArePointsOnOneSideOfPlane(List<MyVector3> points, Plane3 plane)
        {        
            //First check the first point
            bool isInFront = _Geometry.IsPointOutsidePlane(points[0], plane);

            for (int i = 1; i < points.Count; i++)
            {
                bool isOtherOutside = _Geometry.IsPointOutsidePlane(points[i], plane);

                //We have found a point which is not at the same side of the plane as the first point
                if (isInFront != isOtherOutside)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
