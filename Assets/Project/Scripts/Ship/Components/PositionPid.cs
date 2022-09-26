using Project.Scripts.Ship.Abstracts;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Scripts.Ship.Components
{
    /// <summary>
    /// This PID is able to control the spaceship position depending on the selected Stabilization type.
    ///     -> Velocity, the PID will try to reduce the velocity to 0.
    ///     -> Position, the PID will try to reach and keep the specified position (by reducing the velocity then resolving the position offset).
    /// </summary>
    public class PositionPid : AShipComponent
    {
        public enum Axis
        {
            X,
            Y,
            Z
        }

        public enum StabilizationType
        {
            None,
            Velocity,
            Position
        }
        
        private const float PositionClamp = 5f;
        private const float VelocityClamp = 1f;

        public override bool IsOn
        {
            get => base.IsOn;

            set
            {
                if (value == IsOn)
                    return;

                base.IsOn = value;
            }
        }
        /// <summary>
        /// Get/Set the PID weight.
        /// </summary>
        public float ControlWeight
        {
            get => controlWeight;

            set
            {
                if (value < 0)
                    value = 0;

                controlWeight = value;
            }
        }

        public Vector3 ExpectedPosition { get; set; }
        public StabilizationType PositionStabilizationType
        {
            get => positionStabilizationType;

            set => positionStabilizationType = value;
        }
        
        public Vector3 CurrentCorrection { get; private set; }
  

        [FormerlySerializedAs("velocityPID")] [SerializeField] private Vector3 velocityPid = Vector3.zero;
        [FormerlySerializedAs("positionPID")] [SerializeField] private Vector3 positionPid = Vector3.zero;
        [SerializeField] [Min(0)] private float controlWeight = 2.5f;
        [SerializeField] private StabilizationType positionStabilizationType = StabilizationType.None;
        [SerializeField] private DoubleAxisThruster strafeThrusters;
        [SerializeField] private AxisThruster yStrafeThrusters;

        private readonly PidController _xVelocityControllerSettings = new PidController();
        private readonly PidController _xPositionControllerSettings = new PidController();
        private readonly PidController _zVelocityControllerSettings = new PidController();
        private readonly PidController _zPositionControllerSettings = new PidController();
        private readonly PidController _yVelocityControllerSettings = new PidController();
        private readonly PidController _yPositionControllerSettings = new PidController();
        
        private void OnValidate()
        {
            InitializePIDs();
        }

        private void Awake()
        {
            //Initializing
            InitializePIDs();
        }

        protected override void Update()
        {
            base.Update();

            if (!IsOn)
                return;

            var correction = ComputeCorrection(Time.deltaTime);
            var currentCorrection = Vector3.zero;

            //Apply the correction
            if (PositionStabilizationType != StabilizationType.None)
            {
                if (strafeThrusters)
                    strafeThrusters.RequestDirection(new Vector2(correction.x, correction.z), ControlWeight);
                if (yStrafeThrusters)
                    yStrafeThrusters.RequestDirection(correction.y, ControlWeight);

                currentCorrection = correction;
            }

            CurrentCorrection = currentCorrection;
        }
   
        private void InitializePIDs()
        {
            //Velocity
            _xVelocityControllerSettings.Parameters = velocityPid;
            _yVelocityControllerSettings.Parameters = velocityPid;
            _zVelocityControllerSettings.Parameters = velocityPid;

            //Direction
            _xPositionControllerSettings.Parameters = positionPid;
            _yPositionControllerSettings.Parameters = positionPid;
            _zPositionControllerSettings.Parameters = positionPid;
        }

        private static Vector3 VectorClampingScale(Vector3 vector3, float clampValue)
        {
            if (clampValue == 0) 
                return vector3;
            
            vector3.x = Mathf.Clamp(vector3.x, -clampValue, clampValue) / clampValue;
            vector3.y = Mathf.Clamp(vector3.y, -clampValue, clampValue) / clampValue;
            vector3.z = Mathf.Clamp(vector3.z, -clampValue, clampValue) / clampValue;

            return vector3;
        }

        private Vector3 GetVectorPidCorrection(Vector3 vector3, Vector3 target, float clampValue, float deltaTime)
        {
            var correction = Vector3.zero;

            correction.x = _xVelocityControllerSettings.RefreshPid(vector3.x, target.x, deltaTime);
            correction.y = _yVelocityControllerSettings.RefreshPid(vector3.y, target.y, deltaTime);
            correction.z = _zVelocityControllerSettings.RefreshPid(vector3.z, target.z, deltaTime);

            return VectorClampingScale(correction, clampValue);
        }

        private Vector3 ComputeCorrection(float deltaTime)
        {
            var correction = Vector3.zero;
            
                if (PositionStabilizationType == StabilizationType.Velocity)
                {
                    var localVelocity = AShipInformation.Instance.transform.InverseTransformDirection(AShipInformation.Instance.Rigidbody.velocity);
                    correction += GetVectorPidCorrection(localVelocity, Vector3.zero, VelocityClamp, deltaTime);
                }
                else if(positionStabilizationType == StabilizationType.Position)
                {
                    var localTargetPosition = AShipInformation.Instance.transform.InverseTransformPoint(ExpectedPosition);
                    correction += GetVectorPidCorrection(Vector3.zero, localTargetPosition, PositionClamp, deltaTime);
                }

                return VectorClampingScale(correction, 1);
        }
   
        /// <summary>
        /// Specific public function that allows to set the stabilization type through the inspector.
        /// </summary>
        /// <param name="stabilizationType">The int corresponding to the stabilization enum</param>
        public void SetStabilizationType(int stabilizationType)
        {
            PositionStabilizationType = (StabilizationType)stabilizationType;
        }
    }
}
