using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


// Initialize an empty 3D array of null values 10 by 10 by 10

public class MetaTilePool : IMetaTileProbability
{
    public List<int> tags; // list of tags

    public enum RESULTTYPE
    {
        SUCCESS,
        FAILURE,
        COMPLETE,
    }
    public List<MetaTileProbability> metatileProbabilities;

    public List<int> GetTags()
    {
        return tags;
    }

    public override MetaTile GetMetaTile()
    {
        // Calculate the total weight from the dictionary
        float totalWeight = 0f;
        foreach (MetaTileProbability metatileprobability in metatileProbabilities)
        {
            totalWeight += metatileprobability.weight;
        }

        // Choose a random weight
        float randomWeight = UnityEngine.Random.value * totalWeight;

        // Find the metatile probability that corresponds to the random weight
        for (int i = 0; i < metatileProbabilities.Count; i++)
        {
            if (randomWeight < metatileProbabilities[i].weight)
            {
                return metatileProbabilities[i].metaTileProbability.GetMetaTile();
            }
            randomWeight -= metatileProbabilities[i].weight;

        }

        // If no metatile probability was selected (which shouldn't happen if the weights are set up correctly), return null
        Debug.Log("No metatile probability was selected.");
        return null;
    }

    public static Vector3Int SelectRandomEmptyPosition(Tile[,,] environment)
    {
        List<Vector3Int> emptyPositions = new List<Vector3Int>();

        // Find all empty positions
        for (int x = 0; x < environment.GetLength(0); x++)
        {
            for (int y = 0; y < environment.GetLength(1); y++)
            {
                for (int z = 0; z < environment.GetLength(2); z++)
                {
                    if (environment[x, y, z] == null)
                    {
                        emptyPositions.Add(new Vector3Int(x, y, z));
                    }
                }
            }
        }

        // If there are no empty positions, return an invalid position
        if (emptyPositions.Count == 0)
        {
            return new Vector3Int(-1, -1, -1);
        }

        // Select a random empty position
        return emptyPositions[UnityEngine.Random.Range(0, emptyPositions.Count)];
    }



    public RESULTTYPE GetAndPlaceMetaTile(Tile[,,] environment, out MetaTile placedMetaTile, out Vector3Int placedPosition)
    {

        const int maxAttempts = 100;
        int attempt = 0;
        placedMetaTile = null;
        placedPosition = Vector3Int.zero;

        while (attempt < maxAttempts)
        {
            // Select a random empty position
            Vector3Int placementPosition = SelectRandomEmptyPosition(environment);
            // Debug.Log("placementPosition: " + placementPosition);

            // If there are no empty positions, return
            if (placementPosition.x < 0) return RESULTTYPE.COMPLETE;

            // Select a metatile
            MetaTile metaTile = GetMetaTile();
            //Debug.Log("metatile name: " + metaTile.name);
            // Try all rotations
            // add flags so that you can lock rotations (e.g. don't want archway to be upside down, can be defined in metatile itself)
            // Try all rotations
            for (int xRotation = 0; xRotation < 4; xRotation++)
            {
                if (metaTile.rotationDirections.x == 0 && xRotation > 0)
                {
                    Debug.Log("GetAndPlaceMetaTile() can't rotate in x direction");
                    continue;
                }


                for (int yRotation = 0; yRotation < 4; yRotation++)
                {
                    if (metaTile.rotationDirections.y == 0 && yRotation > 0)
                    {
                        Debug.Log("GetAndPlaceMetaTile() can't rotate in y direction");
                        continue;
                    }
                    for (int zRotation = 0; zRotation < 4; zRotation++)
                    {
                        if (metaTile.rotationDirections.z == 0 && zRotation > 0)
                        {
                            Debug.Log("GetAndPlaceMetaTile() can't rotate in z direction");
                            continue;
                        }
                        // Rotate the metatile
                        metaTile.transform.rotation = Quaternion.Euler(90 * xRotation, 90 * yRotation, 90 * zRotation);
                        // TODO: modify the order of the tile's faceTypes array to match the rotation


                        // Try to place the metatile
                        bool canPlace = metaTile.CanPlace(environment, placementPosition.x, placementPosition.y, placementPosition.z);
                        Debug.Log("GetAndPlaceMetaTile() canPlace(): " + canPlace);
                        if (canPlace)
                        {
                            // The metatile can be placed, so place it
                            metaTile.PlaceMetaTile(environment, placementPosition.x, placementPosition.y, placementPosition.z);
                            placedMetaTile = metaTile;
                            placedPosition = placementPosition;
                            Debug.Log("SUCCESS: GetAndPlaceMetaTile()");
                            return RESULTTYPE.SUCCESS;

                        }
                    }
                }
            }

            attempt++;
            Debug.Log("GetAndPlaceMetaTile() attempt++");
        }

        Debug.Log("FAILURE: GetAndPlaceMetaTile()");
        return RESULTTYPE.FAILURE;

    }
}

[Serializable]
public struct MetaTileProbability
{
    public IMetaTileProbability metaTileProbability;
    public float weight;
}

public class IMetaTileProbability : MonoBehaviour
{
    public virtual List<int> GetTags()
    {
        return null;
    }
    public virtual MetaTile GetMetaTile()
    {
        return null;
    }

}
