using System.Collections.Generic;
using Project.Scripts.Ship.Abstracts;
using UnityEngine;

namespace Project.Scripts.Ship.Components
{
    /// <summary>
    /// AShipComponent that applies a rotation to the attached Transform, on the specified local axis.
    /// </summary>
    public class Rotator : AShipComponent
    {
        public override bool IsOn 
        { 
            get => base.IsOn;
            
            set 
            {
                if (value == IsOn)
                    return;

                requestedAngleOffset = 0;

                base.IsOn = value;
            }
        }

        public float RequestedAngleOffset => requestedAngleOffset;
        public float CurrentAngleOffset { get; private set; }
        public Vector2 Limits => limits;

        [SerializeField] private Vector3 localAxis = Vector3.forward;
        [SerializeField] private Vector2 limits;
        [SerializeField] [Min(0)] private float smoothDampDeltaTime = 1f;
        [SerializeField] [Min(0)] private float smoothDampMaximumSpeed = 10f;
        [SerializeField] private float requestedAngleOffset = 0f;

        private readonly List<WeightedFloat> _weightedFloats = new List<WeightedFloat>();
        private Quaternion _originalLocalRotation = Quaternion.identity;
        private float _currentVelocity;
   
        private void OnValidate()
        {
            if (limits.y < limits.x)
                limits.y = limits.x;
        }

        private void Awake()
        {
            _originalLocalRotation = transform.localRotation;

            localAxis = localAxis.normalized;
        }

        protected override void Update()
        {
            base.Update();

            ComputeRequestedAngle();

            CurrentAngleOffset = Mathf.SmoothDampAngle(CurrentAngleOffset, RequestedAngleOffset, ref _currentVelocity, smoothDampDeltaTime, smoothDampMaximumSpeed);
            transform.localRotation = _originalLocalRotation * Quaternion.Euler(localAxis * CurrentAngleOffset);
        }
        
        private void ComputeRequestedAngle()
        {
            float newRequestedAngle = 0;

            if(IsOn && _weightedFloats.Count > 0)
            {
                float totalWeight = 0;
                foreach (var weightedFloat in _weightedFloats)
                {
                    totalWeight += weightedFloat.Weight;
                    newRequestedAngle += weightedFloat.Value * weightedFloat.Weight;
                }
                newRequestedAngle /= totalWeight;
            }

            requestedAngleOffset = newRequestedAngle;
            _weightedFloats.Clear();
        }

        public void RequestAngle(float angle, float weight)
        {
            angle %= 360;

            if (weight < 0)
                weight = 0;

            _weightedFloats.Add(new WeightedFloat { Value = angle, Weight = weight });
        }
    }
}
