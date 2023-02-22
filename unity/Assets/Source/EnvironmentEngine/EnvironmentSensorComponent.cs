using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using UnityEngine.Networking;
using Unity.MLAgents.Sensors;

public class EnvironmentSensorComponent : SensorComponent
{
    EnvironmentBrain mSensor;

    public override ISensor CreateSensor()
    {
        mSensor = GetComponent<EnvironmentBrain>();
        return mSensor;
    }

    public override int[] GetObservationShape()
    {
        return mSensor.GetObservationShape();
    }
}
