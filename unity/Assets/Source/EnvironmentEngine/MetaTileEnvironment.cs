using System.Threading;
using System.Net.Sockets;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor.Tilemaps;

public class MetaTileEnvironment : MonoBehaviour
{
    private Dictionary<MetaTile, List<bool>>[,,] mValidMetaTiles = new Dictionary<MetaTile, List<bool>>[10, 10, 10];
    // 10 by 10 by 10 list of dictionaries. Each dictionary represents a position,
    // you give one dict a metatile and it returns a list of bools that correspond
    // to whether config[i] is legal for that metatile at that position.

    public static int mWidth = 10;
    /*private static Dictionary<int, List<int>> matchingMatrix = new Dictionary<int, List<int>>()
    {
        {0, new List<int> {0,1,2}},
        { 1, new List<int> { 0 } }, // 0 = bedrock connects to bedrock
        { 2, new List<int> { 2} }, // 1 = surface connects to air
        { 3, new List<int> { 1, 2} }, // 2 = air connects to bedrock, floor and air
    };*/

    public Tile[,,] environment = new Tile[mWidth, mWidth, mWidth];

    public int[,,,] faces = new int[3, 11, 11, 11]; // 3 directions, 10x10x10 environment

    public int[,,] environmentEntropies = new int[mWidth, mWidth, mWidth];

    public float voxelSize = 1;

    public MetaTilePool metatilepool;

    private List<MetaTile> metatileList = new List<MetaTile>();


    public List<MetaTile> placedMetaTiles = new List<MetaTile>();
    public List<Quaternion> placedRotations = new List<Quaternion>();
    public List<Vector3Int> placedPositions = new List<Vector3Int>();
    public List<bool> placedFlipped = new List<bool>();

    public enum TileState { NotPlaced, Wavefront, Placed };

    public TileState[,,] tileState = new TileState[10, 10, 10];

    public List<Vector3Int> wavefrontPositions = new();

    public List<float> wavefrontEntropies = new();

    public bool DEBUG = false;
    public Transform debugTile;

    public enum Orientation
    {
        UpFront,
        UpBack,
        UpLeft,
        UpRight,

        DownFront,
        DownBack,
        DownLeft,
        DownRight,

        FrontUp,
        FrontDown,
        FrontLeft,
        FrontRight,

        BackUp,
        BackDown,
        BackLeft,
        BackRight,

        LeftUp,
        LeftDown,
        LeftFront,
        LeftBack,

        RightUp,
        RightDown,
        RightFront,
        RightBack
    }

    public static Dictionary<Orientation, Quaternion> OrientationToQuaternion = new Dictionary<Orientation, Quaternion>
    {
        { Orientation.UpFront, Quaternion.Euler(0, 0, 0) },
        { Orientation.UpBack, Quaternion.Euler(0, 180, 0) },
        { Orientation.UpLeft, Quaternion.Euler(0, -90, 0) },
        { Orientation.UpRight, Quaternion.Euler(0, 90, 0) },

        { Orientation.DownFront, Quaternion.Euler(180, 180, 0) },
        { Orientation.DownBack, Quaternion.Euler(180, 0, 0) },
        { Orientation.DownLeft, Quaternion.Euler(180, -90, 0) },
        { Orientation.DownRight, Quaternion.Euler(180, 90, 0) },

        { Orientation.FrontUp, Quaternion.Euler(90, 180, 0) },
        { Orientation.FrontDown, Quaternion.Euler(90, 0, 0) },
        { Orientation.FrontLeft, Quaternion.Euler(90, -90, 0) },
        { Orientation.FrontRight, Quaternion.Euler(90, 90, 0) },

        { Orientation.BackUp, Quaternion.Euler(-90, 0, 0) },
        { Orientation.BackDown, Quaternion.Euler(-90, 180, 0) },
        { Orientation.BackLeft, Quaternion.Euler(-90, -90, 0) },
        { Orientation.BackRight, Quaternion.Euler(-90, 90, 0) },

        { Orientation.LeftUp, Quaternion.Euler(0, 90, -90) },
        { Orientation.LeftDown, Quaternion.Euler(0, -90, -90) },
        { Orientation.LeftFront, Quaternion.Euler(0, 0, -90) },
        { Orientation.LeftBack, Quaternion.Euler(0, 180, -90) },

        { Orientation.RightUp, Quaternion.Euler(0,-90,90) },
        { Orientation.RightDown, Quaternion.Euler(0, 90, 90) },
        { Orientation.RightFront, Quaternion.Euler(0, 0, 90) },
        { Orientation.RightBack, Quaternion.Euler(0, 180, 90) },
    };

