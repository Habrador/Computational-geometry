//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System;

////TODO: Can maybe use the circle test to determine the exact position of an edge position???
//public static class VoronoiSiteBySiteAlgorithm
//{
//    //Algorithm 1. Incremental by adding site by site O(n*n)
//    //https://courses.cs.washington.edu/courses/cse326/00wi/projects/voronoi.html
//    //breakInt is an int where we stop the iteration for debugging purposes
//    public static List<Cell> GenerateVoronoiDiagram(List<Vector3> sites, float halfWidth, int breakInt)
//    {
//        bool isDebugOn = false;
    
//        //Initialize edges and cells to be empty
//        List<Edge> edges = new List<Edge>();

//        List<Cell> cells = new List<Cell>();



//        //
//        // Step 1. Add three or four "points at infinity" to cells, to bound the diagram
//        //
//        float width = halfWidth * 3f;

//        float bigWidth = halfWidth * 10f;

//        //Add appropriate edges to these cells - will form a triangle
//        //And all triangles will form a diamond shape with center at center of points
//        Vector3 C = new Vector3(0f, 0f, 0f);
//        Vector3 R = new Vector3(bigWidth, 0f, 0f);
//        Vector3 T = new Vector3(0f, 0f, bigWidth);
//        Vector3 L = new Vector3(-bigWidth, 0f, 0f);
//        Vector3 B = new Vector3(0f, 0f, -bigWidth);

//        //BL
//        Cell cell1 = new Cell(new Vector3(-width, 0f, -width));

//        cell1.edges.Add(new Edge(C, L));
//        cell1.edges.Add(new Edge(L, B));
//        cell1.edges.Add(new Edge(B, C));

//        cells.Add(cell1);


//        //BR
//        Cell cell2 = new Cell(new Vector3(width, 0f, -width));

//        cell2.edges.Add(new Edge(C, B));
//        cell2.edges.Add(new Edge(B, R));
//        cell2.edges.Add(new Edge(R, C));

//        cells.Add(cell2);


//        //TR
//        Cell cell3 = new Cell(new Vector3(width, 0f, width));

//        cell3.edges.Add(new Edge(C, R));
//        cell3.edges.Add(new Edge(R, T));
//        cell3.edges.Add(new Edge(T, C));

//        cells.Add(cell3);


//        //TL
//        Cell cell4 = new Cell(new Vector3(-width, 0f, width));

//        cell4.edges.Add(new Edge(C, T));
//        cell4.edges.Add(new Edge(T, L));
//        cell4.edges.Add(new Edge(L, C));

//        cells.Add(cell4);



//        //
//        // Step 2. Add the sites one by one and rebuild the diagram
//        //
//        int iteration = 0;

//        //Loop through all sites we want to add
//        for (int i = 0; i < sites.Count; i++)
//        {
//            //Create a new cell with site as its site
//            Cell newCell = new Cell(sites[i]);

//            //For each existing cell
//            for (int j = 0; j < cells.Count; j++)
//            {
//                Cell existingCell = cells[j];


//                //Find perpendicular bisector of the line segment connecting the two sites
//                Vector3 vecBetween = (newCell.cellPos - existingCell.cellPos).normalized;

//                //This direction is always ccw around the site we are adding
//                Vector3 pbVec = new Vector3(vecBetween.z, 0f, -vecBetween.x);

//                //The position of this vector
//                Vector3 centerPos = (newCell.cellPos + existingCell.cellPos) * 0.5f;
                    

//                //Create a data structure to hold the critical points and edges to delete
//                List<Vector3> criticalPoints = new List<Vector3>();

//                List<Edge> edgesToDelete = new List<Edge>();

//                //Loop through all edges belonging to the current site
//                for (int k = 0; k < existingCell.edges.Count; k++)
//                {
//                    Edge edge = existingCell.edges[k];

//                    //Test the spatial relationship between e and pb (determine which side of a line a given point is on)
//                    Vector3 edge_p1 = edge.p1;
//                    Vector3 edge_p2 = edge.p2;


