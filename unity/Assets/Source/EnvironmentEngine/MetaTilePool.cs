using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NUnit.Framework.Interfaces;


// Initialize an empty 3D array of null values 10 by 10 by 10

public class MetatilePool : IMetatileContainer
{
    public List<int> tags; // list of tags

    public TileFacePalette palette;

    public enum RESULTTYPE
    {
        SUCCESS,
        FAILURE,
        COMPLETE,
    }
    public List<MetatileProbability> metatileProbabilities;

    public List<int> GetTags()
    {
        return tags;
    }

    public override MetaTile GetMetatile()
    {
        return null;
    }

    // TODO: filter the list of metatiles by the possible faces in filterFaces
    // the metatile must contain at least one tile with a face in filterFaces
    // if this tile has a face in filterFaces, then check the other faces of the tile against the other faces in possibleFaces

    public List<MetatileProbability> BuildMetatilePoolDeepCopy()
    {
        List<MetatileProbability> metatileProbabilitiesForSampling = new List<MetatileProbability>();
        GetWeightedMetatiles(1, metatileProbabilitiesForSampling);
        return metatileProbabilitiesForSampling;
    }

    public void GetWeightedMetatiles(float weight, List<MetatileProbability> deepCopy)
    {
        float totalWeight = 0;
        for (int i = 0; i < metatileProbabilities.Count; i++)
        {
            totalWeight += metatileProbabilities[i].GetDynamicWeight();
        }

        int depth_count = 0;
        for (int i = 0; i < metatileProbabilities.Count; i++)
        {
            if (metatileProbabilities[i].metatileContainer is MetaTile)
            {
                //Debug.Log("is Metatile");
                MetatileProbability metatileprobability = new MetatileProbability();
                metatileprobability.metatileContainer = metatileProbabilities[i].metatileContainer;
                metatileprobability.SetDynamicWeight(metatileProbabilities[i].GetDynamicWeight() / totalWeight * weight);
                deepCopy.Add(metatileprobability);
            }
            else if (metatileProbabilities[i].metatileContainer is MetatilePool)
            {
                depth_count++;
                if (depth_count > 20)
                {
                    throw new Exception("depth count too big, something went wrong");
                }
                //Debug.Log("is not Metatile, going one level deeper");
                //Debug.Log("Type: " + metatileProbabilities[i].metatileProbability.GetType());
                MetatilePool metatilepool = (MetatilePool)metatileProbabilities[i].metatileContainer;
                //Debug.Log("metatilepool: " + metatilepool);
                metatilepool.GetWeightedMetatiles(metatileProbabilities[i].GetDynamicWeight() / totalWeight * weight, deepCopy);
            }

            else
            {
                Debug.Log("is not Metatile or MetatilePool");
                // throw error
                throw new ArgumentException("Parameter cannot be null", metatileProbabilities[i].metatileContainer.GetType().ToString());
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

    public static MetaTile DrawMetatileWithoutReplacement(List<MetatileProbability> metatileProbabilities)
    {
        float totalWeight = 0f;
        foreach (MetatileProbability metatileprobability in metatileProbabilities)
        {
            totalWeight += metatileprobability.GetDynamicWeight();
        }

        float randomWeight = UnityEngine.Random.value * totalWeight;

        for (int i = 0; i < metatileProbabilities.Count; i++)
        {
            if (randomWeight < metatileProbabilities[i].GetDynamicWeight())
            {
                MetaTile metatileForRemoval = metatileProbabilities[i].metatileContainer.DrawMetatile();
                metatileProbabilities.RemoveAt(i);
                return metatileForRemoval;
            }
            randomWeight -= metatileProbabilities[i].GetDynamicWeight();

        }

        Debug.Log("No metatile probability was selected.");
        return null;
    }


    public override List<MetaTile> GetMetatiles()
    {
        List<MetaTile> metatiles = new List<MetaTile>();
        for (int i = 0; i < metatileProbabilities.Count; i++)
        {
            // get the metatiles from each metatile probability
            metatiles.AddRange(metatileProbabilities[i].metatileContainer.GetMetatiles());
        }
        return metatiles;
    }

    public override MetaTile DrawMetatile()
    {

        // Calculate the total weight from the dictionary
        float totalWeight = 0f;
        foreach (MetatileProbability metatileprobability in metatileProbabilities)
        {
            totalWeight += metatileprobability.GetDynamicWeight();
        }

        // Choose a random weight
        float randomWeight = UnityEngine.Random.value * totalWeight;

        // Find the metatile probability that corresponds to the random weight
        for (int i = 0; i < metatileProbabilities.Count; i++)
        {
            if (randomWeight < metatileProbabilities[i].GetDynamicWeight())
            {
                return metatileProbabilities[i].metatileContainer.DrawMetatile();
            }
            randomWeight -= metatileProbabilities[i].GetDynamicWeight();

        }

        // If no metatile probability was selected (which shouldn't happen if the weights are set up correctly), return null
        Debug.Log("No metatile probability was selected.");
        return null;
    }

    public void UpdateDynamicWeight(IMetatileContainer child, float newWeight)
    {
        for (int i = 0; i < metatileProbabilities.Count; i++)
        {
            if (metatileProbabilities[i].metatileContainer == child)
            {
                MetatileProbability metatileprobability = metatileProbabilities[i];
                metatileprobability.SetDynamicWeight(newWeight);
                metatileProbabilities[i] = metatileprobability;
                return;
            }
        }
    }

    public override void ResetDynamicWeight()
    {
        for (int i = 0; i < metatileProbabilities.Count; i++)
        {
            metatileProbabilities[i].metatileContainer.ResetDynamicWeight();
            MetatileProbability metatileprobability = metatileProbabilities[i];
            metatileprobability.SetDynamicWeight(metatileprobability.weight);
            metatileProbabilities[i] = metatileprobability;
        }
    }

    //     public RESULTTYPE GetAndPlaceMetatile(Tile[,,] environment, int[,,,] faces, int[,,] environmentEntropies, Dictionary<int, List<int>> matchingMatrix, out Metatile placedMetatile, out Vector3Int placedPosition)
    // {

    //     const int maxAttempts = 100;
    //     int attempt = 0;
    //     placedMetatile = null;
    //     placedPosition = Vector3Int.zero;

    //     while (attempt < maxAttempts)
    //     {
    //         // Select a low entropy position
    //         Vector3Int placementPosition = SelectLowestEntropyPosition(environment, faces, environmentEntropies, matchingMatrix);
    //         // Debug.Log("placementPosition: " + placementPosition);

    //         // If there are no empty positions, return
    //         if (placementPosition.x < 0) return RESULTTYPE.COMPLETE;

    //         // Select a metatile
    //         Metatile metatile = GetMetatile();
    //         //Debug.Log("metatile name: " + metatile.name);
    //         // Try all rotations
    //         // add flags so that you can lock rotations (e.g. don't want archway to be upside down, can be defined in metatile itself)
    //         // Try all rotations
    //         for (int xRotation = 0; xRotation < 4; xRotation++)
    //         {
    //             if (metatile.rotationDirections.x == 0 && xRotation > 0)
    //             {
    //                 Debug.Log("GetAndPlaceMetatile() can't rotate in x direction");
    //                 continue;
    //             }


    //             for (int yRotation = 0; yRotation < 4; yRotation++)
    //             {
    //                 if (metatile.rotationDirections.y == 0 && yRotation > 0)
    //                 {
    //                     Debug.Log("GetAndPlaceMetatile() can't rotate in y direction");
    //                     continue;
    //                 }
    //                 for (int zRotation = 0; zRotation < 4; zRotation++)
    //                 {
    //                     if (metatile.rotationDirections.z == 0 && zRotation > 0)
    //                     {
    //                         Debug.Log("GetAndPlaceMetatile() can't rotate in z direction");
    //                         continue;
    //                     }
    //                     // Rotate the metatile
    //                     metatile.transform.rotation = Quaternion.Euler(90 * xRotation, 90 * yRotation, 90 * zRotation);
    //                     // TODO: modify the order of the tile's faceTypes array to match the rotation


    //                     // Try to place the metatile
    //                     bool canPlace = metatile.CanPlace(environment, placementPosition.x, placementPosition.y, placementPosition.z);
    //                     Debug.Log("GetAndPlaceMetatile() canPlace(): " + canPlace);
    //                     if (canPlace)
    //                     {
    //                         // The metatile can be placed, so place it
    //                         metatile.PlaceMetatile(environment, placementPosition.x, placementPosition.y, placementPosition.z);
    //                         placedMetatile = metatile;
    //                         placedPosition = placementPosition;
    //                         Debug.Log("SUCCESS: GetAndPlaceMetatile()");
    //                         return RESULTTYPE.SUCCESS;

    //                     }
    //                 }
    //             }
    //         }

    //         attempt++;
    //         Debug.Log("GetAndPlaceMetatile() attempt++");
    //     }

    //     Debug.Log("FAILURE: GetAndPlaceMetatile()");
    //     return RESULTTYPE.FAILURE;

    // }
}

[Serializable]
public struct MetatileProbability
{
    public IMetatileContainer metatileContainer;
    public float weight;
    private float dynamicWeight;

    public void SetDynamicWeight(float newWeight)
    {
        dynamicWeight = newWeight;
    }
    public float GetDynamicWeight()
    {
        return dynamicWeight;
    }

}

public class IMetatileContainer : MonoBehaviour
{
    public MetatilePool parent;

    public virtual List<int> GetTags()
    {
        return null;
    }
    public virtual MetaTile DrawMetatile()
    {
        return null;
    }

    public virtual List<MetaTile> GetMetatiles()
    {
        return null;
    }

    public virtual MetaTile GetMetatile()
    {
        return null;
    }

    public void UpdateDynamicWeight(float newWeight)
    {
        if (parent == null)
        {
            Debug.Log("parent is null");
            return;
        }
        else
        {
            parent.UpdateDynamicWeight(this, newWeight);
        }
    }

    public virtual void ResetDynamicWeight()
    {
        return;
    }


}
