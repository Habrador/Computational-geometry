using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Habrador_Computational_Geometry
{
    //Triangulate a concave hull (with holes) by using an algorithm called Ear Clipping
    //Based on: 
    //- "Triangulation by Ear Clipping" by David Eberly
    //- "Ear-Clipping Based Algorithms of Generating High-quality Polygon Triangulation" by people
    //Can also triangulate convex hulls but there are faster algorithms for that 
    //This alorithm is called ear clipping and it's O(n*n) 
    //Another common algorithm is dividing it into trapezoids and it's O(n log n)
    //One can maybe do it in O(n) time but no such version is known
    public static class _EarClipping
    {
        //The points on the hull (vertices) should be ordered counter-clockwise (and no doubles)
        //The holes should be ordered clockwise (and no doubles)
        //Optimize triangles means that we will get a better-looking triangulation, which resembles a constrained Delaunay triangulation
        public static HashSet<Triangle2> Triangulate(List<MyVector2> vertices, List<List<MyVector2>> allHoleVertices = null, bool optimizeTriangles = true)
        {
            //Validate the data
            if (vertices == null || vertices.Count <= 2)
            {
                Debug.LogWarning("Can't triangulate with Ear Clipping because too few vertices on the hull");

                return null;
            }

           


            //Step -1. Merge the holes with the points on the hull into one big polygon with invisible edges between the holes and the hull
            if (allHoleVertices != null && allHoleVertices.Count > 0)
            {
                vertices = EarClippingHoleMethods.MergeHolesWithHull(vertices, allHoleVertices);
            }


            //TestAlgorithmsHelpMethods.DebugDrawCircle(vertices[29].ToVector3(1f), 0.3f, Color.red);


            //Step 0. Create a linked list connecting all vertices with each other which will make the calculations easier and faster
            List<LinkedVertex> verticesLinked = new List<LinkedVertex>();

            for (int i = 0; i < vertices.Count; i++)
            {
                LinkedVertex v = new LinkedVertex(vertices[i]);

                verticesLinked.Add(v);
            }

            //Link them to each other
            for (int i = 0; i < verticesLinked.Count; i++)
            {
                LinkedVertex v = verticesLinked[i];

                v.prevLinkedVertex = verticesLinked[MathUtility.ClampListIndex(i - 1, verticesLinked.Count)];
                v.nextLinkedVertex = verticesLinked[MathUtility.ClampListIndex(i + 1, verticesLinked.Count)];
            }

            //Debug.Log("Number of vertices: " + CountLinkedVertices(verticesLinked[0]));
            


            //Step 1. Find:
            //- Convex vertices (interior angle smaller than 180 degrees)
            //- Reflect vertices (interior angle greater than 180 degrees) so should maybe be called concave vertices?
            //Interior angle is the angle between two vectors inside the polygon if we move around the polygon counter-clockwise
            //If they are neither we assume they are reflect (or we will end up with odd triangulations)
            HashSet<LinkedVertex> convexVerts = new HashSet<LinkedVertex>();
            HashSet<LinkedVertex> reflectVerts = new HashSet<LinkedVertex>();

            foreach (LinkedVertex v in verticesLinked)
            {            
                bool isConvex = IsVertexConvex(v);

                if (isConvex)
                {
                    convexVerts.Add(v);
                }
                else
                {
                    reflectVerts.Add(v);
                }
            }



            //Step 2. Find the initial ears
            HashSet<LinkedVertex> earVerts = new HashSet<LinkedVertex>();

            //An ear is always a convex vertex
            foreach (LinkedVertex v in convexVerts)
            {
                //And we only need to test the reflect vertices
                if (IsVertexEar(v, reflectVerts))
                {
                    earVerts.Add(v);
                }
            }


            //Debug
            //DisplayVertices(earVertices);



            //Step 3. Build the triangles
            HashSet<Triangle2> triangulation = new HashSet<Triangle2>();

            //We know how many triangles we will get (number of vertices - 2) which is true for all simple polygons
            //This can be used to stop the algorithm
            int maxTriangles = verticesLinked.Count - 2;

            //Because we use a while loop, having an extra safety is always good so we dont get stuck in infinite loop
            int safety = 0;

            while (true)
            {
                //Pick an ear vertex and form a triangle
                LinkedVertex ear = GetEarVertex(earVerts, optimizeTriangles);

                if (ear == null)
                {
                    Debug.Log("Cant find ear");

                    break;
                }

                LinkedVertex v_prev = ear.prevLinkedVertex;
                LinkedVertex v_next = ear.nextLinkedVertex;

                Triangle2 t = new Triangle2(ear.pos, v_prev.pos, v_next.pos);

                //Try to flip this triangle according to Delaunay triangulation
                if (optimizeTriangles)
                {
                    OptimizeTriangle(t, triangulation);    
                }
                else
                {
                    triangulation.Add(t);
                }

                

                //Check if we have found all triangles
                //This should also prevent us from getting stuck in an infinite loop
                if (triangulation.Count >= maxTriangles)
                {
                    break;
                }


                //If we havent found all triangles we have to reconfigure the data structure

                //Remove the ear we used to build a triangle
                convexVerts.Remove(ear);
                earVerts.Remove(ear);

                //Reconnect the vertices because one vertex has now been removed
                v_prev.nextLinkedVertex = v_next;
                v_next.prevLinkedVertex = v_prev;

                //Reconfigure the adjacent vertices
                ReconfigureAdjacentVertex(v_prev, convexVerts, reflectVerts, earVerts);
                ReconfigureAdjacentVertex(v_next, convexVerts, reflectVerts, earVerts);


                //if (safety > 4)
                //{
                //    Debug.Log(earVerts.Count);

                //    Debug.DrawLine(v_next.pos.ToVector3(), Vector3.zero, Color.blue, 3f);

                //    //Debug.Log(IsVertexEar(v_next, reflectVerts));

                //    Debug.Log(earVerts.Contains(v_next));

                //    break;
                //}



                safety += 1;

                if (safety > 50000)
                {
                    Debug.Log("Ear Clipping is stuck in an infinite loop!");

                    break;
                }
            }



            //Step 4. Improve triangulation
            //Some triangles may be too sharp, and if you want a nice looking triangle, you should try to swap edges
            //according to Delaunay triangulation
            //A report suggests that should be done while createing the triangulation
            //But maybe it's easier to do it afterwards with some standardized constrained Delaunay triangulation?
            //But that would also be stupid because then we could have used the constrained Delaunay from the beginning!


            return triangulation;
        }



        //Optimize a new triangle according to Delaunay triangulation
        //TODO: This process would have been easier if we had used the HalfEdge data structure
        private static void OptimizeTriangle(Triangle2 t, HashSet<Triangle2> triangulation)
        {
            bool hasOppositeEdge;

            Triangle2 tOpposite;

            Edge2 edgeToSwap;

            FindEdgeInTriangulation(t, triangulation, out hasOppositeEdge, out tOpposite, out edgeToSwap);

            //If it has no opposite edge we just add triangle to the triangulation because it can't be improved
            if (!hasOppositeEdge)
            {
                triangulation.Add(t);

                return;
            }

            //Debug.Log("Has opposite edge");

            //Step 3. Check if we should swap this edge according to Delaunay triangulation rules
            //a, b, c belongs to the triangle and d is the point on the other triangle
            //a-c is the edge, which is important so we can flip it, by making the edge b-d
            MyVector2 a = edgeToSwap.p2;
            MyVector2 c = edgeToSwap.p1;
            MyVector2 b = t.GetVertexWhichIsNotPartOfEdge(edgeToSwap);
            MyVector2 d = tOpposite.GetVertexWhichIsNotPartOfEdge(edgeToSwap);

            bool shouldFlipEdge = DelaunayMethods.ShouldFlipEdge(a, b, c, d);
            //bool shouldFlipEdge = DelaunayMethods.ShouldFlipEdgeStable(a, b, c, d);

            if (shouldFlipEdge)
            {
                //First remove the old triangle
                triangulation.Remove(tOpposite);

                //Build two new triangles
                Triangle2 t1 = new Triangle2(a, b, d);
                Triangle2 t2 = new Triangle2(b, c, d);

                triangulation.Add(t1);
                triangulation.Add(t2);

                //Debug.Log("Flipped edge");
            }
            else
            {
                triangulation.Add(t);
            }
        }



        //Find an edge in a triangulation and return the triangle the edge is attached to
        private static void FindEdgeInTriangulation(Triangle2 tNew, HashSet<Triangle2> triangulation, out bool hasOppositeEdge, out Triangle2 tOpposite, out Edge2 edgeToSwap)
        {
            //Step 1. Find the triangle's biggest interior angle and its opposite edge
            float angleP1 = CalculateInteriorAngle(tNew.p3, tNew.p1, tNew.p2);
            float angleP2 = CalculateInteriorAngle(tNew.p1, tNew.p2, tNew.p3);
            float angleP3 = Mathf.PI - (angleP1 + angleP2);

            MyVector2 vertexWithBiggestInteriorAngle = tNew.p1;

            if (angleP2 > angleP1)
            {
                vertexWithBiggestInteriorAngle = tNew.p2;

                if (angleP3 > angleP2)
                {
                    vertexWithBiggestInteriorAngle = tNew.p3;
                }
            }
            else if (angleP3 > angleP1)
            {
                vertexWithBiggestInteriorAngle = tNew.p3;
            }

            edgeToSwap = tNew.FindOppositeEdgeToVertex(vertexWithBiggestInteriorAngle);


            //Step 2. Check if this edge exists among the already generated triangles, which means we have a neighbor
            hasOppositeEdge = false;

            tOpposite = new Triangle2();

            foreach (Triangle2 tTest in triangulation)
            {
                if (tTest.IsEdgePartOfTriangle(edgeToSwap))
                {
                    hasOppositeEdge = true;

                    tOpposite = tTest;

                    break;
                }
            }
        }



        //Reconfigure an adjacent vertex that was used to build a triangle
        private static void ReconfigureAdjacentVertex(LinkedVertex v, HashSet<LinkedVertex> convexVerts, HashSet<LinkedVertex> reflectVerts, HashSet<LinkedVertex> earVerts)
        {
            //If the adjacent vertex was reflect...
            if (reflectVerts.Contains(v))
            {
                //it may now be convex...
                if (IsVertexConvex(v))
                {
                    reflectVerts.Remove(v);
                    convexVerts.Add(v);

                    //and possible a new ear
                    if (IsVertexEar(v, reflectVerts))
                    {
                        earVerts.Add(v);
                    }
                }
            }
            //If an adjacent vertex was convex, it will always still be convex
            else
            {
                bool isEar = IsVertexEar(v, reflectVerts);

                //This vertex was an ear but is no longer an ear
                if (earVerts.Contains(v) && !isEar)
                {
                    earVerts.Remove(v);
                }
                //This vertex wasn't an ear but has now become an ear
                else if (isEar)
                {
                    earVerts.Add(v);
                }
            }
        }



        //Get the best ear vertex
        private static LinkedVertex GetEarVertex(HashSet<LinkedVertex> earVertices, bool optimizeTriangles)
        {
            LinkedVertex bestEarVertex = null;

            //To get better looking triangles we should always get the ear with the smallest interior angle
            if (optimizeTriangles)
            {
                float smallestInteriorAngle = Mathf.Infinity;

                foreach (LinkedVertex v in earVertices)
                {
                    float interiorAngle = CalculateInteriorAngle(v);

                    if (interiorAngle < smallestInteriorAngle)
                    {
                        bestEarVertex = v;

                        smallestInteriorAngle = interiorAngle;
                    }
                }
            }
            //Just get first best ear vertex
            else
            {
                foreach (LinkedVertex v in earVertices)
                {
                    bestEarVertex = v;

                    break;
                }
            }


            return bestEarVertex;
        }



        //Is a vertex an ear?
        private static bool IsVertexEar(LinkedVertex vertex, HashSet<LinkedVertex> reflectVertices)
        {
            //Consider the triangle
            MyVector2 p_prev = vertex.prevLinkedVertex.pos;
            MyVector2 p = vertex.pos;
            MyVector2 p_next = vertex.nextLinkedVertex.pos;

            Triangle2 t = new Triangle2(p_prev, p, p_next);

            //If any of the other vertices is within this triangle, then this vertex is not an ear
            //We only need to check the reflect vertices
            foreach (LinkedVertex otherVertex in reflectVertices)
            {
                MyVector2 test_p = otherVertex.pos;

                //Dont compare with any of the vertices the triangle consist of
                if (test_p.Equals(p_prev) || test_p.Equals(p) || test_p.Equals(p_next))
                {
                    continue;
                }

                //If a relect vertex intersects with the triangle, then this vertex is not an ear
                if (_Intersections.PointTriangle(t, test_p, includeBorder: true))
                {
                    return false;
                }
            }


            //No vertex is intersecting with the triangle, so this vertex must be an ear
            return true;
        }



        //Is a vertex convex? (if not its concave or neither if its a straight line)
        private static bool IsVertexConvex(LinkedVertex v)
        {
            MyVector2 p_prev = v.prevLinkedVertex.pos;
            MyVector2 p = v.pos;
            MyVector2 p_next = v.nextLinkedVertex.pos;

            return IsVertexConvex(p_prev, p, p_next);
        }

        public static bool IsVertexConvex(MyVector2 p_prev, MyVector2 p, MyVector2 p_next)
        {
            float interiorAngle = CalculateInteriorAngle(p_prev, p, p_next);

            /*
            //Colinear point (pi = 180 degrees)
            if (Mathf.Abs(interiorAngle - Mathf.PI) <= MathUtility.EPSILON)
            {
                //Is it concave or convex? God knows!
                //According to a paper, the vertex is convex if the interior angle is less than 180 degrees
                //One can remove them if they cause trouble (the triangulation will still fill the area)
                //And maybe add them back at the end by splitting triangles
                return false;
            }
            */
            //Convex
            if (interiorAngle < Mathf.PI)
            {
                return true;
            }
            //Concave
            else
            {
                return false;
            }
        }



        //Get interior angle (the angle within the polygon) of a vertex
        private static float CalculateInteriorAngle(LinkedVertex v)
        {
            MyVector2 p_prev = v.prevLinkedVertex.pos;
            MyVector2 p = v.pos;
            MyVector2 p_next = v.nextLinkedVertex.pos;

            return CalculateInteriorAngle(p_prev, p, p_next);
        }

        private static float CalculateInteriorAngle(MyVector2 p_prev, MyVector2 p, MyVector2 p_next)
        {
            //Two vectors going from the vertex
            //You (most likely) don't need to normalize these
            MyVector2 p_to_p_prev = p_prev - p;
            MyVector2 p_to_p_next = p_next - p;

            //The angle between the two vectors [rad]
            //This will calculate the outside angle
            float angle = MathUtility.AngleFromToCCW(p_to_p_prev, p_to_p_next);

            //The interior angle is the opposite of the outside angle
            float interiorAngle = (Mathf.PI * 2f) - angle;

            return interiorAngle;
        }



        //Count vertices that are linked to each other in a looping way
        private static int CountLinkedVertices(LinkedVertex startVertex)
        {
            int counter = 1;

            LinkedVertex currentVertex = startVertex;

            while (true)
            {
                currentVertex = currentVertex.nextLinkedVertex;
            
                if (currentVertex == startVertex)
                {
                    break;
                }
            
                counter += 1;
            
                if (counter > 50000)
                {
                    Debug.Log("Stuck in infinite loop!");

                    break;
                }
            }

            return counter;
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
