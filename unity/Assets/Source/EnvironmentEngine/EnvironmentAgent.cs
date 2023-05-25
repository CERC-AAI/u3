using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using System.Reflection;

public class IntActionInfo : ActionInfo
{

    override public void setDiscreteActions(List<int> values)
    {
        mFieldInfo.SetValue(mParent, values[0]);
    }

    override public void encodeDiscreteActions(List<int> values)
    {
        values[0] = (int)mFieldInfo.GetValue(mParent);
    }

    override public int getIntegerCount()
    {
        return 1;
    }
}

public class BooleanActionInfo : ActionInfo
{
    public override void init()
    {
        mMinValue = 0;
        mMaxValue = 1;

        base.init();
    }

    override public void setDiscreteActions(List<int> values)
    {
        if (values[0] == 1)
        {
            mFieldInfo.SetValue(mParent, true);
        }
        else
        {
            mFieldInfo.SetValue(mParent, false);
        }
    }

    override public void encodeDiscreteActions(List<int> values)
    {
        bool value = (bool)mFieldInfo.GetValue(mParent);

        if (value)
        {
            values[0] = 1;
        }
        else
        {
            values[0] = 0;
        }
    }

    override public int getIntegerCount()
    {
        return 1;
    }
}

public class EnumActionInfo : ActionInfo
{
    public override void init()
    {
        mMinValue = 0;
        mMaxValue = mFieldInfo.FieldType.GetEnumValues().Length;
    }

    override public void setDiscreteActions(List<int> values)
    {
        mFieldInfo.SetValue(mParent, mFieldInfo.FieldType.GetEnumValues().GetValue(values[0]));
    }

    override public void encodeDiscreteActions(List<int> values)
    {
        values[0] = (int)mFieldInfo.GetValue(mParent);
    }

    override public int getIntegerCount()
    {
        return 1;
    }
}

public class Vector2IntActionInfo : ActionInfo
{
    override public void setDiscreteActions(List<int> values)
    {
        mFieldInfo.SetValue(mParent, new Vector2Int(values[0], values[1]));
    }

    override public void encodeDiscreteActions(List<int> values)
    {
        Vector2Int value = (Vector2Int)mFieldInfo.GetValue(mParent);

        values[0] = value.x;
        values[1] = value.y;
    }

    override public int getIntegerCount()
    {
        return 2;
    }
}

public class Vector3IntActionInfo : ActionInfo
{
    override public void setDiscreteActions(List<int> values)
    {
        mFieldInfo.SetValue(mParent, new Vector3Int(values[0], values[1], values[2]));
    }

    override public void encodeDiscreteActions(List<int> values)
    {
        Vector3Int value = (Vector3Int)mFieldInfo.GetValue(mParent);

        values[0] = value.x;
        values[1] = value.y;
        values[2] = value.z;
    }

    override public int getIntegerCount()
    {
        return 3;
    }
}

public class FloatActionInfo : ActionInfo
{
    override public void setContinuousActions(List<float> values)
    {
        mFieldInfo.SetValue(mParent, values[0]);
    }

    override public void encodeContinuousActions(List<float> values)
    {
        values[0] = (float)mFieldInfo.GetValue(mParent);
    }

    override public int getFloatCount()
    {
        return 1;
    }
}

public class Vector2ActionInfo : ActionInfo
{
    override public void setContinuousActions(List<float> values)
    {
        mFieldInfo.SetValue(mParent, new Vector2(values[0], values[1]));
    }

    override public void encodeContinuousActions(List<float> values)
    {
        Vector2 value = (Vector2)mFieldInfo.GetValue(mParent);

        values[0] = value.x;
        values[1] = value.y;
    }

    override public int getFloatCount()
    {
        return 2;
    }
}

public class Vector3ActionInfo : ActionInfo
{
    override public void setContinuousActions(List<float> values)
    {
        mFieldInfo.SetValue(mParent, new Vector3(values[0], values[1], values[2]));
    }

