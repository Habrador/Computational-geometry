using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class ExtrudeMeshAlongCurve
    {
        //Generate a mesh
        public static Mesh GenerateMesh(List<InterpolationTransform> transforms, MeshProfile profile, float profileScale)
        {
            if (profile == null)
            {
                Debug.Log("You need to assign a mesh profile");

                return null;
            }

            if (transforms == null || transforms.Count <= 1)
            {
                Debug.Log("You need more transforms");

                return null;
            }


            //Test that the profile is correct
            //InterpolationTransform testTrans = transforms[1];

            //DisplayMeshProfile(profile, testTrans, profileScale);

            //Vertices
            List<Vector3> vertices = new List<Vector3>();

            //Normals
            List<Vector3> normals = new List<Vector3>();

            for (int step = 0; step < transforms.Count; step++)
            {
                InterpolationTransform thisTransform = transforms[step];

                for (int i = 0; i < profile.vertices.Length; i++)
                {
                    MyVector2 localPos2d = profile.vertices[i].point;

                    MyVector3 localPos = new MyVector3(localPos2d.x, localPos2d.y, 0f);

                    MyVector3 pos = thisTransform.LocalToWorld_Pos(localPos * profileScale);

                    vertices.Add(pos.ToVector3());


                    //Normals
                    MyVector2 localNormal2d = profile.vertices[i].normal;

                    MyVector3 localNormal = new MyVector3(localNormal2d.x, localNormal2d.y, 0f);

                    MyVector3 normal = thisTransform.LocalToWorld_Dir(localNormal);

                    normals.Add(normal.ToVector3());
                }
            }

            //Triangles
            List<int> triangles = new List<int>();

            //We connect the first transform with the next transform, ignoring the last transform because it doesnt have a next
            for (int step = 0; step < transforms.Count - 1; step++)
            {
                //The index where this profile starts in the list of all vertices in the entire mesh
                int profileIndexThis = step * profile.vertices.Length;
                //The index where the next profile starts
                int profileIndexNext = (step + 1) * profile.vertices.Length;

                //Each line has 2 points 
                for (int line = 0; line < profile.lineIndices.Length; line++)
                {
                    int lineIndexA = profile.lineIndices[line].x;
                    int lineIndexB = profile.lineIndices[line].y;

                    //Now we can identify the vertex we need in the list of all vertices in the entire mesh
                    //The current profile
                    int thisA = profileIndexThis + lineIndexA;
                    int thisB = profileIndexThis + lineIndexB;
                    //The next profile
                    int nextA = profileIndexNext + lineIndexA;
                    int nextB = profileIndexNext + lineIndexB;

                    //Build two triangles
                    triangles.Add(thisA);
                    triangles.Add(nextA);
                    triangles.Add(nextB);

                    triangles.Add(thisB);
                    triangles.Add(thisA);
                    triangles.Add(nextB);
                }
            }

            Mesh mesh = new Mesh();

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.normals = normals.ToArray();

            //mesh.RecalculateNormals();

            return mesh;
        }

    }
}
