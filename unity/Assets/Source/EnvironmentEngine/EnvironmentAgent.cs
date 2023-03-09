using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;

[RequireComponent(typeof(U3Agent))]
public class EnvironmentAgent : EnvironmentComponent
{
    U3Agent mAgentScript;

    protected override void Initialize()
    {
        mAgentScript = GetComponent<U3Agent>();

        base.Initialize();
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
    }

    virtual public void Heuristic(in ActionBuffers actionsOut)
    {
        Debug.Log("Heuristic");
    }

    virtual public void DoEndEpisode()
    {
        mEngine.AgentEndedEpisode(this);
    }

}
