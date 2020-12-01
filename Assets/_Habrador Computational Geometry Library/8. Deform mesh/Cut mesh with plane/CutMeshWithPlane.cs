using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Cut a meth with a plane
    public class CutMeshWithPlane 
    {
        //Should return null if the mesh couldn't be cut because it doesn't intersect with the plane
        //Otherwise it should return two new meshes
        public static List<Mesh> CutMesh(Mesh mesh, Plane3 plane)
        {
            //Validate the input data
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

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int triangleIndex1 = triangles[i + 0];
                int triangleIndex2 = triangles[i + 1];
                int triangleIndex3 = triangles[i + 2];

                MyVector3 p1 = vertices[triangleIndex1].ToMyVector3();
                MyVector3 p2 = vertices[triangleIndex2].ToMyVector3();
                MyVector3 p3 = vertices[triangleIndex3].ToMyVector3();

                MyVector3 n1 = normals[triangleIndex1].ToMyVector3();
                MyVector3 n2 = normals[triangleIndex2].ToMyVector3();
                MyVector3 n3 = normals[triangleIndex3].ToMyVector3();

                //First check on which side of the plane these vertices are
                //If they are all on one side we dont have to cut the triangle
                bool is_p1_front = _Geometry.IsPointFrontOfPlane(plane, p1);
                bool is_p2_front = _Geometry.IsPointFrontOfPlane(plane, p2);
                bool is_p3_front = _Geometry.IsPointFrontOfPlane(plane, p3);

                //All are in front
                if (is_p1_front && is_p2_front && is_p3_front)
                {
                    //Add vertices
                    int index_1 = F_Mesh.AddVertexAndReturnIndex(p1);
                    int index_2 = F_Mesh.AddVertexAndReturnIndex(p2);
                    int index_3 = F_Mesh.AddVertexAndReturnIndex(p3);

                    //Build the triangles
                    F_Mesh.AddTrianglePositions(index_1, index_2, index_3);

                    //Add normals
                    F_Mesh.AddNormal(n1, index_1);
                    F_Mesh.AddNormal(n2, index_2);
                    F_Mesh.AddNormal(n3, index_3);
                }
                //All are in back
                else if (!is_p1_front && !is_p2_front && !is_p3_front)
                {
                    //Add vertices
                    int index_1 = B_Mesh.AddVertexAndReturnIndex(p1);
                    int index_2 = B_Mesh.AddVertexAndReturnIndex(p2);
                    int index_3 = B_Mesh.AddVertexAndReturnIndex(p3);

                    //Build the triangles
                    B_Mesh.AddTrianglePositions(index_1, index_2, index_3);

                    //Add normals
                    B_Mesh.AddNormal(n1, index_1);
                    B_Mesh.AddNormal(n2, index_2);
                    B_Mesh.AddNormal(n3, index_3);
                }


                //Triangles in Unity are ordered clockwise, so form edges and do edge-plane intersection
                //Edge3 e1 = new Edge3(p1, p2);
                //Edge3 e2 = new Edge3(p2, p3);
                //Edge3 e3 = new Edge3(p3, p1);


            }



            //Generate the new meshes
            List<Mesh> cuttedMeshes = new List<Mesh>();

            if (F_Mesh.vertices.Count > 0)
            {
                cuttedMeshes.Add(F_Mesh.ConvertToUnityMesh());    
            }
            if (B_Mesh.vertices.Count > 0)
            {
                cuttedMeshes.Add(B_Mesh.ConvertToUnityMesh());
            }


            return cuttedMeshes;
        }
    }
}
