using UnityEngine;

namespace Project.Scripts.Interactions.Abstracts
{
    /// <summary>
    /// A switch controller to enable or disable a functionality.
    /// </summary>
    public abstract class ASwitchController : AValueShipCommand<bool> 
    {
        protected bool StartValue => startValue;
        
        [SerializeField] private bool startValue;

        protected virtual void Awake()
        {
            Value = startValue;
        }
    }
}
