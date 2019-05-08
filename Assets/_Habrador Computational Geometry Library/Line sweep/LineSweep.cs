//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//public static class LineSweep
//{
//    //Find out if line segments in 2d space are intersecting
//    //Will set a bool to true if an input edge is intersecting
//    public static List<Edge> GetLineIntersections(List<Edge> lineSegments)
//    {
//        //Create a list of event points the line sweep will stop at and look if a line is intersecting
//        List<Vertex> eventPoints = new List<Vertex>(lineSegments.Count * 2);

//        for (int i = 0; i < lineSegments.Count; i++)
//        {
//            Vertex v1 = lineSegments[i].v1;
//            Vertex v2 = lineSegments[i].v2;

//            //Also make sure the vertex knows which line it is attached to
//            v1.edge = lineSegments[i];
//            v2.edge = lineSegments[i];

//            eventPoints.Add(v1);
//            eventPoints.Add(v2);
//        }

//        //Sort the event points from smallest to largest x coordinate
//        eventPoints = eventPoints.OrderBy(n => n.position.x).ToList();

//        //A list with active line segments the sweep line is currenty intersecting
//        List<Edge> activeLineSegments = new List<Edge>();

//        //Loop through all event points
//        for (int i = 0; i < eventPoints.Count; i++)
//        {
//            Vertex p = eventPoints[i];

//            //Each vertex has just one edge connected to it
//            Edge e = p.edge;

//            //Remove it from the list of active line segments if its an endpoint, so we dont compare with itself
//            bool isEndPoint = activeLineSegments.Contains(e);

//            if (isEndPoint)
//            {
//                activeLineSegments.Remove(e);
//            }

//            //Check for intersecting line segments in the list of active line segments
//            for (int j = 0; j < activeLineSegments.Count; j++)
//            {                
//                //Might find multiple intersection of the same line, so change in the future
//                if (IsEdgeEdgeIntersecting(e, activeLineSegments[j]))
//                {
//                    e.isIntersecting = true;
//                    activeLineSegments[j].isIntersecting = true;
//                }
//            }

//            //If this is a start point, then we we have found a new line and need to add it
//            if (!isEndPoint)
//            {
//                activeLineSegments.Add(e);
//            }
//        }

//        return lineSegments;
//    }



//    public static bool IsEdgeEdgeIntersecting(Edge e1, Edge e2)
//    {
//        Vector2 l1_p1 = e1.p1.XZ();
//        Vector2 l1_p2 = e1.p2.XZ();

//        Vector2 l2_p1 = e2.p1.XZ();
//        Vector2 l2_p2 = e2.p2.XZ();

//        bool isIntersecting = Intersections.AreLinesIntersecting(l1_p1, l1_p2, l2_p1, l2_p2, true);

//        return isIntersecting;
//    }
//}
