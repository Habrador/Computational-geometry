using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

//MetaBalls in 2d space (= MetaCircles) by using the Marching Squares algorithm
//Based on http://jamie-wong.com/2014/08/19/metaballs-and-marching-squares/
public class MetaCirclesController : MonoBehaviour
{
    public GameObject circleParent;

    public int seed;

    //The size of the map
    public int mapSize;

    //The size of an individual square in the map
    public float squareSize = 0.5f;

    //Factor determining shape of the metaballs, should be around 1
    public float metaballFactor = 1f;

    //So we can display the map in OnDrawGizmos
    private float[,] map;

    private Habrador_Computational_Geometry.Marching_Squares.SquareGrid grid;

    private float minCircleSize = 1f;
    private float maxCircleSize = 4f;



    public void GenerateMap()
    {
        if (squareSize <= 0f)
        {
            Debug.LogError("Square size has to be greate than 0");

            return;
        }

        int squares = Mathf.FloorToInt(mapSize / squareSize);
    
        map = new float[squares, squares];
        
        FillMap();

        //Generate the mesh with marching squares algorithm
        grid = MarchingSquares.GenerateMesh(map, squareSize, shouldSmooth: true);
    }



    private void FillMap()
    {
        if (circleParent == null || circleParent.transform.childCount <= 0)
        {
            return;
        }

        Transform[] circles = circleParent.GetComponentsInChildren<Transform>();

        float[] circleRadiuses = new float[circles.Length];


        Random.InitState(seed);

        //First one is always the parent itself so ignore it
        for (int i = 1; i < circles.Length; i++)
        {
            float radius = Random.Range(minCircleSize, maxCircleSize);

            circleRadiuses[i] = radius;
        }
        //Debug.Log(circleRadiuses[1]);

        //Loop through all cells and check if they are within the radius of one of the circles
        int xLength = map.GetLength(0);
        int zLength = map.GetLength(1);

        for (int x = 0; x < xLength; x++)
        {
            for (int z = 0; z < zLength; z++)
            {
                float cellCenterX = -mapSize * 0.5f + x * squareSize + squareSize * 0.5f;

                float cellCenterZ = -mapSize * 0.5f + z * squareSize + squareSize * 0.5f;

                Vector3 pos = new Vector3(cellCenterX, 0f, cellCenterZ);


                //Loop through all circles
                //for (int i = 1; i < circles.Length; i++)
                //{
                //    Vector3 circlePos = circles[i].position;

                //    float radius = circleRadiuses[i];

                //    //Fill the squares that are within the circle
                //    if ((pos - circlePos).sqrMagnitude < radius * radius)
                //    {
                //        map[x, z] = 1;

                //        break;
                //    }
                //}


                //Fill the circles metaball style
                float sum = 0f;

                for (int i = 1; i < circles.Length; i++)
                {
                    Vector3 circlePos = circles[i].position;

                    float radius = circleRadiuses[i];

                    sum += (radius * radius) / (Mathf.Pow((pos.x - circlePos.x), 2f) + Mathf.Pow((pos.z - circlePos.z), 2f));
                }

                //if (sum >= metaballFactor)
                //{
                //    map[x, z] = 1;
                //}

                map[x, z] = sum;
            }
        }
    }



    private void OnDrawGizmos()
    {
        //Blue means solid, red means empty
        //DisplayMap();

        DisplayCircles();

        DisplayGeneratedMesh();

        //DisplayContourEdges();
    }



    private void DisplayMap()
    {
        if (map == null)
        {
            return;
        }


        int xLength = map.GetLength(0);
        int zLength = map.GetLength(1);

        float halfMapSizeX = xLength * 0.5f * squareSize;
        float halfMapSizeZ = zLength * 0.5f * squareSize;

        float halfSquareSize = squareSize * 0.5f;

        for (int x = 0; x < xLength; x++)
        {
            for (int z = 0; z < zLength; z++)
            {
                Gizmos.color = (map[x, z] == 1) ? Color.blue : Color.red;

                float cellCenterX = -halfMapSizeX + x * squareSize + halfSquareSize;

                float cellCenterZ = -halfMapSizeZ + z * squareSize + halfSquareSize;

                Vector3 pos = new Vector3(cellCenterX, 0f, cellCenterZ);

                Gizmos.DrawCube(pos, Vector3.one * squareSize * 0.9f);
            }
        }
    }


    
    private void DisplayCircles()
    {
        if (circleParent == null || circleParent.transform.childCount <= 0)
        {
            return;
        }
    
        Transform[] circles = circleParent.GetComponentsInChildren<Transform>();

        Random.InitState(seed);

        Gizmos.color = Color.white;

        //First one is always the parent itself so ignore it
        for (int i = 1; i < circles.Length; i++)
        {
            float radius = Random.Range(minCircleSize, maxCircleSize);

            Gizmos.DrawWireSphere(circles[i].position, radius);
        }
    }



    //The mesh we generate with the Marching Squares algorithm
    private void DisplayGeneratedMesh()
    {
        if (grid == null)
        {
            return;
        }


        Mesh mesh = grid.GenerateUnityMesh(0f);

        //TestAlgorithmsHelpMethods.DisplayMesh(mesh, Color.black);
        TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors(mesh, 0);
    }



    private void DisplayContourEdges()
    {
        if (grid == null)
        {
            return;
        }

        List<Edge2> edges = grid.contourEdges;

        Gizmos.color = Color.white;

        foreach (Edge2 e in edges)
        {
            Gizmos.DrawLine(e.p1.ToVector3(), e.p2.ToVector3());
        }
    }
}
