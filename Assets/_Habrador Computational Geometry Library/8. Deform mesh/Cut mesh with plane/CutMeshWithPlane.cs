using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Cut a meth with a plane
    //TODO:
    //- Remove sharp triangles to get a better triangulation by measuring the length of each edge. This should also fix problem with ugly normals
    //- A faster way would be to add the data to some temp data structure. And when we know the mesh is intersecting with the plane, then we build the actual mesh because generating a mesh requires list searching to avoid duplicates
    //- Fix so that when we generating the polygon to fill the hole, make sure it takes into account that sometimes we have double vertices at hard edges
    //- Normalize the data
    //- In some cases a "outside" and/or "inside" mesh may consist of multiple meshes, so you may need to split those and return more than 2 meshes
    //- Submeshes should be avoided because of performance, so ignore those. Use uv to illustrate where the cut is. If you need to illustrate the cut with a different material, you can return two meshes and use the one that was part of the originl mesh to generate the convex hull 
    public static class CutMeshWithPlane 
    {
        //Should return null if the mesh couldn't be cut because it doesn't intersect with the plane
        //Otherwise it should return two new meshes
        //meshTrans is needed so we can transform the cut plane to the mesh's local space
        public static List<Mesh> CutMesh(Transform meshTrans, Plane3 cutPlaneGlobal)
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
            MyMesh F_Mesh = new MyMesh();
            MyMesh B_Mesh = new MyMesh();

            //The data belonging to the original mesh
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Vector3[] normals = mesh.normals;

            //Save the new edges we add when cutting triangles that intersects with the plane
            //Need to be edges so we can later connect them with each other to fill the hole
            HashSet<Edge3> newEdges = new HashSet<Edge3>();

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

                //Our own data structure
                MyMeshVertex v1 = new MyMeshVertex(p1, n1);
                MyMeshVertex v2 = new MyMeshVertex(p2, n2);
                MyMeshVertex v3 = new MyMeshVertex(p3, n3);


                //First check on which side of the plane these vertices are
                //If they are all on one side we dont have to cut the triangle
                bool is_p1_front = _Geometry.IsPointOutsidePlane(cutPlane, v1.pos);
                bool is_p2_front = _Geometry.IsPointOutsidePlane(cutPlane, v2.pos);
                bool is_p3_front = _Geometry.IsPointOutsidePlane(cutPlane, v3.pos);


                //Build triangles belonging to respective mesh

                //All are outside the plane
                if (is_p1_front && is_p2_front && is_p3_front)
                {
                    AddTriangleToMesh(v1, v2, v3, F_Mesh);
                }
                //All are inside the plane
                else if (!is_p1_front && !is_p2_front && !is_p3_front)
                {
                    AddTriangleToMesh(v1, v2, v3, B_Mesh);
                }
                //The vertices are on different sides of the plane, so we need to cut the triangle into 3 new triangles
                else
                {
                    //We get 6 cases where each vertex is on its own in front or in the back of the plane
                    
                    //p1 is outside
                    if (is_p1_front && !is_p2_front && !is_p3_front)
                    {
                        CutTriangleOneOutside(v1, v2, v3, F_Mesh, B_Mesh, newEdges, cutPlane);
                    }
                    //p1 is inside
                    else if (!is_p1_front && is_p2_front && is_p3_front)
                    {
                        CutTriangleTwoOutside(v2, v3, v1, F_Mesh, B_Mesh, newEdges, cutPlane);
                    }

                    //p2 is outside
                    else if (!is_p1_front && is_p2_front && !is_p3_front)
                    {
                        CutTriangleOneOutside(v2, v3, v1, F_Mesh, B_Mesh, newEdges, cutPlane);
                    }
                    //p2 is inside
                    else if (is_p1_front && !is_p2_front && is_p3_front)
                    {
                        CutTriangleTwoOutside(v3, v1, v2, F_Mesh, B_Mesh, newEdges, cutPlane);
                    }

                    //p3 is outside
                    else if (!is_p1_front && !is_p2_front && is_p3_front)
                    {
                        CutTriangleOneOutside(v3, v1, v2, F_Mesh, B_Mesh, newEdges, cutPlane);
                    }
                    //p3 is inside
                    else if (is_p1_front && is_p2_front && !is_p3_front)
                    {
                        CutTriangleTwoOutside(v1, v2, v3, F_Mesh, B_Mesh, newEdges, cutPlane);
                    }
                    else
                    {
                        Debug.Log("No case was gound where we split triangle into 3 new triangles");
                    }
                }
            }


            //Generate the new meshes only needed if we cut the original mesh
            if (F_Mesh.vertices.Count == 0 || B_Mesh.vertices.Count == 0)
            {
                return null;
            }



            //Fill the holes in the mesh
            //FillHoles(newEdges, F_Mesh, B_Mesh, cutPlane);



            //Generate Unity standardized unity meshes
            List<Mesh> cuttedMeshes = new List<Mesh>()
            {
                    F_Mesh.ConvertToUnityMesh("F mesh", generateNormals: true),
                    B_Mesh.ConvertToUnityMesh("B mesh", generateNormals: true)
            };

            return cuttedMeshes;
        }



        //Fill the holes in the mesh
        //There might be multiple holes depending on the shape of the original mesh
        private static void FillHoles(HashSet<Edge3> newEdges, MyMesh F_mesh, MyMesh B_mesh, Plane3 cutPlane)
        {
            //Add the first edge
            Edge3 startEdge = newEdges.FakePop();

            List<Edge3> fillPolygon = new List<Edge3>()
            {
                startEdge
            };

            //Loop through all other new edges until the polygon is back where it started
            int safety = 0;

            while (newEdges.Count > 0)
            {
                MyVector3 lastVertexInPolygon = fillPolygon[fillPolygon.Count - 1].p2;
            
                foreach (Edge3 e in newEdges)
                {
                    //This edge starts at the last vertex 
                    if (e.p1.Equals(lastVertexInPolygon))
                    {
                        fillPolygon.Add(e);

                        newEdges.Remove(e);

                        break;
                    }
                }
            
            
                safety += 1;

                if (safety > 50000)
                {
                    Debug.Log("Stuck in infinite loop");

                    break;
                }
            }

            /*
            float size = 0.01f;
            foreach (MyVector3 v in fillPolygon)
            {
                MyVector3 dir = MyVector3.Normalize(v - new MyVector3(0f, 0f, 0f));
            
                Debug.DrawLine(Vector3.zero, v.ToVector3() + dir.ToVector3() * size, Color.white, 20f);

                size += 0.01f;
            }
            */

            //Build the triangles, which are the same for both sides, except their normal
            MyMesh F_holeMesh = new MyMesh();
            MyMesh B_holeMesh = new MyMesh();

            foreach (Edge3 e in fillPolygon)
            {
                MyVector3 p1 = e.p1;
                MyVector3 p2 = e.p2;
                MyVector3 p3 = new MyVector3(0f, 0f, 0f); //Temp solution

                MyVector3 F_normal = -cutPlane.normal;
                MyVector3 B_normal = cutPlane.normal;

                MyMeshVertex F_v1 = new MyMeshVertex(p1, F_normal);
                MyMeshVertex F_v2 = new MyMeshVertex(p2, F_normal);
                MyMeshVertex F_v3 = new MyMeshVertex(p3, F_normal);

                MyMeshVertex B_v1 = new MyMeshVertex(p1, B_normal);
                MyMeshVertex B_v2 = new MyMeshVertex(p2, B_normal);
                MyMeshVertex B_v3 = new MyMeshVertex(p3, B_normal);

                AddTriangleToMesh(F_v2, F_v1, F_v3, F_holeMesh);

                AddTriangleToMesh(F_v1, F_v2, F_v3, B_holeMesh);
            }


            //Merge the hole with the cutted mesh
            F_mesh.MergeMesh(F_holeMesh);
            B_mesh.MergeMesh(B_holeMesh);
        }



        //Cut a triangle where one vertex is outside and the other vertices are inside
        //Make sure they are sorted clockwise: O1-I1-I2
        //F means that this vertex is outside of the plane
        private static void CutTriangleOneOutside(MyMeshVertex O1, MyMeshVertex I1, MyMeshVertex I2, MyMesh F_Mesh, MyMesh B_Mesh, HashSet<Edge3> newEdges, Plane3 cutPlane)
        {
            //Cut the triangle by using edge-plane intersection
            //Triangles in Unity are ordered clockwise, so form edges that intersects with the plane:
            Edge3 e_O1I1 = new Edge3(O1.pos, I1.pos);
            //Edge3 e_B1B2 = new Edge3(B1, B2); //Not needed because never intersects with the plane
            Edge3 e_I2O1 = new Edge3(I2.pos, O1.pos);

            //The positions of the intersection vertices
            MyVector3 pos_O1I1 = _Intersections.GetLinePlaneIntersectionPoint(cutPlane, e_O1I1);
            MyVector3 pos_I2O1 = _Intersections.GetLinePlaneIntersectionPoint(cutPlane, e_I2O1);

            //The normals of the intersection vertices
            float percentageBetween_O1I1 = MyVector3.Distance(O1.pos, pos_O1I1) / MyVector3.Distance(O1.pos, I1.pos);
            float percentageBetween_I2O1 = MyVector3.Distance(I2.pos, pos_I2O1) / MyVector3.Distance(I2.pos, O1.pos);

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
            //F
            AddTriangleToMesh(O1, v_O1I1, v_I2O1, F_Mesh);
            //B
            AddTriangleToMesh(v_O1I1, I1, I2, B_Mesh);
            AddTriangleToMesh(v_O1I1, I2, v_I2O1, B_Mesh);

            //Add the new edge so we can later fill the hole
            Edge3 newEdge = new Edge3(v_O1I1.pos, v_I2O1.pos);

            newEdges.Add(newEdge);
        }



        //Cut a triangle where two vertices are inside and the other vertex is outside
        //Make sure they are sorted clockwise: O1-O2-I1
        //F means that this vertex is outside the plane
        private static void CutTriangleTwoOutside(MyMeshVertex O1, MyMeshVertex O2, MyMeshVertex I1, MyMesh F_Mesh, MyMesh B_Mesh, HashSet<Edge3> newEdges, Plane3 cutPlane)
        {
            //Cut the triangle by using edge-plane intersection
            //Triangles in Unity are ordered clockwise, so form edges that intersects with the plane:
            Edge3 e_O2I1 = new Edge3(O2.pos, I1.pos);
            //Edge3 e_F1F2 = new Edge3(F1, F2); //Not needed because never intersects with the plane
            Edge3 e_I1O1 = new Edge3(I1.pos, O1.pos);

            //The positions of the intersection vertices
            MyVector3 pos_O2I1 = _Intersections.GetLinePlaneIntersectionPoint(cutPlane, e_O2I1);
            MyVector3 pos_I1O1 = _Intersections.GetLinePlaneIntersectionPoint(cutPlane, e_I1O1);

            //The normals of the intersection vertices
            float percentageBetween_O2I1 = MyVector3.Distance(O2.pos, pos_O2I1) / MyVector3.Distance(O2.pos, I1.pos);
            float percentageBetween_I1O1 = MyVector3.Distance(I1.pos, pos_I1O1) / MyVector3.Distance(I1.pos, O1.pos);

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
            //F
            AddTriangleToMesh(O2, v_O2I1, v_I1O1, F_Mesh);
            AddTriangleToMesh(O2, v_I1O1, O1, F_Mesh);
            //B
            AddTriangleToMesh(v_O2I1, I1, v_I1O1, B_Mesh);

            //Add the new edge so we can later fill the hole
            Edge3 newEdge = new Edge3(v_O2I1.pos, v_I1O1.pos);

            newEdges.Add(newEdge);
        }



        //Help method to build a triangle and add it to a mesh
        private static void AddTriangleToMesh(MyMeshVertex v1, MyMeshVertex v2, MyMeshVertex v3, MyMesh mesh)
        {
            //Add vertices
            int index_1 = mesh.AddVertexAndReturnIndex(v1, shareVertices: true);
            int index_2 = mesh.AddVertexAndReturnIndex(v2, shareVertices: true);
            int index_3 = mesh.AddVertexAndReturnIndex(v3, shareVertices: true);

            //Build the triangles
            mesh.AddTrianglePositions(index_1, index_2, index_3);
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