//                    if (isDebugOn && breakInt == iteration)
//                    {
//                        Gizmos.color = Color.white;
//                        Gizmos.DrawWireSphere(edge_p1, 0.5f);
//                        Gizmos.DrawWireSphere(edge_p2, 0.5f);

//                        //Debug.Log("Edge length: " + (edge_p1 - edge_p2).magnitude);

//                        //Debug.Log("Edges: " + existingCell.edges.Count);

//                        Gizmos.color = Color.blue;

//                        Gizmos.DrawLine(centerPos + pbVec * 200f, centerPos - pbVec * 200f);

//                        Gizmos.color = Color.blue;

//                        Gizmos.DrawWireSphere(newCell.cellPos, 0.5f);

//                        Gizmos.color = Color.yellow;

//                        Gizmos.DrawWireSphere(existingCell.cellPos, 0.5f);
//                    }

//                    //If e is on the near side of pb (closer to the new site we are currently adding than to the existing site), 
//                    //mark it to be deleted. This means that if the entire line is to the left of the line we are drawing now, delete it
//                    // < 0 -> to the right
//                    // = 0 -> on the line
//                    // > 0 -> to the left
//                    float relation_p1 = Geometry.DistanceFromPointToPlane(vecBetween, centerPos, edge_p1);
//                    float relation_p2 = Geometry.DistanceFromPointToPlane(vecBetween, centerPos, edge_p2);

//                    //http://sandervanrossen.blogspot.se/2009/12/realtime-csg-part-1.html
//                    //So, for example, when you determine the distance of a point to a plane you need to define for yourself that a point will be:
//                    //on the negative side of a plane when it's distance is < -epsilon.
//                    //on the positive side if it's distance is > epsilon.
//                    //on the plane itself if it's distance is between >= -epsilon and <= epsilon.
//                    //Make sure that you don't mix up, for example, > with >=, or you'll get all kinds of weird hard to track down bugs!
//                    //Be consistent!


//                    if (isDebugOn && breakInt == iteration)
//                    {
//                        Debug.Log(relation_p1);
//                        Debug.Log(relation_p2);
//                    }

//                    //Make the edge a little longer because of floating point precisions, or the intersection algortihm will not work
//                    //Vector3 edgeDir = (edge_p2 - edge_p1).normalized;

//                    float tolerance = 0.0001f;

//                    //Vector3 edge_p2_extended = edge_p2 + edgeDir * tolerance;
//                    //Vector3 edge_p1_extended = edge_p1 - edgeDir * tolerance;

//                    //Both points are to the left
//                    //if ((relation_p1 > 0f || Mathf.Approximately(relation_p1, 0f)) && (relation_p2 > 0f || Mathf.Approximately(relation_p2, 0f)))
//                    //float left_tolerance = -0.001f;
//                    //if ((relation_p1 > 0f && relation_p2 >= left_tolerance) || (relation_p2 > 0f && relation_p1 >= left_tolerance))
//                    //if (
//                    //    (relation_p1 > tolerance && (relation_p2 > tolerance || (relation_p2 >= -tolerance && relation_p2 <= tolerance))) || 
//                    //    (relation_p2 > tolerance && (relation_p1 > tolerance || (relation_p1 >= -tolerance && relation_p1 <= tolerance)))
//                    //    )
//                    if (relation_p1 > tolerance && relation_p2 > tolerance)
//                    {
//                        edgesToDelete.Add(edge);
//                    }
//                    else if (relation_p1 < -tolerance && relation_p2 < -tolerance)
//                    //else if (
//                    //    (relation_p1 < tolerance && (relation_p2 < tolerance || (relation_p2 >= -tolerance && relation_p2 <= tolerance))) ||
//                    //    (relation_p2 < tolerance && (relation_p1 < tolerance || (relation_p1 >= -tolerance && relation_p1 <= tolerance)))
//                    //    )
//                    {
//                        continue;
//                    }
//                    //else if (Intersections.AreLinePlaneIntersecting(vecBetween, centerPos, edge_p1_extended, edge_p2_extended))
//                    else
//                    {
//                        //if (relation_p1 >= -tolerance && relation_p1 <= tolerance && relation_p2 >= -tolerance && relation_p2 <= tolerance)
//                        //{
//                        //    Debug.Log("On the edge");
                        
