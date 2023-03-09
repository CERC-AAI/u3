using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicsObject : EnvironmentComponent
{
    public Transform mInterpolateRoot;
    public float interpolationSpeed = 1.0f;
    public bool syncRotation = true;

    MeshRenderer[] mMeshes;

    //This tracks how much interpolation has occured
    float mInterpolatedAmount = 0;

    Vector3 mPreviousWorldPosition;
    Quaternion mPreviousWorldRotation;

    Vector3 mPreviousGraphicsPosition;
    Quaternion mPreviousGraphicsRotation;

    protected override void Initialize()
    {
        mMeshes = GetComponentsInChildren<MeshRenderer>();

        mInterpolatedAmount = 0;

        base.Initialize();
    }

    public override void OnRunStarted()
    {
        mPreviousWorldPosition = transform.position;
        mPreviousWorldRotation = transform.rotation;
        if (mInterpolateRoot)
        {
            mPreviousGraphicsPosition = mInterpolateRoot.transform.position;
            mPreviousGraphicsRotation = mInterpolateRoot.transform.rotation;
        }

        base.OnRunStarted();
    }

    public override void OnLateUpdate(float deltaTime)
    {
        if (mInterpolateRoot)
        {
            if (interpolationSpeed > 0 && deltaTime > 0)
            {
                //Vector3 vecloity = (transform.position - mPreviousWorldPosition) * mEngine.GetFixedDeltaTime();
                //float angularVelocity = Quaternion.Angle(transform.rotation, mPreviousWorldRotation) * mEngine.GetFixedDeltaTime();

                float rawInterpolation = Mathf.Clamp01(deltaTime / mEngine.GetFixedDeltaTime());
                float interpolation = rawInterpolation / (1 - mInterpolatedAmount) * interpolationSpeed;

                mInterpolateRoot.position = Vector3.Lerp(mPreviousGraphicsPosition, transform.position, interpolation);
                if (syncRotation)
                {
                    mInterpolateRoot.rotation = Quaternion.Lerp(mPreviousGraphicsRotation, transform.rotation, interpolation);
                }


                mInterpolatedAmount += rawInterpolation;
            }
            else
            {
                mInterpolateRoot.localPosition = Vector3.zero;
                mInterpolateRoot.localRotation = Quaternion.identity;
            }


            mPreviousGraphicsPosition = mInterpolateRoot.transform.position;
            mPreviousGraphicsRotation = mInterpolateRoot.transform.rotation;
        }

        base.OnLateUpdate(deltaTime);
    }

    public override void OnFixedUpdate(float fixedDeltaTime)
    {
        mPreviousWorldPosition = transform.position;
        mPreviousWorldRotation = transform.rotation;
    }

    public override void OnLateFixedUpdate(float fixedDeltaTime)
    {
        mInterpolatedAmount = 0;

        base.OnLateFixedUpdate(fixedDeltaTime);
    }

    virtual public void SetTint(Color color)
    {
        for (int i = 0; i < mMeshes.Length; i++)
        {
            mMeshes[i].material.color = color;
        }
    }
}
