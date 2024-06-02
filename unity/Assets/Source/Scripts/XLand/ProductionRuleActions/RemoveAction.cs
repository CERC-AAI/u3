using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;
using NUnit;

[Serializable]
public class RemoveAction : ProductionRuleAction
{
    override public void Execute(ProductionRuleObject subject, ProductionRuleObject obj, EnvironmentEngine env)
    {
        ProductionRuleManager productionRuleManager = env.GetCachedEnvironmentComponent<ProductionRuleManager>();

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
            ExecuteAction(obj, env, subject, obj);
        }
        else if (predicateObjects == PredicateObjects.NONE)
        {
            ProductionRuleObject productionRuleObject = productionRuleManager.GetProductionRuleObjectByIdentifier(identifier);
            ExecuteAction(subject, env, subject, obj);
        }
    }

    override public void ExecuteAction(ProductionRuleObject productionRuleObject, EnvironmentEngine env, ProductionRuleObject subject, ProductionRuleObject obj)
    {
        //CheckParameterValidity(productionRuleObject, env);

        Remove(productionRuleObject, env);
    }

    /*override public void CheckParameterValidity(ProductionRuleObject productionRuleObject, EnvironmentEngine env)
    {
        if (productionRuleObject == null)
        {
            throw new ArgumentException("Invalid call to ExecuteAction: ProductionRuleObject is null");
        }
    }*/

    override public string Encode()
    {
        if (predicateObjects == PredicateObjects.SUBJECT)
        {
            return $"remove the subject";
        }
        else if (predicateObjects == PredicateObjects.OBJECT)
        {
            return $"remove the object";
        }
        else if (predicateObjects == PredicateObjects.BOTH)
        {
            return $"remove both subject and object";
        }
        else if (predicateObjects == PredicateObjects.NONE)
        {
            return $"remove a {this.identifier.Encode()}";
        }

        return "error";
    }
}
