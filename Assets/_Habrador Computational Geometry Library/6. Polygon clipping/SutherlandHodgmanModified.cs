using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //If we modify the Sutherland-Hodgman algorithm we can do boolean operations on polygon
    public static class SutherlandHodgmanModified
    {
        //The original algorithm calculates the intersection between two polygons, this will instead get the outside
        //Assumes the polygons are oriented counter clockwise
        //poly is the polygon we want to cut
        //Assumes the polygon we want to remove from the other polygon is convex, so clipPolygon has to be convex
        //We will end up with the !intersection of the polygons
        public static List<List<MyVector2>> ClipPolygonInverted(List<MyVector2> poly, List<Plane2> clippingPlanes)
        {
            //The result may be more than one polygons
            List<List<MyVector2>> finalPolygons = new List<List<MyVector2>>();

            List<MyVector2> vertices = new List<MyVector2>(poly);

            //The remaining polygon after each cut
            List<MyVector2> vertices_tmp = new List<MyVector2>();

            //Clip the polygon
            for (int i = 0; i < clippingPlanes.Count; i++)
            {
                Plane2 plane = clippingPlanes[i];

                //A new polygon which is the part of the polygon which is outside of this plane
                List<MyVector2> outsidePolygon = new List<MyVector2>();

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
                        MyVector2 rayDir = MyVector2.Normalize(v2 - v1);

                        Ray2 ray = new Ray2(v1, rayDir);

                        MyVector2 intersectionPoint = _Intersections.GetRayPlaneIntersectionPoint(plane, ray);

                        outsidePolygon.Add(intersectionPoint);

                        vertices_tmp.Add(intersectionPoint);

                        vertices_tmp.Add(v2);
                    }
                    //Case 4. Inside -> Outside, save intersection point and v2
                    else if (dist_to_v1 >= 0f && dist_to_v2 < 0f)
                    {
                        MyVector2 rayDir = MyVector2.Normalize(v2 - v1);

                        Ray2 ray = new Ray2(v1, rayDir);

                        MyVector2 intersectionPoint = _Intersections.GetRayPlaneIntersectionPoint(plane, ray);

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
        public static List<List<MyVector2>> BooleanOperations(List<MyVector2> poly, List<MyVector2> clipPoly, BooleanOperation booleanOperation)
        {
            List<List<MyVector2>> finalPolygon = new List<List<MyVector2>>();

            //First check if the polygons are intersecting
            //One way to do this is to get the intersection between the polygons
            //which is 0 if they dont intersect


            //Intersection - Remove everything except where both A and B intersect
            if (booleanOperation == BooleanOperation.Intersection)
            {
                List<MyVector2> intersectionPolygon = SutherlandHodgman.ClipPolygon(poly, clipPoly);

                finalPolygon.Add(intersectionPolygon);

                //Debug.Log(intersectionPolygon.Count);
            }
            //Difference - Remove from A where B intersect with A. Remove everything from B
            else if (booleanOperation == BooleanOperation.Difference)
            {
                List<Plane2> clippingPlanes = SutherlandHodgman.GetClippingPlanes(clipPoly);

                finalPolygon = ClipPolygonInverted(poly, clippingPlanes);
            }
            //ExclusiveOr - Remove from A and B where A and B intersect
            else if (booleanOperation == BooleanOperation.ExclusiveOr)
            {
                //A not B
                List<Plane2> clippingPlanes = SutherlandHodgman.GetClippingPlanes(clipPoly);

                finalPolygon = ClipPolygonInverted(poly, clippingPlanes);

                //B not A
                clippingPlanes = SutherlandHodgman.GetClippingPlanes(poly);

                finalPolygon.AddRange(ClipPolygonInverted(clipPoly, clippingPlanes));
            }
            //Union - Combine A and B into one. Keep everything from A and B
            else if (booleanOperation == BooleanOperation.Union)
            {
                //A not B
                List<Plane2> clippingPlanes = SutherlandHodgman.GetClippingPlanes(clipPoly);

                finalPolygon = ClipPolygonInverted(poly, clippingPlanes);

                //B not A
                clippingPlanes = SutherlandHodgman.GetClippingPlanes(poly);

                finalPolygon.AddRange(ClipPolygonInverted(clipPoly, clippingPlanes));

                //A and B
                List<MyVector2> intersectionPolygon = SutherlandHodgman.ClipPolygon(poly, clipPoly);

                finalPolygon.Add(intersectionPolygon);
            }

            return finalPolygon;
        }
    }
}
