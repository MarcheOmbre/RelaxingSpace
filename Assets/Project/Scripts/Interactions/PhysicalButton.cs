using Project.Scripts.Interactions.Abstracts;
using UnityEngine;

namespace Project.Scripts.Interactions
{
    /// <summary>
    /// Interactable physical button.
    /// </summary>
    public class PhysicalButton : AButton
    {
        private const float ValidationDepth = 0.8f;

        [SerializeField] [Min(0)] private float maxDepth = 0.02f;
        [SerializeField] private float followDT = 0.05f;
        [SerializeField] private float fallbackDT = 0.01f;

        private Transform _currentInteractionTransform;
        private float _originalLocalY;
        private float _offset;
        private float _velocity;
        private float _currentDepth;

        protected virtual void Awake()
        {
            _originalLocalY = transform.localPosition.y;
            _currentDepth = _originalLocalY;
        }

        protected virtual void Update()
        {
            var selfMove = CurrentControlMode != ControlMode.Master || _currentInteractionTransform == null;

            //If it's moving with the interaction, we do not refresh the position here.
            if (!selfMove)
                return;

            RefreshSpatialRepresentation(fallbackDT);
        }

        protected override void OnInteractionStart(Transform interactionTransform)
        {
            base.OnInteractionStart(interactionTransform);

            _currentInteractionTransform = interactionTransform;

            var newYPos = transform.parent ? transform.parent.InverseTransformPoint(_currentInteractionTransform.position).y : _currentInteractionTransform.position.y;
            _offset = transform.localPosition.y - newYPos;
        }

        protected virtual void RefreshSpatialRepresentation(float deltaTime)
        {
            var localPosition = transform.localPosition;
            localPosition.y = Mathf.SmoothDamp(localPosition.y, _currentDepth, ref _velocity, deltaTime);
            transform.localPosition = localPosition;
        }

        protected override void OnInteractionFinish(Transform interactionTransform)
        {
            base.OnInteractionFinish(interactionTransform);

            _currentInteractionTransform = null;
            _offset = 0;
            _currentDepth = _originalLocalY;
        }

        protected override bool IsClicking()
        {
            //Compute the current angle
            return _currentDepth <= _originalLocalY - ValidationDepth * maxDepth;
        }

        public override void UpdateInteraction()
        {
            base.UpdateInteraction();

            if (CurrentControlMode != ControlMode.Master || !_currentInteractionTransform)
                return;

            //Get variables
            var newYPos = transform.parent ? transform.parent.InverseTransformPoint(_currentInteractionTransform.position).y : _currentInteractionTransform.position.y;
            _currentDepth = Mathf.Clamp(newYPos + _offset, _originalLocalY - maxDepth, _originalLocalY);

            //Update the rotation
            RefreshSpatialRepresentation(followDT);
        }
    }
}