using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class FrogGame : EnvironmentBrain
{
    public enum Actions
    {
        NOOP = 0,
        RIGHT,
        UP,
        LEFT,
        DOWN,
    }

    public override void OnEnvironmentActionReceived(float[] vectorAction)
    {
        base.OnEnvironmentActionReceived(vectorAction);
    }

    public override void OnTurnEnd()
    {
        AddReward(-0.01f);

        base.OnTurnEnd();
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = (int)Actions.NOOP;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            actionsOut[0] = (int)Actions.RIGHT;
        }
        else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            actionsOut[0] = (int)Actions.UP;
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            actionsOut[0] = (int)Actions.LEFT;
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            actionsOut[0] = (int)Actions.DOWN;
        }
    }
}
