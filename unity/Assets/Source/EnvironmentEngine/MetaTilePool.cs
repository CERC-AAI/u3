using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NUnit.Framework.Interfaces;


// Initialize an empty 3D array of null values 10 by 10 by 10

public class MetaTilePool : IMetaTileProbability
{
    public List<int> tags; // list of tags

    public TileFacePalette palette;

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

    public List<float> weights;

    // TODO: filter the list of metatiles by the possible faces in filterFaces
    // the metatile must contain at least one tile with a face in filterFaces
    // if this tile has a face in filterFaces, then check the other faces of the tile against the other faces in possibleFaces

    public List<MetaTileProbability> BuildMetaTilePoolDeepCopy()
    {
        List<MetaTileProbability> metatileProbabilitiesForSampling = new List<MetaTileProbability>();
        GetWeightedMetaTiles(1, metatileProbabilitiesForSampling);
        return metatileProbabilitiesForSampling;
    }

    public void GetWeightedMetaTiles(float weight, List<MetaTileProbability> deepCopy)
    {
        int depth_count = 0;
        for (int i = 0; i < metatileProbabilities.Count; i++)
        {
            if (metatileProbabilities[i].metaTileProbability is MetaTile)
            {
                Debug.Log("is MetaTile");
                MetaTileProbability metatileprobability = new MetaTileProbability();
                metatileprobability.metaTileProbability = metatileProbabilities[i].metaTileProbability;
                metatileprobability.weight = metatileProbabilities[i].weight * weight;
                deepCopy.Add(metatileprobability);
            }
            else if (metatileProbabilities[i].metaTileProbability is MetaTilePool)
            {
                depth_count++;
                if (depth_count > 2)
                {
                    throw new Exception("depth count too big, something went wrong");
                }
                Debug.Log("is not Metatile, going one level deeper");
                Debug.Log("Type: " + metatileProbabilities[i].metaTileProbability.GetType());
                MetaTilePool metatilepool = (MetaTilePool)metatileProbabilities[i].metaTileProbability;
                Debug.Log("metatilepool: " + metatilepool);
                metatilepool.GetWeightedMetaTiles(metatileProbabilities[i].weight * weight, deepCopy);
            }

            else
            {
                Debug.Log("is not Metatile or MetatilePool");
                // throw error
                throw new ArgumentException("Parameter cannot be null", metatileProbabilities[i].metaTileProbability.GetType().ToString());
            }

            // if (depth_count > 0)
            // {
            //     Debug.Log("depth_count: " + depth_count);
            // }

            // if (depth_count > 2)
            // {
            //     Debug.Log("depth count too big, something went wrong");
            //     break;
            // }
        }

    }

    public static MetaTile DrawMetaTileWithoutReplacement(List<MetaTileProbability> metatileProbabilities)
    {
        float totalWeight = 0f;
        foreach (MetaTileProbability metatileprobability in metatileProbabilities)
        {
            totalWeight += metatileprobability.weight;
        }

        float randomWeight = UnityEngine.Random.value * totalWeight;

        for (int i = 0; i < metatileProbabilities.Count; i++)
        {
            if (randomWeight < metatileProbabilities[i].weight)
            {
                MetaTile metatileForRemoval = metatileProbabilities[i].metaTileProbability.DrawMetaTile();
                metatileProbabilities.RemoveAt(i);
                return metatileForRemoval;
            }
            randomWeight -= metatileProbabilities[i].weight;

        }

        // If no metatile probability was selected (which shouldn't happen if the weights are set up correctly), return null
        Debug.Log("No metatile probability was selected.");
        return null;
    }


    public override List<MetaTile> GetMetaTiles()
    {
        List<MetaTile> metaTiles = new List<MetaTile>();
        for (int i = 0; i < metatileProbabilities.Count; i++)
        {
            // get the metatiles from each metatile probability
            metaTiles.AddRange(metatileProbabilities[i].metaTileProbability.GetMetaTiles());
        }
        return metaTiles;
    }

