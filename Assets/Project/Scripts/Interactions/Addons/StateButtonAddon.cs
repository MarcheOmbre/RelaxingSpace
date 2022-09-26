using UnityEngine;
using UnityEngine.Events;

namespace Project.Scripts.Interactions.Addons
{
    [RequireComponent(typeof(VirtualButton))]
    public class StateButtonAddon : MonoBehaviour
    {
        public VirtualButton VirtualButton => _virtualButton;

        public int CurrentState
        {
            get => _currentState;

            protected set
            {
                //Circle clamping
                if (value > states.Length - 1)
                    value = 0;
                else if (value < 0)
                    value = states.Length - 1;

                //Check value
                if (value == _currentState || states == null)
                    return;

                _currentState = value;
                onStateChanged?.Invoke(value);
                states[_currentState]?.Invoke();
            }
        }

        public UnityEvent<int> onStateChanged = new UnityEvent<int>();

        [SerializeField] private UnityEvent[] states;
        [SerializeField] private bool actionOnClick;

        private VirtualButton _virtualButton;
        private int _currentState;

        protected virtual void Awake()
        {
            _virtualButton = gameObject.GetComponent<VirtualButton>();
        }

        protected virtual void Start()
        {
            //Initializing
            onStateChanged?.Invoke(0);
        }

        protected virtual void OnEnable()
        {
            if (actionOnClick)
                _virtualButton.onClick.AddListener(Increment);
            else
                _virtualButton.onRelease.AddListener(Increment);
        }

        protected virtual void OnDisable()
        {
            if (actionOnClick)
                _virtualButton.onClick.RemoveListener(Increment);
            else
                _virtualButton.onRelease.RemoveListener(Increment);
        }

        private void Increment()
        {
            CurrentState++;
        }

        public void SetState(int state)
        {
            CurrentState = state;
        }
    }
}