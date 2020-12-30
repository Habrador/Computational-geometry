using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Cut a meth with a plane
    //TODO:
    //- Remove small edges on the cut edge to get a better triangulation by measuring the length of each edge. This should also fix problem with ugly normals. They are also causing trouble when we identify hole-edges, so sometimes we get small triangles as separate meshes
    //- Normalize the data to 0-1 to avoid floating point precision issues
    //- Submeshes should be avoided anyway because of performance, so ignore those. Use uv to illustrate where the cut is. If you need to illustrate the cut with a different material, you can return two meshes and use the one that was part of the originl mesh to generate the convex hull 
    //- Is failing if the mesh we cut has holes in it at the bottom, and the mesh intersects with one of those holes. But that's not a problem because then we can't fill the hole anyway! 

    //- Time measurements for optimizations (bunny):
    //- AABB-plane test: 0.004 s
    //- Separate meshes into outside/inside plane: 0.023 s of which pos-plane takes 0.002 s
    //- Connect opposite edges: 0.003
    //- Find mesh islands: 0.02
    public static class CutMeshWithPlane 
    {
        //Should return null if the mesh couldn't be cut because it doesn't intersect with the plane
        //Otherwise it should return the new meshes
        //meshTrans is needed so we can transform the cut plane to the mesh's local space 
        //halfEdgeMeshData should thus be in local space
        public static HashSet<HalfEdgeData3> CutMesh(Transform meshTrans, HalfEdgeData3 halfEdgeMeshData, OrientedPlane3 orientedCutPlaneGlobal)
        {
            //Validate the input data
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


            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

            timer.Start();

            //The plane with just a normal
            Plane3 cutPlaneGlobal = orientedCutPlaneGlobal.Plane3;

            //First check if the AABB of the mesh is intersecting with the plane
            //Otherwise we can't cut the mesh, so its a waste of time
            bool isIntersecting = IsMeshAABBIntersectingWithPlane(meshTrans, cutPlaneGlobal);

            if (!isIntersecting)
            {
                return null;
            }

            Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to do the AABB-plane intersection test");

            timer.Restart();



            //The two meshes we might end up with after the cut
            //One is in front of the plane and another is in back of the plane
            HalfEdgeData3 newMeshO = new HalfEdgeData3();
            HalfEdgeData3 newMeshI = new HalfEdgeData3();

            //Save the new edges we add when cutting triangles that intersects with the plane
            //Needs to be edges so we can later connect them with each other to fill the hole
            //And to remove small triangles
            //We only need to save the outside edges, because we can identify the inside edges because each edge has an opposite edge
            HashSet<HalfEdge3> newEdgesO = new HashSet<HalfEdge3>();

            //Transform the plane from global space to local space of the mesh
            MyVector3 planePosLocal = meshTrans.InverseTransformPoint(cutPlaneGlobal.pos.ToVector3()).ToMyVector3();
            MyVector3 planeNormalLocal = meshTrans.InverseTransformDirection(cutPlaneGlobal.normal.ToVector3()).ToMyVector3();

            Plane3 cutPlaneLocal = new Plane3(planePosLocal, planeNormalLocal);


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
                    //We get 6 cases where each vertex is on its own in front or in the back of the plane


                    //p1 is outside
                    if (is_p1_outside && !is_p2_outside && !is_p3_outside)
                    {
                        CutTriangleOneOutside(v1, v2, v3, newMeshO, newMeshI, newEdgesO, cutPlaneLocal);
                    }
                    //p1 is inside
                    else if (!is_p1_outside && is_p2_outside && is_p3_outside)
                    {
                        CutTriangleTwoOutside(v2, v3, v1, newMeshO, newMeshI, newEdgesO, cutPlaneLocal);
                    }

                    //p2 is outside
                    else if (!is_p1_outside && is_p2_outside && !is_p3_outside)
                    {
                        CutTriangleOneOutside(v2, v3, v1, newMeshO, newMeshI, newEdgesO, cutPlaneLocal);
                    }
                    //p2 is inside
                    else if (is_p1_outside && !is_p2_outside && is_p3_outside)
                    {
                        CutTriangleTwoOutside(v3, v1, v2, newMeshO, newMeshI, newEdgesO, cutPlaneLocal);
                    }

                    //p3 is outside
                    else if (!is_p1_outside && !is_p2_outside && is_p3_outside)
                    {
                        CutTriangleOneOutside(v3, v1, v2, newMeshO, newMeshI, newEdgesO, cutPlaneLocal);
                    }
                    //p3 is inside
                    else if (is_p1_outside && is_p2_outside && !is_p3_outside)
                    {
                        CutTriangleTwoOutside(v1, v2, v3, newMeshO, newMeshI, newEdgesO, cutPlaneLocal);
                    }

                    //Something is strange if we end up here...
                    else
                    {
                        Debug.Log("No case was gound where we split triangle into 3 new triangles");
                    }
                }
            }


            //Generate the new meshes only needed the old mesh intersected with the plane
            if (newMeshO.verts.Count == 0 || newMeshI.verts.Count == 0)
            {
                return null;
            }


            Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to separate the meshes");

            timer.Restart();

            //Find opposite edges to each edge
            //Most edges should already have an opposite edge, but we need to connected some of the new edges with each other
            newMeshO.ConnectAllEdgesFast();
            newMeshI.ConnectAllEdgesFast();

            Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to connect the opposite edges");


            //Display all edges which have no opposite for debugging
            //Remember that this will NOT display the holes because the hole-edges are connected across the border
            //DebugHalfEdge.DisplayEdgesWithNoOpposite(newMeshO.edges, meshTrans, Color.white);
            //DebugHalfEdge.DisplayEdgesWithNoOpposite(newMeshI.edges, meshTrans, Color.white);

            //This will display the hole
            DebugHalfEdge.DisplayEdges(newEdgesO, meshTrans, Color.white);


            //Remove small triangles at the seam where we did the cut because they will cause shading issues if the surface is smooth
            //RemoveSmallTriangles(F_Mesh, newEdges);


            //Fill the holes in the mesh
            //HashSet<Hole> allHoles = FillHoles(newEdgesI, newEdgesO, orientedCutPlaneGlobal, meshTrans, planeNormalLocal);


            //Separate the meshes (they are still connected at their cut edge)
            foreach (HalfEdge3 e in newEdgesO)
            {
                if (e.oppositeEdge != null)
                {
                    HalfEdge3 eOpposite = e.oppositeEdge;

                    //Remove the connection
                    e.oppositeEdge = null;
                    eOpposite = null;
                }
            }


            timer.Restart();

            //Split each mesh into separate meshes if the original mesh is not connected, meaning it has islands
            HashSet<HalfEdgeData3> newMeshesO = SeparateMeshIslands(newMeshO);
            HashSet<HalfEdgeData3> newMeshesI = SeparateMeshIslands(newMeshI);

            Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to find mesh islands");


            //Connect the holes with respective mesh
            //AddHolesToMeshes(newMeshesO, newMeshesI, allHoles);


            timer.Restart();
            
            //Combine before return
            HashSet<HalfEdgeData3> allNewMeshes = newMeshesO;

            allNewMeshes.UnionWith(newMeshesI);

            Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to combined the final meshes");

            return allNewMeshes;
        }



        private static void AddHolesToMeshes(HashSet<HalfEdgeData3> newMeshesO, HashSet<HalfEdgeData3> newMeshesI, HashSet<Hole> allHoles)
        {
            //This may happen if the original mesh has holes in it and the plane intersects one of those holes. 
            //Then we can't identify the hole as closed and we can't repair the hole anyway 
            if (allHoles == null)
            {
                return;
            }
        
            foreach (Hole hole in allHoles)
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



        //Separate a mesh by its islands (if it has islands)
        private static HashSet<HalfEdgeData3> SeparateMeshIslands(HalfEdgeData3 meshData)
        {
            HashSet<HalfEdgeData3> meshIslands = new HashSet<HalfEdgeData3>();

            HashSet<HalfEdgeFace3> allFaces = meshData.faces;


            //Separate by flood-filling

            //Faces belonging to a separate island
            HashSet<HalfEdgeFace3> facesOnThisIsland = new HashSet<HalfEdgeFace3>();

            //Faces we havent flodded from yet
            Queue<HalfEdgeFace3> facesToFloodFrom = new Queue<HalfEdgeFace3>();

            //Add a first face to the queue
            HalfEdgeFace3 firstFace = allFaces.FakePop();

            facesToFloodFrom.Enqueue(firstFace);

            int numberOfIslands = 0;

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

                    //We still have faces to visit, so they must be on a new island
                    if (allFaces.Count > 0)
                    {
                        facesOnThisIsland = new HashSet<HalfEdgeFace3>();

                        //Add a first face to the queue
                        firstFace = allFaces.FakePop();

                        facesToFloodFrom.Enqueue(firstFace);
                    }
                    else
                    {
                        Debug.Log($"This mesh has {numberOfIslands} islands");
                    
                        break;
                    }
                }
            
                HalfEdgeFace3 f = facesToFloodFrom.Dequeue();

                facesOnThisIsland.Add(f);

                //Remove from the original mesh so we can identify if we need to start at a new island
                allFaces.Remove(f);

                //Find neighboring faces 
                edges.Clear();

                edges.Add(f.edge);
                edges.Add(f.edge.nextEdge);
                edges.Add(f.edge.nextEdge.nextEdge);

                foreach (HalfEdge3 e in edges)
                {
                    if (e.oppositeEdge != null)
                    {
                        HalfEdgeFace3 fNeighbor = e.oppositeEdge.face;

                        //If we haven't seen this face before
                        if (!facesOnThisIsland.Contains(fNeighbor) && !facesToFloodFrom.Contains(fNeighbor))
                        {
                            facesToFloodFrom.Enqueue(fNeighbor);
                        }
                    }

                    //Here we could mabe save all edges with no opposite, meaning its an edge at the hole 
                }
                

                safety += 1;

                if (safety > 50000)
                {
                    Debug.Log("Stuck in infinite loop when generating mesh islands");

                    break;
                }
            }

            return meshIslands;
        }



        //Fill the hole (or holes) in the mesh
        private static HashSet<Hole> FillHoles(HashSet<HalfEdge3> holeEdgesI, HashSet<HalfEdge3> holeEdgesO, OrientedPlane3 orientedCutPlane, Transform meshTrans, MyVector3 planeNormal)
        {
            if (holeEdgesI == null || holeEdgesI.Count == 0)
            {
                Debug.Log("This mesh has no hole");

                return null;
            }


            //Find all separate holes
            HashSet<List<HalfEdge3>> allHoles = IdentifySeparateHoles(holeEdgesI);

            if (allHoles.Count == 0)
            {
                Debug.LogWarning("Couldn't identify any holes even though we have hole edges");

                return null;
            }

            //Debug
            //foreach (List<HalfEdge3> hole in allHoles)
            //{
            //    foreach (HalfEdge3 e in hole)
            //    {
            //        Debug.DrawLine(meshTrans.TransformPoint(e.v.position.ToVector3()), Vector3.zero, Color.white, 5f);
            //    }
            //}


            //Fill the hole with a mesh
            HashSet<Hole> holeMeshes = new HashSet<Hole>();

            foreach (List<HalfEdge3> hole in allHoles)
            {
                HalfEdgeData3 holeMeshI = new HalfEdgeData3();
                HalfEdgeData3 holeMeshO = new HalfEdgeData3();

                //Transform vertices to local position of the cut plane to make it easier to triangulate with Ear Clipping
                //Ear CLipping wants vertices in 2d
                List<MyVector2> sortedEdges_2D = new List<MyVector2>();

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

                    sortedEdges_2D.Add(p2D);
                }


                //Triangulate with Ear Clipping
                HashSet<Triangle2> triangles = _EarClipping.Triangulate(sortedEdges_2D, null, optimizeTriangles: false);

                //Debug.Log($"Number of triangles from Ear Clipping: {triangles.Count}");

                //Transform vertices to mesh space and half-edge data structure
                foreach (Triangle2 t in triangles)
                {
                    //3d space
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
                    MyMeshVertex v1_I = new MyMeshVertex(p1Mesh.ToMyVector3(), planeNormal);
                    MyMeshVertex v2_I = new MyMeshVertex(p2Mesh.ToMyVector3(), planeNormal);
                    MyMeshVertex v3_I = new MyMeshVertex(p3Mesh.ToMyVector3(), planeNormal);

                    //For inside mesh
                    MyMeshVertex v1_O = new MyMeshVertex(p1Mesh.ToMyVector3(), -planeNormal);
                    MyMeshVertex v2_O = new MyMeshVertex(p2Mesh.ToMyVector3(), -planeNormal);
                    MyMeshVertex v3_O = new MyMeshVertex(p3Mesh.ToMyVector3(), -planeNormal);

                    //Now we can finally add this triangle to the half-edge data structure
                    //AddTriangleToMesh(v1_I, v2_I, v3_I, holeMeshI, null);
                    //AddTriangleToMesh(v1_O, v3_O, v2_O, holeMeshO, null);
                }

                //We also need an edge belonging to the mesh (not hole mesh) to easier merge mesh with hole
                //The hole edges were generated for the Inside mesh
                HalfEdge3 holeEdgeI = hole[0];

                //But we also need an edge for the Outside mesh
                bool foundCorrespondingEdge = false;

                MyVector3 eGoingTo = holeEdgeI.v.position;
                MyVector3 eGoingFrom = holeEdgeI.prevEdge.v.position;

                foreach (HalfEdge3 holeEdgeO in holeEdgesO)
                {
                    MyVector3 eOppsiteGoingTo = holeEdgeO.v.position;
                    MyVector3 eOppsiteGoingFrom = holeEdgeO.prevEdge.v.position;

                    if (eOppsiteGoingTo.Equals(eGoingFrom) && eOppsiteGoingFrom.Equals(eGoingTo))
                    {
                        Hole newHoleMesh = new Hole(holeMeshI, holeMeshO, holeEdgeI, holeEdgeO);

                        holeMeshes.Add(newHoleMesh);

                        foundCorrespondingEdge = true;

                        break;
                    }
                }

                if (!foundCorrespondingEdge)
                {
                    Debug.Log("Couldnt find opposite edge in hole, so no hole was added");
                }
            }

            return holeMeshes;
        }



        //We might end up with multiple holes, so we need to identify all of them
        //Input is just a list of all edges that form the hole(s)
        //The output list is sorted so we can walk around the hole
        private static HashSet<List<HalfEdge3>> IdentifySeparateHoles(HashSet<HalfEdge3> cutEdges)
        {
            HashSet<List<HalfEdge3>> allHoles = new HashSet<List<HalfEdge3>>();

            //Alternative create a linked list, which should identify all holes automatically?

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