    public static readonly Dictionary<Orientation, List<Tile.FACETYPE>> OrientationToPermutation = new Dictionary<Orientation, List<Tile.FACETYPE>>
    {
        { Orientation.UpFront, new List<Tile.FACETYPE> { Tile.FACETYPE.TOP, Tile.FACETYPE.BOTTOM, Tile.FACETYPE.LEFT, Tile.FACETYPE.RIGHT, Tile.FACETYPE.FRONT, Tile.FACETYPE.BACK } },
        { Orientation.UpBack, new List<Tile.FACETYPE> { Tile.FACETYPE.TOP, Tile.FACETYPE.BOTTOM, Tile.FACETYPE.RIGHT, Tile.FACETYPE.LEFT, Tile.FACETYPE.BACK, Tile.FACETYPE.FRONT } },
        { Orientation.UpLeft, new List<Tile.FACETYPE> { Tile.FACETYPE.TOP, Tile.FACETYPE.BOTTOM, Tile.FACETYPE.BACK, Tile.FACETYPE.FRONT, Tile.FACETYPE.LEFT, Tile.FACETYPE.RIGHT } },
        { Orientation.UpRight, new List<Tile.FACETYPE> { Tile.FACETYPE.TOP, Tile.FACETYPE.BOTTOM, Tile.FACETYPE.FRONT, Tile.FACETYPE.BACK, Tile.FACETYPE.RIGHT, Tile.FACETYPE.LEFT } },

        { Orientation.DownFront, new List<Tile.FACETYPE> { Tile.FACETYPE.BOTTOM, Tile.FACETYPE.TOP, Tile.FACETYPE.RIGHT, Tile.FACETYPE.LEFT, Tile.FACETYPE.FRONT, Tile.FACETYPE.BACK } },
        { Orientation.DownBack, new List<Tile.FACETYPE> { Tile.FACETYPE.BOTTOM, Tile.FACETYPE.TOP, Tile.FACETYPE.LEFT, Tile.FACETYPE.RIGHT, Tile.FACETYPE.BACK, Tile.FACETYPE.FRONT } },
        { Orientation.DownLeft, new List<Tile.FACETYPE> { Tile.FACETYPE.BOTTOM, Tile.FACETYPE.TOP, Tile.FACETYPE.FRONT, Tile.FACETYPE.BACK, Tile.FACETYPE.LEFT, Tile.FACETYPE.RIGHT } },
        { Orientation.DownRight, new List<Tile.FACETYPE> { Tile.FACETYPE.BOTTOM, Tile.FACETYPE.TOP, Tile.FACETYPE.BACK, Tile.FACETYPE.FRONT, Tile.FACETYPE.RIGHT, Tile.FACETYPE.LEFT } },

        { Orientation.FrontUp, new List<Tile.FACETYPE> { Tile.FACETYPE.FRONT, Tile.FACETYPE.BACK, Tile.FACETYPE.RIGHT, Tile.FACETYPE.LEFT, Tile.FACETYPE.TOP, Tile.FACETYPE.BOTTOM } },
        { Orientation.FrontDown, new List<Tile.FACETYPE> { Tile.FACETYPE.FRONT, Tile.FACETYPE.BACK, Tile.FACETYPE.LEFT, Tile.FACETYPE.RIGHT, Tile.FACETYPE.BOTTOM, Tile.FACETYPE.TOP } },
        { Orientation.FrontLeft, new List<Tile.FACETYPE> { Tile.FACETYPE.FRONT, Tile.FACETYPE.BACK, Tile.FACETYPE.TOP, Tile.FACETYPE.BOTTOM, Tile.FACETYPE.LEFT, Tile.FACETYPE.RIGHT } },
        { Orientation.FrontRight, new List<Tile.FACETYPE> { Tile.FACETYPE.FRONT, Tile.FACETYPE.BACK, Tile.FACETYPE.BOTTOM, Tile.FACETYPE.TOP, Tile.FACETYPE.RIGHT, Tile.FACETYPE.LEFT } },

        { Orientation.BackUp, new List<Tile.FACETYPE> { Tile.FACETYPE.BACK, Tile.FACETYPE.FRONT, Tile.FACETYPE.LEFT, Tile.FACETYPE.RIGHT, Tile.FACETYPE.TOP, Tile.FACETYPE.BOTTOM } },
        { Orientation.BackDown, new List<Tile.FACETYPE> { Tile.FACETYPE.BACK, Tile.FACETYPE.FRONT, Tile.FACETYPE.RIGHT, Tile.FACETYPE.LEFT, Tile.FACETYPE.BOTTOM, Tile.FACETYPE.TOP } },
        { Orientation.BackLeft, new List<Tile.FACETYPE> { Tile.FACETYPE.BACK, Tile.FACETYPE.FRONT, Tile.FACETYPE.BOTTOM, Tile.FACETYPE.TOP, Tile.FACETYPE.LEFT, Tile.FACETYPE.RIGHT } },
        { Orientation.BackRight, new List<Tile.FACETYPE> { Tile.FACETYPE.BACK, Tile.FACETYPE.FRONT, Tile.FACETYPE.TOP, Tile.FACETYPE.BOTTOM, Tile.FACETYPE.RIGHT, Tile.FACETYPE.LEFT } },

        { Orientation.LeftUp, new List<Tile.FACETYPE> { Tile.FACETYPE.LEFT, Tile.FACETYPE.RIGHT, Tile.FACETYPE.FRONT, Tile.FACETYPE.BACK, Tile.FACETYPE.TOP, Tile.FACETYPE.BOTTOM } },
        { Orientation.LeftDown, new List<Tile.FACETYPE> { Tile.FACETYPE.LEFT, Tile.FACETYPE.RIGHT, Tile.FACETYPE.BACK, Tile.FACETYPE.FRONT, Tile.FACETYPE.BOTTOM, Tile.FACETYPE.TOP } },
        { Orientation.LeftFront, new List<Tile.FACETYPE> { Tile.FACETYPE.LEFT, Tile.FACETYPE.RIGHT, Tile.FACETYPE.BOTTOM, Tile.FACETYPE.TOP, Tile.FACETYPE.FRONT, Tile.FACETYPE.BACK } },
        { Orientation.LeftBack, new List<Tile.FACETYPE> { Tile.FACETYPE.LEFT, Tile.FACETYPE.RIGHT, Tile.FACETYPE.TOP, Tile.FACETYPE.BOTTOM, Tile.FACETYPE.BACK, Tile.FACETYPE.FRONT } },

        { Orientation.RightUp, new List<Tile.FACETYPE> { Tile.FACETYPE.RIGHT, Tile.FACETYPE.LEFT, Tile.FACETYPE.BACK, Tile.FACETYPE.FRONT, Tile.FACETYPE.TOP, Tile.FACETYPE.BOTTOM } },
        { Orientation.RightDown, new List<Tile.FACETYPE> { Tile.FACETYPE.RIGHT, Tile.FACETYPE.LEFT, Tile.FACETYPE.FRONT, Tile.FACETYPE.BACK, Tile.FACETYPE.BOTTOM, Tile.FACETYPE.TOP } },
        { Orientation.RightFront, new List<Tile.FACETYPE> { Tile.FACETYPE.RIGHT, Tile.FACETYPE.LEFT, Tile.FACETYPE.TOP, Tile.FACETYPE.BOTTOM, Tile.FACETYPE.FRONT, Tile.FACETYPE.BACK } },
        { Orientation.RightBack, new List<Tile.FACETYPE> { Tile.FACETYPE.RIGHT, Tile.FACETYPE.LEFT, Tile.FACETYPE.BOTTOM, Tile.FACETYPE.TOP, Tile.FACETYPE.BACK, Tile.FACETYPE.FRONT } }
    };


    public List<MetaTileProbability> mDynamicWeights = new List<MetaTileProbability>();

    public void UpdateDynamicWeights()
    {
        //Called whenever you add a tile to the environment. This can either recalculate the weights, or just refresh the list using the "DeepCopy" function.
        // TODO: encapsulate in MetaTilePool
        // mDynamicWeights = metatilepool.UpdateDynamicWeights();
    }

    public float GetMetaTileWeight(MetaTile tile)
    {
        int index = mDynamicWeights.FindIndex((MetaTileProbability thisTile) => thisTile.metaTileProbability.GetMetaTile() == tile);

        if (index == -1)
        {
            return 0;
        }

        return mDynamicWeights[index].weight;
    }

    public void RecalculateMetaTileValidity(Vector3Int wavefrontPosition)
    {
        //Helper function to update the monster structure above. Should deal with pruning/initalizing the dictionary as needed.

        Dictionary<MetaTile, List<bool>> wavefrontPositionMetaTileValidityDict = mValidMetaTiles[wavefrontPosition.x, wavefrontPosition.y, wavefrontPosition.z];
        if (wavefrontPositionMetaTileValidityDict == null)
        {
            mValidMetaTiles[wavefrontPosition.x, wavefrontPosition.y, wavefrontPosition.z] = new Dictionary<MetaTile, List<bool>>();
            foreach (MetaTile metatile in metatileList)
            {
                mValidMetaTiles[wavefrontPosition.x, wavefrontPosition.y, wavefrontPosition.z][metatile] = new List<bool>();
                for (int i = 0; i < metatile.GetConfigurations().Count; i++)
                {
                    mValidMetaTiles[wavefrontPosition.x, wavefrontPosition.y, wavefrontPosition.z][metatile].Add(true);
                }
            }
            wavefrontPositionMetaTileValidityDict = mValidMetaTiles[wavefrontPosition.x, wavefrontPosition.y, wavefrontPosition.z];
        }

        // print($"wavefrontPositionMetaTileValidityDict.Count: {wavefrontPositionMetaTileValidityDict.Count}");

        foreach (MetaTile metatile in wavefrontPositionMetaTileValidityDict.Keys)
        {
            List<bool> validConfigurations = wavefrontPositionMetaTileValidityDict[metatile];

            // print($"validConfigurations.Count: {validConfigurations.Count}");
            // if (validConfigurations == null)
            // {
            //     mValidMetaTiles[wavefrontPosition.x, wavefrontPosition.y, wavefrontPosition.z][metatile] = new List<bool>();
            //     validConfigurations = mValidMetaTiles[wavefrontPosition.x, wavefrontPosition.y, wavefrontPosition.z][metatile];
            // }

            for (int i = 0; i < validConfigurations.Count(); i++)
            {
                if (validConfigurations[i])
                {
                    MetaTile.Configuration configTuple = metatile.GetConfiguration(i);
                    bool canPlace = CanPlaceMetaTile(wavefrontPosition, metatile, configTuple.origin, configTuple.orientation, configTuple.flipped);
                    if (!canPlace)
                    {
                        validConfigurations[i] = false;
                    }
                }
            }

            // sampledMetaTileisAllowed[sampledMetaTiles.IndexOf(metatile)] = isAllowed;
        }

    }

