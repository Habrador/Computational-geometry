using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Habrador_Computational_Geometry
{
    //Similar to Unity's mesh (known as face-vertex data structure) 
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



        //Add triangles (oriented clock-wise) to the mesh
        public void AddTriangles(HashSet<Triangle3<MyMeshVertex>> trianglesToAdd, MeshStyle meshStyle)
        {
            //Soft edges is maybe slow as well???
            if (meshStyle == MeshStyle.HardEdges || meshStyle == MeshStyle.SoftEdges)
            {
                foreach (Triangle3<MyMeshVertex> triangle in trianglesToAdd)
                {
                    AddTriangle(triangle.p1, triangle.p2, triangle.p3, meshStyle);
                }
            }
            //If we have many triangles and want both soft- and hard edges, then adding triangle by triangle is very slow
            //because we to search for both position and normal 
            else
            {
                //...so we have to use a lookup table where we store position and normal, which may cause floating-point precision issues
                //Maybe we could avoid this if the half-edge data structure pointed to positions in a list???
                Dictionary<MyMeshVertex, int> vertexLookup = new Dictionary<MyMeshVertex, int>();

                foreach (Triangle3<MyMeshVertex> triangle in trianglesToAdd)
                {
                    MyMeshVertex v1 = triangle.p1;
                    MyMeshVertex v2 = triangle.p2;
                    MyMeshVertex v3 = triangle.p3;

                    AddVertexFromLookup(v1, vertexLookup);
                    AddVertexFromLookup(v2, vertexLookup);
                    AddVertexFromLookup(v3, vertexLookup);
                }
            }
        }

        //Help method to above
        private void AddVertexFromLookup(MyMeshVertex v, Dictionary<MyMeshVertex, int> vertexLookup)
        {
            int index = -1;

            bool indexExists = vertexLookup.TryGetValue(v, out index);

            if (!indexExists)
            {
                vertices.Add(v.position);
                normals.Add(v.normal);

                triangles.Add(vertices.Count - 1);

                vertexLookup.Add(v, vertices.Count - 1);
            }
            else
            {
                triangles.Add(index);
            }
        }



        //Add a triangle (oriented clock-wise) to the mesh
        public void AddTriangle(MyMeshVertex v1, MyMeshVertex v2, MyMeshVertex v3, MeshStyle meshStyle)
        {
            int index_1 = AddVertexAndReturnIndex(v1, meshStyle);
            int index_2 = AddVertexAndReturnIndex(v2, meshStyle);
            int index_3 = AddVertexAndReturnIndex(v3, meshStyle);

            triangles.Add(index_1);
            triangles.Add(index_2);
            triangles.Add(index_3);
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
                        
                        //If we want only soft edges we don't need to compare the normal
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
