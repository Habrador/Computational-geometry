using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //A collection of classes that implements the Half-Edge Data Structure
    //From https://www.openmesh.org/media/Documentations/OpenMesh-6.3-Documentation/a00010.html

    //Store data, so we dont have to convert from edges to faces and back
    //Sometimes we want to iterate over the faces, and sometimes edges, and sometimes vertices
    public class HalfEdgeData
    {
        public HashSet<HalfEdgeVertex> vertices;

        public HashSet<HalfEdgeFace> faces;

        public HashSet<HalfEdge> edges;



        public HalfEdgeData()
        {
            this.vertices = new HashSet<HalfEdgeVertex>();

            this.faces = new HashSet<HalfEdgeFace>();

            this.edges = new HashSet<HalfEdge>();
        }



        //Get a list with unique edges
        //Currently we have two half-edges for each edge, making it time consuming
        //So this method is not always needed, but can be useful
        public List<HalfEdge> GetUniqueEdges()
        {
            List<HalfEdge> uniqueEdges = new List<HalfEdge>();

            foreach (HalfEdge e in edges)
            {
                Vector3 p1 = e.v.position;
                Vector3 p2 = e.prevEdge.v.position;

                bool isInList = false;

                for (int j = 0; j < uniqueEdges.Count; j++)
                {
                    HalfEdge testEdge = uniqueEdges[j];

                    Vector3 p1_test = testEdge.v.position;
                    Vector3 p2_test = testEdge.prevEdge.v.position;

                    if ((p1 == p1_test && p2 == p2_test) || (p2 == p1_test && p1 == p2_test))
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
    public class HalfEdgeVertex
    {
        //The position of the vertex
        public Vector3 position;

        //Each vertex references an half-edge that starts at this point
        //Might seem strange because each halfEdge references a vertex the edge is going to?
        public HalfEdge edge;



        public HalfEdgeVertex(Vector3 position)
        {
            this.position = position;
        }
    }



    //This face could be a triangle or whatever we need
    public class HalfEdgeFace
    {
        //Each face references one of the halfedges bounding it
        //If you need the vertices, you can use this edge
        public HalfEdge edge;



        public HalfEdgeFace(HalfEdge edge)
        {
            this.edge = edge;
        }
    }



    //An edge going in a direction
    public class HalfEdge
    {
        //The vertex it points to
        public HalfEdgeVertex v;

        //The face it belongs to
        public HalfEdgeFace face;

        //The next half-edge inside the face (ordered clockwise)
        //The document says counter-clockwise but clockwise is easier because that's how Unity is displaying triangles
        public HalfEdge nextEdge;

        //The opposite half-edge belonging to the neighbor
        public HalfEdge oppositeEdge;

        //(optionally: the previous halfedge in the face
        public HalfEdge prevEdge;



        public HalfEdge(HalfEdgeVertex v)
        {
            this.v = v;
        }
    }
}
