using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Standardized methods that are the same for all
    public static class HelpMethods
    {
        //Orient triangles so they have the correct orientation
        public static HashSet<Triangle2> OrientTrianglesClockwise(HashSet<Triangle2> triangles)
        {
            //Convert to list or we will no be able to update the orientation
            List<Triangle2> trianglesList = new List<Triangle2>(triangles);

            for (int i = 0; i < trianglesList.Count; i++)
            {
                Triangle2 t = trianglesList[i];

                if (!Geometry.IsTriangleOrientedClockwise(t.p1, t.p2, t.p3))
                {
                    t.ChangeOrientation();

                    trianglesList[i] = t;

                    //Debug.Log("Changed orientation");
                }
            }

            //Back to hashset
            triangles = new HashSet<Triangle2>(trianglesList);

            return triangles;
        }



        //Calculate the AABB of a set of points
        public static AABB GetAABB(List<MyVector2> points)
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            for (int i = 0; i < points.Count; i++)
            {
                MyVector2 p = points[i];

                if (p.x < minX)
                {
                    minX = p.x;
                }
                if (p.x > maxX)
                {
                    maxX = p.x;
                }

                if (p.y < minY)
                {
                    minY = p.y;
                }
                if (p.y > maxY)
                {
                    maxY = p.y;
                }
            }

            AABB box = new AABB(minX, maxX, minY, maxY);

            return box;
        }


        //
        // Dimension conversions
        //


        //Convert a list from 2d to 3d if we know it's the y coordinate that's missing
        //public static List<Vector3> ConvertListFrom2DTo3D(List<Vector2> list_2d)
        //{
        //    List<Vector3> list_3d = new List<Vector3>();

        //    foreach (Vector2 point in list_2d)
        //    {
        //        list_3d.Add(point.XYZ());
        //    }

        //    return list_3d;
        //}



        //Convert a list from 3d to 2d if we know it's the y coordinate that should be removed
        //public static List<Vector2> ConvertListFrom3DTo2D(List<Vector3> list_3d)
        //{
        //    List<Vector2> list_2d = new List<Vector2>();

        //    foreach (Vector3 point in list_3d)
        //    {
        //        list_2d.Add(point.XZ());
        //    }

        //    return list_2d;
        //}



        //Convert a hashset from 2d to 3d if we know it's the y coordinate that's missing
        //public static HashSet<Vector3> ConvertListFrom2DTo3D(HashSet<Vector2> list_2d)
        //{
        //    HashSet<Vector3> list_3d = new HashSet<Vector3>();

        //    foreach (Vector2 point in list_2d)
        //    {
        //        list_3d.Add(point.XYZ());
        //    }

        //    return list_3d;
        //}



        //Convert a hashset from 3d to 2d if we know it's the y coordinate that should be removed
        //public static HashSet<Vector2> ConvertListFrom3DTo2D(HashSet<Vector3> list_3d)
        //{
        //    HashSet<Vector2> list_2d = new HashSet<Vector2>();

        //    foreach (Vector3 point in list_3d)
        //    {
        //        list_2d.Add(point.XZ());
        //    }

        //    return list_2d;
        //}
    }
}
