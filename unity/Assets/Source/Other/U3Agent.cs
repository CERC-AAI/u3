using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors.Reflection;
using System.Reflection;
using Unity.MLAgents.Sensors;

//This class exists solely to shuttle the callbacks from ML Agents to the U3 EnvironmentAgent class
public class U3Agent : Agent
{
    EnvironmentAgent mAgent;
    bool mUsingHeuristics = false;

    public void Start()
    {
        mAgent = GetComponent<EnvironmentAgent>();

        AddSensor(new VectorSensor(9));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!mUsingHeuristics)
        {
            mAgent.OnActionReceived(actions);
        }
        mAgent.GetEngine().OnAgentActionReceived(mAgent, mAgent.ShouldBlockDecision(actions));

        mUsingHeuristics = false;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        mAgent.Heuristic(in actionsOut);
        mUsingHeuristics = true;
    }

    private void Update()
    {
        FieldInfo field = typeof(Agent).GetField("sensors",
                            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

        List<ISensor> value = (List<ISensor>)field.GetValue(this);
    }

    public void AddSensor(ISensor sensor)
    {
        if (sensor != null)
        {
            FieldInfo field = typeof(Agent).GetField("sensors",
                                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

            List<ISensor> value = (List<ISensor>)field.GetValue(this);
            value.Add(sensor);
            field.SetValue(this, value);
        }
    }
}
