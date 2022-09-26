using UnityEngine;

namespace Project.Scripts.Interactions.Abstracts
{
    /// <summary>
    /// Interaction that provides a normalized value between 0 and 1.
    /// </summary>
    public abstract class ASingleAxisController : AValueShipCommand<float>
    {
        public override float Value 
        { 
            get => base.Value; 

            protected set => base.Value = Mathf.Clamp(value, -1, 1);
        }
    }
}
