using UnityEngine;
using UnityEditor;
using StarterAssets;

[CustomEditor(typeof(EtraCharacterMainController))]
public class UnityEditorForEtraCharacterMainController : Editor
{
    public override void OnInspectorGUI()
    {
        //Create Apply Gameplay Settings button under public enum variables
        EtraCharacterMainController character = target as EtraCharacterMainController;
        EditorGUILayout.Space(20);
        Rect lastRect = GUILayoutUtility.GetLastRect();
        Rect buttonRect = new Rect(lastRect.x, lastRect.y + 90 , lastRect.width, EditorGUIUtility.singleLineHeight);
        if (GUI.Button(buttonRect, "Apply Gameplay Settings"))
        {
            character.applyGameplayChangesInspectorButtonPressed();
        }
        //Then, draw default inspector
        DrawDefaultInspector();
    }
}
