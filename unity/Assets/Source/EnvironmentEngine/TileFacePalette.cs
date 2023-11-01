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
    public float weight;
}

public class TileFacePalette : MonoBehaviour
{
    public static string defaultPalettePath = "Palettes/TestPalette";

    [Serializable]
    public class MatchedFaces
    {
        public int mA;
        public int mB;
        public float mWeight = 1.0f;

        public MatchedFaces(int a, int b, float weight)
        {
            mA = a;
            mB = b;
            mWeight = weight;
        }
    }

    public float voxelSize = 1;
    public List<TileFace> tileFaces = new List<TileFace>();


    [HideInInspector]
    public List<MatchedFaces> matchingMatrix = new List<MatchedFaces>();

    List<List<int>> mPossibleFaces = new List<List<int>>();
    List<int> mAllFaces = new List<int>();

    /*public bool CanConnect(int tileFace1, int tileFace2)
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
    }*/

    public float ConnectionWeight(int tileFace1, int tileFace2)
    {
        for (int i = 0; i < matchingMatrix.Count; i++)
        {
            if (matchingMatrix[i].mA == tileFace1 && matchingMatrix[i].mB == tileFace2)
            {
                return matchingMatrix[i].mWeight;
            }
        }

        return 0.0f;
    }

    public List<int> GetPossibleConnections(int tileFace)
    {
        if (tileFace == -1)
        {
            if (mAllFaces.Count == 0)
            {
                for (int j = 0; j < tileFaces.Count; j++)
                {
                    mAllFaces.Add(j);
                }
            }

            return mAllFaces;
        }

        if (mPossibleFaces.Count == 0)
        {
            for (int j = 0; j < tileFaces.Count; j++)
            {
                mPossibleFaces.Add(new List<int>());

                for (int i = 0; i < matchingMatrix.Count; i++)
                {
                    if (matchingMatrix[i].mA == j && matchingMatrix[i].mWeight != 0)
                    {
                        mPossibleFaces[j].Add(matchingMatrix[i].mB);
                    }
                }
            }
        }

        return mPossibleFaces[tileFace];
    }
}
