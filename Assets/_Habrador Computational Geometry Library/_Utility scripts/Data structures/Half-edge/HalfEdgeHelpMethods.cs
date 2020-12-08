using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class HalfEdgeHelpMethods
    {
        //
        // Flip triangle edge
        //
        //So the edge shared by two triangles is going between the two other vertices originally not part of the edge
        public static void FlipTriangleEdge(HalfEdge2 e)
        {
            //The data we need
            //This edge's triangle edges
            HalfEdge2 e_1 = e;
            HalfEdge2 e_2 = e_1.nextEdge;
            HalfEdge2 e_3 = e_1.prevEdge;
            //The opposite edge's triangle edges
            HalfEdge2 e_4 = e_1.oppositeEdge;
            HalfEdge2 e_5 = e_4.nextEdge;
            HalfEdge2 e_6 = e_4.prevEdge;
            //The 4 vertex positions
            MyVector2 aPos = e_1.v.position;
            MyVector2 bPos = e_2.v.position;
            MyVector2 cPos = e_3.v.position;
            MyVector2 dPos = e_5.v.position;

            //The 6 old vertices, we can use
            HalfEdgeVertex2 a_old = e_1.v;
            HalfEdgeVertex2 b_old = e_1.nextEdge.v;
            HalfEdgeVertex2 c_old = e_1.prevEdge.v;
            HalfEdgeVertex2 a_opposite_old = e_4.prevEdge.v;
            HalfEdgeVertex2 c_opposite_old = e_4.v;
            HalfEdgeVertex2 d_old = e_4.nextEdge.v;

            //Flip

            //Vertices
            //Triangle 1: b-c-d
            HalfEdgeVertex2 b = b_old;
            HalfEdgeVertex2 c = c_old;
            HalfEdgeVertex2 d = d_old;
            //Triangle 1: b-d-a
            HalfEdgeVertex2 b_opposite = a_opposite_old;
            b_opposite.position = bPos;
            HalfEdgeVertex2 d_opposite = c_opposite_old;
            d_opposite.position = dPos;
            HalfEdgeVertex2 a = a_old;


            //Change half-edge - half-edge connections
            e_1.nextEdge = e_3;
            e_1.prevEdge = e_5;

            e_2.nextEdge = e_4;
            e_2.prevEdge = e_6;

            e_3.nextEdge = e_5;
            e_3.prevEdge = e_1;

            e_4.nextEdge = e_6;
            e_4.prevEdge = e_2;

            e_5.nextEdge = e_1;
            e_5.prevEdge = e_3;

            e_6.nextEdge = e_2;
            e_6.prevEdge = e_4;

            //Half-edge - vertex connection
            e_1.v = b;
            e_2.v = b_opposite;
            e_3.v = c;
            e_4.v = d_opposite;
            e_5.v = d;
            e_6.v = a;

            //Half-edge - face connection
            HalfEdgeFace2 f1 = e_1.face;
            HalfEdgeFace2 f2 = e_4.face;

            e_1.face = f1;
            e_3.face = f1;
            e_5.face = f1;

            e_2.face = f2;
            e_4.face = f2;
            e_6.face = f2;

            //Face - half-edge connection
            f1.edge = e_3;
            f2.edge = e_4;

            //Vertices connection, which should have a reference to a half-edge going away from the vertex
            //Triangle 1: b-c-d
            b.edge = e_3;
            c.edge = e_5;
            d.edge = e_1;
            //Triangle 1: b-d-a
            b_opposite.edge = e_4;
            d_opposite.edge = e_6;
            a.edge = e_2;

            //Opposite-edges are not changing!
            //And neither are we adding, removing data so we dont need to update the lists with all data
        }



        //
        // Split triangle edge
        //
        //Split an edge at a point on the edge to form four new triangles, while removing two old
        //public static void SplitTriangleEdge(HalfEdge e, Vector3 splitPosition)
        //{

        //}



        //
        // Split triangle face
        //
        //Split a face (which we know is a triangle) at a point to create three new triangles while removing the old triangle
        //Could maybe make it more general so we can split a face, which consists of n edges
        public static void SplitTriangleFaceAtPoint(HalfEdgeFace2 f, MyVector2 splitPosition, HalfEdgeData2 data)
        {
            //The edges that belongs to this face
            HalfEdge2 e_1 = f.edge;
            HalfEdge2 e_2 = e_1.nextEdge;
            HalfEdge2 e_3 = e_2.nextEdge;

            //A list with new edges so we can connect the new edges with an edge on the opposite side
            HashSet<HalfEdge2> newEdges = new HashSet<HalfEdge2>();

            CreateNewFace(e_1, splitPosition, data, newEdges);
            CreateNewFace(e_2, splitPosition, data, newEdges);
            CreateNewFace(e_3, splitPosition, data, newEdges);

            //Debug.Log("New edges " + newEdges.Count);

            //Find the opposite connections
            foreach (HalfEdge2 e in newEdges)
            {
                //If we have already found a opposite
                if (e.oppositeEdge != null)
                {
                    continue;
                }

                MyVector2 eGoingTo = e.v.position;
                MyVector2 eGoingFrom = e.prevEdge.v.position;
            
                foreach (HalfEdge2 eOpposite in newEdges)
                {
                    if (e == eOpposite || eOpposite.oppositeEdge != null)
                    {
                        continue;
                    }

                    MyVector2 eGoingTo_Other = eOpposite.v.position;
                    MyVector2 eGoingFrom_Other = eOpposite.prevEdge.v.position;

                    if (eGoingTo.Equals(eGoingFrom_Other) && eGoingFrom.Equals(eGoingTo_Other))
                    {
                        e.oppositeEdge = eOpposite;
                        //Might as well connect it from the other way as well
                        eOpposite.oppositeEdge = e;

                        //Debug.Log("Found opposite");
                    }
                }
            }

            //Delete the old triangle
            DeleteTriangleFace(f, data, false);
        }



        //Create a new triangle face when splitting triangle face
        private static void CreateNewFace(HalfEdge2 e_old, MyVector2 splitPosition, HalfEdgeData2 data, HashSet<HalfEdge2> newEdges)
        {
            //This triangle has the following positons
            MyVector2 p_split = splitPosition;
            MyVector2 p_next = e_old.prevEdge.v.position;
            MyVector2 p_prev = e_old.v.position;

            //Create the new stuff
            HalfEdgeVertex2 v_split = new HalfEdgeVertex2(p_split);
            HalfEdgeVertex2 v_next = new HalfEdgeVertex2(p_next);
            HalfEdgeVertex2 v_prev = new HalfEdgeVertex2(p_prev);

            //This is the edge that has the same position as the old edge 
            HalfEdge2 e_1 = new HalfEdge2(v_prev);
            HalfEdge2 e_2 = new HalfEdge2(v_split);
            HalfEdge2 e_3 = new HalfEdge2(v_next);

            //The new face
            HalfEdgeFace2 f = new HalfEdgeFace2(e_1);


            //Create the connections
            //The new edge e has the same opposite as the old edge
            e_1.oppositeEdge = e_old.oppositeEdge;
            //But the opposite edge needs a new reference to this edge if its not a border
            if (e_1.oppositeEdge != null)
            {
                e_old.oppositeEdge.oppositeEdge = e_1;
            }
            
            //The other new edges will find the opposite in a loop when we have created all new edges
            newEdges.Add(e_2);
            newEdges.Add(e_3);

            //Create the connections between the edges
            e_1.nextEdge = e_2;
            e_1.prevEdge = e_3;

            e_2.nextEdge = e_3;
            e_2.prevEdge = e_1;

            e_3.nextEdge = e_1;
            e_3.prevEdge = e_2;

            //Each edge needs to connect to a face
            e_1.face = f;
            e_2.face = f;
            e_3.face = f;

            //The vertices need an edge that starts at that point
            v_split.edge = e_3;
            v_next.edge = e_1;
            v_prev.edge = e_2;

            //Add them to the lists
            data.faces.Add(f);

            data.edges.Add(e_1);
            data.edges.Add(e_2);
            data.edges.Add(e_3);

            data.vertices.Add(v_split);
            data.vertices.Add(v_next);
            data.vertices.Add(v_prev);
        }



        //
        // Delete a triangle
        //
        public static void DeleteTriangleFace(HalfEdgeFace2 t, HalfEdgeData2 data, bool shouldSetOppositeToNull)
        {
            //Update the data structure
            //In the half-edge data structure there's an edge going in the opposite direction
            //on the other side of this triangle with a reference to this edge, so we have to set these to null
            HalfEdge2 t_e1 = t.edge;
            HalfEdge2 t_e2 = t_e1.nextEdge;
            HalfEdge2 t_e3 = t_e2.nextEdge;

            //If we want to remove the triangle and create a hole
            //But sometimes we have created a new triangle and then we cant set the opposite to null
            if (shouldSetOppositeToNull)
            {
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
            }


            //Remove from the data structure

            //Remove from the list of all triangles
            data.faces.Remove(t);

            //Remove the edges from the list of all edges
            data.edges.Remove(t_e1);
            data.edges.Remove(t_e2);
            data.edges.Remove(t_e3);

            //Remove the vertices
            data.vertices.Remove(t_e1.v);
            data.vertices.Remove(t_e2.v);
            data.vertices.Remove(t_e3.v);
        }
    }
}
