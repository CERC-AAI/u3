using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : EnvironmentComponent
{
    public float maxHP = 20;
    public float currentHP = 20;

    LoadBar mHealthBar;

    protected override void Init()
    {
        mHealthBar = GetComponentInChildren<LoadBar>();

        base.Init();
    }

    public void addHP(float amount)
    {
        currentHP = Math.Min(maxHP, currentHP + amount);

        if (mHealthBar)
        {
            mHealthBar.SetPercent(currentHP / maxHP);
        }
    }


    public void subHP(float amount)
    {
        currentHP = Math.Max(0, currentHP - amount);

        if (mHealthBar)
        {
            mHealthBar.SetPercent(currentHP / maxHP);
        }
    }

    public void setHP(float amount)
    {
        currentHP = amount;

        if (mHealthBar)
        {
            mHealthBar.SetPercent(currentHP / maxHP);
        }
    }

    public void setMaxHP(float amount)
    {
        maxHP = amount;

        if (mHealthBar)
        {
            mHealthBar.SetPercent(currentHP / maxHP);
        }
    }

    protected override void BuildRunStateJSON(JSONObject root)
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
    }
}
