using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileFacePalette))]
public class TileFacePaletteEditor : Editor
{
    static bool isOpen = true;
    public static string defaultPalettePath = "Palettes/TestPalette";

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        TileFacePalette selectedPalette = (TileFacePalette)target;

        //SerializedProperty matchingMatrix = serializedObject.FindProperty("matchingMatrix").Copy();

        // matchingMatrix.ClearArray();

        int oldFaceCount = selectedPalette.tileFaces.Count;

        DrawDefaultInspector();

        isOpen = EditorGUILayout.Foldout(isOpen, "Face Matching Matrix");

        if (selectedPalette.tileFaces.Count != oldFaceCount)
        {
            if (selectedPalette.tileFaces.Count > oldFaceCount)
            {
                for (int i = oldFaceCount; i < selectedPalette.tileFaces.Count; i++)
                {
                    selectedPalette.matchingMatrix.Add(new TileFacePalette.MatchedFaces(i, i)); 
                }
            }
        }

        if (isOpen)
        {

            int numFaces = selectedPalette.tileFaces.Count;

            float labelSize = 0;
            float checkboxSize = 25;
            float indent = 10;
            float colorSize = 25;
            int margin = 2;

            for (int i = 0; i < numFaces; i++)
            {
                Vector2 textDimensions = GUI.skin.label.CalcSize(new GUIContent(selectedPalette.tileFaces[i].name)); 
                if (labelSize < textDimensions.x)
                {
                    labelSize = textDimensions.x;
                }
            }
            labelSize += colorSize;


            GUILayout.BeginScrollView(new Vector2(0, 0), GUILayout.Height(checkboxSize * numFaces + labelSize + indent));

            Rect blankRect = new Rect(0, 0, labelSize + indent, labelSize);
            Rect topLabelRect = new Rect(labelSize + indent, 0, checkboxSize * numFaces, labelSize);
            Rect leftLabelRect = new Rect(0, labelSize, indent + labelSize, checkboxSize * numFaces);
            Rect checkBoxRect = new Rect(indent + labelSize, labelSize, checkboxSize * numFaces, checkboxSize * numFaces);

            /*EditorGUI.DrawRect(blankRect, Color.red);
            EditorGUI.DrawRect(topLabelRect, Color.blue);
            EditorGUI.DrawRect(leftLabelRect, Color.green);
            EditorGUI.DrawRect(checkBoxRect, Color.black);*/


            Vector3 root = new Vector3(labelSize + indent, labelSize - colorSize, 0);
            EditorGUIUtility.RotateAroundPivot(-90f, root);
            for (int i = 0; i < numFaces; i++)
            {
                Rect labelRect = new Rect(indent + margin + labelSize, labelSize + checkboxSize * (i - 1), labelSize, checkboxSize);
                EditorGUI.LabelField(labelRect, selectedPalette.tileFaces[i].name);
            }
            EditorGUIUtility.RotateAroundPivot(90f, root);

            for (int i = 0; i < numFaces; i++)
            {
                Rect labelRect = new Rect(indent, labelSize + checkboxSize * i, labelSize, checkboxSize);
                EditorGUI.LabelField(labelRect, selectedPalette.tileFaces[i].name);

                Rect colorRectColumn = new Rect(indent + labelSize - colorSize, labelSize + checkboxSize * i, colorSize - margin, colorSize - margin);
                EditorGUI.DrawRect(colorRectColumn, selectedPalette.tileFaces[i].color);

                Rect colorRectRow = new Rect(labelSize + indent + checkboxSize * i, labelSize - colorSize, colorSize - margin, colorSize - margin);
                EditorGUI.DrawRect(colorRectRow, selectedPalette.tileFaces[i].color);
            }



            List<TileFacePalette.MatchedFaces> matchingMatrixData = new List<TileFacePalette.MatchedFaces>();
            for (int i = 0; i < selectedPalette.matchingMatrix.Count; i++)
            {
                matchingMatrixData.Add(new TileFacePalette.MatchedFaces(selectedPalette.matchingMatrix[i].mA, selectedPalette.matchingMatrix[i].mB));
            }

            selectedPalette.matchingMatrix.Clear();

            bool[][] tempData = new bool[numFaces][];
            for (int i = 0; i < numFaces; i++)
            {
                tempData[i] = new bool[numFaces];
            }

            for (int i = 0; i < matchingMatrixData.Count; i++)
            {
                if (matchingMatrixData[i].mA < numFaces && matchingMatrixData[i].mB < numFaces)
                {
                    tempData[matchingMatrixData[i].mA][matchingMatrixData[i].mB] = true;
                    tempData[matchingMatrixData[i].mB][matchingMatrixData[i].mA] = true;
                }
            }


            int index = 0;
            for (int i = 0; i < numFaces; i++)
            {
                for (int j = 0; j < numFaces; j++)
                {
                    if (i <= j)
                    {
                        bool oldValue = tempData[i][j];
                        Rect checkboxRect = new Rect(labelSize + indent + checkboxSize * i + margin + 2, labelSize + checkboxSize * j, colorSize - margin, colorSize - margin);
                        bool toggled = GUI.Toggle(checkboxRect, tempData[i][j], "");

                        if (toggled)
                        {
                            selectedPalette.matchingMatrix.Add(new TileFacePalette.MatchedFaces(i, j));

                            index++;
                        }

                        if (oldValue != toggled)
                        {
                            EditorUtility.SetDirty(selectedPalette);
                        }
                    }
                }
            }

            GUILayout.EndScrollView();
        }

        serializedObject.ApplyModifiedProperties();


        /*    for (int i = 0; i < numFaces+1; i++)
        {
            for (int j = 0; j < numFaces+1; j++)
            {
                if (i == 0)
                {
                    if (j > 0)
                    {
                        EditorGUI.LabelField();
                    }
                }
                else
                {
                    if (j == 0)
                    {
                    }
                    else
                    {
                        TileFace faceI = selectedPalette.tileFaces[i - 1];
                    }
                }
            }
        }*/

    }
}