using Project.Scripts.Ship.Abstracts;
using UnityEngine;

namespace Project.Scripts.Ship.Components
{
    /// <summary>
    /// The axis controller for reactors, useful to control two reactors through a simple command.
    /// </summary>
    public class AxisThruster : AShipComponent
    {
        public override bool IsOn 
        { 
            get => base.IsOn;

            set 
            {
                if (base.IsOn == value)
                    return;

                if (left)
                    left.IsOn = value;
                if (right)
                    right.IsOn = value;

                base.IsOn = value;
            }
        }

        public float RequestedDirection => left && right ? right.RequestedForce - left.RequestedForce : 0;

        /// <summary>
        /// Get the current normalized direction#
        /// Left {-1 to 1} Right
        /// </summary>
        public float CurrentNormalizedForce => left && right ? right.CurrentNormalizedForce - left.CurrentNormalizedForce : 0;
        
        [SerializeField] private Thruster left;
        [SerializeField] private Thruster right;
        
        public void RequestDirection(float directionForce, float weight)
        {
            if (!IsOn)
                return;

            directionForce = Mathf.Clamp(directionForce, -1, 1);
            if (right)
                right.RequestForce(Mathf.Clamp(directionForce, 0, 1), weight);
            if (left)
                left.RequestForce(Mathf.Clamp(directionForce, -1, 0) * -1, weight);
        }
    }
}
