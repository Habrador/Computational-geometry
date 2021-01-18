using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Habrador_Computational_Geometry
{
    //Cut a meth with a plane
    //TODO:
    //- Can we use DOTS/GPU/threads to improve performance? Several sub-algorithms can be done in parallell
    //- Figure out why we need to unnormalize when converting from mesh space to plane space. Maybe easier to just use the plane in local pos when converting...

    //Time measurements for optimizations (bunny) total time: 0.08
    //- AABB-plane test: 0.003
    //- Separate meshes into outside/inside plane: 0.015 
    //- Connect opposite edges: 0.004
    //- Remove small edges: 0.015
    //- Identify and fill holes: 0.025
    //- Find mesh islands: 0.015
    //- Connect hole with mesh: 0.004
    public static class CutMeshWithPlane 
    {
        /// <summary>
        /// Cuts a mesh with a plane
        /// Returns null if the mesh couldn't be cut because it doesn't intersect with the plane
        /// Cant handle sub-meshes, but they should be avoided anyway because of performance reasons!
        /// Is generating an odd result if the mesh we cut has holes in it, and the plane intersects with one of those holes
        /// </summary>
        /// <param name="meshTrans">The transform the mesh is attached to, so we can transform the cut plane to local space</param>
        /// <param name="halfEdgeMeshData">The mesh on the half-edge form in local space</param>
        /// <param name="orientedCutPlaneGlobal">The plane we use to cut the mesh</param>
        /// <param name="fillHoles">If we should fill the holes in the mesh, which is obviously not possible if we cut a plane</param>
        /// <returns></returns>
        public static List<HalfEdgeData3> CutMesh(Transform meshTrans, HalfEdgeData3 halfEdgeMeshData, OrientedPlane3 orientedCutPlaneGlobal, bool fillHoles)
        {
            bool measureTime = true;
        

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
            if (measureTime) timer.Start();

            //The plane with just a normal
            Plane3 cutPlaneGlobal = orientedCutPlaneGlobal.Plane3;

            bool isIntersecting = IsMeshAABBIntersectingWithPlane(meshTrans, cutPlaneGlobal);

            if (!isIntersecting)
            {
                Debug.Log("This mesh's AABB didn't intersect with the plane, so we couldn't cut it.");
            
                return null;
            }

            if (measureTime) Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to do the AABB-plane intersection test");



            //
            // Transform the plane from global to local space of the mesh, which is faster than transforming the mesh from local to global space 
            //

            MyVector3 planePosLocal = meshTrans.InverseTransformPoint(cutPlaneGlobal.pos.ToVector3()).ToMyVector3();
            MyVector3 planeNormalLocal = meshTrans.InverseTransformDirection(cutPlaneGlobal.normal.ToVector3()).ToMyVector3();

            Plane3 cutPlaneLocal = new Plane3(planePosLocal, planeNormalLocal);



            //
            // Normalize to 0-1
            //
            
            //Will make it easier to remove small edges if all meshes have the same range, because easier to define "small" edge
            List<MyVector3> allPoints = new List<MyVector3>();

            HashSet<HalfEdgeVertex3> verts = halfEdgeMeshData.verts;

            foreach (HalfEdgeVertex3 v in verts)
            {
                allPoints.Add(v.position);
            }

            //Also add the plane
            allPoints.Add(cutPlaneLocal.pos);

            Normalizer3 normalizer = new Normalizer3(allPoints);

            //Normalize everything
            cutPlaneLocal.pos = normalizer.Normalize(cutPlaneLocal.pos);

            foreach (HalfEdgeVertex3 v in verts)
            {
                v.position = normalizer.Normalize(v.position);
            }

            //meshTrans.position = normalizer.Normalize(meshTrans.position.ToMyVector3()).ToVector3();

            //orientedCutPlaneGlobal.planeTrans.position = normalizer.Normalize(orientedCutPlaneGlobal.planeTrans.position.ToMyVector3()).ToVector3();
            //Debug.Log(meshTrans.localScale);


            //
            // Separate the old mesh into two new meshes (or just one if the mesh is not intersecting with the plane)
            //
            if (measureTime) timer.Restart();

            //The two meshes we might end up with after the cut
            //One is "outside" of the plane and another is "inside" the plane
            HalfEdgeData3 newMeshO = new HalfEdgeData3();
            HalfEdgeData3 newMeshI = new HalfEdgeData3();

            //Save the new edges we add when cutting triangles that intersects with the plane
            //Needs to be edges so we can later connect them with each other to fill the hole
            //And to remove small triangles
            //We only need to save the outside edges, because we can identify the inside edges because each edge has an opposite edge
            HashSet<HalfEdge3> cutEdgesO = new HashSet<HalfEdge3>();
            
            //This new meshes might have islands (and thus be not connected) but we check for that later
            SeparateMeshWithPlane(halfEdgeMeshData, newMeshO, newMeshI, cutPlaneLocal, cutEdgesO);

            if (measureTime) Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to separate the meshes");

            //Generate new meshes only if the old mesh intersected with the plane
            if (newMeshO.faces.Count == 0 || newMeshI.faces.Count == 0)
            {
                Debug.Log("This mesh didn't intersect with the plane, so we couldn't cut it.");
            
                return null;
            }



            //
            // Find opposite edge to each edge
            //
            if (measureTime) timer.Restart();

            //Most edges should already have an opposite edge, but we need to connected some of the new triangles edges with each other
            //This can maybe be improved because we know which edges have no connection and we could just search through those
            newMeshO.ConnectAllEdgesFast();
            newMeshI.ConnectAllEdgesFast();

            if (measureTime) Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to connect the opposite edges");


            //Display all edges which have no opposite for debugging
            //Remember that this will NOT display the holes because the hole-edges are connected across the border
            //DebugHalfEdge.DisplayEdgesWithNoOpposite(newMeshO.edges, meshTrans, Color.white);
            //DebugHalfEdge.DisplayEdgesWithNoOpposite(newMeshI.edges, meshTrans, Color.white);

            //This will display the hole(s)
            //DebugHalfEdge.DisplayEdges(cutEdgesO, meshTrans, Color.white, normalizer);



            //
            // Remove small triangles at the seam where we did the cut
            //
            if (measureTime) timer.Restart();

            //The small edges may cause shading issues and the fewer edges we have the faster it will take to fill the holes
            RemoveSmallTriangles(cutEdgesO, newMeshO, newMeshI);

            if (measureTime) Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to remove small edges");



            //
            // Identify all holes and fill the holes
            //
            HashSet<CutMeshHole> allHoles = null;

            if (fillHoles)
            {
                if (measureTime) timer.Restart();

                allHoles = FillHoles(cutEdgesO, orientedCutPlaneGlobal, meshTrans, planeNormalLocal, normalizer);

                if (measureTime) Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to identify and fill holes");
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
            if (measureTime) timer.Restart();

            HashSet<HalfEdgeData3> newMeshesO = SeparateMeshIslands(newMeshO);
            HashSet<HalfEdgeData3> newMeshesI = SeparateMeshIslands(newMeshI);

            if (measureTime) Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to find mesh islands");



            //
            // Connect each hole mesh with respective mesh
            //
            if (fillHoles)
            {
                if (measureTime) timer.Restart();

                //It should be faster to do this after identifying each mesh island 
                //because that process requires flood-filling which is slower the more triangles each mesh has
                AddHolesToMeshes(newMeshesO, newMeshesI, allHoles);

                if (measureTime) Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to match hole with mesh");
            }

            //Debug
            //foreach (HalfEdgeData3 data in newMeshesO)
            //{
            //    DebugHalfEdge.DisplayEdgesWithNoOpposite(data.edges, meshTrans, Color.white, normalizer);
            //}



            //
            // Combine the inside and outside meshes
            //

            List<HalfEdgeData3> allNewMeshes = new List<HalfEdgeData3>();

            allNewMeshes.AddRange(newMeshesO);
            allNewMeshes.AddRange(newMeshesI);



            //
            // UnNormalize
            //

            foreach (HalfEdgeData3 data in allNewMeshes)
            {
                HashSet<HalfEdgeVertex3> vertsNormalized = data.verts;

                foreach (HalfEdgeVertex3 v in vertsNormalized)
                {
                    v.position = normalizer.UnNormalize(v.position);
                }
            }

            //meshTrans.position = normalizer.UnNormalize(meshTrans.position.ToMyVector3()).ToVector3();

            //orientedCutPlaneGlobal.planeTrans.position = normalizer.UnNormalize(orientedCutPlaneGlobal.planeTrans.position.ToMyVector3()).ToVector3();

            return allNewMeshes;
        }



        //
        // Remove small triangles
        //

        //We could maybe use the ideas from mesh simplification to identify edges to remove?
        private static void RemoveSmallTriangles(HashSet<HalfEdge3> cutEdgesO, HalfEdgeData3 newMeshO, HalfEdgeData3 newMeshI)
        {
            //Remove all edges shorter than this length
            //Remember we have normalized all values to be 0-1
            float maxLength = 0.001f;

            float maxLengthSqr = maxLength * maxLength;

            int numberOfEdgesRemoved = 0;

            HashSet<HalfEdge3> removedEdges = new HashSet<HalfEdge3>();

            int safety = 0;

            while (true)
            {
                foreach (HalfEdge3 e in cutEdgesO)
                {
                    float distanceSqr = e.LengthSqr();

                    //Is this edge small?
                    if (distanceSqr < maxLengthSqr)
                    {
                        MyVector3 mergePosition = (e.v.position + e.prevEdge.v.position) * 0.5f;

                        //Ignore that the normal might change when we move the vertices
                        //Because we move only a small distance, the normal should be the same
                        //This will also make it easier to handle hard edges

                        //Disconnect the edge from its opposite
                        HalfEdge3 eOpposite = e.oppositeEdge;

                        e.oppositeEdge = null;

                        //And disconnect the opposite from this edge if it had an opposite edge
                        if (eOpposite != null)
                        {
                            eOpposite.oppositeEdge = null;

                            newMeshI.ContractTriangleHalfEdge(eOpposite, mergePosition);
                        }

                        newMeshO.ContractTriangleHalfEdge(e, mergePosition);

                        numberOfEdgesRemoved += 1;

                        removedEdges.Add(e);
                    }
                }


                //If we found no small edges, we don't have to search anymore
                if (removedEdges.Count == 0)
                {
                    break;
                }


                //Remove the edges we merged from the list of all edges
                foreach (HalfEdge3 e in removedEdges)
                {
                    cutEdgesO.Remove(e);
                }

                removedEdges.Clear();


                safety += 1;

                if (safety > 50000)
                {
                    Debug.Log("Stuck in infinite loop when removing small edges");
                
                    break;
                }
            }


            //Debug.Log($"Removed {numberOfEdgesRemoved} small edges");
        }



        //
        // Separates a mesh by a plane
        //

        //Performance can maybe be improved by the fact that some triangles are sharing vertices, so first check all vertices if they are outside/inside the plane and then for each triangle, find the vertices in some data structure...
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

                //Do we need to take into account if a vertex is on the plane???


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

            //But now we also need to connect the opposite edges of the hole edges with the opposite edges of the mesh edges
            //This assumes we "reset" the vertices in the hole mesh after generating the hole, or we have floating point
            //precision issues from the normalizations
            foreach (HalfEdgeData3 mesh in newMeshesO)
            {
                mesh.ConnectAllEdgesFast();
            }

            foreach (HalfEdgeData3 mesh in newMeshesI)
            {
                mesh.ConnectAllEdgesFast();
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
                        //Debug.Log($"This mesh has {numberOfIslands} islands");
                    
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
        private static HashSet<CutMeshHole> FillHoles(HashSet<HalfEdge3> holeEdgesO, OrientedPlane3 orientedCutPlane, Transform meshTrans, MyVector3 planeNormalLocal, Normalizer3 normalizer)
        {
            //Time measurements for optimizations (bunny):
            //- Separate holes: 0.003
            //- Hole edges to 2d space: 0
            //- Ear clipping algorithm: 0.006
            //- Triangles from 2d space to half-edge: 0.003

            //System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        
            if (holeEdgesO == null)
            {
                Debug.Log("This mesh has no hole");

                return null;
            }


            //
            // Find all separate holes
            //
            //timer.Start();
            
            HashSet<List<HalfEdge3>> allHoles = IdentifySeparateHoles(holeEdgesO);

            //timer.Stop();

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
                //Ear Clipping wants vertices in 2d
                //timer.Start();

                List<MyVector2> sortedVertices_2D = new List<MyVector2>();

                Transform planeTrans = orientedCutPlane.planeTrans;

                foreach (HalfEdge3 e in hole)
                {
                    MyVector3 pMeshSpace = e.v.position;

                    //Now we need to unnormalize because there's something odd going on when converting between spaces
                    pMeshSpace = normalizer.UnNormalize(pMeshSpace);

                    //Mesh space to Global space
                    Vector3 pGlobalSpace = meshTrans.TransformPoint(pMeshSpace.ToVector3());

                    //Global space to Plane space
                    Vector3 pPlaneSpace = planeTrans.InverseTransformPoint(pGlobalSpace);

                    //y is normal direction so should be 0
                    MyVector2 p2D = new MyVector2(pPlaneSpace.x, pPlaneSpace.z);

                    sortedVertices_2D.Add(p2D);
                }

                //timer.Stop();


                //Triangulate with Ear Clipping

                //Need to reverse to standardize for the Ear Elipping algorithm
                sortedVertices_2D.Reverse();

                //We also need to normalize the points for the Ear Clipping
                Normalizer2 normalizer2D = new Normalizer2(sortedVertices_2D);

                List<MyVector2> sortedVertices_2D_normalized = normalizer2D.Normalize(sortedVertices_2D);

                //timer.Start();

                HashSet<Triangle2> triangles_normalized = _EarClipping.Triangulate(sortedVertices_2D_normalized, allHoleVertices: null, optimizeTriangles: false);

                //timer.Stop();

                //Unnormalize
                HashSet<Triangle2> triangles = normalizer2D.UnNormalize(triangles_normalized);

                //Debug.Log($"Number of triangles from Ear Clipping: {triangles.Count}");


                //Transform triangles to mesh space and half-edge data structure
                //timer.Start();

                foreach (Triangle2 t in triangles)
                {
                    //TODO: We dont need to translate it back if we create a lookup table with vertex in 2d space and vertex in 3d space'
                    //We didn't add any new vertices... and many of these are doubles as well
                    //Another problem is that when we normalized for Ear Clipping and the un-normalized, 
                    //the vertex is no longer exactly the same as it was before (there's a very small difference)
                    //This will later cause trouble when we go from half-edge to unity mesh in an optimized way

                    //2d to 3d space
                    Vector3 p1 = new Vector3(t.p1.x, 0f, t.p1.y);
                    Vector3 p2 = new Vector3(t.p2.x, 0f, t.p2.y);
                    Vector3 p3 = new Vector3(t.p3.x, 0f, t.p3.y);

                    //Plane space to Global space
                    p1 = planeTrans.TransformPoint(p1);
                    p2 = planeTrans.TransformPoint(p2);
                    p3 = planeTrans.TransformPoint(p3);

                    //Global space to Mesh space
                    p1 = meshTrans.InverseTransformPoint(p1);
                    p2 = meshTrans.InverseTransformPoint(p2);
                    p3 = meshTrans.InverseTransformPoint(p3);

                    //Normalize and to MyVector3
                    MyVector3 p1MyVec3 = normalizer.Normalize(p1.ToMyVector3());
                    MyVector3 p2MyVec3 = normalizer.Normalize(p2.ToMyVector3());
                    MyVector3 p3MyVec3 = normalizer.Normalize(p3.ToMyVector3());

                    //When we normalized for Ear Clipping and the un-normalized, 
                    //the vertex is no longer exactly the same as it was before (there's a very small difference)
                    //This will later cause trouble when we go from half-edge to unity mesh in an optimized way
                    //This is a fast operation, so is fine for now
                    foreach (HalfEdge3 e in hole)
                    {
                        if (e.v.position.Equals(p1MyVec3))
                        {
                            p1MyVec3 = e.v.position;

                            break;
                        }
                    }
                    foreach (HalfEdge3 e in hole)
                    {
                        if (e.v.position.Equals(p2MyVec3))
                        {
                            p2MyVec3 = e.v.position;

                            break;
                        }
                    }
                    foreach (HalfEdge3 e in hole)
                    {
                        if (e.v.position.Equals(p3MyVec3))
                        {
                            p3MyVec3 = e.v.position;

                            break;
                        }
                    }

                    //For inside mesh
                    MyMeshVertex v1_I = new MyMeshVertex(p1MyVec3, planeNormalLocal);
                    MyMeshVertex v2_I = new MyMeshVertex(p2MyVec3, planeNormalLocal);
                    MyMeshVertex v3_I = new MyMeshVertex(p3MyVec3, planeNormalLocal);

                    //For outside mesh
                    MyMeshVertex v1_O = new MyMeshVertex(p1MyVec3, -planeNormalLocal);
                    MyMeshVertex v2_O = new MyMeshVertex(p2MyVec3, -planeNormalLocal);
                    MyMeshVertex v3_O = new MyMeshVertex(p3MyVec3, -planeNormalLocal);

                    //Now we can finally add this triangle to the half-edge data structure
                    holeMeshI.AddTriangle(v1_I, v2_I, v3_I);
                    holeMeshO.AddTriangle(v1_O, v3_O, v2_O);
                }

                //Connect the opposite edges
                holeMeshI.ConnectAllEdgesFast();
                holeMeshO.ConnectAllEdgesFast();

                //timer.Stop();


                //We also need to save an edge belonging to the mesh to easier merge mesh with hole
                //The hole edges were generated by using edges in the outside mesh
                HalfEdge3 holeEdgeO = hole[0];
                HalfEdge3 holeEdgeI = holeEdgeO.oppositeEdge;

                CutMeshHole newHole = new CutMeshHole(holeMeshI, holeMeshO, holeEdgeI, holeEdgeO);

                allHoleMeshes.Add(newHole);
            }

            //Debug.Log($"Whatever we timed in fill holes took {timer.ElapsedMilliseconds / 1000f} seconds");

            return allHoleMeshes;
        }



        //We might end up with multiple holes, so we need to identify all of them
        //Input is just a list of all edges that form the hole(s)
        //The output list is sorted so we can walk around the hole (if there was no empty hole in the mesh we cut)
        //Should return a list of half-edges because makes it faster to identify hole-mesh
        //If there were holes in the original mesh, then we may end up with strange meshes when we try to fill the holes we made
        private static HashSet<List<HalfEdge3>> IdentifySeparateHoles(HashSet<HalfEdge3> holeEdgesOriginal)
        {
            //System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();


            HashSet<List<HalfEdge3>> allHoles = new HashSet<List<HalfEdge3>>();

            //Clone the list with cut edges because we need it to be intact
            HashSet<HalfEdge3> holeEdges = new HashSet<HalfEdge3>();


            //We can borrow the half-edge data structure to make a linked list
            //Set the opposite edge to the actual hole edge in the mesh so we can identify where the edge starts and which original edge it corresponds to
            foreach (HalfEdge3 edge in holeEdgesOriginal)
            {
                HalfEdge3 newEdge = new HalfEdge3(edge.v);

                newEdge.oppositeEdge = edge;

                holeEdges.Add(newEdge);
            }

            //timer.Start();

            //Loop through all edges and find which edge comes next and which edge comes before
            //and when the loop is done all edges should be connected (if there are no holes)
            //We could maybe use a lookup table isntead of searching through all edges???
            foreach (HalfEdge3 edge in holeEdges)
            {
                bool hasFoundPreviousEdge = false;
                bool hasFoundNextEdge = false;

                //This edge might have already been connected to other edges earlier in the loop
                if (edge.nextEdge != null && edge.prevEdge != null)
                {
                    continue;
                }
            
                foreach (HalfEdge3 edgeOther in holeEdges)
                {
                    //Dont compare with itself
                    if (edge == edgeOther)
                    {
                        continue;
                    }


                    //Try find next edge
                    if (edgeOther.prevEdge == null && !hasFoundNextEdge)
                    {
                        //If the edge ends where edgeOther starts
                        if (edge.v.position.Equals(edgeOther.oppositeEdge.prevEdge.v.position))
                        {
                            edge.nextEdge = edgeOther;
                            edgeOther.prevEdge = edge;

                            hasFoundNextEdge = true;
                        }
                    }


                    //Try find previous edge
                    if (edgeOther.nextEdge == null && !hasFoundPreviousEdge)
                    {
                        //If the edge starts where edgeOther ends
                        if (edge.oppositeEdge.prevEdge.v.position.Equals(edgeOther.v.position))
                        {
                            edge.prevEdge = edgeOther;
                            edgeOther.nextEdge = edge;

                            hasFoundPreviousEdge = true;
                        }
                    }

                    //We have found both edges so we don't need to search anymore
                    if (hasFoundNextEdge && hasFoundPreviousEdge)
                    {
                        break;
                    }
                }
            }

            //timer.Stop();

            //We need to find all edges that starts at a hole
            HashSet<HalfEdge3> edgesThatStartsAtHole = new HashSet<HalfEdge3>();

            foreach (HalfEdge3 edge in holeEdges)
            {
                if (edge.prevEdge == null)
                {
                    edgesThatStartsAtHole.Add(edge);
                }
            }

            //Debug.Log($"Edges that starts at a hole: {edgesThatStartsAtHole.Count}");


            //Now we need to separate the linked lists to lists where one edge comes after the other
            //Also remember to put in the actual hole edge and not the fake-edge we used to generate linked lists!!!

            //First add the hole edges that start at a hole in the mesh which is not the hole we created
            foreach (HalfEdge3 edge in edgesThatStartsAtHole)
            {
                List<HalfEdge3> thisHole = new List<HalfEdge3>();

                HalfEdge3 currentEdge = edge;

                int safety = 0;

                do
                {
                    thisHole.Add(currentEdge.oppositeEdge);

                    holeEdges.Remove(currentEdge);

                    currentEdge = currentEdge.nextEdge;

                    safety += 1;

                    if (safety > 50000)
                    {
                        Debug.Log("Stuck in infinite loop when generate holes that start at a hole");

                        break;
                    }
                }
                while (currentEdge != null);

                allHoles.Add(thisHole);
            }

            //Debug.Log(holeEdges.Count);
           
            //Then find the holes that loops all the way around
            int safety2 = 0;

            while (true)
            {
                List<HalfEdge3> thisHole = new List<HalfEdge3>();

                //Pick a start edge
                HalfEdge3 currentEdge = holeEdges.FakePop();

                //Save so we can stop the algorithm
                HalfEdge3 startEdge = currentEdge;

                int safety3 = 0;

                do
                {
                    thisHole.Add(currentEdge.oppositeEdge);
                    
                    holeEdges.Remove(currentEdge);
                   
                    currentEdge = currentEdge.nextEdge;

                    safety3 += 1;

                    if (safety3 > 50000)
                    {
                        Debug.Log("Stuck in infinite loop when generate holes that start at a hole");

                        break;
                    }
                }
                while (currentEdge != startEdge);

                allHoles.Add(thisHole);

                //Debug.Log($"Found hole with {thisHole.Count} edges");


                if (holeEdges.Count == 0)
                {
                    //Debug.Log($"The mesh has {allHoles.Count} holes");

                    break;
                }


                safety2 += 1;
                
                if (safety2 > 50000)
                {
                    Debug.Log("Stuck in infinite loop when generate hole edges");

                    break;
                }
            }


            //Debug.Log($"Whatever we timed took {timer.ElapsedMilliseconds / 1000f} seconds");


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
            //System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
    

            //To get the AABB in world space we can use the mesh renderer
            MeshRenderer mr = meshTrans.GetComponent<MeshRenderer>();

            
            if (mr == null)
            {
                Debug.Log("A mesh renderer is not attached so we can't speed up the performance :(");

                //So we have to return true because we don't know

                return true;
            }

            
            AABB3 aabb = new AABB3(mr.bounds);
            
            //The corners of this box 
            List<MyVector3> corners = aabb.GetCorners();
            
            if (corners != null && corners.Count > 1)
            {
                //The points are in world space so use the plane in world space
                if (ArePointsOnOneSideOfPlane(corners, cutPlaneGlobal))
                {
                    Debug.Log("This mesh can't be cut because its AABB doesnt intersect with the plane");

                    return false;
                }
            }
            

            //Debug.Log($"Whatever we timed in AABB-plane took {timer.ElapsedMilliseconds / 1000f} seconds");

            return true;
        }



        //Is a list of points on one side of a plane?
        public static bool ArePointsOnOneSideOfPlane(List<MyVector3> points, Plane3 plane)
        {        
            //First check the first point
            bool isInFront = _Geometry.IsPointOutsidePlane(points[0], plane);

            //Then check the rest of the points
            for (int i = 1; i < points.Count; i++)
            {
                bool isOtherOutside = _Geometry.IsPointOutsidePlane(points[i], plane);

                //We have found a point which is not at the same side of the plane as the first point
                //So the AABB is intersecting with the plane
                if (isInFront != isOtherOutside)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
