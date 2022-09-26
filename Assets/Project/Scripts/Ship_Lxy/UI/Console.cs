using Project.Scripts.Interactions;
using Project.Scripts.Ship.Abstracts;
using Project.Scripts.Ship.Components;
using Project.Scripts.Ship_Lxy.Handling;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Scripts.Ship_Lxy.UI
{
    /// <summary>
    /// Class that tracks and writes text in the console.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class Console : MonoBehaviour
    {
        private enum CommandType
        {
            None,
            User,
            Superuser,
            System
        }

        private const string UserLabel = "$ : ";
        private const string SuperuserLabel = "# : ";
        private const string SystemLabel = "=> ";
        private const string EnabledLabel = "on";
        private const string DisabledLabel = "off";

        private const string WelcomeLog = "Welcome! Wishing you a safe and relaxing journey";

        private const string SessionRecoveringLog =
            "Session recovered, please do not turn off the system during the flight";

        private const string StatusLog = "status changed";
        private const string DrivingModeLog = "driving mode changed";

        private const string LightSwitchLog = "light switch";
        private const string ScreenSwitchLog = "screen switch";
        private const string AutomaticModeSwitchLog = "automatic mode switch";

        private const string PositionPidStabilizationLog = "position stabilization type";
        private const string TorquePidYStabilizationLog = "torque Y stabilization type";
        private const string TorquePidXZStabilizationLog = "torque XZ stabilization type";

        [SerializeField] private Ship1Information ship1Information;
        [SerializeField] [Min(0)] private int maxCharacter = 600;
        [FormerlySerializedAs("lighSwitch")] [SerializeField] private Switch lightSwitch;
        [SerializeField] private Switch screenSwitch;
        [SerializeField] private Switch automaticSwitch;

        private TMP_Text _contentText;

        //Status message
        private bool _isInitialized;
        private CommandType _userPrefix = CommandType.User;

        //Tracking
        private PositionPid.StabilizationType _lastTrackedPositionPid = PositionPid.StabilizationType.None;
        private TorquePid.StabilizationType _lastYTrackedTorquePid = TorquePid.StabilizationType.None;
        private TorquePid.StabilizationType _lastXZTrackedTorquePid = TorquePid.StabilizationType.None;

        private void Awake()
        {
            //Initializing
            _contentText = gameObject.GetComponent<TMP_Text>();
            ship1Information.onStatusChanged.AddListener(OpeningMessage);
        }

        private void OnEnable()
        {
            //Switch
            lightSwitch.onValueChanged.AddListener(this.TrackLightSwitch);
            screenSwitch.onValueChanged.AddListener(this.TrackScreenSwitch);
            automaticSwitch.onValueChanged.AddListener(this.TrackAutomaticMode);

            //Components
            ship1Information.onStatusChanged.AddListener(TrackStatus);
            ship1Information.onDrivingModeChanged.AddListener(TrackDrivingMode);
        }

        private void Update()
        {
            //Refresh user prefix
            _userPrefix = ship1Information.CurrentDrivingMode == Ship1Information.AutomaticDrivingMode
                ? CommandType.User
                : CommandType.Superuser;

            //Tracking
            TrackPositionPid();
            TrackTorquePid();
        }

        private void OnDisable()
        {
            //Switch
            lightSwitch.onValueChanged.RemoveListener(this.TrackLightSwitch);
            screenSwitch.onValueChanged.RemoveListener(this.TrackScreenSwitch);
            automaticSwitch.onValueChanged.RemoveListener(this.TrackAutomaticMode);

            //Components
            ship1Information.onStatusChanged.RemoveListener(this.TrackStatus);
            ship1Information.onDrivingModeChanged.RemoveListener(this.TrackDrivingMode);

            _isInitialized = false;
        }

        private void OnDestroy()
        {
            //Events
            ship1Information.onStatusChanged.RemoveListener(this.OpeningMessage);
        }

        private void OpeningMessage(string status)
        {
            if (_isInitialized)
                return;

            if (status != AShipInformation.OffStatus)
            {
                if (status == Ship1Information.LandedStatus)
                {
                    //Welcome message
                    _contentText.text = string.Empty;
                    WriteLine(CommandType.System, WelcomeLog);
                }
                else
                    WriteLine(CommandType.System, SessionRecoveringLog);

                _isInitialized = true;
            }
            else
                _isInitialized = false;
        }

        private void TrackStatus(string status)
        {
            WriteLine(CommandType.System, $"{StatusLog} : {status}");
        }

        private void TrackDrivingMode(string drivingMode)
        {
            WriteLine(CommandType.System, $"{DrivingModeLog} : {drivingMode}");
        }

        private void TrackPositionPid()
        {
            var stabilizationType = ship1Information.PositionPid.PositionStabilizationType;

            if (_lastTrackedPositionPid == stabilizationType)
                return;

            WriteLine(_userPrefix, $"{PositionPidStabilizationLog} : {stabilizationType}");
            _lastTrackedPositionPid = stabilizationType;
        }

        /// <summary>
        /// Tracks the Position PID current StabilizationType.
        /// </summary>
        private void TrackTorquePid()
        {
            //Y Stabilization
            var stabilizationType = ship1Information.TorquePid.YStabilizationType;
            if (_lastYTrackedTorquePid != stabilizationType)
            {
                WriteLine(_userPrefix, $"{TorquePidYStabilizationLog} : {stabilizationType}");
                _lastYTrackedTorquePid = stabilizationType;
            }

            //XZ Stabilization
            stabilizationType = ship1Information.TorquePid.XZStabilizationType;

            if (_lastXZTrackedTorquePid == stabilizationType)
                return;

            WriteLine(_userPrefix, $"{TorquePidXZStabilizationLog} : {stabilizationType}");
            _lastXZTrackedTorquePid = stabilizationType;
        }

        private void TrackLightSwitch(bool enable)
        {
            WriteLine(_userPrefix, $"{LightSwitchLog} : {(enable ? EnabledLabel : DisabledLabel)}");
        }

        private void TrackScreenSwitch(bool enable)
        {
            WriteLine(_userPrefix, $"{ScreenSwitchLog} : {(enable ? EnabledLabel : DisabledLabel)}");
        }

        private void TrackAutomaticMode(bool enable)
        {
            WriteLine(_userPrefix, $"{AutomaticModeSwitchLog} : {(enable ? EnabledLabel : DisabledLabel)}");
        }

        private void WriteLine(CommandType commandType, string content)
        {
            //Checking
            if (string.IsNullOrWhiteSpace(content))
                return;

            //Add line return
            if (!string.IsNullOrWhiteSpace(_contentText.text))
                content += "\n";

            //Add user mark
            switch (commandType)
            {
                case CommandType.User:
                    content = UserLabel + content;
                    break;
                case CommandType.Superuser:
                    content = SuperuserLabel + content;
                    break;
                case CommandType.System:
                    content = SystemLabel + content;
                    break;
                case CommandType.None:
                default:
                    break;
            }

            //Clamp and set the string
            content += _contentText.text;
            _contentText.text = content.Substring(0, Mathf.Min(content.Length, maxCharacter));
        }
    }
}