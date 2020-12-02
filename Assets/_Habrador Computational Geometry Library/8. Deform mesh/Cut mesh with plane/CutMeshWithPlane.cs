using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Cut a meth with a plane
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
        

            //The two meshes we might end up with after the cut
            //One is in front of the plane and another is in back of the plane
            MyMesh F_Mesh = new MyMesh();
            MyMesh B_Mesh = new MyMesh();

            //Loop through all triangles in the original mesh
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Vector3[] normals = mesh.normals;

            //Save the new edges we add when cutting triangles that intersects with the plane
            //Need to be edges so we can later connect them with each other to fill the hole
            HashSet<Edge3> newEdges = new HashSet<Edge3>();

            //Transform the plane from global space to local space
            MyVector3 planePosLocal = meshTrans.InverseTransformPoint(cutPlaneGlobal.pos.ToVector3()).ToMyVector3();
            MyVector3 planeNormalLocal = meshTrans.InverseTransformDirection(cutPlaneGlobal.normal.ToVector3()).ToMyVector3();

            Plane3 cutPlane = new Plane3(planePosLocal, planeNormalLocal);

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int triangleIndex1 = triangles[i + 0];
                int triangleIndex2 = triangles[i + 1];
                int triangleIndex3 = triangles[i + 2];

                Vector3 p1_unity = vertices[triangleIndex1];
                Vector3 p2_unity = vertices[triangleIndex2];
                Vector3 p3_unity = vertices[triangleIndex3];

                MyVector3 p1 = p1_unity.ToMyVector3();
                MyVector3 p2 = p2_unity.ToMyVector3();
                MyVector3 p3 = p3_unity.ToMyVector3();

                //MyVector3 n1 = normals[triangleIndex1].ToMyVector3();
                //MyVector3 n2 = normals[triangleIndex2].ToMyVector3();
                //MyVector3 n3 = normals[triangleIndex3].ToMyVector3();

                //First check on which side of the plane these vertices are
                //If they are all on one side we dont have to cut the triangle
                bool is_p1_front = _Geometry.IsPointFrontOfPlane(cutPlane, p1);
                bool is_p2_front = _Geometry.IsPointFrontOfPlane(cutPlane, p2);
                bool is_p3_front = _Geometry.IsPointFrontOfPlane(cutPlane, p3);


                //Build triangles belonging to respective mesh

                //All are in front of the plane
                if (is_p1_front && is_p2_front && is_p3_front)
                {
                    AddTriangleToMesh(p1, p2, p3, F_Mesh);
                }
                //All are in back of the plane
                else if (!is_p1_front && !is_p2_front && !is_p3_front)
                {
                    AddTriangleToMesh(p1, p2, p3, B_Mesh);
                }
                //The vertices are on different sides of the plane, so we need to cut the triangle into 3 new triangles
                else
                {
                    //We get 6 cases where each vertex is on its own in front or in the back of the plane
                    
                    //p1 is front
                    if (is_p1_front && !is_p2_front && !is_p3_front)
                    {
                        CutTriangleOneInFront(p1, p2, p3, F_Mesh, B_Mesh, newEdges, cutPlane);
                    }
                    //p1 is back
                    else if (!is_p1_front && is_p2_front && is_p3_front)
                    {
                        CutTriangleTwoInFront(p2, p3, p1, F_Mesh, B_Mesh, newEdges, cutPlane);
                    }

                    //p2 is front
                    else if (!is_p1_front && is_p2_front && !is_p3_front)
                    {
                        CutTriangleOneInFront(p2, p3, p1, F_Mesh, B_Mesh, newEdges, cutPlane);
                    }
                    //p2 is back
                    else if (is_p1_front && !is_p2_front && is_p3_front)
                    {
                        CutTriangleTwoInFront(p3, p1, p2, F_Mesh, B_Mesh, newEdges, cutPlane);
                    }

                    //p3 is front
                    else if (!is_p1_front && !is_p2_front && is_p3_front)
                    {
                        CutTriangleOneInFront(p3, p1, p2, F_Mesh, B_Mesh, newEdges, cutPlane);
                    }
                    //p3 is back
                    else if (is_p1_front && is_p2_front && !is_p3_front)
                    {
                        CutTriangleTwoInFront(p1, p2, p3, F_Mesh, B_Mesh, newEdges, cutPlane);
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
            //There might be multiple holes depending on the shape of the original mesh
            List<MyVector3> fillPolygon = new List<MyVector3>();




            //Generate Unity standardized unity meshes
            List<Mesh> cuttedMeshes = new List<Mesh>() 
            {
                    F_Mesh.ConvertToUnityMesh(),
                    B_Mesh.ConvertToUnityMesh()
            };

            return cuttedMeshes;
        }



        //Cut a triangle where one vertex is in front and the other vertices are back
        //Make sure they are sorted clockwise: F1-B1-B2
        private static void CutTriangleOneInFront(MyVector3 F1, MyVector3 B1, MyVector3 B2, MyMesh F_Mesh, MyMesh B_Mesh, HashSet<Edge3> newEdges, Plane3 cutPlane)
        {
            //Cut the triangle by using edge-plane intersection
            //Triangles in Unity are ordered clockwise, so form edges:
            Edge3 e_F1B1 = new Edge3(F1, B1);
            //Edge3 e_B1B2 = new Edge3(B1, B2); //Not needed because never intersects with the plane
            Edge3 e_B2F1 = new Edge3(B2, F1);

            MyVector3 v_F1B1 = _Intersections.GetLinePlaneIntersectionPoint(cutPlane, e_F1B1);
            MyVector3 v_B2F1 = _Intersections.GetLinePlaneIntersectionPoint(cutPlane, e_B2F1);

            //Form 3 new triangles
            //F
            AddTriangleToMesh(F1, v_F1B1, v_B2F1, F_Mesh);
            //B
            AddTriangleToMesh(v_F1B1, B1, B2, B_Mesh);
            AddTriangleToMesh(v_F1B1, B2, v_B2F1, B_Mesh);

            //Add the new edge
            Edge3 newEdge = new Edge3(v_F1B1, v_B2F1);

            newEdges.Add(newEdge);
        }



        //Cut a triangle where two vertices are in front and the other vertex is back
        //Make sure they are sorted clockwise: F1-F2-B1
        private static void CutTriangleTwoInFront(MyVector3 F1, MyVector3 F2, MyVector3 B1, MyMesh F_Mesh, MyMesh B_Mesh, HashSet<Edge3> newEdges, Plane3 cutPlane)
        {
            //Cut the triangle by using edge-plane intersection
            //Triangles in Unity are ordered clockwise, so form edges:
            Edge3 e_F2B1 = new Edge3(F2, B1);
            //Edge3 e_F1F2 = new Edge3(F1, F2); //Not needed because never intersects with the plane
            Edge3 e_B1F1 = new Edge3(B1, F1);

            MyVector3 v_F2B1 = _Intersections.GetLinePlaneIntersectionPoint(cutPlane, e_F2B1);
            MyVector3 v_B1F1 = _Intersections.GetLinePlaneIntersectionPoint(cutPlane, e_B1F1);

            //Form 3 new triangles
            //F
            AddTriangleToMesh(F2, v_F2B1, v_B1F1, F_Mesh);
            AddTriangleToMesh(F2, v_B1F1, F1, F_Mesh);
            //B
            AddTriangleToMesh(v_F2B1, B1, v_B1F1, B_Mesh);

            //Add the new edge
            Edge3 newEdge = new Edge3(v_F2B1, v_B1F1);

            newEdges.Add(newEdge);
        }



        //Help method to build a triangle and add it to a mesh
        private static void AddTriangleToMesh(MyVector3 p1, MyVector3 p2, MyVector3 p3, MyMesh mesh)
        {
            //Add vertices
            int index_1 = mesh.AddVertexAndReturnIndex(p1, shareVertices: true);
            int index_2 = mesh.AddVertexAndReturnIndex(p2, shareVertices: true);
            int index_3 = mesh.AddVertexAndReturnIndex(p3, shareVertices: true);

            //Build the triangles
            mesh.AddTrianglePositions(index_1, index_2, index_3);

            //Add normals
            //F_Mesh.AddNormal(n1, index_1);
            //F_Mesh.AddNormal(n2, index_2);
            //F_Mesh.AddNormal(n3, index_3);
        }
    }
}