//                        //    continue;
//                        //}
                    
//                        Vector3 intersectionPoint = Intersections.GetLinePlaneIntersectionCoordinate(vecBetween, centerPos, edge_p1, edge_p2);

//                        criticalPoints.Add(intersectionPoint);

//                        //Both points are to the right
//                        //if (relation_p1 <= -tolerance && relation_p2 <= -tolerance)
//                        //{
//                        //    //Move the one thats least to the right to the intersection coordinate
//                        //    if (relation_p1 > relation_p2)
//                        //    {
//                        //        edge.v1.position = intersectionPoint;
//                        //    }
//                        //    else
//                        //    {
//                        //        edge.v2.position = intersectionPoint;
//                        //    }
//                        //}
//                        //The points are on different sides of the line, so move the left one
//                        //else if (relation_p1 > 0f || Mathf.Approximately(relation_p1, 0f))
//                        //If p1 is to the left or on the line, move it to the intersection point
//                        if (relation_p1 >= -tolerance)
//                        {
//                            edge.p1 = intersectionPoint;
//                        }
//                        else
//                        {
//                            edge.p2 = intersectionPoint;
//                        }

//                        if (isDebugOn && breakInt == iteration)
//                        {
//                            Gizmos.color = Color.red;

//                            Gizmos.DrawWireSphere(intersectionPoint, 1f);
//                        }
//                    }



//                    if (isDebugOn && breakInt == iteration)
//                    {
//                        i = int.MaxValue - 1;
//                        j = int.MaxValue - 1;

//                        break;
//                    }

//                    iteration += 1;
//                }

//                //Critical points should now have 0 or 2 points, if 2 points, add it
//                if (isDebugOn && breakInt == iteration)
//                {
//                    Debug.Log("Intersection points " + criticalPoints.Count);
//                }

//                if (criticalPoints.Count == 2)
//                {
//                    Edge newEdge = new Edge(criticalPoints[0], criticalPoints[1]);

//                    //Debug.Log("Add new edge");
//                    //Gizmos.DrawLine(criticalPoints[0], criticalPoints[1]);

//                    //Add this edge to whoever wants it
//                    existingCell.edges.Add(newEdge);
//                    newCell.edges.Add(newEdge);
//                    //edges.Add(newEdge);
//                }

//                //else if (criticalPoints.Count > 2)
//                //{
//                //    Vector3 p1 = criticalPoints[0];

//                //    //Find the coordinate which is furthest away from this point and connect it
//                //    Vector3 p2 = criticalPoints[1];

//                //    for (int m = 2; m < criticalPoints.Count; m++)
//                //    {
//                //        if ((criticalPoints[m] - p1).sqrMagnitude > (p2 - p1).sqrMagnitude)
//                //        {
//                //            p2 = criticalPoints[m];
//                //        }
//                //    }

//                //    Edge newEdge = new Edge(p1, p2);

//                //    //Debug.Log("Add new edge");
//                //    //Gizmos.DrawLine(criticalPoints[0], criticalPoints[1]);

//                //    //Add this edge to whoever wants it
//                //    existingCell.edges.Add(newEdge);
//                //    newCell.edges.Add(newEdge);
//                //    edges.Add(newEdge);
//                //}

//                //Delete the edges we should delete
//                for (int l = 0; l < edgesToDelete.Count; l++)
//                {
//                    existingCell.edges.Remove(edgesToDelete[l]);
//                    edges.Remove(edgesToDelete[l]);
//                }


//                //Look for duplicate edges
//                //for (int l = 0; l < edges.Count; l++)
//                //{
//                //    if ((edges[l].v1.position - edges[l].v2.position).magnitude < 0.002f)
//                //    {
//                //        Debug.Log("Found duplicate edge");
//                //    }
//                //}


//                //Make sure the edges connect
//                for (int l = 0; l < edges.Count; l++)
//                {
//                    Vertex v1 = edges[l].v1;
//                    Vertex v2 = edges[l].v2;

//                    Vector3 closestToV1 = Vector3.zero;
//                    Vector3 closestToV2 = Vector3.zero;

