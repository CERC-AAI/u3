using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

public class Tile : MonoBehaviour
{
    // TODO: rename to face?
    public enum EDGETYPE { TOP, BOTTOM, LEFT, RIGHT, FRONT, BACK };
    [HideInInspector]
    public int[] edgeIDs = new int[6]; // 6 edges for a cube
    MetaTile mParentMetaTile;


    // Hardcoded matching matrix
    // We assume each color matches only with itself
    // Hardcoded matching matrix
    // We assume each ID matches only with itself

    // three types of edges: ground, bedrock, air
    // air can attach to anything
    // ground edge can attach to air and ground
    // bottom of ground is bedrock or base, can only attach to bedrock (not air)

    void OnEnable()
    {
        mParentMetaTile = GetComponentInParent<MetaTile>();
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
        for (EDGETYPE i = EDGETYPE.TOP; i <= EDGETYPE.BACK; i++)
        {
            Color color = Color.clear;
            Vector3 position = Vector3.zero;
            Vector3 size = Vector3.zero;

            int faceID = edgeIDs[(int)i];
            TileFace faceData = parentMetaTile.pallete.tileFaces[faceID];
            switch (i)
            {
                case EDGETYPE.TOP:
                    if (parentMetaTile.HasTile(thisPosition + new Vector3Int(0, 1, 0)))
                    {
                        break;
                    }

                    color = faceData.color;
                    position = transform.position + new Vector3(0, voxelSize / 2 * 1.01f, 0);
                    size = new Vector3(voxelSize / 2, 0, voxelSize / 2);
                    break;

                case EDGETYPE.BOTTOM:
                    if (parentMetaTile.HasTile(thisPosition - new Vector3Int(0, 1, 0)))
                    {
                        break;
                    }

                    color = faceData.color;
                    position = transform.position - new Vector3(0, voxelSize / 2 * 1.01f, 0);
                    size = new Vector3(voxelSize / 2, 0, voxelSize / 2);
                    break;

                case EDGETYPE.LEFT:
                    if (parentMetaTile.HasTile(thisPosition - new Vector3Int(1, 0, 0)))
                    {
                        break;
                    }

                    color = faceData.color;
                    position = transform.position - new Vector3(voxelSize / 2 * 1.01f, 0, 0);
                    size = new Vector3(0, voxelSize / 2, voxelSize / 2);
                    break;

                case EDGETYPE.RIGHT:
                    if (parentMetaTile.HasTile(thisPosition + new Vector3Int(1, 0, 0)))
                    {
                        break;
                    }

                    color = faceData.color;
                    position = transform.position + new Vector3(voxelSize / 2 * 1.01f, 0, 0);
                    size = new Vector3(0, voxelSize / 2, voxelSize / 2);
                    break;

                case EDGETYPE.FRONT:
                    if (parentMetaTile.HasTile(thisPosition - new Vector3Int(0, 0, 1)))
                    {
                        break;
                    }

                    color = faceData.color;
                    position = transform.position - new Vector3(0, 0, voxelSize / 2 * 1.01f);
                    size = new Vector3(voxelSize / 2, voxelSize / 2, 0);
                    break;

                case EDGETYPE.BACK:
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
