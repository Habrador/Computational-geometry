using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingSquaresController : MonoBehaviour 
{
    public int mapSize;

    //Used in cellular automata
    [Range(0, 100)]
    public int randomFillPercent;
    [Range(0, 20)]
    public int numberOfSmooths;

    public int seed;

    private int[,] map;

    private MarchingSquares.SquareGrid grid;




    public void GenerateMap()
    {
        map = new int[mapSize, mapSize];

        RandomFillMap();

        //Smooth the map to create a cave shape
        for (int i = 0; i < numberOfSmooths; i++)
        {
            SmoothMap();
        }

        //Generate the mesh with marching squares
        grid = MarchingSquares.GenerateMesh(map, 1f);
    }



    //Fill the map randomly and add a border
    private void RandomFillMap()
    {
        Random.InitState(seed);

        for (int x = 0; x < mapSize; x++)
        {
            for (int z = 0; z < mapSize; z++)
            {
                //Set each tile to either 0 or 1
                //The border is always wall
                if (x == 0 || x == mapSize - 1 || z == 0 || z == mapSize - 1)
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



    //Smooth the map to create a cave shape
    private void SmoothMap()
    {
        //Should use old values when counting neighbors or the count will not be correct
        //if we are updating the walls as we count them
        int[,] oldMapValues = map.Clone() as int[,];
    
        //Dont look at the walls, which is why we start at 1
        for (int x = 1; x < mapSize - 1; x++)
        {
            for (int z = 1; z < mapSize - 1; z++)
            {
                int wallCount = GetSurroundingWallCount(x, z, oldMapValues);

                //4 is maximum because we are looking at the North-West wall, etc
                if (wallCount > 4)
                {
                    map[x, z] = 1;
                }
                else if (wallCount < 4)
                {
                    map[x, z] = 0;
                }
            }
        }
    }



    //How many neighbors around this cell are walls?
    private int GetSurroundingWallCount(int cellX, int cellZ, int[,] oldMapValues)
    {
        int wallCount = 0;

        for (int neighborX = cellX - 1; neighborX <= cellX + 1; neighborX ++)
        {
            for (int neighborZ = cellZ - 1; neighborZ <= cellZ + 1; neighborZ++)
            {
                //Dont look at itself
                //Dont need to worry about outside of grid because we are checking at cells inside the walls
                //so we cant end up outside of the grid
                //Should be || and not && because we are not checking the 8 surrounding walls, but the 4 diagonals
                if (neighborX != cellX || neighborZ != cellZ)
                {
                    wallCount += oldMapValues[neighborX, neighborZ];
                }
            }
        }

        return wallCount;
    }



    //Debug
    private void OnDrawGizmos()
    {
        //DIsplay the map
        //if (map != null)
        //{
        //    for (int x = 0; x < mapSize; x++)
        //    {
        //        for (int z = 0; z < mapSize; z++)
        //        {
        //            Gizmos.color = (map[x, z] == 1) ? Color.black : Color.white;

        //            Vector3 pos = new Vector3(-mapSize * 0.5f + x + 0.5f, 0f, -mapSize * 0.5f + z + 0.5f);

        //            Gizmos.DrawCube(pos, Vector3.one);
        //        }
        //    }
        //}

        //Display the marching squares
        if (grid != null)
        {
            int xLength = grid.squares.GetLength(0);
            int zLength = grid.squares.GetLength(1);

            for (int x = 0; x < xLength; x++)
            {
                for (int z = 0; z < zLength; z++)
                {
                    MarchingSquares.Square square = grid.squares[x, z];

                    Gizmos.color = square.TL.isActive ? Color.blue : Color.red;
                    Gizmos.DrawSphere(square.TL.pos, 0.2f);

                    Gizmos.color = square.TR.isActive ? Color.blue : Color.red;
                    Gizmos.DrawSphere(square.TR.pos, 0.2f);

                    Gizmos.color = square.BL.isActive ? Color.blue : Color.red;
                    Gizmos.DrawSphere(square.BL.pos, 0.2f);

                    Gizmos.color = square.BR.isActive ? Color.blue : Color.red;
                    Gizmos.DrawSphere(square.BR.pos, 0.2f);


                    //Gizmos.color = Color.green;

                    //Gizmos.DrawSphere(square.T.pos, 0.1f);
                    //Gizmos.DrawSphere(square.L.pos, 0.1f);
                    //Gizmos.DrawSphere(square.B.pos, 0.1f);
                    //Gizmos.DrawSphere(square.R.pos, 0.1f);
                }
            }
        }



        //Display the mesh
        if (grid != null)
        {
            Mesh mesh = new Mesh();

            mesh.vertices = grid.vertices.ToArray();

            mesh.triangles = grid.triangles.ToArray();

            mesh.RecalculateNormals();

            Gizmos.DrawMesh(mesh);
        }
    }
}
