using System;
using System.Collections.Generic;
using Project.Scripts.Ship.Abstracts;
using UnityEngine;
using UnityEngine.Events;

namespace Project.Scripts.Ship.Components
{
    /// <summary>
    /// AShipComponent that applies a specific force (at the specific local direction) on the specified AShipInformation's Rigidbody.
    /// The force is depending on the RequestedForce and the power curve.
    /// The ForceMode used is ForceMode.Acceleration.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class Thruster : AShipComponent
    {
        public enum ForceType
        {
            Force,
            Torque,
            ForceAtPosition
        }
        
        public static readonly int PowerAnimatorHash = Animator.StringToHash("Power");

        public float CurrentNormalizedForce
        {
            get => _currentNormalizedForce;

            private set
            {
                if (Math.Abs(value - _currentNormalizedForce) <= Mathf.Epsilon) 
                    return;
                
                _currentNormalizedForce = value;
                onNormalizedValueChanged.Invoke(_currentNormalizedForce);
            }
        }
 
        public override bool IsOn 
        { 
            get => base.IsOn;

            set
            {
                if (value == IsOn)
                    return;

                requestedForce = 0;

                base.IsOn = value;
            }
        }
        
        public float RequestedForce => requestedForce;
        
        
        public UnityEvent<float> onNormalizedValueChanged = new UnityEvent<float>();
        
        [SerializeField] private ForceType forceType = ForceType.Force;
        [SerializeField] private Vector3 thrusterLocalDirection = Vector3.forward;
        [SerializeField] [Min(0)] private float maximumForce = 100f;
        [SerializeField] private AnimationCurve powerCurve;
        [SerializeField] [Min(0)] private float reactionSpeed = 1f;
        [SerializeField] [Range(0, 1)] private float requestedForce;

        private Animator _animator;
        private readonly List<WeightedFloat> _weightedFloats = new List<WeightedFloat>();
        private float _currentNormalizedForce;
  
        protected virtual void Awake()
        {
            //Initializing 
            _animator = gameObject.GetComponent<Animator>();
            thrusterLocalDirection = thrusterLocalDirection.normalized;
        }

        protected virtual void Start()
        {
            //Call events
            onNormalizedValueChanged.Invoke(_currentNormalizedForce);
        }

        protected virtual void FixedUpdate()
        {
            //Smooth damp the force
            var force = Mathf.MoveTowards(CurrentNormalizedForce, RequestedForce, reactionSpeed * Time.deltaTime);

            //Get the direction and rotation of the force
            var powerVector = transform.TransformDirection(thrusterLocalDirection) * Mathf.Clamp01(powerCurve.Evaluate(force)) * maximumForce;

            switch (forceType)
            {
                //Apply the force
                case ForceType.Force:
                    AShipInformation.Instance.Rigidbody.AddForce(powerVector, ForceMode.Acceleration);
                    break;
                case ForceType.Torque:
                    AShipInformation.Instance.Rigidbody.AddTorque(powerVector, ForceMode.Acceleration);
                    break;
                case ForceType.ForceAtPosition:
                    AShipInformation.Instance.Rigidbody.AddForceAtPosition(powerVector, transform.position, ForceMode.Acceleration);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _animator.SetFloat(PowerAnimatorHash, force);
            CurrentNormalizedForce = force;
        }

        protected override void Update()
        {
            base.Update();

            ComputeForces();
        }

        private void ComputeForces()
        {
            float newRequestedForce = 0;

            if(IsOn && _weightedFloats.Count > 0)
            {
                float totalWeight = 0;
                foreach (var weightedFloat in _weightedFloats)
                {
                    totalWeight += weightedFloat.Weight;
                    newRequestedForce += weightedFloat.Value * weightedFloat.Weight;
                }
                newRequestedForce /= totalWeight;
            }

            requestedForce = newRequestedForce;
            _weightedFloats.Clear();
        }

        public void RequestForce(float force, float weight)
        {
            force = Mathf.Clamp01(force);

            if (weight < 0)
                weight = 0;

            _weightedFloats.Add(new WeightedFloat { Value = force, Weight = weight });
        }
    }
}
