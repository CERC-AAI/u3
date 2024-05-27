using System.Threading;
using System.Net.Sockets;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using NUnit.Framework;
using System.Reflection;
using System.IO;
using static MetaTile;
using QuickGraph;
using QuickGraph.Algorithms;
using UnityEngine.InputSystem;
using System.IO.Abstractions;

public class MetatileManager : EnvironmentComponent
{
    public int mWidth = 10, mLength = 10, mHeight = 5;
    public int mManyMetatileCount = 2;
    public int mMaxConfigurationCount = 20;

    public float voxelSize = 1;

    public float connectivityThreshold = 0.5f;

    public string saveFile = "";
    public string loadFile = "";

    public bool VISUALIZE_GRAPH = false;
    public Transform debugArrow;
    public Transform debugNode;

    //TOP, BOTTOM, LEFT, RIGHT, FRONT, BACK
    [HideInInspector]
    public List<int> edgeTiles = new List<int> { 10, 2, 1, 4, 3, 5 };
    //Dictionary<Tile.FACETYPE, int> boundaryTile  = new Dictionary<Tile.FACETYPE, int>();
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
        public Dictionary<MetaTile, List<ConfigurationValidity>> mValidMetatiles = new Dictionary<MetaTile, List<ConfigurationValidity>>();
        public Dictionary<MetaTile, List<int>> mPutativeMetatileIndices = new Dictionary<MetaTile, List<int>>();
        //public float mEntropy = -1;
        public Tile mTile = null;
        public TileState mState = TileState.NotPlaced;
        public string mTileType = "";
        public List<int> mFaceIndices = new List<int>() { -1, -1, -1, -1, -1, -1 };
        public List<List<int>> mValidFaceTypes = new List<List<int>>();
        public Dictionary<MetaTile, bool> mValidMetatileList = new Dictionary<MetaTile, bool>();
        public Quaternion mRotation;

        public int CheckBoundaryTile(Vector3Int position, MetatileManager metatileManager,  Tile.FACETYPE face)
        {

            //Debug.Log($"{position}, face = {face}");
            switch ( face )
            {
                case Tile.FACETYPE.TOP:
                    // Code for handling top boundary
                    if (position.y == metatileManager.mHeight - 1)
                        return metatileManager.edgeTiles[0];
                    break;
                case Tile.FACETYPE.BOTTOM:
                    // Code for handling bottom boundary
                    if (position.y == 0)
                        return metatileManager.edgeTiles[1];
                    break;
                case Tile.FACETYPE.LEFT:
                    if ( position.x == 0)
                        return metatileManager.edgeTiles[2];
                    break;
                case Tile.FACETYPE.RIGHT:
                    if (position.x == metatileManager.mLength - 1)
                        return metatileManager.edgeTiles[3];
                    break;
                case Tile.FACETYPE.FRONT:
                    if ( position.z == 0)
                        return metatileManager.edgeTiles[4];
                    break;
                case Tile.FACETYPE.BACK:
                    if (position.z == metatileManager.mWidth - 1)
                        return metatileManager.edgeTiles[5];
                    break;;
                default:
                    // Code to handle any other case not explicitly handled above
                    return -1;     
            }
            return -1;
        }

        public EnvironmentTileState()
        {
        }

        public bool Initialize(List<MetaTile> metatiles, Vector3Int position, MetatileManager metatileManager)
        {
            bool hadRestrictions = false;

            for (Tile.FACETYPE i = Tile.FACETYPE.TOP; i <= Tile.FACETYPE.BACK; i++)
            {
                mValidFaceTypes.Add(new List<int>());
                int tileType = CheckBoundaryTile(position, metatileManager, i);

                mValidFaceTypes[(int)i] = metatiles[0].parent.palette.GetPossibleConnections(tileType);

                if (tileType >= 0)
                {
                    hadRestrictions = true;
                }
            }

            if (metatiles.Count > 0)
            {
                foreach (MetaTile metatile in metatiles)
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
            }

            return hadRestrictions;
        }
    }

    public EnvironmentTileState[,,] environment;

    public class Wavefront
    {
        public List<Vector3Int> positions = new List<Vector3Int>();
        public List<float> entropies = new List<float>();
    }

    public Wavefront wavefront = new Wavefront();

    public class placedMetatile
    {
        public MetaTile metatile;
        public Vector3Int position;
        public Quaternion rotation;
        public Orientation orientation;
        public bool flipped;
        public Transform payload;

        public placedMetatile(MetaTile metatile, Vector3Int position, Quaternion rotation, Orientation orientation, bool flipped)
        {
            this.metatile = metatile;
            this.position = position;
            this.rotation = rotation;
            this.orientation = orientation;
            this.flipped = flipped;
        }

    }

    public List<placedMetatile> placedMetatiles = new List<placedMetatile>();

    static protected Dictionary<string, List<Transform>> mInactiveObjectPool = new Dictionary<string, List<Transform>>();

    static protected Dictionary<Transform, string> mActiveObjectPool = new Dictionary<Transform, string>();

    static Transform mPoolRoot;

    public MetatilePool metatilepool;

    private List<MetaTile> mMetatileList = new List<MetaTile>();

    public List<Transform> placedPayloads = new List<Transform>();

    public enum TileState { NotPlaced, Wavefront, Placed };
    public TileState[,,] tileState;

    public int[,] heightMap;

    float mGenerationTime;

    Dictionary<Vector2Int, HashSet<Vector2Int>> Edges = new Dictionary<Vector2Int, HashSet<Vector2Int>>();

    HashSet<Vector2Int> SeenNodes;
    Queue<Vector2Int> nodeQueue = new Queue<Vector2Int>();

    HashSet<Vector2Int> seenNodes2d;
    Queue<Vector2Int> nodeQueue2d = new Queue<Vector2Int>();

    List<Vector3> mPermissibleSpawns = new List<Vector3>();

    Vector3Int maxHeight;
    public bool timelapse = false;
    public bool DEBUG = false;
    public Transform debugTile;
    public bool mGeneratingEnvironment = false;

    List<Tile.FACETYPE> mTempPermutationList = new List<Tile.FACETYPE>() { Tile.FACETYPE.TOP, Tile.FACETYPE.BOTTOM, Tile.FACETYPE.LEFT, Tile.FACETYPE.RIGHT, Tile.FACETYPE.FRONT, Tile.FACETYPE.BACK };
    List<int> mTempTileFaceList = new List<int>();
    List<int> mPotentialConfigurationList = new List<int>();

    struct MetatileConfigurationWeights
    {
        public MetaTile metatile;
        public int configurationIndex;
        public float weight;
    }
    Vector3Int mPlacementPosition = new Vector3Int(0, 0, 0);


    List<float> mFinalWeights = new List<float>();
    List<bool> mUseIndicies = new List<bool>();

    public Floor floor;



    public override void InitParameters(JSONObject jsonParameters)
    {
        if (jsonParameters)
        {
            jsonParameters.GetField(out mWidth, "env_width", mWidth);
            jsonParameters.GetField(out mLength, "env_length", mLength);
            jsonParameters.GetField(out mHeight, "env_height", mHeight);
            jsonParameters.GetField(out saveFile, "world_save_file", saveFile);
            jsonParameters.GetField(out loadFile, "world_load_file", loadFile);
            jsonParameters.GetField(out connectivityThreshold, "min_connectivity", connectivityThreshold);
        }

        base.InitParameters(jsonParameters);
    }

    public override void OnRunStarted()
    {
        Debug.Log("MetaTileManager"); 
        
        metatilepool.palette.Initialize();

        InitializeMetatileList(metatilepool);
        InitializeEnvironment();

        if (loadFile != "")
        {
            float startTime = Time.realtimeSinceStartup;

            LoadPayloads(loadFile);

            Debug.Log($"Took {Time.realtimeSinceStartup - startTime} seconds to load.");

            OnMapLoaded();
        }
        else
        {
            while (GenerateEnvironment(metatilepool).MoveNext())
            {
            }
        }

        base.OnRunStarted();
    }

    public void Update()
    {
        if (DEBUG && !mGeneratingEnvironment)
        {
            StartCoroutine(GenerateEnvironment(metatilepool));
        }

        if (floor)
        {
            floor.SetSize(new Vector2(mWidth * voxelSize, mLength * voxelSize));
        }
    }

    public void CalculateHeightMap()
    {
        for (int x = 0; x < environment.GetLength(0); x++)
        {
            for (int z = 0; z < environment.GetLength(2); z++)
            {
                for (int y = environment.GetLength(1) - 1; y >= 0; y-- )
                {
                    if ( environment[x,y,z].mTileType.ToLower() != "empty"  &&
                         environment[x,y,z].mTileType.ToLower() != "" )
                    {
                        heightMap[x , z] = y+1;
                        if ( y > maxHeight.y )
                        {
                            maxHeight.x = x;
                            maxHeight.y = y;
                            maxHeight.z = z;
                        }
                        break;
                        
                    }
                }
                //Debug.Log($"Height at {x} {z} = {heightMap[x, z]}");
            }
        } 
    }

    List<HashSet<Vector2Int>> FindConnectedComponents(Dictionary<Vector2Int, HashSet<Vector2Int>> graph)
    {
        // Convert the graph to a QuickGraph graph
        var qGraph = ConvertToQuickGraph(Edges);

        //var components = new List<HashSet<Vector2Int>>();
        var algo =  new QuickGraph.Algorithms.ConnectedComponents.StronglyConnectedComponentsAlgorithm<Vector2Int, Edge<Vector2Int>> (qGraph);
        algo.Compute();

        var algoComponents = algo.Components;

        // First, print algo.Components to see the initial state
        /*Debug.Log("Initial Components:");
        foreach (var kv in algoComponents)
        {
            Debug.Log($"Key: {kv.Key.x}, {kv.Key.y}, Value: {kv.Value}");
        }*/
        // Convert the strongly connected components to the desired format
        // Group the vertices by their component index
        var components = algoComponents
            .GroupBy(kv => kv.Value)
            .Select(group => new HashSet<Vector2Int>(group.Select(kv => kv.Key)))
            .ToList();

        // Iterate over each component (HashSet of Vector2Int)
        /*for (int i = 0; i < components.Count; i++)
        {
            Debug.Log($"Component {i + 1}:"); // Label each component with a number for easier tracking

            // Iterate over each Vector2Int in the current HashSet
            foreach (Vector2Int vertex in components[i])
            {
                Debug.Log($"{vertex.x} {vertex.y}"); // Print x and y coordinates of the vertex
            }
        }*/

        return components;
    }

    // Method to convert the graph to a QuickGraph graph
   BidirectionalGraph<Vector2Int, Edge<Vector2Int>> ConvertToQuickGraph(Dictionary<Vector2Int, HashSet<Vector2Int>> graph)
    {
        var qGraph = new BidirectionalGraph<Vector2Int, Edge<Vector2Int>>();

        foreach (var vertex in graph.Keys)
        {
            qGraph.AddVertex(vertex);
        }

        // Add edges to the QuickGraph graph
        foreach (var kvp in graph)
        {
            var v1 = kvp.Key;
            qGraph.AddVertex(v1);
            foreach (var v2 in kvp.Value)
            {   
                
                qGraph.AddEdge(new Edge<Vector2Int>(v1, v2));
            }
        }

        return qGraph; // Explicit cast
    }

    public bool CheckNonEmpty( string mTileType )
    {
        if( mTileType.ToLower() == "solid" || 
            mTileType.ToLower() == "ramp"  ||
            mTileType.ToLower() == "corner" )
        {
            return true;
        }

        return false;
    }

