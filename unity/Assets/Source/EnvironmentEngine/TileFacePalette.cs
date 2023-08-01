using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


[Serializable]
public class TileFace
{
    public Color color;
    public string name;
    int mIndex;
}

public class TileFacePalette : MonoBehaviour
{
    [Serializable]
    public class MatchedFaces
    {
        public int mA;
        public int mB;

        public MatchedFaces(int a, int b)
        {
            mA = a;
            mB = b;
        }
    }

    public float voxelSize = 1;
    public List<TileFace> tileFaces = new List<TileFace>();

    [HideInInspector]
    public List<MatchedFaces> matchingMatrix = new List<MatchedFaces>();

    public bool CanConnect(int tileFace1, int tileFace2)
    {

        for (int i = 0; i < matchingMatrix.Count; i++)
        {
            if (matchingMatrix[i].mA == tileFace1 && matchingMatrix[i].mB == tileFace2 ||
                matchingMatrix[i].mA == tileFace2 && matchingMatrix[i].mB == tileFace1)
            {
                return true;
            }
        }

        return false;
    }
}