    // public int CountTotalConfigurations(Vector3Int position)
    // {

    // }

    public int CountValidConfigurations(Vector3Int position)
    {
        //Helper function used for entropy calculation. Counts the number of valid metatiles. Not sure which of these functions to use for the entropy calculation.
        // at least 1 valid configuration
        int count = 0;
        foreach (KeyValuePair<MetaTile, List<bool>> pair in mValidMetaTiles[position.x, position.y, position.z])
        {
            foreach (bool validConfiguration in pair.Value)
            {
                if (validConfiguration)
                {
                    count++;
                }
            }
        }
        return count;
    }

    public int CountValidMetaTiles(Vector3Int position)
    {
        //Helper function used for entropy calculation. Counts the number of valid metatiles. Not sure which of these functions to use for the entropy calculation.
        // at least 1 valid configuration
        int count = 0;
        foreach (KeyValuePair<MetaTile, List<bool>> pair in mValidMetaTiles[position.x, position.y, position.z])
        {
            if (pair.Value.Contains(true))
            {
                count++;
            }

        }

        // print($"count : {count}");
        return count;
    }

    public MetaTile DrawMetaTile(List<MetaTile> list)
    {
        //Helper function to draw a metatile (respecting pool weights) for a given list,
        //or a given position. The only difference is if you want to encapsulate the
        //drawing procedure, or make it specific to the environment format...
        //probably doesn't really matter. You could even write both functions.

        //for each metatile in the list, get the corresponding weight from the dynamic weights list
        //and add it to a list of weights
        List<float> weights = new List<float>();
        foreach (MetaTile metatile in list)
        {
            weights.Add(GetMetaTileWeight(metatile));
        }

        //use the weights to select a random metatile from the list
        float totalWeight = 0;
        foreach (float weight in weights)
        {
            totalWeight += weight;
        }

        float randomWeight = UnityEngine.Random.value * totalWeight;

        for (int i = 0; i < list.Count; i++)
        {
            if (randomWeight < weights[i])
            {
                MetaTile metatileForRemoval = list[i];
                return metatileForRemoval;
            }
            randomWeight -= weights[i];
        }

        //select a random metatile from the list using the weights
        return null;


    }

    public MetaTile DrawMetaTile(Vector3Int position)
    {
        List<MetaTile> list = mValidMetaTiles[position.x, position.y, position.z].Keys.ToList();
        return DrawMetaTile(list);

    }