//                    float closestDistanceToV1 = Mathf.Infinity;
//                    float closestDistanceToV2 = Mathf.Infinity;

//                    for (int m = 0; m < edges.Count; m++)
//                    {
//                        if (m == l)
//                        {
//                            continue;
//                        }

//                        if ((v1.position - edges[m].v1.position).sqrMagnitude < closestDistanceToV1)
//                        {
//                            closestDistanceToV1 = (v1.position - edges[m].v1.position).sqrMagnitude;

//                            closestToV1 = edges[m].v1.position;
//                        }
//                        if ((v1.position - edges[m].v2.position).sqrMagnitude < closestDistanceToV1)
//                        {
//                            closestDistanceToV1 = (v1.position - edges[m].v2.position).sqrMagnitude;

//                            closestToV1 = edges[m].v2.position;
//                        }

//                        if ((v2.position - edges[m].v1.position).sqrMagnitude < closestDistanceToV2)
//                        {
//                            closestDistanceToV2 = (v2.position - edges[m].v1.position).sqrMagnitude;

//                            closestToV2 = edges[m].v1.position;
//                        }
//                        if ((v2.position - edges[m].v2.position).sqrMagnitude < closestDistanceToV2)
//                        {
//                            closestDistanceToV2 = (v2.position - edges[m].v2.position).sqrMagnitude;

//                            closestToV2 = edges[m].v2.position;
//                        }

//                        v1.position = closestToV1;
//                        v2.position = closestToV2;
//                    }
//                }

//            }
            
//            cells.Add(newCell);
//        }



//        //
//        // Step 3. Connect the edges around each cell
//        //
//        //Remove the first 4 cells we added in the beginning because they are not needed anymore
//        cells.RemoveRange(0, 4);

//        for (int i = 0; i < cells.Count; i++)
//        {
//            //We should move around the cell counter-clockwise so make sure all edges are oriented in that way
//            List<Edge> cellEdges = cells[i].edges;

//            for (int j = cellEdges.Count - 1; j >= 0; j--)
//            {
//                Vertex edge_v1 = cellEdges[j].v1;
//                Vertex edge_v2 = cellEdges[j].v2;

//                //Remove this edge if it is small
//                //if ((edge_v1.position - edge_v2.position).sqrMagnitude < 0.01f)
//                //{
//                //    cellEdges.RemoveAt(j);

//                //    continue;
//                //}

//                Vector3 edgeCenter = (edge_v1.position + edge_v2.position) * 0.5f;

//                //Now we can make a line between the cell and the edge
//                Vector2 a = new Vector2(cells[i].cellPos.x, cells[i].cellPos.z);
//                Vector2 b = new Vector2(edgeCenter.x, edgeCenter.z);

//                //The point to the left of this line is coming after the other point if we are moving counter-clockwise
//                if (Geometry.IsAPointLeftOfVector(a, b, edge_v1.GetPos2D_XZ()))
//                {
//                    //Flip because we want to go from v1 to v2
//                    Vector3 temp = edge_v2.position;

//                    edge_v2.position = edge_v1.position;

//                    edge_v1.position = temp;
//                }
//            }



//            //Connect the edges
//            List<Vector3> edgesCoordinates = cells[i].borderCoordinates;

//            Edge startEdge = cellEdges[0];

//            edgesCoordinates.Add(startEdge.v2.position);

//            Vertex currentVertex = startEdge.v2;

//            for (int j = 1; j < cellEdges.Count; j++)
//            {
//                //Find the next edge
//                for (int k = 1; k < cellEdges.Count; k++)
//                {
//                    Vector3 thisEdgeStart = cellEdges[k].v1.position;

//                    //TODO Better to find the closest vertex and then remove it than using a tolerance because the  we know we will add all edges
//                    //In that case also only need to orient one vector to be counter-clockwise???
//                    if ((thisEdgeStart - currentVertex.position).sqrMagnitude < 0.01f)
//                    {
//                        edgesCoordinates.Add(cellEdges[k].v2.position);

//                        currentVertex = cellEdges[k].v2;

//                        break;
//                    }
//                }
//            }
//        }



