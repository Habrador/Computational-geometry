using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Habrador_Computational_Geometry
{
    //Triangulate a concave hull (with holes) by using an algorithm called Ear Clipping
    public static class EarClipping
    {
        //This assumes that we have a hull and now we want to triangulate it
        //The points on the hull should be ordered counter-clockwise
        //This alorithm is called ear clipping and it's O(n*n) Another common algorithm is dividing it into trapezoids and it's O(n log n)
        //One can maybe do it in O(n) time but no such version is known
        //Assumes we have at least 3 points
        public static HashSet<Triangle2> Triangulate(List<MyVector2> pointsOnHull)
        {
            HashSet<Triangle2> triangles = new HashSet<Triangle2>();
        
            return triangles;
        }
    }
}
