using UnityEngine;

namespace Project.Scripts.Ship.Abstracts
{
    /// <summary>
    /// The base class for the ShipComponents.
    /// </summary>
    public class AShipComponent : MonoBehaviour
    {
        public struct WeightedFloat
        {
            public float Value;
            public float Weight;
        }
        public virtual bool IsOn { get; set; }

        private bool _lastState;
        
        protected virtual void Update()
        {
            if (_lastState == AShipInformation.Instance.IsOn) 
                
                return;
            IsOn = AShipInformation.Instance.IsOn;
            _lastState = IsOn;
        }

        public void SetOn(bool enable) { IsOn = enable; }
    }
}
