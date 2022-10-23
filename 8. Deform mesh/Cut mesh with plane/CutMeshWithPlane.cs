using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Cut a meth with a plane
    //TODO:
    //- Remove small edges on the cut edge to get a better triangulation by measuring the length of each edge. This should also fix problem with ugly normals. They are also causing trouble when we identify hole-edges, so sometimes we get small triangles as separate meshes
    //- Normalize the data to 0-1 to avoid floating point precision issues
    //- Submeshes should be avoided because of performance, so ignore those. Use uv to illustrate where the cut is. If you need to illustrate the cut with a different material, you can return two meshes and use the one that was part of the originl mesh to generate the convex hull 
    //- Is failing if the mesh we cut has holes in it at the bottom, and the mesh intersects with one of those holes. But that's not a problem because then we can't fill the hole anyway.  
    //- When cutting triangles - always cut edges from outside -> inside. If you cut one from outside and the other from inside, the result is not the same because of floating point issues, which may cause trouble when finding opposite edges
    //- Use the cut-edge to analyze how many holes we have. If we have just one hole, we don't need to flood-fill and thus we don't need to convert the mesh to the half-edge data structure. But it might be problematic to merge small edges if we are not on the half-edge data structure...
    //- Input should be half-edge, not mesh. If we want to cut a mesh multiple times, then we would have to convert from mesh to half-edge multiple times. Converting to half-edge is a bottleneck. 
    public static class CutMeshWithPlane 
    {
        //Should return null if the mesh couldn't be cut because it doesn't intersect with the plane
        //Otherwise it should return two new meshes
        //meshTrans is needed so we can transform the cut plane to the mesh's local space 
        public static List<Mesh> CutMesh(Transform meshTrans, OrientedPlane3 orientedCutPlaneGlobal)
        {
            //Validate the input data
            if (meshTrans == null)
            {
                Debug.Log("There's transform to cut");

                return null;
            }

            Mesh mesh = meshTrans.GetComponent<MeshFilter>().mesh;

            if (mesh == null)
            {
                Debug.Log("There's no mesh to cut");

                return null;
            }


            //The plane with just a normal
            Plane3 cutPlaneGlobal = orientedCutPlaneGlobal.Plane3;

            //First check if the AABB of the mesh is intersecting with the plane
            //Otherwise we can't cut the mesh, so its a waste of time

            //To get the AABB in world space we need to use the mesh renderer
            MeshRenderer mr = meshTrans.GetComponent<MeshRenderer>();

            if (mr != null)
            {
                AABB3 aabb = new AABB3(mr.bounds);

                //The corners of this box 
                HashSet<MyVector3> corners = aabb.GetCorners();

                if (corners != null && corners.Count > 1)
                {
                    //The points are in world space so use the plane in world space
                    if (ArePointsOnOneSideOfPlane(new List<MyVector3>(corners), cutPlaneGlobal))
                    {
                        Debug.Log("This mesh can't be cut because its AABB doesnt intersect with the plane");
                    
                        return null;
                    }
                }
            }



            //The two meshes we might end up with after the cut
            //One is in front of the plane and another is in back of the plane
            HalfEdgeData3 newMeshO = new HalfEdgeData3();
            HalfEdgeData3 newMeshI = new HalfEdgeData3();

            //The data belonging to the original mesh
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Vector3[] normals = mesh.normals;

            //Save the new edges we add when cutting triangles that intersects with the plane
            //Need to be edges so we can later connect them with each other to fill the hole
            //And to remove small triangles
            HashSet<HalfEdge3> newEdgesO = new HashSet<HalfEdge3>();
            HashSet<HalfEdge3> newEdgesI = new HashSet<HalfEdge3>();


            //Transform the plane from global space to local space of the mesh
            MyVector3 planePosLocal = meshTrans.InverseTransformPoint(cutPlaneGlobal.pos.ToVector3()).ToMyVector3();
            MyVector3 planeNormalLocal = meshTrans.InverseTransformDirection(cutPlaneGlobal.normal.ToVector3()).ToMyVector3();

            Plane3 cutPlane = new Plane3(planePosLocal, planeNormalLocal);


            //Loop through all triangles in the original mesh
            for (int i = 0; i < triangles.Length; i += 3)
            {
                //Get the triangle data we need
                int triangleIndex1 = triangles[i + 0];
                int triangleIndex2 = triangles[i + 1];
                int triangleIndex3 = triangles[i + 2];

                //Positions
                Vector3 p1_unity = vertices[triangleIndex1];
                Vector3 p2_unity = vertices[triangleIndex2];
                Vector3 p3_unity = vertices[triangleIndex3];

                MyVector3 p1 = p1_unity.ToMyVector3();
                MyVector3 p2 = p2_unity.ToMyVector3();
                MyVector3 p3 = p3_unity.ToMyVector3();

                //Normals
                MyVector3 n1 = normals[triangleIndex1].ToMyVector3();
                MyVector3 n2 = normals[triangleIndex2].ToMyVector3();
                MyVector3 n3 = normals[triangleIndex3].ToMyVector3();

                //To make it easier to send data to methods
                MyMeshVertex v1 = new MyMeshVertex(p1, n1);
                MyMeshVertex v2 = new MyMeshVertex(p2, n2);
                MyMeshVertex v3 = new MyMeshVertex(p3, n3);


                //First check on which side of the plane these vertices are
                //If they are all on one side we dont have to cut the triangle
                bool is_p1_front = _Geometry.IsPointOutsidePlane(v1.position, cutPlane);
                bool is_p2_front = _Geometry.IsPointOutsidePlane(v2.position, cutPlane);
                bool is_p3_front = _Geometry.IsPointOutsidePlane(v3.position, cutPlane);


                //Build triangles belonging to respective mesh

                //All are outside the plane
                if (is_p1_front && is_p2_front && is_p3_front)
                {
                    AddTriangleToMesh(v1, v2, v3, newMeshO, newEdges: null);
                }
                //All are inside the plane
                else if (!is_p1_front && !is_p2_front && !is_p3_front)
                {
                    AddTriangleToMesh(v1, v2, v3, newMeshI, newEdges: null);
                }
                //The vertices are on different sides of the plane, so we need to cut the triangle into 3 new triangles
                else
                {
                    //We get 6 cases where each vertex is on its own in front or in the back of the plane
                    
                    //p1 is outside
                    if (is_p1_front && !is_p2_front && !is_p3_front)
                    {
                        CutTriangleOneOutside(v1, v2, v3, newMeshO, newMeshI, newEdgesI, newEdgesO, cutPlane);
                    }
                    //p1 is inside
                    else if (!is_p1_front && is_p2_front && is_p3_front)
                    {
                        CutTriangleTwoOutside(v2, v3, v1, newMeshO, newMeshI, newEdgesI, newEdgesO, cutPlane);
                    }

                    //p2 is outside
                    else if (!is_p1_front && is_p2_front && !is_p3_front)
                    {
                        CutTriangleOneOutside(v2, v3, v1, newMeshO, newMeshI, newEdgesI, newEdgesO, cutPlane);
                    }
                    //p2 is inside
                    else if (is_p1_front && !is_p2_front && is_p3_front)
                    {
                        CutTriangleTwoOutside(v3, v1, v2, newMeshO, newMeshI, newEdgesI, newEdgesO, cutPlane);
                    }

                    //p3 is outside
                    else if (!is_p1_front && !is_p2_front && is_p3_front)
                    {
                        CutTriangleOneOutside(v3, v1, v2, newMeshO, newMeshI, newEdgesI, newEdgesO, cutPlane);
                    }
                    //p3 is inside
                    else if (is_p1_front && is_p2_front && !is_p3_front)
                    {
                        CutTriangleTwoOutside(v1, v2, v3, newMeshO, newMeshI, newEdgesI, newEdgesO, cutPlane);
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


            //Find opposite edges to each edge
            //This is a slow process, so should be done only if the mesh is intersecting with the plane
            newMeshO.ConnectAllEdgesSlow();
            newMeshI.ConnectAllEdgesSlow();

            //Display all edges which have no opposite
            DebugHalfEdge.DisplayEdgesWithNoOpposite(newMeshO.edges, meshTrans, Color.white);
            DebugHalfEdge.DisplayEdgesWithNoOpposite(newMeshI.edges, meshTrans, Color.white);


            //Remove small triangles at the seam where we did the cut because they will cause shading issues if the surface is smooth
            //RemoveSmallTriangles(F_Mesh, newEdges);


            //Split each mesh into separate meshes if the original mesh is not connected, meaning it has islands
            HashSet<HalfEdgeData3> newMeshesO = SeparateMeshIslands(newMeshO);
            HashSet<HalfEdgeData3> newMeshesI = SeparateMeshIslands(newMeshI);


            //Fill the holes in the mesh
            HashSet<Hole> allHoles = FillHoles(newEdgesI, newEdgesO, orientedCutPlaneGlobal, meshTrans, planeNormalLocal);


            //Connect the holes with respective mesh
            AddHolesToMeshes(newMeshesO, newMeshesI, allHoles);


            //Finally generate standardized Unity meshes
            List<Mesh> cuttedUnityMeshes = new List<Mesh>();

            foreach (HalfEdgeData3 meshData in newMeshesO)
            {
                MyMesh myMesh = meshData.ConvertToMyMesh("Outside mesh", MyMesh.MeshStyle.HardAndSoftEdges);

                Mesh unityMesh = myMesh.ConvertToUnityMesh(generateNormals: false);

                cuttedUnityMeshes.Add(unityMesh);
            }

            foreach (HalfEdgeData3 meshData in newMeshesI)
            {
                MyMesh myMesh = meshData.ConvertToMyMesh("Inside mesh", MyMesh.MeshStyle.HardAndSoftEdges);

                Mesh unityMesh = myMesh.ConvertToUnityMesh(generateNormals: false);

                cuttedUnityMeshes.Add(unityMesh);
            }



            return cuttedUnityMeshes;
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
                    AddTriangleToMesh(v1_I, v2_I, v3_I, holeMeshI, null);
                    AddTriangleToMesh(v1_O, v3_O, v2_O, holeMeshO, null);
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
        //F means that this vertex is outside of the plane
        private static void CutTriangleOneOutside(MyMeshVertex O1, MyMeshVertex I1, MyMeshVertex I2, HalfEdgeData3 newMeshO, HalfEdgeData3 newMeshI, HashSet<HalfEdge3> newEdgesI, HashSet<HalfEdge3> newEdgesO, Plane3 cutPlane)
        {
            //Cut the triangle by using edge-plane intersection
            //Triangles in Unity are ordered clockwise, so form edges that intersects with the plane:
            Edge3 e_O1I1 = new Edge3(O1.position, I1.position);
            //Edge3 e_B1B2 = new Edge3(B1, B2); //Not needed because never intersects with the plane
            Edge3 e_I2O1 = new Edge3(I2.position, O1.position);

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

            //The intersection vertices
            MyMeshVertex v_O1I1 = new MyMeshVertex(pos_O1I1, normal_O1I1);
            MyMeshVertex v_I2O1 = new MyMeshVertex(pos_I2O1, normal_I2O1);


            //Form 3 new triangles
            //Outside
            AddTriangleToMesh(v_O1I1, v_I2O1, O1, newMeshO, newEdgesO);
            //Inside
            AddTriangleToMesh(v_O1I1, I1, I2, newMeshI, null);
            AddTriangleToMesh(v_I2O1, v_O1I1, I2, newMeshI, newEdgesI);
        }



        //Cut a triangle where two vertices are inside and the other vertex is outside
        //Make sure they are sorted clockwise: O1-O2-I1
        //F means that this vertex is outside the plane
        private static void CutTriangleTwoOutside(MyMeshVertex O1, MyMeshVertex O2, MyMeshVertex I1, HalfEdgeData3 newMeshO, HalfEdgeData3 newMeshI, HashSet<HalfEdge3> newEdgesI, HashSet<HalfEdge3> newEdgesO, Plane3 cutPlane)
        {
            //Cut the triangle by using edge-plane intersection
            //Triangles in Unity are ordered clockwise, so form edges that intersects with the plane:
            Edge3 e_O2I1 = new Edge3(O2.position, I1.position);
            //Edge3 e_F1F2 = new Edge3(F1, F2); //Not needed because never intersects with the plane
            Edge3 e_I1O1 = new Edge3(I1.position, O1.position);

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

            //The intersection vertices
            MyMeshVertex v_O2I1 = new MyMeshVertex(pos_O2I1, normal_O2I1);
            MyMeshVertex v_I1O1 = new MyMeshVertex(pos_I1O1, normal_I1O1);


            //Form 3 new triangles
            //Outside
            AddTriangleToMesh(v_O2I1, v_I1O1, O2, newMeshO, newEdgesO);
            AddTriangleToMesh(O2, v_I1O1, O1, newMeshO, null);
            //Inside
            AddTriangleToMesh(v_I1O1, v_O2I1, I1, newMeshI, newEdgesI);
        }



        //Help method to build a triangle and add it to a mesh
        //v1-v2-v3 should be sorted clock-wise
        //v1-v2 should be the cut edge (if we have a cut edge), and we know this triangle has a cut edge if newEdges != null
        private static void AddTriangleToMesh(MyMeshVertex v1, MyMeshVertex v2, MyMeshVertex v3, HalfEdgeData3 mesh, HashSet<HalfEdge3> newEdges)
        {
            //Create three new vertices
            HalfEdgeVertex3 half_v1 = new HalfEdgeVertex3(v1.position, v1.normal);
            HalfEdgeVertex3 half_v2 = new HalfEdgeVertex3(v2.position, v2.normal);
            HalfEdgeVertex3 half_v3 = new HalfEdgeVertex3(v3.position, v3.normal);

            //Create three new half-edges that points TO these vertices
            HalfEdge3 e_to_v1 = new HalfEdge3(half_v1);
            HalfEdge3 e_to_v2 = new HalfEdge3(half_v2);
            HalfEdge3 e_to_v3 = new HalfEdge3(half_v3);

            //Create the face (which is a triangle) which needs a reference to one of the edges
            HalfEdgeFace3 f = new HalfEdgeFace3(e_to_v1);


            //Connect the data:

            //Connect the edges clock-wise
            e_to_v1.nextEdge = e_to_v2;
            e_to_v2.nextEdge = e_to_v3;
            e_to_v3.nextEdge = e_to_v1;

            e_to_v1.prevEdge = e_to_v3;
            e_to_v2.prevEdge = e_to_v1;
            e_to_v3.prevEdge = e_to_v2;

            //Each vertex needs a reference to an edge going FROM that vertex
            half_v1.edge = e_to_v2;
            half_v2.edge = e_to_v3;
            half_v3.edge = e_to_v1;

            //Each edge needs a reference to the face
            e_to_v1.face = f;
            e_to_v2.face = f;
            e_to_v3.face = f;

            //Each edge needs an opposite edge
            //This is slow process but we need it to be able to split meshes which are not connected
            //You could do this afterwards when all triangles have been generate, but Im not sure which is the fastest...

            //Save the data
            mesh.verts.Add(half_v1);
            mesh.verts.Add(half_v2);
            mesh.verts.Add(half_v3);

            mesh.edges.Add(e_to_v1);
            mesh.edges.Add(e_to_v2);
            mesh.edges.Add(e_to_v3);

            mesh.faces.Add(f);


            //Save the new edge
            if (newEdges != null)
            {
                //We know the knew edge goes from v1 to v2, so we should save the half-edge that points to v2
                newEdges.Add(e_to_v2);
            }
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
