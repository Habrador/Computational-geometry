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
            HashSet<Triangle2> triangles = new HashSet<Triangle2>();

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
            //The reflect vertices (interior angle greater than 180 degrees)
            //Interior angle is the angle between two vectors inside the polygon
            List<LinkedVertex> convexVertices = new List<LinkedVertex>();
            List<LinkedVertex> reflectVertices = new List<LinkedVertex>();

            for (int i = 0; i < vertices.Count; i++)
            {
                LinkedVertex p_prev = verticesLinked[MathUtility.ClampListIndex(i - 1, vertices.Count)];
                LinkedVertex p = verticesLinked[i];
                LinkedVertex p_next = verticesLinked[MathUtility.ClampListIndex(i + 1, vertices.Count)];

                //The angle between these vectors
                MyVector2 p_to_p_prev = p_prev.pos - p.pos;
                MyVector2 p_to_p_next = p_next.pos - p.pos;

                float angle = MathUtility.AngleFromToCCW(p_to_p_prev, p_to_p_next);

                //The interior angle is the opposite angle
                float interiorAngle = (Mathf.PI * 2f) - angle;

                if (interiorAngle < Mathf.PI)
                {
                    convexVertices.Add(p);
                }
                else
                {
                    reflectVertices.Add(p);
                }
            }


            //Step 2. Find the ears
            List<LinkedVertex> earVertices = new List<LinkedVertex>();

            //An ear is always one or more of the convex vertices, so we only need to check those
            for (int i = 0; i < convexVertices.Count; i++)
            {
                LinkedVertex thisVertex = convexVertices[i];

                //Consider the triangle
                MyVector2 p_prev = thisVertex.prevLinkedVertex.pos;
                MyVector2 p = thisVertex.pos;
                MyVector2 p_next = thisVertex.nextLinkedVertex.pos;

                Triangle2 t = new Triangle2(p_prev, p, p_next);

                //If any of the other vertices is within this triangle, then this vertex is not an ear
                bool isOtherPointIsIntersectingWithTriangle = false;

                foreach (MyVector2 otherVertex in vertices)
                {
                    //Dont compare with any of the vertices the triangle consist of
                    if (otherVertex.Equals(p_prev) || otherVertex.Equals(p) || otherVertex.Equals(p_next))
                    {
                        continue;
                    }

                    if (_Intersections.PointTriangle(t, otherVertex, includeBorder: true))
                    {
                        isOtherPointIsIntersectingWithTriangle = true;
                        break;
                    }
                }

                if (!isOtherPointIsIntersectingWithTriangle)
                {
                    earVertices.Add(thisVertex);
                }

            }


            //Debug
            //DisplayVertices(earVertices);


            return triangles;
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
