using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;

public class EnvironmentAgent : Agent
{
    EnvironmentBrain mBrain;

    private void Init()
    {
        CheckBrain();
    }

    private void Update()
    {
        CheckBrain();
    }

    void CheckBrain()
    {
        if (mBrain == null)
        {
            mBrain = GetComponent<EnvironmentBrain>();
            mBrain.CheckAgent();
        }
    }

    public override void Initialize()
    {
        CheckBrain();

        mBrain.Initialize();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        CheckBrain();

        float[] vectorAction = new float[actions.DiscreteActions.Length];
        for (int i = 0; i < vectorAction.Length; i++)
        {
            vectorAction[i] = actions.DiscreteActions[i];
        }

        mBrain.OnEnvironmentActionReceived(vectorAction);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        CheckBrain();

        float[] vectorAction = new float[actionsOut.DiscreteActions.Length];

        mBrain.Heuristic(vectorAction);

        ActionSegment<int> actions = actionsOut.DiscreteActions;

        for (int i = 0; i < vectorAction.Length; i++)
        {
            actions[i] = (int)vectorAction[0];
        }
    }

    public override void OnEpisodeBegin()
    {
        CheckBrain();

        if (mBrain.isRunning())
        {
            mBrain.OnEpisodeBegin();
        }
    }
}
