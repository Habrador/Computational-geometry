using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Generates Voronoi diagrams with different algorithms
    public static class _Voronoi
    {
        //Algorithm 1. Delaunay to Voronoi (Will also generate the delaunay)
        public static HashSet<VoronoiCell2> DelaunyToVoronoi(HashSet<MyVector2> sites)
        {
            HashSet<VoronoiCell2> voronoiCells = DelaunayToVoronoiAlgorithm.GenerateVoronoiDiagram(sites);

            return voronoiCells;
        }



        //Algorithm 2. Voronoi by adding point after point



        //Algorithm 3. 3D Delaunay to Voronoi
        public static HashSet<VoronoiCell3> Delaunay3DToVoronoi(HalfEdgeData3 delaunayTriangulation)
        {
            HashSet<VoronoiCell3> voronoiCells = Delaunay3DToVoronoiAlgorithm.GenerateVoronoiDiagram(delaunayTriangulation);

            return voronoiCells;
        }
    }
}
