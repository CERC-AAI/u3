using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;

[Serializable]
public class ProductionRule
{
    public List<ProductionRuleCondition> conditions; // may contain multiple CONDITION. These use AND logic.
    public List<ProductionRuleAction> actions;


    public ProductionRule(ProductionRuleCondition condition, ProductionRuleAction action)
    {
        this.conditions = new List<ProductionRuleCondition>();
        this.conditions.Add(condition);
        this.actions = new List<ProductionRuleAction>();
        this.actions.Add(action);
    }

    public ProductionRule(List<ProductionRuleCondition> condition, List<ProductionRuleAction> action)
    {
        this.conditions = condition;
        this.actions = action;
    }


    public bool CheckCallback(ProductionRuleManager.CONDITION callbackCondition, ProductionRuleObject subject, ProductionRuleObject obj, EnvironmentEngine env)
    {
        foreach (ProductionRuleCondition condition in conditions)
        {
            if (!condition.IsSatisfied(callbackCondition, subject, obj, env))
            {
                return false;
            }
        }
        return true;

    }
    public bool CheckRule(ProductionRuleObject subject, ProductionRuleObject obj, EnvironmentEngine env)
    {
        foreach (ProductionRuleCondition condition in conditions)
        {
            if (!condition.IsSatisfied(ProductionRuleManager.CONDITION.NONE, subject, obj, env))
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
        // update the graph
        // disabled since we aren't using the graph approach
        // env.GetCachedEnvironmentComponent<ProductionRuleManager>().UpdateProductionRuleGraph(this); // TODO: this is a hack, we need to update the graph with list of actions
    }

    public void forwardPropagateState()
    {
        foreach (ProductionRuleAction productionRuleAction in actions)
        {
            productionRuleAction.forwardPropagateState();
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

    public void OnTrialOver()
    {
        for (int i = 0; i < conditions.Count; i++)
        {
            conditions[i].OnTrialOver();
        }
        for (int i = 0; i < actions.Count; i++)
        {
            actions[i].OnTrialOver();
        }
    }

}
