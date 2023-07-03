using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : U3DKinematicObject
{
    public override void EvaluateAtTime(double time)
    {
        Vector3 position = transform.position;
        position.y = -Mathf.Cos((float)time / 10 * 2 * Mathf.PI) * 2 + 2;
        transform.position = position;

        //Debug.Log("Move elevator");

        base.EvaluateAtTime(time);
    }
}
