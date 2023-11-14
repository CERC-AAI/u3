using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MetatilePool))]
public class MetatilePoolEditor : Editor
{
    static bool isOpen = true;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        MetatilePool selectedMetatilePool = (MetatilePool)target;

        if (GUILayout.Button("Update Children"))
        {
            for (int i = 0; i < selectedMetatilePool.metatileProbabilities.Count; i++)
            {
                selectedMetatilePool.metatileProbabilities[i].metatileContainer.parent = selectedMetatilePool;
            }

            EditorUtility.SetDirty(selectedMetatilePool);
        }
    }
}