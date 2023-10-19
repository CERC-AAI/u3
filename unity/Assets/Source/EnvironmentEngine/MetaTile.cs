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
    public Vector3Int rotationDirections = Vector3Int.one;  // allowed rotation directions (0 = rotation not allowed, 1 = rotation allowed)
    public bool canFlip = true;  // allowed rotation directions (0 = rotation not allowed, 1 = rotation allowed)

    public struct Configuration
    {
        public Vector3 origin;
        public MetaTileEnvironment.Orientation orientation;
        public bool flipped;
    }


    List<Configuration> mConfigurationMap = new List<Configuration>();

    // map that corresponds these tuples to indices

    public Configuration GetConfiguration(int index)
    {
        Initialization();
        return mConfigurationMap[index];
    }

    public int GetConfiguration(Vector3 origin, MetaTileEnvironment.Orientation orientation, bool flipped)
    {
        Initialization();
        return mConfigurationMap.FindIndex((Configuration configMap) => { return origin == configMap.origin && orientation == configMap.orientation && flipped == configMap.flipped; });
    }

    public List<Configuration> GetConfigurations()
    {
        Initialization();
        return mConfigurationMap;
    }

    public override List<int> GetTags()
    {
        return tags;
    }

    public override MetaTile DrawMetaTile()
    {
        // TODO: do we need to instantiate a game object here?
        return this;
    }

    public override List<MetaTile> GetMetaTiles()
    {
        List<MetaTile> metaTiles = new List<MetaTile>();
        metaTiles.Add(this);
        return metaTiles;
    }

    public override MetaTile GetMetaTile()
    {
        return this;
    }


    public TileFacePalette GetPalette()
    {
        if (parent != null)
        {
            return parent.palette;
        }

        Debug.LogError("Meta tile had no parent.");
        return Resources.Load<TileFacePalette>(TileFacePaletteEditor.defaultPalettePath);
    }

    public bool CanConnect(int faceType1, int faceType2)
    {
        return GetPalette().CanConnect(faceType1, faceType2);
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
                // Debug.Log("environment[envX, envY, envZ] :" + environment[envX, envY, envZ]);
                return false;  // the metatile would overwrite a tile
            }

            // TODO: check neighbor tiles for adjacency conflicts
            foreach (Tile.FACETYPE faceType in Enum.GetValues(typeof(Tile.FACETYPE)))
            {
                Vector3Int offsetVector = Vector3Int.zero;
                Tile.FACETYPE compareEdge = Tile.FACETYPE.BOTTOM;
                //TODO: deal with rotations
                switch (faceType)
                {
                    case Tile.FACETYPE.TOP:
                        offsetVector = new Vector3Int(0, 1, 0);
                        compareEdge = Tile.FACETYPE.BOTTOM;
                        break;

                    case Tile.FACETYPE.BOTTOM:
                        offsetVector = new Vector3Int(0, -1, 0);
                        compareEdge = Tile.FACETYPE.TOP;
                        break;

                    case Tile.FACETYPE.LEFT:
                        offsetVector = new Vector3Int(-1, 0, 0);
                        compareEdge = Tile.FACETYPE.RIGHT;
                        break;

                    case Tile.FACETYPE.RIGHT:
                        offsetVector = new Vector3Int(1, 0, 0);
                        compareEdge = Tile.FACETYPE.LEFT;
                        break;

                    case Tile.FACETYPE.FRONT:
                        offsetVector = new Vector3Int(0, 0, -1);
                        compareEdge = Tile.FACETYPE.BACK;
                        break;

                    case Tile.FACETYPE.BACK:
                        offsetVector = new Vector3Int(0, 0, 1);
                        compareEdge = Tile.FACETYPE.FRONT;
                        break;
                }


                Tile tempTile = GetTile(environment, environmentPosition + offsetVector);
                if (tempTile != null && !HasTile(position + offsetVector)) //Ignore internal connections
                {
                    if (!CanConnect(tile.faceIDs[(int)faceType], tempTile.faceIDs[(int)compareEdge]))
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

    public void DepositPayload(Vector3Int position, Quaternion rotation, bool flipped, bool debug = false)
    {
        if (payload != null)
        {
            Transform payloadCopy = Instantiate(payload);
            payloadCopy.transform.position = position;
            payloadCopy.transform.localRotation = rotation;

            if (flipped)
            {
                payloadCopy.transform.localScale = new Vector3(1, -1, 1);
            }
        }

        if (debug)
        {
            GameObject tempParent = new GameObject("Tile holder");
            tempParent.transform.position = position;

            foreach (Tile tile in tiles)
            {
                Tile thisTile = Instantiate(tile);

                thisTile.transform.parent = tempParent.transform;
                thisTile.transform.localPosition = tile.transform.localPosition;

                thisTile.SetParent(this);
            }

            tempParent.transform.localRotation = rotation;
        }
    }

    private void Initialization()
    {
        if (mConfigurationMap.Count > 0)
        {
            return;
        }
        foreach (MetaTileEnvironment.Orientation orientation in Enum.GetValues(typeof(MetaTileEnvironment.Orientation)))
        {
            foreach (bool flipped in new List<bool> { false, true })
            {
                if (flipped && !this.canFlip ||
                    MetaTileEnvironment.OrientationToQuaternion[orientation].x != 0 && this.rotationDirections.x == 0 ||
                    MetaTileEnvironment.OrientationToQuaternion[orientation].y != 0 && this.rotationDirections.y == 0 ||
                    MetaTileEnvironment.OrientationToQuaternion[orientation].z != 0 && this.rotationDirections.z == 0)
                {
                    continue;
                }
                foreach (Tile tile in this.tiles) // iterate over every tile in the metatile as the origin
                {

                    Configuration config;
                    config.origin = tile.transform.localPosition;
                    config.orientation = orientation;
                    config.flipped = flipped;


                    mConfigurationMap.Add(config);
                }
            }
        }
    }
}
