using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.AI;

public class MetaTileEnvironment : MonoBehaviour
{

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

    public List<MetaTile> placedMetaTiles = new List<MetaTile>();

    public List<Vector3Int> placedPositions = new List<Vector3Int>();

    public enum TileState { NotPlaced, Wavefront, Placed };

    public TileState[,,] tileState = new TileState[10, 10, 10];

    public List<Vector3Int> wavefrontPositions = new();

    public List<float> wavefrontEntropies = new();

    public bool DEBUG = false;


    /*public List<int> MapPositionToFaces(int[,,,] faces, Vector3Int position)
    {
        List<int> faceList = new List<int>();

        // add faces from the x direction
        // TODO: use the enum TOP, BOTTOM, etc.
        // Unity uses a lefthanded coordinate system
        int yFace2 = faces[1, position.x, position.y + 1, position.z]; //Top
        int yFace1 = faces[1, position.x, position.y, position.z]; //Bottom

        int xFace1 = faces[0, position.x, position.y, position.z]; //Left
        int xFace2 = faces[0, position.x + 1, position.y, position.z]; //Right

        int zFace1 = faces[2, position.x, position.y, position.z]; //Front 
        int zFace2 = faces[2, position.x, position.y, position.z + 1]; //Back

        // create a list of lists of faces
        faceList.Add(yFace2);
        faceList.Add(yFace1);
        faceList.Add(xFace1);
        faceList.Add(xFace2);
        faceList.Add(zFace1);
        faceList.Add(zFace2);

        return faceList;
    }*/

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

            // select the lowest entropy position
            // Finds the position with the minimum entropy value in the wavefrontPositions list
            // find the minimum entropy value in the wavefrontEntropies list
            float minEntropy = wavefrontEntropies.Min();

            // find the index of the minimum entropy value in the wavefrontEntropies list
            int minEntropyIndex = wavefrontEntropies.IndexOf(minEntropy);

            Vector3Int placementPosition = wavefrontPositions[minEntropyIndex];
            // select the position with the minimum entropy value
            Debug.Log("SelectPlacementPosition() placementPosition: " + placementPosition);
            return placementPosition;
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

    public List<int> GetFaceList(Vector3Int position)
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

        if (DEBUG)
        {
            Debug.Log("Get placed faces:");
            Debug.Log($"    Top: {faceList[(int)Tile.FACETYPE.TOP]}");
            Debug.Log($"    Bottom: {faceList[(int)Tile.FACETYPE.BOTTOM]}");
            Debug.Log($"    Left: {faceList[(int)Tile.FACETYPE.LEFT]}");
            Debug.Log($"    Right: {faceList[(int)Tile.FACETYPE.RIGHT]}");
            Debug.Log($"    Front: {faceList[(int)Tile.FACETYPE.FRONT]}");
            Debug.Log($"    Back: {faceList[(int)Tile.FACETYPE.BACK]}");
        }

