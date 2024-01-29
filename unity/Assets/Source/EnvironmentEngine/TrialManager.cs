using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;
using Unity.Mathematics;

public class TrialManager
{

    public class InitialAgentState
    {
        public Vector3 position;
        public Quaternion rotation;
        // public Vector3 velocity;
        // public Vector3 angularVelocity;
    }

    public class InitialProductionRuleObjectState
    {
        public string shape;
        public string color;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        // public Vector3 velocity;
        // public Vector3 angularVelocity;
    }

    public int trialCounter = 0;
    public int maxTrials = 10;
    InitialAgentState agentState = new InitialAgentState();
    List<InitialProductionRuleObjectState> productionRuleObjectStates = new List<InitialProductionRuleObjectState>();
    public int TrialResetUpdateFrequency = 10000;
    public int trialDurationCounter = 0;

    public void SaveInitialState(U3DPlayer player, List<ProductionRuleObject> productionRuleObjects)
    {
        SaveAgentState(player);
        SaveProductionRuleObjectStates(productionRuleObjects);
    }

    public InitialAgentState GetInitialAgentState()
    {
        return agentState;
    }

    public List<InitialProductionRuleObjectState> GetInitialProductionRuleObjectStates()
    {
        return productionRuleObjectStates;
    }

    public bool IsTrialOver()
    {
        trialDurationCounter++;
        if (trialDurationCounter >= TrialResetUpdateFrequency)
        {
            return true;
        }
        return false;
    }

    public bool IsMaxTrials()
    {
        return trialCounter >= maxTrials;
    }
    public void IncrementTrialCounter()
    {
        trialCounter++;
    }

    public void ResetTrialDurationCounter()
    {
        trialDurationCounter = 0;
    }

    public void IncrementTrialDurationCounter()
    {
        trialDurationCounter++;
    }
    public void SaveAgentState(U3DPlayer agent)
    {
        // Save the agent's position, rotation, velocity, angular velocity, etc.
        // Save the agent's position, rotation, velocity, angular velocity, etc.
        agentState.position = agent.transform.position;
        agentState.rotation = agent.transform.rotation;
    }

    public void SaveProductionRuleObjectStates(List<ProductionRuleObject> productionRuleObjects)
    {
        // Save the ProductionRuleObject's position, rotation, velocity, angular velocity, etc.
        // Save the ProductionRuleObject's position, rotation, velocity, angular velocity, etc.
        foreach (ProductionRuleObject productionRuleObject in productionRuleObjects)
        {
            InitialProductionRuleObjectState productionRuleObjectState = new InitialProductionRuleObjectState
            {
                shape = productionRuleObject.GetIdentifier().ObjectShape,
                color = productionRuleObject.GetIdentifier().ObjectColor,
                position = productionRuleObject.transform.position,
                rotation = productionRuleObject.transform.rotation,
                scale = productionRuleObject.transform.localScale
            };
            productionRuleObjectStates.Add(productionRuleObjectState);
        }
    }

}