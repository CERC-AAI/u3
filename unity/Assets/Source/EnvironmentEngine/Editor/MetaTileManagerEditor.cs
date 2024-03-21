using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MetatileManager))]
public class MetatileManagerEditor : Editor
{
    static bool isOpen = true;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        MetatileManager selectedManager = (MetatileManager)target;

        List<int> edgeIDs = new List<int>();

        SerializedProperty serializedProperty = serializedObject.FindProperty("edgeTiles").Copy();

        if (serializedProperty.isArray && selectedManager.metatilepool != null)
        {
            List<TileFace> faces = selectedManager.metatilepool.palette.tileFaces;

            string[] faceOptions = new string[faces.Count + 1];
            faceOptions[0] = "Any"; //Any
            for (int i = 0; i < faces.Count; i++)
            {
                faceOptions[i + 1] = faces[i].name;
            }

            serializedProperty.Next(true);
            serializedProperty.Next(true);
            serializedProperty.Next(true);

            isOpen = EditorGUILayout.Foldout(isOpen, "Boundary faces");

            if (isOpen)
            {
                for (int i = 0; i < selectedManager.edgeTiles.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel($"{((Tile.FACETYPE)i).ToString()}: ");
                    int index = EditorGUILayout.Popup(selectedManager.edgeTiles[i] + 1, faceOptions) - 1;
                    EditorGUILayout.Space(20);
                    Rect colorRect = GUILayoutUtility.GetLastRect();
                    colorRect.width -= 2;
                    colorRect.height -= 2;
                    Color color = Color.white;
                    if (selectedManager.edgeTiles[i] >= 0 && selectedManager.edgeTiles[i] < faces.Count)
                    {
                        color = faces[selectedManager.edgeTiles[i]].color;
                    }
                    EditorGUI.DrawRect(colorRect, color);
                    EditorGUILayout.EndHorizontal();

                    serializedProperty.intValue = (int)index;
                    if (i < selectedManager.edgeTiles.Count)
                    {
                        serializedProperty.Next(false);
                    }
                }
            }
        }

        serializedObject.ApplyModifiedProperties();

    }
}