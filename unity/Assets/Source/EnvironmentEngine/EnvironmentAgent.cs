using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using System.Reflection;

[RequireComponent(typeof(U3Agent))]
public class EnvironmentAgent : EnvironmentComponent
{

    U3Agent mAgentScript;
    BehaviorParameters mBehaviorParameters;

    public List<ActionInfo> mActions = new List<ActionInfo>();
    public List<SensorInfo> mSensors = new List<SensorInfo>();

    int mLastInputFrame = 0;

    protected override void Initialize()
    {
        mAgentScript = GetComponent<U3Agent>();
        mBehaviorParameters = GetComponent<BehaviorParameters>();

        base.Initialize();

        BuildActionList();

        BuildSensorList();

        mAgentScript.InitAgent();
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

        mActions.Clear();

        for (int i = 0; i < environmentComponents.Length; i++)
        {
            environmentComponents[i].AppendActionLists(mActions);
        }

        int totalContinuousActions = 0;
        List<int> branchSizes = new List<int>();
        for (int i = 0; i < mActions.Count; i++)
        {
            totalContinuousActions += mActions[i].getFloatCount();
            for (int j = 0; j < mActions[i].getIntegerCount(); j++)
            {
                branchSizes.Add((int)(mActions[i].getMaxValue() - mActions[i].getMinValue()));
            }
        }

        ActionSpec totalAgentBehaviors = new ActionSpec();

        totalAgentBehaviors.NumContinuousActions = totalContinuousActions;

        int[] totalBranchSizes = new int[branchSizes.Count];
        for (int i = 0; i < branchSizes.Count; i++)
        {
            totalBranchSizes[i] = branchSizes[i];
        }
        totalAgentBehaviors.BranchSizes = totalBranchSizes;

        if ((behaviorParameters.BrainParameters.ActionSpec.NumContinuousActions != 0 || behaviorParameters.BrainParameters.ActionSpec.NumDiscreteActions != 0))
        {
            Debug.LogError("You have manually defined the action spec of the agent (" + name + "). Please use AppendActionLists() to set action callbacks.");
        }

        behaviorParameters.BrainParameters.ActionSpec = totalAgentBehaviors;

        //Turn it off and on again to make sure the changes update.
        //GetComponent<Agent>().enabled = false;
        //GetComponent<Agent>().enabled = true;
    }

    protected virtual void BuildSensorList()
    {
        EnvironmentComponent[] environmentComponents = GetComponents<EnvironmentComponent>();
        mSensors.Clear();

        for (int i = 0; i < environmentComponents.Length; i++)
        {
            environmentComponents[i].AppendSensorLists(mSensors);
            //Debug.Log("Number of sensors after appending: " + mSensors.Count);
        }

        // Get the existing U3SensorComponent or add a new one if it doesn't exist
        U3SensorComponent sensorComponent = gameObject.GetComponent<U3SensorComponent>();
        if (sensorComponent == null)
        {
            sensorComponent = gameObject.AddComponent<U3SensorComponent>();
        }

        foreach (SensorInfo sensorInfo in mSensors)
        {
            sensorComponent.AddSensorInfo(sensorInfo);
        }
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

        mAgentScript.RequestDecision();
    }

    public void CollectObservations()
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

    public void OnActionReceived(ActionBuffers actions)
    {
        List<int> currentDiscreteBuffer = new List<int>();
        List<float> currentContinuousBuffer = new List<float>();
        int discreteOffset = 0;
        int continuousOffset = 0;

        for (int i = 0; i < mActions.Count; i++)
        {
            currentDiscreteBuffer.Clear();
            currentContinuousBuffer.Clear();

            for (int j = discreteOffset; j < discreteOffset + mActions[i].getIntegerCount(); j++)
            {
                currentDiscreteBuffer.Add(actions.DiscreteActions[j]);
            }
            for (int j = continuousOffset; j < continuousOffset + mActions[i].getFloatCount(); j++)
            {
                currentContinuousBuffer.Add(actions.ContinuousActions[j]);
            }

            discreteOffset += mActions[i].getIntegerCount();
            continuousOffset += mActions[i].getFloatCount();

            if (currentDiscreteBuffer.Count > 0)
            {
                mActions[i].setActions(currentDiscreteBuffer);
            }
            if (currentContinuousBuffer.Count > 0)
            {
                mActions[i].setActions(currentContinuousBuffer);
            }
        }
    }

    virtual public void Heuristic(in ActionBuffers actionsOut)
    {
        EnvironmentComponent[] environmentComponents = mParentObject.GetEnvironmentComponents();
        mParentObject.ProcessInputs();
        for (int i = 0; i < environmentComponents.Length; i++)
        {
            environmentComponents[i].StoreUserInputs();
        }
        // Debug.Log("Heuristic is called.");
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
