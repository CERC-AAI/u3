using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.AI;

public class MetaTileEnvironment : MonoBehaviour
{

    public static int mWidth = 10;
    private static Dictionary<int, List<int>> matchingMatrix = new Dictionary<int, List<int>>()
    {
        {0, new List<int> {0,1,2}},
        { 1, new List<int> { 0 } }, // 0 = bedrock connects to bedrock
        { 2, new List<int> { 2} }, // 1 = surface connects to air
        { 3, new List<int> { 1, 2} }, // 2 = air connects to bedrock, floor and air
    };

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


    public List<int> MapPositionToFaces(int[,,,] faces, Vector3Int position)
    {
        List<int> faceList = new List<int>();

        // add faces from the x direction
        // TODO: use the enum TOP, BOTTOM, etc.
        // Unity uses a lefthanded coordinate system
        int xFace1 = faces[0, position.x, position.y, position.z];
        int xFace2 = faces[0, position.x + 1, position.y, position.z];
        int yFace1 = faces[1, position.x, position.y, position.z];
        int yFace2 = faces[1, position.x, position.y + 1, position.z];
        int zFace1 = faces[2, position.x, position.y, position.z];
        int zFace2 = faces[2, position.x, position.y, position.z + 1];

        // create a list of lists of faces
        faceList.Add(xFace1);
        faceList.Add(xFace2);
        faceList.Add(yFace1);
        faceList.Add(yFace2);
        faceList.Add(zFace1);
        faceList.Add(zFace2);

        return faceList;
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
                return new Vector3Int(-1, -1, -1);
            }

            // Select a random empty position
            return emptyPositions[UnityEngine.Random.Range(0, emptyPositions.Count)];
        }
        else
        {
            // select the lowest entropy position
            // Finds the position with the minimum entropy value in the wavefrontPositions list
            // find the minimum entropy value in the wavefrontEntropies list
            float minEntropy = wavefrontEntropies.Min();

            // find the index of the minimum entropy value in the wavefrontEntropies list
            int minEntropyIndex = wavefrontEntropies.IndexOf(minEntropy);

            // select the position with the minimum entropy value
            return wavefrontPositions[minEntropyIndex];
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

    public List<List<int>> GetPossibleFaces(Vector3Int position)
    {
        List<int> faceList = new List<int>();

        // add faces from the x direction
        int xFace1 = faces[0, position.x, position.y, position.z];
        int xFace2 = faces[0, position.x + 1, position.y, position.z];
        int yFace1 = faces[1, position.x, position.y, position.z];
        int yFace2 = faces[1, position.x, position.y + 1, position.z];
        int zFace1 = faces[2, position.x, position.y, position.z];
        int zFace2 = faces[2, position.x, position.y, position.z + 1];

        // create a list of lists of faces
        faceList.Add(xFace1);
        faceList.Add(xFace2);
        faceList.Add(yFace1);
        faceList.Add(yFace2);
        faceList.Add(zFace1);
        faceList.Add(zFace2);

        // replace null faces with -1
        for (int i = 0; i < faceList.Count; i++)
        {
            if (faceList[i] == null)
            {
                faceList[i] = -1;
            }
        }

        // return the list of faces permitted for each face at the position according to the matching matrix
        List<List<int>> possibleFaces = new List<List<int>>();
        foreach (int face in faceList)
        {
            possibleFaces.Add(matchingMatrix[face]);
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
        foreach (List<int> faceList in possibleFaces)
        {
            if (faceList.Count < shortestListLength)
            {
                shortestListLength = faceList.Count;
                filterFaces = faceList;
                filterFaceIdx = possibleFaces.IndexOf(faceList);
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
                List<int> tileFaces = MapPositionToFaces(faces, placementPosition + tilePosition);
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

    public MetaTile GetMetaTile(Vector3Int placementPosition, MetaTilePool metatilepool)
    {


        // Create a list of all legal metatiles from the pool
        List<MetaTile> metatiles = new List<MetaTile>();

        // TODO: fix this to exhaustively grab all possible metatiles
        foreach (MetaTileProbability metatileprobability in metatilepool.metatileProbabilities)
        {
            metatiles.Add(metatileprobability.metaTileProbability.GetMetaTile(placementPosition, environment, faces, matchingMatrix));
        }


        List<MetaTile> filteredMetatiles = CustomFiltering(placementPosition, metatiles);

        // select a random metatile from the filtered list
        return filteredMetatiles[UnityEngine.Random.Range(0, filteredMetatiles.Count)];
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
            List<int> tileFaces = MapPositionToFaces(faces, environmentPosition);
            foreach (int face in tileFaces)
            {
                // if the face is not in the list of possible faces at the index of the face in tileFaces, return false
                if (!possibleFaces[tileFaces.IndexOf(face)].Contains(face))
                {
                    return false;
                }
            }

        }

        return true;  // no conflicts were found

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
            faces[0, envX, envY, envZ] = tile.faceIDs[0];
            faces[0, envX + 1, envY, envZ] = tile.faceIDs[1];
            faces[1, envX, envY, envZ] = tile.faceIDs[2];
            faces[1, envX, envY + 1, envZ] = tile.faceIDs[3];
            faces[2, envX, envY, envZ] = tile.faceIDs[4];
            faces[2, envX, envY, envZ + 1] = tile.faceIDs[5];

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

        foreach (Vector3Int position in wavefrontPositions)
        {
            // get the index of the position in the wavefrontPositions list
            int wavefrontIndex = wavefrontPositions.IndexOf(position);
            wavefrontEntropies[wavefrontIndex] = CalculateEntropy(position);
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
        List<int> faceList = MapPositionToFaces(faces, position);

        // Calculate the entropy
        int entropy = 0;
        for (int i = 0; i < faceList.Count; i++)
        {
            entropy += matchingMatrix[faceList[i]].Count;
        }

        return entropy;
    }


    public void GenerateEnvironment(MetaTilePool metatilepool)
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
            candidateMetatile = GetMetaTile(placementPosition, metatilepool);

            // Try to find a place to put the metatile
            if (CanPlaceMetaTile(placementPosition, candidateMetatile))
            {
                PlaceMetaTile(placementPosition, candidateMetatile);
                CollapseWaveFunction();
                resultType = MetaTilePool.RESULTTYPE.SUCCESS;
                timeoutCounter = 0;
                placedMetaTiles.Add(candidateMetatile);
                placedPositions.Add(placementPosition);
                Debug.Log("placedMetaTiles.Add(candidateMetatile);");
            }
            else
            {
                resultType = MetaTilePool.RESULTTYPE.FAILURE;
                timeoutCounter++;
                Debug.Log("timeoutCounter++");
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
    }

    public void Awake()
    {
        GenerateEnvironment(metatilepool);
    }
}
