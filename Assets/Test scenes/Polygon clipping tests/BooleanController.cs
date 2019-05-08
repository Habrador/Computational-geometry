//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class BooleanController : MonoBehaviour
//{
//    public Transform meshATrans;
//    public Transform meshBTrans;

//    public Transform polyAParent;
//    public Transform polyBParent;

//    //Which of the boolean operations should we do on the triangles
//    public BooleanOperation booleanOperation;
	
	
	
//	void OnDrawGizmos() 
//	{
//        ////Triangles
//        //List<Triangle> originalTrianglesA = GetTrianglesFromMesh(meshATrans);
//        //List<Triangle> originalTrianglesB = GetTrianglesFromMesh(meshBTrans);

//        ////Experiment with just 2 triangles
//        //Triangle t1 = originalTrianglesA[0];
//        //Triangle t2 = originalTrianglesB[0];

//        ////The algortihms require polygons as input - not triangles
//        //List<Vector2> polygonOne_2d = new List<Vector2>();
//        //List<Vector2> polygonTwo_2d = new List<Vector2>();

//        //polygonOne_2d.Add(new Vector2(t1.v1.position.x, t1.v1.position.z));
//        //polygonOne_2d.Add(new Vector2(t1.v2.position.x, t1.v2.position.z));
//        //polygonOne_2d.Add(new Vector2(t1.v3.position.x, t1.v3.position.z));

//        //polygonTwo_2d.Add(new Vector2(t2.v1.position.x, t2.v1.position.z));
//        //polygonTwo_2d.Add(new Vector2(t2.v2.position.x, t2.v2.position.z));
//        //polygonTwo_2d.Add(new Vector2(t2.v3.position.x, t2.v3.position.z));

//        //List<Vector3> polygonOne = new List<Vector3>();
//        //List<Vector3> polygonTwo = new List<Vector3>();

//        //polygonOne.Add(t1.v1.position);
//        //polygonOne.Add(t1.v2.position);
//        //polygonOne.Add(t1.v3.position);

//        //polygonTwo.Add(t2.v1.position);
//        //polygonTwo.Add(t2.v2.position);
//        //polygonTwo.Add(t2.v3.position);


//        //Polygons
//        List<Vector3> polygonOne = GetVerticesFromParent(polyAParent);
//        List<Vector3> polygonTwo = GetVerticesFromParent(polyBParent);

//        //3d to 2d
//        List<Vector2> polygonOne2D = new List<Vector2>();
//        List<Vector2> polygonTwo2D = new List<Vector2>();

//        for (int i = 0; i < polygonOne.Count; i++)
//        {
//            polygonOne2D.Add(new Vector2(polygonOne[i].x, polygonOne[i].z));
//        }

//        for (int i = 0; i < polygonTwo.Count; i++)
//        {
//            polygonTwo2D.Add(new Vector2(polygonTwo[i].x, polygonTwo[i].z));
//        }



//        //
//        // Clipping algortihms
//        //

//        //Algortihm 1. Sutherland-Hodgman will return the intersection of the polygons, but can maybe be modified?
//        //Requires that the clipping polygon (the polygon we want to remove from the other polygon) is convex
//        //List<Vector2> polygonAfterClippingIntersection = SutherlandHodgman.ClipPolygon(polygonOne_2d, polygonTwo_2d);
//        //List<List<Vector2>> finalPolygon = SutherlandHodgman.BooleanOperations(polygonOne_2d, polygonTwo_2d, booleanOperation);

//        //Display the polygon
//        //TriangulateAndDisplayPolygon(finalPolygon, true);


//        //Alorithm 2. Greiner-Hormann. Can do all boolean operations on all types of polygons
//        //but fails when a vertex is on the other polygon's edge
//        List<List<Vector2>> finalPolygon = GreinerHormann.ClipPolygons(polygonOne2D, polygonTwo2D, booleanOperation);

//        //Debug.Log(finalPolygon.Count);

//        //Display the polygon
//        TriangulateAndDisplayPolygon(finalPolygon, false);




//        //
//        // Display
//        //
//        //if (triangles != null)
//        //{
//        //    MeshOperations.OrientTrianglesClockwise(triangles);

//        //    Mesh mesh = MeshOperations.GenerateMeshFromTriangles(triangles, transform);

//        //    Gizmos.DrawMesh(mesh);
//        //}

//        //Draw the polygons vertices
//        //float vertexSize = 0.05f;

//        //Gizmos.color = Color.white;
//        //for (int i = 0; i < polygonOne.Count; i++)
//        //{
//        //    Gizmos.DrawSphere(polygonOne[i], vertexSize);
//        //}

//        //Gizmos.color = Color.blue;
//        //for (int i = 0; i < polygonTwo.Count; i++)
//        //{
//        //    Gizmos.DrawSphere(polygonTwo[i], vertexSize);
//        //}

