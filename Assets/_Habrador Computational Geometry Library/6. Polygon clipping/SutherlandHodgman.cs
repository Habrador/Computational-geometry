using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Calculate the intersection of two polygons
    //https://en.wikipedia.org/wiki/Sutherland%E2%80%93Hodgman_algorithm
    ///Requires that the clipping polygon (the polygon we want to remove from the other polygon) is convex
    public static class SutherlandHodgman
    {
        //Sometimes its more efficient to calculate the planes once before we call the method
        //if we want to cut several polygons with the same planes
        public static List<MyVector2> ClipPolygon(List<MyVector2> poly, List<MyVector2> clipPoly)
        {
            //Calculate the clipping planes
            List<Plane2> clippingPlanes = GetClippingPlanes(clipPoly);

            List<MyVector2> vertices = ClipPolygon(poly, clippingPlanes);

            return vertices;
        }



        //Assumes the polygons are oriented counter clockwise
        //poly is the polygon we want to cut
        //Assumes the polygon we want to remove from the other polygon is convex, so clipPolygon has to be convex
        //We will end up with the intersection of the polygons
        public static List<MyVector2> ClipPolygon(List<MyVector2> poly, List<Plane2> clippingPlanes)
        {
            //Clone the vertices because we will remove vertices from this list
            List<MyVector2> vertices = new List<MyVector2>(poly);

            //Save the new vertices temporarily in this list before transfering them to vertices
            List<MyVector2> vertices_tmp = new List<MyVector2>();

            //Clip the polygon
            for (int i = 0; i < clippingPlanes.Count; i++)
            {
                Plane2 plane = clippingPlanes[i];

                for (int j = 0; j < vertices.Count; j++)
                {
                    int jPlusOne = MathUtility.ClampListIndex(j + 1, vertices.Count);

                    MyVector2 v1 = vertices[j];
                    MyVector2 v2 = vertices[jPlusOne];

                    //Calculate the distance to the plane from each vertex
                    //This is how we will know if they are inside or outside
                    //If they are inside, the distance is positive, which is why the planes normals have to be oriented to the inside
                    float dist_to_v1 = _Geometry.GetSignedDistanceFromPointToPlane(v1, plane);
                    float dist_to_v2 = _Geometry.GetSignedDistanceFromPointToPlane(v2, plane);

                    //TODO: What will happen if they are exactly 0? Should maybe use a tolerance of 0.001

                    //Case 1. Both are outside (= to the right), do nothing 

                    //Case 2. Both are inside (= to the left), save v2
                    if (dist_to_v1 >= 0f && dist_to_v2 >= 0f)
                    {
                        vertices_tmp.Add(v2);
                    }
                    //Case 3. Outside -> Inside, save intersection point and v2
                    else if (dist_to_v1 < 0f && dist_to_v2 >= 0f)
                    {
                        MyVector2 rayDir = MyVector2.Normalize(v2 - v1);

                        Ray2 ray = new Ray2(v1, rayDir);

                        MyVector2 intersectionPoint = _Intersections.GetRayPlaneIntersectionPoint(plane, ray);

                        vertices_tmp.Add(intersectionPoint);

                        vertices_tmp.Add(v2);
                    }
                    //Case 4. Inside -> Outside, save intersection point
                    else if (dist_to_v1 >= 0f && dist_to_v2 < 0f)
                    {
                        MyVector2 rayDir = MyVector2.Normalize(v2 - v1);

                        Ray2 ray = new Ray2(v1, rayDir);

                        MyVector2 intersectionPoint = _Intersections.GetRayPlaneIntersectionPoint(plane, ray);

                        vertices_tmp.Add(intersectionPoint);
                    }
                }

                //Add the new vertices to the list of vertices
                vertices.Clear();

                vertices.AddRange(vertices_tmp);

                vertices_tmp.Clear();
            }

            return vertices;
        }



        //Get the clipping planes
        public static List<Plane2> GetClippingPlanes(List<MyVector2> clipPoly)
        {
            //Calculate the clipping planes
            List<Plane2> clippingPlanes = new List<Plane2>();

            for (int i = 0; i < clipPoly.Count; i++)
            {
                int iPlusOne = MathUtility.ClampListIndex(i + 1, clipPoly.Count);

                MyVector2 v1 = clipPoly[i];
                MyVector2 v2 = clipPoly[iPlusOne];

                //Doesnt have to be center but easier to debug
                MyVector2 planePos = (v1 + v2) * 0.5f;

                MyVector2 planeDir = v2 - v1;

                //Should point inwards - do we need to normalize???
                MyVector2 planeNormal = new MyVector2(-planeDir.y, planeDir.x);

                //Gizmos.DrawRay(planePos, planeNormal.normalized * 0.1f);

                clippingPlanes.Add(new Plane2(planePos, planeNormal));
            }

            return clippingPlanes;
        }
    }
}
