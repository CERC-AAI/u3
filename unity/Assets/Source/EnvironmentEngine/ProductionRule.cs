using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;

public class ProductionRule
{
    public List<ProductionRuleCondition> conditions; // may contain multiple CONDITION
    public List<ProductionRuleAction> actions;

    public ProductionRule(List<ProductionRuleCondition> condition, List<ProductionRuleAction> action)
    {
        this.conditions = condition;
        this.actions = action;
    }


    public bool CheckRule(ProductionRuleObject subject, ProductionRuleObject obj, EnvironmentEngine env)
    {
        foreach (ProductionRuleCondition condition in conditions)
        {
            if (!condition.IsSatisfied(subject, obj, env))
            {
                return false;
            }
        }
        return true;
    }

    public void ExecuteRule(ProductionRuleObject subject, ProductionRuleObject obj, EnvironmentEngine env)
    {
        foreach (ProductionRuleAction action in actions)
        {
            action.Execute(subject, obj, env);
        }
    }

    // public bool CanBeSubject(ProductionRuleObject obj)
    // {
    //     return this.condition.MatchesObject(obj);
    // }

    public string Encode()
    {
        string encoded = "If ";
        for (int i = 0; i < conditions.Count; i++)
        {
            encoded += conditions[i].Encode();
            if (i < conditions.Count - 1)
            {
                encoded += " and ";
            }
        }
        encoded += " then ";
        for (int i = 0; i < actions.Count; i++)
        {
            encoded += actions[i].Encode();
            if (i < actions.Count - 1)
            {
                encoded += " and ";
            }
        }
        return encoded;

    }

}