    public Vector3Int SelectPlacementPosition()
    {
        // Selects a position to place a metatile
        if (placedMetaTiles.Count == 0)
        {
            // if the wavefront is empty, select a random empty position
            // there might already be pre-placed metatiles
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
                Debug.Log("SelectPlacementPosition() no empty positions");
                return new Vector3Int(-1, -1, -1);
            }

            // Select a random empty position
            Debug.Log("SelectPlacementPosition() emptyPositions.Count: " + emptyPositions.Count);
            return emptyPositions[UnityEngine.Random.Range(0, emptyPositions.Count)];
        }
        else
        {
            if (wavefrontPositions.Count == 0)
            {
                Debug.Log("SelectPlacementPosition() no wavefront positions");
                return new Vector3Int(-1, -1, -1);
            }

            float minEntropyValue = wavefrontEntropies.Min();

            int minEntropyIndex = wavefrontEntropies.IndexOf(minEntropyValue);

            Vector3Int minEntropyPosition = wavefrontPositions[minEntropyIndex];
            // select the position with the minimum entropy value
            Debug.Log("SelectPlacementPosition() placementPosition: " + minEntropyPosition);
            return minEntropyPosition;
        }

    }

    public Tile GetTile(Vector3Int position)
    {
        if (position.x < 0 || position.x >= environment.GetLength(0) ||
            position.y < 0 || position.y >= environment.GetLength(1) ||
            position.z < 0 || position.z >= environment.GetLength(2))
        {
            return null;  // the tile is out of bounds
        }

        return environment[position.x, position.y, position.z];
    }

    string GetFaceName(int faceID)
    {
        if (faceID == -1)
        {
            return "Empty";
        }

        return metatilepool.palette.tileFaces[faceID].name;
    }

    float GetFaceWeight(int faceID)
    {
        if (faceID == -1)
        {
            return 0;
        }

        return metatilepool.palette.tileFaces[faceID].weight;
    }

    public List<int> GetFaceList(Vector3Int position, bool debug = false)
    {
        List<int> faceList = new List<int>();

        // add faces from the x direction
        int left = faces[0, position.x, position.y, position.z];
        int right = faces[0, position.x + 1, position.y, position.z];
        int bottom = faces[1, position.x, position.y, position.z];
        int top = faces[1, position.x, position.y + 1, position.z];
        int front = faces[2, position.x, position.y, position.z];
        int back = faces[2, position.x, position.y, position.z + 1];

        // create a list of lists of faces
        faceList.Add(top);
        faceList.Add(bottom);
        faceList.Add(left);
        faceList.Add(right);
        faceList.Add(front);
        faceList.Add(back);

        /*if (debug)
        {
            Debug.Log($"Get placed faces {position}:");
            Debug.Log($"    Top: {GetFaceName(faceList[(int)Tile.FACETYPE.TOP])}");
            Debug.Log($"    Bottom: {GetFaceName(faceList[(int)Tile.FACETYPE.BOTTOM])}");
            Debug.Log($"    Left: {GetFaceName(faceList[(int)Tile.FACETYPE.LEFT])}");
            Debug.Log($"    Right: {GetFaceName(faceList[(int)Tile.FACETYPE.RIGHT])}");
            Debug.Log($"    Front: {GetFaceName(faceList[(int)Tile.FACETYPE.FRONT])}");
            Debug.Log($"    Back: {GetFaceName(faceList[(int)Tile.FACETYPE.BACK])}");
        }*/

        return faceList;
    }

    public void SetFaceList(Vector3Int position, List<int> faceList, bool debug = false)
    {
        faces[1, position.x, position.y + 1, position.z] = faceList[(int)Tile.FACETYPE.TOP];
        faces[1, position.x, position.y, position.z] = faceList[(int)Tile.FACETYPE.BOTTOM];
        faces[0, position.x, position.y, position.z] = faceList[(int)Tile.FACETYPE.LEFT];
        faces[0, position.x + 1, position.y, position.z] = faceList[(int)Tile.FACETYPE.RIGHT];
        faces[2, position.x, position.y, position.z] = faceList[(int)Tile.FACETYPE.FRONT];
        faces[2, position.x, position.y, position.z + 1] = faceList[(int)Tile.FACETYPE.BACK];

        /*if (debug)
        {
            Debug.Log($"Placed faces {position}:");
            Debug.Log($"    Top: {GetFaceName(faceList[(int)Tile.FACETYPE.TOP])}");
            Debug.Log($"    Bottom: {GetFaceName(faceList[(int)Tile.FACETYPE.BOTTOM])}");
            Debug.Log($"    Left: {GetFaceName(faceList[(int)Tile.FACETYPE.LEFT])}");
            Debug.Log($"    Right: {GetFaceName(faceList[(int)Tile.FACETYPE.RIGHT])}");
            Debug.Log($"    Front: {GetFaceName(faceList[(int)Tile.FACETYPE.FRONT])}");
            Debug.Log($"    Back: {GetFaceName(faceList[(int)Tile.FACETYPE.BACK])}");
        }*/

    }

    public List<Vector3Int> GetAdjacentPositions(Vector3Int Position)
    {
        List<Vector3Int> adjacentPositions = new List<Vector3Int>();
        adjacentPositions.Add(new Vector3Int(Position.x - 1, Position.y, Position.z));
        adjacentPositions.Add(new Vector3Int(Position.x + 1, Position.y, Position.z));
        adjacentPositions.Add(new Vector3Int(Position.x, Position.y - 1, Position.z));
        adjacentPositions.Add(new Vector3Int(Position.x, Position.y + 1, Position.z));
        adjacentPositions.Add(new Vector3Int(Position.x, Position.y, Position.z - 1));
        adjacentPositions.Add(new Vector3Int(Position.x, Position.y, Position.z + 1));

        // remove any illegal positions
        adjacentPositions.RemoveAll(position => position.x < 0 || position.x >= environment.GetLength(0) ||
            position.y < 0 || position.y >= environment.GetLength(1) ||
            position.z < 0 || position.z >= environment.GetLength(2));

        return adjacentPositions;
    }

    public List<List<int>> GetPossibleFaces(Vector3Int position)
    {
        List<int> faceList = GetFaceList(position, DEBUG);

        // return the list of faces permitted for each face at the position according to the matching matrix
        List<List<int>> possibleFaces = new List<List<int>>();
        foreach (int face in faceList)
        {
            //Untouched
            if (face == -1)
            {
                possibleFaces.Add(Enumerable.Range(0, metatilepool.palette.tileFaces.Count).ToList());
            }
            else
            {
                possibleFaces.Add(metatilepool.palette.GetPossibleConnections(face));
            }
        }

        return possibleFaces;
    }

    public List<MetaTileProbability> CustomFiltering(Vector3Int placementPosition, List<MetaTileProbability> metatiles)
    {
        List<MetaTileProbability> filteredMetatiles = new List<MetaTileProbability>();
        // TODO: look into how WFC normally does this
        // TODO: allow metatilePools to weight dynamically (decay weights over time)
        List<List<int>> possibleFaces = GetPossibleFaces(placementPosition);
        // select the shortest list of faces in possible faces and name it filterFaces
        List<int> filterFaces = new List<int>();
        int filterFaceIdx = 0;
        int shortestListLength = int.MaxValue;
        // FIX ME
        //foreach (List<int> faceList in possibleFaces)
        for (int i = 0; i < possibleFaces.Count; i++)
        {
            if (possibleFaces[i].Count < shortestListLength)
            {
                shortestListLength = possibleFaces[i].Count;
                filterFaces = possibleFaces[i];
                filterFaceIdx = i;
            }
        }

        // filter the list of metatiles by the possible faces in filterFaces
        // the metatile must contain at least one tile with a face in filterFaces
        // if this tile has a face in filterFaces, then check the other faces of the tile against the other faces in possibleFaces
        // this is an example of a pre-filtering pass
        foreach (MetaTileProbability metatile in metatiles)
        {
            foreach (Tile tile in metatile.metaTileProbability.GetMetaTile().tiles)
            {
                //Vector3Int tilePosition = new Vector3Int((int)tile.transform.localPosition.x, (int)tile.transform.localPosition.y, (int)tile.transform.localPosition.z);
                //FIX ME
                //List<int> tileFaces = GetFaceList(placementPosition + tilePosition);
                List<int> tileFaces = new List<int>(tile.faceIDs);
                bool tileIsLegal = false;
                for (int i = 0; i < tileFaces.Count; i++)
                {
                    if (filterFaces.Contains(tileFaces[i]))
                    {
                        tileIsLegal = true;
                        break;
                    }
                }
                if (tileIsLegal)
                {
                    filteredMetatiles.Add(metatile);
                    break;
                }
            }
        }

        return filteredMetatiles;
    }

    /*public MetaTile DrawMetaTile(Vector3Int placementPosition, MetaTilePool metatilepool)
    {
        // Create a list of all legal metatiles from the pool
        List<MetaTileProbability> metatiles = new List<MetaTileProbability>();

        if (DEBUG)
        {
            Debug.Log("Metatile Count before adding: " + metatiles.Count);
        }

        foreach (MetaTileProbability metatileprobability in metatilepool.metatileProbabilities)
        {
            metatiles.Add(metatileprobability.metaTileProbability.DrawMetaTile(placementPosition, environment, faces, metatilepool.palette));
        }

        if (DEBUG)
        {
            Debug.Log("Metatile Count after adding: " + metatiles.Count);
        }

        List<MetaTile> filteredMetatiles = CustomFiltering(placementPosition, metatiles);

        if (DEBUG)
        {
            Debug.Log("Metatile Count after filtering: " + filteredMetatiles.Count);
        }

        // select a random metatile from the filtered list
        return filteredMetatiles[UnityEngine.Random.Range(0, filteredMetatiles.Count)];
    }*/

    public List<MetaTileProbability> GetMetaTiles(Vector3Int placementPosition, MetaTilePool metatilepool)
    {
        // Create a list of all legal metatiles from the pool
        List<MetaTileProbability> metatiles = new List<MetaTileProbability>();

        if (DEBUG)
        {
            Debug.Log("Metatile Count before adding: " + metatiles.Count);
        }

        // TODO: fix this to exhaustively grab all possible metatiles
        foreach (MetaTileProbability metatileprobability in metatilepool.metatileProbabilities)
        {
            metatiles.Add(metatileprobability);
        }

        if (DEBUG)
        {
            Debug.Log("Metatile Count after adding: " + metatiles.Count);
        }

        List<MetaTileProbability> filteredMetatiles = CustomFiltering(placementPosition, metatiles);

        if (DEBUG)
        {
            Debug.Log("Metatile Count after filtering: " + filteredMetatiles.Count);
        }

        return filteredMetatiles;

        /*if (filteredMetatiles.Count > 0)
        {
            // select a random metatile from the filtered list
            return filteredMetatiles[UnityEngine.Random.Range(0, filteredMetatiles.Count)];
        }
        else
        {
            return null;
        }*/
    }

    private bool CanPlaceMetaTile(Vector3Int placementPosition, MetaTile metatile, Vector3 currentOrigin, Orientation orientation, bool flipped)
    {
        if (flipped && !metatile.canFlip ||
            OrientationToQuaternion[orientation].x != 0 && metatile.rotationDirections.x == 0 ||
            OrientationToQuaternion[orientation].y != 0 && metatile.rotationDirections.y == 0 ||
            OrientationToQuaternion[orientation].z != 0 && metatile.rotationDirections.z == 0)
        {
            return false;
        }
        foreach (Tile tile in metatile.tiles)
        {
            // from enum to list of integers that corresponds to a permuted list of faces

            List<Tile.FACETYPE> permutation = new List<Tile.FACETYPE>(OrientationToPermutation[orientation]);

            // multiply the tile position by a quaternion
            UnityEngine.Vector3 unRotatedPosition = new(tile.transform.localPosition.x, tile.transform.localPosition.y, tile.transform.localPosition.z);
            if (flipped)
            {
                unRotatedPosition.y = unRotatedPosition.y * -1;
                //(permutation[1], permutation[0]) = (permutation[0], permutation[1]);
                int topIndex = permutation.IndexOf(Tile.FACETYPE.TOP);
                int bottomIndex = permutation.IndexOf(Tile.FACETYPE.BOTTOM);
                (permutation[bottomIndex], permutation[topIndex]) = (permutation[topIndex], permutation[bottomIndex]);

            }
            Vector3 rotatedPosition = OrientationToQuaternion[orientation] * unRotatedPosition;

            Vector3 newRotatedPosition = rotatedPosition - currentOrigin;
            // not sure if you can multiply Vector3Ints by quaternion
            Vector3Int tilePosition = new Vector3Int(Mathf.RoundToInt(newRotatedPosition.x), Mathf.RoundToInt(newRotatedPosition.y), Mathf.RoundToInt(newRotatedPosition.z));

            Vector3Int environmentPosition = placementPosition + tilePosition;

            if (environmentPosition.x < 0 || environmentPosition.x >= environment.GetLength(0) ||
                environmentPosition.y < 0 || environmentPosition.y >= environment.GetLength(1) ||
                environmentPosition.z < 0 || environmentPosition.z >= environment.GetLength(2))
            {
                return false;  // the metatile is out of bounds
            }

            if (GetTile(environmentPosition) != null)
            {
                // Debug.Log("environment[envX, envY, envZ] :" + environment[envX, envY, envZ]);
                return false;  // the metatile would overwrite a tile
            }

            // TODO: check neighbor tiles for adjacency conflicts
            List<List<int>> possibleFaces = GetPossibleFaces(environmentPosition);
            List<int> tileFaces = new List<int>(tile.faceIDs);
            for (int i = 0; i < tileFaces.Count; i++)
            {
                // if the face is not in the list of possible faces at the index of the face in tileFaces, return false
                // tilefaces[permutation[i]]

                if (!possibleFaces[i].Contains(tileFaces[(int)permutation[i]]))
                {
                    return false;
                }
            }

        }

        return true;  // no conflicts were found

    }

    public void PlaceMetaTile(Vector3Int placementPosition, MetaTile metatile, Orientation orientation, bool flipped)
    {
        //Debug.Log("placing metatile " + this.name);

        foreach (Tile tile in metatile.tiles)
        {
            List<Tile.FACETYPE> permutation = new List<Tile.FACETYPE>(OrientationToPermutation[orientation]);

            // multiply the tile position by a quaternion
            UnityEngine.Vector3 unRotatedPosition = tile.transform.localPosition;
            if (flipped)
            {
                unRotatedPosition.y = unRotatedPosition.y * -1;
                //(permutation[1], permutation[0]) = (permutation[0], permutation[1]);
                int topIndex = permutation.IndexOf(Tile.FACETYPE.TOP);
                int bottomIndex = permutation.IndexOf(Tile.FACETYPE.BOTTOM);
                (permutation[bottomIndex], permutation[topIndex]) = (permutation[topIndex], permutation[bottomIndex]);
            }
            UnityEngine.Vector3 rotatedPosition = OrientationToQuaternion[orientation] * unRotatedPosition;
            // not sure if you can multiply Vector3Ints by quaternion
            Vector3Int tilePosition = new Vector3Int(Mathf.RoundToInt(rotatedPosition.x), Mathf.RoundToInt(rotatedPosition.y), Mathf.RoundToInt(rotatedPosition.z));

            int envX = placementPosition.x + tilePosition.x;
            int envY = placementPosition.y + tilePosition.y;
            int envZ = placementPosition.z + tilePosition.z;

            environment[envX, envY, envZ] = tile;

            List<int> tempIDs = new List<int>();
            for (int i = 0; i < permutation.Count && i < tile.faceIDs.Length; i++)
            {
                tempIDs.Add(tile.faceIDs[(int)permutation[i]]);
            }

            Vector3Int position = new Vector3Int(envX, envY, envZ);

            // update the faces
            // TODO: replace with TOP, BOTTOM, etc. enum
            //Debug.Log($"Tile face {tile.faceIDs[0]}");
            SetFaceList(new Vector3Int(envX, envY, envZ), tempIDs, DEBUG);

            if (DEBUG)
            {
                //Draw each face
                for (Tile.FACETYPE i = Tile.FACETYPE.TOP; i <= Tile.FACETYPE.BACK; i++)
                {
                    Color color = Color.clear;
                    Vector3 facePosition = Vector3.zero;
                    Vector3 size = Vector3.zero;
                    string name = "";

                    int faceID = tempIDs[(int)i];
                    TileFace faceData = metatilepool.palette.tileFaces[faceID];
                    switch (i)
                    {
                        case Tile.FACETYPE.TOP:
                            color = faceData.color;
                            facePosition = (Vector3)position + new Vector3(0, voxelSize / 2 * 1.01f, 0);
                            size = transform.rotation * new Vector3(voxelSize / 2, 0, voxelSize / 2);
                            name = $"{permutation[(int)i]}";
                            break;

                        case Tile.FACETYPE.BOTTOM:
                            color = faceData.color;
                            facePosition = (Vector3)position - new Vector3(0, voxelSize / 2 * 1.01f, 0);
                            size = transform.rotation * new Vector3(voxelSize / 2, 0, voxelSize / 2);
                            name = $"{permutation[(int)i]}";
                            break;

                        case Tile.FACETYPE.LEFT:
                            color = faceData.color;
                            facePosition = (Vector3)position - new Vector3(voxelSize / 2 * 1.01f, 0, 0);
                            size = transform.rotation * new Vector3(0, voxelSize / 2, voxelSize / 2);
                            name = $"{permutation[(int)i]}";
                            break;

                        case Tile.FACETYPE.RIGHT:
                            color = faceData.color;
                            facePosition = (Vector3)position + new Vector3(voxelSize / 2 * 1.01f, 0, 0);
                            size = transform.rotation * new Vector3(0, voxelSize / 2, voxelSize / 2);
                            name = $"{permutation[(int)i]}";
                            break;

                        case Tile.FACETYPE.FRONT:
                            color = faceData.color;
                            facePosition = (Vector3)position - new Vector3(0, 0, voxelSize / 2 * 1.01f);
                            size = transform.rotation * new Vector3(voxelSize / 2, voxelSize / 2, 0);
                            name = $"{permutation[(int)i]}";
                            break;

                        case Tile.FACETYPE.BACK:
                            color = faceData.color;
                            facePosition = (Vector3)position + new Vector3(0, 0, voxelSize / 2 * 1.01f);
                            size = transform.rotation * new Vector3(voxelSize / 2, voxelSize / 2, 0);
                            name = $"{permutation[(int)i]}";
                            break;
                    }

                    Transform tempObject = GameObject.Instantiate(debugTile);
                    tempObject.GetComponent<Renderer>().material.SetColor("_Color", color);
                    tempObject.position = facePosition;
                    tempObject.localScale = size;
                    tempObject.name = $"Tile_{name}_{position}";
                }
            }

            if (DEBUG)
            {
                Debug.Log($"Placed faces {new Vector3Int(envX, envY, envZ)} - {orientation} - flipped? {flipped}:");
                Debug.Log($"    Top: {GetFaceName(tempIDs[(int)Tile.FACETYPE.TOP])}");
                Debug.Log($"    Bottom: {GetFaceName(tempIDs[(int)Tile.FACETYPE.BOTTOM])}");
                Debug.Log($"    Left: {GetFaceName(tempIDs[(int)Tile.FACETYPE.LEFT])}");
                Debug.Log($"    Right: {GetFaceName(tempIDs[(int)Tile.FACETYPE.RIGHT])}");
                Debug.Log($"    Front: {GetFaceName(tempIDs[(int)Tile.FACETYPE.FRONT])}");
                Debug.Log($"    Back: {GetFaceName(tempIDs[(int)Tile.FACETYPE.BACK])}");
            }

            if (tileState[envX, envY, envZ] == TileState.Placed)
            {
                // throw an error and stop the program
                Debug.Log("envX: " + envX + " envY: " + envY + " envZ: " + envZ);
                Debug.Log("environment[envX, envY, envZ] :" + environment[envX, envY, envZ]);
                Debug.Log("tileState[envX, envY, envZ] :" + tileState[envX, envY, envZ]);
                throw new Exception("OverWriteError: tileState[envX, envY, envZ] == TileState.Placed");
            }

            else if (tileState[envX, envY, envZ] == TileState.Wavefront)
            {
                // get the index of the position in the wavefrontPositions list
                int wavefrontIndex = wavefrontPositions.IndexOf(new Vector3Int(envX, envY, envZ));

                // remove the position from the wavefrontPositions list
                wavefrontPositions.RemoveAt(wavefrontIndex);

                // remove the entropy from the wavefrontEntropies list
                wavefrontEntropies.RemoveAt(wavefrontIndex);
            }

            // update the tile state
            tileState[envX, envY, envZ] = TileState.Placed;

            // add the neighbors of the tile to the wavefront if their tile state is not placed
            List<Vector3Int> adjacentPositions = GetAdjacentPositions(new Vector3Int(envX, envY, envZ));

            // add the adjacent positions to the wavefront if they are not placed
            foreach (Vector3Int adjacentPosition in adjacentPositions)
            {
                if (tileState[adjacentPosition.x, adjacentPosition.y, adjacentPosition.z] == TileState.NotPlaced)
                {
                    // update the tile state
                    tileState[adjacentPosition.x, adjacentPosition.y, adjacentPosition.z] = TileState.Wavefront;

                    // add the position to the wavefrontPositions list
                    wavefrontPositions.Add(adjacentPosition);

                    // add the entropy to the wavefrontEntropies list
                    wavefrontEntropies.Add(-1);
                }
            }
        }

    }

    public void CollapseWaveFunction()
    {
        for (int position = 0; position < wavefrontPositions.Count; position++)
        {
            RecalculateMetaTileValidity(wavefrontPositions[position]);
            wavefrontEntropies[position] = CalculateEntropy(wavefrontPositions[position]);
        }

    }

    // private List<MetaTile> SampleMetaTiles(int num_sampled_metatiles)
    // {

    //     List<MetaTileProbability> metatilepoolForSampling = metatilepool.BuildMetaTilePoolDeepCopy();
    //     Debug.Log("metatilepoolForSampling.metatileProbabilities.Count: " + metatilepoolForSampling.Count);

    //     List<MetaTile> sampledMetaTiles = new List<MetaTile>();
    //     for (int i = 0; i < num_sampled_metatiles; i++)
    //     {
    //         // draw without replacement
    //         MetaTile sampledMetaTile = MetaTilePool.DrawMetaTileWithoutReplacement(metatilepoolForSampling);
    //         if (sampledMetaTile == null)
    //         {
    //             // if null then stop
    //             Debug.Log("CollapseWaveFunction: ran out of metatiles to sample");
    //             break;
    //         }
    //         sampledMetaTiles.Add(sampledMetaTile);
    //     }

    //     return (sampledMetaTiles);
    // }

    public float CalculateEntropy(Vector3Int wavefrontPosition)
    {

        List<int> wavefrontPositionFaces = GetFaceList(wavefrontPosition);
        float faceWeight = 0;
        foreach (int face in wavefrontPositionFaces)
        {
            if (face != -1)
            {
                faceWeight += metatilepool.palette.tileFaces[face].weight;
            }
        }
        return CountValidMetaTiles(wavefrontPosition) / (faceWeight + 0.001f);

        // // Do full translation/rotation tests and cache adjacent tile legality
        // // Dilation of tile checks and translation?
        // // Add face priorities to the metatiles, when approxmating count, multiply

        // // TODO: every position has a dict of {metatile, list of valid configurations}
        // List<bool> sampledMetaTileisAllowed = Enumerable.Repeat(false, sampledMetaTiles.Count).ToList();
        // float sumWeight = 0;

        // foreach (MetaTile metatile in sampledMetaTiles)
        // {
        //     bool isAllowed = false;
        //     foreach (Orientation orientation in Enum.GetValues(typeof(Orientation)))
        //     {
        //         foreach (bool flipped in new List<bool> { false, true })
        //         {
        //             foreach (Tile tile in metatile.tiles) // iterate over every tile in the metatile as the origin
        //             {
        //                 if (CanPlaceMetaTile(wavefrontPosition, metatile, tile.transform.localPosition, orientation, flipped))
        //                 {
        //                     isAllowed = true;
        //                     sumWeight += 1;
        //                     break;
        //                 }

        //             }
        //             if (isAllowed)
        //             {
        //                 break;
        //             }
        //         }
        //         if (isAllowed)
        //         {
        //             break;
        //         }
        //     }
        //     // sampledMetaTileisAllowed[sampledMetaTiles.IndexOf(metatile)] = isAllowed;
        // }

        // // return sumWeight;

        // List<int> wavefrontPositionFaces = GetFaceList(wavefrontPosition);
        // float faceWeight = 0;
        // foreach (int face in wavefrontPositionFaces)
        // {
        //     if (face != -1)
        //     {
        //         faceWeight += metatilepool.palette.tileFaces[face].weight;
        //     }
        // }
        // return sumWeight / (faceWeight + 0.001f);
    }

    /*public bool ValidateOrientation(Tile.FACETYPE topOrientation, Tile.FACETYPE frontOrientation, Tile.FACETYPE rightOrientation)
    {
        // assert that there is only one of (top, bottom) and (left, right) and (front, back) in the orientations
        // TODO: not all of these checks are necessary. Which ones can be removed?

        if (topOrientation == Tile.FACETYPE.TOP || topOrientation == Tile.FACETYPE.BOTTOM)
        {
            if (frontOrientation == Tile.FACETYPE.TOP || frontOrientation == Tile.FACETYPE.BOTTOM || rightOrientation == Tile.FACETYPE.TOP || rightOrientation == Tile.FACETYPE.BOTTOM)
            {
                // throw an error and stop the program
                Debug.Log("InvalidOrientationError: topOrientation == Tile.FACETYPE.TOP || topOrientation == Tile.FACETYPE.BOTTOM");
                return false;

            }
        }

        if (topOrientation == Tile.FACETYPE.LEFT || topOrientation == Tile.FACETYPE.RIGHT)
        {
            if (frontOrientation == Tile.FACETYPE.LEFT || frontOrientation == Tile.FACETYPE.RIGHT || rightOrientation == Tile.FACETYPE.LEFT || rightOrientation == Tile.FACETYPE.RIGHT)
            {
                // throw an error and stop the program
                Debug.Log("InvalidOrientationError: topOrientation == Tile.FACETYPE.LEFT || topOrientation == Tile.FACETYPE.RIGHT");
                return false;

            }
        }

        if (topOrientation == Tile.FACETYPE.FRONT || topOrientation == Tile.FACETYPE.BACK)
        {
            if (frontOrientation == Tile.FACETYPE.FRONT || frontOrientation == Tile.FACETYPE.BACK || rightOrientation == Tile.FACETYPE.FRONT || rightOrientation == Tile.FACETYPE.BACK)
            {
                // throw an error and stop the program
                Debug.Log("InvalidOrientationError: topOrientation == Tile.FACETYPE.FRONT || topOrientation == Tile.FACETYPE.BACK");
                return false;
            }
        }

        if (frontOrientation == Tile.FACETYPE.TOP || frontOrientation == Tile.FACETYPE.BOTTOM)
        {
            if (topOrientation == Tile.FACETYPE.TOP || topOrientation == Tile.FACETYPE.BOTTOM || rightOrientation == Tile.FACETYPE.TOP || rightOrientation == Tile.FACETYPE.BOTTOM)
            {
                // throw an error and stop the program
                Debug.Log("InvalidOrientationError: frontOrientation == Tile.FACETYPE.TOP || frontOrientation == Tile.FACETYPE.BOTTOM");
                return false;
            }
        }

        if (frontOrientation == Tile.FACETYPE.LEFT || frontOrientation == Tile.FACETYPE.RIGHT)
        {
            if (topOrientation == Tile.FACETYPE.LEFT || topOrientation == Tile.FACETYPE.RIGHT || rightOrientation == Tile.FACETYPE.LEFT || rightOrientation == Tile.FACETYPE.RIGHT)
            {
                // throw an error and stop the program
                Debug.Log("InvalidOrientationError: frontOrientation == Tile.FACETYPE.LEFT || frontOrientation == Tile.FACETYPE.RIGHT");
                return false;
            }
        }

        if (frontOrientation == Tile.FACETYPE.FRONT || frontOrientation == Tile.FACETYPE.BACK)
        {
            if (topOrientation == Tile.FACETYPE.FRONT || topOrientation == Tile.FACETYPE.BACK || rightOrientation == Tile.FACETYPE.FRONT || rightOrientation == Tile.FACETYPE.BACK)
            {
                // throw an error and stop the program
                Debug.Log("InvalidOrientationError: frontOrientation == Tile.FACETYPE.FRONT || frontOrientation == Tile.FACETYPE.BACK");
                return false;
            }
        }

        if (rightOrientation == Tile.FACETYPE.TOP || rightOrientation == Tile.FACETYPE.BOTTOM)
        {
            if (topOrientation == Tile.FACETYPE.TOP || topOrientation == Tile.FACETYPE.BOTTOM || frontOrientation == Tile.FACETYPE.TOP || frontOrientation == Tile.FACETYPE.BOTTOM)
            {
                // throw an error and stop the program
                Debug.Log("InvalidOrientationError: rightOrientation == Tile.FACETYPE.TOP || rightOrientation == Tile.FACETYPE.BOTTOM");
                return false;
            }
        }

        if (rightOrientation == Tile.FACETYPE.LEFT || rightOrientation == Tile.FACETYPE.RIGHT)
        {
            if (topOrientation == Tile.FACETYPE.LEFT || topOrientation == Tile.FACETYPE.RIGHT || frontOrientation == Tile.FACETYPE.LEFT || frontOrientation == Tile.FACETYPE.RIGHT)
            {
                // throw an error and stop the program
                Debug.Log("InvalidOrientationError: rightOrientation == Tile.FACETYPE.LEFT || rightOrientation == Tile.FACETYPE.RIGHT");
                return false;
            }
        }

        if (rightOrientation == Tile.FACETYPE.FRONT || rightOrientation == Tile.FACETYPE.BACK)
        {
            if (topOrientation == Tile.FACETYPE.FRONT || topOrientation == Tile.FACETYPE.BACK || frontOrientation == Tile.FACETYPE.FRONT || frontOrientation == Tile.FACETYPE.BACK)
            {
                // throw an error and stop the program
                Debug.Log("InvalidOrientationError: rightOrientation == Tile.FACETYPE.FRONT || rightOrientation == Tile.FACETYPE.BACK");
                return false;
            }
        }

        return true;
    }

    public List<int> RotateFaces(List<int> faceList, Tile.FACETYPE topOrientation, Tile.FACETYPE frontOrientation, Tile.FACETYPE rightOrientation)
    {
        // this function permutes the order of facelist to match the rotation of the tile

        int topFace = -1;
        int bottomFace = -1;
        int leftFace = -1;
        int rightFace = -1;
        int frontFace = -1;
        int backFace = -1;

        List<int> rotatedFaceList = new List<int>();



        if (topOrientation == Tile.FACETYPE.TOP)
        {
            topFace = faceList[0];
            bottomFace = faceList[1];
        }

        if (topOrientation == Tile.FACETYPE.BOTTOM)
        {
            topFace = faceList[1];
            bottomFace = faceList[0];
        }

        if (topOrientation == Tile.FACETYPE.LEFT)
        {
            topFace = faceList[2];
            bottomFace = faceList[3];
        }

        if (topOrientation == Tile.FACETYPE.RIGHT)
        {
            topFace = faceList[3];
            bottomFace = faceList[2];
        }

        if (topOrientation == Tile.FACETYPE.FRONT)
        {
            topFace = faceList[4];
            bottomFace = faceList[5];
        }

        if (topOrientation == Tile.FACETYPE.BACK)
        {
            topFace = faceList[5];
            bottomFace = faceList[4];
        }

        if (frontOrientation == Tile.FACETYPE.TOP)
        {
            frontFace = faceList[0];
            backFace = faceList[1];
        }

        if (frontOrientation == Tile.FACETYPE.BOTTOM)
        {
            frontFace = faceList[1];
            backFace = faceList[0];
        }

        if (frontOrientation == Tile.FACETYPE.LEFT)
        {
            frontFace = faceList[2];
            backFace = faceList[3];
        }

        if (frontOrientation == Tile.FACETYPE.RIGHT)
        {
            frontFace = faceList[3];
            backFace = faceList[2];
        }

        if (frontOrientation == Tile.FACETYPE.FRONT)
        {
            frontFace = faceList[4];
            backFace = faceList[5];
        }

        if (frontOrientation == Tile.FACETYPE.BACK)
        {
            frontFace = faceList[5];
            backFace = faceList[4];
        }

        if (rightOrientation == Tile.FACETYPE.TOP)
        {
            leftFace = faceList[0];
            rightFace = faceList[1];
        }

        if (rightOrientation == Tile.FACETYPE.BOTTOM)
        {
            leftFace = faceList[1];
            rightFace = faceList[0];
        }

        if (rightOrientation == Tile.FACETYPE.LEFT)
        {
            leftFace = faceList[2];
            rightFace = faceList[3];
        }

        if (rightOrientation == Tile.FACETYPE.RIGHT)
        {
            leftFace = faceList[3];
            rightFace = faceList[2];
        }

        if (rightOrientation == Tile.FACETYPE.FRONT)
        {
            leftFace = faceList[4];
            rightFace = faceList[5];
        }

        if (rightOrientation == Tile.FACETYPE.BACK)
        {
            leftFace = faceList[5];
            rightFace = faceList[4];
        }

        rotatedFaceList = new List<int> { topFace, bottomFace, leftFace, rightFace, frontFace, backFace };
        if (rotatedFaceList.Contains(-1))
        {
            throw new Exception("InvalidOrientationError: newfaceList.Contains(-1)");
        }
        return rotatedFaceList;

    }*/

    public IEnumerator GenerateEnvironment(MetaTilePool metatilepool)
    {
        metatileList = metatilepool.GetMetaTiles();

        mValidMetaTiles = new Dictionary<MetaTile, List<bool>>[10, 10, 10];
        placedFlipped.Clear();
        placedMetaTiles.Clear();
        placedPositions.Clear();
        placedRotations.Clear();

        GameObject placementIndicator = null;
        if (DEBUG)
        {
            placementIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        }

        // set up the attempt loop around the metatile pool
        MetaTilePool.RESULTTYPE resultType = MetaTilePool.RESULTTYPE.SUCCESS;
        // int timeoutCounter = 0;
        MetaTile candidateMetatile;

        Vector3Int placementPosition = new Vector3Int(0, 0, 0);


        while (resultType != MetaTilePool.RESULTTYPE.COMPLETE)
        {
            // if the wavefront is empty, select a random empty position
            placementPosition = SelectPlacementPosition();

            // If there are no empty positions, return
            // TODO: update the check on if there are any empty positions
            if (placementPosition.x < 0)
            {
                Debug.Log("No empty positions, placement is complete.");
                resultType = MetaTilePool.RESULTTYPE.COMPLETE;

                for (int i = 0; i < placedMetaTiles.Count; i++)
                {
                    placedMetaTiles[i].DepositPayload(placedPositions[i], placedRotations[i], placedFlipped[i]);
                }

                break;
            }

            if (placedMetaTiles.Count == 0)
            {
                RecalculateMetaTileValidity(placementPosition);
            }

            List<MetaTileProbability> filteredMetatiles = GetMetaTiles(placementPosition, metatilepool);
            int filteredMetatileCount = filteredMetatiles.Count;

            if (placementIndicator)
            {
                placementIndicator.transform.position = placementPosition;
            }

            resultType = MetaTilePool.RESULTTYPE.FAILURE;

            Dictionary<MetaTile, List<bool>> placementPositionMetaTileValidityDict = mValidMetaTiles[placementPosition.x, placementPosition.y, placementPosition.z];


            //Loop through all permissable meta tiles and try to place them
            while (resultType != MetaTilePool.RESULTTYPE.SUCCESS && filteredMetatiles.Count > 0)
            {
                // Select a candidate metatile
                // candidateMetatile = DrawMetaTile(placementPosition, metatilepool);
                int candidateIndex = UnityEngine.Random.Range(0, filteredMetatiles.Count);
                candidateMetatile = filteredMetatiles[candidateIndex].metaTileProbability.GetMetaTile();

                List<bool> validConfigurations = placementPositionMetaTileValidityDict[candidateMetatile];


                int index = 0;
                // print($"Valid configurations for {candidateMetatile.name}: {validConfigurations.Count}");
                foreach (bool validConfigurationBool in validConfigurations)
                {
                    if (validConfigurationBool)
                    {
                        MetaTile.Configuration configTuple = candidateMetatile.GetConfiguration(index);

                        Vector3Int placementPositionOriginOffset = placementPosition - new Vector3Int((int)configTuple.origin.x, (int)configTuple.origin.y, (int)configTuple.origin.z);
                        PlaceMetaTile(placementPositionOriginOffset, candidateMetatile, configTuple.orientation, configTuple.flipped);
                        CollapseWaveFunction();

                        resultType = MetaTilePool.RESULTTYPE.SUCCESS;
                        placedMetaTiles.Add(candidateMetatile);
                        placedPositions.Add(placementPositionOriginOffset);
                        placedRotations.Add(OrientationToQuaternion[configTuple.orientation]);
                        placedFlipped.Add(configTuple.flipped);
                        Debug.Log($"SUCCESS, {candidateMetatile.name}, position={placementPositionOriginOffset}, orientation={configTuple.orientation}, flipped = {configTuple.flipped}, Count={placedMetaTiles.Count}");

                        if (DEBUG)
                        {
                            candidateMetatile.DepositPayload(placementPositionOriginOffset, OrientationToQuaternion[configTuple.orientation], configTuple.flipped, false);
                            Debug.Break();

                            yield return null;
                        }

                        break;

                    }
                    index++;
                }



                if (DEBUG)
                {
                    yield return null;
                }

                if (resultType != MetaTilePool.RESULTTYPE.SUCCESS)
                {
                    filteredMetatiles.RemoveAt(candidateIndex);
                }
            }

            if (filteredMetatiles.Count == 0)
            {
                Debug.Log($"No metatile to place at {placementPosition}. Tried {filteredMetatileCount} tiles.");
                resultType = MetaTilePool.RESULTTYPE.FAILURE;

                placementIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                placementIndicator.transform.position = placementPosition;

                for (int i = 0; i < placedMetaTiles.Count; i++)
                {
                    placedMetaTiles[i].DepositPayload(placedPositions[i], placedRotations[i], placedFlipped[i]);
                }
                break;
            }
        }
    }

    public void Awake()
    {
        UpdateDynamicWeights();
        //Initialize face array
        for (int i = 0; i < faces.GetLength(0); i++)
        {
            for (int j = 0; j < faces.GetLength(1); j++)
            {
                for (int k = 0; k < faces.GetLength(2); k++)
                {
                    for (int l = 0; l < faces.GetLength(3); l++)
                    {
                        faces[i, j, k, l] = -1;
                    }
                }
            }
        }
        //Intitalize tile arrays
        for (int i = 0; i < tileState.GetLength(0); i++)
        {
            for (int j = 0; j < tileState.GetLength(1); j++)
            {
                for (int k = 0; k < tileState.GetLength(2); k++)
                {
                    tileState[i, j, k] = TileState.NotPlaced;
                    environmentEntropies[i, j, k] = 0;
                    environment[i, j, k] = null;
                }
            }
        }

        //Making this a coroutine enbales step by step debugging. This is not fast, and should be changed back at some point.
        StartCoroutine(GenerateEnvironment(metatilepool));
        //GenerateEnvironment(metatilepool);
    }
}

static class RandomExtensions
{
    public static void Shuffle<T>(this System.Random rng, T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            int k = rng.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }
}