// need to add it to nodeQueue and need to check if it has been visited before 
    public void AddEdge( Vector2Int key, Vector2Int value )
    {
        if ( key == value ) {
            return;
        }
        
        /*if (Edges.ContainsKey( key ))
        {
            Edges[ key ].Add( value );
        }
        else
        {
            HashSet<Vector2Int> values = new HashSet<Vector2Int>();
            values.Add( value );
            Edges[ key ] = values;
        }*/
        Edges[key].Add(value);

        if (VISUALIZE_GRAPH)
        {
            Transform EdgeArrow = Instantiate<Transform>(debugArrow);
            Vector2 position = ((Vector2)key + (Vector2)value) / 2;

            EdgeArrow.transform.position = new Vector3(position.x, heightMap[key.x, key.y], position.y);
            EdgeArrow.Rotate(Quaternion.FromToRotation(EdgeArrow.forward, new Vector3(value.x - key.x, 0, value.y - key.y)).eulerAngles);
        }


        /*if ( !SeenNodes.Contains( value ) )
        {
            nodeQueue.Enqueue( value );

        }*/

    }

    public void AddNodesIfRamp( Vector2Int key, Quaternion rotation )
    {
        float y_rotate = rotation.eulerAngles.y;
        // 4 cases, 0, 90, 180, 270
        // 0 , 1, 2, 3
        // positive x, neg z, neg x, positive z
        int[] x_cases_changes = new int[] { 1, 0, -1, 0 };
        int[] z_cases_changes = new int[] { 0, -1, 0, 1 };

        int keyHeight = heightMap[key.x, key.y];

        //Down ramp
        int node2_x = key.x + x_cases_changes[Mathf.RoundToInt(y_rotate / 90f)];
        int node2_z = key.y + z_cases_changes[Mathf.RoundToInt(y_rotate / 90f)];

        if (node2_x >= 0 && node2_x < heightMap.GetLength(0) &&
            node2_z >= 0 && node2_z < heightMap.GetLength(1))
        {
            Vector2Int node2 = new Vector2Int(node2_x, node2_z);
            int nodeHeight = heightMap[node2.x, node2.y];
            /*if ( !SeenNodes.Contains( node2 ) )
            {
                return;
            }*/
            if (keyHeight >= nodeHeight)
            {
                AddEdge(key, node2);
            }
            if (keyHeight - 1 <= nodeHeight) // -1 accounts for ramps that go "off cliffs"
            {
                AddEdge(node2, key);
            }
        }


        //Up ramp
        node2_x = key.x - x_cases_changes[Mathf.RoundToInt(y_rotate / 90f)];
        node2_z = key.y - z_cases_changes[Mathf.RoundToInt(y_rotate / 90f)];

        if (node2_x >= 0 && node2_x < heightMap.GetLength(0) &&
            node2_z >= 0 && node2_z < heightMap.GetLength(1))
        {
            Vector2Int node2 = new Vector2Int(node2_x, node2_z);
            int nodeHeight = heightMap[node2.x, node2.y];
            /*if ( !SeenNodes.Contains( node2 ) )
            {
                return;
            }*/
            if (keyHeight >= nodeHeight)
            {
                AddEdge(key, node2);
            }
            if (keyHeight <= nodeHeight)
            {
                AddEdge(node2, key);
            }
        }

    }

    public void AddEdgeIfRequired( Vector2Int node1, Vector2Int node2 )
    {   
        int x2 = node2.x;
        int x1 = node1.x;
        int z1 = node1.y;
        //int y = node2.y;
        int z2 = node2.y;
        //Debug.Log($"Coord: {x2} {z2}");
        if (node1 == node2 )
        {   
            return; 
        }

        if( heightMap[ x2, z2 ] <= heightMap[ x1, z1 ] ) // Can walk flat and even down
        {
            // Need to check if its not blocked
            // for simplicity
            AddEdge( node1, node2 );
            //AddEdge( node2, node2 );
        }
        // Need to consider if I need to check ramp here too. Most likely not

        // else
        // {
        //     AddEdge( node1, node2 );
        // } 
    }

    public Tile getTile( Vector3Int node )
    {
        return environment[ node.x, node.y, node.z ].mTile;
    }

    public string getTileName( Vector3Int node )
    {
        return environment[ node.x, node.y, node.z ].mTileType.ToLower();
    }

    void PrettyPrintEdges()
    {   
        if (Edges.Count == 0)
        {
            Debug.Log("No edges are available in the graph.");
        }
        else
        {
            foreach (KeyValuePair<Vector2Int, HashSet<Vector2Int>> entry in Edges)
            {
                string edgesList = "";
                foreach (Vector2Int edge in entry.Value)
                {
                    edgesList += edge.ToString() + ", ";
                }
                if (edgesList.Length > 0)
                {
                    edgesList = edgesList.Remove(edgesList.Length - 2); // Remove the last ", "
                }
            }
        }
    } 

    public void DiagonalCheckAndAddEdge( Vector3Int node1, Vector3Int node2 )
    {   
        Tile tileNode1 = getTile( node1 );
        Tile tileNode2 = getTile( node2 );
        float y_rotate1 = tileNode1.transform.rotation.y;
        float y_rotate2 = tileNode2.transform.rotation.z;

        // 4 cases, 0, 90, 180, 270
        int[] skip_cases_node1 = new int[] {0, 0, 0, 0};
        int[] skip_cases_node2 = new int[] {0, 0, 0, 0};

        // pos z, negative x // back left
        // pos z, pos x //
        // neg z, pos x
        // neg z, neg z

        if ( getTileName( node1 ) == "corner" )
        {
            
        }

        if( getTileName( node2 ) == "corner" )
        {

        }

        if (skip_cases_node1[0] == 0 && skip_cases_node2[0] == 0) // front_left
        {
            // node2 = new Vector3Int( Math.Max( x - 1, 0), height, Math.Max(z - 1, 0) );
            // AddEdgeIfRequired( currentNode, node2 );
        }

        if( skip_cases_node1[1] == 0 && skip_cases_node2[1] == 0 ) // front right
        {

        }

        if( skip_cases_node1[2] == 0 && skip_cases_node2[2] == 0 ) // back left
        {

        }

        if( skip_cases_node1[3] == 0 && skip_cases_node2[3] == 0 ) // back right
        {

        }

    }

    public void ExploreNeighbours(Vector2Int currentNode, int height) {

        int x = currentNode.x;
        int z = currentNode.y;

        //Debug.Log($"X: {x}, Y: {z}");
        // Need to consider corner case
        // without rotation corner skips positive z negative x
        // front
        // down-left
        // Need to consider unrotated
        if (environment[x, height, z].mTileType.ToLower() == "ramp")
        { 
            AddNodesIfRamp(currentNode, environment[x, height, z].mRotation);
            return;
        }

            // Edges[ currentNode ].Add( new Vector3Int( Math.Min( x + 1, 0), height - 1, z) );
        Vector2Int node2 = new Vector2Int( x, Math.Min( z + 1, environment.GetLength(2) - 1 ) );
        AddEdgeIfRequired( currentNode, node2 );

        // back
        node2 =  new Vector2Int( x, Math.Max( z - 1, 0) );
        AddEdgeIfRequired( currentNode, node2 );

        // left
        node2 = new Vector2Int( Math.Max(x - 1,0), z );
        AddEdgeIfRequired( currentNode, node2 );

        // right
        node2 = new Vector2Int( Math.Min( x + 1, environment.GetLength(0) - 1 ),  z );
        AddEdgeIfRequired( currentNode, node2 );

        //front left
        node2 = new Vector2Int( Math.Max( x - 1, 0), Math.Max(z - 1, 0) );
        /*AddEdgeIfRequired( currentNode, node2 );

        // front-right
        node2 =  new Vector2Int(  Math.Min( x + 1, environment.GetLength(0) - 1 ),  Math.Max(z - 1, 0 ) );
        AddEdgeIfRequired( currentNode, node2 );

        // back-left
        node2 = new Vector2Int( Math.Max( x - 1,0), Math.Min(z + 1, environment.GetLength(2) - 1));
        AddEdgeIfRequired( currentNode, node2 );
        
        // back right
        node2 = new Vector2Int( Math.Min( x + 1, environment.GetLength(0) - 1 ), Math.Min(z + 1, environment.GetLength(2) - 1));
        AddEdgeIfRequired( currentNode, node2 );*/

       
        
    }
    public void bfs( Vector2Int startNode, int height )
    {   

        if ( !SeenNodes.Contains( startNode ) )
        {
            SeenNodes.Add( startNode );
            nodeQueue.Enqueue(startNode); // Enqueue the starting node   
        }
        else {
            return;
        }
    
        while (nodeQueue.Count > 0)
        {
            Vector2Int currentNode = nodeQueue.Dequeue(); // Dequeue the ne`xt node

            // Add neighbors
            ExploreNeighbours( currentNode, Math.Max( 0, height - 1 ) );
        }
    }

    public void CreateGraph()
    {

        for (int x = 0; x < environment.GetLength(0); x++)
        {
            for (int z = 0; z < environment.GetLength(2); z++)
            {
                Vector2Int currentNode = new Vector2Int(x, z);
                Edges[currentNode] = new HashSet<Vector2Int>();
            }
        }
        // Initialize by marking every tile except ramp at x = 0, z = 0 as 0
        //SeenNodes = new HashSet<Vector2Int>();
        for ( int x = 0; x < environment.GetLength(0); x++ )
        {
            for( int z = 0; z < environment.GetLength(2); z++ )
            {
                int height = heightMap[x, z];
                Vector2Int currentNode = new Vector2Int(x, z);
                ExploreNeighbours(currentNode, Math.Max(0, height - 1));

                //Debug.Log($"Starting Search at {x}{z}");
                // Check 
                /*int height = heightMap[ x, z ];
                Vector2Int key = new Vector2Int( x, z );
                bfs( key, height );*/
            }
        }

    }
    // public void create2dGraph()
    // {
    //     SeenNodes2d = new HashSet<Vector2Int>();
    //     for( int x = 0; x < environment.GetLength(0); x++ )
    //     {
    //         for( int z = 0; z < environment.GetLength(2); z++ )
    //         {   
    //             // Check 
    //             int height = heightMap[ x, z ];
    //             Vector2Int key = new Vector2Int( x, z );
    //             bfs2d( key );
    //         }
    //     } 
    // }

    public IEnumerator GenerateEnvironment(MetatilePool metatilepool)
    {
        // Uncomment this for the same environment each time
        //UnityEngine.Random.InitState(1);


        mGeneratingEnvironment = true;

        float startTime = Time.realtimeSinceStartup;
        GameObject placementIndicator = InitializePlacementIndicator();

        MetatilePool.RESULTTYPE resultType = MetatilePool.RESULTTYPE.SUCCESS;
        while (resultType != MetatilePool.RESULTTYPE.COMPLETE)
        {
            mPlacementPosition = SelectPlacementPosition();
            placementIndicator = UpdatePlacementIndicator(placementIndicator, mPlacementPosition);

            if (IsInitialPlacement())
            {
                RecalculateMetatileValidity(mPlacementPosition);
            }

            if (IsCompletePlacement(mPlacementPosition))
            {
                Debug.Log("No empty positions, placement is complete.");
                DepositPayloads();
                break;
            }

            EnvironmentTileState tileState = environment[mPlacementPosition.x, mPlacementPosition.y, mPlacementPosition.z];
            List<MetatileProbability> filteredMetatiles = GetFilteredMetatiles(mPlacementPosition, metatilepool);


            List<MetatileConfigurationWeights> metatileConfigurationWeights = new List<MetatileConfigurationWeights>();

            foreach (KeyValuePair<MetaTile, List<ConfigurationValidity>> pair in tileState.mValidMetatiles)
            {
                MetaTile metatile = pair.Key;
                float metatileWeight = 0;
                foreach (MetatileProbability metatileprobability in filteredMetatiles)
                {
                    if (metatileprobability.metatileContainer == pair.Key)
                    {
                        metatileWeight = metatileprobability.GetDynamicWeight();
                        break;
                    }
                }
                if (metatileWeight == 0)
                {
                    continue;
                }

                //List<ConfigurationValidity> validConfigurations = pair.Value;

                List<float> configurationFaceWeights = CalculateMetatileConfigurationFaceWeights(mPlacementPosition, metatile);

                for (int configurationIndex = 0; configurationIndex < configurationFaceWeights.Count; configurationIndex++)
                {
                    int configuration = tileState.mPutativeMetatileIndices[metatile][configurationIndex];
                    float configurationWeight = metatileWeight * configurationFaceWeights[configurationIndex];
                    metatileConfigurationWeights.Add(new MetatileConfigurationWeights { metatile = metatile, configurationIndex = configuration, weight = configurationWeight / configurationFaceWeights.Count });
                }
            }
            resultType = MetatilePool.RESULTTYPE.FAILURE;
            //while (resultType != MetatilePool.RESULTTYPE.SUCCESS && filteredMetatiles.Count > 0)
            //{
            // Metatile candidateMetatile = MetatilePool.DrawMetatileWithoutReplacement(filteredMetatiles);

            // List<ConfigurationValidity> validConfigurations = tileState.mValidMetatiles[candidateMetatile];
            // setPotentialConfigurationList(validConfigurations);
            // int selectedConfigurationIndex = SelectConfiguration(validConfigurations, candidateMetatile, mPlacementPosition, tileState);
            // if (IsNoValidConfiguration(selectedConfigurationIndex))
            // {
            //     if (DEBUG)
            //     {
            //         yield return null;
            //     }
            //     continue;
            // }
            /*if (metatileConfigurationWeights.Count == 0)
            {
                if (DEBUG)
                {
                    yield return null;
                }
                break;
            }*/
            if (metatileConfigurationWeights.Count > 0)
            {
                MetatileConfigurationWeights chosenMetatileConfiguration = DrawMetaTileWithConfiguration(metatileConfigurationWeights);
                MetaTile candidateMetatile = chosenMetatileConfiguration.metatile;
                int selectedConfigurationIndex = chosenMetatileConfiguration.configurationIndex;
                MetaTile.Configuration configuration = candidateMetatile.GetConfiguration(selectedConfigurationIndex);

                Quaternion rotation = OrientationToQuaternion[configuration.orientation];
                Vector3Int rotatedOffset = RotateOffset(configuration);
                Vector3Int placementPositionOriginOffset = mPlacementPosition - rotatedOffset;

                PlaceMetatile(candidateMetatile, mPlacementPosition, configuration);
                CollapseWaveFunction();
                placedMetatiles.Add(new placedMetatile(candidateMetatile, placementPositionOriginOffset, rotation, configuration.orientation, configuration.flipped));

                resultType = MetatilePool.RESULTTYPE.SUCCESS;

                if (DEBUG || timelapse)
                {
                    candidateMetatile.DepositPayload(placementPositionOriginOffset, rotation, configuration.flipped, this, voxelSize);
                    if (DEBUG)
                    {
                        Debug.Log($"SUCCESS, {candidateMetatile.name}, position={placementPositionOriginOffset}, origin={rotatedOffset}, orientation={configuration.orientation}, flipped = {configuration.flipped}, Count={placedMetatiles.Count}");
                        Debug.Break();
                    }
                    yield return null;
                }

                if (DEBUG)
                {
                    yield return null;
                }
            }

            List<MetatileProbability> startList = new List<MetatileProbability>(filteredMetatiles);
            if (resultType != MetatilePool.RESULTTYPE.SUCCESS && metatileConfigurationWeights.Count == 0)
            {
                Debug.Log($"No metatile to place at {mPlacementPosition}. Tried {filteredMetatiles.Count} tiles:");
                foreach (MetatileProbability tile in startList)
                {
                    Debug.Log($"    {tile.metatileContainer.transform.name}");
                }
                resultType = MetatilePool.RESULTTYPE.FAILURE;

                placementIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                placementIndicator.transform.position = mPlacementPosition;

                DepositPayloads();
                break;
            }
        }

        //mGeneratingEnvironment = false;
        mGenerationTime = Time.realtimeSinceStartup - startTime;
        Debug.Log($"Took {Time.realtimeSinceStartup - startTime} seconds to generate.");

        OnGenerationComplete();
    }

    //Returns a shallow copy
    public List<Vector3> GetPermissibleSpawnLocations()
    {
        List<Vector3> SpawnLocations = new List<Vector3>(mPermissibleSpawns);

        return SpawnLocations;
    }

    void OnGenerationComplete()
    {
        OnMapLoaded();
    }

    void OnMapLoaded()
    {
        float startTime = Time.realtimeSinceStartup;

        //Calculate the heightmap for environment.
        CalculateHeightMap();

        Debug.Log($" Max Height is at coordinate {maxHeight.x}{maxHeight.z} = {maxHeight.y}");

        //Graph Builder
        CreateGraph();
        PrettyPrintEdges();
        // Find the connected components
        var components = FindConnectedComponents(Edges);

        // Find the maximum connected component
        var maxComponent = components.OrderByDescending(comp => comp.Count).First();
    
        Debug.Log($"Maximum Connected Component: {maxComponent.Count}");
        /*int count = 0;
        foreach (var component in components)
        {
            count += 1; 
            foreach (Vector2Int vertex in component)
            {

                Debug.Log($"CC {count}: {vertex.x} {vertex.y}");
            }
     
        }*/
        if (saveFile != "")
        {
            if ((float)maxComponent.Count / ((float)mWidth * (float)mLength) >= connectivityThreshold)
            {
                SavePayloads(saveFile);
                //EnvironmentManager.Instance.SendEventToPython("save_complete");
                EnvironmentManager.Instance.SendEventToPython($"save_complete:{mGenerationTime}");
            }
            else
            {
                Debug.Log("Maximum connectivity too low, did not save");
                EnvironmentManager.Instance.SendEventToPython("save_failed");
            }
        }
       

        mPermissibleSpawns.Clear();
        /*for (int x = 0; x < heightMap.GetLength(0); x++)
        {
            for (int z = 0; z < heightMap.GetLength(1); z++)
            {
                int height = heightMap[x, z];

                EnvironmentTileState thisTile = environment[x, Math.Max( height-1, 0 ), z];

                if (thisTile.mTile != null)
                {
                    mPermissibleSpawns.Add(new Vector3(x, Math.Max( height-1, 0 ), z) * voxelSize);// + new Vector3(0, voxelSize/2, 0));
                }
            }
        }*/

        foreach (var node in maxComponent)
        {
            int height = Math.Max(heightMap[node.x, node.y], 0) - 1;

            EnvironmentTileState thisTile = environment[node.x, Math.Max(height, 0), node.y];

            if (thisTile.mTileType != "ramp")
            {
                mPermissibleSpawns.Add(new Vector3(node.x, height, node.y) * voxelSize);

                if (VISUALIZE_GRAPH)
                {
                    Transform DebugNode = Instantiate<Transform>(debugNode);
                    debugNode.position = new Vector3(node.x, height, node.y) * voxelSize;

                    Renderer[] renderers = DebugNode.GetComponentsInChildren<Renderer>();
                    foreach (Renderer renderer in renderers)
                    {
                        renderer.material.color = Color.red;
                    }
                }
            }
        }

        if (mPermissibleSpawns.Count == 0)
        {
            Debug.Log("No spawn locations added");
        }

        Debug.Log($"Took {Time.realtimeSinceStartup - startTime} seconds to calculate permissable area.");
    }

    private MetatileConfigurationWeights DrawMetaTileWithConfiguration(List<MetatileConfigurationWeights> metatileConfigurationWeights)
    {
        float totalWeight = metatileConfigurationWeights.Sum(x => x.weight);
        float randomWeight = GetEngine().GetRandomRange(0, totalWeight);
        float currentWeight = 0;
        int metatileConfigurationIndex = 0;

        for (int i = 0; i < metatileConfigurationWeights.Count; i++)
        {
            currentWeight += metatileConfigurationWeights[i].weight;
            if (currentWeight >= randomWeight)
            {
                metatileConfigurationIndex = i;
                break;
            }
        }

        MetatileConfigurationWeights chosenMetatileConfiguration = metatileConfigurationWeights[metatileConfigurationIndex];
        return chosenMetatileConfiguration;
    }

    private void InitializeEnvironment()
    {
        foreach (Transform payload in placedPayloads)
        {
            returnObjectToPool(payload.gameObject);
        }

        placedMetatiles.Clear();
        placedPayloads.Clear();

        wavefront.entropies.Clear();
        wavefront.positions.Clear();

        environment = new EnvironmentTileState[mWidth, mHeight, mLength];
        //Intitalize tile arrays
        for (int i = 0; i < environment.GetLength(0); i++)
        {
            for (int j = 0; j < environment.GetLength(1); j++)
            {
                for (int k = 0; k < environment.GetLength(2); k++)
                {
                    environment[i, j, k] = new EnvironmentTileState();

                    if (environment[i, j, k].Initialize(mMetatileList, new Vector3Int(i, j, k), this))
                    {
                        AddToWavefront(new Vector3Int(i, j, k));
                    }
                }
            }
        }

        metatilepool.ResetDynamicWeight();

        CollapseWaveFunction();
    }

    private void InitializeMetatileList(MetatilePool metatilepool)
    {
        environment = new EnvironmentTileState[mWidth, mHeight, mLength];
        tileState = new TileState[mWidth, mHeight, mLength];
        heightMap = new int[mWidth, mLength];

        Edges = new Dictionary<Vector2Int, HashSet<Vector2Int>>();

        SeenNodes = new HashSet<Vector2Int>();
        nodeQueue = new Queue<Vector2Int>();

        mMetatileList = metatilepool.GetMetatiles();
        mMetatileList.Sort((MetaTile a, MetaTile b) => a.GetConfigurations().Count.CompareTo(b.GetConfigurations().Count));
    }

    private GameObject InitializePlacementIndicator()
    {
        GameObject placementIndicator = null;
        if (DEBUG)
        {
            placementIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        }

        return placementIndicator;
    }

    public Vector3Int SelectPlacementPosition()
    {
        // Selects a position to place a metatile
        // The position is selected from the wavefront
        // If the wavefront is empty, select a random empty position
        // There might already be pre-placed metatiles
        if (wavefront.positions.Count == 0)
        {
            List<Vector3Int> emptyPositions = GetEmptyPositions();

            if (emptyPositions.Count == 0)
            {
                Debug.Log("SelectPlacementPosition() no empty positions");
                return new Vector3Int(-1, -1, -1); // return invalid position
            }
            // Debug.Log("SelectPlacementPosition() emptyPositions.Count: " + emptyPositions.Count);
            return emptyPositions[GetEngine().GetRandomRange(0, emptyPositions.Count)];
        }
        else
        {
            if (wavefront.positions.Count == 0)
            {
                Debug.Log("SelectPlacementPosition() no wavefront positions");
                return new Vector3Int(-1, -1, -1);
            }

            float minEntropyValue = wavefront.entropies.Min();
            int startPoint = GetEngine().GetRandomRange(0, wavefront.entropies.Count);
            int minEntropyIndex = wavefront.entropies.IndexOf(minEntropyValue, startPoint);
            if (minEntropyIndex < 0)
            {
                minEntropyIndex = wavefront.entropies.IndexOf(minEntropyValue, 0);
            }
            Vector3Int minEntropyPosition = wavefront.positions[minEntropyIndex];

            // select the position with the minimum entropy value
            if (DEBUG)
            {
                Debug.Log("SelectPlacementPosition() placementPosition: " + minEntropyPosition);
            }
            return minEntropyPosition;
        }

    }

    private static GameObject UpdatePlacementIndicator(GameObject placementIndicator, Vector3Int placementPosition)
    {
        if (placementIndicator)
        {
            placementIndicator.transform.position = placementPosition;
        }

        return placementIndicator;
    }

    public void RecalculateMetatileValidity(Vector3Int wavefrontPosition)
    {
        // Recalculates the validity of all metatiles at a position
        EnvironmentTileState tileState = environment[wavefrontPosition.x, wavefrontPosition.y, wavefrontPosition.z];

        int validCount = 0;
        foreach (MetaTile metatile in tileState.mValidMetatiles.Keys)
        {
            if (!tileState.mValidMetatileList[metatile])
            {
                continue;
            }

            List<int> potentialConfigurations = tileState.mPutativeMetatileIndices[metatile];
            List<ConfigurationValidity> validConfigurations = tileState.mValidMetatiles[metatile];

            // print($"validConfigurations.Count: {validConfigurations.Count}");
            // if (validConfigurations == null)
            // {
            //     mValidMetatiles[wavefrontPosition.x, wavefrontPosition.y, wavefrontPosition.z][metatile] = new List<bool>();
            //     validConfigurations = mValidMetatiles[wavefrontPosition.x, wavefrontPosition.y, wavefrontPosition.z][metatile];
            // }


            bool hasValidConfiguration = false;
            // If we have a VALID index it's in the first index
            if (validConfigurations[potentialConfigurations[0]] == ConfigurationValidity.VALID)
            {
                MetaTile.Configuration configTuple = metatile.GetConfiguration(potentialConfigurations[0]);
                bool canPlace = CanPlaceMetatile(
                    wavefrontPosition, metatile, configTuple);
                if (!canPlace)
                {
                    tileState.mValidMetatiles[metatile][potentialConfigurations[0]] = ConfigurationValidity.INVALID;
                    tileState.mPutativeMetatileIndices[metatile].RemoveAt(0);
                }
                else
                {
                    hasValidConfiguration = true;
                }
            }

            if (!hasValidConfiguration)
            {
                for (int i = 0; i < potentialConfigurations.Count; i++)
                {
                    if (validConfigurations[potentialConfigurations[i]] == ConfigurationValidity.UNKNOWN)
                    {
                        MetaTile.Configuration configTuple = metatile.GetConfiguration(potentialConfigurations[i]);
                        bool canPlace = CanPlaceMetatile(
                            wavefrontPosition, metatile, configTuple);
                        if (!canPlace)
                        {
                            tileState.mValidMetatiles[metatile][potentialConfigurations[i]] = ConfigurationValidity.INVALID;
                            tileState.mPutativeMetatileIndices[metatile].RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            tileState.mValidMetatiles[metatile][potentialConfigurations[i]] = ConfigurationValidity.VALID;
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

    public List<float> CalculateMetatileConfigurationFaceWeights(Vector3Int wavefrontPosition, MetaTile metatile)
    {
        // Recalculates the validity of all metatiles at a position
        EnvironmentTileState tileState = environment[wavefrontPosition.x, wavefrontPosition.y, wavefrontPosition.z];
        mFinalWeights.Clear();

        int validCount = 0;
        if (!tileState.mValidMetatileList[metatile])
        {
            return mFinalWeights;
        }

        List<int> potentialConfigurations = tileState.mPutativeMetatileIndices[metatile];
        List<ConfigurationValidity> validConfigurations = tileState.mValidMetatiles[metatile];

        // print($"validConfigurations.Count: {validConfigurations.Count}");
        // if (validConfigurations == null)
        // {
        //     mValidMetatiles[wavefrontPosition.x, wavefrontPosition.y, wavefrontPosition.z][metatile] = new List<bool>();
        //     validConfigurations = mValidMetatiles[wavefrontPosition.x, wavefrontPosition.y, wavefrontPosition.z][metatile];
        // }


        bool hasValidConfiguration = false;
        // If we have a VALID index it's in the first index

        mUseIndicies.Clear();
        if (potentialConfigurations.Count < mMaxConfigurationCount)
        {
            int i = 0;
            for (i = 0; i < mMaxConfigurationCount; i++)
            {
                mUseIndicies.Add(true);
            }
            for (; i < potentialConfigurations.Count; i++)
            {
                mUseIndicies.Add(false);
            }
        }

        for (int i = 0; i < potentialConfigurations.Count; i++)
        {
            bool shouldUseConfiguration = true;
            if (mUseIndicies.Count > 0)
            {
                shouldUseConfiguration = false;

                int randomNumber = GetEngine().GetRandomRange(0, mUseIndicies.Count);
                if (mUseIndicies[randomNumber])
                {
                    mUseIndicies.RemoveAt(randomNumber);
                    shouldUseConfiguration = true;
                }
            }
            if ((i == 0 || shouldUseConfiguration) && (validConfigurations[potentialConfigurations[i]] == ConfigurationValidity.VALID || validConfigurations[potentialConfigurations[i]] == ConfigurationValidity.UNKNOWN))
            {
                List<float> faceWeights = new List<float>();
                MetaTile.Configuration configTuple = metatile.GetConfiguration(potentialConfigurations[i]);
                bool canPlace = CanPlaceMetatile(
                    wavefrontPosition, metatile, configTuple, faceWeights);
                if (!canPlace)
                {
                    mFinalWeights.Add(0);
                }
                else
                {
                    mFinalWeights.Add(faceWeights.Aggregate(1f, (a, b) => a * b));
                }
            }
            else
            {
                mFinalWeights.Add(0);
            }
        }

        return mFinalWeights;

    }


    public void PlaceMetatile(MetaTile metatile, Vector3Int placementPosition, MetaTile.Configuration configTuple)
    {
        //Debug.Log("placing metatile " + this.name);

        Vector3 currentOrigin = configTuple.origin;
        Orientation orientation = configTuple.orientation;
        bool flipped = configTuple.flipped;

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
            environment[envX, envY, envZ].mRotation = OrientationToQuaternion[orientation];


            if (mGeneratingEnvironment)
            {
                List<int> tempIDs = new List<int>();
                for (int i = 0; i < permutation.Count && i < tile.faceIDs.Length; i++)
                {
                    tempIDs.Add(tile.faceIDs[(int)permutation[i]]);
                }

                Vector3Int position = new Vector3Int(envX, envY, envZ);

                //Debug.Log($"Tile face {tile.faceIDs[0]}");
                SetFaceList(new Vector3Int(envX, envY, envZ), tempIDs, DEBUG);

                if (DEBUG) //Display Debug Tiles 
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
            }

            // update the tile state
            environment[envX, envY, envZ].mState = TileState.Placed;
            environment[envX, envY, envZ].mTileType = tile.tileType;

            if (mGeneratingEnvironment)
            {
                // add the neighbors of the tile to the wavefront if their tile state is not placed
                List<Vector3Int> adjacentPositions = GetAdjacentPositions(new Vector3Int(envX, envY, envZ));

                // add the adjacent positions to the wavefront if they are not placed
                foreach (Vector3Int adjacentPosition in adjacentPositions)
                {
                    AddToWavefront(adjacentPosition);
                }
            }
        }


    }

    void AddToWavefront(Vector3Int position)
    {
        if (environment[position.x, position.y, position.z].mState == TileState.NotPlaced)
        {
            // update the tile state
            environment[position.x, position.y, position.z].mState = TileState.Wavefront;

            // add the position to the wavefront.positions list
            wavefront.positions.Add(position);

            // add the entropy to the wavefront.entropies list
            wavefront.entropies.Add(-1);
        }
    }

    public void CollapseWaveFunction()
    {
        for (int wavefrontPosition = 0; wavefrontPosition < wavefront.positions.Count; wavefrontPosition++)
        {
            RecalculateMetatileValidity(wavefront.positions[wavefrontPosition]);
            wavefront.entropies[wavefrontPosition] = CalculateEntropy(wavefront.positions[wavefrontPosition]);
        }
    }

    private static Vector3Int RotateOffset(MetaTile.Configuration configTuple)
    {
        Vector3 unrotatedOrigin = configTuple.origin;
        if (configTuple.flipped)
        {
            unrotatedOrigin.y *= -1;
        }
        Vector3 rotatedOffset = OrientationToQuaternion[configTuple.orientation] * unrotatedOrigin;
        return rotatedOffset.ToVector3Int();
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

    // public bool CanPlaceMetatile(Vector3Int placementPosition, Metatile metatile, Vector3 currentOrigin, Orientation orientation, bool flipped)
    // {
    //     Quaternion quaternion = OrientationToQuaternion[orientation];

    //     if (flipped && !metatile.canFlip ||
    //         quaternion.x != 0 && metatile.rotationDirections.x == 0 ||
    //         quaternion.y != 0 && metatile.rotationDirections.y == 0 ||
    //         quaternion.z != 0 && metatile.rotationDirections.z == 0)
    //     {
    //         return false;
    //     }

    //     foreach (Tile tile in metatile.tiles)
    //     {
    //         // from enum to list of integers that corresponds to a permuted list of faces

    //         //mTempPermutationList.Clear();
    //         //mTempPermutationList.AddRange(OrientationToPermutation[orientation]);

    //         //This wasn't fast
    //         //Vector3Int tilePosition = metatile.GetCachedTileOffset(tile, currentOrigin, orientation, flipped);

    //         // multiply the tile position by a quaternion
    //         UnityEngine.Vector3 unRotatedPosition = new Vector3(tile.GetLocalPosition().x, tile.GetLocalPosition().y, tile.GetLocalPosition().z) - currentOrigin;
    //         if (flipped)
    //         {
    //             unRotatedPosition.y = unRotatedPosition.y * -1;

    //         }
    //         Vector3 rotatedPosition = quaternion * unRotatedPosition;

    //         Vector3 newRotatedPosition = rotatedPosition;
    //         Vector3Int tilePosition = newRotatedPosition.ToVector3Int();

    //         Vector3Int environmentPosition = placementPosition + tilePosition;

    //         if (environmentPosition.x < 0 || environmentPosition.x >= environment.GetLength(0) ||
    //             environmentPosition.y < 0 || environmentPosition.y >= environment.GetLength(1) ||
    //             environmentPosition.z < 0 || environmentPosition.z >= environment.GetLength(2))
    //         {
    //             return false;  // the metatile is out of bounds
    //         }

    //         EnvironmentTileState tileState = environment[environmentPosition.x, environmentPosition.y, environmentPosition.z];

    //         if (tileState.mTile != null)
    //         {
    //             // Debug.Log("environment[envX, envY, envZ] :" + environment[envX, envY, envZ]);
    //             return false;  // the metatile would overwrite a tile
    //         }

    //         //Only consider tiles in the wavefront
    //         if (tileState.mState == TileState.NotPlaced)
    //         {
    //             continue;
    //         }
    //         else if (tileState.mState == TileState.Placed)
    //         {
    //             return false;
    //         }

    //         // Only calculate purmutations if tiles have succeeded
    //         List<Tile.FACETYPE> permutation = OrientationToPermutation[orientation];

    //         //We directly overwrite to save speed
    //         for (int i = 0; i < permutation.Count; i++)
    //         {
    //             mTempPermutationList[i] = permutation[i];
    //         }

    //         if (flipped)
    //         {
    //             int topIndex = mTempPermutationList.IndexOf(Tile.FACETYPE.TOP);
    //             int bottomIndex = mTempPermutationList.IndexOf(Tile.FACETYPE.BOTTOM);
    //             (mTempPermutationList[bottomIndex], mTempPermutationList[topIndex]) = (mTempPermutationList[topIndex], mTempPermutationList[bottomIndex]);
    //         }

    //         // TODO: check neighbor tiles for adjacency conflicts
    //         List<List<int>> possibleFaces = GetValidFaceTypesAt(environmentPosition);
    //         int poolFaceCount = metatilepool.palette.tileFaces.Count;

    //         //mTempTileFaceList.Clear();
    //         //mTempTileFaceList.AddRange(tile.faceIDs);
    //         for (int i = 0; i < tile.faceIDs.Length; i++)
    //         {
    //             //If the face list as all possibilities, then we don't have to check them
    //             if (possibleFaces[i].Count != poolFaceCount && !possibleFaces[i].Contains(tile.faceIDs[(int)mTempPermutationList[i]]))
    //             {
    //                 return false;
    //             }
    //         }

    //     }

    //     return true;  // no conflicts were found

    // }

    public bool CanPlaceMetatile(Vector3Int placementPosition, MetaTile metatile, MetaTile.Configuration configTuple, List<float> faceWeights = null)
    {

        Vector3 currentOrigin = configTuple.origin;
        Orientation orientation = configTuple.orientation;
        bool flipped = configTuple.flipped;

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
            List<List<int>> possibleFaces = GetValidFaceTypesAt(environmentPosition);
            int poolFaceCount = metatilepool.palette.tileFaces.Count;

            List<int> faceList = null;
            if (faceWeights != null && tile.GetLocalPosition() == Vector3.zero)
            {
                faceList = GetFaceList(environmentPosition);
            }

            //mTempTileFaceList.Clear();
            //mTempTileFaceList.AddRange(tile.faceIDs);
            for (int i = 0; i < tile.faceIDs.Length; i++)
            {
                //If the face list has all possibilities, then we don't have to check them
                if (possibleFaces[i].Count != poolFaceCount && !possibleFaces[i].Contains(tile.faceIDs[(int)mTempPermutationList[i]]))
                {
                    return false;
                }

                if (faceList != null)
                {
                    if (faceList[i] != -1)
                    {
                        faceWeights.Add(metatilepool.palette.ConnectionWeight(faceList[i], tile.faceIDs[(int)mTempPermutationList[i]]));
                    }
                }
            }


        }

        return true;  // no conflicts were found

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

        return CountValidMetatiles(wavefrontPosition) * 10000 - (faceWeight + 0.001f) + wavefrontPosition.y * 100000;

    }

    public int CountValidMetatiles(Vector3Int position)
    {
        int count = 0;
        foreach (KeyValuePair<MetaTile, bool> pair in environment[position.x, position.y, position.z].mValidMetatileList)
        {
            if (pair.Value)
            {
                count++;
            }
        }

        // print($"count : {count}");
        return count;
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

        if (debug)
        {
            Debug.Log($"Placed faces {position}:");
            Debug.Log($"    Top: {GetFaceName(faceList[(int)Tile.FACETYPE.TOP])}");
            Debug.Log($"    Bottom: {GetFaceName(faceList[(int)Tile.FACETYPE.BOTTOM])}");
            Debug.Log($"    Left: {GetFaceName(faceList[(int)Tile.FACETYPE.LEFT])}");
            Debug.Log($"    Right: {GetFaceName(faceList[(int)Tile.FACETYPE.RIGHT])}");
            Debug.Log($"    Front: {GetFaceName(faceList[(int)Tile.FACETYPE.FRONT])}");
            Debug.Log($"    Back: {GetFaceName(faceList[(int)Tile.FACETYPE.BACK])}");
        }

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

    public List<List<int>> GetValidFaceTypesAt(Vector3Int position)
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

    public List<MetatileProbability> GetFilteredMetatiles(Vector3Int placementPosition, MetatilePool metatilepool)
    {
        // Create a list of all legal metatiles from the pool
        List<MetatileProbability> metatiles = metatilepool.BuildMetatilePoolDeepCopy();

        if (DEBUG)
        {
            Debug.Log("Metatile Count after adding: " + metatiles.Count);
        }

        List<MetatileProbability> filteredMetatiles = FilterMetatiles(placementPosition, metatiles);

        if (DEBUG)
        {
            Debug.Log("Metatile Count after filtering: " + filteredMetatiles.Count);
        }

        return filteredMetatiles;
    }

    public List<MetatileProbability> FilterMetatiles(Vector3Int placementPosition, List<MetatileProbability> metatiles)
    {
        List<MetatileProbability> filteredMetatiles = new List<MetatileProbability>();
        // TODO: look into how WFC normally does this
        // TODO: allow metatilePools to weight dynamically (decay weights over time)
        List<List<int>> validAdjacentFaceTypeLists = GetValidFaceTypesAt(placementPosition);
        // select the shortest list of faces in possible faces and name it filterFaces
        List<int> mostConstrainedFaceTypes = new List<int>();
        int mostConstrainedFaceTypesCount = int.MaxValue;
        // TODO: FIX ME
        foreach (List<int> validAdjacentFaceTypes in validAdjacentFaceTypeLists)
        {
            if (validAdjacentFaceTypes.Count < mostConstrainedFaceTypesCount)
            {
                mostConstrainedFaceTypes = validAdjacentFaceTypes;
                mostConstrainedFaceTypesCount = validAdjacentFaceTypes.Count;
            }
        }

        // for (int positionFaceIdx = 0; positionFaceIdx < validAdjacentFaceTypeLists.Count; positionFaceIdx++)
        // {
        //     if (validAdjacentFaceTypeLists[positionFaceIdx].Count < mostConstrainedFaceTypesCount)
        //     {
        //         mostConstrainedFaceTypesCount = validAdjacentFaceTypeLists[positionFaceIdx].Count;
        //         mostConstrainedFaceTypes = validAdjacentFaceTypeLists[positionFaceIdx];
        //     }
        // }

        // example of a pre-filtering pass
        foreach (MetatileProbability metatile in metatiles)
        {
            foreach (Tile tile in metatile.metatileContainer.GetMetatiles()[0].tiles)
            {
                //Vector3Int tilePosition = new Vector3Int((int)tile.transform.localPosition.x, (int)tile.transform.localPosition.y, (int)tile.transform.localPosition.z);
                //TODO: FIX ME
                //List<int> tileFaces = GetFaceList(placementPosition + tilePosition);
                List<int> currentTileFaceTypes = new List<int>(tile.faceIDs);
                bool tileIsLegal = false;
                for (int i = 0; i < currentTileFaceTypes.Count; i++)
                {
                    if (mostConstrainedFaceTypes.Contains(currentTileFaceTypes[i]))
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

    private void setPotentialConfigurationList(List<ConfigurationValidity> validConfigurations)
    {
        mPotentialConfigurationList.Clear();
        for (int i = 0; i < validConfigurations.Count; i++)
        {
            if (validConfigurations[i] == ConfigurationValidity.VALID || validConfigurations[i] == ConfigurationValidity.UNKNOWN)
            {
                mPotentialConfigurationList.Add(i);
            }
        }
    }

    private int SelectConfiguration(List<ConfigurationValidity> validConfigurations, MetaTile candidateMetatile, Vector3Int placementPosition, EnvironmentTileState tileState)
    {
        while (mPotentialConfigurationList.Count > 0)
        {
            int putativeConfigurationIndex = GetEngine().GetRandomRange(0, mPotentialConfigurationList.Count);
            int candidateConfigurationIndex = mPotentialConfigurationList[putativeConfigurationIndex];

            if (validConfigurations[candidateConfigurationIndex] == ConfigurationValidity.VALID)
            {
                return candidateConfigurationIndex;
            }
            else if (validConfigurations[candidateConfigurationIndex] == ConfigurationValidity.UNKNOWN)
            {
                if (CheckAndSetConfigurationValidity(candidateConfigurationIndex, candidateMetatile, placementPosition, tileState))
                {
                    return candidateConfigurationIndex;
                }
            }
            mPotentialConfigurationList.RemoveAt(putativeConfigurationIndex);
        }
        return -1; // return -1 if no valid configuration is found
    }

    private bool CheckAndSetConfigurationValidity(int configurationIndex, MetaTile candidateMetatile, Vector3Int placementPosition, EnvironmentTileState tileState)
    {
        MetaTile.Configuration configTuple = candidateMetatile.GetConfiguration(configurationIndex);
        bool placeable = CanPlaceMetatile(placementPosition, candidateMetatile, configTuple);
        tileState.mValidMetatiles[candidateMetatile][configurationIndex] = placeable ? ConfigurationValidity.VALID : ConfigurationValidity.INVALID;
        return placeable;
    }

    private static global::System.Boolean IsCompletePlacement(Vector3Int placementPosition)
    {
        return placementPosition.x < 0;
    }

    private global::System.Boolean IsInitialPlacement()
    {
        return placedMetatiles.Count == 0;
    }

    private static global::System.Boolean IsNoValidConfiguration(global::System.Int32 selectedConfigurationIndex)
    {
        return selectedConfigurationIndex == -1;
    }


    private void DepositPayloads()
    {
        for (int i = 0; i < placedMetatiles.Count; i++)
        {
            placedPayloads.AddRange(placedMetatiles[i].metatile.DepositPayload(
                placedMetatiles[i].position,
                placedMetatiles[i].rotation,
                placedMetatiles[i].flipped,
                this,
                voxelSize
            ));
        }
    }

    void SavePayloads(string fileName)
    {
        JSONObject fullComponent = new JSONObject();

        fullComponent.AddField("w", mWidth);
        fullComponent.AddField("h", mHeight);
        fullComponent.AddField("l", mLength);
        fullComponent.AddField("v", voxelSize);

        JSONObject componentList = new JSONObject();

        for (int i = 0; i < placedMetatiles.Count; i++)
        {
            JSONObject componentJSON = new JSONObject();
            componentJSON.AddField("p_x", placedMetatiles[i].position.x);
            componentJSON.AddField("p_y", placedMetatiles[i].position.y);
            componentJSON.AddField("p_z", placedMetatiles[i].position.z);
            componentJSON.AddField("o", (int)placedMetatiles[i].orientation);
            componentJSON.AddField("f", placedMetatiles[i].flipped);
            componentJSON.AddField("t", placedMetatiles[i].metatile.name);

            componentList.Add(componentJSON);
        }
        fullComponent.AddField("d", componentList);

        //string path = Path.Combine(Application.dataPath, $"{fileName}.json");
        string path = fileName;
        path = path.Replace("{dataPath}", $"{Application.dataPath}/../Datasets/");

        string directoryPath = Path.GetDirectoryName(path);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // Write the JSON string to a file
        File.WriteAllText(path, fullComponent.ToString());
    }
    void LoadPayloads(string fileName)
    {
        //string path = Path.Combine(Application.dataPath, $"{fileName}.json");
        string path = fileName;
        path = path.Replace("{dataPath}", $"{Application.dataPath}/../Datasets/");

        string jsonData = File.ReadAllText(path);

        JSONObject jSONObject = new JSONObject(jsonData);

        mWidth = (int)jSONObject["w"].i;
        mHeight = (int)jSONObject["h"].i;
        mLength = (int)jSONObject["l"].i;

        voxelSize = jSONObject["v"].n;

        List<JSONObject> placedTiles = jSONObject["d"].list;

        Dictionary<string, MetaTile> metaTileMap = new Dictionary<string, MetaTile>();
        List<MetaTile> metaTiles = metatilepool.GetMetatiles();

        foreach (MetaTile metaTile in metaTiles)
        {
            metaTileMap[metaTile.name] = metaTile;
        }

        placedMetatiles.Clear();
        for (int i = 0; i < placedTiles.Count; i++)
        {
            Vector3Int position = new Vector3Int((int)placedTiles[i]["p_x"].i, (int)placedTiles[i]["p_y"].i, (int)placedTiles[i]["p_z"].i);
            Orientation orientation = (Orientation)placedTiles[i]["o"].n;
            bool flipped = placedTiles[i]["f"].b;

            string metaTileName = placedTiles[i]["t"].str;

            MetaTile metaTile = metaTileMap[metaTileName];

            Configuration configuration = new Configuration();
            configuration.orientation = orientation;
            configuration.flipped = flipped;
            configuration.origin = new Vector3();

            PlaceMetatile(metaTile, position, configuration);

            placedPayloads.AddRange(metaTile.DepositPayload(
                position,
                OrientationToQuaternion[configuration.orientation],
                flipped,
                this,
                voxelSize
            ));
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
