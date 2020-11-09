using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaCirclesController : MonoBehaviour
{
    public GameObject circleParent;

    public int seed;

    public int mapSize;

    //So we can display the map in OnDrawGizmos
    private int[,] map;

    //The size of an individual square
    private readonly float squareSize = 0.5f;



    public void GenerateMap()
    {
        map = new int[mapSize, mapSize];
        
        FillMap();
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
            float radius = Random.Range(1f, 6f);

            circleRadiuses[i] = radius;
        }
        //Debug.Log(circleRadiuses[1]);

        //Loop through all cells and check if they are within the radius of one of the circles
        for (int x = 0; x < mapSize; x++)
        {
            for (int z = 0; z < mapSize; z++)
            {
                float cellCenterX = -mapSize * 0.5f * squareSize + x * squareSize + squareSize * 0.5f;

                float cellCenterZ = -mapSize * 0.5f * squareSize + z * squareSize + squareSize * 0.5f;

                Vector3 pos = new Vector3(cellCenterX, 0f, cellCenterZ);


                //Loop through all circles
                for (int i = 1; i < circles.Length; i++)
                {
                    Vector3 circlePos = circles[i].position;
                    
                    float radius = circleRadiuses[i];
                   
                    if ((pos - circlePos).sqrMagnitude < radius * radius)
                    {
                        map[x, z] = 1;
                        
                        break;
                    }
                }
            }
        }
    }



    private void OnDrawGizmos()
    {
        //Blue means solid, red means empty
        DisplayMap();

        DisplayCircles();
    }



    private void DisplayMap()
    {
        if (map == null)
        {
            return;
        }


        int xLength = map.GetLength(0);
        int zLength = map.GetLength(1);

        //Debug.Log(xLength);

        //int counter = 0;

        for (int x = 0; x < xLength; x++)
        {
            for (int z = 0; z < zLength; z++)
            {
                Gizmos.color = (map[x, z] == 1) ? Color.blue : Color.red;

                //Gizmos.color = (counter % 2 != 0) ? Color.blue : Color.red;

                float cellCenterX = -xLength * 0.5f * squareSize + x * squareSize + squareSize * 0.5f;

                float cellCenterZ = -zLength * 0.5f * squareSize + z * squareSize + squareSize * 0.5f;

                Vector3 pos = new Vector3(cellCenterX, 0f, cellCenterZ);

                Gizmos.DrawCube(pos, Vector3.one * squareSize * 0.9f);

                //Gizmos.color = Color.white;

                //Gizmos.DrawLine(pos, Vector3.zero);

                //counter += 1;
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
            float radius = Random.Range(1f, 6f);

            Gizmos.DrawWireSphere(circles[i].position, radius);
        }
    }
}
