using UnityEngine.Events;

namespace Project.Scripts.Interactions.Abstracts
{
    /// <summary>
    /// Class that represents the buttons. It contains some parameters relatives to the buttons (Like clicked or on click).
    /// </summary>
    public abstract class AButton : AValueShipCommand<bool>
    {
        public bool IsClicked { get; private set; }

        public UnityEvent onClick = new UnityEvent();
        public UnityEvent onRelease = new UnityEvent();

        public override void UpdateInteraction()
        {
            if (CurrentControlMode != ControlMode.Master)
                return;

            if (IsClicked == IsClicking()) 
                return;
            
            IsClicked = !IsClicked;
            Value = IsClicked;

            if (IsClicked)
                onClick?.Invoke();
            else
                onRelease?.Invoke();
        }

        protected abstract bool IsClicking();
    }
}