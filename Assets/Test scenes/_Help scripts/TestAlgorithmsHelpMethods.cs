using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;



//Display meshes, points, etc so we dont have to do it in each file
public static class TestAlgorithmsHelpMethods
{
    //
    // Common help methods
    //

    //Get all child points to a parent transform
    public static List<Vector3> GetPointsFromParent(Transform parentTrans)
    {
        if (parentTrans == null)
        {
            Debug.Log("No parent so cant get children");

            return null;
        }

        //Is not including the parent
        int children = parentTrans.childCount;

        List<Vector3> childrenPositions = new List<Vector3>();

        for (int i = 0; i < children; i++)
        {
            childrenPositions.Add(parentTrans.GetChild(i).position);
        }

        return childrenPositions;
    }



    //
    // Display shapes with Gizmos
    //

    //Display some points
    public static void DisplayPoints(HashSet<Vector3> points, float radius, Color color)
    {
        if (points == null)
        {
            return;
        }
    
        Gizmos.color = color;

        foreach (Vector3 p in points)
        {
            Gizmos.DrawSphere(p, radius);
        }
    }



    //Display an arrow at the end of vector from a to b
    public static void DisplayArrow(Vector3 a, Vector3 b, float size, Color color)
    {
        //We also need to know the direction of the vector, so we need to draw a small arrow
        Vector3 vecDir = (b - a).normalized;

        Vector3 vecDirPerpendicular = new Vector3(vecDir.z, 0f, -vecDir.x);

        Vector3 arrowTipStart = b - vecDir * size;

        //Draw the arrows 4 lines
        Gizmos.color = color;

        //Arrow tip
        Gizmos.DrawLine(arrowTipStart, arrowTipStart + vecDirPerpendicular * size);
        Gizmos.DrawLine(arrowTipStart + vecDirPerpendicular * size, b);
        Gizmos.DrawLine(b, arrowTipStart - vecDirPerpendicular * size);
        Gizmos.DrawLine(arrowTipStart - vecDirPerpendicular * size, arrowTipStart);

        //Arrow line
        Gizmos.DrawLine(a, arrowTipStart);
    }



    //Display triangle
    public static void DisplayTriangleEdges(Vector3 a, Vector3 b, Vector3 c, Color color)
    {
        Gizmos.color = color;

        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(b, c);
        Gizmos.DrawLine(c, a);
    }



    //Display a plane
    public static void DrawPlane(MyVector2 planePos_2d, MyVector2 planeNormal, Color color)
    {
        Vector3 planeDir = new Vector3(planeNormal.y, 0f, -planeNormal.x);

        Vector3 planePos = planePos_2d.ToVector3();

        //Draw the plane which is just a long line
        float infinite = 100f;

        Gizmos.color = color;

        Gizmos.DrawRay(planePos, planeDir * infinite);
        Gizmos.DrawRay(planePos, -planeDir * infinite);

        //Draw the plane normal
        Gizmos.DrawLine(planePos, planePos + planeNormal.ToVector3() * 1f);
    }



