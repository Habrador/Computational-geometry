using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Habrador_Computational_Geometry
{
    //Similar to Unity's mesh - known as face-vertex data structure 
    public class MyMesh
    {
        public List<MyVector3> vertices;
        public List<MyVector3> normals;
        public List<int> triangles;

        //Cant be name because then we get the name of the game object
        public string meshName;

        //What mesh style do we want?
        //Hard edges only, etc...
        public enum MeshStyle
        {
            HardEdges,
            SoftEdges,
            HardAndSoftEdges
        }


        public MyMesh(string meshName = null)
        {
            this.meshName = meshName;
        
            vertices = new List<MyVector3>();
            normals = new List<MyVector3>();
            triangles = new List<int>();
        }



        public MyMesh(Mesh mesh_Unity)
        {
            //Standardize data
            Vector3[] vertices_Unity = mesh_Unity.vertices;
            Vector3[] normals_Unity = mesh_Unity.normals;

            //Vector3 -> MyVector3 
            this.vertices = vertices_Unity.Select(x => x.ToMyVector3()).ToList();
            this.normals = normals_Unity.Select(x => x.ToMyVector3()).ToList();

            //Triangles are the same
            this.triangles = new List<int>(mesh_Unity.triangles);
        }



        //Add a triangle (oriented clock-wise) to the mesh
        public void AddTriangle(MyMeshVertex v1, MyMeshVertex v2, MyMeshVertex v3, MeshStyle meshStyle)
        {
            int index1 = AddVertexAndReturnIndex(v1, meshStyle);
            int index2 = AddVertexAndReturnIndex(v2, meshStyle);
            int index3 = AddVertexAndReturnIndex(v3, meshStyle);

            AddTrianglePositions(index1, index2, index3);
        }



        //Add a vertex to the mesh and return its position in the array
        //If we want only hard edges, set shareVertices to false. Otherwise we will get a smooth surface
        //If we want combination of smooth surface and hard edges, set shareVertices and hasHardEdges to true
        public int AddVertexAndReturnIndex(MyMeshVertex v, MeshStyle meshStyle)
        {
            int vertexPosInList = -1;

            if (meshStyle == MeshStyle.SoftEdges || meshStyle == MeshStyle.HardAndSoftEdges)
            {
                for (int i = 0; i < vertices.Count; i++)
                {
                    MyVector3 thisPos = vertices[i];
                   
                    if (thisPos.Equals(v.position))
                    {
                        //Here we have to compare both position and normal or we can't get hard edges in combination with soft edges
                        MyVector3 thisNormal = normals[i];

                        if (meshStyle == MeshStyle.HardAndSoftEdges && thisNormal.Equals(v.normal))
                        {
                            vertexPosInList = i;

                            return vertexPosInList;
                        }
                        
                        //Sometimes we dont have a normal to compare
                        if (meshStyle == MeshStyle.SoftEdges)
                        {
                            vertexPosInList = i;

                            return vertexPosInList;
                        }
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
        public Mesh ConvertToUnityMesh(bool generateNormals, string meshName = null)
        {
            Mesh mesh = new Mesh();

            //MyVector3 to Vector3
            Vector3[] vertices_Unity = vertices.Select(x => x.ToVector3()).ToArray();
          
            mesh.vertices = vertices_Unity;

            mesh.SetTriangles(triangles, 0);

            //Generate normals
            if (normals.Count == 0 || generateNormals)
            {
                mesh.RecalculateNormals();
            }
            else
            {
                //MyVector3 to Vector3
                Vector3[] normals_Unity = normals.Select(x => x.ToVector3()).ToArray();

                mesh.normals = normals_Unity;
            }

            if (meshName != null)
            {
                mesh.name = meshName;
            }
            else
            {
                if (this.meshName != null)
                {
                    mesh.name = this.meshName;
                }
            }

            

            mesh.RecalculateBounds();

            //Debug.Log(vertices_Unity.Length);
            //Debug.Log(triangles.Count);

            return mesh;
        }
    }
}
