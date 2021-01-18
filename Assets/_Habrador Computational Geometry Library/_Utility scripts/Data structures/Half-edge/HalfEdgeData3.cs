using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //A collection of classes that implements the Half-Edge Data Structure
    //From https://fgiesen.wordpress.com/2012/02/21/half-edge-based-mesh-representations-theory/
    // http://graphics.stanford.edu/courses/cs248-18-spring-content/lectures/07_geometryprocessing/07_geometryprocessing_slides.pdf

    //3D space
    //TODO: 
    // - An idea is to keep the original lists and then the half-edge data structure is references these lists making it easier to change vertex positions, etc? Then we only need to compare 2 ints when comparing edge directions, which should be faster than comparing 6 floats. So instead of point at a Vector3, each vertex should point at a position in a list of all vertices. This should also prevent floatig point problems when using the position as key in a dictionary
    //- Make sure the methods are more general - they are now only working on triangles
    //- COmbine some methods with 2d version: Flip edge is working in the same way in both 2d and 3d
    //- Convert to MyMesh should maybe be in MyMesh class
    public class HalfEdgeData3
    {
        //Should be called verts because have the same #letters as faces, edges, so makes it pretty
        public HashSet<HalfEdgeVertex3> verts; 

        public HashSet<HalfEdgeFace3> faces;

        public HashSet<HalfEdge3> edges;

        public enum ConnectOppositeEdges
        {
            No,
            Fast,
            Slow
        }


        public HalfEdgeData3()
        {
            this.verts = new HashSet<HalfEdgeVertex3>();

            this.faces = new HashSet<HalfEdgeFace3>();

            this.edges = new HashSet<HalfEdge3>();
        }



        //Convert from MyMesh (which is face-vertex data structure) to half-edge data structure
        //In the half-edge data structure, each edge has an opposite edge, which may have to be connected
        //You can connect these in a fast way only if there are no floating point precision issues
        public HalfEdgeData3(MyMesh mesh, ConnectOppositeEdges connectOppositeEdges) : this()
        {
            //Loop through all triangles in the mesh
            List<int> triangles = mesh.triangles;

            List<MyVector3> vertices = mesh.vertices;
            List<MyVector3> normals = mesh.normals;

            for (int i = 0; i < triangles.Count; i += 3)
            {
                int index1 = triangles[i + 0];
                int index2 = triangles[i + 1];
                int index3 = triangles[i + 2];

                MyVector3 p1 = vertices[index1];
                MyVector3 p2 = vertices[index2];
                MyVector3 p3 = vertices[index3];

                MyVector3 n1 = normals[index1];
                MyVector3 n2 = normals[index2];
                MyVector3 n3 = normals[index3];

                MyMeshVertex v1 = new MyMeshVertex(p1, n1);
                MyMeshVertex v2 = new MyMeshVertex(p2, n2);
                MyMeshVertex v3 = new MyMeshVertex(p3, n3);

                AddTriangle(v1, v2, v3);
            }

            
            //Find opposite edges to each edge
            if (connectOppositeEdges == ConnectOppositeEdges.Fast)
            {
                ConnectAllEdgesFast();
            }
            else if (connectOppositeEdges == ConnectOppositeEdges.Slow)
            {
                ConnectAllEdgesSlow();
            }
        }



        //
        // Find opposite edge to edge 
        //

        //Connect all edges with each other which means we have all data except opposite edge of each (or just some) edge
        //This should be kinda fast because when we have found an opposite edge, we can at the same time connect the opposite edge to the edge
        //And when it is connected we don't need to test if it's pointing at the vertex when seaching for opposite edges
        public void ConnectAllEdgesSlow()
        {
            //First find all edges with no connection (which will improve performance if only a few edges are missing opposites)
            HashSet<HalfEdge3> edgesWithNoOppositeConnection = new HashSet<HalfEdge3>();
            
            foreach (HalfEdge3 e in edges)
            {
                if (e.oppositeEdge == null)
                {
                    edgesWithNoOppositeConnection.Add(e);
                }
            }


            //Then find the opposite edges
            foreach (HalfEdge3 e in edgesWithNoOppositeConnection)
            {
                //This means we have aleady assigned this opposite edge 
                //because it belonged to an opposite edge of an edge we already iterated over
                if (e.oppositeEdge != null)
                {
                    continue;
                }
            
                TryFindOppositeEdge(e, edgesWithNoOppositeConnection);
            }
        }

        //If we know that the vertex positions were created in the same way (no floating point precision issues) 
        //we can generate a lookup table of all edges which should make it faster to find an opposite edge for each edge
        //This method takes rough 0.1 seconds for the bunny, while the slow method takes 1.6 seconds
        public void ConnectAllEdgesFast()
        {
            ConnectAllEdgesFast(this.edges);
        }

        public void ConnectAllEdgesFast(HashSet<HalfEdge3> myEdges)
        {
            //Create the lookup table
            //Important in this case that Edge3 is a struct
            Dictionary<Edge3, HalfEdge3> edgeLookup = new Dictionary<Edge3, HalfEdge3>();

            //We can also maybe create a list of all edges which are not connected, so we don't have to search through all edges again?
            //List<HalfEdge3> unconnectedEdges = new List<HalfEdge3>();

            foreach (HalfEdge3 e in myEdges)
            {
                //Dont add it if its opposite is not null
                //Sometimes we run this method if just a few edges are not connected
                //This means this edge is already connected, so it cant possibly be connected with the edges we want to connect
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
            foreach (HalfEdge3 e in myEdges)
            {
                //This edge is already connected
                //Is faster to first do a null check
                if (e.oppositeEdge != null)
                //if (!(e.oppositeEdge is null)) //Is slightly slower
                {
                    continue;
                }

                //Each edge points TO a vertex, so the opposite edge goes in the opposite direction
                MyVector3 p1 = e.v.position;
                MyVector3 p2 = e.prevEdge.v.position;

                Edge3 edgeToLookup = new Edge3(p1, p2);

                //This is slightly faster than first edgeLookup.ContainsKey(edgeToLookup)
                HalfEdge3 eOther = null;

                edgeLookup.TryGetValue(edgeToLookup, out eOther);

                if (eOther != null)
                {
                    //Connect them with each other
                    e.oppositeEdge = eOther;

                    eOther.oppositeEdge = e;
                }

                //This edge doesnt exist so opposite edge must be null
            }
        }



        //Connect an edge with an unknown opposite edge which has not been connected
        //If no opposite edge exists, it means it has no neighbor which is possible if there's a hole
        public void TryFindOppositeEdge(HalfEdge3 e)
        {
            TryFindOppositeEdge(e, this.edges);
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



        //
        // Merge this half edge mesh with another half-edge mesh
        //
        public void MergeMesh(HalfEdgeData3 otherMesh)
        {
            this.verts.UnionWith(otherMesh.verts);
            this.faces.UnionWith(otherMesh.faces);
            this.edges.UnionWith(otherMesh.edges);
        }



        //
        // Convert half-edge triangles to MyMesh
        //

        //Is only working if we have stored triangles in the data structure!
        //shareVertices means that we want a smooth surface where some vertices are shared between triangles
        public MyMesh ConvertToMyMesh(string meshName, MyMesh.MeshStyle meshStyle)
        {
            MyMesh myMesh = new MyMesh(meshName);

            //Loop through each triangle
            //foreach (HalfEdgeFace3 f in faces)
            //{
            //    //These have been stored clock-wise, which is what a mesh wants
            //    HalfEdgeVertex3 v1 = f.edge.v;
            //    HalfEdgeVertex3 v2 = f.edge.nextEdge.v;
            //    HalfEdgeVertex3 v3 = f.edge.nextEdge.nextEdge.v;

            //    //Standardize
            //    MyMeshVertex my_v1 = new MyMeshVertex(v1.position, v1.normal);
            //    MyMeshVertex my_v2 = new MyMeshVertex(v2.position, v2.normal);
            //    MyMeshVertex my_v3 = new MyMeshVertex(v3.position, v3.normal);

            //    myMesh.AddTriangle(my_v1, my_v2, my_v3, meshStyle);
            //}

            HashSet<Triangle3<MyMeshVertex>> triangles = new HashSet<Triangle3<MyMeshVertex>>();

            //Loop through each triangle
            foreach (HalfEdgeFace3 f in faces)
            {
                //These have been stored clock-wise, which is what a mesh wants
                HalfEdgeVertex3 v1 = f.edge.v;
                HalfEdgeVertex3 v2 = f.edge.nextEdge.v;
                HalfEdgeVertex3 v3 = f.edge.nextEdge.nextEdge.v;

                //Standardize
                MyMeshVertex my_v1 = new MyMeshVertex(v1.position, v1.normal);
                MyMeshVertex my_v2 = new MyMeshVertex(v2.position, v2.normal);
                MyMeshVertex my_v3 = new MyMeshVertex(v3.position, v3.normal);

                Triangle3<MyMeshVertex> triangle = new Triangle3<MyMeshVertex>(my_v1, my_v2, my_v3);

                triangles.Add(triangle);
            }

            myMesh.AddTriangles(triangles, meshStyle);

            return myMesh;
        }



        //
        // We have faces, but we also want a list with vertices, edges, etc
        //
        
        public static HalfEdgeData3 GenerateHalfEdgeDataFromFaces(HashSet<HalfEdgeFace3> faces)
        {
            HalfEdgeData3 meshData = new HalfEdgeData3();

            //What we need to fill
            HashSet<HalfEdge3> edges = new HashSet<HalfEdge3>();

            HashSet<HalfEdgeVertex3> verts = new HashSet<HalfEdgeVertex3>();

            foreach (HalfEdgeFace3 f in faces)
            {
                //Get all edges in this face
                List<HalfEdge3> edgesInFace = f.GetEdges();

                foreach (HalfEdge3 e in edgesInFace)
                {
                    edges.Add(e);
                    verts.Add(e.v);
                }
            }

            meshData.faces = faces;
            meshData.edges = edges;
            meshData.verts = verts;

            return meshData;
        }



        //
        // Add a triangle to the data structure
        //

        //We dont have a normal so we have to calculate it, so make sure v1-v2-v3 is clock-wise
        public HalfEdgeFace3 AddTriangle(MyVector3 p1, MyVector3 p2, MyVector3 p3, bool findOppositeEdge = false)
        {
            MyVector3 normal = MyVector3.Normalize(MyVector3.Cross(p3 - p2, p1 - p2));

            MyMeshVertex v1 = new MyMeshVertex(p1, normal);
            MyMeshVertex v2 = new MyMeshVertex(p2, normal);
            MyMeshVertex v3 = new MyMeshVertex(p3, normal);

            HalfEdgeFace3 f = AddTriangle(v1, v2, v3);

            return f;
        }


        //v1-v2-v3 should be clock-wise which is Unity standard
        public HalfEdgeFace3 AddTriangle(MyMeshVertex v1, MyMeshVertex v2, MyMeshVertex v3, bool findOppositeEdge = false)
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
            //This is slow process 
            //You could do this afterwards when all triangles have been generate
            //Doing it in this method takes 2.7 seconds for the bunny
            //Doing it afterwards takes 0.1 seconds by using the fast method and 1.6 seconds for the slow method
            //The reason is that we keep searching the list for an opposite which doesnt exist yet, so we get more searches even though
            //the list is shorter as we build up the mesh
            //But you could maybe do it here if you just add a new triangle?
            if (findOppositeEdge)
            {
                TryFindOppositeEdge(e_to_v1);
                TryFindOppositeEdge(e_to_v2);
                TryFindOppositeEdge(e_to_v3);
            }


            //Save the data
            this.verts.Add(half_v1);
            this.verts.Add(half_v2);
            this.verts.Add(half_v3);

            this.edges.Add(e_to_v1);
            this.edges.Add(e_to_v2);
            this.edges.Add(e_to_v3);

            this.faces.Add(f);

            return f;
        }


        //Is useful if we for example already have half-edge data and want to split it into different meshes
        public void AddTriangle(HalfEdgeFace3 f, bool findOppositeEdge = false)
        {
            faces.Add(f);

            edges.Add(f.edge);
            edges.Add(f.edge.nextEdge);
            edges.Add(f.edge.nextEdge.nextEdge);

            verts.Add(f.edge.v);
            verts.Add(f.edge.nextEdge.v);
            verts.Add(f.edge.nextEdge.nextEdge.v);
        }



        //
        // Delete a face from this data structure
        //

        public void DeleteFace(HalfEdgeFace3 f)
        {
            //Get all edges belonging to this face
            //TODO: This creates garbage because we create a list for each face, so maybe better to move the loop to here...
            List<HalfEdge3> edgesToRemove = f.GetEdges();

            if (edgesToRemove == null)
            {
                Debug.LogWarning("This face can't be deleted because the edges are not fully connected");

                return;
            }

            foreach (HalfEdge3 edgeToRemove in edgesToRemove)
            {
                //The opposite edge to this edge is referencing this edges, so remove that connection
                if (edgeToRemove.oppositeEdge != null)
                {
                    edgeToRemove.oppositeEdge.oppositeEdge = null;
                }

                //Remove the edge and the vertex the edge points to from the list of all vertices and edges
                this.edges.Remove(edgeToRemove);
                this.verts.Remove(edgeToRemove.v);

                //Set face reference to null, which is needed for some other methods
                edgeToRemove.face = null;
            }

            //Remove the face from the list of all faces
            this.faces.Remove(f);
        }



        //
        // Contract an edge if we know we are dealing only with triangles
        //

        //Returns all edge pointing to the new vertex
        public HashSet<HalfEdge3> ContractTriangleHalfEdge(HalfEdge3 e, MyVector3 mergePos, System.Diagnostics.Stopwatch timer = null)
        {
            //Step 1. Get all edges pointing to the vertices we will merge
            //And edge is going TO a vertex, so this edge goes from v1 to v2
            HalfEdgeVertex3 v1 = e.prevEdge.v;
            HalfEdgeVertex3 v2 = e.v;

            //timer.Start();
            //It's better to get these before we remove triangles because then we will get a messed up half-edge system? 
            HashSet<HalfEdge3> edgesGoingToVertex_v1 = v1.GetEdgesPointingToVertex(this);
            HashSet<HalfEdge3> edgesGoingToVertex_v2 = v2.GetEdgesPointingToVertex(this);
            //timer.Stop();

            //Step 2. Remove the triangles, which will create a hole, 
            //and the edges on the opposite sides of the hole are connected
            RemoveTriangleAndConnectOppositeSides(e);

            //We might also have an opposite triangle, so we may have to delete a total of two triangles
            if (e.oppositeEdge != null)
            {
                RemoveTriangleAndConnectOppositeSides(e.oppositeEdge);
            }


            //Step 3. Move the vertices to the merge position
            //Some of these edges belong to the triangles we removed, but it doesnt matter because this operation is fast

            //We can at the same time find the edges pointing to the new vertex
            HashSet<HalfEdge3> edgesPointingToVertex = new HashSet<HalfEdge3>();

            if (edgesGoingToVertex_v1 != null)
            {
                foreach (HalfEdge3 edgeToV in edgesGoingToVertex_v1)
                {
                    //This edge belonged to one of the faces we removed
                    if (edgeToV.face == null)
                    {
                        continue;
                    }
                
                    edgeToV.v.position = mergePos;

                    edgesPointingToVertex.Add(edgeToV);
                }
            }
            if (edgesGoingToVertex_v2 != null)
            {
                foreach (HalfEdge3 edgeToV in edgesGoingToVertex_v2)
                {
                    //This edge belonged to one of the faces we removed
                    if (edgeToV.face == null)
                    {
                        continue;
                    }

                    edgeToV.v.position = mergePos;

                    edgesPointingToVertex.Add(edgeToV);
                }
            }


            return edgesPointingToVertex;
        }

        //Help method to above
        private void RemoveTriangleAndConnectOppositeSides(HalfEdge3 e)
        {
            //AB is the edge we want to contract
            HalfEdge3 e_AB = e;
            HalfEdge3 e_BC = e.nextEdge;
            HalfEdge3 e_CA = e.nextEdge.nextEdge;

            //The triangle belonging to this edge
            HalfEdgeFace3 f_ABC = e.face;

            //Delete the triangle (which will also delete the vertices and edges belonging to that face)
            //We have to do it before we re-connect the edges because it sets all opposite edges to null, making a hole
            DeleteFace(f_ABC);

            //Connect the opposite edges of the edges which are not a part of the edge we want to delete
            //This will connect the opposite sides of the hole
            //We move vertices in another method
            if (e_BC.oppositeEdge != null)
            {
                //The edge on the opposite side of BC should have its opposite edge connected with the opposite edge of CA
                e_BC.oppositeEdge.oppositeEdge = e_CA.oppositeEdge;
            }
            if (e_CA.oppositeEdge != null)
            {
                e_CA.oppositeEdge.oppositeEdge = e_BC.oppositeEdge;
            }
        }
    }



    //
    // A position in the half-edge data structure
    //
    public class HalfEdgeVertex3
    {
        //The position of the vertex
        public MyVector3 position;
        //In 3d space we also need a normal, which should maybe be a class so it can be null
        //Instead of storing normals, uvs, etc for each vertex, some people are using a data structure called "wedge"
        //A wedge includes the same normal, uv, etc for the vertices that's sharing this data. 
        //For example, some normals are the same to get a smooth edge and then they all have the same wedge
        //So if the wedge is not the same for two vertices with the same position, we know we have to add both vertices because we have an hard edge
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



        //Return all edges going to this vertex = all edges that references this vertex position
        //meshData is needed if we cant spring around this vertex because there might be a hole in the mesh
        //If so we have to search through all edges in the entire mesh
        public HashSet<HalfEdge3> GetEdgesPointingToVertex(HalfEdgeData3 meshData)
        {
            HashSet<HalfEdge3> allEdgesGoingToVertex = new HashSet<HalfEdge3>();

            //This is the edge that goes to this vertex
            HalfEdge3 currentEdge = this.edge.prevEdge;

            //if (currentEdge == null) Debug.Log("Edge is null");

            int safety = 0;

            do
            {
                allEdgesGoingToVertex.Add(currentEdge);

                //This edge is going to the vertex but in another triangle
                HalfEdge3 oppositeEdge = currentEdge.oppositeEdge;

                if (oppositeEdge == null)
                {
                    //Debug.LogWarning("We cant rotate around this vertex because there are holes in the mesh");

                    //Better to clear than to null or we have to create a new hashset when filling it the brute force way 
                    allEdgesGoingToVertex.Clear();

                    break;
                }

                //if (oppositeEdge == null) Debug.Log("Opposite edge is null");

                currentEdge = oppositeEdge.prevEdge;

                safety += 1;

                if (safety > 1000)
                {
                    Debug.LogWarning("Stuck in infinite loop when getting all edges around a vertex");

                    allEdgesGoingToVertex.Clear();

                    break;
                }
            }
            while (currentEdge != this.edge.prevEdge);



            //If there are holes in the triangulation around the vertex, 
            //we have to use the brute force approach and look at edges
            if (allEdgesGoingToVertex.Count == 0 && meshData != null)
            {
                HashSet<HalfEdge3> edges = meshData.edges;

                foreach (HalfEdge3 e in edges)
                {
                    //An edge points TO a vertex
                    if (e.v.position.Equals(position))
                    {
                        allEdgesGoingToVertex.Add(e);
                    }
                }
            }


            return allEdgesGoingToVertex;
        }
    }



    //
    // A face in the half-edge data structure
    //

    //This face could be a triangle or whatever we need
    public class HalfEdgeFace3
    {
        //Each face references one of the halfedges bounding it
        //If you need the vertices, you can use this edge
        public HalfEdge3 edge;

        //Flood-filling is a common operation, and this bool should improve the performance
        public bool hasVisisted;


        public HalfEdgeFace3(HalfEdge3 edge)
        {
            this.edge = edge;
        }


        //Get all edges that make up this face
        //If you need all vertices you can use this method because each edge points to a vertex
        public List<HalfEdge3> GetEdges()
        {
            List<HalfEdge3> allEdges = new List<HalfEdge3>();
        
            HalfEdge3 currentEdge = this.edge;

            int safety = 0;

            do
            {
                allEdges.Add(currentEdge);

                currentEdge = currentEdge.nextEdge;

                safety += 1;

                if (safety > 100000)
                {
                    Debug.LogWarning("Stuck in infinite loop when getting all edges from a face");

                    return null;
                }
            }
            while (currentEdge != this.edge);

            return allEdges;
        }
    }



    //
    // An edge in the half-edge data structure
    //

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


        //The length of this edge
        public float Length()
        {
            //The edge points TO a vertex
            MyVector3 p2 = v.position;
            MyVector3 p1 = prevEdge.v.position;

            float length = MyVector3.Distance(p1, p2);

            return length;
        }

        public float LengthSqr()
        {
            //The edge points TO a vertex
            MyVector3 p2 = v.position;
            MyVector3 p1 = prevEdge.v.position;

            float length = MyVector3.SqrDistance(p1, p2);

            return length;
        }
    }
}