    //Display the edges of a mesh's triangles with some color
    public static void DisplayMeshEdges(Mesh mesh, Color sideColor)
    {
        if (mesh == null)
        {
            return;
        }

        //Display the triangles with a random color
        int[] meshTriangles = mesh.triangles;

        Vector3[] meshVertices = mesh.vertices;

        for (int i = 0; i < meshTriangles.Length; i += 3)
        {
            Vector3 p1 = meshVertices[meshTriangles[i + 0]];
            Vector3 p2 = meshVertices[meshTriangles[i + 1]];
            Vector3 p3 = meshVertices[meshTriangles[i + 2]];

            Gizmos.color = sideColor;

            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p1);
        }
    }



    //Display a connected set of points, like a convex hull
    //Can also show direction by displaying a tiny arrow
    public static void DisplayConnectedPoints(List<Vector3> points, Color color, bool showDirection = false)
    {
        if (points == null)
        {
            return;
        }

        Gizmos.color = color;

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 p1 = points[MathUtility.ClampListIndex(i - 1, points.Count)];
            Vector3 p2 = points[MathUtility.ClampListIndex(i + 0, points.Count)];

            //Direction is important so we should display an arrow show the order of the points
            if (i == 0 && showDirection)
            {
                TestAlgorithmsHelpMethods.DisplayArrow(p1, p2, 0.2f, color);
            }
            else
            {
                Gizmos.DrawLine(p1, p2);
            }

            Gizmos.DrawWireSphere(p1, 0.1f);
        }
    }



    //
    // Display shapes with Mesh
    //

    //Display some mesh where each triangle could have a random color
    private static void DisplayMesh(Mesh mesh, bool useRandomColor, int seed, Color meshColor)
    {
        if (mesh == null)
        {
            Debug.Log("Cant display the mesh because there's no mesh!");
        
            return;
        }

        //Display the entire mesh with a single color
        if (!useRandomColor)
        {
            Gizmos.color = meshColor;

            mesh.RecalculateNormals();

            Gizmos.DrawMesh(mesh);
        }
        //Display the individual triangles with a random color
        else
        {
            int[] meshTriangles = mesh.triangles;

            Vector3[] meshVertices = mesh.vertices;

            Random.InitState(seed);

            for (int i = 0; i < meshTriangles.Length; i += 3)
            {
                //Make a single mesh triangle
                Vector3 p1 = meshVertices[meshTriangles[i + 0]];
                Vector3 p2 = meshVertices[meshTriangles[i + 1]];
                Vector3 p3 = meshVertices[meshTriangles[i + 2]];

                Mesh triangleMesh = new Mesh();

                triangleMesh.vertices = new Vector3[] { p1, p2, p3 };

                triangleMesh.triangles = new int[] { 0, 1, 2 };

                triangleMesh.RecalculateNormals();


                //Color the triangle
                Gizmos.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);

                //float grayScale = Random.Range(0f, 1f);

                //Gizmos.color = new Color(grayScale, grayScale, grayScale, 1f);


                //Display it
                Gizmos.DrawMesh(triangleMesh);
            }
        }
    }

    //Just one color
    public static void DisplayMesh(Mesh mesh, Color meshColor)
    {
        int seed = 0;
    
        DisplayMesh(mesh, false, seed, meshColor);
    }

    //Random color
    //Seed is determining the random color
    public static void DisplayMeshWithRandomColors(Mesh mesh, int seed)
    {
        DisplayMesh(mesh, true, seed, Color.black);
    }



    //Connected list of points
    public static void DisplayConnectedLinesMesh(List<MyVector2> points, float lineWidth, Color color)
    {
        HashSet<Triangle2> triangles = _GenerateMesh.ConnectedLineSegments(points, lineWidth, isConnected: true);
       
        Mesh mesh = _TransformBetweenDataStructures.Triangles2ToMesh(triangles, false);
        
        TestAlgorithmsHelpMethods.DisplayMesh(mesh, color);
    }



    //Corners in a mesh
    public static void DisplayMeshCorners(Mesh mesh, float radius, Color color)
    {
        Vector3[] vertices = mesh.vertices;

        Gizmos.color = color;

        foreach(Vector3 v in vertices)
        {
            Gizmos.DrawSphere(v, radius);
        }
    }



    //Circle
    public static void DisplayCircleMesh(MyVector2 center, float radius, int resolution, Color color)
    {
        HashSet<Triangle2> triangles = _GenerateMesh.Circle(center, radius, resolution);

        Mesh mesh = _TransformBetweenDataStructures.Triangles2ToMesh(triangles, false);

        TestAlgorithmsHelpMethods.DisplayMesh(mesh, color);
    }



    //Line
    public static void DisplayLineMesh(MyVector2 a, MyVector2 b, float width, Color color)
    {
        HashSet<Triangle2> triangles = _GenerateMesh.LineSegment(a, b, width);

        Mesh mesh = _TransformBetweenDataStructures.Triangles2ToMesh(triangles, false);

        TestAlgorithmsHelpMethods.DisplayMesh(mesh, color);
    }



    //Plane
    public static void DisplayPlaneMesh(MyVector2 pos, MyVector2 normal, float width, Color color)
    {
        MyVector2 planeDir = new MyVector2(normal.y, -normal.x);

        //Draw the plane which is just a long line
        float infinite = 100f;

        //Draw a loooong line to show an infinite plane
        MyVector2 a = pos + planeDir * infinite;
        MyVector2 b = pos - planeDir * infinite;

        HashSet<Triangle2> triangles = _GenerateMesh.LineSegment(a, b, width);

        Mesh mesh = _TransformBetweenDataStructures.Triangles2ToMesh(triangles, false);

        TestAlgorithmsHelpMethods.DisplayMesh(mesh, color);

        //Display the normal with an arrow
        float arrowLength = 4f;

        float arrowSize = width + 0.5f;

        DisplayArrowMesh(pos, pos + normal * arrowLength, width, arrowSize, color);
    }



    //Arrow
    public static void DisplayArrowMesh(MyVector2 a, MyVector2 b, float width, float arrowSize, Color color)
    {
        HashSet<Triangle2> triangles = _GenerateMesh.Arrow(a, b, width, arrowSize);

        Mesh mesh = _TransformBetweenDataStructures.Triangles2ToMesh(triangles, false);

        TestAlgorithmsHelpMethods.DisplayMesh(mesh, color);
    }



    //Triangle
    public static void DisplayTriangleMesh(MyVector2 a, MyVector2 b, MyVector2 c, Color color)
    {
        Triangle2 t = new Triangle2(a, b, c);

        HashSet<Triangle2> triangles = new HashSet<Triangle2>();

        triangles.Add(t);

        triangles = HelpMethods.OrientTrianglesClockwise(triangles);

        Mesh mesh = _TransformBetweenDataStructures.Triangles2ToMesh(triangles, false);

        TestAlgorithmsHelpMethods.DisplayMesh(mesh, color);
    }


    //
    // Generate points
    //

    //Find all vertices of a "Plane" (which is Unitys predefined mesh called plane)
    public static HashSet<Vector3> GeneratePointsFromPlane(Transform planeTrans)
    {
        HashSet<Vector3> points = new HashSet<Vector3>();

        Mesh mesh = planeTrans.GetComponent<MeshFilter>().sharedMesh;

        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPos = planeTrans.TransformPoint(vertices[i]);

            points.Add(worldPos);
        }

        return points;
    }



    //Generate random points within a square located at (0,0), so 2d space
    public static HashSet<Vector2> GenerateRandomPoints2D(int seed, float halfSquareSize, int numberOfPoints)
    {
        HashSet<Vector2> randomPoints = new HashSet<Vector2>();

        //Generate random numbers with a seed
        Random.InitState(seed);

        float max = halfSquareSize;
        float min = -halfSquareSize;

        for (int i = 0; i < numberOfPoints; i++)
        {
            float randomX = Random.Range(min, max);
            float randomY = Random.Range(min, max);

            randomPoints.Add(new Vector2(randomX, randomY));
        }

        return randomPoints;
    }


    //Generate random points within a cube located at (0,0,0), so 3d space
    public static HashSet<Vector3> GenerateRandomPoints3D(int seed, float halfCubeSize, int numberOfPoints)
    {
        HashSet<Vector3> randomPoints = new HashSet<Vector3>();

        //Generate random numbers with a seed
        Random.InitState(seed);

        float max = halfCubeSize;
        float min = -halfCubeSize;

        for (int i = 0; i < numberOfPoints; i++)
        {
            float randomX = Random.Range(min, max);
            float randomY = Random.Range(min, max);
            float randomZ = Random.Range(min, max);

            randomPoints.Add(new Vector3(randomX, randomY, randomZ));
        }

        return randomPoints;
    }


    //Generate random points on a sphere located at (0,0,0)
    public static HashSet<Vector3> GenerateRandomPointsOnSphere(int seed, float radius, int numberOfPoints)
    {
        HashSet<Vector3> randomPoints = new HashSet<Vector3>();

        //Generate random numbers with a seed
        Random.InitState(seed);

        for (int i = 0; i < numberOfPoints; i++)
        {
            Vector3 posOnSphere = Random.onUnitSphere * radius;

            randomPoints.Add(posOnSphere);
        }

        return randomPoints;
    }



    //
    // Display shapes with Debug.DrawLine()
    //

    //Display a circle, which doesnt exist built-in - only DrawLine and DrawRay
    public static void DebugDrawCircle(Vector3 center, float radius, Color color)
    {
        Vector3 pos = center + Vector3.right * radius;

        int segments = 12;

        float anglePerSegment = (Mathf.PI * 2f) / (float)segments;

        float angle = anglePerSegment;

        for (int i = 0; i < segments; i++)
        {        
            float nextPosX = center.x + Mathf.Cos(angle) * radius;
            float nextPosZ = center.z + Mathf.Sin(angle) * radius;

            Vector3 nextPos = new Vector3(nextPosX, center.y, nextPosZ);

            Debug.DrawLine(pos, nextPos, color, 2f);

            pos = nextPos;

            angle += anglePerSegment;
        }
    }


    //Display a circle in 3d
    public static void DebugDrawCircle3D(Vector3 center, float radius, Color color)
    {
        Vector3 posR = center + Vector3.right * radius;
        Vector3 posF = center + Vector3.forward * radius;
        Vector3 posU = center + Vector3.right * radius;

        int segments = 12;

        float anglePerSegment = (Mathf.PI * 2f) / (float)segments;

        float angle = anglePerSegment;

        for (int i = 0; i < segments; i++)
        {
            float nextPosX_R = center.x + Mathf.Cos(angle) * radius;
            float nextPosZ_R = center.z + Mathf.Sin(angle) * radius;

            Vector3 nextPosR = new Vector3(nextPosX_R, center.y, nextPosZ_R);

            float nextPosZ_F = center.z + Mathf.Cos(angle) * radius;
            float nextPosY_F = center.y + Mathf.Sin(angle) * radius;

            Vector3 nextPosF = new Vector3(center.x, nextPosY_F, nextPosZ_F);

            float nextPosX_U = center.x + Mathf.Cos(angle) * radius;
            float nextPosY_U = center.y + Mathf.Sin(angle) * radius;

            Vector3 nextPosU = new Vector3(nextPosX_U, nextPosY_U, center.z);

            Debug.DrawLine(posR, nextPosR, color, 2f);
            Debug.DrawLine(posF, nextPosF, color, 2f);
            Debug.DrawLine(posU, nextPosU, color, 2f);

            posR = nextPosR;
            posF = nextPosF;
            posU = nextPosU;

            angle += anglePerSegment;
        }
    }



    //Display a triangle with a normal at the center
    public static void DebugDrawTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal, Color lineColor, Color normalColor)
    {
        Debug.DrawLine(p1, p2, lineColor, 2f);
        Debug.DrawLine(p2, p3, lineColor, 2f);
        Debug.DrawLine(p3, p1, lineColor, 2f);

        Vector3 center = _Geometry.CalculateTriangleCenter(p1.ToMyVector3(), p2.ToMyVector3(), p3.ToMyVector3()).ToVector3();

        Debug.DrawLine(center, center + normal, normalColor, 2f);
    }



    //Display a face which we know is a triangle with its normal at the center
    public static void DebugDrawTriangle(HalfEdgeFace3 f, Color lineColor, Color normalColor, Normalizer3 normalizer = null)
    {
        MyVector3 p1 = f.edge.v.position;
        MyVector3 p2 = f.edge.nextEdge.v.position;
        MyVector3 p3 = f.edge.nextEdge.nextEdge.v.position;

        if (normalizer != null)
        {
            p1 = normalizer.UnNormalize(p1);
            p2 = normalizer.UnNormalize(p2);
            p3 = normalizer.UnNormalize(p3);
        }

        Vector3 normal = f.edge.v.normal.ToVector3();

        TestAlgorithmsHelpMethods.DebugDrawTriangle(p1.ToVector3(), p2.ToVector3(), p3.ToVector3(), normal * 0.5f, Color.white, Color.red);

        //Debug.Log("Displayed Triangle");

        //To test the the triangle is clock-wise
        //TestAlgorithmsHelpMethods.DebugDrawCircle(p1_test, 0.1f, Color.red);
        //TestAlgorithmsHelpMethods.DebugDrawCircle(p2_test, 0.2f, Color.blue);
    }



    //
    // Display data structures
    //
    
    public static void DisplayMyVector3(MyVector3 v)
    {
        Debug.Log($"({v.x}, {v.y}, {v.z})");
    }
}
