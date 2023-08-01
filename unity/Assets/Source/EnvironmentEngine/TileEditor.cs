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

        SerializedProperty serializedProperty = serializedObject.FindProperty("edgeIDs").Copy();

        if (serializedProperty.isArray && selectedTile.GetMetaTile() != null)
        {
            List<TileFace> faces = selectedTile.GetMetaTile().pallete.tileFaces;

            string[] faceOptions = new string[faces.Count];
            for (int i = 0; i < faces.Count; i++)
            {
                faceOptions[i] = faces[i].name;
            }

            serializedProperty.Next(true);
            serializedProperty.Next(true);
            serializedProperty.Next(true);

            isOpen = EditorGUILayout.Foldout(isOpen, "Edge IDs");

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
                    EditorGUI.DrawRect(colorRect, faces[selectedTile.faceIDs[i]].color);
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