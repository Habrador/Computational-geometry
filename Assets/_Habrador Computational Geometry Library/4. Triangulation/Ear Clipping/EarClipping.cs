using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Habrador_Computational_Geometry
{
    //Triangulate a concave hull (with holes) by using an algorithm called Ear Clipping
    //Can also triangulate convex hulls but there are faster algorithms for that 
    //This alorithm is called ear clipping and it's O(n*n) 
    //Another common algorithm is dividing it into trapezoids and it's O(n log n)
    //One can maybe do it in O(n) time but no such version is known
    public static class EarClipping
    {
        //The points on the hull (vertices) should be ordered counter-clockwise
        public static HashSet<Triangle2> Triangulate(List<MyVector2> vertices)
        {
            //The final triangles
            HashSet<Triangle2> triangulation = new HashSet<Triangle2>();


            //Validate the data
            if (vertices == null || vertices.Count <= 2)
            {
                Debug.LogWarning("Can't triangulate with Ear Clipping because too few vertices on the hull");

                return null;
            }


            //Step 0. Create a linked list connecting all vertices with each other which will make the calculations easier
            List<LinkedVertex> verticesLinked = new List<LinkedVertex>();

            for (int i = 0; i < vertices.Count; i++)
            {
                verticesLinked.Add(new LinkedVertex(vertices[i]));
            }

            //Link them
            for (int i = 0; i < verticesLinked.Count; i++)
            {
                LinkedVertex v = verticesLinked[i];

                v.prevLinkedVertex = verticesLinked[MathUtility.ClampListIndex(i - 1, verticesLinked.Count)];
                v.nextLinkedVertex = verticesLinked[MathUtility.ClampListIndex(i + 1, verticesLinked.Count)];
            }


            //Step 1. Find:
            //The convex vertices (interior angle smaller than 180 degrees)
            //The reflect vertices (interior angle greater than 180 degrees) so should maybe be called concave vertices?
            //Interior angle is the angle between two vectors inside the polygon
            HashSet<LinkedVertex> convexVertices = new HashSet<LinkedVertex>();
            HashSet<LinkedVertex> reflectVertices = new HashSet<LinkedVertex>();

            foreach (LinkedVertex v in verticesLinked)
            {
                bool isConvex = IsVertexConvex(v);

                if (isConvex)
                {
                    convexVertices.Add(v);
                }
                else
                {
                    reflectVertices.Add(v);
                }
            }


            //Step 2. Find the ears
            List<LinkedVertex> earVertices = new List<LinkedVertex>();

            //An ear is always one or more of the convex vertices, so we only need to check those
            foreach (LinkedVertex convexVertex in convexVertices)
            {
                if (IsVertexEar(convexVertex, reflectVertices))
                {
                    earVertices.Add(convexVertex);
                }
            }


            //Debug
            //DisplayVertices(earVertices);



            //Step 3. Build the triangles
            int safety = 0;

            while (true)
            {
                if (earVertices.Count == 2)
                {
                    break;
                }


                //Pick an ear vertex and form a triangle
                LinkedVertex ear = earVertices[0];

                LinkedVertex v_prev = ear.prevLinkedVertex;
                LinkedVertex v_next = ear.nextLinkedVertex;

                Triangle2 t = new Triangle2(ear.pos, v_prev.pos, v_next.pos);

                triangulation.Add(t);


                //Reconfigure the data structure

                //Remove the ear from the list of all convex vertices
                convexVertices.Remove(ear);
                earVertices.Remove(ear);


                //Reconnect the vertices
                v_prev.nextLinkedVertex = v_next;
                v_next.prevLinkedVertex = v_prev;
                
                //If an adjacent vertex was convex, it will remain convex, so do nothing

                //If an adjacent vertex was an ear it may no longer be an ear
                if (earVertices.Contains(v_prev))
                {
                    if (!IsVertexEar(v_prev, reflectVertices))
                    {
                        earVertices.Remove(v_prev);
                    }
                }
                if (earVertices.Contains(v_next))
                {
                    if (!IsVertexEar(v_next, reflectVertices))
                    {
                        earVertices.Remove(v_next);
                    }
                }

                //If an adjacent vertex was reflect, it may now be convex and possible a new ear
                if (reflectVertices.Contains(v_prev))
                {
                    if (IsVertexConvex(v_prev))
                    {
                        reflectVertices.Remove(v_prev);
                        convexVertices.Add(v_prev);

                        if (IsVertexEar(v_prev, reflectVertices))
                        {
                            earVertices.Add(v_prev);
                        }
                    }
                }
                if (reflectVertices.Contains(v_next))
                {
                    if (IsVertexConvex(v_next))
                    {
                        reflectVertices.Remove(v_next);
                        convexVertices.Add(v_next);

                        if (IsVertexEar(v_next, reflectVertices))
                        {
                            earVertices.Add(v_next);
                        }
                    }
                }


                safety += 1;

                if (safety > 5000)
                {
                    Debug.Log("Ear Clipping is stuck in an infinite loop!");

                    break;
                }
            }


            return triangulation;
        }



        //Help method to check if a vertex is an ear
        private static bool IsVertexEar(LinkedVertex vertex, HashSet<LinkedVertex> reflectVertices)
        {
            //Consider the triangle
            MyVector2 p_prev = vertex.prevLinkedVertex.pos;
            MyVector2 p = vertex.pos;
            MyVector2 p_next = vertex.nextLinkedVertex.pos;

            Triangle2 t = new Triangle2(p_prev, p, p_next);

            //If any of the other vertices is within this triangle, then this vertex is not an ear
            //We only need to check the reflex vertices
            foreach (LinkedVertex otherLinkedVertex in reflectVertices)
            {
                MyVector2 otherVertex = otherLinkedVertex.pos;

                //Dont compare with any of the vertices the triangle consist of
                if (otherVertex.Equals(p_prev) || otherVertex.Equals(p) || otherVertex.Equals(p_next))
                {
                    continue;
                }

                //If a relect vertex intersects with the triangle, then this vertex is not an ear
                if (_Intersections.PointTriangle(t, otherVertex, includeBorder: true))
                {
                    return false;
                }
            }


            //No vertex is intersecting with the triangle, so this vertex must be an ear
            return true;
        }



        //Help method to check if a vertex is convex (if not its concave)
        private static bool IsVertexConvex(LinkedVertex v)
        {
            MyVector2 p_prev = v.prevLinkedVertex.pos;
            MyVector2 p = v.pos;
            MyVector2 p_next = v.nextLinkedVertex.pos;

            //The angle between these vectors
            MyVector2 p_to_p_prev = p_prev - p;
            MyVector2 p_to_p_next = p_next - p;

            //This will calculate the outside angle
            float angle = MathUtility.AngleFromToCCW(p_to_p_prev, p_to_p_next);

            //The interior angle is the opposite of the outside angle
            float interiorAngle = (Mathf.PI * 2f) - angle;

            //This means that a vertex on a straight line will be convex
            if (interiorAngle <= Mathf.PI)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        //Debug stuff
        private static void DisplayVertices(List<LinkedVertex> vertices)
        {
            foreach (LinkedVertex v in vertices)
            {
                Debug.DrawLine(v.pos.ToVector3(), Vector3.zero, Color.blue, 3f);
            }
        }
    }
}
