using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Generate a voronoi diagram by first generating a delaunay triangulation
    //From https://stackoverflow.com/questions/85275/how-do-i-derive-a-voronoi-diagram-given-its-point-set-and-its-delaunay-triangula
    public static class DelaunayToVoronoiAlgorithm
    {
        public static List<VoronoiCell2> GenerateVoronoiDiagram(HashSet<MyVector2> sites)
        {
            //First generate the delaunay triangulation
            //This one has caused a bug so should be avoided
            //HalfEdgeData2 data = _Delaunay.FlippingEdges(sites, new HalfEdgeData2());
            //This one is faster and more accurate, so use it. But if you are using it, make sure to normalize the sites!
            HalfEdgeData2 data = _Delaunay.PointByPoint(sites, new HalfEdgeData2());


            //Generate the voronoi diagram

            return GenerateVoronoiFromDelaunay(data);
        }

        public static List<VoronoiCell2> GenerateVoronoiFromDelaunay(HalfEdgeData2 delaunayData)
        {
            //Step 1. For every delaunay edge, compute a voronoi edge
            //The voronoi edge is the edge connecting the circumcenters of two neighboring delaunay triangles
            List<VoronoiEdge2> voronoiEdges = new List<VoronoiEdge2>();

            HashSet<HalfEdgeFace2> triangles = delaunayData.faces;

            foreach (HalfEdgeFace2 t in triangles)
            {
                //Each triangle consists of these edges
                HalfEdge2 e1 = t.edge;
                HalfEdge2 e2 = e1.nextEdge;
                HalfEdge2 e3 = e2.nextEdge;

                //Calculate the circumcenter for this triangle
                MyVector2 v1 = e1.v.position;
                MyVector2 v2 = e2.v.position;
                MyVector2 v3 = e3.v.position;

                //The circumcenter is the center of a circle where the triangles corners is on the circumference of that circle
                //The circumcenter is also known as a voronoi vertex, which is a position in the diagram where we are equally
                //close to the surrounding sites
                MyVector2 voronoiVertex = _Geometry.CalculateCircleCenter(v1, v2, v3);
                //Debug.Log(voronoiVertex.x + " " + voronoiVertex.y);
                //This will generate double edges - one belonging to each site, and could maybe be improved in the future
                //by using the half-edge data structure
                TryAddVoronoiEdgeFromTriangleEdge(e1, voronoiVertex, voronoiEdges);
                TryAddVoronoiEdgeFromTriangleEdge(e2, voronoiVertex, voronoiEdges);
                TryAddVoronoiEdgeFromTriangleEdge(e3, voronoiVertex, voronoiEdges);
            }


            //Step 2. Find the voronoi cells where each cell is a list of all edges belonging to a site
            List<VoronoiCell2> voronoiCells = new List<VoronoiCell2>();

            for (int i = 0; i < voronoiEdges.Count; i++)
            {
                VoronoiEdge2 e = voronoiEdges[i];

                //Find the position in the list of all cells that includes this site
                int cellPos = TryFindCellPos(e, voronoiCells);

                //No cell was found so we need to create a new cell
                if (cellPos == -1)
                {
                    VoronoiCell2 newCell = new VoronoiCell2(e.sitePos, e.color);
                    //newCell.color = Color.green;// Random.ColorHSV(); // ToDo get color from halfedge vert

                    voronoiCells.Add(newCell);

                    newCell.edges.Add(e);
                }
                else
                {
                    voronoiCells[cellPos].edges.Add(e);
                }
            }


            return voronoiCells;
        }



        //Find the position in the list of all cells that includes this site
        //Returns -1 if no cell is found
        private static int TryFindCellPos(VoronoiEdge2 e, List<VoronoiCell2> voronoiCells)
        {
            for (int i = 0; i < voronoiCells.Count; i++)
            {
                if (e.sitePos.Equals(voronoiCells[i].sitePos))
                {
                    return i;
                }
            }

            return -1;
        }



        //Try to add a voronoi edge. Not all edges have a neighboring triangle, and if it hasnt we cant add a voronoi edge
        private static void TryAddVoronoiEdgeFromTriangleEdge(HalfEdge2 e, MyVector2 voronoiVertex, List<VoronoiEdge2> allEdges)
        {
            //Ignore if this edge has no neighboring triangle
            if (e.oppositeEdge == null)
            {
                return;
            }

            //Calculate the circumcenter of the neighbor
            HalfEdge2 eNeighbor = e.oppositeEdge;

            MyVector2 v1 = eNeighbor.v.position;
            MyVector2 v2 = eNeighbor.nextEdge.v.position;
            MyVector2 v3 = eNeighbor.nextEdge.nextEdge.v.position;

            MyVector2 voronoiVertexNeighbor = _Geometry.CalculateCircleCenter(v1, v2, v3);

            //Create a new vornoi edge between the voronoi vertices
            VoronoiEdge2 edge = new VoronoiEdge2(voronoiVertex, voronoiVertexNeighbor, e.prevEdge.v.position, e.prevEdge.v.color);

            allEdges.Add(edge);
        }
    }
}
