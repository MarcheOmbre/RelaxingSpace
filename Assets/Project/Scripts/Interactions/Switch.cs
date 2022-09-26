using Project.Scripts.Interactions.Abstracts;
using Project.Scripts.Tools;
using UnityEngine;

namespace Project.Scripts.Interactions
{
    /// <summary>
    /// Represents the switch buttons that can be pushed with a finger.
    /// If the switch button exceeds an angle during the interaction, it is automatically switched.
    /// </summary>
    public class Switch : ASwitchController
    {
#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            var maxAngle = angle * 2;
            
            if (switchAngle > maxAngle)
                switchAngle = maxAngle;
        }
#endif

        [SerializeField][Min(0)] private float angle = 30f;
        [SerializeField] [Min(0)] private float switchAngle = 5f;
        [SerializeField] private float followDT = 0.05f;
        [SerializeField] private float fallbackDT = 0.01f;

        private Transform _currentInteractionTransform;
        private float? _offset;
        private float _velocity;
        private float _currentAngle;

        protected virtual void Start()
        {
            //Set start value
            if(Value != StartValue)
                Value = StartValue;
            else
                onValueChanged?.Invoke(Value);

            RefreshSnapping();
        }

        protected virtual void Update()
        {
            var selfMove = CurrentControlMode != ControlMode.Master || _currentInteractionTransform == null;

            //If it's moving with the interaction, we do not refresh the position here.
            if (!selfMove)
                return;

            RefreshSpatialRepresentation(fallbackDT);
        }

        private void RefreshSnapping()
        {
            _currentAngle = Value ? angle : -angle;
        }
 
        protected override void OnInteractionStart(Transform interactionTransform)
        {
            _currentInteractionTransform = interactionTransform;

            base.OnInteractionStart(interactionTransform);
        }

        protected virtual void RefreshSpatialRepresentation(float deltaTime)
        {
            var cachedTransform = transform;
            cachedTransform.localRotation = cachedTransform.localRotation.SmoothDamp(Quaternion.Euler(Vector3.right * _currentAngle), ref _velocity, deltaTime);
        }

        protected override void OnInteractionFinish(Transform interactionTransform)
        {
            base.OnInteractionFinish(interactionTransform);

            _currentInteractionTransform = null;
            _offset = null;

            RefreshSnapping();
        }

        public override void UpdateInteraction()
        {
            if (CurrentControlMode != ControlMode.Master || !_currentInteractionTransform)
                return;

            //Get variables
            var parentRotation = transform.parent ? transform.parent.rotation : Quaternion.identity;
            var parentRight = parentRotation * Vector3.right;
            var direction = Vector3.ProjectOnPlane((_currentInteractionTransform.position - transform.position).normalized, parentRight);

            //Compute offset
            _offset ??= Vector3.SignedAngle(direction, transform.up, parentRight);

            //Compute the current angle
            _currentAngle = Vector3.SignedAngle(parentRotation * Vector3.up, direction, parentRight) + _offset.Value;
            _currentAngle = Mathf.Clamp(_currentAngle, -angle, angle);
            //Update the rotation
            RefreshSpatialRepresentation(followDT);

            //Check for switch
            if ((Value || !(_currentAngle > -angle + switchAngle)) && (!Value || !(_currentAngle < angle - switchAngle))) 
                return;
            
            Value = !Value;

            //Force stop interaction
            ForceStopInteraction();
        }

        public override bool SetSlaveValue(bool value)
        {
            var success = base.SetSlaveValue(value);

            if(success)
                RefreshSnapping();

            return success; 
        }
    }
}
