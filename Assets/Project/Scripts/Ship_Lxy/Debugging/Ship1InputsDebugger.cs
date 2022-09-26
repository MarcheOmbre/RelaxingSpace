using System;
using Project.Scripts.Interactions.Abstracts;
using Project.Scripts.Ship.Abstracts;
using Project.Scripts.Ship.Debugging;
using Project.Scripts.Ship_Lxy.Handling;
using UnityEngine;

namespace Project.Scripts.Ship_Lxy.Debugging
{
    /// <summary>
    /// Allows to test the Ship1Information components through the keyboard.
    /// </summary>
    public class Ship1InputsDebugger : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private KeyCode motorInput;
        [SerializeField] private KeyCode landingGearsInput;
        [SerializeField] private SingleAxis takeOffThrusters;
        [SerializeField] private SingleAxis turboReactor;
        [SerializeField] private DoubleAxis strafeThrusters;
        [SerializeField] private SingleAxis yStrafeThrusters;
        [SerializeField] private DoubleAxis torqueThrusters;
        [SerializeField] private SingleAxis yTorqueThrusters;

        private void Awake()
        {
            //Reset commands
            takeOffThrusters.Reset();
            turboReactor.Reset();
            strafeThrusters.Reset();
            yStrafeThrusters.Reset();
            torqueThrusters.Reset();
            yTorqueThrusters.Reset();
        }
        private void Update()
        {
            var ship1Information = (Ship1Information)AShipInformation.Instance;

            //Motor
            if (Input.GetKeyDown(motorInput) && ship1Information.MotorSwitch)
            {
                if(ship1Information.MotorSwitch.CurrentControlMode == AShipCommand.ControlMode.Master)
                {
                    ship1Information.MotorSwitch.CurrentControlMode = AShipCommand.ControlMode.Slave;
                    ship1Information.MotorSwitch.SetSlaveValue(!ship1Information.MotorSwitch.Value);
                    ship1Information.MotorSwitch.CurrentControlMode = AShipCommand.ControlMode.Master;
                }
                else
                    turboReactor.Reset();
            }

            //Landing gears
            if (Input.GetKeyDown(landingGearsInput) && ship1Information.LandingGearsButton)
            {
                //TODO Controls for STEERING WHEEL
            }

            //Take-off
            if (RefreshControlMode(ship1Information.TakeOffController, Math.Abs(takeOffThrusters.Value - takeOffThrusters.DeadZone) > Mathf.Epsilon) == AShipCommand.ControlMode.Slave)
                ship1Information.TakeOffController.SetSlaveValue(takeOffThrusters.Value);
            else
                takeOffThrusters.Reset();

            //Turbo
            if (RefreshControlMode(ship1Information.TurboReactorController, Math.Abs(turboReactor.Value - turboReactor.DeadZone) > Mathf.Epsilon) == AShipCommand.ControlMode.Slave)
                ship1Information.TurboReactorController.SetSlaveValue(turboReactor.Value);
            else
                turboReactor.Reset();

            //Strafe
            if (RefreshControlMode(ship1Information.XYStrafe, strafeThrusters.Value != strafeThrusters.DeadZone) == AShipCommand.ControlMode.Slave)
                ship1Information.XYStrafe.SetSlaveValue(strafeThrusters.Value);
            else
                strafeThrusters.Reset();

            //Torque
            if (RefreshControlMode(ship1Information.TorqueController, torqueThrusters.Value != torqueThrusters.DeadZone) == AShipCommand.ControlMode.Slave)
                ship1Information.TorqueController.SetSlaveValue(torqueThrusters.Value);
            else
                torqueThrusters.Reset();

            //Directions controllers
            var directionValue = new Vector2(yTorqueThrusters.Value, yStrafeThrusters.Value);
            if (RefreshControlMode(ship1Information.YTorqueZStrafe, directionValue !=new Vector2(yTorqueThrusters.DeadZone, yStrafeThrusters.DeadZone)) == AShipCommand.ControlMode.Slave)
            {
                ship1Information.YTorqueZStrafe.SetSlaveValue(directionValue);
            }
            else
            {
                yTorqueThrusters.Reset();
                yStrafeThrusters.Reset();
            }
        }

        private AShipCommand.ControlMode RefreshControlMode(AShipCommand aShipCommand, bool keyEnable)
        {
            AShipCommand.ControlMode controlMode = AShipCommand.ControlMode.Master;

            if (aShipCommand)
            {
                controlMode = aShipCommand.CurrentControlMode;

                if (controlMode == AShipCommand.ControlMode.Master && keyEnable)
                    controlMode = AShipCommand.ControlMode.Slave;
                else if (controlMode == AShipCommand.ControlMode.Slave && !keyEnable)
                    controlMode = AShipCommand.ControlMode.Master;

                aShipCommand.CurrentControlMode = controlMode;
            }

            return controlMode;
        }
#endif
    }
}
