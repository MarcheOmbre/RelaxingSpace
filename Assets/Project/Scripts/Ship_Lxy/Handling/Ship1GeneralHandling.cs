using Project.Scripts.Ship.Components;
using UnityEngine;

namespace Project.Scripts.Ship_Lxy.Handling
{
    /// <summary>
    /// The Ship1 specific handling.
    /// </summary>
    [RequireComponent(typeof(Ship1Information))]
    public class Ship1GeneralHandling : MonoBehaviour
    {
        private const float InputWeight = 1f;

        private Ship1Information _ship1Information;
        private bool _positionPidSet;
        private bool _torquePidSet;

        protected virtual void Awake()
        {
            _ship1Information = gameObject.GetComponent<Ship1Information>();
        }

        protected virtual void OnEnable()
        {
            //Motor switch
            _ship1Information.MotorSwitch.onValueChanged.AddListener(this.MotorSwitch);
        }

        protected virtual void Update()
        {
            InputsHandling();
            TorquePidHandling();
            PositionPidHandling();
        }

        protected virtual void OnDisable()
        {
            //Motor switch
            _ship1Information.MotorSwitch.onValueChanged.RemoveListener(this.MotorSwitch);
        }

        private void InputsHandling()
        {
            //Motor On/Off
            if (_ship1Information.MotorSwitch.Value != _ship1Information.IsOn)
                _ship1Information.IsOn = _ship1Information.MotorSwitch.Value;

            //Take Off
            if (_ship1Information.TakeOffController.NormalizedValue > 0 && _ship1Information.TakeOffThrusters)
            {
                _ship1Information.TakeOffThrusters.RequestForce(_ship1Information.TakeOffController.NormalizedValue, InputWeight);
            }

            //Turbo
            if (_ship1Information.TurboReactorController.NormalizedValue > 0 && _ship1Information.TurboReactor)
                _ship1Information.TurboReactor.RequestForce(_ship1Information.TurboReactorController.NormalizedValue, InputWeight);


            //Torque
            if (_ship1Information.TorqueController.Value != Vector2.zero && _ship1Information.TorqueThrusters)
            {
                Vector2 torqueValue = _ship1Information.TorqueController.Value;
                torqueValue.x *= Mathf.Abs(torqueValue.x);
                torqueValue.y *= Mathf.Abs(torqueValue.y);
                _ship1Information.TorqueThrusters.RequestDirection(torqueValue, InputWeight);
            }

            //STRAFE AND TORQUE SPECIFICS
            var xzStrafe = Vector2.zero;

            //XY Strafe
            float yStrafe = _ship1Information.XYStrafe.Value.y;
            xzStrafe.x = _ship1Information.XYStrafe.Value.x;

            //Y Torque Z Strafe
            float yTorque = _ship1Information.YTorqueZStrafe.Value.x;
            xzStrafe.y = _ship1Information.YTorqueZStrafe.Value.y;

            if(_ship1Information.StrafeThrusters && xzStrafe != Vector2.zero)
                _ship1Information.StrafeThrusters.RequestDirection(xzStrafe, InputWeight);
            if(_ship1Information.YTorqueThrusters && yTorque != 0)
               _ship1Information.YTorqueThrusters.RequestDirection(yTorque * Mathf.Abs(yTorque), InputWeight);
            if(_ship1Information.YStrafeThrusters && yStrafe != 0)
                _ship1Information.YStrafeThrusters.RequestDirection(yStrafe, InputWeight);
        }

        private void MotorSwitch(bool enable)
        {
            _ship1Information.IsOn = enable;
        }

        private void TorquePidHandling()
        {
            switch (_torquePidSet)
            {
                //If the ship is hovering a groundable object, we set its Expected up to the ground normal. Otherwise, we try to keep the rotation of the spaceship.
                case false when _ship1Information.TorquePid.XZStabilizationType == TorquePid.StabilizationType.Direction:
                {
                    var rotation = _ship1Information.Rigidbody.rotation;
                    var forward = rotation * Vector3.forward;
                    var up = rotation * Vector3.up;

                    if (_ship1Information.RadarAltimeter.CurrentPhysicData.gravity.sqrMagnitude > 0)
                        up = _ship1Information.RadarAltimeter.CurrentPhysicData.gravity.normalized * -1;

                    _ship1Information.TorquePid.ExpectedRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(forward, up), up);
                    _torquePidSet = true;
                    break;
                }
                case true when _ship1Information.TorquePid.XZStabilizationType != TorquePid.StabilizationType.Direction:
                    _torquePidSet = false;
                    break;
            }
        }

        private void PositionPidHandling()
        {
            //Refresh PositionPID position
            if (_ship1Information.PositionPid.PositionStabilizationType == PositionPid.StabilizationType.Position)
            {
                if (_positionPidSet) 
                    return;
                
                _ship1Information.PositionPid.ExpectedPosition = _ship1Information.Rigidbody.position;
                _positionPidSet = true;
            }
            else if (_positionPidSet)
                _positionPidSet = false;
        }
    }
}
