using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;
using Unity.Mathematics;

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

    public int trialCounter = 0;
    [Tooltip("Total trials to use.")]
    public int maxTrials = 3;
    [Tooltip("Trial duration in seconds.")]
    public float trialResetUpdateFrequency = 10.0f;
    public float trialDurationCounter = 0;

    int mAgentCount = 0;
    List<ObjectState> mObjectStates = new List<ObjectState>();


    public override void OnRunStarted()
    {
        Debug.Log("TrialManager");

        base.OnRunStarted();

        EnvironmentComponent[] components = GetEngine().GetEnvironmentComponents();
        for (int i = 0; i < components.Length; i++)
        {
            ObjectState objectState = components[i].SaveTrialData();

            if (objectState != null)
            {
                mObjectStates.Add(objectState);
            }
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

    public override void OnUpdate(float deltaTime)
    {
        trialDurationCounter += deltaTime;
        CheckTrial();

        base.OnUpdate(deltaTime);
    }

    private void CheckTrial()
    {
        if (IsTrialOver())
        {
            if (OnTrialOverCallbacks != null)
            {
                OnTrialOverCallbacks();
            }

            if (!IsMaxTrials())
            {
                StartNewTrial();
            }
            else if (IsMaxTrials())
            {
                //Debug.Log("Max trials reached");
            }
        }
    }

    public void OnTrialOverCallback()
    {

    }

    private void StartNewTrial()
    {
        IncrementTrialCounter();
        ResetTrialDurationCounter();
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