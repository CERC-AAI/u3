using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class U3DEnvironment : EnvironmentEngine
{
    U3CharacterSystem mKinematicCharacterSystem;

    protected override void Initialize()
    {
        if (mKinematicCharacterSystem == null)
        {
            mKinematicCharacterSystem = gameObject.AddComponent<U3CharacterSystem>();

            mKinematicCharacterSystem.SetEngine(this);

            mKinematicCharacterSystem.Settings = ScriptableObject.CreateInstance<KCCSettings>();
        }

        base.Initialize();
    }

    /*public override void OnObjectLateUpdate(float deltaTime)
    {
        mKinematicCharacterSystem.OnLateUpdate(deltaTime);

        base.OnObjectLateUpdate(deltaTime);
    }*/

    protected override void OnFinalUpdate(float deltaTime)
    {
        mKinematicCharacterSystem.OnLateUpdate();

        base.OnFinalUpdate(deltaTime);
    }


    private void LateUpdate()
    {
        //mKinematicCharacterSystem.OnLateUpdate();
    }

    public override void OnObjectFixedUpdate(float fixedDeltaTime)
    //void FixedUpdate()
    {
        //float fixedDeltaTime  = Time.fixedDeltaTime;

        mKinematicCharacterSystem.OnFixedUpdate(fixedDeltaTime);

        base.OnObjectFixedUpdate(fixedDeltaTime);
    }

    public override U3CharacterSystem GetKinematicCharacterSystem()
    {
        return mKinematicCharacterSystem;
    }
}