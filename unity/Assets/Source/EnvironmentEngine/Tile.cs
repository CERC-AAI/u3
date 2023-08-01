using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int[] faceTypes = new int[6]; // 6 faces for a cube

    // Directions: 0 = -x, 1 = +x, 2 = -y, 3 = +y, 4 = -z, 5 = +z

    // Placeholder: Hardcoded matching matrix
    private static Dictionary<int, List<int>> matchingMatrix = new Dictionary<int, List<int>>()
    {
        { 0, new List<int> { 0 } }, // 0 = bedrock connects to bedrock
        { 1, new List<int> { 2} }, // 1 = surface connects to air
        { 2, new List<int> { 1, 2} }, // 2 = air connects to bedrock, floor and air
    };


    public bool CanPlaceTile(Tile[,,] environment, Vector3Int position)
    {
        // Check if the faces of the tile are compatible with the neighboring tiles according to the matching matrix
        // Debug.Log("CanPlaceTile() position: " + position);
        int[] neighborFaces = GetNeighborFaces(environment, position);
        // Debug.Log("neighborFaces: " + neighborFaces[0] + " " + neighborFaces[1] + " " + neighborFaces[2] + " " + neighborFaces[3] + " " + neighborFaces[4] + " " + neighborFaces[5] + " ");

        for (int i = 0; i < 6; i++)
        {
            if (neighborFaces[i] == -1) // no face in this direction
            {
                continue;
            }

            if (!matchingMatrix[faceTypes[i]].Contains(neighborFaces[i]))
            {
                return false;
            }
        }

        return true;
    }

    public static Tile GetNeighbor(Tile[,,] environment, int x, int y, int z)
    {
        if (x < 0 || x >= environment.GetLength(0) ||
            y < 0 || y >= environment.GetLength(1) ||
            z < 0 || z >= environment.GetLength(2))
        {
            return null;
        }

        return environment[x, y, z];
    }

    public static int[] GetNeighborFaces(Tile[,,] environment, Vector3Int position)
    {
        int x = position.x;
        int y = position.y;
        int z = position.z;

        // Get the appropriate int from the faceTypes field of the neighboring tiles in each direction
        Tile[] neighbors = new Tile[6];
        neighbors[0] = GetNeighbor(environment, x - 1, y, z);
        neighbors[1] = GetNeighbor(environment, x + 1, y, z);
        neighbors[2] = GetNeighbor(environment, x, y - 1, z);
        neighbors[3] = GetNeighbor(environment, x, y + 1, z);
        neighbors[4] = GetNeighbor(environment, x, y, z - 1);
        neighbors[5] = GetNeighbor(environment, x, y, z + 1);

        int[] neighborFaces = new int[6];
        neighborFaces[0] = neighbors[0] == null ? -1 : neighbors[0].faceTypes[1];
        neighborFaces[1] = neighbors[1] == null ? -1 : neighbors[1].faceTypes[0];
        neighborFaces[2] = neighbors[2] == null ? -1 : neighbors[2].faceTypes[3];
        neighborFaces[3] = neighbors[3] == null ? -1 : neighbors[3].faceTypes[2];
        neighborFaces[4] = neighbors[4] == null ? -1 : neighbors[4].faceTypes[5];
        neighborFaces[5] = neighbors[5] == null ? -1 : neighbors[5].faceTypes[4];

        return neighborFaces;
    }

}
