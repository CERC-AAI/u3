using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;

public enum PredicateObjects
{
    SUBJECT,
    OBJECT,
    BOTH,
    NONE
}

[Serializable]
public class ProductionRuleAction
{
    const int MAX_SPAWNS = 3; 

    public PredicateObjects predicateObjects;

    public ProductionRuleIdentifier identifier;

    public ProductionRuleManager.ACTION action;

    public float floatValue;


    public string debugPrintString = "Test print";

    [HideInInspector]
    int mTotalSpawns = 0;

    public static ProductionRuleAction GetProductionRuleAction(ProductionRuleManager.ACTION action, PredicateObjects predicateObjects = PredicateObjects.NONE, float floatValue = 0.0f, ProductionRuleIdentifier identifier = null)
    {
        /*ProductionRuleAction newAction = null;

        string className = action.ToString().ToLower();
        className = char.ToUpper(className[0]) + className.Substring(1) + "Action";

        Type classType = Type.GetType(className);
        if (classType != null)
        {
            newAction = (ProductionRuleAction)Activator.CreateInstance(classType);
        }
        else
        {
            Debug.LogError($"Could not find class for ProductionRule action: {action}. Expected class to be named: {className}");
        }

        return newAction;*/

        ProductionRuleAction newRule = new ProductionRuleAction();
        newRule.Initialize(action, predicateObjects, floatValue, identifier);

        return newRule;
    }

    virtual public void Initialize(ProductionRuleManager.ACTION action, PredicateObjects predicateObjects, float floatValue, ProductionRuleIdentifier identifier = null)
    {
        this.action = action;
        this.floatValue = floatValue;
        this.predicateObjects = predicateObjects;
        this.identifier = identifier;
    }

    virtual public void Execute(ProductionRuleObject subject, ProductionRuleObject obj, EnvironmentEngine env)
    {
        ProductionRuleManager productionRuleManager = env.GetCachedEnvironmentComponent<ProductionRuleManager>();

        // Make sure predicate is valid
        if (predicateObjects == PredicateObjects.OBJECT && obj == null)
        {
            if (subject != null)
            {
                predicateObjects = PredicateObjects.SUBJECT;
            }
            else
            {
                predicateObjects = PredicateObjects.NONE;
            }
        }

        if (predicateObjects == PredicateObjects.SUBJECT)
        {
            ExecuteAction(subject, env, subject, obj);
        }
        else if (predicateObjects == PredicateObjects.OBJECT)
        {
            ExecuteAction(obj, env, subject, obj);
        }
        else if (predicateObjects == PredicateObjects.BOTH)
        {
            ExecuteAction(subject, env, subject, obj);
            if (obj)
            {
                ExecuteAction(obj, env, subject, obj);
            }
        }
        else if (predicateObjects == PredicateObjects.NONE)
        {
            if (action != ProductionRuleManager.ACTION.SPAWN && action != ProductionRuleManager.ACTION.SWAP)
            {
                ProductionRuleObject productionRuleObject = productionRuleManager.GetProductionRuleObjectByIdentifier(identifier);
                ExecuteAction(productionRuleObject, env, subject, obj);
            }
        }
        else
        {
            throw new ArgumentException("PredicateObjects not recognized");
        }

        if (action == ProductionRuleManager.ACTION.SPAWN || action == ProductionRuleManager.ACTION.SWAP)
        {
            ProductionRuleObject productionRulePrefab = productionRuleManager.productionRuleObjectPrefab.GetComponent<ProductionRuleObject>();
            ExecuteAction(productionRulePrefab, env, subject, obj);
        }
    }

    virtual public void ExecuteAction(ProductionRuleObject productionRuleObject, EnvironmentEngine env, ProductionRuleObject subject, ProductionRuleObject obj)
    {
        //CheckParameterValidity(productionRuleObject, env);

        Vector3 subjectPosition = Vector3.zero;
        if (subject)
        {
            subjectPosition = subject.transform.position;
        }

        bool isNewObject = productionRuleObject != subject && productionRuleObject != obj;

        switch (action)
        {
            case ProductionRuleManager.ACTION.SPAWN:
                if (isNewObject && productionRuleObject)
                {
                    Spawn(productionRuleObject, env, subjectPosition);
                }
                break;

            case ProductionRuleManager.ACTION.REMOVE:
                //Debug.Log(debugPrintString);
                Remove(productionRuleObject, env);
                break;

            case ProductionRuleManager.ACTION.SWAP:
                //Debug.Log(debugPrintString);
                if (productionRuleObject)
                {
                    if (isNewObject)
                    {
                        Spawn(productionRuleObject, env, subjectPosition);
                    }
                    else
                    {
                        Remove(productionRuleObject, env);
                    }
                }
                break;

            case ProductionRuleManager.ACTION.REWARD:
                Debug.Log($"Reward: {floatValue}");
                Reward(env);
                break;

            case ProductionRuleManager.ACTION.REWARD_TOGGLE:
                Debug.Log($"Reward Toggle: {floatValue}");
                Reward(env);

                ProductionRuleManager manager = env.GetCachedEnvironmentComponent<ProductionRuleManager>();
                if (manager != null)
                {
                    List<ProductionRule> rules = manager.GetAllProdRules();
                    List<ProductionRule> rewardRules = new List<ProductionRule>();
                    for (int i = 0; i < rules.Count;  i++)
                    {
                        if (rules[i].actions[0] == this)
                        {
                            rules[i].ToggleActive(false);
                        }
                        else if (rules[i].actions[0].action == ProductionRuleManager.ACTION.REWARD_TOGGLE)
                        {
                            rewardRules.Add(rules[i]);
                        }
                    }
                    ShuffleUtility.ShuffleList(rewardRules);
                    rewardRules[0].ToggleActive(true);
                }
                break;

            case ProductionRuleManager.ACTION.PRINT:
                Debug.Log(debugPrintString);
                break;

            default:
                throw new ArgumentException("ACTION not recognized");
        }
    }

