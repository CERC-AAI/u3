using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KinematicCharacterController.Examples
{
    public class PhysicsObject : MonoBehaviour, IMoverController
    {
        public PhysicsMover Mover;

        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        
        private void Start()
        {
            _originalPosition = Mover.Rigidbody.position;
            _originalRotation = Mover.Rigidbody.rotation;

            Mover.MoverController = this;
        }

        public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
        {
            goalPosition = transform.position;

            goalRotation = transform.rotation;
        }
    }
}