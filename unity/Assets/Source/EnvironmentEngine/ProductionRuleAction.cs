using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;

public class ProductionRuleAction
{
    private List<ACTION> actions;
    // private List<object> actionParameters;

    private float reward;

    private ProductionRuleObject spawnedObject;

    private ProductionRuleObject removedObject;

    public ProductionRuleAction(List<ACTION> actions, float reward)
    {
        this.actions = actions;
        this.reward = reward;
    }

    public bool Execute(ProductionRuleObject subject, ProductionRuleObject obj, EnvironmentEngine env)
    {
        throw new ArgumentException("Actions are not implemented yet");
    }

    public string Encode()
    {
        for (int i = 0; i < this.actions.Count; i++)
        {
            if (this.actions[i] == ACTION.SPAWN)
            {
                return $"spawn a {this.spawnedObject}";
            }
            else if (this.actions[i] == ACTION.REMOVE)
            {
                return $"remove a {this.removedObject}";
            }
            else if (this.actions[i] == ACTION.REWARD)
            {
                return $"reward {this.reward}";
            }
            else
            {
                throw new ArgumentException("Action not recognized");
            }
        }

        return "";

    }
}
