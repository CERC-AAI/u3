using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MetaTilePool))]
public class MetaTilePoolEditor : Editor
{
    static bool isOpen = true;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        MetaTilePool selectedMetatilePool = (MetaTilePool)target;

        if (GUILayout.Button("Update Children"))
        {
            for (int i = 0; i < selectedMetatilePool.metatileProbabilities.Count; i++)
            {
                selectedMetatilePool.metatileProbabilities[i].metaTileProbability.parent = selectedMetatilePool;
            }            

            EditorUtility.SetDirty(selectedMetatilePool);
        }
    }
}