    public override MetaTile DrawMetaTile()
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
                return metatileProbabilities[i].metaTileProbability.DrawMetaTile();
            }
            randomWeight -= metatileProbabilities[i].weight;

        }

        // If no metatile probability was selected (which shouldn't happen if the weights are set up correctly), return null
        Debug.Log("No metatile probability was selected.");
        return null;
    }

    //     public RESULTTYPE GetAndPlaceMetaTile(Tile[,,] environment, int[,,,] faces, int[,,] environmentEntropies, Dictionary<int, List<int>> matchingMatrix, out MetaTile placedMetaTile, out Vector3Int placedPosition)
    // {

    //     const int maxAttempts = 100;
    //     int attempt = 0;
    //     placedMetaTile = null;
    //     placedPosition = Vector3Int.zero;

    //     while (attempt < maxAttempts)
    //     {
    //         // Select a low entropy position
    //         Vector3Int placementPosition = SelectLowestEntropyPosition(environment, faces, environmentEntropies, matchingMatrix);
    //         // Debug.Log("placementPosition: " + placementPosition);

    //         // If there are no empty positions, return
    //         if (placementPosition.x < 0) return RESULTTYPE.COMPLETE;

    //         // Select a metatile
    //         MetaTile metaTile = GetMetaTile();
    //         //Debug.Log("metatile name: " + metaTile.name);
    //         // Try all rotations
    //         // add flags so that you can lock rotations (e.g. don't want archway to be upside down, can be defined in metatile itself)
    //         // Try all rotations
    //         for (int xRotation = 0; xRotation < 4; xRotation++)
    //         {
    //             if (metaTile.rotationDirections.x == 0 && xRotation > 0)
    //             {
    //                 Debug.Log("GetAndPlaceMetaTile() can't rotate in x direction");
    //                 continue;
    //             }


    //             for (int yRotation = 0; yRotation < 4; yRotation++)
    //             {
    //                 if (metaTile.rotationDirections.y == 0 && yRotation > 0)
    //                 {
    //                     Debug.Log("GetAndPlaceMetaTile() can't rotate in y direction");
    //                     continue;
    //                 }
    //                 for (int zRotation = 0; zRotation < 4; zRotation++)
    //                 {
    //                     if (metaTile.rotationDirections.z == 0 && zRotation > 0)
    //                     {
    //                         Debug.Log("GetAndPlaceMetaTile() can't rotate in z direction");
    //                         continue;
    //                     }
    //                     // Rotate the metatile
    //                     metaTile.transform.rotation = Quaternion.Euler(90 * xRotation, 90 * yRotation, 90 * zRotation);
    //                     // TODO: modify the order of the tile's faceTypes array to match the rotation


    //                     // Try to place the metatile
    //                     bool canPlace = metaTile.CanPlace(environment, placementPosition.x, placementPosition.y, placementPosition.z);
    //                     Debug.Log("GetAndPlaceMetaTile() canPlace(): " + canPlace);
    //                     if (canPlace)
    //                     {
    //                         // The metatile can be placed, so place it
    //                         metaTile.PlaceMetaTile(environment, placementPosition.x, placementPosition.y, placementPosition.z);
    //                         placedMetaTile = metaTile;
    //                         placedPosition = placementPosition;
    //                         Debug.Log("SUCCESS: GetAndPlaceMetaTile()");
    //                         return RESULTTYPE.SUCCESS;

    //                     }
    //                 }
    //             }
    //         }

    //         attempt++;
    //         Debug.Log("GetAndPlaceMetaTile() attempt++");
    //     }

    //     Debug.Log("FAILURE: GetAndPlaceMetaTile()");
    //     return RESULTTYPE.FAILURE;

    // }
}

[Serializable]
public struct MetaTileProbability
{
    public IMetaTileProbability metaTileProbability;
    public float weight;
}

public class IMetaTileProbability : MonoBehaviour
{
    public MetaTilePool parent;

    public virtual List<int> GetTags()
    {
        return null;
    }
    public virtual MetaTile DrawMetaTile()
    {
        return null;
    }

    public virtual List<MetaTile> GetMetaTiles()
    {
        return null;
    }

}
