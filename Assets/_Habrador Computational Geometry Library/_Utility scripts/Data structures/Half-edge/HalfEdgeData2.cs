using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //A collection of classes that implements the Half-Edge Data Structure
    //From https://www.openmesh.org/media/Documentations/OpenMesh-6.3-Documentation/a00010.html

    //2D space
    public class HalfEdgeData2
    {
        public HashSet<HalfEdgeVertex2> vertices;

        public HashSet<HalfEdgeFace2> faces;

        public HashSet<HalfEdge2> edges;



        public HalfEdgeData2()
        {
            this.vertices = new HashSet<HalfEdgeVertex2>();

            this.faces = new HashSet<HalfEdgeFace2>();

            this.edges = new HashSet<HalfEdge2>();
        }



        //Get a list with unique edges
        //Currently we have two half-edges for each edge, making it time consuming
        //So this method is not always needed, but can be useful
        //But be careful because it takes time to generate this list as well, so measure that the algorithm is faster by using this list
        public HashSet<HalfEdge2> GetUniqueEdges()
        {
            HashSet<HalfEdge2> uniqueEdges = new HashSet<HalfEdge2>();

            foreach (HalfEdge2 e in edges)
            {
                MyVector2 p1 = e.v.position;
                MyVector2 p2 = e.prevEdge.v.position;

                bool isInList = false;

                //TODO: Put these in a lookup dictionary to improve performance
                foreach (HalfEdge2 eUnique in uniqueEdges)
                {
                    MyVector2 p1_test = eUnique.v.position;
                    MyVector2 p2_test = eUnique.prevEdge.v.position;

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
    public class HalfEdgeVertex2
    {
        //The position of the vertex
        public MyVector2 position;

        //Each vertex references an half-edge that starts at this point
        //Might seem strange because each halfEdge references a vertex the edge is going to?
        public HalfEdge2 edge;



        public HalfEdgeVertex2(MyVector2 position)
        {
            this.position = position;
        }
    }



    //This face could be a triangle or whatever we need
    public class HalfEdgeFace2
    {
        //Each face references one of the halfedges bounding it
        //If you need the vertices, you can use this edge
        public HalfEdge2 edge;



        public HalfEdgeFace2(HalfEdge2 edge)
        {
            this.edge = edge;
        }
    }



    //An edge going in a direction
    public class HalfEdge2
    {
        //The vertex it points to
        public HalfEdgeVertex2 v;

        //The face it belongs to
        public HalfEdgeFace2 face;

        //The next half-edge inside the face (ordered clockwise)
        //The document says counter-clockwise but clockwise is easier because that's how Unity is displaying triangles
        public HalfEdge2 nextEdge;

        //The opposite half-edge belonging to the neighbor
        public HalfEdge2 oppositeEdge;

        //(optionally) the previous halfedge in the face
        //If we assume the face is closed, then we could identify this edge by walking forward
        //until we reach it
        public HalfEdge2 prevEdge;



        public HalfEdge2(HalfEdgeVertex2 v)
        {
            this.v = v;
        }
    }
}
