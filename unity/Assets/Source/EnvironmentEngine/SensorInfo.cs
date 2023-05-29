using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;
using System.Reflection;


public class SensorInfo
{
    protected FieldInfo mFieldInfo;
    protected GameObject mParent;

    public static Dictionary<Type, Type> mSensorInfoMap = new Dictionary<Type, Type>()
    {
        // Add mappings here like this:
        // [typeof(YourType)] = typeof(YourSensorInfoSubclass),
    };


    public void setBaseValues(FieldInfo fieldInfo, GameObject parent, Sensor sensorAttribute)
    {
        mFieldInfo = fieldInfo;
        mParent = parent;
        // Now you can use sensorAttribute as you need
        init();
    }

    public virtual void init()
    {
        // Any initialization code goes here
    }

    virtual public void CollectObservations(VectorSensor sensor)
    {
        // Any sensor-specific code goes here
    }

    public virtual ISensor CreateSensor()
    {
        // This method should be overridden by subclasses.
        throw new NotImplementedException("CreateSensor() must be implemented in subclasses of SensorInfo");
    }

}

public class PositionSensorInfo : SensorInfo
{
    public override void init()
    {
        // Any position-specific initialization code goes here
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Transform transform = (Transform)mFieldInfo.GetValue(mParent);
        sensor.AddObservation(transform.localPosition);
    }

    public override ISensor CreateSensor()
    {
        // Create and return a sensor that observes the local position.
        return new VectorSensor(3); // This is just an example, you should replace this with the actual code to create the sensor.
    }

}

public class RotationSensorInfo : SensorInfo
{
    public override void init()
    {
        // Any rotation-specific initialization code goes here
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Transform transform = (Transform)mFieldInfo.GetValue(mParent);
        sensor.AddObservation(transform.localRotation);
    }

    public override ISensor CreateSensor()
    {
        // Create and return a sensor that observes the local rotation.
        return new VectorSensor(4); // This is just an example, you should replace this with the actual code to create the sensor.
    }

}
