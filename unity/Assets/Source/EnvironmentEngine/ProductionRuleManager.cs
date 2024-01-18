using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;
using NUnit;

// Class structure is fine, but start from scratch
// ProductionRuleComponent that attaches to the MetatileEnvironment
// Deals with any sort of storing, loading, updating production rules
// ProductionRuleObjects interface into ProductionRuleComponent, and you can contain all the logic in there

// Three folders: objects, actions, conditions
// Various logic for these different things would be in there
// Doesn't need to be prefab, can just be an enum in the code
// Just one folder for ProductionRuleObjects
// Create object in game, MakeProductionRuleObject, pass in shape and color, makes correct shape and color, then meshes for shapes of interest

// ProductionRuleObject overrides EnvironmentComponent OnCollision, the parameter will be the other object collided with.
// If you're a productionRuleObject, do you match my identifier? If so, then if we have a production rule for our collision, then do X.

public enum ACTION
{
    SPAWN,
    REMOVE,
    REWARD,
    PRINT
}

public enum CONDITION
{
    NEAR,
    CONTACT,
    USE,
    DROP,
    PICKUP,
    HOLD,
    SEE
}

// replace with Vector3s
// public class Point
// {
//     public int X { get; set; }
//     public int Y { get; set; }

//     public Point(int x, int y)
//     {
//         X = x;
//         Y = y;
//     }
// }

// Every monobehaviour in separate scripts

public class ProductionRuleManager : EnvironmentComponent
{
    // A list to store all production rules
    private List<ProductionRule> productionRules = new List<ProductionRule>();

    private List<ProductionRuleObject> allProdRuleObjects = new List<ProductionRuleObject>();
    public float NEAR_DISTANCE = 1.0f;

    [Serializable]
    public class ProductionRulePrefab
    {
        public string name;
        public GameObject prefab;

    }

    public List<ProductionRulePrefab> productionRulePrefabs = new List<ProductionRulePrefab>();

    [Serializable]
    public class DefaultProductionRules
    {
        // pairs of condition and action
        public List<ProductionRuleCondition> conditions;
        public List<ProductionRuleAction> actions;
    }

    public List<DefaultProductionRules> defaultProductionRules = new List<DefaultProductionRules>();

    void Awake()
    {
        // TODO: Load production rules
        // otherwise create dummy rules
        // Here's the code:
        // if (productionRules.Count == 0)
        // ProductionRule rule = new ProductionRule(condition, action);
        // AddRule(rule);

        LoadDefaultProductionRules();
    }

    // Method to add a rule to the list
    public void AddRule(ProductionRule rule)
    {
        productionRules.Add(rule);
    }

    // Method to remove a rule from the list
    public void RemoveRule(ProductionRule rule)
    {
        productionRules.Remove(rule);
    }

    public void AddProdRuleObject(ProductionRuleObject obj)
    {
        allProdRuleObjects.Add(obj);
    }

    public void RemoveProdRuleObject(ProductionRuleObject obj)
    {
        allProdRuleObjects.Remove(obj);
    }

    // Update is called once per frame
    void Update()
    {
        CheckAndExecuteRules(GetEngine());
    }

    void LoadDefaultProductionRules()
    {
        for (int i = 0; i < defaultProductionRules.Count; i++)
        {
            ProductionRule rule = new ProductionRule(defaultProductionRules[i].conditions, defaultProductionRules[i].actions);
            AddRule(rule);
        }
    }

    public float GetNearDistance()
    {
        return NEAR_DISTANCE;
    }
    private void CheckAndExecuteRules(EnvironmentEngine env)
    {
        foreach (ProductionRule rule in productionRules)
        {
            foreach (ProductionRuleObject subject in allProdRuleObjects)
            {
                foreach (ProductionRuleObject obj in allProdRuleObjects)
                {
                    if (subject != obj && rule.CheckRule(subject, obj, env))
                    {
                        rule.ExecuteRule(subject, obj, env);
                    }
                }
            }
        }
    }

    public GameObject getPrefabFromName(string name)
    {
        for (int i = 0; i < productionRulePrefabs.Count; i++)
        {
            if (productionRulePrefabs[i].name == name)
            {
                return productionRulePrefabs[i].prefab;
            }
        }

        return null;
    }
}
// Change to ProductionRuleComponent
// MetatileEnv done -> ProductionRuleEnv generates objects -> Backend scripts to create ProductionRuleObjects, call Initalize with color/shape and register callbacks
// Define ProductionRuleObject Prefab (empty game object with ProductionRuleObject script attached), then call Initialize on it
// Management of the ProductionRuleObjects will be done by the ProductionRuleEnvironment
// Singleton pattern: only one ProductionRuleEnvironment in the scene, always interface with that one
// Similar logic to python script, conditions and actions, fancy if statements
// Need to talk about how to actually implement actions and conditions
// Simplest way: every frame, check the state of the environment. If it matches a condition, then do the action.
// These below don't change very much, extend from monobehaviour (or not) to make sure they're accessible from Unity
