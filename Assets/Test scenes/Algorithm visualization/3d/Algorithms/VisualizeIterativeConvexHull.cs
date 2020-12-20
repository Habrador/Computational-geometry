using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

public class VisualizeIterativeConvexHull : MonoBehaviour
{
    private VisualizerController3D controller;


    //These points should be normalized
    public void StartVisualizer(HashSet<MyVector3> points)
    {
        controller = GetComponent<VisualizerController3D>();

        HalfEdgeData3 convexHull = new HalfEdgeData3();

        //Generate the first tertahedron
        IterativeHullAlgorithm3D.BuildFirstTetrahedron(points, convexHull);
        

        //Main visualization algorithm
        StartCoroutine(GenerateHull(points, convexHull));
    }



    private IEnumerator GenerateHull(HashSet<MyVector3> points, HalfEdgeData3 convexHull)
    {
        //PAUSE FOR VISUALIZATION
        //Display what we have so far
        controller.DisplayMeshMain(convexHull.faces);
        controller.HideAllVisiblePoints(convexHull.verts);

        yield return new WaitForSeconds(10f);

        
        //Add all other points one-by-one
        List<MyVector3> pointsToAdd = new List<MyVector3>(points);

        foreach (MyVector3 p in pointsToAdd)
        {
            //Is this point within the tetrahedron
            bool isWithinHull = _Intersections.PointWithinConvexHull(p, convexHull);

            if (isWithinHull)
            {
                points.Remove(p);

                controller.HideVisiblePoint(p);

                continue;
            }


            //PAUSE FOR VISUALIZATION
            //Display active point
            controller.DisplayActivePoint(p);

            //Rotate camera to this point
            //Important to turn this vector to 2d
            Vector3 unity_pos = controller.normalizer.UnNormalize(p).ToVector3();

            controller.cameraScript.SetWantedHeight(unity_pos.y);

            unity_pos.y = 0f;

            controller.cameraScript.SetWantedDirection((Vector3.zero - unity_pos).normalized);

            yield return new WaitForSeconds(2f);


            //Find visible triangles and edges on the border between the visible and invisible triangles
            HashSet<HalfEdgeFace3> visibleTriangles = null;
            HashSet<HalfEdge3> borderEdges = null;

            IterativeHullAlgorithm3D.FindVisibleTrianglesAndBorderEdgesFromPoint(p, convexHull, out visibleTriangles, out borderEdges);

            //Remove all visible triangles
            foreach (HalfEdgeFace3 triangle in visibleTriangles)
            {
                convexHull.DeleteFace(triangle);
            }


            //PAUSE FOR VISUALIZATION
            //For visualization purposes we now need to create two meshes and then remove the triangles again
            controller.DisplayMeshMain(convexHull.faces);
            controller.DisplayMeshOther(visibleTriangles);
            controller.HideAllVisiblePoints(convexHull.verts);

            yield return new WaitForSeconds(2f);


            //PAUSE FOR VISUALIZATION
            //Remove all now visible triangles that forms the hole
            List<HalfEdgeFace3> visibleTrianglesList = new List<HalfEdgeFace3>(visibleTriangles);

            for (int i = 0; i < visibleTrianglesList.Count; i++)
            {
                visibleTriangles.Remove(visibleTrianglesList[i]);

                controller.DisplayMeshOther(visibleTriangles);

                yield return new WaitForSeconds(0.5f);
            }


            //Save all ned edges so we can connect them with an opposite edge
            //To make it faster you can use the ideas in the Valve paper to get a sorted list of newEdges
            HashSet<HalfEdge3> newEdges = new HashSet<HalfEdge3>();

            foreach (HalfEdge3 borderEdge in borderEdges)
            {
                //Each edge is point TO a vertex
                MyVector3 p1 = borderEdge.prevEdge.v.position;
                MyVector3 p2 = borderEdge.v.position;

                //The border edge belongs to a triangle which is invisible
                //Because triangles are oriented clockwise, we have to add the vertices in the other direction
                //to build a new triangle with the point
                HalfEdgeFace3 newTriangle = convexHull.AddTriangle(p2, p1, p);


                //PAUSE FOR VISUALIZATION
                controller.DisplayMeshMain(convexHull.faces);

                yield return new WaitForSeconds(0.5f);


                //Connect the new triangle with the opposite edge on the border
                //When we create the face we give it a reference edge which goes to p2
                //So the edge we want to connect is the next edge
                HalfEdge3 edgeToConnect = newTriangle.edge.nextEdge;

                edgeToConnect.oppositeEdge = borderEdge;
                borderEdge.oppositeEdge = edgeToConnect;

                //Two edges are still not connected, so save those
                HalfEdge3 e1 = newTriangle.edge;
                //HalfEdge3 e2 = newTriangle.edge.nextEdge;
                HalfEdge3 e3 = newTriangle.edge.nextEdge.nextEdge;

                newEdges.Add(e1);
                //newEdges.Add(e2);
                newEdges.Add(e3);
            }

            
            //Two edges in each triangle are still not connected with an opposite edge
            foreach (HalfEdge3 e in newEdges)
            {
                if (e.oppositeEdge != null)
                {
                    continue;
                }

                convexHull.TryFindOppositeEdge(e, newEdges);
            }


            //PAUSE FOR VISUALIZATION
            //controller.DisplayMeshMain(convexHull.faces);

            //yield return new WaitForSeconds(2f);
            controller.HideVisiblePoint(p);
        }


        controller.HideActivePoint();

        controller.cameraScript.SetWantedDirection(Vector3.zero);
        controller.cameraScript.SetWantedHeight(0f);

        //controller.DisplayMeshMain(convexHull.faces);

        //yield return new WaitForSeconds(5f);


        yield return null;
    }
}
