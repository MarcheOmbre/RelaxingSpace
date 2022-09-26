using Interhaptics.Modules.Interaction_Builder.Core;
using Interhaptics.Modules.Interaction_Builder.Core.Abstract;
using System.Collections.Generic;
using UnityEngine;

namespace Interhaptics.ObjectSnapper.sample
{
    /// <summary>
    /// Create a "Steering wheel" interaction based on theh two hands direction.
    /// </summary>
    [RequireComponent(typeof(InteractionObject))]
    public class SteeringWheel : MonoBehaviour
    {
        #region Constants
        private const float PLACEHOLDER_Angle = 70f;
        #endregion

        #region Properties
        /// <summary>
        /// Get the real current angle.
        /// </summary>
        public float CurrentAngle { get { return _currentAngle; } }
        /// <summary>
        /// Get the normalized value between -1 and 1. If the maxAngle value is set to 0, then it will return 0.
        /// </summary>
        public float NormalizedAngle { get { return maxAngle > 0 ? (((_currentAngle + maxAngle) / (maxAngle * 2)) - 0.5f) * 2 : 0; } }
        #endregion

        #region Variables
        [SerializeField] private Vector3 localRotationVector = Vector3.forward;
        [SerializeField] [Min(0)] private float maxAngle = PLACEHOLDER_Angle;

        private InteractionBuilderManager _interactionBuilderManager = null;
        private InteractionObject _interactionObject = null;

        private Vector3 _lastDirection = Vector3.zero;
        private Quaternion _originalLocalRotation = Quaternion.identity;

        List<AInteractionBodyPart> _handsList = new List<AInteractionBodyPart>();
        List<AInteractionBodyPart> _currentInteractionHands = new List<AInteractionBodyPart>();
        private float _currentAngle = 0;
        #endregion

        #region Life Cycle
        private void OnValidate()
        {
            if (localRotationVector == Vector3.zero)
                localRotationVector = Vector3.forward;
        }

        private void Awake()
        {
            _interactionObject = gameObject.GetComponent<InteractionObject>();
            _interactionBuilderManager = GameObject.FindObjectOfType<InteractionBuilderManager>();

            _originalLocalRotation = transform.localRotation;

            localRotationVector = localRotationVector.normalized;

            if (_interactionBuilderManager.LeftHand)
                _handsList.Add(_interactionBuilderManager.LeftHand);
            if (_interactionBuilderManager.RightHand)
                _handsList.Add(_interactionBuilderManager.RightHand);
        }

        private void OnEnable()
        {
            _interactionObject.OnObjectComputed.AddListener(this.ComputedObject);

            foreach (AInteractionBodyPart aInteractionBodyPart in _handsList)
            {
                if (aInteractionBodyPart != null)
                {
                    aInteractionBodyPart.OnInteractionStartEvent.AddListener(this.OnInteractionStart);
                    aInteractionBodyPart.OnInteractionFinishEvent.AddListener(this.OnInteractionFinish);
                }
            }
        }

        private void OnDisable()
        {
            foreach (AInteractionBodyPart aInteractionBodyPart in _handsList)
            {
                if (aInteractionBodyPart != null)
                {
                    aInteractionBodyPart.OnInteractionStartEvent.AddListener(this.OnInteractionStart);
                    aInteractionBodyPart.OnInteractionFinishEvent.AddListener(this.OnInteractionFinish);
                }
            }

            _interactionObject.OnObjectComputed.RemoveListener(this.ComputedObject);
        }
        #endregion

        #region Privates
        private Vector3 GetHandsDirection()
        {
            Vector3 direction = Vector3.zero;

            for (int i = _currentInteractionHands.Count - 1; i >= 0; i--)
            {
                if (_currentInteractionHands[i] == null)
                    _currentInteractionHands.RemoveAt(i);
            }

            if (_currentInteractionHands.Count >= 2)
                direction = (_currentInteractionHands[1].transform.position - _currentInteractionHands[0].transform.position).normalized;
            else if (_currentInteractionHands.Count == 1)
                direction = (_currentInteractionHands[0].transform.position - transform.position).normalized;

            direction = Vector3.ProjectOnPlane(direction, transform.TransformDirection(localRotationVector));

            return direction;
        }

        private void OnInteractionStart(AInteractionBodyPart bodyPart, InteractionObject interactionObject)
        {
            if (interactionObject == _interactionObject && !_currentInteractionHands.Contains(bodyPart))
                _currentInteractionHands.Add(bodyPart);

            _lastDirection = this.GetHandsDirection();
        }

        private void ComputedObject()
        {
            if (!_interactionBuilderManager)
                return;

            //Get direction
            Vector3 handsDirection = this.GetHandsDirection();

            _currentAngle = Mathf.Clamp(_currentAngle + Vector3.SignedAngle(_lastDirection, handsDirection, transform.TransformDirection(localRotationVector)), -maxAngle, maxAngle);

            _lastDirection = handsDirection;

            transform.localRotation = _originalLocalRotation * Quaternion.Euler(localRotationVector * _currentAngle);
        }

        private void OnInteractionFinish(AInteractionBodyPart bodyPart, InteractionObject interactionObject)
        {
            if (interactionObject == _interactionObject && _currentInteractionHands.Contains(bodyPart))
                _currentInteractionHands.Remove(bodyPart);

            _lastDirection = this.GetHandsDirection();
        }
        #endregion
    }
}
