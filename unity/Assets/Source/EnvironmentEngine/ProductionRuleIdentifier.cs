using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;

[Serializable]
public class ProductionRuleIdentifier
{
    public Dictionary<string, Tuple<float, float, float>> colorDict = new Dictionary<string, Tuple<float, float, float>>()
    {
        {"red", new Tuple<float, float, float>(1.0f, 0.0f, 0.0f)},
        {"green", new Tuple<float, float, float>(0.0f, 1.0f, 0.0f)},
        {"blue", new Tuple<float, float, float>(0.0f, 0.0f, 1.0f)},
        {"purple", new Tuple<float, float, float>(0.0f, 0.0f, 1.0f)},
        {"yellow", new Tuple<float, float, float>(1.0f, 1.0f, 0.0f)},
        {"grey", new Tuple<float, float, float>(0.5f, 0.5f, 0.5f)},
        {"white", new Tuple<float, float, float>(1.0f, 1.0f, 1.0f)},
        {"orange", new Tuple<float, float, float>(1.0f, 0.5f, 0.0f)},
        {"brown", new Tuple<float, float, float>(0.5f, 0.25f, 0.0f)},
        {"pink", new Tuple<float, float, float>(1.0f, 0.6f, 0.6f)},
        {"black", new Tuple<float, float, float>(0.0f, 0.0f, 0.0f)},
        {"cyan", new Tuple<float, float, float>(0.0f, 1.0f, 1.0f)},
        {"dark_green", new Tuple<float, float, float>(0.0f, 0.5f, 0.0f)},
        {"dark_grey", new Tuple<float, float, float>(0.25f, 0.25f, 0.25f)},
        {"dark_brown", new Tuple<float, float, float>(0.25f, 0.125f, 0.0f)},
        {"dark_purple", new Tuple<float, float, float>(0.5f, 0.0f, 0.5f)},
        {"dark_red", new Tuple<float, float, float>(0.5f, 0.0f, 0.0f)},
        {"dark_blue", new Tuple<float, float, float>(0.0f, 0.0f, 0.5f)},
        {"dark_yellow", new Tuple<float, float, float>(0.5f, 0.5f, 0.0f)},
        {"dark_orange", new Tuple<float, float, float>(0.5f, 0.25f, 0.0f)},
        {"dark_pink", new Tuple<float, float, float>(0.5f, 0.3f, 0.3f)},
        {"dark_cyan", new Tuple<float, float, float>(0.0f, 0.5f, 0.5f)}
    };

    // Create a dictionary of shapes and their corresponding prefabs
    public Dictionary<string, GameObject> shapeDict = new Dictionary<string, GameObject>()
    {
        {"sphere", Resources.Load<GameObject>("Prefabs/Sphere")},
        {"cube", Resources.Load<GameObject>("Prefabs/Cube")},
        {"cylinder", Resources.Load<GameObject>("Prefabs/Cylinder")},
        {"agent", Resources.Load<GameObject>("Prefabs/Agent")}
    };

    public string ObjectShape;
    public string ObjectColor;

    public ProductionRuleIdentifier(string shape, string color = null)
    {
        if (!shapeDict.ContainsKey(shape))
        {
            throw new ArgumentException($"Invalid shape: {shape}");
        }

        if (color != null && !colorDict.ContainsKey(color))
        {
            throw new ArgumentException($"Invalid color: {color}");
        }

        ObjectShape = shape;
        ObjectColor = color;
    }

    public string Encode()
    {
        if (ObjectShape == "agent")
        {
            return "agent";
        }

        return ObjectColor != null ? $"a {ObjectColor} {ObjectShape}" : $"a {ObjectShape}";
    }

    // public bool MatchesObject(ProductionRuleObject obj)
    // {
    //     // Example implementation, assuming obj has methods or properties to get its shape and color
    //     bool shapeMatches = string.Equals(ObjectShape, obj.GetShape(), StringComparison.OrdinalIgnoreCase);
    //     bool colorMatches = (ObjectColor == null) || string.Equals(ObjectColor, obj.GetColor(), StringComparison.OrdinalIgnoreCase);

    //     return shapeMatches && colorMatches;
    // }

}
