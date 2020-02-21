using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class _ConvexHull
    {
        //Algorithm 1. Jarvis March - slow but simple
        public static List<MyVector2> JarvisMarch(HashSet<MyVector2> points)
        {
            //Has to return a list and not hashset because the points have an order coming after each other
            List<MyVector2> pointsOnHull = JarvisMarchAlgorithm.GenerateConvexHull(points);

            return pointsOnHull;
        }
    }
}
