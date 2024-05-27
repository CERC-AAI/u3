using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;

public class EnvironmentTask : EnvironmentComponentHolder
{
    public bool startEpisodeOnRun = true;


    EnvironmentAgent mAgent;

    float mTotalReward = 0;

    protected override void Initialize()
    {
        mAgent = GetComponent<EnvironmentAgent>();

        base.Initialize();
    }

    public override void OnRunStarted()
    {
        base.OnRunStarted();

        if (startEpisodeOnRun)
        {
            StartEpisode();
        }
    }

    public override void OnRunEnded()
    {
        base.OnRunEnded();

        EndEpisode();
    }

    virtual public void StartEpisode()
    {
        mEngine.OnTaskEpisodeStarted(this);

        mTotalReward = 0;

        for (int i = 0; i < mComponents.Length; i++)
        {
            mComponents[i].OnEpisodeStarted();
        }
    }

    virtual public void EndEpisode()
    {
        for (int i = 0; i < mComponents.Length; i++)
        {
            mComponents[i].OnEpisodeEnded();
        }

        mAgent.DoEndEpisode();
    }

    public void AddReward(float reward)
    {
        mTotalReward += reward;
        mAgent.OnAddedReward(reward);
    }

    public void SetReward(float reward)
    {
        mTotalReward = reward;
        mAgent.OnSetReward(reward);
    }
}