    override public void encodeContinuousActions(List<float> values)
    {
        Vector3 value = (Vector3)mFieldInfo.GetValue(mParent);

        values[0] = value.x;
        values[1] = value.y;
        values[2] = value.z;
    }

    override public int getFloatCount()
    {
        return 3;
    }
}

public class ActionInfo
{
    public const float DEFAULT_VALUE_RANGE = 10000;

    public static Dictionary<Type, Type> mActionInfoMap = new Dictionary<Type, Type>()
    {
        [typeof(int)] = typeof(IntActionInfo),
        [typeof(float)] = typeof(FloatActionInfo),
    };

    public delegate void Heuristic();

    protected FieldInfo mFieldInfo;
    protected object mParent;
    protected float mMinValue = -DEFAULT_VALUE_RANGE;
    protected float mMaxValue = DEFAULT_VALUE_RANGE;

    public void setBaseValues(FieldInfo fieldInfo, object parent, Action action)
    {
        mFieldInfo = fieldInfo;
        mParent = parent;

        mMinValue = action.minValue;
        mMaxValue = action.maxValue;

        init();
    }

    public virtual void init()
    {

    }

    virtual public void setDiscreteActions(List<int> values)
    {
        Debug.LogError("Please implement setDiscreteActions function for class: " + GetType());
    }

    virtual public void setContinuousActions(List<float> values)
    {
        Debug.LogError("Please implement setContinuousActions function for class: " + GetType());
    }

    virtual public void encodeDiscreteActions(List<int> values)
    {
        Debug.LogError("Please implement encodeDiscreteActions function for class: " + GetType());
    }

    virtual public void encodeContinuousActions(List<float> values)
    {
        Debug.LogError("Please implement encodeContinuousActions function for class: " + GetType());
    }

    virtual public int getIntegerCount()
    {
        return 0;
    }

    virtual public int getFloatCount()
    {
        return 0;
    }

    virtual public float getMinValue()
    {
        return mMinValue;
    }

    virtual public float getMaxValue()
    {
        return mMaxValue;
    }
}

[RequireComponent(typeof(U3Agent))]
public class EnvironmentAgent : EnvironmentComponent
{

    U3Agent mAgentScript;
    BehaviorParameters mBehaviorParameters;

    public List<ActionInfo> mActions = new List<ActionInfo>();

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
                mActions[i].setDiscreteActions(currentDiscreteBuffer);
            }
            if (currentContinuousBuffer.Count > 0)
            {
                mActions[i].setContinuousActions(currentContinuousBuffer);
            }
        }
    }

    virtual public void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;

        List<int> currentDiscreteBuffer = new List<int>();
        List<float> currentContinuousBuffer = new List<float>();
        int discreteOffset = 0;
        int continuousOffset = 0;

        for (int i = 0; i < mActions.Count; i++)
        {
            currentDiscreteBuffer.Clear();
            currentContinuousBuffer.Clear();

            if (mActions[i].getIntegerCount() > 0)
            {
                for (int j = 0; j < mActions[i].getIntegerCount(); j++)
                {
                    currentDiscreteBuffer.Add(0);
                }

                mActions[i].encodeDiscreteActions(currentDiscreteBuffer);
            }
            if (mActions[i].getFloatCount() > 0)
            {
                for (int j = 0; j < mActions[i].getFloatCount(); j++)
                {
                    currentContinuousBuffer.Add(0);
                }

                mActions[i].encodeContinuousActions(currentContinuousBuffer);
            }

            for (int j = 0; j < currentDiscreteBuffer.Count; j++)
            {
                discreteActions[discreteOffset + j] = currentDiscreteBuffer[j];
            }
            for (int j = 0; j < currentContinuousBuffer.Count; j++)
            {
                continuousActions[continuousOffset + j] = currentContinuousBuffer[j];
            }

            discreteOffset += mActions[i].getIntegerCount();
            continuousOffset += mActions[i].getFloatCount();
        }
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
