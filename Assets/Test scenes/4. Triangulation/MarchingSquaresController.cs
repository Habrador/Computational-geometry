using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;


//Generate a mesh by using the Marching Squares Algorithm
public class MarchingSquaresController : MonoBehaviour 
{
    public int mapSizeX;
    public int mapSizeZ;

    //Used to generate test data
    [Range(0, 100)]
    public int randomFillPercent;
    //To get the same test data
    public int seed;

    //So we can display the map in OnDrawGizmos
    private int[,] map;

    private Habrador_Computational_Geometry.Marching_Squares.SquareGrid grid;




    public void GenerateMap()
    {
        map = new int[mapSizeX, mapSizeZ];

        FillMapRandomly();

        //Generate the mesh with marching squares algorithm
        grid = MarchingSquares.GenerateMesh(map, 1f);
    }



    //Fill the map randomly
    private void FillMapRandomly()
    {
        Random.InitState(seed);

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int z = 0; z < mapSizeZ; z++)
            {
                map[x, z] = (Random.Range(0f, 100f) < randomFillPercent) ? 1 : 0;
            }
        }
    }



    //Debug
    private void OnDrawGizmos()
    {
        //Blue means solid, red means empty
        //DisplayMap();

        DisplayGeneratedMesh();

        //Blue means solid, red means empty
        DisplayMarchingSquaresData();
    }



    private void DisplayMap()
    {
        if (map == null)
        {
            return;
        }


        int xLength = map.GetLength(0);
        int zLength = map.GetLength(1);

        for (int x = 0; x < xLength; x++)
        {
            for (int z = 0; z < zLength; z++)
            {
                Gizmos.color = (map[x, z] == 1) ? Color.blue : Color.red;

                Vector3 pos = new Vector3(-xLength * 0.5f + x + 0.5f, 0f, -zLength * 0.5f + z + 0.5f);

                Gizmos.DrawCube(pos, Vector3.one);
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
                Gizmos.DrawSphere(square.TL.pos, sphereRadius);

                Gizmos.color = square.TR.isActive ? Color.blue : Color.red;
                Gizmos.DrawSphere(square.TR.pos, sphereRadius);

                Gizmos.color = square.BL.isActive ? Color.blue : Color.red;
                Gizmos.DrawSphere(square.BL.pos, sphereRadius);

                Gizmos.color = square.BR.isActive ? Color.blue : Color.red;
                Gizmos.DrawSphere(square.BR.pos, sphereRadius);


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


        Mesh mesh = new Mesh();

        mesh.vertices = grid.vertices.ToArray();

        mesh.triangles = grid.triangles.ToArray();

        mesh.RecalculateNormals();

        TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors(mesh, 0);
    }
}
