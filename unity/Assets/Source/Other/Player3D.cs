using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;

[RequireComponent(typeof(Movement))]
public class Player3D : EnvironmentAgent
{
    //public members
    public float tilesPerSecondSpeed = 1;
    public bool randomizeStartPosition = true;

    //private members
    Movement mMovement;
    HealthBar mHealthBar;

    [Action((int)GridEnvironment.Actions.NOOP)]
    public ActionInfo.DiscreteAction doMovement;

    override protected void Initialize()
    {
        mMovement = GetComponent<Movement>();
        mHealthBar = GetComponent<HealthBar>();

        base.Initialize();

        doMovement = new ActionInfo.DiscreteAction(DoMovement);
    }

    public override void OnRunStarted()
    {
        mMovement.SetVelocity(Vector3.zero);

        base.OnRunStarted();
    }

    public override bool ShouldRequestDecision(long fixedUdpateNumber)
    {

        return base.ShouldRequestDecision(fixedUdpateNumber);
    }

    override public bool ShouldBlockDecision(ActionBuffers actions)
    {
        return false;// actions.DiscreteActions[0] == (int)GridEnvironment.Actions.NOOP;
    }

    protected override void OnDied()
    {
        base.OnDied();

        mMovement.Velocity = Vector2.zero;
    }

    void DoMovement(int actionValue)
    {
        float speed = tilesPerSecondSpeed;

        switch ((GridEnvironment.Actions)actionValue)
        {
            case GridEnvironment.Actions.NOOP:
                mMovement.Velocity = Vector2.zero * speed;
                break;

            case GridEnvironment.Actions.UP:
                mMovement.Velocity = Vector2.up * speed;
                break;

            case GridEnvironment.Actions.LEFT:
                mMovement.Velocity = Vector2.left * speed;
                break;

            case GridEnvironment.Actions.DOWN:
                mMovement.Velocity = Vector2.down * speed;
                break;

            case GridEnvironment.Actions.RIGHT:
                mMovement.Velocity = Vector2.right * speed;
                break;

            default:
                throw new ArgumentException("Invalid action value");
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        discreteActions[0] = (int)GridEnvironment.Actions.NOOP;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            discreteActions[0] = (int)GridEnvironment.Actions.RIGHT;
            return;
        }
        else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            discreteActions[0] = (int)GridEnvironment.Actions.UP;
            return;
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActions[0] = (int)GridEnvironment.Actions.LEFT;
            return;
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            discreteActions[0] = (int)GridEnvironment.Actions.DOWN;
            return;
        }
    }
}
