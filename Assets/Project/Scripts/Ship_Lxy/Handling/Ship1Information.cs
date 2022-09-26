using System.Collections.Generic;
using Project.Scripts.Interactions;
using Project.Scripts.Interactions.Abstracts;
using Project.Scripts.Interactions.Addons;
using Project.Scripts.Ship.Abstracts;
using Project.Scripts.Ship.Components;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Scripts.Ship_Lxy.Handling
{
    /// <summary>
    /// The Ship1 definition.
    /// It contains four status:
    ///     -> OFF
    ///     -> Landed
    ///     -> UnderGravity
    ///     -> InSpace
    /// </summary>
    public class Ship1Information : AShipInformation
    {
        public const string LandedStatus = "Landed";
        public const string UnderGravityStatus = "UnderGravity";
        public const string NoGravityStatus = "InSpace";
        public const string AutomaticDrivingMode = "Automatic";

        public const float LandedMaximumSpeed = 0.05f;
        
        public ASwitchController MotorSwitch => motorSwitch;
        public RadarAltimeter RadarAltimeter => radarAltimeter;
        public TorquePid TorquePid => torquePid;
        public StateButtonAddon TorqueXZStabilizationButton => torqueXZStabilizationButton;
        public StateButtonAddon TorqueYStabilizationButton => torqueYStabilizationButton;
        public PositionPid PositionPid => positionPid;
        public StateButtonAddon PositionStabilizationButton => positionStabilizationButton;
        public StateButtonAddon LandingGearsButton => landingGearsButton;
        public LandingGear LandingGears => landingGears;
        public Thruster TakeOffThrusters => takeOffThrusters;
        public Handle TakeOffController => takeOffController;
        public Thruster TurboReactor => turboReactor;
        public Handle TurboReactorController => turboReactorController;
        public DoubleAxisThruster StrafeThrusters => strafeThrusters;
        public ADoubleAxisController XYStrafe => xYStrafe;
        public DoubleAxisThruster TorqueThrusters => torqueThrusters;
        public ADoubleAxisController TorqueController => torqueController;
        public AxisThruster YStrafeThrusters => yStrafeThrusters;
        public AxisThruster YTorqueThrusters => yTorqueThrusters;
        public ADoubleAxisController YTorqueZStrafe => yTorqueZStrafe;

        [Header("Tools")]
        [SerializeField] private RadarAltimeter radarAltimeter;
        [FormerlySerializedAs("torquePID")] [SerializeField] private TorquePid torquePid;
        [SerializeField] private StateButtonAddon torqueXZStabilizationButton;
        [SerializeField] private StateButtonAddon torqueYStabilizationButton;
        [FormerlySerializedAs("positionPID")] [SerializeField] private PositionPid positionPid;
        [SerializeField] private StateButtonAddon positionStabilizationButton;
        [Header("Mechanics")]
        [SerializeField] private ASwitchController motorSwitch;
        [SerializeField] private LandingGear landingGears;
        [SerializeField] private StateButtonAddon landingGearsButton;
        [Header("Thrusters")]
        [SerializeField] private Thruster takeOffThrusters;
        [SerializeField] private Handle takeOffController;
        [SerializeField] private Thruster turboReactor;
        [SerializeField] private Handle turboReactorController;
        [SerializeField] private DoubleAxisThruster strafeThrusters;
        [SerializeField] private ADoubleAxisController xYStrafe;
        [SerializeField] private DoubleAxisThruster torqueThrusters;
        [SerializeField] private ADoubleAxisController torqueController;
        [SerializeField] private AxisThruster yTorqueThrusters;
        [SerializeField] private AxisThruster yStrafeThrusters;
        [SerializeField] private ADoubleAxisController yTorqueZStrafe;

        protected override void Awake()
        {
            base.Awake();
            
            //DEBUG
            CurrentDrivingMode = AutomaticDrivingMode;
        }
        
        protected override IEnumerable<string> InitializeStatus()
        {
            return new[]
            {
                OffStatus,
                LandedStatus,
                UnderGravityStatus,
                NoGravityStatus
            };
        }

        protected override void RefreshStatus()
        {
            if (!Application.isPlaying)
                return;

            //Variables
            var landingGearsStatus = landingGears.CurrentStatus;
            var landingGearsGrounded = landingGears.IsGrounded;
            var rigidbodyVelocity = Rigidbody.velocity;
            var rigidbodyAngularVelocity = Rigidbody.angularVelocity;

            string status;

            //Landed
            if (landingGearsStatus == LandingGear.Status.Out && landingGearsGrounded &&
                rigidbodyVelocity.magnitude < LandedMaximumSpeed && rigidbodyAngularVelocity.magnitude < LandedMaximumSpeed)
                status = LandedStatus;
            //Flying
            else if (radarAltimeter.CurrentPhysicData.gravity.sqrMagnitude > 0)
                status = UnderGravityStatus;
            else
                status = NoGravityStatus;

            CurrentStatus = status;
        }
        /// <summary>
        /// Initialize the DrivingModes:
        ///   -> Automatic: The ship automatically control some commands depending on the current Status.
        ///   -> Assisted: The ship refuses to go further if the different commands aren't set well.
        ///   -> Attentive: The ship will give some advices and remind the player.
        ///   -> Manual: The player is free to drive as he want. But he can destroy the spaceship.
        /// </summary>
        /// <returns>The list of DrivingMode</returns>
        protected override IEnumerable<string> InitializeDrivingModes()
        {
            return new[]
            {
                AutomaticDrivingMode,
                ManualDrivingMode
            };
        }
    }
}
