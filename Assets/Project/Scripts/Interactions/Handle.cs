using Project.Scripts.Interactions.Abstracts;
using Project.Scripts.Tools;
using UnityEngine;

namespace Project.Scripts.Interactions
{
    /// <summary>
    /// Handle command that uses the hand position to define the stick rotation.
    /// The Handle is rotating around its parent X axis.
    /// Value: X axis rotation (forward and backward).
    /// </summary>
    public class Handle : ASingleAxisController
    {
        private const float MaxAngleMax = 60f;

        public bool Fallback
        {
            get => fallback;

            set => fallback = value;
        }

        public float NormalizedValue => (Value - deadValue) / 2;


        [SerializeField][Range(-1, 1)] private float deadValue;
        [SerializeField] [Range(0, MaxAngleMax)] private float maxAngle = 25f;
        [SerializeField] [Min(0)] private Vector3 localPivotPoint = Vector3.zero;
        [SerializeField] private bool fallback;
        [SerializeField] private float followDT = 0.2f;
        [SerializeField] private float fallbackDT = 0.5f;

        private Quaternion? _offset;
        private Transform _currentInteractionTransform;
        private float _velocity;

        private void Start()
        {
            Value = deadValue;
        }

        private void Update()
        {
            var selfMove = CurrentControlMode != ControlMode.Master || _currentInteractionTransform == null;

            //If it's moving with the interaction, we do not refresh the position here.
            if (!selfMove)
                return;

            if (CurrentControlMode == ControlMode.Master && fallback && Value != 0)
                Value = deadValue;

            RefreshSpatialRepresentation(fallbackDT);
        }

        protected override void OnInteractionStart(Transform interactionTransform)
        {
            _currentInteractionTransform = interactionTransform;

            base.OnInteractionStart(interactionTransform);
        }

        protected virtual void RefreshSpatialRepresentation(float deltaTime)
        {
            var angle = Value * maxAngle;
            transform.localRotation = MathematicsExtension.SmoothDamp(transform.localRotation, Quaternion.Euler(Vector3.right * angle), ref _velocity, deltaTime);
        }

        protected override void OnInteractionFinish(Transform interactionTransform)
        {
            base.OnInteractionFinish(interactionTransform);

            _currentInteractionTransform = null;
            _offset = null;
        }

        public override void UpdateInteraction()
        {
            if (CurrentControlMode != ControlMode.Master || !_currentInteractionTransform)
                return;

            //Get and clamp the direction
            var parentRotation = transform.parent ? transform.parent.rotation : Quaternion.identity;

            var direction = Vector3.ProjectOnPlane(Quaternion.Inverse(parentRotation) * 
                                                   (_currentInteractionTransform.position - transform.TransformPoint(localPivotPoint)).normalized, Vector3.right).normalized;

            //Compute the offset if necessary
            _offset ??= Quaternion.FromToRotation(direction, Quaternion.Inverse(parentRotation) * transform.up);

            direction = _offset.Value * direction;

            Value = Mathf.Clamp(Vector3.SignedAngle(Vector3.up, direction, Vector3.right), -maxAngle, maxAngle)/maxAngle;

            //Apply rotation
            RefreshSpatialRepresentation(followDT);
        }
    }
}
