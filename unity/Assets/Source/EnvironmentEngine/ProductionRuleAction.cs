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
    public ACTION action;
    // private List<object> actionParameters;

    public float reward;

    public ProductionRuleObject spawnedObject;

    public ProductionRuleObject removedObject;

    public string debugPrintString = "Test print";

    public ProductionRuleAction(ACTION action, float reward)
    {
        this.action = action;
        this.reward = reward;
    }

    public void Execute(ProductionRuleObject subject, ProductionRuleObject obj, EnvironmentEngine env)
    {
        switch (action)
        {
            // case ACTION.SPAWN:
            //     return Spawn(subject, obj, env);

            // case ACTION.REMOVE:
            //     return Remove(subject, obj, env);

            // case ACTION.REWARD:
            //     return Reward(subject, obj, env);

            case ACTION.PRINT:
                Debug.Log(debugPrintString);
                break;
            default:
                throw new ArgumentException("Action not recognized");
        }
    }

    public string Encode()
    {
        if (action == ACTION.SPAWN)
        {
            return $"spawn a {this.spawnedObject}";
        }
        else if (action == ACTION.REMOVE)
        {
            return $"remove a {this.removedObject}";
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
