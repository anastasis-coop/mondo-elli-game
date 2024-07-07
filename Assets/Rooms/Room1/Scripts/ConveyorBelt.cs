using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace Room1
{
    public class ConveyorBelt : MonoBehaviour
    {
        private enum ConveyorDirection { CounterClockwise = -1, Clockwise = 1 }

        [SerializeField]
        private ConveyorRoom _conveyorRoom;
        
        [Space]
        [SerializeField, Min(0)]
        private float _speed;

        [SerializeField]
        private ConveyorDirection _direction;

        [SerializeField]
        private bool _maintainVelocityOnLeave;

        [SerializeField]
        private MeshRenderer[] _rollers;

        private FlickerableComponent[] _flickerables;
        
        public bool IsStopped { get; set; }
        
        public float Speed { get; private set; }

        public float SpeedOverride { get; set; } = -1;

        private void Awake()
        {
            _flickerables = GetComponentsInChildren<FlickerableComponent>();
            Speed = _speed * _conveyorRoom.trackSpeedMultiplier;
        }

        private void Update()
        {
            if (IsStopped) return;

            var speed = SpeedOverride < 0 ? Speed : SpeedOverride;
            var linearDelta = speed * Time.deltaTime * (int)_direction;
            foreach (var roller in _rollers)
            {
                var rollerTransform = roller.transform;
                var radius = roller.localBounds.extents.x;
                var angleDelta = linearDelta / (2 * Mathf.PI * radius) * 360;
                if (Mathf.Approximately(rollerTransform.eulerAngles.y, 180)) angleDelta *= -1;
                roller.transform.Rotate(rollerTransform.forward, angleDelta);
            }
        }

        private void OnCollisionStay(Collision collisionInfo)
        {
            var rigidbody = collisionInfo.rigidbody;
            if (rigidbody == null) return;
            
            var speed = 0f;
            if (!IsStopped) speed = SpeedOverride < 0 ? Speed : SpeedOverride;
            rigidbody.position += speed * (int)_direction * Time.deltaTime * transform.right;
        }

        private void OnCollisionExit(Collision collisionInfo)
        {
            var rigidbody = collisionInfo.rigidbody;
            if (rigidbody == null || !_maintainVelocityOnLeave) return;

            var speed = 0f;
            if (!IsStopped) speed = SpeedOverride < 0 ? Speed : SpeedOverride;
            rigidbody.velocity = speed * (int)_direction * transform.right;
        }

        public void StartFlickering(Color color)
        {
            foreach (var flickerable in _flickerables)
                flickerable.StartFlickering(color);
        }

        public void StopFlickering()
        {
            foreach (var flickerable in _flickerables)
                flickerable.StopFlickering();
        }
    }
    
}
