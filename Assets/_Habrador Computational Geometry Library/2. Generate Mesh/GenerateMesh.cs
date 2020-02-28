using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class GenerateMesh
    {
        //Generate a square grid where each cell has two triangles
        public static HashSet<Triangle2> GenerateGrid(float width, int cells)
        {
            HashSet<Triangle2> grid = Grid.GenerateGrid(width, cells);

            return grid;
        }



        //
        // Shapes
        //

        //Circle
        public static HashSet<Triangle2> GenerateCircle(MyVector2 center, float radius, int resolution)
        {
            HashSet<Triangle2> triangles = Shapes.Circle(center, radius, resolution);

            return triangles;
        }

        //Circle with hole in it
        public static HashSet<Triangle2> GenerateCircleHollow(MyVector2 center, float radius, int resolution, float width)
        {
            HashSet<Triangle2> triangles = Shapes.CircleHollow(center, radius, resolution, width);

            return triangles;
        }

        //Line segment
        public static HashSet<Triangle2> GenerateLineSegment(MyVector2 p1, MyVector2 p2, float width)
        {
            HashSet<Triangle2> triangles = Shapes.LineSegment(p1, p2, width);

            return triangles;
        }

        //Connected lines
        //isConnected means if the end points are connected to form a loop
        public static HashSet<Triangle2> ConnectedLineSegments(List<MyVector2> points, float width, bool isConnected)
        {
            HashSet<Triangle2> triangles = Shapes.ConnectedLineSegments(points, width, isConnected);

            return triangles;
        }

        //Arrow
        public static HashSet<Triangle2> Arrow(MyVector2 p1, MyVector2 p2, float lineWidth, float arrowSize)
        {
            HashSet<Triangle2> triangles = Shapes.Arrow(p1, p2, lineWidth, arrowSize);

            return triangles;
        }
    }
}
