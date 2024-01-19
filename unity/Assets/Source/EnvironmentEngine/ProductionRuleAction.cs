using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;

[Serializable]
public class ProductionRuleAction
{

    public enum PredicateObjects
    {
        SUBJECT,
        OBJECT,
        BOTH,
        NONE
    }

    public PredicateObjects predicateObjects;

    public ProductionRuleIdentifier identifier;

    public ACTION action;
    // private List<object> actionParameters;

    public float reward;


    public string debugPrintString = "Test print";

    public ProductionRuleAction(ACTION action, float reward, PredicateObjects predicateObjects, ProductionRuleIdentifier identifier = null)
    {
        this.action = action;
        this.reward = reward;
        this.predicateObjects = predicateObjects;
        this.identifier = identifier;
    }

    public void Execute(ProductionRuleObject subject, ProductionRuleObject obj, EnvironmentEngine env)
    {
        ProductionRuleManager productionRuleManager = env.GetCachedEnvironmentComponent<ProductionRuleManager>();

        // if (predicateObjects == PredicateObjects.NONE)
        // {
        //     if (identifier == null)
        //     {
        //         ProductionRuleObject productionRuleObject = productionRuleManager.GetProductionRuleObjectByIdentifier(identifier);
        //         ExecuteAction(productionRuleObject, env);

        //     }
        //     else
        //     {
        //         // create a new object
        //         ProductionRuleObject productionRuleObject = new ProductionRuleObject(identifier);
        //         ExecuteAction(productionRuleObject, env);

        //     }

        // }

        if (predicateObjects == PredicateObjects.NONE)
        {
            ProductionRuleObject productionRuleObject = productionRuleManager.GetProductionRuleObjectByIdentifier(identifier);
            ExecuteAction(productionRuleObject, env);
        }

        else if (predicateObjects == PredicateObjects.SUBJECT)
        {
            ExecuteAction(subject, env);
        }
        else if (predicateObjects == PredicateObjects.OBJECT)
        {
            ExecuteAction(obj, env);
        }
        else if (predicateObjects == PredicateObjects.BOTH)
        {
            ExecuteAction(subject, env);
            ExecuteAction(obj, env);
        }
        else
        {
            throw new ArgumentException("PredicateObjects not recognized");
        }
    }

    public void ExecuteAction(ProductionRuleObject productionRuleObject, EnvironmentEngine env)
    {

        CheckParameterValidity(productionRuleObject, env);
        switch (action)
        {
            case ACTION.SPAWN:
                Debug.Log(debugPrintString);
                // Spawn(productionRuleObject, env);
                break;

            case ACTION.REMOVE:
                Debug.Log(debugPrintString);
                Remove(productionRuleObject, env);
                break;

            // case ACTION.REWARD:
            //     return Reward(subject, obj, env);

            case ACTION.PRINT:
                Debug.Log(debugPrintString);
                break;
            default:
                throw new ArgumentException("Action not recognized");
        }
    }

    // public void Spawn(ProductionRuleObject productionRuleObject, EnvironmentEngine env)
    // {
    //     // CreateEnvironmentObject takes a base
    //     productionRuleObject.GetEngine().CreateEnvironmentObject(productionRuleObject.gameObject);
    // }
    public void Remove(ProductionRuleObject productionRuleObject, EnvironmentEngine env)
    {
        productionRuleObject.Remove();
    }

    public void CheckParameterValidity(ProductionRuleObject productionRuleObject, EnvironmentEngine env)
    {
        switch (action)
        {
            case ACTION.SPAWN:
            case ACTION.REMOVE:
                if (productionRuleObject == null)
                {
                    throw new ArgumentException("Invalid call to ExecuteAction: ProductionRuleObject is null");
                }
                break;
            case ACTION.REWARD:
            case ACTION.PRINT:
                break;
            default:
                throw new ArgumentException("Action not recognized");
        }

    }

    public string Encode()
    {
        if (action == ACTION.SPAWN)
        {
            return $"spawn a {this.identifier.Encode()}";
        }
        else if (action == ACTION.REMOVE)
        {
            return $"remove a {this.identifier.Encode()}";
        }
        else if (action == ACTION.REWARD)
        {
            return $"reward {this.reward}";
        }
        else if (action == ACTION.PRINT)
        {
            return $"print {this.reward}";
        }
        else
        {
            throw new ArgumentException("Action not recognized");
        }

    }
}
