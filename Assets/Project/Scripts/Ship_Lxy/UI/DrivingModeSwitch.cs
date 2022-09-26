using Project.Scripts.Interactions.Abstracts;
using Project.Scripts.Ship.Abstracts;
using Project.Scripts.Ship_Lxy.Handling;
using UnityEngine;

namespace Project.Scripts.Ship_Lxy.UI
{
    /// <summary>
    /// Modify the specified Ship1Information DrivingMode on ASwitchController OnValueChanged.
    /// </summary>
    [RequireComponent(typeof(ASwitchController))]
    public class DrivingModeSwitch : MonoBehaviour
    {
        private ASwitchController _switchController;
  
        private void Awake()
        {
            //Initialization
            _switchController = gameObject.GetComponent<ASwitchController>();
        }

        private void OnEnable()
        {
            _switchController.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDisable()
        {
            _switchController.onValueChanged.RemoveListener(OnValueChanged);
        }
        
        private static void OnValueChanged(bool value)
        {
            var ship1Information = (Ship1Information)AShipInformation.Instance;
            ship1Information.CurrentDrivingMode = value ? Ship1Information.AutomaticDrivingMode : AShipInformation.ManualDrivingMode;
        }
    }
}