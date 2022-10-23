using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class Delaunay3DToVoronoiAlgorithm 
    {
        //Generate a Voronoi diagram in 3d space given a Delaunay triangulation in 3d space
        public static HashSet<VoronoiCell3> GenerateVoronoiDiagram(HalfEdgeData3 delaunayTriangulation)
        {
            //If we dont need the voronoi sitePos, which is the center of the voronoi cell, we can use the half-edge data structure
            //If not we have the create a child class for voronoi
            HashSet<VoronoiCell3> voronoiDiagram = new HashSet<VoronoiCell3>();


            //Step 1. Generate a center of circle for each triangle because this process is slow in 3d space
            Dictionary<HalfEdgeFace3, MyVector3> circleCenterLookup = new Dictionary<HalfEdgeFace3, MyVector3>();

            HashSet<HalfEdgeFace3> delaunayTriangles = delaunayTriangulation.faces;

            foreach (HalfEdgeFace3 triangle in delaunayTriangles)
            {
                MyVector3 p1 = triangle.edge.v.position;
                MyVector3 p2 = triangle.edge.nextEdge.v.position;
                MyVector3 p3 = triangle.edge.nextEdge.nextEdge.v.position;

                MyVector3 circleCenter = _Geometry.CalculateCircleCenter(p1, p2, p3);

                //https://www.redblobgames.com/x/1842-delaunay-voronoi-sphere/ suggested circleCenter should be moved to get a better surface
                //But it generates a bad result
                //float d = Mathf.Sqrt(circleCenter.x * circleCenter.x + circleCenter.y * circleCenter.y + circleCenter.z * circleCenter.z);

                //MyVector3 circleCenterMove = new MyVector3(circleCenter.x / d, circleCenter.y / d, circleCenter.z / d);

                //circleCenter = circleCenterMove;

                circleCenterLookup.Add(triangle, circleCenter);
            }


            //Step 2. Generate the voronoi cells
            HashSet<HalfEdgeVertex3> delaunayVertices = delaunayTriangulation.verts;

            //In the half-edge data structure we have multiple vertices at the same position, 
            //so we have to track which vertex positions have been added
            HashSet<MyVector3> addedSites = new HashSet<MyVector3>();


            foreach (HalfEdgeVertex3 v in delaunayVertices)
            {
                //Has this site already been added?
                if (addedSites.Contains(v.position))
                {
                    continue;
                }

            
                addedSites.Add(v.position);

                //This vertex is a cite pos in the voronoi diagram
                VoronoiCell3 cell = new VoronoiCell3(v.position);

                voronoiDiagram.Add(cell);

                //All triangles are fully connected so no null opposite edges should exist
                //So to generate the voronoi cell, we just rotate clock-wise around each vertex in the delaunay triangulation

                HalfEdge3 currentEdge = v.edge;
            
                int safety = 0;

                while (true)
                {
                    //Build an edge going from the opposite face to this face
                    //Each vertex has an edge going FROM it
                    HalfEdgeFace3 oppositeTriangle = currentEdge.oppositeEdge.face;

                    HalfEdgeFace3 thisTriangle = currentEdge.face;

                    MyVector3 oppositeCircleCenter = circleCenterLookup[oppositeTriangle];

                    MyVector3 thisCircleCenter = circleCenterLookup[thisTriangle];

                    VoronoiEdge3 edge = new VoronoiEdge3(oppositeCircleCenter, thisCircleCenter, v.position);

                    cell.edges.Add(edge);

                    //Jump to the next triangle
                    //Each vertex has an edge going FROM it
                    //And we want to rotate around a vertex clockwise
                    //So the edge we should jump over is:
                    HalfEdge3 jumpEdge = currentEdge.nextEdge.nextEdge;

                    HalfEdge3 oppositeEdge = jumpEdge.oppositeEdge;

                    //Are we back where we started?
                    if (oppositeEdge == v.edge)
                    {
                        break;
                    }

                    currentEdge = oppositeEdge;

                
                    safety += 1;

                    if (safety > 10000)
                    {
                        Debug.Log("Stuck in infinite loop when generating voronoi cells");

                        break;
                    }
                }
            }

            return voronoiDiagram;
        }
        
    }
}
