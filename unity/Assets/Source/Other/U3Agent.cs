using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;

//This class exists solely to shuttle the callbacks from ML Agents to the U3 EnvironmentAgent class
public class U3Agent : Agent
{
    EnvironmentAgent mAgent;

    public void Start()
    {
        mAgent = GetComponent<EnvironmentAgent>();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        mAgent.OnActionReceived(actions);
        mAgent.GetEngine().OnAgentActionReceived(mAgent, mAgent.ShouldBlockDecision(actions));
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        mAgent.Heuristic(in actionsOut);
    }
}
