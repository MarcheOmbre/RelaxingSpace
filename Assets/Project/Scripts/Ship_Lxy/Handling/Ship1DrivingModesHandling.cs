using Project.Scripts.Ship.Components;
using UnityEngine;

namespace Project.Scripts.Ship_Lxy.Handling
{
    /// <summary>
    /// Represents the Ship1 components handler. It handles the ship different DrivingModes.
    /// </summary>
    [RequireComponent(typeof(Ship1Information))]
    public class Ship1DrivingModesHandling : MonoBehaviour
    {
        private Ship1Information _ship1Information;

        private void Awake()
        {
            _ship1Information = gameObject.GetComponent<Ship1Information>();
        }

        private void Update()
        {
            if (!_ship1Information.IsOn)
                return;

            var isAutomatic = _ship1Information.CurrentDrivingMode == Ship1Information.AutomaticDrivingMode;

            UIEnableHandling(isAutomatic);

            if (!isAutomatic)
                return;
            
            LandingGearsHandling();
            AutomaticTorquePidHandling();
            AutomaticPositionPidHandling();
            AutomaticTakeOffThrusterHandling();
        }

        private void UIEnableHandling(bool isAutomatic)
        {
            if (_ship1Information.TorqueXZStabilizationButton)
                _ship1Information.TorqueXZStabilizationButton.VirtualButton.IsInteractable = !isAutomatic;
            if (_ship1Information.TorqueYStabilizationButton)
                _ship1Information.TorqueYStabilizationButton.VirtualButton.IsInteractable = !isAutomatic;
        }

        /// <summary>
        /// Handles the LandingGear. 
        /// when the vehicle is hovering, the wheel are pull out.
        /// When the vehicle is in space, the wheels are retracted.
        /// </summary>
        private void LandingGearsHandling()
        {
            var isHovering = _ship1Information.RadarAltimeter.CurrentGroundInfo != null;

            if(_ship1Information.LandingGears.CurrentStatus == LandingGear.Status.Out && !isHovering ||
                _ship1Information.LandingGears.CurrentStatus == LandingGear.Status.In && isHovering)
                _ship1Information.LandingGears.Retract(!isHovering);
        }

        private void AutomaticTorquePidHandling()
        {
            if (_ship1Information.CurrentStatus == Ship1Information.NoGravityStatus)
                _ship1Information.TorqueXZStabilizationButton.SetState((int)TorquePid.StabilizationType.Velocity);
            else
                _ship1Information.TorqueXZStabilizationButton.SetState((int)TorquePid.StabilizationType.Direction);

            _ship1Information.TorqueYStabilizationButton.SetState((int)TorquePid.StabilizationType.Velocity);
        }

        private void AutomaticPositionPidHandling()
        {
            const int velocityState = (int)PositionPid.StabilizationType.Velocity;
            if (_ship1Information.CurrentStatus == Ship1Information.NoGravityStatus && _ship1Information.PositionStabilizationButton.CurrentState != velocityState)
                _ship1Information.PositionStabilizationButton.SetState(velocityState);
        }

        public void AutomaticTakeOffThrusterHandling() => _ship1Information.TakeOffThrusters.IsOn = _ship1Information.CurrentStatus != Ship1Information.NoGravityStatus;
    }
}
