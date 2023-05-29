using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Defines ability of object to move about the world
[RequireComponent(typeof(Movement))]
public class Pushable : EnvironmentComponent
{
    // Private members
    Movement mMovement;

    Vector3 mOriginalTurnVelocity = Vector3.zero;
    bool mHadCollision = false;
    Dictionary<EnvironmentObject, Vector3> mDeltaVelocities = new Dictionary<EnvironmentObject, Vector3>();

    protected override void Initialize()
    {
        mMovement = GetComponent<Movement>();

        base.Initialize();
    }

    protected override void DoRegisterCallbacks()
    {
        if (mEngine is GridEnvironment)
        {
            RegisterCallback(ref ((GridEnvironment)mEngine).OnEndTurnCallbacks, OnEndTurn);
        }

        base.DoRegisterCallbacks();
    }

    virtual public void OnStartTurn()
    {
        mOriginalTurnVelocity = mMovement.Velocity;
    }

    virtual public void OnEndTurn()
    {
        //Debug.Log("OnEndTurn");

        if (mHadCollision)
        {
            mMovement.Velocity = mOriginalTurnVelocity;
        }
        mHadCollision = false;
    }

    /*public override void OnFixedUpdate(float fixedDeltaTime)
    {
        if (mDeltaVelocities.Count == 0)
        {
            mOriginalVelocity = mMovement.Velocity;
        }
        mDeltaVelocities.Clear();

        base.OnFixedUpdate(fixedDeltaTime);
    }

    public override void OnLateFixedUpdate(float fixedDeltaTime)
    {
        if (mDeltaVelocities.Count > 0)
        {
            foreach (var pair in mDeltaVelocities)
            {
                mMovement.Velocity += pair.Value;
            }
        }

        base.OnLateFixedUpdate(fixedDeltaTime);
    }*/

    public override void OnCollision(EnvironmentObject otherObject)
    {
        mHadCollision = true; 

        Movement otherMovement = otherObject.GetComponent<Movement>();

        if (otherMovement && otherMovement.Velocity != Vector3.zero)
        {
            mDeltaVelocities[otherObject] = otherMovement.Velocity;
        }

        base.OnCollision(otherObject);
    }

    public override void OnPostCollision(EnvironmentObject otherObject)
    {
        Movement otherMovement = otherObject.GetComponent<Movement>();

        if (otherMovement && mDeltaVelocities.ContainsKey(otherObject))
        {
            mMovement.Velocity = mDeltaVelocities[otherObject];
            otherMovement.Velocity = mDeltaVelocities[otherObject];
        }

        base.OnPostCollision(otherObject);
    }
}
