using UnityEngine.Events;

namespace Project.Scripts.Interactions.Abstracts
{
    /// <summary>
    /// The base class for the interactions.
    /// </summary>
    public abstract class AValueShipCommand<T> : AShipCommand
    {
        public virtual T Value
        {
            get => _value;

            protected set
            {
                if (value.Equals(_value)) 
                    return;
                
                _value = value;
                onValueChanged.Invoke(_value);
            }
        }

        public UnityEvent<T> onValueChanged = new UnityEvent<T>();

        private T _value;

        public virtual bool SetSlaveValue(T value)
        {
            var isSlave = CurrentControlMode == ControlMode.Slave;

            if (isSlave)
                Value = value;

            return isSlave;
        }
    }
}