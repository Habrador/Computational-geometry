using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
//using Habrador_Computational_Geometry.Marching_Squares;


//Based on Procedural Cave Generation (E02. Marching Squares): https://www.youtube.com/watch?v=yOgIncKp0BE
public class MarchingSquaresController : MonoBehaviour 
{
    public int mapSizeX;
    public int mapSizeZ;

    //Used in cellular automata
    [Range(0, 100)]
    public int randomFillPercent;
    [Range(0, 20)]
    public int numberOfSmooths;

    public int seed;

    private int[,] map;

    private Habrador_Computational_Geometry.Marching_Squares.SquareGrid grid;




    public void GenerateMap()
    {
        map = new int[mapSizeX, mapSizeZ];

        FillMapRandomly();

        //Generate the mesh with marching squares
        grid = MarchingSquares.GenerateMesh(map, 1f);
    }



    //Fill the map randomly and add a border
    private void FillMapRandomly()
    {
        Random.InitState(seed);

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int z = 0; z < mapSizeZ; z++)
            {
                //Set each tile to either 0 or 1
                //The border is always wall
                if (x == 0 || x == mapSizeX - 1 || z == 0 || z == mapSizeZ - 1)
                {
                    map[x, z] = 1;
                }
                else
                {
                    map[x, z] = (Random.Range(0f, 100f) < randomFillPercent) ? 1 : 0;
                }
            }
        }
    }



    //Debug
    private void OnDrawGizmos()
    {
        DisplayMap();

        //Blue means solid, red means empty
        DisplayMarchingSquaresData();

        //DisplayMesh();
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

                float sphereRadius = 0.1f;

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



    private void DisplayMesh()
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
