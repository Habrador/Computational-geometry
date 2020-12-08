using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Cut a meth with a plane
    //TODO:
    //- Remove sharp triangles to get a better triangulation by measuring the length of each edge. This should also fix problem with ugly normals
    //- Normalize the data to 0-1 to avoid floating point precision issues
    //- A "outside" and/or "inside" mesh may consist of multiple meshes, so you may need to split those and return more than 2 meshes
    //- Submeshes should be avoided because of performance, so ignore those. Use uv to illustrate where the cut is. If you need to illustrate the cut with a different material, you can return two meshes and use the one that was part of the originl mesh to generate the convex hull 
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
                bool is_p1_front = _Geometry.IsPointOutsidePlane(cutPlane, v1.position);
                bool is_p2_front = _Geometry.IsPointOutsidePlane(cutPlane, v2.position);
                bool is_p3_front = _Geometry.IsPointOutsidePlane(cutPlane, v3.position);


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
                        CutTriangleOneOutside(v1, v2, v3, newMeshO, newMeshI, newEdgesO, newEdgesI, cutPlane);
                    }
                    //p1 is inside
                    else if (!is_p1_front && is_p2_front && is_p3_front)
                    {
                        CutTriangleTwoOutside(v2, v3, v1, newMeshO, newMeshI, newEdgesO, newEdgesI, cutPlane);
                    }

                    //p2 is outside
                    else if (!is_p1_front && is_p2_front && !is_p3_front)
                    {
                        CutTriangleOneOutside(v2, v3, v1, newMeshO, newMeshI, newEdgesO, newEdgesI, cutPlane);
                    }
                    //p2 is inside
                    else if (is_p1_front && !is_p2_front && is_p3_front)
                    {
                        CutTriangleTwoOutside(v3, v1, v2, newMeshO, newMeshI, newEdgesO, newEdgesI, cutPlane);
                    }

                    //p3 is outside
                    else if (!is_p1_front && !is_p2_front && is_p3_front)
                    {
                        CutTriangleOneOutside(v3, v1, v2, newMeshO, newMeshI, newEdgesO, newEdgesI, cutPlane);
                    }
                    //p3 is inside
                    else if (is_p1_front && is_p2_front && !is_p3_front)
                    {
                        CutTriangleTwoOutside(v1, v2, v3, newMeshO, newMeshI, newEdgesO, newEdgesI, cutPlane);
                    }
                    else
                    {
                        Debug.Log("No case was gound where we split triangle into 3 new triangles");
                    }
                }
            }


            //Generate the new meshes only needed the old mesh intersected with the plane
            if (newMeshO.vertices.Count == 0 || newMeshI.vertices.Count == 0)
            {
                return null;
            }



            //Make sure each edge has an opposite edge if an opposite edge exists
            //We have to connect all edges because we later need it to make a triangulation walk to find mesh islands
            newMeshO.ConnectAllEdges();
            newMeshI.ConnectAllEdges();


            //Remove small triangles at the seam where we did the cut because they will cause shading issues if the surface is smooth
            //RemoveSmallTriangles(F_Mesh, newEdges);


            //Split the meshes into separate meshes if the original mesh is not connected



            //Fill the holes in the mesh which is easier to do after we split the meshes
            //FillHole(newMeshO, newEdgesO, cutPlane);
            FillHole(newMeshI, newEdgesI, cutPlane, meshTrans);



            //Generate Unity standardized unity meshes
            List<Mesh> cuttedMeshes = new List<Mesh>()
            {
                    newMeshO.ConvertToUnityMesh("F mesh", shareVertices: true, generateNormals: false),
                    newMeshI.ConvertToUnityMesh("B mesh", shareVertices: true, generateNormals: false)
            };

            return cuttedMeshes;
        }


        
        //Fill the hole in the mesh
        //There might be multiple holes depending on the shape of the original mesh, so make sure you've already separated the meshes
        //So cutEdges may belong to several meshes???
        //meshTrans is only needed for debugging so debug lines have the same dimensions as the final mesh
        private static void FillHole(HalfEdgeData3 mesh, HashSet<HalfEdge3> cutEdges, Plane3 cutPlane, Transform meshTrans)
        {
            if (cutEdges == null || cutEdges.Count == 0)
            {
                Debug.Log("This mesh has no hole");

                return;
            }
        
        
            //Find an edge in the mesh which is null and where the two other edges are connected to opposite edge
            //HalfEdge3 startEdge = null;

            //HashSet<HalfEdge3> edges = mesh.edges;

            //foreach (HalfEdge3 e in edges)
            //{
            //    if (e.oppositeEdge == null && e.nextEdge.oppositeEdge != null && e.prevEdge.oppositeEdge != null)
            //    {
            //        startEdge = e;

            //        break;
            //    }
            //}

            //if (startEdge == null)
            //{
            //    Debug.Log("No start edge to fill the hole could be found");

            //    return;
            //}

            //Faster to just pick a start edge if we assume all edges in cutEdges belongs to a single hole
            HalfEdge3 startEdge = cutEdges.FakePop();

            //Add it back so we can stop the algorithm
            cutEdges.Add(startEdge);


            //This means we have found a cut edge
            List<HalfEdge3> sortedHoleEdges = new List<HalfEdge3>() { startEdge };
            //Is needed to stop the algorithm
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
                else if (nextEdge == startEdge)
                {
                    Debug.Log($"Number of edges to fill this hole: {sortedHoleEdges.Count}");
                    
                    break;
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


            //Debug
            //foreach (HalfEdge3 e in sortedHoleEdges)
            //{
            //    Debug.DrawLine(meshTrans.TransformPoint(e.v.position.ToVector3()), Vector3.zero, Color.white, 5f);
            //}

            //Transform these vertices to local position of cut plane, to make it easier to triangulate with Ear Clipping algorithm
        }



        //Cut a triangle where one vertex is outside and the other vertices are inside
        //Make sure they are sorted clockwise: O1-I1-I2
        //F means that this vertex is outside of the plane
        private static void CutTriangleOneOutside(MyMeshVertex O1, MyMeshVertex I1, MyMeshVertex I2, HalfEdgeData3 newMeshO, HalfEdgeData3 newMeshI, HashSet<HalfEdge3> newEdgesO, HashSet<HalfEdge3> newEdgesI, Plane3 cutPlane)
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
        private static void CutTriangleTwoOutside(MyMeshVertex O1, MyMeshVertex O2, MyMeshVertex I1, HalfEdgeData3 newMeshO, HalfEdgeData3 newMeshI, HashSet<HalfEdge3> newEdgesO, HashSet<HalfEdge3> newEdgesI, Plane3 cutPlane)
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
        //v1-v2 should be the cut edge, and we know this triangle has a cut edge if newEdges != null
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

            //Each edge also needs an opposite edge, which is maybe better to add later because it requires some searching
            //But maybe more efficient to do it here as we fill the data structures?


            //Save the data
            mesh.vertices.Add(half_v1);
            mesh.vertices.Add(half_v2);
            mesh.vertices.Add(half_v3);

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
            bool isInFront = _Geometry.IsPointOutsidePlane(plane, points[0]);

            for (int i = 1; i < points.Count; i++)
            {
                bool isOtherOutside = _Geometry.IsPointOutsidePlane(plane, points[i]);

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
