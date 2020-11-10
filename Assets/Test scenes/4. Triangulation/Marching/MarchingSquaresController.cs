using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;


//Generate a mesh by using the Marching Squares Algorithm
public class MarchingSquaresController : MonoBehaviour 
{
    //The size of the map
    public int mapSizeX;
    public int mapSizeZ;
    //The size of a square in the map
    public float squareSize;

    //Used to generate test data
    [Range(0, 100)]
    public int randomFillPercent;
    //To get the same test data
    public int seed;

    //So we can display the map in OnDrawGizmos
    private float[,] map;

    private Habrador_Computational_Geometry.Marching_Squares.SquareGrid grid;




    public void GenerateMap()
    {
        if (squareSize <= 0f)
        {
            Debug.LogError("Square size has to be greate than 0");
        
            return;
        }
    
        int squaresX = Mathf.FloorToInt(mapSizeX / squareSize);
        int squaresZ = Mathf.FloorToInt(mapSizeZ / squareSize);

        map = new float[squaresX, squaresZ];

        FillMapRandomly();

        //Generate the mesh with marching squares algorithm
        grid = MarchingSquares.GenerateMesh(map, squareSize, shouldSmooth: false);
    }



    //Fill the map randomly
    private void FillMapRandomly()
    {
        Random.InitState(seed);

        int squaresX = map.GetLength(0);
        int squaresZ = map.GetLength(1);

        for (int x = 0; x < squaresX; x++)
        {
            for (int z = 0; z < squaresZ; z++)
            {
                map[x, z] = (Random.Range(0f, 100f) < randomFillPercent) ? 1f : 0f;
            }
        }
    }



    //Debug
    private void OnDrawGizmos()
    {
        //Blue means solid, red means empty
        DisplayMap();

        DisplayGeneratedMesh();

        DisplayContourEdges();

        //Blue means solid, red means empty
        DisplayMarchingSquaresData();
    }



    private void DisplayMap()
    {
        if (map == null)
        {
            return;
        }


        int nodeCountX = map.GetLength(0);
        int nodeCountZ = map.GetLength(1);

        float halfMapWidthX = nodeCountX * squareSize * 0.5f;
        float halfMapWidthZ = nodeCountZ * squareSize * 0.5f;

        float halfSquareSize = squareSize * 0.5f;

        for (int x = 0; x < nodeCountX; x++)
        {
            for (int z = 0; z < nodeCountZ; z++)
            {
                Gizmos.color = (map[x, z] == 1) ? Color.blue : Color.red;

                float xPos = -halfMapWidthX + x * squareSize + halfSquareSize;
                float zPos = -halfMapWidthZ + z * squareSize + halfSquareSize;

                Vector3 pos = new Vector3(xPos, 0f, zPos);

                Gizmos.DrawCube(pos, Vector3.one * squareSize * 0.9f);
            }
        }
    }



    private void DisplayMarchingSquaresData()
    {
        if (grid == null)
        {
            return;
        }


        int xLength = grid.squares.GetLength(0);
        int zLength = grid.squares.GetLength(1);

        for (int x = 0; x < xLength; x++)
        {
            for (int z = 0; z < zLength; z++)
            {
                Habrador_Computational_Geometry.Marching_Squares.Square square = grid.squares[x, z];

                float sphereRadius = 0.05f;

                Gizmos.color = square.TL.isActive ? Color.blue : Color.red;
                Gizmos.DrawSphere(square.TL.pos.ToVector3(), sphereRadius);

                Gizmos.color = square.TR.isActive ? Color.blue : Color.red;
                Gizmos.DrawSphere(square.TR.pos.ToVector3(), sphereRadius);

                Gizmos.color = square.BL.isActive ? Color.blue : Color.red;
                Gizmos.DrawSphere(square.BL.pos.ToVector3(), sphereRadius);

                Gizmos.color = square.BR.isActive ? Color.blue : Color.red;
                Gizmos.DrawSphere(square.BR.pos.ToVector3(), sphereRadius);


                //Gizmos.color = Color.green;

                //Gizmos.DrawSphere(square.T.pos, 0.1f);
                //Gizmos.DrawSphere(square.L.pos, 0.1f);
                //Gizmos.DrawSphere(square.B.pos, 0.1f);
                //Gizmos.DrawSphere(square.R.pos, 0.1f);
            }
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
