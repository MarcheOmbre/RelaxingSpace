using Project.Scripts.Interactions.Abstracts;
using Project.Scripts.Tools;
using UnityEngine;

namespace Project.Scripts.Interactions
{
    /// <summary>
    /// Joystick command that uses the hand position to define the stick rotation.
    /// The Joystick is rotating around its parent X and Z axis.
    /// Value.x: Z axis rotation (left and right).
    /// Value.y: x axis rotation (forward and backward).
    /// </summary>
    public class Joystick : ADoubleAxisController
    {
        private const float MaxAngleMax = 60f;

        [SerializeField] [Range(0, MaxAngleMax)] private float maxAngle = 25f;
        [SerializeField] [Min(0)] private Vector3 localPivotPoint = Vector3.zero;
        [SerializeField] private float followDT = 0.2f;
        [SerializeField] private float fallbackDT = 0.5f;

        private Quaternion? _offset;
        private Transform _currentInteractionTransform;
        private float _velocity;

        private void Update()
        {
            var selfMove = CurrentControlMode != ControlMode.Master || _currentInteractionTransform == null;

            //If it's moving with the interaction, we do not refresh the position here.
            if (!selfMove)
                return;

            if (CurrentControlMode == ControlMode.Master && Value != Vector2.zero)
                Value = Vector2.zero;

            RefreshSpatialRepresentation(fallbackDT);
        }

        protected override void OnInteractionStart(Transform interactionTransform)
        {
            _currentInteractionTransform = interactionTransform;

            base.OnInteractionStart(interactionTransform);
        }

        protected virtual void RefreshSpatialRepresentation(float deltaTime)
        {
            var angles = Value * maxAngle;
            
            var cachedTransform = transform;
            cachedTransform.localRotation = cachedTransform.localRotation.SmoothDamp(Quaternion.Euler(Vector3.back * angles.x + Vector3.right * angles.y), ref _velocity, deltaTime);
        }

        protected override void OnInteractionFinish(Transform interactionTransform)
        {
            _currentInteractionTransform = null;
            _offset = null;

            base.OnInteractionFinish(interactionTransform);
        }

        public override void UpdateInteraction()
        {
            if (CurrentControlMode != ControlMode.Master || !_currentInteractionTransform)
                return;

            //Get and clamp the direction
            var parentRotation = transform.parent ? transform.parent.rotation : Quaternion.identity;
            var direction = Quaternion.Inverse(parentRotation) * (_currentInteractionTransform.position - transform.TransformPoint(localPivotPoint)).normalized;

            _offset ??= Quaternion.FromToRotation(direction, Vector3.up);

            direction = _offset.Value * direction;

            var cross = Vector3.Cross(Vector3.up, direction).normalized;
            var currentValue = cross * (Vector3.SignedAngle(Vector3.up, direction, cross) / maxAngle);
            Value = new Vector2(-currentValue.z, currentValue.x);

            //Apply rotation
            RefreshSpatialRepresentation(followDT);
        }
    }
}
