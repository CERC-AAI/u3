using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEditor.PackageManager;
using UnityEditor;
using UnityEngine;

public class EtraStarterAssetSetup : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        var inPackages = importedAssets.Any(path => path.StartsWith("Packages/")) ||
            deletedAssets.Any(path => path.StartsWith("Packages/")) ||
            movedAssets.Any(path => path.StartsWith("Packages/")) ||
            movedFromAssetPaths.Any(path => path.StartsWith("Packages/"));

        if (inPackages)
        {
            InitializeOnLoad();
        }
    }

    [InitializeOnLoadMethod]
    private static void InitializeOnLoad()
    {

        addLayer("EtraFPSUsableItem");

    }



    public static void addTag(string tagName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        string s = tagName;

        bool found = false;
        int amountOfTags = 0;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            amountOfTags++;
            if (t.stringValue.Equals(s)) { found = true; break; }
        }


        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(amountOfTags);
            SerializedProperty n = tagsProp.GetArrayElementAtIndex(amountOfTags);
            n.stringValue = s;
            Debug.Log("Tag:" + tagName + " added by EtraStarterAssetSetup.cs");
        }

        tagManager.ApplyModifiedProperties();
        
    }


    public static void addLayer(string layerName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = tagManager.FindProperty("layers");

        string s = layerName;

        // First check if it is not already present
        bool found = false;
        int amountOfLayers = 0;
        for (int i = 0; i < layersProp.arraySize; i++)
        {
            SerializedProperty t = layersProp.GetArrayElementAtIndex(i);
            if (!t.stringValue.Equals(""))
            {
                amountOfLayers++;
            }

            if (t.stringValue.Equals(s)) { found = true; break; }
        }


        if (amountOfLayers >= 32)
        {
            Debug.LogError("Cannot add additional layer " + layerName + " because max layers are reached. " +
                "Etra's Starter assets needs the additional layer " + layerName + " in order for some features to function.");
            return;
        }
        // if not found, add it
        if (!found)
        {
            bool openLayerFound = false;

            while (openLayerFound == false)
            {
                SerializedProperty t = layersProp.GetArrayElementAtIndex(amountOfLayers);

                if (!t.stringValue.Equals(""))
                {
                    amountOfLayers--;
                }
                else
                {
                    openLayerFound = true;
                }

            }

            layersProp.InsertArrayElementAtIndex(amountOfLayers);
            SerializedProperty n = layersProp.GetArrayElementAtIndex(amountOfLayers);
            n.stringValue = s;
            Debug.Log("Layer:" + layerName + " added by EtraStarterAssetSetup.cs");
        }


        tagManager.ApplyModifiedProperties();
        

    }

}
