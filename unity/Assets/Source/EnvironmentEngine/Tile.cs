using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Tile : MonoBehaviour
{
    // TODO: rename to face?
    public enum EDGETYPE { TOP, BOTTOM, LEFT, RIGHT, FRONT, BACK };
    public int[] edgeIDs = new int[6]; // 6 edges for a cube

    // Hardcoded matching matrix
    // We assume each color matches only with itself
    // Hardcoded matching matrix
    // We assume each ID matches only with itself

    // three types of edges: ground, bedrock, air
    // air can attach to anything
    // ground edge can attach to air and ground
    // bottom of ground is bedrock or base, can only attach to bedrock (not air)
    private static Dictionary<int, List<int>> matchingMatrix = new Dictionary<int, List<int>>()
    {
        { 0, new List<int> { 0, 2} },
        { 1, new List<int> { 0, 1 } },
        { 2, new List<int> { 0, 2 } },
    };

    public bool CanConnect(Tile otherTile, int thisEdge, int otherEdge)
    {
        int thisID = edgeIDs[thisEdge];
        int otherID = otherTile.edgeIDs[otherEdge];
        return matchingMatrix[thisID].Contains(otherID);
    }

    // public bool CanPlaceTile(Tile[,,] environment, Vector3Int position, Vector3Int rotation)
    // {
    //     // TODO: look at all possible neighbors and see if they match
    //     // if they match, return true
    //     // if they don't match, return false
    //     // if there is no neighbor, return true

    // }
    // Get the neighboring tile in the environment in the specified direction
    // Directions: 0 = -x, 1 = +y, 2 = +z, 3 = +x, 4 = -y, 5 = -z
    private static Tile GetNeighbor(Tile[,,] environment, int x, int y, int z, int direction)
    {
        switch (direction)
        {
            case 0: return x > 0 ? environment[x - 1, y, z] : null;
            case 1: return y < environment.GetLength(1) - 1 ? environment[x, y + 1, z] : null;
            case 2: return z < environment.GetLength(2) - 1 ? environment[x, y, z + 1] : null;
            case 3: return x < environment.GetLength(0) - 1 ? environment[x + 1, y, z] : null;
            case 4: return y > 0 ? environment[x, y - 1, z] : null;
            case 5: return z > 0 ? environment[x, y, z - 1] : null;
            default: throw new ArgumentException("Invalid direction.");
        }
    }

}
