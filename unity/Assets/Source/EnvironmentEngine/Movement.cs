using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Defines ability of object to move about the world
public class Movement : EnvironmentComponent
{
    // Public members
    public Vector3 Velocity
    {
        get { return GetVelocity(); }
        set { SetVelocity(mEngine.ApplyEnvironmentVelocity(value)); }
    }

    public Vector3 AngularVelocity
    {
        get { return GetAngularVelocity(); }
        set { SetAngularVelocity(mEngine.ApplyEnvironmentAngularVelocity(value)); }
    }

    public EnvironmentCallback<Vector3> OnVelocityChangedCallbacks;

    // Private members
    Rigidbody mRigidbody;
    Rigidbody2D mRigidbody2D;
    //GraphicsObject mGraphicsObject;

    Vector3 mVelocity;
    Vector3 mAngularVelocity;

    Vector3 mPreviousVelocity;

    override protected void Initialize()
    {
        mRigidbody = GetComponent<Rigidbody>();
        mRigidbody2D = GetComponent<Rigidbody2D>();
        //mGraphicsObject = GetComponent<GraphicsObject>();

        mVelocity = Vector3.zero;
        mAngularVelocity = Vector3.zero;
        mPreviousVelocity = Vector3.zero;

        if (mRigidbody)
        {
            mRigidbody.velocity = Vector3.zero;
        }
        if (mRigidbody2D)
        {
            mRigidbody2D.velocity = Vector2.zero;
        }

        base.Initialize();
    }

    public override void OnFixedUpdate(float fixedDeltaTime)
    {
        if (mRigidbody && mRigidbody.isKinematic)
        {
            mParentObject.Position += mVelocity * fixedDeltaTime;
        }
        else if (mRigidbody2D && mRigidbody2D.bodyType == RigidbodyType2D.Kinematic)
        {
            mParentObject.Position += mVelocity * fixedDeltaTime;
        }

        base.OnFixedUpdate(fixedDeltaTime);
    }

    public void SetVelocity(Vector3 targetVelocity)
    {
        if (mRigidbody && !mRigidbody.isKinematic)
        {
            mRigidbody.velocity = targetVelocity;
        }
        else if (mRigidbody2D && mRigidbody2D.bodyType == RigidbodyType2D.Dynamic)
        {
            mRigidbody2D.velocity = targetVelocity;
        }

        if (targetVelocity != mVelocity)
        {
            if (OnVelocityChangedCallbacks != null)
            {
                OnVelocityChangedCallbacks(targetVelocity);
            }

            mVelocity = targetVelocity;
        }

        /*if (mGraphicsObject)
        {
            mGraphicsObject.SetFacing(targetVelocity);
        }*/
    }

    public Vector3 GetVelocity()
    {
        if (mRigidbody && !mRigidbody.isKinematic)
        {
            return mRigidbody.velocity;
        }
        else if (mRigidbody2D && mRigidbody2D.bodyType == RigidbodyType2D.Dynamic)
        {
            return mRigidbody2D.velocity;
        }
        else
        {
            return mVelocity;
        }
    }

    public void SetAngularVelocity(Vector3 targetVelocity)
    {
        if (mRigidbody && !mRigidbody.isKinematic)
        {
            mRigidbody.angularVelocity = targetVelocity;
        }
        else if (mRigidbody2D && mRigidbody2D.bodyType == RigidbodyType2D.Dynamic)
        {
            mRigidbody2D.angularVelocity = targetVelocity.z;
        }
        else
        {
            mAngularVelocity = targetVelocity;
        }
    }

    public Vector3 GetAngularVelocity()
    {
        if (mRigidbody && !mRigidbody.isKinematic)
        {
            return mRigidbody.velocity;
        }
        else if (mRigidbody2D && mRigidbody2D.bodyType == RigidbodyType2D.Dynamic)
        {
            return new Vector3 (0, 0, mRigidbody2D.angularVelocity);
        }
        else
        {
            return mAngularVelocity;
        }
    }

    public void MoveTowards(Vector3 targetPosition)
    {
        SetVelocity(targetPosition - mParentObject.Position);
    }

    public void MoveTo(Vector3 targetPosition)
    {
        mParentObject.Position = targetPosition;
    }
}
