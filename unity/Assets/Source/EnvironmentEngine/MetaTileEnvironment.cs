using System.Threading;
using System.Net.Sockets;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class MetatileEnvironment : MonoBehaviour
{
    public static int mWidth = 10, mLength = 10, mHeight = 10;
    public static int mManyMetatileCount = 2;
    public float voxelSize = 1;

    public enum ConfigurationValidity
    {
        VALID,
        INVALID,
        UNKNOWN
    }

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

    public class EnvironmentTileState
    {
        public Dictionary<Metatile, List<ConfigurationValidity>> mValidMetatiles = new Dictionary<Metatile, List<ConfigurationValidity>>();
        public Dictionary<Metatile, List<int>> mPutativeMetatileIndices = new Dictionary<Metatile, List<int>>();
        public float mEntropy = -1;
        public Tile mTile = null;
        public TileState mState = TileState.NotPlaced;
        public List<int> mFaceIndices = new List<int>() { -1, -1, -1, -1, -1, -1 };
        public List<List<int>> mValidFaceTypes = new List<List<int>>();
        public Dictionary<Metatile, bool> mValidMetatileList = new Dictionary<Metatile, bool>();

        public EnvironmentTileState(List<Metatile> metatiles)
        {
            if (metatiles.Count > 0)
            {
                foreach (Metatile metatile in metatiles)
                {
                    mValidMetatiles[metatile] = new List<ConfigurationValidity>();
                    mPutativeMetatileIndices[metatile] = new List<int>();
                    for (int l = 0; l < metatile.GetConfigurations().Count; l++)
                    {
                        mValidMetatiles[metatile].Add(ConfigurationValidity.UNKNOWN);
                        mPutativeMetatileIndices[metatile].Add(l);
                    }
                    mValidMetatileList[metatile] = true;
                }

                for (Tile.FACETYPE i = Tile.FACETYPE.TOP; i <= Tile.FACETYPE.BACK; i++)
                {
                    mValidFaceTypes.Add(new List<int>());
                    mValidFaceTypes[(int)i] = metatiles[0].parent.palette.GetPossibleConnections(-1);
                }
            }
        }
    }

    public EnvironmentTileState[,,] environment = new EnvironmentTileState[mWidth, mWidth, mWidth];

    public class Wavefront
    {
        public List<Vector3Int> positions = new List<Vector3Int>();
        public List<float> entropies = new List<float>();
    }

    public Wavefront wavefront = new Wavefront();

    public class placedMetatile
    {
        public Metatile metatile;
        public Vector3Int position;
        public Quaternion rotation;
        public bool flipped;
        public Transform payload;

        public placedMetatile(Metatile metatile, Vector3Int position, Quaternion rotation, bool flipped)
        {
            this.metatile = metatile;
            this.position = position;
            this.rotation = rotation;
            this.flipped = flipped;
        }
    }

    public List<placedMetatile> placedMetatiles = new List<placedMetatile>();

    static protected Dictionary<string, List<Transform>> mInactiveObjectPool = new Dictionary<string, List<Transform>>();

    static protected Dictionary<Transform, string> mActiveObjectPool = new Dictionary<Transform, string>();

    static Transform mPoolRoot;

    public MetatilePool metatilepool;

    private List<Metatile> mMetatileList = new List<Metatile>();

    public List<Transform> placedPayloads = new List<Transform>();

    public enum TileState { NotPlaced, Wavefront, Placed };
    public TileState[,,] tileState = new TileState[mWidth, mHeight, mLength];

    public bool timelapse = false;
    public bool DEBUG = false;
    public Transform debugTile;
    public bool mGeneratingEnvironment = false;

    List<Tile.FACETYPE> mTempPermutationList = new List<Tile.FACETYPE>() { Tile.FACETYPE.TOP, Tile.FACETYPE.BOTTOM, Tile.FACETYPE.LEFT, Tile.FACETYPE.RIGHT, Tile.FACETYPE.FRONT, Tile.FACETYPE.BACK };
    List<int> mTempTileFaceList = new List<int>();
    List<int> mPutativeConfigurationList = new List<int>();

    public void RecalculateMetatileValidity(Vector3Int wavefrontPosition)
    {
        //Helper function to update the monster structure above. Should deal with pruning/initalizing the dictionary as needed.

        EnvironmentTileState tileState = environment[wavefrontPosition.x, wavefrontPosition.y, wavefrontPosition.z];

        int validCount = 0;
        foreach (Metatile metatile in tileState.mValidMetatiles.Keys)
        {
            if (!tileState.mValidMetatileList[metatile])
            {
                continue;
            }

            List<int> putativeConfigurations = tileState.mPutativeMetatileIndices[metatile];
            List<ConfigurationValidity> validConfigurations = tileState.mValidMetatiles[metatile];

            // print($"validConfigurations.Count: {validConfigurations.Count}");
            // if (validConfigurations == null)
            // {
            //     mValidMetatiles[wavefrontPosition.x, wavefrontPosition.y, wavefrontPosition.z][metatile] = new List<bool>();
            //     validConfigurations = mValidMetatiles[wavefrontPosition.x, wavefrontPosition.y, wavefrontPosition.z][metatile];
            // }


            bool hasValidConfiguration = false;
            // If we have a VALID index it's in the first index
            if (validConfigurations[putativeConfigurations[0]] == ConfigurationValidity.VALID)
            {
                Metatile.Configuration configTuple = metatile.GetConfiguration(putativeConfigurations[0]);
                bool canPlace = CanPlaceMetatile(
                    wavefrontPosition, metatile, configTuple.origin, configTuple.orientation, configTuple.flipped);
                if (!canPlace)
                {
                    tileState.mValidMetatiles[metatile][putativeConfigurations[0]] = ConfigurationValidity.INVALID;
                    tileState.mPutativeMetatileIndices[metatile].RemoveAt(0);
                }
                else
                {
                    hasValidConfiguration = true;
                }
            }

            if (!hasValidConfiguration)
            {
                for (int i = 0; i < putativeConfigurations.Count; i++)
                {
                    if (validConfigurations[putativeConfigurations[i]] == ConfigurationValidity.UNKNOWN)
                    {
                        Metatile.Configuration configTuple = metatile.GetConfiguration(putativeConfigurations[i]);
                        bool canPlace = CanPlaceMetatile(
                            wavefrontPosition, metatile, configTuple.origin, configTuple.orientation, configTuple.flipped);
                        if (!canPlace)
                        {
                            tileState.mValidMetatiles[metatile][putativeConfigurations[i]] = ConfigurationValidity.INVALID;
                            tileState.mPutativeMetatileIndices[metatile].RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            tileState.mValidMetatiles[metatile][putativeConfigurations[i]] = ConfigurationValidity.VALID;
                            hasValidConfiguration = true;
                            break;
                        }
                    }
                }
            }

            if (!hasValidConfiguration)
            {
                tileState.mValidMetatileList[metatile] = false;
            }
            else
            {
                validCount++;
            }

            if (validCount >= mManyMetatileCount)
            {
                break;
            }

            // sampledMetatileisAllowed[sampledMetatiles.IndexOf(metatile)] = isAllowed;
        }

    }

    public int CountValidMetatiles(Vector3Int position)
    {
        int count = 0;
        foreach (KeyValuePair<Metatile, bool> pair in environment[position.x, position.y, position.z].mValidMetatileList)
        {
            if (pair.Value)
            {
                count++;
            }
        }

        // print($"count : {count}");
        return count;
    }

    public Vector3Int SelectPlacementPosition()
    {
        // Selects a position to place a metatile
        // The position is selected from the wavefront
        // If the wavefront is empty, select a random empty position
        // There might already be pre-placed metatiles
        if (placedMetatiles.Count == 0)
        {
            List<Vector3Int> emptyPositions = GetEmptyPositions();

            if (emptyPositions.Count == 0)
            {
                Debug.Log("SelectPlacementPosition() no empty positions");
                return new Vector3Int(-1, -1, -1); // return invalid position
            }
            // Debug.Log("SelectPlacementPosition() emptyPositions.Count: " + emptyPositions.Count);
            return emptyPositions[UnityEngine.Random.Range(0, emptyPositions.Count)];
        }
        else
        {
            if (wavefront.positions.Count == 0)
            {
                Debug.Log("SelectPlacementPosition() no wavefront positions");
                return new Vector3Int(-1, -1, -1);
            }

            float minEntropyValue = wavefront.entropies.Min();

            int minEntropyIndex = wavefront.entropies.IndexOf(minEntropyValue);

            Vector3Int minEntropyPosition = wavefront.positions[minEntropyIndex];
            // select the position with the minimum entropy value
            if (DEBUG)
            {
                Debug.Log("SelectPlacementPosition() placementPosition: " + minEntropyPosition);
            }
            return minEntropyPosition;
        }

    }

    private List<Vector3Int> GetEmptyPositions()
    {
        List<Vector3Int> emptyPositions = new List<Vector3Int>();
        for (int x = 0; x < environment.GetLength(0); x++)
        {
            for (int y = 0; y < environment.GetLength(1); y++)
            {
                for (int z = 0; z < environment.GetLength(2); z++)
                {
                    if (environment[x, y, z].mTile == null)
                    {
                        emptyPositions.Add(new Vector3Int(x, y, z));
                    }
                }
            }
        }

        return emptyPositions;
    }

    public Tile GetTile(Vector3Int position)
    {
        if (position.x < 0 || position.x >= environment.GetLength(0) ||
            position.y < 0 || position.y >= environment.GetLength(1) ||
            position.z < 0 || position.z >= environment.GetLength(2))
        {
            return null;  // the tile is out of bounds
        }

        return environment[position.x, position.y, position.z].mTile;
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
        EnvironmentTileState tileState = environment[position.x, position.y, position.z];

        return tileState.mFaceIndices;
    }

    void SetFace(Vector3Int position, Tile.FACETYPE faceType, int newFace)
    {
        if (position.x >= 0 && position.x < environment.GetLength(0) &&
            position.y >= 0 && position.y < environment.GetLength(1) &&
            position.z >= 0 && position.z < environment.GetLength(2))
        {
            environment[position.x, position.y, position.z].mFaceIndices[(int)faceType] = newFace;

            if (newFace > -1)
            {
                environment[position.x, position.y, position.z].mValidFaceTypes[(int)faceType] = metatilepool.palette.GetPossibleConnections(newFace);
            }
        }
    }

    public void SetFaceList(Vector3Int position, List<int> faceList, bool debug = false)
    {
        //Set this position faces.
        EnvironmentTileState tileState = environment[position.x, position.y, position.z];
        SetFace(position, Tile.FACETYPE.TOP, faceList[(int)Tile.FACETYPE.TOP]);
        SetFace(position, Tile.FACETYPE.BOTTOM, faceList[(int)Tile.FACETYPE.BOTTOM]);
        SetFace(position, Tile.FACETYPE.LEFT, faceList[(int)Tile.FACETYPE.LEFT]);
        SetFace(position, Tile.FACETYPE.RIGHT, faceList[(int)Tile.FACETYPE.RIGHT]);
        SetFace(position, Tile.FACETYPE.FRONT, faceList[(int)Tile.FACETYPE.FRONT]);
        SetFace(position, Tile.FACETYPE.BACK, faceList[(int)Tile.FACETYPE.BACK]);


        //Set OTHER position faces.
        SetFace(new Vector3Int(position.x, position.y + 1, position.z), Tile.FACETYPE.BOTTOM, faceList[(int)Tile.FACETYPE.TOP]);
        SetFace(new Vector3Int(position.x, position.y - 1, position.z), Tile.FACETYPE.TOP, faceList[(int)Tile.FACETYPE.BOTTOM]);
        SetFace(new Vector3Int(position.x - 1, position.y, position.z), Tile.FACETYPE.RIGHT, faceList[(int)Tile.FACETYPE.LEFT]);
        SetFace(new Vector3Int(position.x + 1, position.y, position.z), Tile.FACETYPE.LEFT, faceList[(int)Tile.FACETYPE.RIGHT]);
        SetFace(new Vector3Int(position.x, position.y, position.z - 1), Tile.FACETYPE.BACK, faceList[(int)Tile.FACETYPE.FRONT]);
        SetFace(new Vector3Int(position.x, position.y, position.z + 1), Tile.FACETYPE.FRONT, faceList[(int)Tile.FACETYPE.BACK]);

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

    public List<Vector3Int> GetAdjacentPositions(Vector3Int position)
    {
        List<Vector3Int> adjacentPositions = new List<Vector3Int>();
        adjacentPositions.Add(new Vector3Int(position.x + 1, position.y, position.z));
        adjacentPositions.Add(new Vector3Int(position.x - 1, position.y, position.z));
        adjacentPositions.Add(new Vector3Int(position.x, position.y - 1, position.z));
        adjacentPositions.Add(new Vector3Int(position.x, position.y + 1, position.z));
        adjacentPositions.Add(new Vector3Int(position.x, position.y, position.z - 1));
        adjacentPositions.Add(new Vector3Int(position.x, position.y, position.z + 1));

        // remove any illegal positions
        adjacentPositions.RemoveAll(position => position.x < 0 || position.x >= environment.GetLength(0) ||
            position.y < 0 || position.y >= environment.GetLength(1) ||
            position.z < 0 || position.z >= environment.GetLength(2));

        return adjacentPositions;
    }

    public List<List<int>> GetPossibleFaces(Vector3Int position)
    {
        /*List<int> faceList = GetFaceList(position, DEBUG);

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
        }*/

        return environment[position.x, position.y, position.z].mValidFaceTypes;
    }

    public List<MetatileProbability> CustomFiltering(Vector3Int placementPosition, List<MetatileProbability> metatiles)
    {
        List<MetatileProbability> filteredMetatiles = new List<MetatileProbability>();
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
        foreach (MetatileProbability metatile in metatiles)
        {
            foreach (Tile tile in metatile.metatileContainer.GetMetatiles()[0].tiles)
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

    public List<MetatileProbability> GetMetatiles(Vector3Int placementPosition, MetatilePool metatilepool)
    {
        // Create a list of all legal metatiles from the pool
        List<MetatileProbability> metatiles = metatilepool.BuildMetatilePoolDeepCopy();

        if (DEBUG)
        {
            Debug.Log("Metatile Count after adding: " + metatiles.Count);
        }

        List<MetatileProbability> filteredMetatiles = CustomFiltering(placementPosition, metatiles);

        if (DEBUG)
        {
            Debug.Log("Metatile Count after filtering: " + filteredMetatiles.Count);
        }

        return filteredMetatiles;
    }

    public bool CanPlaceMetatile(Vector3Int placementPosition, Metatile metatile, Vector3 currentOrigin, Orientation orientation, bool flipped)
    {
        Quaternion quaternion = OrientationToQuaternion[orientation];

        if (flipped && !metatile.canFlip ||
            quaternion.x != 0 && metatile.rotationDirections.x == 0 ||
            quaternion.y != 0 && metatile.rotationDirections.y == 0 ||
            quaternion.z != 0 && metatile.rotationDirections.z == 0)
        {
            return false;
        }

        foreach (Tile tile in metatile.tiles)
        {
            // from enum to list of integers that corresponds to a permuted list of faces

            //mTempPermutationList.Clear();
            //mTempPermutationList.AddRange(OrientationToPermutation[orientation]);

            //This wasn't fast
            //Vector3Int tilePosition = metatile.GetCachedTileOffset(tile, currentOrigin, orientation, flipped);

            // multiply the tile position by a quaternion
            UnityEngine.Vector3 unRotatedPosition = new Vector3(tile.GetLocalPosition().x, tile.GetLocalPosition().y, tile.GetLocalPosition().z) - currentOrigin;
            if (flipped)
            {
                unRotatedPosition.y = unRotatedPosition.y * -1;

            }
            Vector3 rotatedPosition = quaternion * unRotatedPosition;

            Vector3 newRotatedPosition = rotatedPosition;
            Vector3Int tilePosition = newRotatedPosition.ToVector3Int();

            Vector3Int environmentPosition = placementPosition + tilePosition;

            if (environmentPosition.x < 0 || environmentPosition.x >= environment.GetLength(0) ||
                environmentPosition.y < 0 || environmentPosition.y >= environment.GetLength(1) ||
                environmentPosition.z < 0 || environmentPosition.z >= environment.GetLength(2))
            {
                return false;  // the metatile is out of bounds
            }

            EnvironmentTileState tileState = environment[environmentPosition.x, environmentPosition.y, environmentPosition.z];

            if (tileState.mTile != null)
            {
                // Debug.Log("environment[envX, envY, envZ] :" + environment[envX, envY, envZ]);
                return false;  // the metatile would overwrite a tile
            }

            //Only consider tiles in the wavefront
            if (tileState.mState == TileState.NotPlaced)
            {
                continue;
            }
            else if (tileState.mState == TileState.Placed)
            {
                return false;
            }

            // Only calculate purmutations if tiles have succeeded
            List<Tile.FACETYPE> permutation = OrientationToPermutation[orientation];

            //We directly overwrite to save speed
            for (int i = 0; i < permutation.Count; i++)
            {
                mTempPermutationList[i] = permutation[i];
            }

            if (flipped)
            {
                int topIndex = mTempPermutationList.IndexOf(Tile.FACETYPE.TOP);
                int bottomIndex = mTempPermutationList.IndexOf(Tile.FACETYPE.BOTTOM);
                (mTempPermutationList[bottomIndex], mTempPermutationList[topIndex]) = (mTempPermutationList[topIndex], mTempPermutationList[bottomIndex]);
            }

            // TODO: check neighbor tiles for adjacency conflicts
            List<List<int>> possibleFaces = GetPossibleFaces(environmentPosition);
            int poolFaceCount = metatilepool.palette.tileFaces.Count;

            //mTempTileFaceList.Clear();
            //mTempTileFaceList.AddRange(tile.faceIDs);
            for (int i = 0; i < tile.faceIDs.Length; i++)
            {
                //If the face list as all possibilities, then we don't have to check them
                if (possibleFaces[i].Count != poolFaceCount && !possibleFaces[i].Contains(tile.faceIDs[(int)mTempPermutationList[i]]))
                {
                    return false;
                }
            }

        }

        return true;  // no conflicts were found

    }

    public void PlaceMetatile(Vector3Int placementPosition, Metatile metatile, Vector3 currentOrigin, Orientation orientation, bool flipped)
    {
        //Debug.Log("placing metatile " + this.name);

        foreach (Tile tile in metatile.tiles)
        {
            List<Tile.FACETYPE> permutation = new List<Tile.FACETYPE>(OrientationToPermutation[orientation]);

            Vector3 unRotatedPosition = tile.GetLocalPosition() - currentOrigin;
            if (flipped)
            {
                unRotatedPosition.y = unRotatedPosition.y * -1;
                int topIndex = permutation.IndexOf(Tile.FACETYPE.TOP);
                int bottomIndex = permutation.IndexOf(Tile.FACETYPE.BOTTOM);
                (permutation[bottomIndex], permutation[topIndex]) = (permutation[topIndex], permutation[bottomIndex]);
            }
            Vector3 rotatedPosition = OrientationToQuaternion[orientation] * unRotatedPosition;
            Vector3Int tilePosition = rotatedPosition.ToVector3Int();// new Vector3Int(Mathf.RoundToInt(rotatedPosition.x), Mathf.RoundToInt(rotatedPosition.y), Mathf.RoundToInt(rotatedPosition.z));

            Vector3Int environmentPosition = placementPosition + tilePosition;

            int envX = environmentPosition.x;
            int envY = environmentPosition.y;
            int envZ = environmentPosition.z;

            environment[envX, envY, envZ].mTile = tile;

            List<int> tempIDs = new List<int>();
            for (int i = 0; i < permutation.Count && i < tile.faceIDs.Length; i++)
            {
                tempIDs.Add(tile.faceIDs[(int)permutation[i]]);
            }

            Vector3Int position = new Vector3Int(envX, envY, envZ);

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

            if (environment[envX, envY, envZ].mState == TileState.Placed)
            {
                // throw an error and stop the program
                Debug.Log("envX: " + envX + " envY: " + envY + " envZ: " + envZ);
                Debug.Log("environment[envX, envY, envZ] :" + environment[envX, envY, envZ]);
                Debug.Log("tileState[envX, envY, envZ] :" + environment[envX, envY, envZ].mState);
                throw new Exception("OverWriteError: tileState[envX, envY, envZ] == TileState.Placed");
            }
            else if (environment[envX, envY, envZ].mState == TileState.Wavefront)
            {
                // get the index of the position in the wavefront.positions list
                int wavefrontIndex = wavefront.positions.IndexOf(new Vector3Int(envX, envY, envZ));

                // remove the position from the wavefront.positions list
                wavefront.positions.RemoveAt(wavefrontIndex);

                // remove the entropy from the wavefront.entropies list
                wavefront.entropies.RemoveAt(wavefrontIndex);
            }

            // update the tile state
            environment[envX, envY, envZ].mState = TileState.Placed;

            // add the neighbors of the tile to the wavefront if their tile state is not placed
            List<Vector3Int> adjacentPositions = GetAdjacentPositions(new Vector3Int(envX, envY, envZ));

            // add the adjacent positions to the wavefront if they are not placed
            foreach (Vector3Int adjacentPosition in adjacentPositions)
            {
                if (environment[adjacentPosition.x, adjacentPosition.y, adjacentPosition.z].mState == TileState.NotPlaced)
                {
                    // update the tile state
                    environment[adjacentPosition.x, adjacentPosition.y, adjacentPosition.z].mState = TileState.Wavefront;

                    // add the position to the wavefront.positions list
                    wavefront.positions.Add(adjacentPosition);

                    // add the entropy to the wavefront.entropies list
                    wavefront.entropies.Add(-1);
                }
            }
        }

    }

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

        return CountValidMetatiles(wavefrontPosition) * 10000 - (faceWeight + 0.001f);

        // // Do full translation/rotation tests and cache adjacent tile legality
        // // Dilation of tile checks and translation?
        // // Add face priorities to the metatiles, when approxmating count, multiply

        // // TODO: every position has a dict of {metatile, list of valid configurations}
        // List<bool> sampledMetatileisAllowed = Enumerable.Repeat(false, sampledMetatiles.Count).ToList();
        // float sumWeight = 0;

        // foreach (Metatile metatile in sampledMetatiles)
        // {
        //     bool isAllowed = false;
        //     foreach (Orientation orientation in Enum.GetValues(typeof(Orientation)))
        //     {
        //         foreach (bool flipped in new List<bool> { false, true })
        //         {
        //             foreach (Tile tile in metatile.tiles) // iterate over every tile in the metatile as the origin
        //             {
        //                 if (CanPlaceMetatile(
        // wavefrontPosition, metatile, tile.transform.localPosition, orientation, flipped))
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
        //     // sampledMetatileisAllowed[sampledMetatiles.IndexOf(metatile)] = isAllowed;
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

    public void CollapseWaveFunction()
    {
        for (int position = 0; position < wavefront.positions.Count; position++)
        {
            RecalculateMetatileValidity(wavefront.positions[position]);
            wavefront.entropies[position] = CalculateEntropy(wavefront.positions[position]);
        }
    }

    public IEnumerator GenerateEnvironment(MetatilePool metatilepool)
    {
        float startTime = Time.realtimeSinceStartup;
        mGeneratingEnvironment = true;

        mMetatileList = metatilepool.GetMetatiles();
        mMetatileList.Sort((Metatile a, Metatile b) => a.GetConfigurations().Count.CompareTo(b.GetConfigurations().Count));

        foreach (Transform payload in placedPayloads)
        {
            returnObjectToPool(payload.gameObject);
        }

        InitializeEnvironment();

        GameObject placementIndicator = null;
        if (DEBUG)
        {
            placementIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        }

        Metatile candidateMetatile;
        Vector3Int placementPosition = new Vector3Int(0, 0, 0);

        MetatilePool.RESULTTYPE resultType = MetatilePool.RESULTTYPE.SUCCESS;
        while (resultType != MetatilePool.RESULTTYPE.COMPLETE)
        {
            placementPosition = SelectPlacementPosition();
            if (placementIndicator)
            {
                placementIndicator.transform.position = placementPosition;
            }

            if (placedMetatiles.Count == 0)
            {
                RecalculateMetatileValidity(placementPosition);
            }

            if (placementPosition.x < 0)
            {
                Debug.Log("No empty positions, placement is complete.");
                DepositPayloads();
                break;
            }

            EnvironmentTileState tileState = environment[placementPosition.x, placementPosition.y, placementPosition.z];
            List<MetatileProbability> filteredMetatiles = GetMetatiles(placementPosition, metatilepool);
            List<MetatileProbability> startList = new List<MetatileProbability>(filteredMetatiles);

            resultType = MetatilePool.RESULTTYPE.FAILURE;
            while (resultType != MetatilePool.RESULTTYPE.SUCCESS && filteredMetatiles.Count > 0)
            {
                candidateMetatile = MetatilePool.DrawMetatileWithoutReplacement(filteredMetatiles);

                List<ConfigurationValidity> validConfigurations = tileState.mValidMetatiles[candidateMetatile];
                mPutativeConfigurationList.Clear();

                for (int i = 0; i < validConfigurations.Count; i++)
                {
                    if (validConfigurations[i] == ConfigurationValidity.VALID || validConfigurations[i] == ConfigurationValidity.UNKNOWN)
                    {
                        mPutativeConfigurationList.Add(i);
                    }
                }

                int selectedConfigurationIndex = -1;
                while (mPutativeConfigurationList.Count > 0)
                {
                    // TODO: change to be weighted by face matching matrix

                    int putativeConfigurationIndex = UnityEngine.Random.Range(0, mPutativeConfigurationList.Count);
                    int candidateConfigurationIndex = mPutativeConfigurationList[putativeConfigurationIndex];
                    if (validConfigurations[candidateConfigurationIndex] == ConfigurationValidity.VALID)
                    {
                        selectedConfigurationIndex = candidateConfigurationIndex;
                        break;
                    }
                    else if (validConfigurations[candidateConfigurationIndex] == ConfigurationValidity.UNKNOWN)
                    {
                        Metatile.Configuration configTuple = candidateMetatile.GetConfiguration(candidateConfigurationIndex);
                        bool placeable = CanPlaceMetatile(placementPosition,
                            candidateMetatile,
                            configTuple.origin,
                            configTuple.orientation,
                            configTuple.flipped
                        );
                        if (placeable)
                        {
                            selectedConfigurationIndex = candidateConfigurationIndex;
                            tileState.mValidMetatiles[candidateMetatile][candidateConfigurationIndex] = ConfigurationValidity.VALID;
                            break;
                        }
                        else
                        {
                            tileState.mValidMetatiles[candidateMetatile][candidateConfigurationIndex] = ConfigurationValidity.INVALID;
                        }

                    }
                    mPutativeConfigurationList.RemoveAt(putativeConfigurationIndex);
                }

                if (selectedConfigurationIndex != -1)
                {
                    Metatile.Configuration configTuple = candidateMetatile.GetConfiguration(selectedConfigurationIndex);

                    PlaceMetatile(placementPosition, candidateMetatile, configTuple.origin, configTuple.orientation, configTuple.flipped);
                    CollapseWaveFunction();

                    Vector3 unrotatedPosition = configTuple.origin;
                    if (configTuple.flipped)
                    {
                        unrotatedPosition.y *= -1;
                    }
                    Vector3 rotatedOffset = OrientationToQuaternion[configTuple.orientation] * unrotatedPosition;

                    Vector3Int placementPositionOriginOffset = placementPosition - rotatedOffset.ToVector3Int();// new Vector3Int((int)rotatedOffset.x, (int)rotatedOffset.y, (int)rotatedOffset.z);

                    if (placementIndicator)
                    {
                        //placementIndicator.transform.position = placementPositionOriginOffset;
                    }
                    resultType = MetatilePool.RESULTTYPE.SUCCESS;
                    placedMetatile newPlacedMetatile = new placedMetatile(
                        candidateMetatile,
                        placementPositionOriginOffset,
                        OrientationToQuaternion[configTuple.orientation],
                        configTuple.flipped);
                    // mPlacedMetatiles.Add(newPlacedMetatile);
                    placedMetatiles.Add(newPlacedMetatile);
                    // placedPositions.Add(placementPositionOriginOffset);
                    // placedRotations.Add(OrientationToQuaternion[configTuple.orientation]);
                    // placedFlipped.Add(configTuple.flipped);

                    if (DEBUG || timelapse)
                    {
                        candidateMetatile.DepositPayload(placementPositionOriginOffset, OrientationToQuaternion[configTuple.orientation], configTuple.flipped);

                        if (DEBUG)
                        {
                            Debug.Log($"SUCCESS, {candidateMetatile.name}, position={placementPositionOriginOffset}, origin={rotatedOffset}, orientation={configTuple.orientation}, flipped = {configTuple.flipped}, Count={placedMetatiles.Count}");

                            Debug.Break();
                        }

                        yield return null;
                    }
                }

                if (DEBUG)
                {
                    yield return null;
                }
            }

            if (resultType != MetatilePool.RESULTTYPE.SUCCESS && filteredMetatiles.Count == 0)
            {
                Debug.Log($"No metatile to place at {placementPosition}. Tried {filteredMetatiles.Count} tiles:");
                foreach (MetatileProbability tile in startList)
                {
                    Debug.Log($"    {tile.metatileContainer.transform.name}");
                }
                resultType = MetatilePool.RESULTTYPE.FAILURE;

                placementIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                placementIndicator.transform.position = placementPosition;

                DepositPayloads();
                break;
            }
        }

        mGeneratingEnvironment = false;

        Debug.Log($"Took {Time.realtimeSinceStartup - startTime} seconds to generate.");
    }

    private void DepositPayloads()
    {
        for (int i = 0; i < placedMetatiles.Count; i++)
        {
            placedPayloads.Add(placedMetatiles[i].metatile.DepositPayload(
                placedMetatiles[i].position,
                placedMetatiles[i].rotation,
                placedMetatiles[i].flipped
            ));
        }
    }

    private void InitializeEnvironment()
    {
        placedMetatiles.Clear();
        placedPayloads.Clear();

        environment = new EnvironmentTileState[mWidth, mWidth, mWidth];
        //Intitalize tile arrays
        for (int i = 0; i < environment.GetLength(0); i++)
        {
            for (int j = 0; j < environment.GetLength(1); j++)
            {
                for (int k = 0; k < environment.GetLength(2); k++)
                {
                    environment[i, j, k] = new EnvironmentTileState(mMetatileList);
                }
            }
        }

        wavefront.entropies.Clear();
        wavefront.positions.Clear();

        metatilepool.ResetDynamicWeight();
    }

    public void Awake()
    {

        //Making this a coroutine enbales step by step debugging. This is not fast, and should be changed back at some point.
        StartCoroutine(GenerateEnvironment(metatilepool));
        //GenerateEnvironment(metatilepool);
    }

    public void Update()
    {
        if (!mGeneratingEnvironment)
        {
            StartCoroutine(GenerateEnvironment(metatilepool));
        }
    }

    static public Transform instantiateNewObject(Transform prefab)
    {
        string path = prefab.gameObject.GetInstanceID().ToString();
        Transform tempObject = null;

        if (mInactiveObjectPool.ContainsKey(path) && mInactiveObjectPool[path].Count > 0 && mInactiveObjectPool[path][0] != null)
        {
            tempObject = mInactiveObjectPool[path][0];
            mInactiveObjectPool[path].RemoveAt(0);

            tempObject.gameObject.SetActive(true);
        }
        else
        {
            tempObject = (Transform)Transform.Instantiate(prefab);
        }

        if (!mActiveObjectPool.ContainsKey(tempObject))
        {
            mActiveObjectPool[tempObject] = path;
        }

        return tempObject;
    }
    static public void returnObjectToPool(GameObject removeObject)
    {
        if (mPoolRoot == null)
        {
            GameObject tempObejct = GameObject.Find("PoolRoot");
            if (tempObejct)
            {
                mPoolRoot = tempObejct.transform;
            }

            if (mPoolRoot == null)
            {
                mPoolRoot = new GameObject("PoolRoot").transform;
            }
        }

        if (mActiveObjectPool.ContainsKey(removeObject.transform))
        {
            string path = mActiveObjectPool[removeObject.transform];

            if (!mInactiveObjectPool.ContainsKey(path))
            {
                mInactiveObjectPool[path] = new List<Transform>();
            }

            mInactiveObjectPool[path].Add(removeObject.transform);
            mActiveObjectPool.Remove(removeObject.transform);

            removeObject.SetActive(false);
            removeObject.transform.parent = mPoolRoot;


            removeObject.transform.position = new Vector3(-9999, -9999);
        }
        else
        {
            if (removeObject.transform.parent != mPoolRoot)
            {
                Destroy(removeObject);
            }
        }
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

public static class Vector3IntExtensions
{
    public static Vector3Int ToVector3Int(this Vector3 vector)
    {
        return new Vector3Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
    }
}
