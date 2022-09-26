using Project.Scripts.Environment;
using Project.Scripts.Ship.Abstracts;
using UnityEngine;

namespace Project.Scripts.Ship.Components
{
    /// <summary>
    /// The RadarAltimeter provides information about the altitude (from the sea level and from the nearest groundable object if below the distance limit).
    /// </summary>
    public class RadarAltimeter : AShipComponent
    {
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (UnityEditor.Selection.activeGameObject == gameObject)
            {
                Vector3 raycastStartPoint = transform.position;
                Vector3 raycastEndPoint = raycastStartPoint + Vector3.down * sphereCastMaxDistance;
                Gizmos.color = new Color(0, 1, 0, 0.5f);
                Gizmos.DrawSphere(raycastStartPoint, sphereCastRadius);
                Gizmos.color = new Color(1, 0, 0, 0.5f);
                Gizmos.DrawSphere(raycastEndPoint, sphereCastRadius);
                Gizmos.color = new Color(0, 0, 1, 0.5f);
                Gizmos.DrawLine(raycastStartPoint + Vector3.down * sphereCastRadius, raycastEndPoint + -transform.up * sphereCastRadius);
            }
        }
#endif
        public struct GroundInfo
        {
            public float Distance;
            public Vector3 Normal;
            public Vector3 Position;
        }
        
        public PhysicData CurrentPhysicData => gravityReceiver ? gravityReceiver.CurrentPhysicData : default;

        public GroundInfo? CurrentGroundInfo { get; private set; }
        
        [SerializeField] private PhysicReceiver gravityReceiver = null;
        [SerializeField] [Min(0)] private float sphereCastRadius = 2f;
        [SerializeField] [Min(0)] private float sphereCastMaxDistance = 4f;

        private int _groundableLayer;
        
        private void Awake()
        {
            //Initializing
            _groundableLayer = 1 << LayerMask.NameToLayer(Shared.GroundableLayer);
        }

        private void OnEnable()
        {
            AnalyzeGround();
        }

        protected override void Update()
        {
            base.Update();

            if (IsOn)
                AnalyzeGround();
        }

        private void AnalyzeGround()
        {
            CurrentGroundInfo = null;

            if (Physics.SphereCast(transform.position, sphereCastRadius, -transform.up, out RaycastHit hit, sphereCastMaxDistance, _groundableLayer))
            {
                CurrentGroundInfo = new GroundInfo()
                {
                    Distance = hit.distance,
                    Normal = hit.normal,
                    Position = hit.point
                };
            }
        }
    }
}