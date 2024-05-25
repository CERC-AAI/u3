using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using NUnit;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;
using static TrialManager;

public class TrialManager : EnvironmentComponent
{
    public class ObjectState
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public EnvironmentComponent saveObject;
    }

    public EnvironmentCallback OnTrialOverCallbacks;
    public EnvironmentCallback OnTrialStartCallbacks;


    public int trialCounter = 0;
    [Tooltip("Total trials to use.")]
    public int maxTrials = 3;
    [Tooltip("Trial duration in seconds.")]
    public float trialResetUpdateFrequency = 10.0f;
    public float trialDurationCounter = 0;
    public float trialRealTimeCounter = 0.0f;

    float mStartTime;

    int mAgentCount = 0;
    List<ObjectState> mObjectStates = new List<ObjectState>();

    protected override void DoRegisterCallbacks()
    {
        base.DoRegisterCallbacks();
    }

    public override void InitParameters(JSONObject jsonParameters)
    {
        if (jsonParameters != null)
        {
            jsonParameters.GetField(out maxTrials, "trial_count", maxTrials);
            jsonParameters.GetField(out trialResetUpdateFrequency, "trial_seconds", trialResetUpdateFrequency);
        }

        base.InitParameters(jsonParameters);
    }

    public override void OnRunStarted()
    {
        Debug.Log("TrialManager");

        base.OnRunStarted();

        mObjectStates.Clear();
        EnvironmentComponent[] components = GetEngine().GetEnvironmentComponents();
        for (int i = 0; i < components.Length; i++)
        {
            ObjectState objectState = components[i].SaveTrialData();

            if (objectState != null)
            {
                mObjectStates.Add(objectState);
            }
        }

        trialCounter = 0;
        mStartTime = Time.realtimeSinceStartup;
        ResetTrialDurationCounter();

        if (OnTrialStartCallbacks != null)
        {
            OnTrialStartCallbacks();
        }
    }

    public bool IsTrialOver()
    {
        if (trialDurationCounter >= trialResetUpdateFrequency)
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

    public override void OnFixedUpdate(float deltaTime)
    {
        trialDurationCounter += deltaTime;
        CheckTrial();
        //Debug.Log(deltaTime);

        trialRealTimeCounter = Time.realtimeSinceStartup - mStartTime;

        base.OnFixedUpdate(deltaTime);
    }

    private void CheckTrial()
    {
        if (IsTrialOver())
        {
            if (OnTrialOverCallbacks != null)
            {
                OnTrialOverCallbacks();
            }

            IncrementTrialCounter();

            if (!IsMaxTrials())
            {
                StartNewTrial();
            }
            else if (IsMaxTrials())
            {
                var playerObject = GetEngine().GetEnvironmentComponent<U3DPlayer>("Player");
                playerObject.DoEndEpisode();
                Debug.Log("Max trials reached");
            }
        }
    }

    public void OnTrialOverCallback()
    {

    }

    private void StartNewTrial()
    {
        mStartTime = Time.realtimeSinceStartup;
        ResetTrialDurationCounter();

        if (OnTrialStartCallbacks != null)
        {
            OnTrialStartCallbacks();
        }

        ResetState();
    }

    public void ResetState()
    {
        foreach (ObjectState objectState in mObjectStates)
        {
            EnvironmentComponent resetObject = GetObject(objectState);

            if (resetObject != null)
            {
                resetObject.LoadTrialData(objectState);
            }
        }
    }

    EnvironmentComponent GetObject(ObjectState objectState)
    {
        EnvironmentComponent resetObject = null;

        // Create object is it's a prefab
        if (objectState.saveObject.GetEngine() == null)
        {
            EnvironmentObject productionRuleEnvironmentObject = GetEngine().CreateEnvironmentObject(objectState.saveObject.gameObject);
            resetObject = (EnvironmentComponent)productionRuleEnvironmentObject.GetComponent(objectState.saveObject.GetType());
        }
        else
        {
            resetObject = objectState.saveObject;
        }

        return resetObject;
    }
}