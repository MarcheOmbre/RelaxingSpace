using UnityEngine;

namespace Project.Scripts.Interactions.Abstracts
{
    /// <summary>
    /// Interaction that provides a Vector2 where each value is between 0 and 1.
    /// </summary>
    public abstract class ADoubleAxisController : AValueShipCommand<Vector2>
    {
        public override Vector2 Value 
        { 
            get => base.Value; 
            
            protected set => base.Value = Vector2.ClampMagnitude(value, 1); 
        }
    }
}