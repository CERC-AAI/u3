using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

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

    public override void OnActionReceived(float[] vectorAction)
    {
        CheckBrain();

        mBrain.OnEnvironmentActionReceived(vectorAction);
    }

    public override void Heuristic(float[] actionsOut)
    {
        CheckBrain();

        mBrain.Heuristic(actionsOut);
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
