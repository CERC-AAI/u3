using System;
using UnityEditor;
using UnityEngine;
using System.Reflection;

public class EtrasResourceGrabbingFunctions : MonoBehaviour
{

    #region getPrefabFromAssetsByName and Overloads
    public static GameObject getPrefabFromAssetsByName(string prefabName)
    {
        string[] filePaths;
        filePaths = AssetDatabase.FindAssets(prefabName);
        if (filePaths.Length == 0 || filePaths[0] == null)
        {
            Debug.LogError(prefabName + " not found in assets. Please restore the prefab.");
            return null;
        }
        GameObject foundObject = (GameObject)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(filePaths[0]), typeof(GameObject));

        return foundObject;
    }


    #endregion

    #region addPrefabFromAssetsByName and Overloads
    public static GameObject addPrefabFromAssetsByName(string prefabName)
    {
        return addPrefabFromAssetsByName(prefabName, null);
    }


    public static GameObject addPrefabFromAssetsByName(string prefabName, Transform parent)
    {
        return addPrefabFromAssetsByName(prefabName, parent, false);
    }


    public static GameObject addPrefabFromAssetsByName(string prefabName, Transform parent, bool allowDuplicates)
    {
        if (!allowDuplicates)
        {
            if (GameObject.Find(prefabName))
            {
                Debug.LogWarning("There is already a " + prefabName + " in the active scene. Please allow duplicates in your addPrefabFromAssetsByName() function call if you wish to allow duplicates of " +prefabName+ ".");
                return null;
            }
        }


        string[] filePaths;
        filePaths = AssetDatabase.FindAssets(prefabName);
        if (filePaths.Length == 0 || filePaths[0] == null) { 
            Debug.LogError(prefabName + " not found in assets. Please restore the prefab.");
            return null; 
        }
        GameObject addedObject = (GameObject)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(filePaths[0]), typeof(GameObject));
        addedObject = Instantiate(addedObject, Vector3.zero, Quaternion.identity);
        PrefabUtility.InstantiatePrefab(addedObject);

        if (!allowDuplicates)
        {
            addedObject.name = prefabName;
        }

        if (parent !=null)
        {
            addedObject.transform.SetParent(parent);
        }

        return addedObject;
    }

    public static GameObject addPrefabFromAssetsByName(string prefabName, Transform parent, bool allowDuplicates, Vector3 localPos)
    {
        GameObject addedObject = addPrefabFromAssetsByName(prefabName, parent, allowDuplicates);
        if (addedObject == null) { return null; }
        addedObject.transform.localPosition= localPos;
        return addedObject;
    }

    public static GameObject addPrefabFromAssetsByName(string prefabName, Transform parent, bool allowDuplicates, Vector3 localPos, Quaternion localRot)
    {
        GameObject addedObject = addPrefabFromAssetsByName(prefabName, parent, allowDuplicates, localPos);
        if (addedObject == null) { return null; }
        addedObject.transform.localRotation = localRot;
        return addedObject;
    }

    public static GameObject addPrefabFromAssetsByName(string prefabName, Transform parent, bool allowDuplicates, Vector3 localPos, Quaternion localRot, Vector3 localScale)
    {
        GameObject addedObject = addPrefabFromAssetsByName(prefabName, parent, allowDuplicates, localPos, localRot);
        if (addedObject == null) { return null; }
        addedObject.transform.localScale = localScale;
        return addedObject;
    }

    #endregion

    #region GetComponentFromAssetsByName
    public static Type GetComponentFromAssetsByName(string name)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.Name == name)
                    return type;
            }
        }
        Debug.LogError("Component " + name + " not found in any asset folder.");
        return null;
    }

    #endregion


    public static bool TryGetComponentInChildren<T>(Transform parentObject)
    {
        if (parentObject.GetComponentInChildren<T>() != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }



}
