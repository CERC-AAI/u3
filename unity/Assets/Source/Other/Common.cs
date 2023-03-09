using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Common
{
    static private Dictionary<string, object> mClientResources = new Dictionary<string, object>();

    static public object getJSONValue(JSONObject jsonObject, string name, Type fieldType)
    {
        if (jsonObject.HasField(name))
        {
            JSONObject field = jsonObject.GetField(name);

            if (fieldType == typeof(string))
            {
                return field.str;
            }
            else if (fieldType == typeof(long) || fieldType == typeof(int) || fieldType == typeof(float))
            {
                return field.n;
            }
            else if (fieldType == typeof(bool))
            {
                return field.b;
            }
            else if (fieldType == typeof(Vector3))
            {
                if (field.HasField("x") && field.HasField("y") && field.HasField("z"))
                {
                    return new Vector3(field.GetField("x").n, field.GetField("y").n, field.GetField("z").n);
                }
                else
                {
                    Debug.LogError("Could not load Vector3 (" + name + ") of type: " + fieldType.Name);
                }
            }
            else if (fieldType == typeof(Vector2))
            {
                if (field.HasField("x") && field.HasField("y"))
                {
                    return new Vector3(field.GetField("x").n, field.GetField("y").n);
                }
                else
                {
                    Debug.LogError("Could not load Vector2 (" + name + ") of type: " + fieldType.Name);
                }
            }
            else if (fieldType == typeof(Vector2Int))
            {
                if (field.HasField("x") && field.HasField("y"))
                {
                    return new Vector3(field.GetField("x").i, field.GetField("y").i);
                }
                else
                {
                    Debug.LogError("Could not load Vector2Int (" + name + ") of type: " + fieldType.Name);
                }
            }
            else if (fieldType == typeof(Vector3Int))
            {
                if (field.HasField("x") && field.HasField("y") && field.HasField("z"))
                {
                    return new Vector3(field.GetField("x").i, field.GetField("y").i, field.GetField("z").i);
                }
                else
                {
                    Debug.LogError("Could not load Vector3Int (" + name + ") of type: " + fieldType.Name);
                }
            }
            /*else if (fieldType == typeof(Transform))
            {
                if (field.HasField("x") && field.HasField("y") && field.HasField("z") &&
                    field.HasField("rot_x") && field.HasField("rot_y") && field.HasField("rot_z") &&
                    field.HasField("scl_x") && field.HasField("scl_y") && field.HasField("scl_z"))
                {
                    return new Vector3(field.GetField("x").n, field.GetField("y").n, field.GetField("z").n);
                }
                else
                {
                    Debug.LogError("Could not load Vector3 (" + name + ") of type: " + fieldType.Name);
                }
            }*/
            else
            {
                Debug.LogError("Could not load Property (" + name + ") of type: " + fieldType.Name);

                return false;
            }
        }

        Debug.LogError("Property (" + name + ") of type: " + fieldType.Name + " not found in JSON");

        return null;
    }

    static public bool saveJSONValue(JSONObject jsonObject, string name, object value, Type fieldType, string defaultValue = null, string className = "")
    {
        if (className != "")
        {
            className += "." + name;
        }
        else
        {
            className = name;
        }

        if (fieldType == typeof(string))
        {
            if (defaultValue == null || (string)value != defaultValue)
            {
                jsonObject.AddField(name, (string)value);
            }
        }
        else if (fieldType == typeof(long))
        {
            if (defaultValue == null || ((long)value ).ToString() != defaultValue)
            {
                jsonObject.AddField(name, (long)value);
            }
        }
        else if (fieldType == typeof(int))
        {
            if (defaultValue == null || ((int)value).ToString() != defaultValue)
            {
                jsonObject.AddField(name, (int)value);
            }
        }
        else if (fieldType == typeof(bool))
        {
            if (defaultValue == null || ((bool)value).ToString() != defaultValue)
            {
                jsonObject.AddField(name, (bool)value);
            }
        }
        else if (fieldType == typeof(float))
        {
            if (defaultValue == null || ((float)value).ToString() != defaultValue)
            {
                jsonObject.AddField(name, (float)value);
            }
        }
        else if (fieldType == typeof(double))
        {
            if (defaultValue == null || ((double)value).ToString() != defaultValue)
            {
                jsonObject.AddField(name, ((double)value).ToString());
            }
        }
        else if (fieldType == typeof(Vector3))
        {
            JSONObject vectorObject = new JSONObject();

            vectorObject.AddField("x", ((Vector3)value).x);
            vectorObject.AddField("y", ((Vector3)value).y);
            vectorObject.AddField("z", ((Vector3)value).z);

            jsonObject.AddField(name, vectorObject);
        }
        else if (fieldType == typeof(Vector2))
        {
            JSONObject vectorObject = new JSONObject();

            vectorObject.AddField("x", ((Vector3)value).x);
            vectorObject.AddField("y", ((Vector3)value).y);

            jsonObject.AddField(name, vectorObject);
        }
        else if (fieldType == typeof(Vector2Int))
        {
            JSONObject vectorObject = new JSONObject();

            vectorObject.AddField("x", ((Vector2Int)value).x);
            vectorObject.AddField("y", ((Vector2Int)value).y);

            jsonObject.AddField(name, vectorObject);
        }
        else if (fieldType == typeof(Vector3Int))
        {
            JSONObject vectorObject = new JSONObject();

            vectorObject.AddField("x", ((Vector3Int)value).x);
            vectorObject.AddField("y", ((Vector3Int)value).y);
            vectorObject.AddField("z", ((Vector3Int)value).z);

            jsonObject.AddField(name, vectorObject);
        }
        else if (fieldType == typeof(Transform))
        {
            if (((Transform)value) == null)
            {
                jsonObject.AddField(name, "null");
            }
            else
            {
                JSONObject transformObject = new JSONObject();

                transformObject.AddField("x", ((Transform)value).localPosition.x);
                transformObject.AddField("y", ((Transform)value).localPosition.y);
                transformObject.AddField("z", ((Transform)value).localPosition.z);

                transformObject.AddField("rot_x", ((Transform)value).localRotation.eulerAngles.x);
                transformObject.AddField("rot_y", ((Transform)value).localRotation.eulerAngles.y);
                transformObject.AddField("rot_z", ((Transform)value).localRotation.eulerAngles.z);

                jsonObject.AddField(name, transformObject);
            }

            
        }
        else if (fieldType == typeof(EnvironmentComponent) || fieldType.IsSubclassOf(typeof(EnvironmentComponent)))
        {
            if (((EnvironmentComponent)value) is null)
            {
                if (defaultValue == null || (defaultValue != "" && defaultValue != "null" && defaultValue != "0"))
                {
                    jsonObject.AddField(name, 0);
                }
            }
            else
            {
                if (defaultValue == null || (defaultValue != ((EnvironmentComponent)value).GetObjectID().ToString()))
                {
                    jsonObject.AddField(name, ((EnvironmentComponent)value).GetObjectID());
                }
            }
        }
        else if (fieldType.IsGenericType && typeof(IDictionary).IsAssignableFrom(fieldType))
        {
            JSONObject dictObject = new JSONObject();

            Type keyType = fieldType.GetGenericArguments()[0];
            Type valueType = fieldType.GetGenericArguments()[1];

            if (keyType == typeof(string))
            {
                List<string> keys = new List<string>();
                foreach (var dictKey in ((IDictionary)value).Keys)
                {
                    keys.Add((string)dictKey);
                }
                int i = 0;
                foreach (var dictValue in ((IDictionary)value).Values)
                {
                    saveJSONValue(dictObject, keys[i], dictValue, valueType, null, className);

                    i++;
                }
            }
            else
            {
                int i = 0;
                foreach (var dictKey in ((IDictionary)value).Keys)
                {
                    saveJSONValue(dictObject, i + "_k", dictKey, keyType, null, className);

                    i++;
                }
                i = 0;
                foreach (var dictValue in ((IDictionary)value).Values)
                {
                    saveJSONValue(dictObject, i + "_v", dictValue, valueType, null, className);

                    i++;
                }
            }

            jsonObject.AddField(name, dictObject);
        }
        else if (fieldType.IsGenericType && typeof(IList).IsAssignableFrom(fieldType))
        {
            JSONObject listObject = new JSONObject();

            Type listType = fieldType.GetGenericArguments()[0];

            for (int i = 0; i < ((IList)value).Count; i++)
            {
                saveJSONValue(listObject, i.ToString(), ((IList)value)[i], listType, null, className);
            }

            jsonObject.AddField(name, listObject);
        }
        else if (fieldType.IsArray)
        {
            JSONObject arrayObject = new JSONObject();

            Type arrayType = fieldType.GetElementType();
            int rank = fieldType.GetArrayRank();

            if (rank == 1)
            {
                for (int i = 0; i < ((Array)value).Length; i++)
                {
                    saveJSONValue(arrayObject, i.ToString(), ((Array)value).GetValue(i), arrayType, defaultValue, className);
                }
            }
            else if (rank == 2)
            {
                for (int i = 0; i < ((Array)value).GetLength(0); i++)
                {
                    for (int j = 0; j < ((Array)value).GetLength(1); j++)
                    {
                        saveJSONValue(arrayObject, i.ToString() + "," + j.ToString(), ((Array)value).GetValue(i, j), arrayType, defaultValue, className);
                    }
                }
            }
            else if (rank == 3)
            {
                for (int i = 0; i < ((Array)value).GetLength(0); i++)
                {
                    for (int j = 0; j < ((Array)value).GetLength(1); j++)
                    {
                        for (int k = 0; k < ((Array)value).GetLength(2); k++)
                        {
                            saveJSONValue(arrayObject, i.ToString() + "," + j.ToString() + "," + k.ToString(), ((Array)value).GetValue(i, j, k), arrayType, defaultValue, className);
                        }
                    }
                }
            }
            else if (rank == 4)
            {
                for (int i = 0; i < ((Array)value).GetLength(0); i++)
                {
                    for (int j = 0; j < ((Array)value).GetLength(1); j++)
                    {
                        for (int k = 0; k < ((Array)value).GetLength(2); k++)
                        {
                            for (int l = 0; l < ((Array)value).GetLength(3); l++)
                            {
                                saveJSONValue(arrayObject, i.ToString() + "," + j.ToString() + "," + k.ToString() + "," + l.ToString(), ((Array)value).GetValue(new int[]{i, j, k, l}), arrayType, defaultValue, className);
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Could not serialize field (" + className + ") of type: " + fieldType.Name + ". Multidimensional array are only supported up to 4 dimensions. Add custom logic to OnPreSave and OnPostLoad, or add the [NotSaved] attribute to surpress this message.");
            }

            jsonObject.AddField(name, arrayObject);
        }
        else
        {
            Debug.LogError("Could not serialize field (" + className + ") of type: " + fieldType.Name + ". Only base types and classes inherited from EnvironmentComponent can be serialized. Add custom logic to OnPreSave and OnPostLoad, or add the [NotSaved] attribute to surpress this message.");

            return false;
        }

        return true;
    }

    static public string getString(JSONObject jsonObject, string name)
    {
        if (jsonObject.HasField(name))
        {
            JSONObject feild = jsonObject.GetField(name);

            if (feild.IsString)
            {
                return feild.str;
            }
        }

        return "";
    }

    static public bool getBoolean(JSONObject jsonObject, string name)
    {
        if (jsonObject.HasField(name))
        {
            JSONObject feild = jsonObject.GetField(name);

            if (feild.IsBool)
            {
                return feild.b;
            }
        }

        return false;
    }

    static public double getNumber(JSONObject jsonObject, string name)
    {
        if (jsonObject.HasField(name))
        {
            JSONObject feild = jsonObject.GetField(name);

            if (feild.IsNumber)
            {
                return feild.n;
            }
        }

        return -1;
    }

    static public List<JSONObject> getArray(JSONObject jsonObject, string name)
    {
        if (jsonObject.HasField(name))
        {
            JSONObject feild = jsonObject.GetField(name);

            if (feild.IsArray || feild.IsObject)
            {
                return feild.list;
            }
        }

        return null;
    }

    static public Transform loadResource(string path, bool silent = false)
    {
        UnityEngine.Object tempObject;

        if (mClientResources.ContainsKey(path))
        {
            tempObject = (GameObject)mClientResources[path];
        }
        else
        {
            tempObject = (GameObject)Resources.Load(path);

            if (tempObject != null)
            {
                mClientResources[path] = tempObject;
            }
            else
            {
                if (!silent)
                {
                    UnityEngine.Debug.LogError("Could not load resource: " + path);
                }
                return null;
            }
        }

        //SQDebug.log2("Instanticate: " + tempObject.name);
        return ((GameObject)GameObject.Instantiate(tempObject)).transform;
    }
}
