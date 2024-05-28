using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using UnityEngine.UIElements;
// using NUnit;

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

public enum Action
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
    THROW,
    HOLD,
    SEE,
    NONE
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
    public List<ProductionRule> productionRules = new List<ProductionRule>();

    private List<ProductionRule> permissibleProductionRules = new List<ProductionRule>();

    private List<ProductionRuleObject> allProdRuleObjects = new List<ProductionRuleObject>();

    public GameObject productionRuleObjectPrefab;

    public float NEAR_DISTANCE = 2.0f;

    private GravityGun mGravityGun;

    public ProductionRuleGraph productionRuleGraph;

    TrialManager mTrialManager;

    [Serializable]
    public class ProductionRulePrefab
    {
        public string name;
        public GameObject prefab;

    }

    [Serializable]
    public class DefaultProductionRules
    {
        // pairs of condition and action
        public List<ProductionRuleCondition> conditions;
        public List<ProductionRuleAction> actions;
    }

    public List<ProductionRulePrefab> productionRulePrefabs = new List<ProductionRulePrefab>();
    public List<DefaultProductionRules> defaultProductionRules = new List<DefaultProductionRules>();

    void Awake()
    {
        LoadDefaultProductionRules();
    }

    public override void OnRunStarted()
    {
        base.OnRunStarted();

        Debug.Log("ProductionRuleManager");

        // New ProductionRuleGraph
        List<ProductionRuleIdentifier> initialState = productionRuleGraph.GetRandomInitialState();
        productionRules = productionRuleGraph.BuildProductionRuleSet(initialState);

        SpawnManager spawnManager = GetEngine().GetCachedEnvironmentComponent<SpawnManager>();

        foreach (ProductionRuleIdentifier ruleIdentifier in initialState)
        {
            spawnManager.AddProductionRuleObjectToSpawn(ruleIdentifier);
        }
    }

    public override void InitParameters(JSONObject jsonParameters)
    {
        if (jsonParameters)
        {
            jsonParameters.GetField(out NEAR_DISTANCE, "prod_rule_near_distance", NEAR_DISTANCE);
        }

        base.InitParameters(jsonParameters);
    }

    protected override void DoRegisterCallbacks()
    {
        mTrialManager = GetEngine().GetEnvironmentComponent<TrialManager>();
        if (mTrialManager)
        {
            RegisterCallback(ref mTrialManager.OnTrialOverCallbacks, OnTrialOver);
            RegisterCallback(ref mTrialManager.OnTrialStartCallbacks, OnTrialStart);
        }

        if (GetGravityGun() != null)
        {
            RegisterCallback(ref GetGravityGun().OnGravityGunDrop, OnGravityGunDrop);
            RegisterCallback(ref GetGravityGun().OnGravityGunThrow, OnGravityGunThrow);
            RegisterCallback(ref GetGravityGun().OnGravityGunPickup, OnGravityGunPickup);
        }

        base.DoRegisterCallbacks();
    }

    public override void OnUpdate(float deltaTime)
    {
        CheckAndExecuteRules(GetEngine());

        base.OnUpdate(deltaTime);
    }

    void OnTrialOver()
    {
        int numProdRuleObjects = allProdRuleObjects.Count;
        for (int i = 0; i < numProdRuleObjects; i++)
        {
            ProductionRuleObject productionRuleObject = allProdRuleObjects[0];
            productionRuleObject.Remove();
        }
    }

    void OnTrialStart()
    {
    }

    public override void OnRunEnded()
    {
        int numProdRuleObjects = allProdRuleObjects.Count;
        for (int i = 0; i < numProdRuleObjects; i++)
        {
            ProductionRuleObject productionRuleObject = allProdRuleObjects[0];
            productionRuleObject.Remove();
        }

        base.OnRunEnded();
    }

    public void AddRule(ProductionRule rule)
    {
        productionRules.Add(rule);
    }

    public void RemoveRule(ProductionRule rule)
    {
        productionRules.Remove(rule);
    }

    public void AddProdRuleObject(ProductionRuleObject obj)
    {
        allProdRuleObjects.Add(obj);
    }

    public ProductionRuleObject GetProductionRuleObjectByIdentifier(ProductionRuleIdentifier identifier)
    {
        foreach (ProductionRuleObject obj in allProdRuleObjects)
        {
            if (obj.GetIdentifier().CompareTo(identifier))
            {
                return obj;
            }
        }
        return null;
    }

    public void RemoveProdRuleObject(ProductionRuleObject obj)
    {
        allProdRuleObjects.Remove(obj);
    }

    // Update is called once per frame

    void LoadDefaultProductionRules()
    {
        for (int i = 0; i < defaultProductionRules.Count; i++)
        {
            ProductionRule rule = new ProductionRule(defaultProductionRules[i].conditions, defaultProductionRules[i].actions);
            AddRule(rule);
        }

        // ProductionRule newProductionRule = SampleNewForwardRule(allProdRuleObjects);
        // AddRule(newProductionRule);
    }

    public float GetNearDistance()
    {
        return NEAR_DISTANCE;
    }

    public void CheckCallback(CONDITION condition, ProductionRuleObject sub, ProductionRuleObject obj, EnvironmentEngine env)
    {
        foreach (ProductionRule rule in productionRules)
        {
            if (rule.CheckCallback(condition, sub, obj, env))
            {
                rule.ExecuteRule(sub, obj, env);
            }
        }
    }

    private void CheckAndExecuteRules(EnvironmentEngine env) //TODO: remove coupling with updating ProductionRuleGraph in ExecuteRule. Split into check and execute
    {
        // Shuffle the production rules
        List<ProductionRule> currentRules = new List<ProductionRule>(productionRules);
        ShuffleUtility.ShuffleList(currentRules);
        foreach (ProductionRule rule in currentRules) // TODO: foreach error with collection is correct because you're modifying the underlying collection: figure this out. Snapshot?
        {
            List<ProductionRuleObject> currentObjects = new List<ProductionRuleObject>(allProdRuleObjects);

            foreach (ProductionRuleObject subject in currentObjects)
            {
                foreach (ProductionRuleObject obj in currentObjects)
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

    public GravityGun GetGravityGun()
    {
        if (mGravityGun == null)
        {
            mGravityGun = (GravityGun)GetEngine().GetEnvironmentComponent<GravityGun>();
        }
        return mGravityGun;
        //return ProductionRuleManager.productionRuleManager;
    }

    public void OnGravityGunDrop(Rigidbody obj)
    {
        ProductionRuleObject productionRuleObject = obj.GetComponentInParent<ProductionRuleObject>();
        CheckCallback(CONDITION.DROP, productionRuleObject, null, GetEngine());
        Debug.Log($"Object dropped {productionRuleObject}");
    }

    public void OnGravityGunPickup(Rigidbody obj)
    {
        ProductionRuleObject productionRuleObject = obj.GetComponentInParent<ProductionRuleObject>();
        CheckCallback(CONDITION.PICKUP, productionRuleObject, null, GetEngine());
        Debug.Log("Object picked up");
    }

    public void OnGravityGunThrow(Rigidbody obj)
    {
        ProductionRuleObject productionRuleObject = obj.GetComponentInParent<ProductionRuleObject>();
        CheckCallback(CONDITION.THROW, productionRuleObject, null, GetEngine());
        Debug.Log("Object thrown");
    }

    public List<ProductionRuleIdentifier> GetCurrentState()
    {
        List<ProductionRuleIdentifier> currentState = new List<ProductionRuleIdentifier>();
        foreach (ProductionRuleObject obj in allProdRuleObjects)
        {
            currentState.Add(obj.GetIdentifier());
        }
        return currentState;
    }

    public List<ProductionRuleObject> GetAllProdRuleObjects()
    {
        return allProdRuleObjects;
    }
}

public class ShuffleUtility
{
    // Shuffle an array
    public static void ShuffleArray<T>(T[] array)
    {
        System.Random rng = new System.Random();
        int n = array.Length;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = array[k];
            array[k] = array[n];
            array[n] = value;
        }
    }

    // Shuffle a list
    public static void ShuffleList<T>(IList<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}