    public void forwardPropagateState()
    {
        // do nothing
    }
    public void Reward(EnvironmentEngine env)
    {
        // Get the agent from the environment engine
        var playerObject = env.GetEnvironmentComponent<U3DPlayer>("Player");
        U3Agent u3Agent = playerObject.GetComponent<U3Agent>();
        u3Agent.AddReward(floatValue);
    }
    public void Spawn(ProductionRuleObject productionRuleObjectPrefab, EnvironmentEngine env, Vector3 position)
    {
        if (mTotalSpawns < MAX_SPAWNS)
        {
            // Instantiate the production rule object
            EnvironmentObject prodRuleObject = env.GetEngine().CreateEnvironmentObject(productionRuleObjectPrefab.gameObject);
            ProductionRuleObject prodRuleObj = prodRuleObject.GetComponent<ProductionRuleObject>();
            prodRuleObj.ProductionRuleObjectInitialize(identifier.ObjectShape, identifier.ObjectColor);
            prodRuleObj.transform.position = position;

            mTotalSpawns++;
        }
    }

    public void Remove(ProductionRuleObject productionRuleObject, EnvironmentEngine env)
    {
        env.RemoveObject(productionRuleObject);
    }

    /*public void Swap(ProductionRuleObject productionRuleObjectPrefab, EnvironmentEngine env, ProductionRuleObject subject, ProductionRuleObject obj)
    {
        Vector3 subjectPosition = subject.transform.position;

        if (predicateObjects == PredicateObjects.SUBJECT)
        {
            Remove(subject, env);
        }
        else if (predicateObjects == PredicateObjects.OBJECT)
        {
            if (obj)
            {
                Remove(obj, env);
            }
            else
            {
                Remove(subject, env);
            }
        }
        else if (predicateObjects == PredicateObjects.BOTH)
        {
            Remove(subject, env);
            if (obj)
            {
                Remove(obj, env);
            }
        }
        else
        {
            throw new ArgumentException("PredicateObjects not recognized, or not supported for Swap action");
        }

        Spawn(productionRuleObjectPrefab, env, subjectPosition);
    }*/

    /*virtual public void CheckParameterValidity(ProductionRuleObject productionRuleObject, EnvironmentEngine env)
    {
        switch (action)
        {
            case ProductionRuleManager.ACTION.SPAWN:
            case ProductionRuleManager.ACTION.REMOVE:
            case ProductionRuleManager.ACTION.SWAP:
                if (productionRuleObject == null)
                {
                    throw new ArgumentException("Invalid call to ExecuteAction: ProductionRuleObject is null");
                }
                break;
            case ProductionRuleManager.ACTION.REWARD:
            case ProductionRuleManager.ACTION.PRINT:
                break;
            default:
                throw new ArgumentException("ACTION not recognized");
        }

    }*/

    virtual public string Encode()
    {
        if (action == ProductionRuleManager.ACTION.SPAWN)
        {
            return $"spawn a {this.identifier.Encode()}";
        }
        else if (action == ProductionRuleManager.ACTION.REMOVE)
        {
            return $"remove a {this.identifier.Encode()}";
        }
        else if (action == ProductionRuleManager.ACTION.SWAP)
        {
            if (predicateObjects == PredicateObjects.SUBJECT)
            {
                return $"swap {this.identifier.Encode()} with subject";
            }
            else if (predicateObjects == PredicateObjects.OBJECT)
            {
                return $"swap {this.identifier.Encode()} with object";
            }
            else if (predicateObjects == PredicateObjects.BOTH)
            {
                return $"swap {this.identifier.Encode()} with both";
            }
            else
            {
                throw new ArgumentException("PredicateObjects not recognized, or not supported for Swap action");
            }

        }
        else if (action == ProductionRuleManager.ACTION.REWARD)
        {
            return $"reward {this.floatValue}";
        }
        else if (action == ProductionRuleManager.ACTION.REWARD_TOGGLE)
        {
            return $"reward toggle {this.floatValue}";
        }
        else if (action == ProductionRuleManager.ACTION.PRINT)
        {
            return $"print {this.debugPrintString}";
        }
        else
        {
            throw new ArgumentException("ACTION not recognized");
        }

    }

    virtual public void OnTrialOver()
    {
        mTotalSpawns = 0;
    }
}