        return faceList;
    }

    public void SetFaceList(Vector3Int position, List<int> faceList)
    {
        faces[1, position.x, position.y + 1, position.z] = faceList[(int)Tile.FACETYPE.TOP];
        faces[1, position.x, position.y, position.z] = faceList[(int)Tile.FACETYPE.BOTTOM];
        faces[0, position.x, position.y, position.z] = faceList[(int)Tile.FACETYPE.LEFT];
        faces[0, position.x + 1, position.y, position.z] = faceList[(int)Tile.FACETYPE.RIGHT];
        faces[2, position.x, position.y, position.z] = faceList[(int)Tile.FACETYPE.FRONT];
        faces[2, position.x, position.y, position.z + 1] = faceList[(int)Tile.FACETYPE.BACK];

        if (DEBUG)
        {
            Debug.Log("Placed faces:");
            Debug.Log($"    Top: {faceList[(int)Tile.FACETYPE.TOP]}");
            Debug.Log($"    Bottom: {faceList[(int)Tile.FACETYPE.BOTTOM]}");
            Debug.Log($"    Left: {faceList[(int)Tile.FACETYPE.LEFT]}");
            Debug.Log($"    Right: {faceList[(int)Tile.FACETYPE.RIGHT]}");
            Debug.Log($"    Front: {faceList[(int)Tile.FACETYPE.FRONT]}");
            Debug.Log($"    Back: {faceList[(int)Tile.FACETYPE.BACK]}");
        }

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
        List<int> faceList = GetFaceList(position);

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

    public List<MetaTile> CustomFiltering(Vector3Int placementPosition, List<MetaTile> metatiles)
    {
        List<MetaTile> filteredMetatiles = new List<MetaTile>();
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
        foreach (MetaTile metatile in metatiles)
        {
            foreach (Tile tile in metatile.tiles)
            {
                Vector3Int tilePosition = new Vector3Int((int)tile.transform.localPosition.x, (int)tile.transform.localPosition.y, (int)tile.transform.localPosition.z);
                //FIX ME
                //List<int> tileFaces = GetFaceList(placementPosition + tilePosition);
                List<int> tileFaces = new List<int>(tile.faceIDs);
                if (filterFaces.Contains(tileFaces[filterFaceIdx]))
                {
                    // check the other faces of the tile against the other faces in possibleFaces
                    bool tileIsLegal = true;
                    for (int i = 0; i < tileFaces.Count; i++)
                    {
                        if (i != filterFaceIdx && !possibleFaces[i].Contains(tileFaces[i]))
                        {
                            tileIsLegal = false;
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
        }

        return filteredMetatiles;
    }

    public MetaTile DrawMetaTile(Vector3Int placementPosition, MetaTilePool metatilepool)
    {


        // Create a list of all legal metatiles from the pool
        List<MetaTile> metatiles = new List<MetaTile>();

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
    }

    public MetaTile GetMetaTile(Vector3Int placementPosition, MetaTilePool metatilepool)
    {
        // Create a list of all legal metatiles from the pool
        List<MetaTile> metatiles = new List<MetaTile>();

        if (DEBUG)
        {
            Debug.Log("Metatile Count before adding: " + metatiles.Count);
        }

        // TODO: fix this to exhaustively grab all possible metatiles
        foreach (MetaTileProbability metatileprobability in metatilepool.metatileProbabilities)
        {
            metatiles.AddRange(metatileprobability.metaTileProbability.GetMetaTiles());
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

        if (filteredMetatiles.Count > 0)
        {
            // select a random metatile from the filtered list
            return filteredMetatiles[UnityEngine.Random.Range(0, filteredMetatiles.Count)];
        }
        else
        {
            return null;
        }
    }

    public bool CanPlaceMetaTile(Vector3Int placementPosition, MetaTile metatile)
    {
        // TODO: add rotations and reflections
        foreach (Tile tile in metatile.tiles)
        {
            Vector3Int tilePosition = new Vector3Int((int)tile.transform.localPosition.x, (int)tile.transform.localPosition.y, (int)tile.transform.localPosition.z);

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
            /*foreach (int face in tileFaces)
            {
                // if the face is not in the list of possible faces at the index of the face in tileFaces, return false
                if (!possibleFaces[tileFaces.IndexOf(face)].Contains(face))
                {
                    return false;
                }
            }*/
            for (int i = 0; i < tileFaces.Count; i++)
            {
                // if the face is not in the list of possible faces at the index of the face in tileFaces, return false
                if (!possibleFaces[i].Contains(tileFaces[i]))
                {
                    return false;
                }
            }

        }

        return true;  // no conflicts were found

    }

    public void PlaceMetaTile(Vector3Int placementPosition, MetaTile metatile)
    {
        //Debug.Log("placing metatile " + this.name);

        foreach (Tile tile in metatile.tiles)
        {
            Vector3Int tilePosition = new Vector3Int((int)tile.transform.localPosition.x, (int)tile.transform.localPosition.y, (int)tile.transform.localPosition.z);

            int envX = placementPosition.x + tilePosition.x;
            int envY = placementPosition.y + tilePosition.y;
            int envZ = placementPosition.z + tilePosition.z;

            environment[envX, envY, envZ] = tile;

            // update the faces
            // TODO: replace with TOP, BOTTOM, etc. enum
            //Debug.Log($"Tile face {tile.faceIDs[0]}");
            SetFaceList(placementPosition, new List<int>(tile.faceIDs));

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
        // calculate the entropies of the tiles in the wavefront

        //This is potentially very slow
        //foreach (Vector3Int position in wavefrontPositions)
        for (int i = 0; i < wavefrontPositions.Count; i++)
        {
            // get the index of the position in the wavefrontPositions list
            wavefrontEntropies[i] = CalculateEntropy(wavefrontPositions[i]);
        }
    }

    public int CalculateEntropy(Vector3Int position)
    {
        // Gets the stored entropy value of a position
        // Calculate the entropy of a position
        // Entropy is the total number of faces that can be placed around a position
        // The lower the entropy, the more constrained the position is
        // The higher the entropy, the more freedom there is to place a face at that position
        // Do full translation/rotation tests and cache adjacent tile legality
        // Dilation of tile checks and translation?

        // Get the faces for each position
        List<int> faceList = GetFaceList(position);

        // Calculate the entropy
        int entropy = metatilepool.palette.tileFaces.Count;
        for (int i = 0; i < faceList.Count; i++)
        {
            entropy = Mathf.Min(entropy, metatilepool.palette.GetPossibleConnections(faceList[i]).Count);
        }

        return entropy;
    }


    public IEnumerator GenerateEnvironment(MetaTilePool metatilepool)
    {

        // set up the attempt loop around the metatile pool
        MetaTilePool.RESULTTYPE resultType = MetaTilePool.RESULTTYPE.SUCCESS;
        int timeoutCounter = 0;
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
                break;
            }

            // Select a candidate metatile
            // candidateMetatile = DrawMetaTile(placementPosition, metatilepool);
            candidateMetatile = GetMetaTile(placementPosition, metatilepool);

            // Try to find a place to put the metatile
            if (candidateMetatile != null && CanPlaceMetaTile(placementPosition, candidateMetatile))
            {
                PlaceMetaTile(placementPosition, candidateMetatile);
                CollapseWaveFunction();
                resultType = MetaTilePool.RESULTTYPE.SUCCESS;
                timeoutCounter = 0;
                placedMetaTiles.Add(candidateMetatile);
                placedPositions.Add(placementPosition);
                Debug.Log("MetaTilePool.RESULTTYPE.SUCCESS, placedMetaTiles.Add(candidateMetatile);, placedMetatiles.Count = " + placedMetaTiles.Count);

                if (DEBUG)
                {
                    candidateMetatile.DepositPayload(placementPosition);
                    Debug.Break();

                    yield return null;
                }
            }
            else
            {
                resultType = MetaTilePool.RESULTTYPE.FAILURE;
                timeoutCounter++;
                Debug.Log("MetaTilePool.RESULTTYPE.FAILURE, timeoutCounter++");
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

        if (timeoutCounter <= 100 && resultType == MetaTilePool.RESULTTYPE.COMPLETE)
        {
            Debug.Log("Complete");

            for (int i = 0; i < placedMetaTiles.Count; i++)
            {
                placedMetaTiles[i].DepositPayload(placedPositions[i]);
            }
        }

        yield return null;
    }

    public void Awake()
    {
        DEBUG = false;

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
