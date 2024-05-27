using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

//base class for dealing with multiple components
public class EnvironmentComponentHolder : EnvironmentComponent
{
    protected EnvironmentComponent[] mComponents = null;
    bool mComponentsInitialized = false;

    override protected void Initialize()
    {
        base.Initialize();


        CheckComponents();
        for (int i = 0; i < mComponents.Length; i++)
        {
            if (mComponents[i] != this)
            {
                mComponents[i].CheckInitialized();
            }
        }
    }

    protected void CheckComponents()
    {
        if (!mComponentsInitialized)
        {
            mComponentsInitialized = true;
            mComponents = GetComponents<EnvironmentComponent>();
        }
    }

    virtual public EnvironmentComponent[] GetEnvironmentComponents()
    {
        CheckComponents();

        return (EnvironmentComponent[])mComponents.Clone();
    }

    virtual public void RunStarted()
    {
        for (int i = 0; i < mComponents.Length; i++)
        {
            mComponents[i].OnRunStarted();
        }
    }

    virtual public void RunEnded()
    {
        for (int i = 0; i < mComponents.Length; i++)
        {
            mComponents[i].OnRunEnded();
        }
    }

    virtual public void StepStarted()
    {
        for (int i = 0; i < mComponents.Length; i++)
        {
            mComponents[i].OnStepStarted();
        }
    }

    virtual public void StepEnded()
    {
        for (int i = 0; i < mComponents.Length; i++)
        {
            mComponents[i].OnStepEnded();
        }
    }

    override public void Remove()
    {
        for (int i = 0; i < mComponents.Length; i++)
        {
            if (mComponents[i] != this)
            {
                mComponents[i].OnRemoved();
            }
        }
    }

    /*override public void InvalidateEngine()
    {
        base.InvalidateEngine();

        for (int i = 0; i < mComponents.Length; i++)
        {
            if (mComponents[i] != this)
            {
                mComponents[i].InvalidateEngine();
            }
        }
    }*/

    virtual public void OnObjectUpdate(float deltaTime)
    {
        for (int i = 0; i < mComponents.Length; i++)
        {
            mComponents[i].OnUpdate(deltaTime);
        }
    }

    virtual public void OnObjectLateUpdate(float deltaTime)
    {
        for (int i = 0; i < mComponents.Length; i++)
        {
            mComponents[i].OnLateUpdate(deltaTime);
        }
    }

    virtual public void OnObjectFixedUpdate(float fixedDeltaTime)
    {
        for (int i = 0; i < mComponents.Length; i++)
        {
            mComponents[i].OnFixedUpdate(fixedDeltaTime);
        }
    }

    virtual public void OnObjectLateFixedUpdate(float fixedDeltaTime)
    {
        for (int i = 0; i < mComponents.Length; i++)
        {
            mComponents[i].OnLateFixedUpdate(fixedDeltaTime);
        }
    }

    /*virtual public bool OnObjectActionRecieved(float[] vectorAction)
    {
        bool shouldEndTurn = false;

        for (int i = 0; i < mComponents.Length; i++)
        {
            if (mComponents[i].OnActionRecieved(vectorAction))
            {
                shouldEndTurn = true;
            }
        }

        return shouldEndTurn;
    }*/
}