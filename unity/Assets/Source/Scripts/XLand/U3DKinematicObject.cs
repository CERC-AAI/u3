using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Playables;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(U3PhysicsMover))]
public class U3DKinematicObject : EnvironmentComponent, IMoverController
{
    public U3PhysicsMover Mover;

    public float Speed = 1f;
    public PlayableDirector Director;

    private Transform _transform;

    protected override void Initialize()
    {
        base.Initialize();

        _transform = this.transform;

        if (Director == null)
        {
            Director = GetComponentInChildren<PlayableDirector>();
        }

        if (Director != null)
        {
            Director.timeUpdateMode = DirectorUpdateMode.Manual;
        }

        if (Mover == null)
        {
            Mover = GetComponentInChildren<U3PhysicsMover>();
        }

        if (Mover == null)
        {
            Debug.LogError("You must include a U3PhysicsMover component on a U3DKinematicObject object.");
        }
        else
        {
            Mover.MoverController = this;
        }
    }

    // This is called every FixedUpdate by our PhysicsMover in order to tell it what pose it should go to
    public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
    {
        // Remember pose before animation
        Vector3 _positionBeforeAnim = _transform.position;
        Quaternion _rotationBeforeAnim = _transform.rotation;

        // Update animation
        EvaluateAtTime(Time.time * Speed);

        // Set our platform's goal pose to the animation's
        goalPosition = _transform.position;
        goalRotation = _transform.rotation;

        // Reset the actual transform pose to where it was before evaluating. 
        // This is so that the real movement can be handled by the physics mover; not the animation
        _transform.position = _positionBeforeAnim;
        _transform.rotation = _rotationBeforeAnim;
    }

    public virtual void EvaluateAtTime(double time)
    {
        if (Director != null)
        {
            Director.time = time % Director.duration;
            Director.Evaluate();
        }
    }
}