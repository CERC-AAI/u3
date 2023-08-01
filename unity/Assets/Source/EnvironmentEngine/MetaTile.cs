using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


// Initialize an empty 3D array of null values 10 by 10 by 10

// Tile is just a definitional thing now

[ExecuteInEditMode]
public class MetaTile : IMetaTileProbability
{
    public List<Tile> tiles;
    public Transform payload;
    public List<int> tags; // list of tags
    public Vector3Int rotationDirections;  // allowed rotation directions (0 = rotation not allowed, 1 = rotation allowed)

    public List<int> GetTags()
    {
        return tags;
    }

    public override MetaTile GetMetaTile()
    {
        // TODO: do we need to instantiate a game object here?
        return this;
    }

    public bool CanPlace(Tile[,,] environment, int startX, int startY, int startZ)
    {

        foreach (Tile tile in tiles)
        {
            Vector3Int position = new Vector3Int((int)tile.transform.localPosition.x, (int)tile.transform.localPosition.y, (int)tile.transform.localPosition.z);

            int envX = startX + position.x;
            int envY = startY + position.y;
            int envZ = startZ + position.z;

            if (envX < 0 || envX >= environment.GetLength(0) ||
                envY < 0 || envY >= environment.GetLength(1) ||
                envZ < 0 || envZ >= environment.GetLength(2))
            {
                return false;  // the metatile is out of bounds
            }

            if (environment[envX, envY, envZ] != null)
            {
                // Debug.Log("environment[envX, envY, envZ] :" + environment[envX, envY, envZ]);
                return false;  // the metatile would overwrite a tile
            }

            // Check if the faces of the tile are compatible with the neighboring tiles according to the matching matrix
            return tile.CanPlaceTile(environment, new Vector3Int(envX, envY, envZ));

        }

        return true;  // no conflicts were found
    }

    public void PlaceMetaTile(Tile[,,] environment, int startX, int startY, int startZ)
    {
        Debug.Log("placing metatile " + this.name);

        foreach (Tile tile in tiles)
        {
            Vector3Int position = new Vector3Int((int)tile.transform.localPosition.x, (int)tile.transform.localPosition.y, (int)tile.transform.localPosition.z);

            int envX = startX + position.x;
            int envY = startY + position.y;
            int envZ = startZ + position.z;

            environment[envX, envY, envZ] = tile;
        }
    }

    public void DepositPayload(Vector3Int position)
    {
        if (payload != null)
        {
            Transform payloadCopy = Instantiate(payload);
            payloadCopy.transform.position = position;
        }
    }
    private void Awake()
    {
    }
}
