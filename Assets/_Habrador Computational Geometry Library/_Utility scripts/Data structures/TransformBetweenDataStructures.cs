using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Transform one representation to another
    public static class TransformBetweenDataStructures
    {
        //
        // From triangle to half-edge
        //
        public static HalfEdgeData TransformFromTriangleToHalfEdge(HashSet<Triangle> triangles, HalfEdgeData data)
        {
            //Step1. Make sure the triangles have the same orientation, which is clockwise
            HelpMethods.OrientTrianglesClockwise(triangles);


            //Step 2. Init the data structure we need
            //HalfEdgeData data = new HalfEdgeData();


            //Step3. Fill the data structure
            foreach (Triangle t in triangles)
            {
                HalfEdgeVertex v1 = new HalfEdgeVertex(t.p1);
                HalfEdgeVertex v2 = new HalfEdgeVertex(t.p2);
                HalfEdgeVertex v3 = new HalfEdgeVertex(t.p3);

                //The vertices the edge points to
                HalfEdge he1 = new HalfEdge(v1);
                HalfEdge he2 = new HalfEdge(v2);
                HalfEdge he3 = new HalfEdge(v3);

                he1.nextEdge = he2;
                he2.nextEdge = he3;
                he3.nextEdge = he1;

                he1.prevEdge = he3;
                he2.prevEdge = he1;
                he3.prevEdge = he2;

                //The vertex needs to know of an edge going from it
                v1.edge = he2;
                v2.edge = he3;
                v3.edge = he1;

                //The face the half-edge is connected to
                HalfEdgeFace face = new HalfEdgeFace(he1);

                //Each edge needs to know of the face connected to this edge
                he1.face = face;
                he2.face = face;
                he3.face = face;


                //Add everything to the lists
                data.edges.Add(he1);
                data.edges.Add(he2);
                data.edges.Add(he3);

                data.faces.Add(face);

                data.vertices.Add(v1);
                data.vertices.Add(v2);
                data.vertices.Add(v3);
            }


            //Step 4. Find the half-edges going in the opposite direction of each edge we have 
            //Is there a faster way to do this because this is the bottleneck?
            foreach (HalfEdge e in data.edges)
            {
                HalfEdgeVertex goingToVertex = e.v;
                HalfEdgeVertex goingFromVertex = e.prevEdge.v;

                foreach (HalfEdge eOther in data.edges)
                {
                    //Dont compare with itself
                    if (e == eOther)
                    {
                        continue;
                    }

                    //Is this edge going between the vertices in the opposite direction
                    //== returns true if two vectors are approximately equal, so dont worry about floating point precision
                    if (goingFromVertex.position == eOther.v.position && goingToVertex.position == eOther.prevEdge.v.position)
                    {
                        e.oppositeEdge = eOther;

                        break;
                    }
                }
            }


            return data;
        }



        //
        // From half-edge to triangle if we know the half-edge consists of triangles
        //
        public static HashSet<Triangle> TransformFromHalfEdgeToTriangle(HalfEdgeData data)
        {
            if (data == null)
            {
                return null;
            }
        
            HashSet<Triangle> triangles = new HashSet<Triangle>();

            foreach (HalfEdgeFace face in data.faces)
            {
                Vector3 p1 = face.edge.v.position;
                Vector3 p2 = face.edge.nextEdge.v.position;
                Vector3 p3 = face.edge.nextEdge.nextEdge.v.position;

                Triangle t = new Triangle(p1, p2, p3);

                triangles.Add(t);
            }

            return triangles;
        }



        //
        // From mesh to triangle
        //
        public static HashSet<Triangle> ConvertFromMeshToTriangle(Mesh mesh)
        {
            HashSet<Triangle> triangles = new HashSet<Triangle>();

            Vector3[] meshVertices = mesh.vertices;

            int[] meshTriangles = mesh.triangles;

            for (int i = 0; i < meshTriangles.Length; i += 3)
            {
                Vector3 v1 = meshVertices[meshTriangles[i + 0]];
                Vector3 v2 = meshVertices[meshTriangles[i + 1]];
                Vector3 v3 = meshVertices[meshTriangles[i + 2]];

                Triangle t = new Triangle(v1, v2, v3);

                triangles.Add(t);
            }

            return triangles;
        }



        //
        // From triangle to mesh
        //

        //Version 1. Check that each vertex exists only once in the final mesh
        public static Mesh ConvertFromTriangleToMeshCompressed(HashSet<Triangle> triangles, bool checkTriangleOrientation)
        {
            if (triangles == null)
            {
                return null;
            }
        
            //Step1. Make sure the triangles have the same orientation, which is clockwise
            if (checkTriangleOrientation)
            {
                HelpMethods.OrientTrianglesClockwise(triangles);
            }
            

            //Step 2. Create the list with unique vertices
            //A hashset will make it fast to check if a vertex already exists in the collection
            HashSet<Vector3> uniqueVertices = new HashSet<Vector3>();

            foreach (Triangle t in triangles)
            {
                Vector3 v1 = t.p1;
                Vector3 v2 = t.p2;
                Vector3 v3 = t.p3;

                if (!uniqueVertices.Contains(v1))
                {
                    uniqueVertices.Add(v1);
                }
                if (!uniqueVertices.Contains(v2))
                {
                    uniqueVertices.Add(v2);
                }
                if (!uniqueVertices.Contains(v3))
                {
                    uniqueVertices.Add(v3);
                }
            }

            //Create the list with all vertices
            List<Vector3> meshVertices = new List<Vector3>(uniqueVertices);


            //Step3. Create the list with all triangles by using the unique vertices
            List<int> meshTriangles = new List<int>();

            //Use a dictionay to quickly find which positon in the list a Vector3 has
            Dictionary<Vector3, int> vector3Positons = new Dictionary<Vector3, int>();

            for (int i = 0; i < meshVertices.Count; i++)
            {
                vector3Positons.Add(meshVertices[i], i);
            }

            foreach (Triangle t in triangles)
            {
                Vector3 v1 = t.p1;
                Vector3 v2 = t.p2;
                Vector3 v3 = t.p3;

                meshTriangles.Add(vector3Positons[v1]);
                meshTriangles.Add(vector3Positons[v2]);
                meshTriangles.Add(vector3Positons[v3]);
            }


            //Step4. Create the final mesh
            Mesh mesh = new Mesh();

            mesh.vertices = meshVertices.ToArray();
            mesh.triangles = meshTriangles.ToArray();

            return mesh;
        }



        //Version 2. Don't check for duplicate vertices, which can be good if we want a low-poly style mesh
        public static Mesh ConvertFromTriangleToMesh(HashSet<Triangle> triangles, bool checkTriangleOrientation)
        {
            //Step1. Make sure the triangles have the same orientation, which is clockwise
            if (checkTriangleOrientation)
            {
                HelpMethods.OrientTrianglesClockwise(triangles);
            }

            //Create the list with all vertices and triangles
            List<Vector3> meshVertices = new List<Vector3>();

            //Create the list with all triangles
            List<int> meshTriangles = new List<int>();

            int arrayPos = 0;

            foreach (Triangle t in triangles)
            {
                Vector3 v1 = t.p1;
                Vector3 v2 = t.p2;
                Vector3 v3 = t.p3;

                meshVertices.Add(v1);
                meshVertices.Add(v2);
                meshVertices.Add(v3);

                meshTriangles.Add(arrayPos + 0);
                meshTriangles.Add(arrayPos + 1);
                meshTriangles.Add(arrayPos + 2);

                arrayPos += 3;
            }

            //Create the final mesh
            Mesh mesh = new Mesh();

            mesh.vertices = meshVertices.ToArray();
            mesh.triangles = meshTriangles.ToArray();

            return mesh;
        }
    }
}
