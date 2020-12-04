using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Habrador_Computational_Geometry
{
    public class MyMesh
    {
        public List<MyMeshVertex> vertices;
        public List<int> triangles;


        public MyMesh()
        {
            vertices = new List<MyMeshVertex>();
            triangles = new List<int>();
        }



        //Add a vertex to the mesh and return its position in the array
        //If we want hard edges, set shareVertices to false
        //Otherwise we will get a smooth surface
        public int AddVertexAndReturnIndex(MyMeshVertex v, bool shareVertices)
        {
            int vertexPosInList = -1;

            if (shareVertices)
            {
                for (int i = 0; i < vertices.Count; i++)
                {
                    //Here we have to compare both position and normal or we can't get hard edges in combination with soft edges
                    MyVector3 thisPos = vertices[i].pos;
                    MyVector3 thisNormal = vertices[i].normal;

                    if (thisPos.Equals(v.pos) && thisNormal.Equals(v.normal))
                    {
                        vertexPosInList = i;

                        return vertexPosInList;
                    }
                }
            }

            //If we got here it means the vertex is not in the list, so add it as the last vertex
            vertices.Add(v);

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

            //Triangles are not the same because we now have more vertices
            List<int> newTriangles = otherMesh.triangles.Select(x => x + numberOfVerticesBeforeMerge).ToList();

            triangles.AddRange(newTriangles);
        }
        
        
        
        //Convert this mesh to a unity mesh
        public Mesh ConvertToUnityMesh(string name, bool generateNormals)
        {
            Mesh mesh = new Mesh();

            //MyVector3 to Vector3
            Vector3[] vertices_Unity = vertices.Select(x => x.pos.ToVector3()).ToArray();
          
            mesh.vertices = vertices_Unity;

            mesh.SetTriangles(triangles, 0);

            //Generate normals, which is slow so should add normals by using interpolation when cutting triangles?
            if (generateNormals)
            {
                mesh.RecalculateNormals();
            }
            else
            {
                //MyVector3 to Vector3
                Vector3[] normals_Unity = vertices.Select(x => x.normal.ToVector3()).ToArray();

                mesh.normals = normals_Unity;
            }

            mesh.name = name;

            return mesh;
        }
    }
}