//        //
//        // Step 4 clip edges
//        //
//        ClipDiagram(cells, halfWidth);




//        //
//        // Debug
//        //
//        for (int i = 0; i < cells.Count; i++)
//        {
//            //if (i != breakInt)
//            //{
//            //    break;
//            //}

//            Gizmos.color = Color.green;

//            Gizmos.DrawWireSphere(cells[i].cellPos, 0.04f);

//            Gizmos.color = Color.white;


//            //Show edges
//            //List<Edge> cellEdges = cells[i].edges;

//            //for (int j = 0; j < cellEdges.Count; j++)
//            //{
//            //    Gizmos.DrawLine(cellEdges[j].v1.position, cellEdges[j].v2.position);
//            //}

//            List<Vector3> cellCoordinates = cells[i].borderCoordinates;

//            for (int j = 0; j < cellCoordinates.Count; j++)
//            {
//                int jMinusOne = MathUtility.ClampListIndex(j - 1, cellCoordinates.Count);

//                Gizmos.DrawLine(cellCoordinates[jMinusOne], cellCoordinates[j]);
//            }

//            //Display in which order the vertices were added
//            //float vertexSize = 0.1f;

//            //Gizmos.color = Color.blue;

//            //for (int j = 0; j < cells[i].borderCoordinates.Count; j++)
//            //{
//            //    Gizmos.DrawSphere(cells[i].borderCoordinates[j], vertexSize);

//            //    vertexSize += 0.02f;
//            //}
//        }


//        return cells;
//    }



//    //Clip the Voronoi diagram with a clipping algorithm which is a better solution than the solution suggested on the webpage
//    private static void ClipDiagram(List<Cell> cells, float halfWidth)
//    {
//        List<Vector3> clipPolygon = new List<Vector3>();

//        //The positions of the square border
//        Vector3 TL = new Vector3(-halfWidth, 0f, halfWidth);
//        Vector3 TR = new Vector3(halfWidth, 0f, halfWidth);
//        Vector3 BR = new Vector3(halfWidth, 0f, -halfWidth);
//        Vector3 BL = new Vector3(-halfWidth, 0f, -halfWidth);

//        clipPolygon.Add(TL);
//        clipPolygon.Add(BL);
//        clipPolygon.Add(BR);
//        clipPolygon.Add(TR);

//        //Create the clipping planes
//        List<Plane> clippingPlanes = new List<Plane>();

//        for (int i = 0; i < clipPolygon.Count; i++)
//        {
//            int iPlusOne = MathUtility.ClampListIndex(i + 1, clipPolygon.Count);

//            Vector3 v1 = new Vector2(clipPolygon[i].x, clipPolygon[i].z);
//            Vector3 v2 = new Vector2(clipPolygon[iPlusOne].x, clipPolygon[iPlusOne].z);

//            //Doesnt have to be center but easier to debug
//            Vector2 planePos = (v1 + v2) * 0.5f;

//            Vector2 planeDir = v2 - v1;

//            //Should point inwards
//            Vector2 planeNormal = new Vector3(-planeDir.y, planeDir.x).normalized;

//            //Gizmos.DrawRay(planePos, planeNormal * 0.1f);

//            clippingPlanes.Add(new Plane(planePos, planeNormal));
//        }

//        for (int i = 0; i < cells.Count; i++)
//        {
//            //2d space
//            List<Vector2> borderCoordinates2D = new List<Vector2>();

//            for (int j = 0; j < cells[i].borderCoordinates.Count; j++)
//            {
//                Vector3 p = cells[i].borderCoordinates[j];

//                borderCoordinates2D.Add(new Vector2(p.x, p.z));
//            }

//            borderCoordinates2D = SutherlandHodgman.ClipPolygon(borderCoordinates2D, clippingPlanes);

//            cells[i].borderCoordinates.Clear();

//            //From 2d to 3d
//            for (int j = 0; j < cells[i].borderCoordinates.Count; j++)
//            {
//                Vector3 p2d = cells[i].borderCoordinates[j];

//                cells[i].borderCoordinates.Add(new Vector3(p2d.x, 0f, p2d.z));
//            }
//        }
//    }
//}
