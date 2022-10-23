using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Habrador_Computational_Geometry
{
    //All algorithms should be in namespace Habrador_Computational_Geometry.MeshAlgorithms
    public static class _GenerateMesh
    {
        //Generate a square grid where each cell has two triangles
        public static HashSet<Triangle2> GenerateGrid(float width, int cells)
        {
            HashSet<Triangle2> grid = MeshAlgorithms.Grid.GenerateGrid(width, cells);

            return grid;
        }



        //
        // Shapes
        //

        //Circle
        public static HashSet<Triangle2> Circle(MyVector2 center, float radius, int resolution)
        {
            HashSet<Triangle2> triangles = MeshAlgorithms.Shapes.Circle(center, radius, resolution);

            return triangles;
        }

        //Circle with hole in it
        public static HashSet<Triangle2> CircleHollow(MyVector2 center, float innerRadius, int resolution, float width)
        {
            HashSet<Triangle2> triangles = MeshAlgorithms.Shapes.CircleHollow(center, innerRadius, resolution, width);

            return triangles;
        }

        //Line segment
        public static HashSet<Triangle2> LineSegment(MyVector2 p1, MyVector2 p2, float width)
        {
            HashSet<Triangle2> triangles = MeshAlgorithms.Shapes.LineSegment(p1, p2, width);

            return triangles;
        }

        //Connected lines
        //isConnected means if the end points are connected to form a loop
        //These values should be normalized because it's using ray-ray intersection to make the line
        public static HashSet<Triangle2> ConnectedLineSegments(List<MyVector2> points, float width, bool isConnected)
        {
            HashSet<Triangle2> triangles = MeshAlgorithms.Shapes.ConnectedLineSegments(points, width, isConnected);

            return triangles;
        }

        //Arrow
        public static HashSet<Triangle2> Arrow(MyVector2 p1, MyVector2 p2, float lineWidth, float arrowSize)
        {
            HashSet<Triangle2> triangles = MeshAlgorithms.Shapes.Arrow(p1, p2, lineWidth, arrowSize);

            return triangles;
        }
    }
}
