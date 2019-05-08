using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //From https://stackoverflow.com/questions/85275/how-do-i-derive-a-voronoi-diagram-given-its-point-set-and-its-delaunay-triangula
    public class DelaunayToVoronoi
    {
        public static List<VoronoiCell> GenerateVoronoiDiagram(HashSet<Vector3> sites)
        {
            //First generate the delaunay triangulation
            HalfEdgeData data = _Delaunay.TriangulateByFlippingEdges(sites, new HalfEdgeData());


            //Generate the voronoi diagram

            //Step 1. For every delaunay edge, compute a voronoi edge
            //The voronoi edge is the edge connecting the circumcenters of two neighboring delaunay triangles
            List<VoronoiEdge> voronoiEdges = new List<VoronoiEdge>();

            HashSet<HalfEdgeFace> triangles = data.faces;

            foreach (HalfEdgeFace t in triangles)
            {
                //Each triangle consists of these edges
                HalfEdge e1 = t.edge;
                HalfEdge e2 = e1.nextEdge;
                HalfEdge e3 = e2.nextEdge;

                //Calculate the circumcenter for this triangle
                Vector3 v1 = e1.v.position;
                Vector3 v2 = e2.v.position;
                Vector3 v3 = e3.v.position;

                //The circumcenter is the center of a circle where the triangles corners is on the circumference of that circle
                Vector2 center2D = Geometry.CalculateCircleCenter(v1.XZ(), v2.XZ(), v3.XZ());

                //The circumcenter is also known as a voronoi vertex, which is a position in the diagram where we are equally
                //close to the surrounding sites
                Vector3 voronoiVertex = new Vector3(center2D.x, 0f, center2D.y);

                //This will generate double edges - one belonging to each site, and could maybe be improved in the future
                //by using the half-edge data structure
                TryAddVoronoiEdgeFromTriangleEdge(e1, voronoiVertex, voronoiEdges);
                TryAddVoronoiEdgeFromTriangleEdge(e2, voronoiVertex, voronoiEdges);
                TryAddVoronoiEdgeFromTriangleEdge(e3, voronoiVertex, voronoiEdges);
            }


            //Step 2. Find the voronoi cells where each cell is a list of all edges belonging to a site
            List<VoronoiCell> voronoiCells = new List<VoronoiCell>();

            for (int i = 0; i < voronoiEdges.Count; i++)
            {
                VoronoiEdge e = voronoiEdges[i];

                //Find the position in the list of all cells that includes this site
                int cellPos = TryFindCellPos(e, voronoiCells);

                //No cell was found so we need to create a new cell
                if (cellPos == -1)
                {
                    VoronoiCell newCell = new VoronoiCell(e.sitePos);

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
        private static int TryFindCellPos(VoronoiEdge e, List<VoronoiCell> voronoiCells)
        {
            for (int i = 0; i < voronoiCells.Count; i++)
            {
                if (e.sitePos == voronoiCells[i].sitePos)
                {
                    return i;
                }
            }

            return -1;
        }



        //Try to add a voronoi edge. Not all edges have a neighboring triangle, and if it hasnt we cant add a voronoi edge
        private static void TryAddVoronoiEdgeFromTriangleEdge(HalfEdge e, Vector3 voronoiVertex, List<VoronoiEdge> allEdges)
        {
            //Ignore if this edge has no neighboring triangle
            if (e.oppositeEdge == null)
            {
                return;
            }

            //Calculate the circumcenter of the neighbor
            HalfEdge eNeighbor = e.oppositeEdge;

            Vector3 v1 = eNeighbor.v.position;
            Vector3 v2 = eNeighbor.nextEdge.v.position;
            Vector3 v3 = eNeighbor.nextEdge.nextEdge.v.position;

            Vector2 center2D = Geometry.CalculateCircleCenter(v1.XZ(), v2.XZ(), v3.XZ());

            Vector3 voronoiVertexNeighbor = new Vector3(center2D.x, 0f, center2D.y);

            //Create a new vornoi edge between the voronoi vertices
            VoronoiEdge edge = new VoronoiEdge(voronoiVertex, voronoiVertexNeighbor, e.prevEdge.v.position);

            allEdges.Add(edge);
        }
    }



    public class VoronoiEdge
    {
        //These are the voronoi vertices
        public Vector3 v1;
        public Vector3 v2;

        //All positions within a vornoi cell is closer to this position than any other position in the diagram
        public Vector3 sitePos;

        public VoronoiEdge(Vector3 v1, Vector3 v2, Vector3 sitePos)
        {
            this.v1 = v1;
            this.v2 = v2;

            this.sitePos = sitePos;
        }
    }



    public class VoronoiCell
    {
        //All positions within a vornoi cell is closer to this position than any other position in the diagram
        public Vector3 sitePos;

        public List<VoronoiEdge> edges = new List<VoronoiEdge>();

        public VoronoiCell(Vector3 sitePos)
        {
            this.sitePos = sitePos;
        }
    }
}
