using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;

[RequireComponent(typeof(Movement))]
public class GridPlayer : EnvironmentAgent
{
    //public members
    public float tilesPerSecondSpeed = 1;
    public bool randomizeStartPosition = true;

    GridEnvironment mGridEngine;


    //private members
    Movement mMovement;
    HealthBar mHealthBar;

    public float MaxSpeed
    {
        get { return tilesPerSecondSpeed; }
        set { tilesPerSecondSpeed = value; }
    }

    public override void AppendActionLists(List<ActionInfo> actions)
    {
        ActionInfo basicMovement = new ActionInfo(DoMovement, (int)GridEnvironment.Actions.NOOP);

        actions.Add(basicMovement);

        base.AppendActionLists(actions);
    }

    override protected void Initialize()
    {
        mMovement = GetComponent<Movement>();
        mHealthBar = GetComponent<HealthBar>();
        mGridEngine = GetComponentInParent<GridEnvironment>();

        base.Initialize();
    }

    public override void OnRunStarted()
    {
        if (randomizeStartPosition)
        {
            mParentObject.Position = mGridEngine.GetRandomPosition();
        }

        mMovement.SetVelocity(Vector3.zero);

        base.OnRunStarted();
    }

    public override void OnFixedUpdate(float fixedDeltaTime)
    {
        if (mHealthBar && mHealthBar.currentHP <= 0)
        {
            //mEngine.Defeat();
        }

        base.OnFixedUpdate(fixedDeltaTime);
    }

    public override bool ShouldRequestDecision(long fixedUdpateNumber)
    {
        if (mGridEngine)
        {
            return mGridEngine.IsEndOfTurn();
        }

        return base.ShouldRequestDecision(fixedUdpateNumber);
    }

    override public bool ShouldBlockDecision(ActionBuffers actions)
    {
        return actions.DiscreteActions[0] == (int)GridEnvironment.Actions.NOOP;
    }

    override public void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
    }

    protected override void OnDied()
    {
        base.OnDied();

        mMovement.Velocity = Vector2.zero;
    }

    void DoMovement(int actionValue)
    {
        float speed = tilesPerSecondSpeed;
        if (mGridEngine)
        {
            speed = tilesPerSecondSpeed * mGridEngine.gridSize / mGridEngine.GetTurnTime();
        }

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

    /*override public void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);

        
    }*/

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

    /*public override void OnCollision(EnvironmentObject otherObject)
    {
        if (otherObject.name.Contains("Wall"))
        {
            mEngine.AddReward(-0.01f);
        }

        base.OnCollision(otherObject);
    }*/
}
