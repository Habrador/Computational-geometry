using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Basic idea is that we use a grid where the diagonal of a cell has the same radius as the disc
public class PoissonDiscSampling : MonoBehaviour 
{
    public int seed;

    public float radius;

    [Range(0, 100)]
    public int samplesBeforeRejection;

    public Vector2 mapSize;

    private List<Vector3> points;


    public void RunAlgorithm()
    {
        points = GeneratePoints(radius, mapSize, samplesBeforeRejection, seed);
    }


    //Display
    private void OnDrawGizmos()
    {
        if (points == null)
        {
            return;
        }

        Debug.Log(points.Count);
        
        for (int i = 0; i < points.Count; i++)
        {
            Gizmos.DrawSphere(points[i], radius / 2f);
        }

        Gizmos.DrawWireCube(new Vector3(mapSize.x, 0f, mapSize.y) / 2f, new Vector3(mapSize.x, 0f, mapSize.y));
    }



    public List<Vector3> GeneratePoints(float radius, Vector2 sampleRegionSize, int samplesBeforeRejection, int seed)
    {
        //Pythagoras
        float cellSize = radius / Mathf.Sqrt(2f);

        //Refers to positions in the allPoints array which belongs to a specific point
        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];

        List<Vector3> allPoints = new List<Vector3>();

        List<Vector3> spawnPoints = new List<Vector3>();

        //Init by adding a spawn point 
        spawnPoints.Add(new Vector3(sampleRegionSize.x / 2f, 0f, sampleRegionSize.y / 2f));

        int safety = 0;

        while (spawnPoints.Count > 0)
        {
            safety += 1;

            if (safety > 1000)
            {
                Debug.Log("Stuck in infinite loop when poisson discing");
            
                break;
            }
            
            //Need the index so we can remove the point if we couldnt spawn a point around it
            int spawnIndex = Random.Range(0, spawnPoints.Count);

            Vector3 spawnCenter = spawnPoints[spawnIndex];

            bool wasCandidateAccepted = false;

            //Try to spawn a point around this spawnCenter
            for (int i = 0; i < samplesBeforeRejection; i++)
            {
                float angle = Random.value * Mathf.PI * 2f;

                Vector3 dir = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));

                Vector3 candidatePoint = spawnCenter + dir * Random.Range(radius, 2f * radius);

                if (IsPositionValid(candidatePoint, sampleRegionSize, radius, cellSize, allPoints, grid))
                {
                    allPoints.Add(candidatePoint);

                    spawnPoints.Add(candidatePoint);

                    grid[(int)(candidatePoint.x / cellSize), (int)(candidatePoint.z / cellSize)] = allPoints.Count;

                    wasCandidateAccepted = true;

                    break;
                }
            }

            //Remove the spawncenter if no point could be spawned around it
            if (!wasCandidateAccepted)
            {
                spawnPoints.RemoveAt(spawnIndex);
            }
        }

        return allPoints;
    }



    private bool IsPositionValid(Vector3 pos, Vector2 sampleRegionSize, float radius, float cellSize, List<Vector3> allPoints, int[,] grid)
    {
        //Is the point within the map?
        if (pos.x >= 0f && pos.x < sampleRegionSize.x && pos.z >= 0f && pos.z < sampleRegionSize.y)
        {
            int cellX = (int)(sampleRegionSize.x / cellSize);
            int cellY = (int)(sampleRegionSize.y / cellSize);

            //Search in a 5x5 area around this cell
            //Make sure it starts within the grid
            int searchStartX = Mathf.Max(0, cellX - 2);
            int searchStartY = Mathf.Max(0, cellY - 2);

            int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
            int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    //The ints in the array refers to positions in a list
                    //So by taking -1 we can tell if a grid is occupied?
                    int pointIndex = grid[x, y] - 1;

                    if (pointIndex != -1)
                    {
                        float distance = (pos - allPoints[pointIndex]).magnitude;

                        if (distance < radius)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        return false;
    }
}
