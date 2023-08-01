using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

public class Tile : MonoBehaviour
{
    // TODO: rename to face?
    public enum FACETYPE { TOP, BOTTOM, LEFT, RIGHT, FRONT, BACK };
    [HideInInspector]
    public int[] faceIDs = new int[6]; // 6 edges for a cube
    MetaTile mParentMetaTile;


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

            if (!matchingMatrix[faceIDs[i]].Contains(neighborFaces[i]))
            {
                return false;
            }
        }

        return true;
    }

    // three types of edges: ground, bedrock, air
    // air can attach to anything
    // ground edge can attach to air and ground
    // bottom of ground is bedrock or base, can only attach to bedrock (not air)

    void OnEnable()
    {
        mParentMetaTile = GetComponentInParent<MetaTile>();
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
        neighborFaces[0] = neighbors[0] == null ? -1 : neighbors[0].faceIDs[1];
        neighborFaces[1] = neighbors[1] == null ? -1 : neighbors[1].faceIDs[0];
        neighborFaces[2] = neighbors[2] == null ? -1 : neighbors[2].faceIDs[3];
        neighborFaces[3] = neighbors[3] == null ? -1 : neighbors[3].faceIDs[2];
        neighborFaces[4] = neighbors[4] == null ? -1 : neighbors[4].faceIDs[5];
        neighborFaces[5] = neighbors[5] == null ? -1 : neighbors[5].faceIDs[4];

        return neighborFaces;
    }

    // Draw a semitransparent red cube at the transforms position
    void OnDrawGizmos()
    {
        MetaTile parentMetaTile = GetComponentInParent<MetaTile>();

        if (parentMetaTile.pallete == null)
        {
            parentMetaTile.pallete = Resources.Load<TileFacePalette>(TileFacePaletteEditor.defaultPalettePath);
        }
        float voxelSize = parentMetaTile.pallete.voxelSize;
        voxelSize = voxelSize * 1.01f;

        Gizmos.color = new Color(1, 1, 1, 0.25f);
        Gizmos.DrawCube(transform.position, new Vector3(voxelSize, voxelSize, voxelSize));

        List<Color> colors = new List<Color>();
        List<Vector3> positions = new List<Vector3>();
        List<Vector3> sizes = new List<Vector3>();

        Vector3Int thisPosition = new Vector3Int((int)transform.localPosition.x, (int)transform.localPosition.y, (int)transform.localPosition.z);

        //Draw each face
        for (FACETYPE i = FACETYPE.TOP; i <= FACETYPE.BACK; i++)
        {
            Color color = Color.clear;
            Vector3 position = Vector3.zero;
            Vector3 size = Vector3.zero;

            int faceID = faceIDs[(int)i];
            TileFace faceData = parentMetaTile.pallete.tileFaces[faceID];
            switch (i)
            {
                case FACETYPE.TOP:
                    if (parentMetaTile.HasTile(thisPosition + new Vector3Int(0, 1, 0)))
                    {
                        break;
                    }

                    color = faceData.color;
                    position = transform.position + new Vector3(0, voxelSize / 2 * 1.01f, 0);
                    size = new Vector3(voxelSize / 2, 0, voxelSize / 2);
                    break;

                case FACETYPE.BOTTOM:
                    if (parentMetaTile.HasTile(thisPosition - new Vector3Int(0, 1, 0)))
                    {
                        break;
                    }

                    color = faceData.color;
                    position = transform.position - new Vector3(0, voxelSize / 2 * 1.01f, 0);
                    size = new Vector3(voxelSize / 2, 0, voxelSize / 2);
                    break;

                case FACETYPE.LEFT:
                    if (parentMetaTile.HasTile(thisPosition - new Vector3Int(1, 0, 0)))
                    {
                        break;
                    }

                    color = faceData.color;
                    position = transform.position - new Vector3(voxelSize / 2 * 1.01f, 0, 0);
                    size = new Vector3(0, voxelSize / 2, voxelSize / 2);
                    break;

                case FACETYPE.RIGHT:
                    if (parentMetaTile.HasTile(thisPosition + new Vector3Int(1, 0, 0)))
                    {
                        break;
                    }

                    color = faceData.color;
                    position = transform.position + new Vector3(voxelSize / 2 * 1.01f, 0, 0);
                    size = new Vector3(0, voxelSize / 2, voxelSize / 2);
                    break;

                case FACETYPE.FRONT:
                    if (parentMetaTile.HasTile(thisPosition - new Vector3Int(0, 0, 1)))
                    {
                        break;
                    }

                    color = faceData.color;
                    position = transform.position - new Vector3(0, 0, voxelSize / 2 * 1.01f);
                    size = new Vector3(voxelSize / 2, voxelSize / 2, 0);
                    break;

                case FACETYPE.BACK:
                    if (parentMetaTile.HasTile(thisPosition + new Vector3Int(0, 0, 1)))
                    {
                        break;
                    }

                    color = faceData.color;
                    position = transform.position + new Vector3(0, 0, voxelSize / 2 * 1.01f);
                    size = new Vector3(voxelSize / 2, voxelSize / 2, 0);
                    break;
            }

            colors.Add(color);
            sizes.Add(size);
            positions.Add(position);
        }

        List<float> depths = new List<float>();
        List<int> orders = new List<int>();
        for (int i = 0; i < positions.Count; i++)
        {
            depths.Add(-SceneView.currentDrawingSceneView.camera.WorldToViewportPoint(positions[i]).z);
            orders.Add(i);
        }

        //Sort the faces by distance to camera
        orders.Sort((int a, int b) => depths[a].CompareTo(depths[b]));

        for (int i = 0; i < positions.Count; i++)
        {
            int thisIndex = orders[i];

            Color color = colors[thisIndex];
            Vector3 position = positions[thisIndex];
            Vector3 size = sizes[thisIndex];

            Gizmos.color = color;
            Gizmos.DrawCube(position, size);
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(position, size);
        }
    }

    public MetaTile GetMetaTile()
    {
        if (mParentMetaTile == null)
        {
            mParentMetaTile = GetComponentInParent<MetaTile>();
        }

        return mParentMetaTile;
    }
}
