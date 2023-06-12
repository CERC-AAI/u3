using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;
using System.Reflection;

public abstract class SensorInfo : ISensor
{
    protected FieldInfo mFieldInfo;
    protected object mParent;
    protected ObservationSpec mObservationSpec;
    protected string mName;


    public SensorInfo()
    {
    }

    // TODO: fix this with primitive types, specifically to map from C# primitives to SensorInfo
    // TODO: only need this for ints and bools since we have the autonaming convention
    public static Dictionary<Type, Type> mSensorInfoMap = new Dictionary<Type, Type>()
    {
    {typeof(Vector3), typeof(Vector3SensorInfo)},
    {typeof(Quaternion), typeof(QuaternionSensorInfo)},
    {typeof(Camera), typeof(CameraSensorInfo)},
    };


    public bool setBaseValues(FieldInfo fieldInfo, object parent, Sensor sensorAttribute)
    {
        bool wasInitialized = false;

        mFieldInfo = fieldInfo;
        mParent = parent;  // Get the EnvironmentAgent component
                           // Now you can use sensorAttribute as you need
        mName = sensorAttribute.sensorName;
        wasInitialized = init(sensorAttribute);

        if (wasInitialized)
        {
            mObservationSpec = setObservationSpec();
        }

        return wasInitialized;
    }


    public virtual bool init(Sensor sensorAttribute)
    {
        return true;
    }

    public abstract ObservationSpec setObservationSpec();

    public virtual ObservationSpec GetObservationSpec()
    {
        return mObservationSpec;
    }

    public abstract int Write(ObservationWriter writer);

    public virtual byte[] GetCompressedObservation()
    {
        return null;
    }

    public virtual void Update()
    {

    }

    public virtual void Reset()
    {

    }

    public virtual CompressionSpec GetCompressionSpec()
    {
        return CompressionSpec.Default();
    }

    public virtual string GetName()
    {
        if (mName == null)
        {
            return mFieldInfo.Name;
        }
        else
        {
            return mName;
        }
    }
}

public class Vector3SensorInfo : SensorInfo
{
    public override ObservationSpec setObservationSpec()
    {
        return ObservationSpec.Vector(3);
    }

    public override int Write(ObservationWriter writer)
    {
        Vector3 observation = (Vector3)mFieldInfo.GetValue(mParent);
        List<float> observation_list = new List<float> { observation.x, observation.y, observation.z };
        writer.AddList(observation_list);
        Debug.Log("Vector3SensorInfo.Write: " + observation);
        return observation_list.Count;
    }
}

public class QuaternionSensorInfo : SensorInfo
{
    public override ObservationSpec setObservationSpec()
    {
        return ObservationSpec.Vector(4);
    }

    public override int Write(ObservationWriter writer)
    {
        Quaternion observation = (Quaternion)mFieldInfo.GetValue(mParent);
        List<float> observation_list = new List<float> { observation.x, observation.y, observation.z, observation.w };
        writer.AddList(observation_list);
        return observation_list.Count;
    }

}

public class CameraSensorInfo : SensorInfo
{
    CameraSensor mCameraSensor;

    //If the values are set you pass them into the CameraSensor constructor in init(Sensor sensorAttribute)
    // if they're not set, you use the FieldInfo to get the Camera object, and query it for good default values


    public override bool init(Sensor sensorAttribute)
    {
        Camera myCamera = (Camera)mFieldInfo.GetValue(mParent);

        if (myCamera == null)
        {
            return false;
        }

        mName = sensorAttribute.sensorName;
        bool grayscale = sensorAttribute.grayscale;
        SensorCompressionType compression = sensorAttribute.compressionType;

        int width;
        int height;

        if (sensorAttribute.width is not 0)
        {
            width = sensorAttribute.width;
            height = sensorAttribute.height;
        }
        else
        {
            // TODO: test the defaults here
            width = myCamera.pixelWidth;
            height = myCamera.pixelHeight;
        }

        mCameraSensor = new CameraSensor(myCamera, width, height, grayscale, mName, compression);

        return true;
    }

    public override ObservationSpec setObservationSpec()
    {
        mObservationSpec = mCameraSensor.GetObservationSpec();
        //Debug.Log("CameraSensorInfo.setObservationSpec: " + mObservationSpec);
        return mObservationSpec;
    }

    public override int Write(ObservationWriter writer)
    {
        //Debug.Log("CameraSensorInfo.Write: " + mCameraSensor.GetCompressedObservation());
        return mCameraSensor.Write(writer);

    }

    public override ObservationSpec GetObservationSpec()
    {
        return mObservationSpec;
    }

    public override byte[] GetCompressedObservation()
    {
        return mCameraSensor.GetCompressedObservation();
    }

    public override void Update()
    {
        mCameraSensor.Update();
    }

    public override void Reset()
    {
        mCameraSensor.Reset();
    }

    public override CompressionSpec GetCompressionSpec()
    {
        return mCameraSensor.GetCompressionSpec();
    }

}
