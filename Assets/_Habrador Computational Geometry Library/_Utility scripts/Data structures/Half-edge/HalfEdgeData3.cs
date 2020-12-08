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

            this.normal = default;
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
        //The vertex it points to
        public HalfEdgeVertex3 v;

        //The face it belongs to
        public HalfEdgeFace3 face;

        //The next half-edge inside the face (ordered clockwise)
        //The document says counter-clockwise but clockwise is easier because that's how Unity is displaying triangles
        public HalfEdge3 nextEdge;

        //The opposite half-edge belonging to the neighbor (if there's a neighbor)
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
