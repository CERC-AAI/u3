using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using KinematicCharacterController;

/// <summary>
/// The system that manages the simulation of U3CharacterMotor and PhysicsMover
/// </summary>
[DefaultExecutionOrder(-100)]
public class U3CharacterSystem : MonoBehaviour
{
    EnvironmentEngine mEngine;

    public List<U3CharacterMotor> CharacterMotors = new List<U3CharacterMotor>();
    public List<U3PhysicsMover> PhysicsMovers = new List<U3PhysicsMover>();

    private float _lastCustomInterpolationStartTime = -1f;
    private float _lastCustomInterpolationDeltaTime = -1f;

    public KCCSettings Settings;

    public void SetEngine(EnvironmentEngine engine)
    {
        mEngine = engine;
    }

    /*public static void EnsureCreation()
    {
        if (_instance == null)
        {
            GameObject systemGameObject = new GameObject("KinematicCharacterSystem");
            _instance = systemGameObject.AddComponent<U3CharacterSystem>();

            systemGameObject.hideFlags = HideFlags.NotEditable;
            _instance.hideFlags = HideFlags.NotEditable;

            Settings = ScriptableObject.CreateInstance<KCCSettings>();

            GameObject.DontDestroyOnLoad(systemGameObject);
        }
    }

    /// <summary>
    /// Gets the KinematicCharacterSystem instance if any
    /// </summary>
    /// <returns></returns>
    public static U3CharacterSystem GetInstance()
    {
        return _instance;
    }*/

    public void SetCharacterMotorsCapacity(int capacity)
    {
        if (capacity < CharacterMotors.Count)
        {
            capacity = CharacterMotors.Count;
        }
        CharacterMotors.Capacity = capacity;
    }

    /// <summary>
    /// Registers a U3CharacterMotor into the system
    /// </summary>
    public void RegisterCharacterMotor(U3CharacterMotor motor)
    {
        CharacterMotors.Add(motor);
    }

    /// <summary>
    /// Unregisters a U3CharacterMotor from the system
    /// </summary>
    public void UnregisterCharacterMotor(U3CharacterMotor motor)
    {
        CharacterMotors.Remove(motor);
    }

    /// <summary>
    /// Sets the maximum capacity of the physics movers list, to prevent allocations when adding movers
    /// </summary>
    /// <param name="capacity"></param>
    public void SetPhysicsMoversCapacity(int capacity)
    {
        if (capacity < PhysicsMovers.Count)
        {
            capacity = PhysicsMovers.Count;
        }
        PhysicsMovers.Capacity = capacity;
    }

    /// <summary>
    /// Registers a PhysicsMover into the system
    /// </summary>
    public void RegisterPhysicsMover(U3PhysicsMover mover)
    {
        PhysicsMovers.Add(mover);

        mover.Rigidbody.interpolation = RigidbodyInterpolation.None;
    }

    /// <summary>
    /// Unregisters a PhysicsMover from the system
    /// </summary>
    public void UnregisterPhysicsMover(U3PhysicsMover mover)
    {
        PhysicsMovers.Remove(mover);
    }

    /*private void Awake()
    {
        _instance = this;
    }*/

    public void OnFixedUpdate(float deltaTime)
    //public void FixedUpdate()
    {
        //float deltaTime = Time.fixedDeltaTime;
        if (Settings.AutoSimulation)
        {
            if (Settings.Interpolate)
            {
                PreSimulationInterpolationUpdate(deltaTime);
            }

            Simulate(deltaTime, CharacterMotors, PhysicsMovers);

            if (Settings.Interpolate)
            {
                PostSimulationInterpolationUpdate(deltaTime);
            }
        }
    }

    public void OnLateUpdate()
    {
        if (Settings.Interpolate)
        {
            CustomInterpolationUpdate();
        }
    }

    /// <summary>
    /// Remembers the point to interpolate from for U3CharacterMotors and PhysicsMovers
    /// </summary>
    public void PreSimulationInterpolationUpdate(float deltaTime)
    {
        // Save pre-simulation poses and place transform at transient pose
        for (int i = 0; i < CharacterMotors.Count; i++)
        {
            U3CharacterMotor motor = CharacterMotors[i];

            motor.InitialTickPosition = motor.TransientPosition;
            motor.InitialTickRotation = motor.TransientRotation;

            motor.Transform.SetPositionAndRotation(motor.TransientPosition, motor.TransientRotation);
        }

        for (int i = 0; i < PhysicsMovers.Count; i++)
        {
            U3PhysicsMover mover = PhysicsMovers[i];

            mover.InitialTickPosition = mover.TransientPosition;
            mover.InitialTickRotation = mover.TransientRotation;

            mover.Transform.SetPositionAndRotation(mover.TransientPosition, mover.TransientRotation);
            mover.Rigidbody.position = mover.TransientPosition;
            mover.Rigidbody.rotation = mover.TransientRotation;
        }
    }

