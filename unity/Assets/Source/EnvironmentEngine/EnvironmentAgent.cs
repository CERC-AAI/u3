using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;

public class ActionInfo
{
    public enum TYPE
    {
        DISCRETE = 1,
        CONTINUOUS
    }

    public delegate void DiscreteAction(int actionValue);
    public delegate void ContinuousAction(float actionValue);

    public Delegate mActionCallback;
    public int mActionCount = 1;
    public TYPE mType;

    public ActionInfo(DiscreteAction callback, int actionCount)
    {
        mType = TYPE.DISCRETE;
        mActionCount = actionCount;
        mActionCallback = callback;
    }

    public ActionInfo(ContinuousAction callback)
    {
        mType = TYPE.CONTINUOUS;
        mActionCount = 1;
        mActionCallback = callback;
    }

    public void DoAction(int actionValueDiscrete = 0, float actionValueContinuous = 0f)
    {
        switch (mType)
        {
            case TYPE.DISCRETE:
                ((DiscreteAction)mActionCallback)?.Invoke(actionValueDiscrete);
                break;
            case TYPE.CONTINUOUS:
                ((ContinuousAction)mActionCallback)?.Invoke(actionValueContinuous);
                break;
            default:
                Debug.LogError("Invalid action type.");
                break;
        }
    }

}

[RequireComponent(typeof(U3Agent))]
public class EnvironmentAgent : EnvironmentComponent
{

    U3Agent mAgentScript;
    BehaviorParameters mBehaviorParameters;

    List<ActionInfo> mDiscreteActions = new List<ActionInfo>();
    List<ActionInfo> mContinuousActions = new List<ActionInfo>();

    protected override void Initialize()
    {
        mAgentScript = GetComponent<U3Agent>();
        mBehaviorParameters = GetComponent<BehaviorParameters>();

        base.Initialize();

        BuildActionList();
    }


    void Reset()
    {
        BehaviorParameters behaviorParameters = GetComponent<BehaviorParameters>();
        if (behaviorParameters)
        {
            behaviorParameters.BrainParameters.ActionSpec = new ActionSpec();
        }
    }

    protected virtual void BuildActionList()
    {
        EnvironmentComponent[] environmentComponents = GetComponents<EnvironmentComponent>();
        BehaviorParameters behaviorParameters = GetComponent<BehaviorParameters>();

        mDiscreteActions.Clear();
        mContinuousActions.Clear();

        List<ActionInfo> allActions = new List<ActionInfo>();

        for (int i = 0; i < environmentComponents.Length; i++)
        {
            environmentComponents[i].AppendActionLists(allActions);
        }

        for (int i = 0; i < allActions.Count; i++)
        {
            if (allActions[i].mType == ActionInfo.TYPE.DISCRETE)
            {
                mDiscreteActions.Add(allActions[i]);
            }
            else
            {
                mContinuousActions.Add(allActions[i]);
            }
        }

        ActionSpec totalAgentBehaviors = new ActionSpec();

        totalAgentBehaviors.NumContinuousActions = mContinuousActions.Count;

        int[] branchSizes = new int[mDiscreteActions.Count];
        for (int i = 0; i < mDiscreteActions.Count; i++)
        {
            branchSizes[i] = mDiscreteActions[i].mActionCount;
        }
        totalAgentBehaviors.BranchSizes = branchSizes;

        if ((behaviorParameters.BrainParameters.ActionSpec.NumContinuousActions != 0 || behaviorParameters.BrainParameters.ActionSpec.NumDiscreteActions != 0))
        {
            Debug.LogError("You have manually defined the action spec of the agent (" + name + "). Please use AppendActionLists() to set action callbacks.");
        }

        behaviorParameters.BrainParameters.ActionSpec = totalAgentBehaviors;

        //Turn it off and on again to make sure the changes update.
        GetComponent<Agent>().enabled = false;
        GetComponent<Agent>().enabled = true;
    }

    protected override void DoRegisterCallbacks()
    {
        HealthBar healthBar = GetComponent<HealthBar>();
        if (healthBar)
        {
            RegisterCallback(ref healthBar.OnDiedCallbacks, OnDied);
        }

        base.DoRegisterCallbacks();
    }

    //[Callback(typeof(HealthBar), CallbackScope.SELF)]
    virtual protected void OnDied()
    {
        DoEndEpisode();
    }

    public void RequestDecision()
    {
        CollectObservations();

        mAgentScript.RequestDecision();
    }

    void CollectObservations()
    {
    }

    virtual public bool ShouldRequestDecision(long fixedUdpateNumber)
    {
        return true;// fixedUdpateNumber % 10 == 0;
    }

    virtual public bool ShouldBlockDecision(ActionBuffers actions)
    {
        return false;// fixedUdpateNumber % 10 == 0;
    }

    virtual public void OnActionReceived(ActionBuffers actions)
    {
        for (int i = 0; i < mContinuousActions.Count; i++)
        {
            if (actions.ContinuousActions.Length > i)
            {
                mContinuousActions[i].DoAction(0, actions.ContinuousActions[i]);
            }
        }

        for (int i = 0; i < mDiscreteActions.Count; i++)
        {
            if (actions.DiscreteActions.Length > i)
            {
                mDiscreteActions[i].DoAction(actions.DiscreteActions[i], 0f);
            }
        }

    }

    virtual public void Heuristic(in ActionBuffers actionsOut)
    {
        Debug.Log("Heuristic");
    }

    virtual public void DoEndEpisode(bool timedOut = false)
    {
        if (timedOut)
        {
            mAgentScript.EpisodeInterrupted();
        }
        else
        {
            mAgentScript.EndEpisode();
        }
        mEngine.AgentEndedEpisode(this);
    }

    private void OnDisable()
    {
        DoEndEpisode();
    }

    virtual public void OnAddedReward(float reward)
    {
        mAgentScript.AddReward(reward);
    }

    virtual public void OnSetReward(float reward)
    {
        mAgentScript.AddReward(reward);
    }

    public void AddSensor(ISensor sensor)
    {
        mAgentScript.AddSensor(sensor);
    }
}
