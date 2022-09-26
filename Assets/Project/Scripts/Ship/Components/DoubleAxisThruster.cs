using Project.Scripts.Ship.Abstracts;
using UnityEngine;

namespace Project.Scripts.Ship.Components
{
    /// <summary>
    /// The double axis controller for reactors, useful to control four reactors through a simple command.
    /// </summary>
    public class DoubleAxisThruster : AShipComponent
    {
        public override bool IsOn 
        { 
            get => base.IsOn;

            set 
            {
                if (base.IsOn == value)
                    return;
                
                top.IsOn = value;
                left.IsOn = value;
                right.IsOn = value;
                bottom.IsOn = value;

                base.IsOn = value;
            }
        }
        
        /// <summary>
        /// The requested force vector.
        /// X : Left {-1 to 1} Right
        /// Y : Back {-1 to 1} Front
        /// </summary>
        public Vector2 RequestedDirection
        {
            get 
            {
                var force = new Vector2
                {
                    x = right.RequestedForce - left.RequestedForce,
                    y = top.RequestedForce - bottom.RequestedForce
                };
                
                return force;
            }
        }
        /// <summary>
        /// The current force vector.
        /// X : Left {-1 to 1} Right
        /// Y : Back {-1 to 1} Front
        /// </summary>
        public Vector2 CurrentDirection
        {
            get
            {
                var force = new Vector2
                {
                    x = right.CurrentNormalizedForce - left.CurrentNormalizedForce,
                    y = top.CurrentNormalizedForce - bottom.CurrentNormalizedForce
                };
                
                return force;
            }
        }

        [SerializeField] private Thruster top;
        [SerializeField] private Thruster left;
        [SerializeField] private Thruster right;
        [SerializeField] private Thruster bottom;

        public void RequestDirection(Vector2 force, float weight)
        {
            if (!IsOn)
                return;

            force.x = Mathf.Clamp(force.x, -1, 1);
            right.RequestForce(Mathf.Clamp(force.x, 0, 1), weight);
            left.RequestForce(Mathf.Clamp(force.x, -1, 0) * -1, weight);

            force.y = Mathf.Clamp(force.y, -1, 1);
            top.RequestForce(Mathf.Clamp(force.y, 0, 1), weight);
            bottom.RequestForce(Mathf.Clamp(force.y, -1, 0) * -1, weight);
        }
    }
}
