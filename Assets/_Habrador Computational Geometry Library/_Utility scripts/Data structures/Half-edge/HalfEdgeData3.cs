using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //A collection of classes that implements the Half-Edge Data Structure
    //From https://www.openmesh.org/media/Documentations/OpenMesh-6.3-Documentation/a00010.html

    //3D space
    public class HalfEdgeData3
    {
        public HashSet<HalfEdgeVertex3> vertices;

        public HashSet<HalfEdgeFace3> faces;

        public HashSet<HalfEdge3> edges;



        public HalfEdgeData3()
        {
            this.vertices = new HashSet<HalfEdgeVertex3>();

            this.faces = new HashSet<HalfEdgeFace3>();

            this.edges = new HashSet<HalfEdge3>();
        }



        //Get a list with unique edges
        //Currently we have two half-edges for each edge, making it time consuming to go through them 
        public List<HalfEdge3> GetUniqueEdges()
        {
            List<HalfEdge3> uniqueEdges = new List<HalfEdge3>();

            foreach (HalfEdge3 e in edges)
            {
                MyVector3 p1 = e.v.position;
                MyVector3 p2 = e.prevEdge.v.position;

                bool isInList = false;

                for (int j = 0; j < uniqueEdges.Count; j++)
                {
                    HalfEdge3 testEdge = uniqueEdges[j];

                    MyVector3 p1_test = testEdge.v.position;
                    MyVector3 p2_test = testEdge.prevEdge.v.position;

                    if ((p1.Equals(p1_test) && p2.Equals(p2_test)) || (p2.Equals(p1_test) && p1.Equals(p2_test)))
                    {
                        isInList = true;

                        break;
                    }
                }

                if (!isInList)
                {
                    uniqueEdges.Add(e);
                }
            }

            return uniqueEdges;
        }



        //Connect all edges with each other which means we have all data except opposite edge of each edge
        //This should be kinda fast because when we have found an opposite edge, we can at the same time connect the opposite edge to the edge
        //And when it is connected we don't need to test it if it is pointing at the vertex when seaching for opposite edges
        public void ConnectAllEdges()
        {
            //Is it faster to create a separate set where we remove edges that have been connected to make it faster to search?
            //Or maybe we can use a counter because we don't need to search from the beginning when we TryConnectEdge because this method started at the beginning and has already connected those edges
            foreach (HalfEdge3 e in edges)
            {
                if (e.oppositeEdge == null)
                {
                    TryConnectEdge(e);
                }
            }
        }



        //Connect an edge with an unknown opposite edge which has not been connected
        //If no opposite edge exists, it means it has no neighbor which is possible if there's a hole
        public void TryConnectEdge(HalfEdge3 e)
        {
            //We need to find an edge which is going to a position where this edge is coming from
            //An edge is pointing to a position, so we need to use the previous edge
            MyVector3 posToFind = e.prevEdge.v.position;

            foreach (HalfEdge3 eOther in edges)
            {
                //We don't need to check edges that have already been connected
                if (eOther.oppositeEdge != null)
                {
                    continue;
                }
            
                //Is this edge pointing to the vertex?
                if (eOther.v.position.Equals(posToFind))
                {
                    //Dont find edges within the same face because thats not an opposite edge
                    if (e.face == eOther.face)
                    {
                        continue;
                    }

                    //Connect them with each other
                    e.oppositeEdge = eOther;

                    eOther.oppositeEdge = e;

                    break;
                }
            }
        }



        //Merge with another half-edge mesh
        public void MergeMesh(HalfEdgeData3 otherMesh)
        {
            vertices.UnionWith(otherMesh.vertices);
            faces.UnionWith(otherMesh.faces);
            edges.UnionWith(otherMesh.edges);
        }



        //Convert to Unity mesh (if we know we have stored triangles in the data structure)
        //shareVertices means that we want a smooth surface where some vertices are shared between triangles
        public Mesh ConvertToUnityMesh(string name, bool shareVertices, bool generateNormals)
        {
            MyMesh myMesh = new MyMesh();
        
            //Loop through each triangle
            foreach (HalfEdgeFace3 f in faces)
            {
                //These should have been stored clock-wise
                HalfEdgeVertex3 v1 = f.edge.v;
                HalfEdgeVertex3 v2 = f.edge.nextEdge.v;
                HalfEdgeVertex3 v3 = f.edge.nextEdge.nextEdge.v;

                //Standardize
                MyMeshVertex my_v1 = new MyMeshVertex(v1.position, v1.normal);
                MyMeshVertex my_v2 = new MyMeshVertex(v2.position, v2.normal);
                MyMeshVertex my_v3 = new MyMeshVertex(v3.position, v3.normal);

                myMesh.AddTriangle(my_v1, my_v2, my_v3, shareVertices: true);
            }


            Mesh unityMesh = myMesh.ConvertToUnityMesh(name);

            return unityMesh;
        }
    }



    //A position
    public class HalfEdgeVertex3
    {
        //The position of the vertex
        public MyVector3 position;
        //In 3d space we also need a normal
        public MyVector3 normal;

        //Each vertex references an half-edge that starts at this point
        //Might seem strange because each halfEdge references a vertex the edge is going to?
        public HalfEdge3 edge;



        public HalfEdgeVertex3(MyVector3 position)
        {
            this.position = position;
        }

        public HalfEdgeVertex3(MyVector3 position, MyVector3 normal)
        {
            this.position = position;

            this.normal = normal;
        }
    }



    //This face could be a triangle or whatever we need
    public class HalfEdgeFace3
    {
        //Each face references one of the halfedges bounding it
        //If you need the vertices, you can use this edge
        public HalfEdge3 edge;



        public HalfEdgeFace3(HalfEdge3 edge)
        {
            this.edge = edge;
        }
    }



    //An edge going in a direction
    public class HalfEdge3
    {
        //The vertex it points TO
        //This vertex also has an edge reference, which is NOT this edge, but and edge going FROM this vertex
        public HalfEdgeVertex3 v;

        //The face it belongs to
        public HalfEdgeFace3 face;

        //The next half-edge inside the face (ordered clockwise)
        //The document says counter-clockwise but clockwise is easier because that's how Unity is displaying triangles
        public HalfEdge3 nextEdge;

        //The opposite half-edge belonging to the neighbor (if there's a neighbor, otherwise its just null)
        public HalfEdge3 oppositeEdge;

        //(optionally) the previous halfedge in the face
        //If we assume the face is closed, then we could identify this edge by walking forward until we reach it
        public HalfEdge3 prevEdge;



        public HalfEdge3(HalfEdgeVertex3 v)
        {
            this.v = v;
        }
    }
}
