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


    public PredicateObjects predicateObjects;

    public ProductionRuleIdentifier identifier;

    public Action action;
    // private List<object> actionParameters;

    public float reward;


    public string debugPrintString = "Test print";

    public ProductionRuleAction(Action action, float reward, PredicateObjects predicateObjects, ProductionRuleIdentifier identifier = null)
    {
        this.action = action;
        this.reward = reward;
        this.predicateObjects = predicateObjects;
        this.identifier = identifier;
    }

    public void Execute(ProductionRuleObject subject, ProductionRuleObject obj, EnvironmentEngine env)
    {
        ProductionRuleManager productionRuleManager = env.GetCachedEnvironmentComponent<ProductionRuleManager>();

        if (predicateObjects == PredicateObjects.SUBJECT)
        {
            if (action != Action.SPAWN && action != Action.SWAP)
            {
                ExecuteAction(subject, env, subject, obj);
            }
            else
            {
                ProductionRuleObject productionRulePrefab = productionRuleManager.productionRuleObjectPrefab.GetComponent<ProductionRuleObject>();
                ExecuteAction(productionRulePrefab, env, subject, obj);
            }
        }
        else if (predicateObjects == PredicateObjects.OBJECT)
        {
            ExecuteAction(obj, env, subject, obj);
        }
        else if (predicateObjects == PredicateObjects.BOTH)
        {
            ExecuteAction(subject, env, subject, obj);
            ExecuteAction(obj, env, subject, obj);
        }
        else if (predicateObjects == PredicateObjects.NONE)
        {
            if (action != Action.SPAWN)
            {
                ProductionRuleObject productionRuleObject = productionRuleManager.GetProductionRuleObjectByIdentifier(identifier);
                ExecuteAction(productionRuleObject, env, subject, obj);
            }
            else
            {
                ProductionRuleObject productionRulePrefab = productionRuleManager.productionRuleObjectPrefab.GetComponent<ProductionRuleObject>();
                ExecuteAction(productionRulePrefab, env, subject, obj);
            }
        }
        else
        {
            throw new ArgumentException("PredicateObjects not recognized");
        }
    }

    public void ExecuteAction(ProductionRuleObject productionRuleObject, EnvironmentEngine env, ProductionRuleObject subject, ProductionRuleObject obj)
    {

        CheckParameterValidity(productionRuleObject, env);
        switch (action)
        {
            case Action.SPAWN:
                Debug.Log(debugPrintString);
                if (subject == null)
                {
                    Spawn(productionRuleObject, env, Vector3.zero);
                }
                else
                {
                    Spawn(productionRuleObject, env, subject.transform.position);
                }
                break;

            case Action.REMOVE:
                Debug.Log(debugPrintString);
                Remove(productionRuleObject, env);
                break;

            case Action.SWAP:
                Debug.Log(debugPrintString);
                Swap(productionRuleObject, env, subject, obj);
                break;

            case Action.REWARD:
                Debug.Log(debugPrintString);
                Reward(env);
                break;

            case Action.PRINT:
                Debug.Log(debugPrintString);
                break;
            default:
                throw new ArgumentException("Action not recognized");
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
        u3Agent.AddReward(reward);
    }
    public void Spawn(ProductionRuleObject productionRuleObjectPrefab, EnvironmentEngine env, Vector3 position)
    {
        // Instantiate the production rule object
        EnvironmentObject prodRuleObject = env.GetEngine().CreateEnvironmentObject(productionRuleObjectPrefab.gameObject);
        ProductionRuleObject prodRuleObj = prodRuleObject.GetComponent<ProductionRuleObject>();
        prodRuleObj.ProductionRuleObjectInitialize(identifier.ObjectShape, identifier.ObjectColor);
        prodRuleObj.transform.position = position;
    }

    public void Remove(ProductionRuleObject productionRuleObject, EnvironmentEngine env)
    {
        env.RemoveObject(productionRuleObject);
    }

    public void Swap(ProductionRuleObject productionRuleObjectPrefab, EnvironmentEngine env, ProductionRuleObject subject, ProductionRuleObject obj)
    {
        Vector3 subjectPosition = subject.transform.position;

        if (predicateObjects == PredicateObjects.SUBJECT)
        {
            Remove(subject, env);
        }
        else if (predicateObjects == PredicateObjects.OBJECT)
        {
            Remove(obj, env);
        }
        else if (predicateObjects == PredicateObjects.BOTH)
        {
            Remove(subject, env);
            Remove(obj, env);
        }
        else
        {
            throw new ArgumentException("PredicateObjects not recognized, or not supported for Swap action");
        }

        Spawn(productionRuleObjectPrefab, env, subjectPosition);
    }

    public void CheckParameterValidity(ProductionRuleObject productionRuleObject, EnvironmentEngine env)
    {
        switch (action)
        {
            case Action.SPAWN:
            case Action.REMOVE:
            case Action.SWAP:
                if (productionRuleObject == null)
                {
                    throw new ArgumentException("Invalid call to ExecuteAction: ProductionRuleObject is null");
                }
                break;
            case Action.REWARD:
            case Action.PRINT:
                break;
            default:
                throw new ArgumentException("Action not recognized");
        }

    }

    public string Encode()
    {
        if (action == Action.SPAWN)
        {
            return $"spawn a {this.identifier.Encode()}";
        }
        else if (action == Action.REMOVE)
        {
            return $"remove a {this.identifier.Encode()}";
        }
        else if (action == Action.SWAP)
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
        else if (action == Action.REWARD)
        {
            return $"reward {this.reward}";
        }
        else if (action == Action.PRINT)
        {
            return $"print {this.reward}";
        }
        else
        {
            throw new ArgumentException("Action not recognized");
        }

    }
}
