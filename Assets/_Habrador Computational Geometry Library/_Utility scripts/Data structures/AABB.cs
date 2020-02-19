using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Axis-Aligned-Bounding-Box, which is a rectangle aligned along the x and y axis
    //Is used for intersections between other AABB rectangles because this intersection test is fast
    public struct AABB
    {
        public float minX;
        public float maxX;
        public float minY;
        public float maxY;

        public AABB(float minX, float maxX, float minY, float maxY)
        {
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
        }
    }
}
