using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;

public class ProductionRule
{
    public ProductionRuleCondition condition; // may contain multiple CONDITION
    public ProductionRuleAction action;

    public ProductionRule(ProductionRuleCondition condition, ProductionRuleAction action)
    {
        this.condition = condition;
        this.action = action;
    }


    public bool CheckRule(ProductionRuleObject subject, ProductionRuleObject obj, EnvironmentEngine env)
    {
        if (this.condition.IsSatisfied(subject, obj, env))
        {
            return true;
        }
        return false;
    }

    public void ExecuteRule(ProductionRuleObject subject, ProductionRuleObject obj, EnvironmentEngine env)
    {
        this.action.Execute(subject, obj, env);
    }

    // public bool CanBeSubject(ProductionRuleObject obj)
    // {
    //     return this.condition.MatchesObject(obj);
    // }

    public string Encode()
    {
        return $"if {condition.Encode()} then {action.Encode()}";
    }

}
