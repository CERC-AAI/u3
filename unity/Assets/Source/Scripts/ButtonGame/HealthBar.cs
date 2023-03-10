using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : EnvironmentComponent
{
    public float HP
    {
        get { return currentHP; }
        set
        {
            float originalHP = HP;

            currentHP = value;
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);

            if (mHealthBar)
            {
                mHealthBar.SetPercent(currentHP / maxHP);
            }

            if (originalHP > 0 && HP <= 0)
            {
                if (OnDiedCallbacks != null)
                {
                    OnDiedCallbacks();
                }
                //SendMessage("OnDied");
            }
        }
    }

    public float MaxHP
    {
        get { return maxHP; }
        set
        {
            maxHP = value;
            HP = currentHP;

            if (mHealthBar)
            {
                mHealthBar.SetPercent(currentHP / maxHP);
            }
        }
    }

    public float HealthDrainRate
    {
        get { return healthDrainRate; }
        set { healthDrainRate = value; }
    }

    public EnvironmentCallback OnDiedCallbacks;

    [Tooltip("Maximum health the object can have.")]
    public float maxHP = 20;
    [Tooltip("Current health of the agent. Calls 'OnDiedCallbacks' to components when health reaches 0.")]
    public float currentHP = 20;
    [Tooltip("Drain rate is per second, or per turn if in a GridEnvironment.")]
    public float healthDrainRate = 0.2f;

    LoadBar mHealthBar;
    bool mIsPerTurn = false;

    protected override void Initialize()
    {
        base.Initialize();

        if (mEngine is GridEnvironment)
        {
            mIsPerTurn = true;
        }

        mHealthBar = GetComponentInChildren<LoadBar>();
    }

    protected override void DoRegisterCallbacks()
    {
        if (mEngine is GridEnvironment)
        {
            RegisterCallback(ref ((GridEnvironment)mEngine).OnEndTurnCallbacks, OnEndTurn);
        }

        base.DoRegisterCallbacks();
    }

    virtual public void OnEndTurn()
    {
        HP -= HealthDrainRate;
    }


    public override void OnRunStarted()
    {
        base.OnRunStarted();

        HP = MaxHP;
    }

    public override void OnFixedUpdate(float fixedDeltaTime)
    {
        if (!mIsPerTurn)
        {
            HP -= HealthDrainRate * fixedDeltaTime;
        }        

        base.OnFixedUpdate(fixedDeltaTime);
    }

    /*protected override void BuildRunStateJSON(JSONObject root)
    {
        root["hp"] = new JSONObject(currentHP);
        root["maxhp"] = new JSONObject(maxHP);

        base.BuildRunStateJSON(root);
    }

    protected override void LoadRunStateJSON(JSONObject root)
    {
        if (root.keys.Contains("hp"))
        {
            currentHP = root["hp"].n;
        }
        if (root.keys.Contains("maxhp"))
        {
            maxHP = root["maxhp"].n;
        }

        if (mHealthBar)
        {
            mHealthBar.SetPercent(currentHP / maxHP);
        }

        base.LoadRunStateJSON(root);
    }*/
}
