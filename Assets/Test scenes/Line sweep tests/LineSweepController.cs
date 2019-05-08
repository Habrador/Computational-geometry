//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class LineSweepController : MonoBehaviour 
//{
//    public int seed;


    
//    public void OnDrawGizmos()
//    {
//        //Generate random line segments, and hopefully some of these should intersect
//        List<Edge> lineSegments = new List<Edge>();

//        //Generate random numbers with a seed
//        System.Random random = new System.Random(seed);

//        float mapSize = 10f;

//        float max = mapSize;
//        float min = -mapSize;

//        for (int i = 0; i < 5; i++)
//        {
//            float randomX1 = MathUtility.TransformRange((float)random.NextDouble(), max, min);
//            float randomZ1 = MathUtility.TransformRange((float)random.NextDouble(), max, min);

//            Vertex v1 = new Vertex(new Vector3(randomX1, 0f, randomZ1));

//            float randomX2 = MathUtility.TransformRange((float)random.NextDouble(), max, min);
//            float randomZ2 = MathUtility.TransformRange((float)random.NextDouble(), max, min);

//            Vertex v2 = new Vertex(new Vector3(randomX2, 0f, randomZ2));

//            Edge e = new Edge(v1, v2);

//            lineSegments.Add(e);
//        }



//        //Find intersections
//        lineSegments = LineSweep.GetLineIntersections(lineSegments);



//        //
//        // Display edges
//        //
//        for (int i = 0; i < lineSegments.Count; i++)
//        {
//            Edge ls = lineSegments[i];

//            if(ls.isIntersecting)
//            {
//                Gizmos.color = Color.red;
//            }
//            else
//            {
//                Gizmos.color = Color.white;
//            }
        
//            Gizmos.DrawLine(ls.p1, ls.p2);
//        }
//    }
//}
