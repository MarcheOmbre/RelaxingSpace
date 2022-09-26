using Project.Scripts.Ship.Abstracts;
using Project.Scripts.Tools;
using UnityEngine;

namespace Project.Scripts.Ship.Components
{
    /// <summary>
    /// Represents the LandingGear component.
    /// This AShipComponent can be retracted.
    /// It informs if the ship is well grounded or not.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class LandingGear : AShipComponent
    {
        public enum Status
        {
            In,
            Moving,
            Out
        }
        
        public static readonly int OutAnimatorHash = Animator.StringToHash("Out");
        public static readonly int  LandingGearInAnimatorHash = Animator.StringToHash("LandingGear_In");
        public static readonly int AnimatorOutAnimatorHash = Animator.StringToHash("LandingGear_Out");
        
        public Animator Animator { get; private set; }
        public Status CurrentStatus { get; private set; }
        public bool IsGrounded => _groundCollisions > 0;

        private CollisionSharing[] _triggerSharing;
        private int _groundableLayer;
        private int _groundCollisions;

        private void Awake()
        {
            //Initialization
            Animator = gameObject.GetComponent<Animator>();
            _triggerSharing = gameObject.GetComponentsInChildren<CollisionSharing>();
            
            _groundableLayer = LayerMask.NameToLayer(Shared.GroundableLayer);
        }

        protected virtual void OnEnable()
        {
            foreach (var collisionSharing in _triggerSharing)
            {
                if (collisionSharing == null)
                    continue;

                collisionSharing.onTriggerEnter.AddListener(OnTriggerEnter);
                collisionSharing.onTriggerExit.AddListener(OnTriggerExit);
            }
        }

        protected override void Update()
        {
            base.Update();

            RefreshStatus();
        }

        protected virtual void OnDisable()
        {
            foreach (var collisionSharing in _triggerSharing)
            {
                if (collisionSharing == null)
                    continue;

                collisionSharing.onTriggerEnter.RemoveListener(OnTriggerEnter);
                collisionSharing.onTriggerExit.RemoveListener(OnTriggerExit);
            }
        }

        private void RefreshStatus()
        {
            var newStatus = Status.Moving;

            var animatorStateInfo = Animator.GetCurrentAnimatorStateInfo(0);
            if (animatorStateInfo.normalizedTime >= 1)
            {
                var currentStateInfoFullHash = Animator.GetCurrentAnimatorStateInfo(0).shortNameHash;

                if (currentStateInfoFullHash.Equals(LandingGearInAnimatorHash))
                    newStatus = Status.In;
                else if (currentStateInfoFullHash.Equals(AnimatorOutAnimatorHash))
                    newStatus = Status.Out;
            }

            CurrentStatus = newStatus;
        }

        public void Retract(bool retract)
        {
            if (!IsOn)
                return;

            if (retract == Animator.GetBool(OutAnimatorHash))
                Animator.enabled = true;

            Animator.SetBool(OutAnimatorHash, !retract);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == _groundableLayer)
                _groundCollisions++;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == _groundableLayer)
                _groundCollisions--;
        }
    }
}
