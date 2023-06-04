using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;

public class U3SensorComponent : SensorComponent
{
    public List<ISensor> sensors;

    public U3SensorComponent()
    {
        sensors = new List<ISensor>();
    }

    public void AddSensorInfo(SensorInfo sensorInfo)
    {
        sensors.Add(sensorInfo);
    }

    public override ISensor[] CreateSensors()
    {
        return sensors.ToArray();
    }
}
