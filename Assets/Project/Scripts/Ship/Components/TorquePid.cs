using Project.Scripts.Ship.Abstracts;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Scripts.Ship.Components
{
    /// <summary>
    /// This PID is able to control the spaceship torque depending on the selected Stabilization type.
    ///     -> Velocity, the PID will try to reduce the angular velocity to 0.
    ///     -> Direction, the PID will try to reach and keep the specified direction (by reducing the velocity then resolving the direction offset).
    /// </summary>
    public class TorquePid : AShipComponent
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
            Direction
        }
        
        private const float AngleClamp = 10f;
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

        public Quaternion ExpectedRotation { get; set; }
        public StabilizationType XZStabilizationType 
        { 
            get => xzStabilizationType;

            set => xzStabilizationType = value;
        }

        public StabilizationType YStabilizationType 
        { 
            get => yStabilizationType;

            set => yStabilizationType = value;
        }
        
        public Vector3 CurrentCorrection { get; private set; }

        /// <summary>
        /// The PD controller for velocity.
        /// x = kP
        /// y = kI
        /// z = kD
        /// </summary>
        [FormerlySerializedAs("velocityPID")] [SerializeField] private Vector3 velocityPid = Vector3.zero;
        /// <summary>
        /// The PD controller for direction.
        /// x = kP
        /// y = kI
        /// z = kD
        /// </summary>
        [FormerlySerializedAs("directionPID")] [SerializeField] private Vector3 directionPid = Vector3.zero;
        [SerializeField] [Min(0)] private float controlWeight = 2.5f;
        [SerializeField] private StabilizationType xzStabilizationType = StabilizationType.None;
        [SerializeField] private DoubleAxisThruster torqueThrusters;
        [SerializeField] private StabilizationType yStabilizationType = StabilizationType.None;
        [SerializeField] private AxisThruster yTorqueThrusters;

        private readonly PidController _rollVelocityControllerSettings = new PidController();
        private readonly PidController _rollDirectionControllerSettings = new PidController();
        private readonly PidController _pitchVelocityControllerSettings = new PidController();
        private readonly PidController _pitchDirectionControllerSettings = new PidController();
        private readonly PidController _yawVelocityControllerSettings = new PidController();
        private readonly PidController _yawDirectionControllerSettings = new PidController();
        
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
            if (xzStabilizationType != StabilizationType.None && torqueThrusters)
            {
                torqueThrusters.RequestDirection(new Vector2(correction.z, correction.x), ControlWeight);
                currentCorrection.x = correction.x;
                currentCorrection.z = correction.z;
            }
            if (yStabilizationType != StabilizationType.None && yTorqueThrusters)
            {
                yTorqueThrusters.RequestDirection(correction.y, ControlWeight);
                currentCorrection.y = correction.y;
            }

            CurrentCorrection = currentCorrection;
        }

        private void InitializePIDs()
        {
            //Velocity
            _rollVelocityControllerSettings.Parameters = velocityPid;
            _pitchVelocityControllerSettings.Parameters = velocityPid;
            _yawVelocityControllerSettings.Parameters = velocityPid;

            //Direction
            _rollDirectionControllerSettings.Parameters = directionPid;
            _pitchDirectionControllerSettings.Parameters = directionPid;
            _yawDirectionControllerSettings.Parameters = directionPid;
        }

        private static Vector3 VectorClampingScale(Vector3 vector3, float clampValue)
        {
            if (clampValue == 0) 
                return vector3;
            
            vector3.x = Mathf.Clamp(vector3.x, - clampValue, clampValue) / clampValue;
            vector3.y = Mathf.Clamp(vector3.y, - clampValue, clampValue) / clampValue;
            vector3.z = Mathf.Clamp(vector3.z, - clampValue, clampValue) / clampValue;

            return vector3;
        }

        private static float RotationTo180(float angle)
        {
            angle %= 360;
            angle = (angle + 360) % 360;

            if (angle > 180)
                angle -= 360;

            return angle;
        }

        private Vector3 GetVelocityPidCorrection(float deltaTime)
        {
            var correction = Vector3.zero;

            var localVelocity = AShipInformation.Instance.transform.InverseTransformDirection(AShipInformation.Instance.Rigidbody.angularVelocity);
            if (XZStabilizationType != StabilizationType.None)
            {
                correction.x = _pitchVelocityControllerSettings.RefreshPid(localVelocity.x, 0, deltaTime);
                correction.z = -_rollVelocityControllerSettings.RefreshPid(localVelocity.z, 0, deltaTime);
            }
            if(YStabilizationType != StabilizationType.None)
                correction.y = _yawVelocityControllerSettings.RefreshPid(localVelocity.y, 0, deltaTime);

            return VectorClampingScale(correction, VelocityClamp);
        }

        private Vector3 GetDirectionPidCorrection(float deltaTime, Quaternion currentRotation)
        {
            var correction = Vector3.zero;

            var expectedUp = currentRotation * Vector3.up;
            var expectedForward = currentRotation * Vector3.forward;

            if (XZStabilizationType == StabilizationType.Direction)
            {
                expectedUp = ExpectedRotation * Vector3.up;
                expectedForward = Vector3.ProjectOnPlane(expectedForward, expectedUp);
            }

            if (YStabilizationType == StabilizationType.Direction)
                expectedForward = ExpectedRotation * Vector3.forward;

            var localOffset = (Quaternion.Inverse(currentRotation) * Quaternion.LookRotation(expectedForward, expectedUp)).normalized.eulerAngles;
            correction.x = _pitchDirectionControllerSettings.RefreshPid(RotationTo180(localOffset.x * -1), 0, deltaTime);
            correction.y = _yawDirectionControllerSettings.RefreshPid(RotationTo180(localOffset.y * -1), 0, deltaTime);
            correction.z = _rollDirectionControllerSettings.RefreshPid(RotationTo180(localOffset.z), 0, deltaTime);

            return VectorClampingScale(correction, AngleClamp);
        } 

        private Vector3 ComputeCorrection(float deltaTime)
        {
            var correction = Vector3.zero;

            if (!AShipInformation.Instance) 
                return correction / 2;
            
            correction += GetVelocityPidCorrection(deltaTime);
            correction += GetDirectionPidCorrection(deltaTime, AShipInformation.Instance.transform.rotation);

            return correction / 2;
        }
        
        public void SetXZStabilizationType(int stabilizationType) => XZStabilizationType = (StabilizationType)stabilizationType;

        public void SetYStabilizationType(int stabilizationType) => YStabilizationType = (StabilizationType)stabilizationType;
    }
}
