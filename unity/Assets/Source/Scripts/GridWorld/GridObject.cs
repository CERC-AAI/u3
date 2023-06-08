using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class GridObject : EnvironmentObject
{
    Rigidbody2D mRigidbody2D;


    //public members
    public Vector2Int Position
    {
        get 
        { 
            return new Vector2Int(Mathf.RoundToInt(transform.localPosition.x), Mathf.RoundToInt(transform.localPosition.y)); 
        }
        set 
        {
            Vector3 currentPosition = transform.localPosition;

            currentPosition.x = value.x;
            currentPosition.x = value.y;

            transform.localPosition = currentPosition;
        }
    }

    public float Rotation
    {
        get 
        {
            return transform.localRotation.eulerAngles.z; 
        }
        set
        {
            Quaternion currentRotation = transform.localRotation;
            Vector3 eulerAngles = currentRotation.eulerAngles;

            eulerAngles.z = value;

            currentRotation.eulerAngles = eulerAngles;

            transform.localRotation = currentRotation;
        }
    }

    protected override void Initialize()
    {
        mRigidbody2D = GetComponent<Rigidbody2D>();

        base.Initialize();
    }

    override public void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
    }

    /*public override bool OnActionRecieved(float[] vectorAction)
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
    }*/

    /*public override void OnCollision(EnvironmentObject otherObject)
    {
        if (otherObject.name.Contains("Wall"))
        {
            mEngine.AddReward(-0.01f);
        }

        base.OnCollision(otherObject);
    }*/
}
