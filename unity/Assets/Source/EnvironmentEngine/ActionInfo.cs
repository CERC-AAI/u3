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

    override public void setActions(List<int> values)
    {
        mFieldInfo.SetValue(mParent, values[0]);
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
        mMaxValue = 2;

        base.init();
    }

    override public void setActions(List<int> values)
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

    override public void setActions(List<int> values)
    {
        mFieldInfo.SetValue(mParent, mFieldInfo.FieldType.GetEnumValues().GetValue(values[0]));
    }

    override public int getIntegerCount()
    {
        return 1;
    }
}

public class Vector2IntActionInfo : ActionInfo
{
    override public void setActions(List<int> values)
    {
        mFieldInfo.SetValue(mParent, new Vector2Int(values[0], values[1]));
    }

    override public int getIntegerCount()
    {
        return 2;
    }
}

public class Vector3IntActionInfo : ActionInfo
{
    override public void setActions(List<int> values)
    {
        mFieldInfo.SetValue(mParent, new Vector3Int(values[0], values[1], values[2]));
    }
    override public int getIntegerCount()
    {
        return 3;
    }
}

public class FloatActionInfo : ActionInfo
{
    override public void setActions(List<float> values)
    {
        mFieldInfo.SetValue(mParent, values[0]);
    }

    override public int getFloatCount()
    {
        return 1;
    }
}

public class Vector2ActionInfo : ActionInfo
{
    override public void setActions(List<float> values)
    {
        mFieldInfo.SetValue(mParent, new Vector2(values[0], values[1]));
    }

    override public int getFloatCount()
    {
        return 2;
    }
}

public class Vector3ActionInfo : ActionInfo
{
    override public void setActions(List<float> values)
    {
        mFieldInfo.SetValue(mParent, new Vector3(values[0], values[1], values[2]));
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

    protected FieldInfo mFieldInfo;
    protected object mParent;
    protected float mMinValue = -DEFAULT_VALUE_RANGE;
    protected float mMaxValue = DEFAULT_VALUE_RANGE;

    public void setBaseValues(FieldInfo fieldInfo, object parent, ACTION action)
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

    virtual public void setActions(List<int> values)
    {
        Debug.LogError("Please implement setDiscreteActions function for class: " + GetType());
    }

    virtual public void setActions(List<float> values)
    {
        Debug.LogError("Please implement setContinuousActions function for class: " + GetType());
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
