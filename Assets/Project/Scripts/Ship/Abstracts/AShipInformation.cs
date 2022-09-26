using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Project.Scripts.Ship.Abstracts
{
    /// <summary>
    /// Defines the ship structure and its different states depending on what kind of handling is needed.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public abstract class AShipInformation : MonoBehaviour
    {
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Rigidbody == null)
                Rigidbody = gameObject.GetComponent<Rigidbody>();

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.TransformPoint(Rigidbody.centerOfMass), 0.5f);
        }
#endif
        
        public const string OffStatus = "Off";
        public const string ManualDrivingMode = "Manual";

        public bool IsOn
        {
            get => _isOn;

            set
            {
                if (value == _isOn)
                    return;

                if (!value)
                    CurrentStatus = OffStatus;

                _isOn = value;
            }
        }

        public static AShipInformation Instance { get; private set; }

        public string CurrentStatus
        {
            get => _currentStatus;

            protected set
            {
                if (value == CurrentStatus || !Status.Contains(value))
                    return;

                _currentStatus = value;
                onStatusChanged.Invoke(_currentStatus);
            }
        }
        /// <summary>
        /// The AShipInformation defined status (Some complex ships can define more status than usual).
        /// </summary>
        /// <example>
        /// We can define three simple status:
        ///     -> Off
        ///     -> Landed
        ///     -> Flying
        /// But it is also possible to add some advanced status like:
        ///     -> Hovering
        ///     -> AirFlying
        ///     -> SpaceFlying
        /// </example>
        public string[] Status { get; private set; }
  
        public virtual string CurrentDrivingMode
        {
            get => _currentDrivingMode;

            set
            {
                if (value == CurrentDrivingMode || !DrivingMode.Contains(value))
                    return;

                _currentDrivingMode = value;
                onDrivingModeChanged.Invoke(_currentDrivingMode);
            }
        }
        
        /// <summary>
        /// The AShipInformation defined DrivingMode (Some complex ships can define more modes than usual).
        /// These modes define how the ship is controlled by the player.
        /// </summary>
        /// <example>
        /// We can define two simple driving modes:
        ///     -> Manual
        ///     -> Automatic
        /// But it is also possible to add some advanced status like:
        ///     -> Assisted
        ///     -> Attentive
        ///     -> Furtive
        /// </example>
        public string[] DrivingMode { get; private set; }
        public Rigidbody Rigidbody { get; private set; }

        public UnityEvent<string> onStatusChanged = new UnityEvent<string>();
        public UnityEvent<string> onDrivingModeChanged = new UnityEvent<string>();

        private string _currentStatus = OffStatus;
        private string _currentDrivingMode = ManualDrivingMode;
        private bool _isOn;
    
        protected virtual void Awake()
        {
            //Singleton Pattern
            if (Instance)
                Destroy(this);
            else
                Instance = this;

            //Initialization
            Rigidbody = gameObject.GetComponent<Rigidbody>();

            Status = new[] { OffStatus }.Concat(InitializeStatus()).ToArray();
            DrivingMode = new[] { ManualDrivingMode }.Concat(InitializeDrivingModes()).ToArray();
        }

        protected virtual void Start()
        {
            //Starting status
            onStatusChanged.Invoke(CurrentStatus);
            onDrivingModeChanged.Invoke(CurrentDrivingMode);
        }

        protected virtual void Update()
        {
            if (!IsOn)
                return;

            RefreshStatus();
        }

        protected abstract IEnumerable<string> InitializeStatus();

        protected abstract void RefreshStatus();

        protected abstract IEnumerable<string> InitializeDrivingModes();
    }
}