    /// <summary>
    /// Ticks characters and/or movers
    /// </summary>
    public void Simulate(float deltaTime, List<U3CharacterMotor> motors, List<U3PhysicsMover> movers)
    {
        int characterMotorsCount = motors.Count;
        int physicsMoversCount = movers.Count;

#pragma warning disable 0162
        // Update PhysicsMover velocities
        for (int i = 0; i < physicsMoversCount; i++)
        {
            movers[i].VelocityUpdate(deltaTime);
        }

        // Character controller update phase 1
        for (int i = 0; i < characterMotorsCount; i++)
        {
            motors[i].UpdatePhase1(deltaTime);
        }

        // Simulate PhysicsMover displacement
        for (int i = 0; i < physicsMoversCount; i++)
        {
            U3PhysicsMover mover = movers[i];

            mover.Transform.SetPositionAndRotation(mover.TransientPosition, mover.TransientRotation);
            mover.Rigidbody.position = mover.TransientPosition;
            mover.Rigidbody.rotation = mover.TransientRotation;
        }

        // Character controller update phase 2 and move
        for (int i = 0; i < characterMotorsCount; i++)
        {
            U3CharacterMotor motor = motors[i];

            motor.UpdatePhase2(deltaTime);

            motor.Transform.SetPositionAndRotation(motor.TransientPosition, motor.TransientRotation);
        }
#pragma warning restore 0162
    }

    /// <summary>
    /// Initiates the interpolation for U3CharacterMotors and PhysicsMovers
    /// </summary>
    public void PostSimulationInterpolationUpdate(float deltaTime)
    {
        _lastCustomInterpolationStartTime = Time.time;
        _lastCustomInterpolationDeltaTime = deltaTime;

        // Return interpolated roots to their initial poses
        for (int i = 0; i < CharacterMotors.Count; i++)
        {
            U3CharacterMotor motor = CharacterMotors[i];

            motor.Transform.SetPositionAndRotation(motor.InitialTickPosition, motor.InitialTickRotation);
        }

        for (int i = 0; i < PhysicsMovers.Count; i++)
        {
            U3PhysicsMover mover = PhysicsMovers[i];

            if (mover.MoveWithPhysics)
            {
                mover.Rigidbody.position = mover.InitialTickPosition;
                mover.Rigidbody.rotation = mover.InitialTickRotation;

                mover.Rigidbody.MovePosition(mover.TransientPosition);
                mover.Rigidbody.MoveRotation(mover.TransientRotation);
            }
            else
            {
                mover.Rigidbody.position = (mover.TransientPosition);
                mover.Rigidbody.rotation = (mover.TransientRotation);
            }
        }
    }

    /// <summary>
    /// Handles per-frame interpolation
    /// </summary>
    private void CustomInterpolationUpdate()
    {
        float interpolationFactor = Mathf.Clamp01((Time.time - _lastCustomInterpolationStartTime) / _lastCustomInterpolationDeltaTime);

        // Handle characters interpolation
        for (int i = 0; i < CharacterMotors.Count; i++)
        {
            U3CharacterMotor motor = CharacterMotors[i];

            motor.Transform.SetPositionAndRotation(
                Vector3.Lerp(motor.InitialTickPosition, motor.TransientPosition, interpolationFactor),
                Quaternion.Slerp(motor.InitialTickRotation, motor.TransientRotation, interpolationFactor));
        }

        // Handle PhysicsMovers interpolation
        for (int i = 0; i < PhysicsMovers.Count; i++)
        {
            U3PhysicsMover mover = PhysicsMovers[i];

            mover.Transform.SetPositionAndRotation(
                Vector3.Lerp(mover.InitialTickPosition, mover.TransientPosition, interpolationFactor),
                Quaternion.Slerp(mover.InitialTickRotation, mover.TransientRotation, interpolationFactor));

            Vector3 newPos = mover.Transform.position;
            Quaternion newRot = mover.Transform.rotation;
            mover.PositionDeltaFromInterpolation = newPos - mover.LatestInterpolationPosition;
            mover.RotationDeltaFromInterpolation = Quaternion.Inverse(mover.LatestInterpolationRotation) * newRot;
            mover.LatestInterpolationPosition = newPos;
            mover.LatestInterpolationRotation = newRot;
        }

        Debug.Log("CustomInterpolationUpdate");

    }
}