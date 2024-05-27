using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Tile))]
public class TileEditor : Editor
{
    static bool isOpen = true;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        Tile selectedTile = (Tile)target;

        List<int> edgeIDs = new List<int>();

        SerializedProperty serializedProperty = serializedObject.FindProperty("faceIDs").Copy();

        if (serializedProperty.isArray && selectedTile.GetMetatile() != null)
        {
            List<TileFace> faces = selectedTile.GetMetatile().GetPalette().tileFaces;

            string[] faceOptions = new string[faces.Count];
            for (int i = 0; i < faces.Count; i++)
            {
                faceOptions[i] = faces[i].name;
            }

            serializedProperty.Next(true);
            serializedProperty.Next(true);
            serializedProperty.Next(true);

            isOpen = EditorGUILayout.Foldout(isOpen, "Face IDs");

            if (isOpen)
            {
                for (int i = 0; i < selectedTile.faceIDs.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel($"{((Tile.FACETYPE)i).ToString()}: ");
                    int index = EditorGUILayout.Popup(selectedTile.faceIDs[i], faceOptions);
                    EditorGUILayout.Space(20);
                    Rect colorRect = GUILayoutUtility.GetLastRect();
                    colorRect.width -= 2;
                    colorRect.height -= 2;
                    Color color = Color.white;
                    if (selectedTile.faceIDs[i] < faces.Count)
                    {
                        color = faces[selectedTile.faceIDs[i]].color;
                    }
                    EditorGUI.DrawRect(colorRect, color);
                    EditorGUILayout.EndHorizontal();

                    serializedProperty.intValue = (int)index;
                    if (i < selectedTile.faceIDs.Length)
                    {
                        serializedProperty.Next(false);
                    }
                }
            }
        }

        serializedObject.ApplyModifiedProperties();

    }
}