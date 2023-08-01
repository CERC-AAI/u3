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

    public TileFacePalette pallete;

    public List<int> GetTags()
    {
        return tags;
    }

    public override MetaTile GetMetaTile()
    {
        // TODO: do we need to instantiate a game object here?
        return this;
    }

    public bool CanConnect(int faceType1, int faceType2)
    {
        return pallete.CanConnect(faceType1, faceType2);
    }

    public bool CanPlace(Tile[,,] environment, int startX, int startY, int startZ)
    {

        foreach (Tile tile in tiles)
        {
            Vector3Int position = new Vector3Int((int)tile.transform.localPosition.x, (int)tile.transform.localPosition.y, (int)tile.transform.localPosition.z);

            Vector3Int environmentPosition = new Vector3Int(startX, startY, startZ) + position;

            if (environmentPosition.x < 0 || environmentPosition.x >= environment.GetLength(0) ||
                environmentPosition.y < 0 || environmentPosition.y >= environment.GetLength(1) ||
                environmentPosition.z < 0 || environmentPosition.z >= environment.GetLength(2))
            {
                return false;  // the metatile is out of bounds
            }

            if (GetTile(environment, environmentPosition) != null)
            {
                return false;  // the metatile would overwrite a tile
            }

            // TODO: check neighbor tiles for adjacency conflicts
            foreach (Tile.EDGETYPE edgeType in Enum.GetValues(typeof(Tile.EDGETYPE)))
            {
                Vector3Int offsetVector = Vector3Int.zero;
                Tile.EDGETYPE compareEdge = Tile.EDGETYPE.BOTTOM;
                //TODO: deal with rotations
                switch (edgeType)
                {
                    case Tile.EDGETYPE.TOP:
                        offsetVector = new Vector3Int(0, 1, 0);
                        compareEdge = Tile.EDGETYPE.BOTTOM;
                        break;

                    case Tile.EDGETYPE.BOTTOM:
                        offsetVector = new Vector3Int(0, -1, 0);
                        compareEdge = Tile.EDGETYPE.TOP;
                        break;

                    case Tile.EDGETYPE.LEFT:
                        offsetVector = new Vector3Int(-1, 0, 0);
                        compareEdge = Tile.EDGETYPE.RIGHT;
                        break;

                    case Tile.EDGETYPE.RIGHT:
                        offsetVector = new Vector3Int(1, 0, 0);
                        compareEdge = Tile.EDGETYPE.LEFT;
                        break;

                    case Tile.EDGETYPE.FRONT:
                        offsetVector = new Vector3Int(0, 0, -1);
                        compareEdge = Tile.EDGETYPE.BACK;
                        break;

                    case Tile.EDGETYPE.BACK:
                        offsetVector = new Vector3Int(0, 0, 1);
                        compareEdge = Tile.EDGETYPE.FRONT;
                        break;
                }

                
                Tile tempTile = GetTile(environment, environmentPosition + offsetVector);
                if (tempTile != null && !HasTile(position + offsetVector)) //Ignore internal connections
                {
                    if (!CanConnect(tile.edgeIDs[(int)edgeType], tempTile.edgeIDs[(int)compareEdge]))
                    {
                        return false; // Had conflict on top
                    }
                }
            }
        }

        return true;  // no conflicts were found
    }

    public Tile GetTile(Tile[,,] environment, Vector3Int position)
    {
        if (position.x < 0 || position.x >= environment.GetLength(0) ||
            position.y < 0 || position.y >= environment.GetLength(1) ||
            position.z < 0 || position.z >= environment.GetLength(2))
        {
            return null;  // the tile is out of bounds
        }

        return environment[position.x, position.y, position.z];
    }

    public void PlaceMetaTile(Tile[,,] environment, int startX, int startY, int startZ)
    {
        //Debug.Log("placing metatile " + this.name);

        foreach (Tile tile in tiles)
        {
            Vector3Int position = new Vector3Int((int)tile.transform.localPosition.x, (int)tile.transform.localPosition.y, (int)tile.transform.localPosition.z);

            int envX = startX + position.x;
            int envY = startY + position.y;
            int envZ = startZ + position.z;

            environment[envX, envY, envZ] = tile;
        }
    }

    public bool HasTile(Vector3Int position)
    {
        foreach (Tile tile in tiles)
        {
            Vector3Int thisPosition = new Vector3Int((int)tile.transform.localPosition.x, (int)tile.transform.localPosition.y, (int)tile.transform.localPosition.z);
            
            if (thisPosition == position)
            {
                return true;
            }
        }

        return false;
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
