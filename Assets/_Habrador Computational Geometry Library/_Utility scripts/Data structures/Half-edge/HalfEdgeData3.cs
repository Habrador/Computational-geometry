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
        //Should be called verts because have the same #letters as faces, edges, so makes it pretty
        public HashSet<HalfEdgeVertex3> verts; 

        public HashSet<HalfEdgeFace3> faces;

        public HashSet<HalfEdge3> edges;



        public HalfEdgeData3()
        {
            this.verts = new HashSet<HalfEdgeVertex3>();

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
            foreach (HalfEdge3 e in edges)
            {
                if (e.oppositeEdge == null)
                {
                    TryFindOppositeEdge(e);
                }
            }
        }

        //If we know that the vertex positions were created in the same way (no floating point precision issues) 
        //we can generate a lookup table of all edges which makes it faster to find an edge from a position
        public void ConnectAllEdgesFast()
        {
            //Create the lookup table
            //Important in this case that Edge3 is a struct
            Dictionary<Edge3, HalfEdge3> edgeLookup = new Dictionary<Edge3, HalfEdge3>();

            foreach (HalfEdge3 e in edges)
            {
                //Dont add it if its opposite is not null
                if (e.oppositeEdge != null)
                {
                    continue;
                }

                //Each edge points TO a vertex
                MyVector3 p2 = e.v.position;
                MyVector3 p1 = e.prevEdge.v.position;

                edgeLookup.Add(new Edge3(p1, p2), e);
            }

            //Connect edges
            foreach (HalfEdge3 e in edges)
            {
                //This edge is already connected
                if (e.oppositeEdge != null)
                {
                    continue;
                }

                //Each edge points TO a vertex, so the opposite edge goes in the opposite direction
                MyVector3 p1 = e.v.position;
                MyVector3 p2 = e.prevEdge.v.position;

                Edge3 edgeToLookup = new Edge3(p1, p2);

                if (edgeLookup.ContainsKey(edgeToLookup))
                {
                    HalfEdge3 eOther = edgeLookup[edgeToLookup];

                    //Connect them with each other
                    e.oppositeEdge = eOther;

                    eOther.oppositeEdge = e;

                    //Debug.Log("Found opposite edge");
                }
                //This edge doesnt exist so must be null
            }
        }



        //Connect an edge with an unknown opposite edge which has not been connected
        //If no opposite edge exists, it means it has no neighbor which is possible if there's a hole
        public void TryFindOppositeEdge(HalfEdge3 e)
        {
            TryFindOppositeEdge(e, edges);
        }


        //An optimization is to have a list of opposite edges, so we don't have to search ALL edges in the entire triangulation
        public void TryFindOppositeEdge(HalfEdge3 e, HashSet<HalfEdge3> otherEdges)
        {
            //We need to find an edge which is: 
            // - going to a position where this edge is coming from
            // - coming from a position this edge points to
            //An edge is pointing to a position
            MyVector3 pTo = e.prevEdge.v.position;
            MyVector3 pFrom = e.v.position;

            foreach (HalfEdge3 eOther in otherEdges)
            {
                //Don't need to check edges that have already been connected
                if (eOther.oppositeEdge != null)
                {
                    continue;
                }

                //Is this edge pointing from a specific vertex to a specific vertex
                //If so it means we have found an edge going in the other direction
                if (eOther.v.position.Equals(pTo) && eOther.prevEdge.v.position.Equals(pFrom))
                {
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
            verts.UnionWith(otherMesh.verts);
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

                myMesh.AddTriangle(my_v1, my_v2, my_v3, shareVertices);
            }


            Mesh unityMesh = myMesh.ConvertToUnityMesh(name);

            return unityMesh;
        }

        public static Mesh ConvertToUnityMesh(string name, HashSet<HalfEdgeFace3> faces)
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

                myMesh.AddTriangle(my_v1, my_v2, my_v3, shareVertices: false);
            }


            Mesh unityMesh = myMesh.ConvertToUnityMesh(name);

            return unityMesh;
        }



        //We have faces, but we also want a list with vertices, edges, etc
        //Assume the faces are triangles
        public static HalfEdgeData3 GenerateHalfEdgeDataFromFaces(HashSet<HalfEdgeFace3> faces)
        {
            HalfEdgeData3 meshData = new HalfEdgeData3();

            //WHat we need to fill
            HashSet<HalfEdge3> edges = new HashSet<HalfEdge3>();

            HashSet<HalfEdgeVertex3> verts = new HashSet<HalfEdgeVertex3>();

            foreach (HalfEdgeFace3 f in faces)
            {
                edges.Add(f.edge);
                edges.Add(f.edge.nextEdge);
                edges.Add(f.edge.nextEdge.nextEdge);

                verts.Add(f.edge.v);
                verts.Add(f.edge.nextEdge.v);
                verts.Add(f.edge.nextEdge.nextEdge.v);
            }

            meshData.faces = faces;
            meshData.edges = edges;
            meshData.verts = verts;

            return meshData;
        }



        //Add a triangle to this mesh

        //We dont have a normal so we have to calculate it, so make sure v1-v2-v3 is clock-wise
        public HalfEdgeFace3 AddTriangle(MyVector3 p1, MyVector3 p2, MyVector3 p3)
        {
            MyVector3 normal = MyVector3.Normalize(MyVector3.Cross(p3 - p2, p1 - p2));

            MyMeshVertex v1 = new MyMeshVertex(p1, normal);
            MyMeshVertex v2 = new MyMeshVertex(p2, normal);
            MyMeshVertex v3 = new MyMeshVertex(p3, normal);

            HalfEdgeFace3 f = AddTriangle(v1, v2, v3);

            return f;
        }

        //v1-v2-v3 should be clock-wise which is Unity standard
        public HalfEdgeFace3 AddTriangle(MyMeshVertex v1, MyMeshVertex v2, MyMeshVertex v3)
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
            verts.Add(half_v1);
            verts.Add(half_v2);
            verts.Add(half_v3);

            edges.Add(e_to_v1);
            edges.Add(e_to_v2);
            edges.Add(e_to_v3);

            faces.Add(f);

            return f;
        }



        //Delete a face which we know is a triangle
        public void DeleteTriangleFace(HalfEdgeFace3 t)
        {
            //Update the data structure
            //In the half-edge data structure there's an edge going in the opposite direction
            //on the other side of this triangle with a reference to this edge, so we have to set these to null
            HalfEdge3 t_e1 = t.edge;
            HalfEdge3 t_e2 = t_e1.nextEdge;
            HalfEdge3 t_e3 = t_e2.nextEdge;

            //Opposite edge to these edges are referencing these edges, so make sure that connection is removed
            if (t_e1.oppositeEdge != null)
            {
                t_e1.oppositeEdge.oppositeEdge = null;
            }
            if (t_e2.oppositeEdge != null)
            {
                t_e2.oppositeEdge.oppositeEdge = null;
            }
            if (t_e3.oppositeEdge != null)
            {
                t_e3.oppositeEdge.oppositeEdge = null;
            }


            //Remove from the data structure

            //Remove from the list of all triangles
            faces.Remove(t);

            //Remove the edges from the list of all edges
            edges.Remove(t_e1);
            edges.Remove(t_e2);
            edges.Remove(t_e3);

            //Remove the vertices
            verts.Remove(t_e1.v);
            verts.Remove(t_e2.v);
            verts.Remove(t_e3.v);
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
