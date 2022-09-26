using System;
using Project.Scripts.Ship.Abstracts;
using TMPro;
using UnityEngine;

namespace Project.Scripts.Ship_Lxy.UI
{
    /// <summary>
    /// The script that handles the ship1 top UI.
    /// </summary>
    public class TopUI : MonoBehaviour
    {
        private const float RefreshTime = 0.5f;
        private const string VelocityFormat = "F0";
        private const string AngularVelocityFormat = "F2";
        private const string TimeFormat = "hh:mm";

        [SerializeField] private TMP_Text velocityValueText;
        [SerializeField] private TMP_Text angularVelocityText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text drivingModeText;
        [SerializeField] private TMP_Text timeText;

        private float _lastRefreshTime;

        private void LateUpdate()
        {
            var currentTime = Time.time;
            if (currentTime - _lastRefreshTime <= RefreshTime)
                return;


            velocityValueText.SetText(AShipInformation.Instance.Rigidbody.velocity.magnitude.ToString(VelocityFormat));
            angularVelocityText.SetText(AShipInformation.Instance.Rigidbody.angularVelocity.magnitude.ToString(AngularVelocityFormat));
            statusText.SetText(AShipInformation.Instance.CurrentStatus);
            drivingModeText.SetText(AShipInformation.Instance.CurrentDrivingMode);
            timeText.SetText(DateTime.Now.ToUniversalTime().ToString(TimeFormat));

            _lastRefreshTime = currentTime;
        }
    }
}