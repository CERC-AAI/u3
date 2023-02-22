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
            else
            {
                Debug.LogError("Could not load Property (" + name + ") of type: " + fieldType.Name);

                return false;
            }
        }

        Debug.LogError("Property (" + name + ") of type: " + fieldType.Name + " not found in JSON");

        return null;
    }

    static public bool saveJSONValue(JSONObject jsonObject, string name, object value, Type fieldType)
    {
        if (fieldType == typeof(string))
        {
            jsonObject.AddField(name, (string)value);
        }
        else if (fieldType == typeof(long))
        {
            jsonObject.AddField(name, (long)value);
        }
        else if (fieldType == typeof(int))
        {
            jsonObject.AddField(name, (int)value);
        }
        else if (fieldType == typeof(bool))
        {
            jsonObject.AddField(name, (bool)value);
        }
        else if (fieldType == typeof(float))
        {
            jsonObject.AddField(name, (float)value);
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
        else
        {
            Debug.LogError("Could not convert Property (" + name + ") of type: " + fieldType.Name);

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
