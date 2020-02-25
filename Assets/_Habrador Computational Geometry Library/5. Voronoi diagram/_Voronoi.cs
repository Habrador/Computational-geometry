using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Generates Voronoi diagrams with different algorithms
    public static class _Voronoi
    {
        //Algorithm 1. Delaunay to Voronoi
        public static List<VoronoiCell2> DelaunyToVoronoi(HashSet<MyVector2> sites)
        {
            List<VoronoiCell2> voronoiCells = DelaunayToVoronoiAlgorithm.GenerateVoronoiDiagram(sites);

            return voronoiCells;
        }



        //Algorithm 2. Voronoi by adding point after point
    }
}
