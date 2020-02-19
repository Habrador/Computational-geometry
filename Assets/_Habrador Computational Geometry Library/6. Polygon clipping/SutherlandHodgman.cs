using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Calculate the intersection of two polygons
    public static class SutherlandHodgman
    {
        //
        // The standard algortihm to get the intersection
        //

        //Sometimes its more efficient to calculate the planes once before we call the method
        //if we want to cut several polygons with the same planes
        public static List<Vector2> ClipPolygon(List<Vector2> poly, List<Vector2> clipPoly)
        {
            //Calculate the clipping planes
            List<Plane2D> clippingPlanes = GetClippingPlanes(clipPoly);

            List<Vector2> vertices = ClipPolygon(poly, clippingPlanes);

            return vertices;
        }



        //Assumes the polygons are oriented counter clockwise
        //poly is the polygon we want to cut
        //Assumes the polygon we want to remove from the other polygon is convex, so clipPolygon has to be convex
        //We will end up with the intersection of the polygons
        public static List<Vector2> ClipPolygon(List<Vector2> poly, List<Plane2D> clippingPlanes)
        {
            //Clone the vertices because we will remove vertices from this list
            List<Vector2> vertices = new List<Vector2>(poly);

            //Save the new vertices temporarily in this list before transfering them to vertices
            List<Vector2> vertices_tmp = new List<Vector2>();

            //Maybe better to use a linked list?
            //LinkedList<Vector3> vertices = new LinkedList<Vector3>();

            //Clip the polygon
            for (int i = 0; i < clippingPlanes.Count; i++)
            {
                Plane2D plane = clippingPlanes[i];

                for (int j = 0; j < vertices.Count; j++)
                {
                    int jPlusOne = MathUtility.ClampListIndex(j + 1, vertices.Count);

                    Vector2 v1 = vertices[j];
                    Vector2 v2 = vertices[jPlusOne];

                    //Calculate the distance to the plane from each vertex
                    //This is how we will know if they are inside or outside
                    //If they are inside, the distance is positive, which is why the planes normals have to be oriented to the inside
                    float dist_to_v1 = Geometry.DistanceFromPointToPlane(plane.normal, plane.pos, v1);
                    float dist_to_v2 = Geometry.DistanceFromPointToPlane(plane.normal, plane.pos, v2);

                    //TODO: What will happen if they are exactly 0? Should maybe use a tolerance 0f 0.001

                    //Case 1. Both are outside (= to the right), do nothing 

                    //Case 2. Both are inside (= to the left), save v2
                    if (dist_to_v1 >= 0f && dist_to_v2 >= 0f)
                    {
                        vertices_tmp.Add(v2);
                    }
                    //Case 3. Outside -> Inside, save intersection point and v2
                    else if (dist_to_v1 < 0f && dist_to_v2 >= 0f)
                    {
                        Vector2 rayDir = (v2 - v1).normalized;

                        Vector2 intersectionPoint = Intersections.GetRayPlaneIntersectionCoordinate(plane.pos, plane.normal, v1, rayDir);

                        vertices_tmp.Add(intersectionPoint);

                        vertices_tmp.Add(v2);
                    }
                    //Case 4. Inside -> Outside, save intersection point
                    else if (dist_to_v1 >= 0f && dist_to_v2 < 0f)
                    {
                        Vector2 rayDir = (v2 - v1).normalized;

                        Vector2 intersectionPoint = Intersections.GetRayPlaneIntersectionCoordinate(plane.pos, plane.normal, v1, rayDir);

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
        private static List<Plane2D> GetClippingPlanes(List<Vector2> clipPoly)
        {
            //Calculate the clipping planes
            List<Plane2D> clippingPlanes = new List<Plane2D>();

            for (int i = 0; i < clipPoly.Count; i++)
            {
                int iPlusOne = MathUtility.ClampListIndex(i + 1, clipPoly.Count);

                Vector2 v1 = clipPoly[i];
                Vector2 v2 = clipPoly[iPlusOne];

                //Doesnt have to be center but easier to debug
                Vector2 planePos = (v1 + v2) * 0.5f;

                Vector2 planeDir = v2 - v1;

                //Should point inwards - do we need to normalize???
                Vector2 planeNormal = new Vector2(-planeDir.y, planeDir.x);

                //Gizmos.DrawRay(planePos, planeNormal.normalized * 0.1f);

                clippingPlanes.Add(new Plane2D(planePos, planeNormal));
            }

            return clippingPlanes;
        }



        //
        // The modified algortihm to get the !intersection
        //

        //Assumes the polygons are oriented counter clockwise
        //poly is the polygon we want to cut
        //Assumes the polygon we want to remove from the other polygon is convex, so clipPolygon has to be convex
        //We will end up with the !intersection of the polygons
        public static List<List<Vector2>> ClipPolygonInverted(List<Vector2> poly, List<Plane2D> clippingPlanes)
        {
            //The result may be more than one polygons
            List<List<Vector2>> finalPolygons = new List<List<Vector2>>();

            List<Vector2> vertices = new List<Vector2>(poly);

            //The remaining polygon after each cut
            List<Vector2> vertices_tmp = new List<Vector2>();

            //Clip the polygon
            for (int i = 0; i < clippingPlanes.Count; i++)
            {
                Plane2D plane = clippingPlanes[i];

                //A new polygon which is the part of the polygon which is outside of this plane
                List<Vector2> outsidePolygon = new List<Vector2>();

                for (int j = 0; j < vertices.Count; j++)
                {
                    int jPlusOne = MathUtility.ClampListIndex(j + 1, vertices.Count);

                    Vector2 v1 = vertices[j];
                    Vector2 v2 = vertices[jPlusOne];

                    //Calculate the distance to the plane from each vertex
                    //This is how we will know if they are inside or outside
                    //If they are inside, the distance is positive, which is why the planes normals have to be oriented to the inside
                    float dist_to_v1 = Geometry.DistanceFromPointToPlane(plane.normal, plane.pos, v1);
                    float dist_to_v2 = Geometry.DistanceFromPointToPlane(plane.normal, plane.pos, v2);

                    //TODO: What will happen if they are exactly 0?

                    //Case 1. Both are inside (= to the left), save v2 to the other polygon
                    if (dist_to_v1 >= 0f && dist_to_v2 >= 0f)
                    {
                        vertices_tmp.Add(v2);
                    }
                    //Case 2. Both are outside (= to the right), save v1
                    else if (dist_to_v1 < 0f && dist_to_v2 < 0f)
                    {
                        outsidePolygon.Add(v2);
                    }
                    //Case 3. Outside -> Inside, save intersection point
                    else if (dist_to_v1 < 0f && dist_to_v2 >= 0f)
                    {
                        Vector2 rayDir = (v2 - v1).normalized;

                        Vector2 intersectionPoint = Intersections.GetRayPlaneIntersectionCoordinate(plane.pos, plane.normal, v1, rayDir);

                        outsidePolygon.Add(intersectionPoint);

                        vertices_tmp.Add(intersectionPoint);

                        vertices_tmp.Add(v2);
                    }
                    //Case 4. Inside -> Outside, save intersection point and v2
                    else if (dist_to_v1 >= 0f && dist_to_v2 < 0f)
                    {
                        Vector2 rayDir = (v2 - v1).normalized;

                        Vector2 intersectionPoint = Intersections.GetRayPlaneIntersectionCoordinate(plane.pos, plane.normal, v1, rayDir);

                        outsidePolygon.Add(intersectionPoint);

                        outsidePolygon.Add(v2);

                        vertices_tmp.Add(intersectionPoint);
                    }
                }

                //Add the polygon outside of this plane to the list of all polygons that are outside of all planes
                if (outsidePolygon.Count > 0)
                {
                    finalPolygons.Add(outsidePolygon);
                }

                //Add the polygon which was inside of this and previous planes to the polygon we want to test
                vertices.Clear();

                vertices.AddRange(vertices_tmp);

                vertices_tmp.Clear();
            }

            return finalPolygons;
        }



        //
        // Different boolean operations
        //

        //Assumes both polygons are convex unless you want the intersection, then only the clipping polygon
        //needs to be convex
        public static List<List<Vector2>> BooleanOperations(List<Vector2> poly, List<Vector2> clipPoly, BooleanOperation booleanOperation)
        {
            List<List<Vector2>> finalPolygon = new List<List<Vector2>>();

            //First check if the polygons are intersecting
            //One way to do this is to get the intersection between the polygons
            //which is 0 if they dont intersect


            //Intersection - Remove everything except where both A and B intersect
            if (booleanOperation == BooleanOperation.Intersection)
            {
                List<Vector2> intersectionPolygon = ClipPolygon(poly, clipPoly);

                finalPolygon.Add(intersectionPolygon);

                //Debug.Log(intersectionPolygon.Count);
            }
            //Difference - Remove from A where B intersect with A. Remove everything from B
            else if (booleanOperation == BooleanOperation.Difference)
            {
                List<Plane2D> clippingPlanes = GetClippingPlanes(clipPoly);

                finalPolygon = ClipPolygonInverted(poly, clippingPlanes);
            }
            //ExclusiveOr - Remove from A and B where A and B intersect
            else if (booleanOperation == BooleanOperation.ExclusiveOr)
            {
                //A not B
                List<Plane2D> clippingPlanes = GetClippingPlanes(clipPoly);

                finalPolygon = ClipPolygonInverted(poly, clippingPlanes);

                //B not A
                clippingPlanes = GetClippingPlanes(poly);

                finalPolygon.AddRange(ClipPolygonInverted(clipPoly, clippingPlanes));
            }
            //Union - Combine A and B into one. Keep everything from A and B
            else if (booleanOperation == BooleanOperation.Union)
            {
                //A not B
                List<Plane2D> clippingPlanes = GetClippingPlanes(clipPoly);

                finalPolygon = ClipPolygonInverted(poly, clippingPlanes);

                //B not A
                clippingPlanes = GetClippingPlanes(poly);

                finalPolygon.AddRange(ClipPolygonInverted(clipPoly, clippingPlanes));

                //A and B
                List<Vector2> intersectionPolygon = ClipPolygon(poly, clipPoly);

                finalPolygon.Add(intersectionPolygon);
            }

            return finalPolygon;
        }
    }
}
