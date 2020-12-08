using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Habrador_Computational_Geometry
{
    //Similar to Unity's mesh
    public class MyMesh
    {
        public List<MyVector3> vertices;
        public List<MyVector3> normals;
        public List<int> triangles;


        public MyMesh()
        {
            vertices = new List<MyVector3>();
            normals = new List<MyVector3>();
            triangles = new List<int>();
        }



        //Add a triangle (oriented clock-wise) to the mesh
        //If we want hard edges, set shareVertices to false. Otherwise we will get a smooth surface
        public void AddTriangle(MyMeshVertex v1, MyMeshVertex v2, MyMeshVertex v3, bool shareVertices)
        {
            int index1 = AddVertexAndReturnIndex(v1, shareVertices);
            int index2 = AddVertexAndReturnIndex(v2, shareVertices);
            int index3 = AddVertexAndReturnIndex(v3, shareVertices);

            AddTrianglePositions(index1, index2, index3);
        }



        //Add a vertex to the mesh and return its position in the array
        //If we want hard edges, set shareVertices to false. Otherwise we will get a smooth surface
        public int AddVertexAndReturnIndex(MyMeshVertex v, bool shareVertices)
        {
            int vertexPosInList = -1;

            if (shareVertices)
            {
                for (int i = 0; i < vertices.Count; i++)
                {
                    //Here we have to compare both position and normal or we can't get hard edges in combination with soft edges
                    MyVector3 thisPos = vertices[i];
                    MyVector3 thisNormal = normals[i];

                    if (thisPos.Equals(v.position) && thisNormal.Equals(v.normal))
                    {
                        vertexPosInList = i;

                        return vertexPosInList;
                    }
                }
            }

            //If we got here it means the vertex is not in the list, so add it as the last vertex
            vertices.Add(v.position);
            normals.Add(v.normal);

            vertexPosInList = vertices.Count - 1;

            return vertexPosInList;
        }



        //Add a normal at a certain index
        //Run this after adding a vertex
        //public void AddNormal(MyVector3 normal, int index)
        //{
        //    //If the index is larger than how many values in list, add at last pos
        //    //So if index is 1 we want to add the normal to the second pos in the list
        //    //Otherwise the normal should already exist
        //    if (normals.Count <= index)
        //    {
        //        normals.Add(normal);
        //    }
        //}



        //Add triangle
        public void AddTrianglePositions(int index_1, int index_2, int index_3)
        {
            triangles.Add(index_1);
            triangles.Add(index_2);
            triangles.Add(index_3);
        }



        //Merge a mesh with this mesh
        public void MergeMesh(MyMesh otherMesh)
        {
            int numberOfVerticesBeforeMerge = vertices.Count;
        
            vertices.AddRange(otherMesh.vertices);
            normals.AddRange(otherMesh.normals);

            //Triangles are not the same because we now have more vertices
            List<int> newTriangles = otherMesh.triangles.Select(x => x + numberOfVerticesBeforeMerge).ToList();

            triangles.AddRange(newTriangles);
        }
        
        
        
        //Convert this mesh to a unity mesh
        public Mesh ConvertToUnityMesh(string name)
        {
            Mesh mesh = new Mesh();

            //MyVector3 to Vector3
            Vector3[] vertices_Unity = vertices.Select(x => x.ToVector3()).ToArray();
          
            mesh.vertices = vertices_Unity;

            mesh.SetTriangles(triangles, 0);

            //Generate normals
            if (normals.Count == 0)
            {
                mesh.RecalculateNormals();
            }
            else
            {
                //MyVector3 to Vector3
                Vector3[] normals_Unity = normals.Select(x => x.ToVector3()).ToArray();

                mesh.normals = normals_Unity;
            }

            mesh.name = name;

            mesh.RecalculateBounds();

            //Debug.Log(vertices_Unity.Length);
            //Debug.Log(triangles.Count);

            return mesh;
        }
    }
}