//        //Draw the polygons outlines
//        Gizmos.color = Color.white;
//        for (int i = 0; i < polygonOne.Count; i++)
//        {
//            int iPlusOne = MathUtility.ClampListIndex(i + 1, polygonOne.Count);

//            Gizmos.DrawLine(polygonOne[i], polygonOne[iPlusOne]);
//        }

//        Gizmos.color = Color.blue;
//        for (int i = 0; i < polygonTwo.Count; i++)
//        {
//            int iPlusOne = MathUtility.ClampListIndex(i + 1, polygonTwo.Count);

//            Gizmos.DrawLine(polygonTwo[i], polygonTwo[iPlusOne]);
//        }
//    }



//    //Triangulate a list with polygons
//    private void TriangulateAndDisplayPolygon(List<List<Vector2>> polygonList, bool arePolygonsConvex)
//    {
//        Gizmos.color = Color.red;

//        if (polygonList != null)
//        {
//            //2d to 3d space
//            List<List<Vector3>> polygonList_3d = new List<List<Vector3>>();

//            for (int i = 0; i < polygonList.Count; i++)
//            {
//                List<Vector2> poly = polygonList[i];

//                List<Vector3> poly_3d = new List<Vector3>();

//                polygonList_3d.Add(poly_3d);

//                for (int j = 0; j < poly.Count; j++)
//                {
//                    Vector3 pos = new Vector3(poly[j].x, 0f, poly[j].y);

//                    poly_3d.Add(pos);
//                }

//                //break;
//            }

//            //Triangulate and display the polygons as mesh
//            for (int i = 0; i < polygonList_3d.Count; i++)
//            {
//                //Debug.Log(polygonAfterClipping_3d[i].Count);

//                if (polygonList_3d[i].Count > 0)
//                {
//                    List<Triangle> triangles = null;

//                    if (arePolygonsConvex)
//                    {
//                        triangles = TriangulateHullAlgorithms.TriangulateConvexPolygon(polygonList_3d[i]);
//                    }
//                    else
//                    {
//                        //triangles = TriangulateHullAlgorithms.TriangulateConcavePolygon(polygonList_3d[i]);
//                    }
                    
//                    if (triangles != null)
//                    {
//                        //Mesh mesh = MeshOperations.GenerateMeshFromTriangles(triangles, transform);

//                        //Gizmos.DrawMesh(mesh);
//                    }
//                }
//            }

//            //Show the vertices
//            //for (int i = 0; i < polygonList_3d.Count; i++)
//            //{
//            //    List<Vector3> poly = polygonList_3d[i];

//            //    //float size = 0.02f;

//            //    for (int j = 0; j < poly.Count; j++)
//            //    {
//            //        Gizmos.DrawWireSphere(poly[j], 0.05f);
//            //    }
//            //}

//            //Show just one part of the polygon
//            //List<Vector3> poly_temp = polygonAfterClipping_3d[0];

//            ////Debug.Log(poly_temp.Count);
//            //float size = 0.02f;
//            //for (int j = 0; j < poly_temp.Count; j++)
//            //{
//            //    Gizmos.DrawWireSphere(poly_temp[j], size);

//            //    size += 0.02f;
//            //}
//        }
//    }



//    //Get child vertices from parent trans
//    private List<Vector3> GetVerticesFromParent(Transform parent)
//    {
//        int childCount = parent.childCount;

//        List<Vector3> children = new List<Vector3>();

//        for (int i = 0; i < childCount; i++)
//        {
//            children.Add(parent.GetChild(i).position);
//        }

//        return children;
//    }



//    //Generates a list with triangles belonging to a mesh
//    //The triangles are in global space
//    private List<TriangleOld> GetTrianglesFromMesh(Transform meshTrans)
//    {
//        List<TriangleOld> listWithTriangles = new List<TriangleOld>();

//        Mesh mesh = meshTrans.GetComponent<MeshFilter>().sharedMesh;
    
//        int[] trianglePositions = mesh.triangles;

//        Vector3[] vertices = mesh.vertices;

//        for (int i = 0; i < trianglePositions.Length; i+=3)
//        {
//            Vector3 v1 = vertices[trianglePositions[i + 0]];
//            Vector3 v2 = vertices[trianglePositions[i + 1]];
//            Vector3 v3 = vertices[trianglePositions[i + 2]];

//            //Local to global
//            v1 = meshTrans.TransformPoint(v1);
//            v2 = meshTrans.TransformPoint(v2);
//            v3 = meshTrans.TransformPoint(v3);

//            TriangleOld triangle = new TriangleOld(new Vertex(v1), new Vertex(v2), new Vertex(v3));

//            //Make sure you know which orientation the triangles have
//            if (Geometry.IsTriangleOrientedClockwise(triangle.v1.GetPos2D_XZ(), triangle.v2.GetPos2D_XZ(), triangle.v3.GetPos2D_XZ()))
//            {
//                triangle.ChangeOrientation();
//            }

//            listWithTriangles.Add(triangle);
//        }

//        return listWithTriangles;
//    }
//}
