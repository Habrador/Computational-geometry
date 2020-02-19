using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;



public class PolygonClippingController : MonoBehaviour
{
    public Transform polyAParent;
    public Transform polyBParent;



    void OnDrawGizmos()
    {
        //Generate the polygons
        List<Vector3> polygonA = GetVerticesFromParent(polyAParent);
        List<Vector3> polygonB = GetVerticesFromParent(polyBParent);

        //3d to 2d
        List<Vector2> polygonA_2D = HelpMethods.ConvertListFrom3DTo2D(polygonA);
        List<Vector2> polygonB_2D = HelpMethods.ConvertListFrom3DTo2D(polygonB);

        //Display the original polygons
        DisplayPolygon(polygonA, Color.white);
        DisplayPolygon(polygonB, Color.blue);



        List<Vector2> poly = polygonB_2D;
        List<Vector2> clipPoly = polygonA_2D;

        //Clipping algortihms
        //Algortihm 1.Sutherland-Hodgman will return the intersection of the polygons
        //Requires that the clipping polygon (the polygon we want to remove from the other polygon) is convex
        //TestSutherlandHodgman(poly, clipPoly);



        //Alorithm 2. Greiner-Hormann. Can do all boolean operations on all types of polygons
        //but fails when a vertex is on the other polygon's edge
        TestGreinerHormann(poly, clipPoly);
    }



    private void TestSutherlandHodgman(List<Vector2> poly, List<Vector2> clipPoly)
    {
        List<Vector2> polygonAfterClipping = SutherlandHodgman.ClipPolygon(poly, clipPoly);

        //2d to 3d
        List<Vector3> polygonAfterClipping3D = HelpMethods.ConvertListFrom2DTo3D(polygonAfterClipping);

        DisplayPolygon(polygonAfterClipping3D, Color.red);
    }



    private void TestGreinerHormann(List<Vector2> poly, List<Vector2> clipPoly)
    {
        //In this case we can get back multiple parts of the polygon because one of the 
        //polygons doesnt have to be convex
        List<List<Vector2>> finalPolygon = GreinerHormann.ClipPolygons(poly, clipPoly, BooleanOperation.Intersection);

        for (int i = 0; i < finalPolygon.Count; i++)
        {
            //2d to 3d
            List<Vector3> polygonAfterClipping3D = HelpMethods.ConvertListFrom2DTo3D(finalPolygon[i]);

            DisplayPolygon(polygonAfterClipping3D, Color.red);
        }
    }



    //Display one polygon's vertices and lines between the vertices
    private void DisplayPolygon(List<Vector3> vertices, Color color)
    {
        Gizmos.color = color;

        //Draw the polygons vertices
        float vertexSize = 0.05f;

        for (int i = 0; i < vertices.Count; i++)
        {
            Gizmos.DrawSphere(vertices[i], vertexSize);
        }

        //Draw the polygons outlines
        for (int i = 0; i < vertices.Count; i++)
        {
            int iPlusOne = MathUtility.ClampListIndex(i + 1, vertices.Count);

            Gizmos.DrawLine(vertices[i], vertices[iPlusOne]);
        }
    }



    //Get child vertex positions from parent trans
    private List<Vector3> GetVerticesFromParent(Transform parent)
    {
        int childCount = parent.childCount;

        List<Vector3> children = new List<Vector3>();

        for (int i = 0; i < childCount; i++)
        {
            children.Add(parent.GetChild(i).position);
        }

        return children;
    }
}
