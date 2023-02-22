using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Movement))]
public class GridPlayer : Brain
{
    //public members
    public float speed = 3;    

    public float MaxSpeed
    {
        get { return speed; }
        set { speed = value;  }
    }

    //private members
    Movement mMovement;
    HealthBar mHealthBar;

    override protected void Init()
    {
        mMovement = GetComponent<Movement>();
        mHealthBar = GetComponent<HealthBar>();

        base.Init();
    }

    public override void OnLevelLoaded()
    {
        mMovement.SetVelocity(Vector3.zero);

        base.OnLevelLoaded();
    }

    public override void OnFixedUpdate(bool isEndOfTurn)
    {
        if (mHealthBar && mHealthBar.currentHP <= 0)
        {
            mEngine.Defeat();
        }

        base.OnFixedUpdate(isEndOfTurn);
    }

    override public void OnUpdate(bool isEndOfTurn)
    {
    }

    public override bool OnActionRecieved(float[] vectorAction)
    {
        var action = Mathf.FloorToInt(vectorAction[0]);

        switch ((GridEnvironment.Actions)action)
        {
            case GridEnvironment.Actions.NOOP:
                mMovement.SetVelocity(mEngine.GetMoveVelocity(Vector2.zero, speed));
                return false;

            case GridEnvironment.Actions.UP:
                mMovement.SetVelocity(mEngine.GetMoveVelocity(Vector2.up, speed));
                return true;

            case GridEnvironment.Actions.LEFT:
                mMovement.SetVelocity(mEngine.GetMoveVelocity(Vector2.left, speed));
                return true;

            case GridEnvironment.Actions.DOWN:
                mMovement.SetVelocity(mEngine.GetMoveVelocity(Vector2.down, speed));
                return true;

            case GridEnvironment.Actions.RIGHT:
                mMovement.SetVelocity(mEngine.GetMoveVelocity(Vector2.right, speed));
                return true;

            default:
                throw new ArgumentException("Invalid action value");
        }
    }

    public override void OnCollision(EnvironmentObject otherObject)
    {
        if (otherObject.name.Contains("Wall"))
        {
            mEngine.AddReward(-0.01f);
        }

        base.OnCollision(otherObject);
    }
}
