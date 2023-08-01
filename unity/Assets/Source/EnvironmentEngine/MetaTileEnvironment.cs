using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MetaTileEnvironment : MonoBehaviour
{
    public Tile[,,] environment = new Tile[10, 10, 10];

    public float voxelSize = 1;

    public MetaTilePool metatilepool;

    public List<MetaTile> placedMetaTiles = new List<MetaTile>();

    public List<Vector3Int> placedPositions = new List<Vector3Int>();

    public void GenerateEnvironment(MetaTilePool metatilepool)
    {
        MetaTilePool.RESULTTYPE resultType = MetaTilePool.RESULTTYPE.SUCCESS;
        int timeoutCounter = 0;
        MetaTile placedMetaTile;

        while (resultType != MetaTilePool.RESULTTYPE.COMPLETE)
        {
            resultType = metatilepool.GetAndPlaceMetaTile(environment, out placedMetaTile, out Vector3Int placedPosition);

            if (resultType == MetaTilePool.RESULTTYPE.COMPLETE)
            {
                break;
            }

            if (resultType == MetaTilePool.RESULTTYPE.FAILURE)
            {
                timeoutCounter++;
                Debug.Log("timeoutCounter++");
            }
            else
            {
                timeoutCounter = 0;
                placedMetaTiles.Add(placedMetaTile);
                placedPositions.Add(placedPosition);
            }

            if (timeoutCounter > 100)
            {
                Debug.Log("Timeout");
                for (int i = 0; i < placedMetaTiles.Count; i++)
                {
                    placedMetaTiles[i].DepositPayload(placedPositions[i]);
                    // count the number of placed metatiles
                    Debug.Log("placedMetaTiles.Count: " + placedMetaTiles.Count);
                }
                break;
            }
        }

        if (timeoutCounter <= 1000 && resultType == MetaTilePool.RESULTTYPE.COMPLETE)
        {
            Debug.Log("Complete");

            for (int i = 0; i < placedMetaTiles.Count; i++)
            {
                placedMetaTiles[i].DepositPayload(placedPositions[i]);
            }
        }
    }

    public void Awake()
    {
        GenerateEnvironment(metatilepool);
    }
}
