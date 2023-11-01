using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileFacePalette))]
public class TileFacePaletteEditor : Editor
{
    static bool isOpen = true;

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
                    selectedPalette.matchingMatrix.Add(new TileFacePalette.MatchedFaces(i, i, 1)); 
                }
            }
        }

        if (isOpen)
        {

            int numFaces = selectedPalette.tileFaces.Count;

            float checkboxSizeXMax = 60;
            float checkboxSizeY = 20;
            float colorSize = 20;
            float labelSize = 0;
            float indent = 2;
            float topIndent = colorSize;
            int margin = 1;

            string environmentString = "Placed faces";
            string incomingString = "Incoming faces";

            GUIStyle boldStyle = new GUIStyle(GUI.skin.label);
            boldStyle.fontStyle = FontStyle.Bold;

            Vector2 textDimensions = boldStyle.CalcSize(new GUIContent(environmentString));
            if (labelSize < textDimensions.x)
            {
                labelSize = textDimensions.x;
            }
            /*textDimensions = GUI.skin.label.CalcSize(new GUIContent(incomingString));
            if (labelSize < textDimensions.x)
            {
                labelSize = textDimensions.x;
            }*/
            for (int i = 0; i < numFaces; i++)
            {
                textDimensions = GUI.skin.label.CalcSize(new GUIContent(selectedPalette.tileFaces[i].name)); 
                if (labelSize < textDimensions.x)
                {
                    labelSize = textDimensions.x;
                }
            }
            labelSize += colorSize;

            float checkboxSizeX = Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - (labelSize + indent + margin * 2 + 20)) / numFaces - margin);
            checkboxSizeX = Mathf.Min(checkboxSizeX, checkboxSizeXMax);



            GUILayout.BeginScrollView(new Vector2(0, 0), GUILayout.Height(checkboxSizeY * numFaces + labelSize + indent + topIndent));

            Rect blankRect = new Rect(0, topIndent, labelSize + indent, labelSize);
            Rect topLabelRect = new Rect(labelSize + indent, topIndent, checkboxSizeX * numFaces, labelSize);
            Rect leftLabelRect = new Rect(0, labelSize + topIndent, indent + labelSize, checkboxSizeY * numFaces);
            Rect checkBoxRect = new Rect(indent + labelSize, labelSize + topIndent, checkboxSizeX * numFaces, checkboxSizeY * numFaces);

            /*EditorGUI.DrawRect(blankRect, Color.red);
            EditorGUI.DrawRect(topLabelRect, Color.blue);
            EditorGUI.DrawRect(leftLabelRect, Color.green);
            EditorGUI.DrawRect(checkBoxRect, Color.black);*/

            textDimensions = boldStyle.CalcSize(new GUIContent(incomingString));
            Rect labelRect = new Rect(labelSize + indent + (EditorGUIUtility.currentViewWidth - (labelSize + indent + margin * 2 + 20)) / 2 - textDimensions.x / 2, 0, labelSize, checkboxSizeY);
            EditorGUI.LabelField(labelRect, incomingString, boldStyle);

            for (int i = 0; i < numFaces; i++)
            {
                Vector2 root = new Vector3(labelSize + indent + (checkboxSizeX - colorSize) / 2 + (checkboxSizeX * i), topIndent, 0);

                EditorGUIUtility.RotateAroundPivot(-90f, root);
                labelRect = new Rect(indent + (checkboxSizeX - colorSize) / 2 + (checkboxSizeX * i) + margin + colorSize, topIndent, labelSize, checkboxSizeY);
                EditorGUI.LabelField(labelRect, selectedPalette.tileFaces[i].name);
                EditorGUIUtility.RotateAroundPivot(90f, root);
            }

            labelRect = new Rect(indent, topIndent + labelSize - colorSize, labelSize, checkboxSizeY);
            EditorGUI.LabelField(labelRect, environmentString, boldStyle);

            for (int i = 0; i < numFaces; i++)
            {
                labelRect = new Rect(indent, topIndent + labelSize + checkboxSizeY * i, labelSize, checkboxSizeY);
                EditorGUI.LabelField(labelRect, selectedPalette.tileFaces[i].name);

                Rect colorRectColumn = new Rect(indent + labelSize - colorSize, topIndent + labelSize + checkboxSizeY * i, colorSize - margin, colorSize - margin);
                EditorGUI.DrawRect(colorRectColumn, selectedPalette.tileFaces[i].color);

                Rect colorRectRow = new Rect(labelSize + indent + (checkboxSizeX - colorSize) / 2 + (checkboxSizeX * i), topIndent + labelSize - colorSize, colorSize - margin, colorSize - margin);
                EditorGUI.DrawRect(colorRectRow, selectedPalette.tileFaces[i].color);
            }



            List<TileFacePalette.MatchedFaces> matchingMatrixData = new List<TileFacePalette.MatchedFaces>();
            for (int i = 0; i < selectedPalette.matchingMatrix.Count; i++)
            {
                matchingMatrixData.Add(new TileFacePalette.MatchedFaces(selectedPalette.matchingMatrix[i].mA, selectedPalette.matchingMatrix[i].mB, selectedPalette.matchingMatrix[i].mWeight));
            }

            selectedPalette.matchingMatrix.Clear();

            float[][] tempData = new float[numFaces][];
            for (int i = 0; i < numFaces; i++)
            {
                tempData[i] = new float[numFaces];
            }

            for (int i = 0; i < matchingMatrixData.Count; i++)
            {
                if (matchingMatrixData[i].mA < numFaces && matchingMatrixData[i].mB < numFaces)
                {
                    tempData[matchingMatrixData[i].mA][matchingMatrixData[i].mB] = matchingMatrixData[i].mWeight;
                }
            }

            Color oldColor = GUI.contentColor;


            int index = 0;
            for (int i = 0; i < numFaces; i++)
            {
                for (int j = 0; j < numFaces; j++)
                {
                    //if (i <= j)
                    {
                        float oldValue = tempData[i][j];
                        if (oldValue == 0)
                        {
                            GUI.contentColor = Color.grey;
                        }
                        else
                        {
                            if (oldValue > 1)
                            {
                                GUI.contentColor = Color.green;
                            }
                            else if (oldValue < 1)
                            {
                                GUI.contentColor = Color.red;
                            }
                            else
                            {
                                GUI.contentColor = Color.white;
                            }
                        }

                        Rect checkboxRect = new Rect(labelSize + indent + checkboxSizeX * i + margin + 2, topIndent + labelSize + checkboxSizeY * j, checkboxSizeX - margin, checkboxSizeY - margin);
                        float newValue = EditorGUI.FloatField(checkboxRect, tempData[i][j]);

                        if (newValue != 0)
                        {
                            if (oldValue == 0 && tempData[j][i] == 0)
                            {
                                selectedPalette.matchingMatrix.Add(new TileFacePalette.MatchedFaces(j, i, newValue));
                            }
                            selectedPalette.matchingMatrix.Add(new TileFacePalette.MatchedFaces(i, j, newValue));

                            index++;
                        }

                        if (oldValue != newValue)
                        {
                            EditorUtility.SetDirty(selectedPalette);
                        }
                    }
                }
            }

            GUI.contentColor = oldColor;

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