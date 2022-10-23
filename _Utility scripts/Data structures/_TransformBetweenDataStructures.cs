using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Transform one representation to another
    public static class _TransformBetweenDataStructures
    {
        //
        // Triangle to half-edge
        //
        public static HalfEdgeData2 Triangle2ToHalfEdge2(HashSet<Triangle2> triangles, HalfEdgeData2 data)
        {
            //Make sure the triangles have the same orientation, which is clockwise
            triangles = HelpMethods.OrientTrianglesClockwise(triangles);


            //Fill the data structure
            foreach (Triangle2 t in triangles)
            {
                HalfEdgeVertex2 v1 = new HalfEdgeVertex2(t.p1);
                HalfEdgeVertex2 v2 = new HalfEdgeVertex2(t.p2);
                HalfEdgeVertex2 v3 = new HalfEdgeVertex2(t.p3);

                //The vertices the edge points to
                HalfEdge2 he1 = new HalfEdge2(v1);
                HalfEdge2 he2 = new HalfEdge2(v2);
                HalfEdge2 he3 = new HalfEdge2(v3);

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
                HalfEdgeFace2 face = new HalfEdgeFace2(he1);

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
            foreach (HalfEdge2 e in data.edges)
            {
                HalfEdgeVertex2 goingToVertex = e.v;
                HalfEdgeVertex2 goingFromVertex = e.prevEdge.v;

                foreach (HalfEdge2 eOther in data.edges)
                {
                    //Dont compare with itself
                    if (e == eOther)
                    {
                        continue;
                    }

                    //Is this edge going between the vertices in the opposite direction
                    if (goingFromVertex.position.Equals(eOther.v.position) && goingToVertex.position.Equals(eOther.prevEdge.v.position))
                    {
                        e.oppositeEdge = eOther;

                        break;
                    }
                }
            }


            return data;
        }



        //
        // Half-edge to triangle if we know the half-edge consists of triangles
        //
        public static HashSet<Triangle2> HalfEdge2ToTriangle2(HalfEdgeData2 data)
        {
            if (data == null)
            {
                return null;
            }
        
            HashSet<Triangle2> triangles = new HashSet<Triangle2>();

            foreach (HalfEdgeFace2 face in data.faces)
            {
                MyVector2 p1 = face.edge.v.position;
                MyVector2 p2 = face.edge.nextEdge.v.position;
                MyVector2 p3 = face.edge.nextEdge.nextEdge.v.position;

                Triangle2 t = new Triangle2(p1, p2, p3);

                triangles.Add(t);
            }

            return triangles;
        }



        //
        // Unity mesh to triangle
        //
        //The vertices and triangles are the same as in Unitys built-in Mesh, but in 2d space
        public static HashSet<Triangle2> MeshToTriangle2(Vector2[] meshVertices, int[] meshTriangles)
        {
            HashSet<Triangle2> triangles = new HashSet<Triangle2>();

            for (int i = 0; i < meshTriangles.Length; i += 3)
            {
                MyVector2 v1 = new MyVector2(meshVertices[meshTriangles[i + 0]].x, meshVertices[meshTriangles[i + 0]].y);
                MyVector2 v2 = new MyVector2(meshVertices[meshTriangles[i + 1]].x, meshVertices[meshTriangles[i + 1]].y);
                MyVector2 v3 = new MyVector2(meshVertices[meshTriangles[i + 2]].x, meshVertices[meshTriangles[i + 2]].y);

                Triangle2 t = new Triangle2(v1, v2, v3);

                triangles.Add(t);
            }

            return triangles;
        }



        //
        // Triangle to Unity mesh
        //

        //Version 1. Check that each vertex exists only once in the final mesh
        //Make sure the triangles have the correct orientation
        public static Mesh Triangle3ToCompressedMesh(HashSet<Triangle3> triangles)
        {
            if (triangles == null)
            {
                return null;
            }
                    

            //Step 2. Create the list with unique vertices
            //A hashset will make it fast to check if a vertex already exists in the collection
            HashSet<MyVector3> uniqueVertices = new HashSet<MyVector3>();

            foreach (Triangle3 t in triangles)
            {
                MyVector3 v1 = t.p1;
                MyVector3 v2 = t.p2;
                MyVector3 v3 = t.p3;

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
            List<MyVector3> meshVertices = new List<MyVector3>(uniqueVertices);


            //Step3. Create the list with all triangles by using the unique vertices
            List<int> meshTriangles = new List<int>();

            //Use a dictionay to quickly find which positon in the list a Vector3 has
            Dictionary<MyVector3, int> vector2Positons = new Dictionary<MyVector3, int>();

            for (int i = 0; i < meshVertices.Count; i++)
            {
                vector2Positons.Add(meshVertices[i], i);
            }

            foreach (Triangle3 t in triangles)
            {
                MyVector3 v1 = t.p1;
                MyVector3 v2 = t.p2;
                MyVector3 v3 = t.p3;

                meshTriangles.Add(vector2Positons[v1]);
                meshTriangles.Add(vector2Positons[v2]);
                meshTriangles.Add(vector2Positons[v3]);
            }


            //Step4. Create the final mesh
            Mesh mesh = new Mesh();

            //From MyVector3 to Vector3
            Vector3[] meshVerticesArray = new Vector3[meshVertices.Count];

            for (int i = 0; i < meshVerticesArray.Length; i++)
            {
                MyVector3 v = meshVertices[i];

                meshVerticesArray[i] = new Vector3(v.x, v.y, v.z);
            }

            mesh.vertices = meshVerticesArray;
            mesh.triangles = meshTriangles.ToArray();

            //Should maybe recalculate bounds and normals, maybe better to do that outside this method???
            //mesh.RecalculateBounds();
            //mesh.RecalculateNormals();

            return mesh;
        }



        //Version 2. Don't check for duplicate vertices, which can be good if we want a low-poly style mesh
        //Make sure the triangles have the correct orientation
        public static Mesh Triangle3ToMesh(HashSet<Triangle3> triangles)
        {
            //Create the list with all vertices and triangles
            List<MyVector3> meshVertices = new List<MyVector3>();

            //Create the list with all triangles
            List<int> meshTriangles = new List<int>();

            int arrayPos = 0;

            foreach (Triangle3 t in triangles)
            {
                MyVector3 v1 = t.p1;
                MyVector3 v2 = t.p2;
                MyVector3 v3 = t.p3;

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

            //From MyVector3 to Vector3
            Vector3[] meshVerticesArray = new Vector3[meshVertices.Count];

            for (int i = 0; i < meshVerticesArray.Length; i++)
            {
                MyVector3 v = meshVertices[i];

                meshVerticesArray[i] = new Vector3(v.x, v.y, v.z);
            }

            mesh.vertices = meshVerticesArray;
            mesh.triangles = meshTriangles.ToArray();

            return mesh;
        }



        //
        // From Triangle2 to Unity mesh
        //

        //meshHeight is the y coordinate in 3d space
        public static Mesh Triangles2ToMesh(HashSet<Triangle2> triangles, bool useCompressedMesh, float meshHeight = 0f)
        {
            //2d to 3d
            HashSet<Triangle3> triangles_3d = new HashSet<Triangle3>();

            foreach (Triangle2 t in triangles)
            {
                triangles_3d.Add(new Triangle3(t.p1.ToMyVector3_Yis3D(meshHeight), t.p2.ToMyVector3_Yis3D(meshHeight), t.p3.ToMyVector3_Yis3D(meshHeight)));
            }

            //To mesh
            if (useCompressedMesh)
            {
                Mesh mesh = _TransformBetweenDataStructures.Triangle3ToCompressedMesh(triangles_3d);

                return mesh;
            }
            else
            {
                Mesh mesh = _TransformBetweenDataStructures.Triangle3ToMesh(triangles_3d);

                return mesh;
            }
        }
    }
}
