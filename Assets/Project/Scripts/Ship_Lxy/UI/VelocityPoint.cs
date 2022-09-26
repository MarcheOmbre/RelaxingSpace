using Project.Scripts.Ship.Abstracts;
using TMPro;
using UnityEngine;

namespace Project.Scripts.Ship_Lxy.UI
{
    /// <summary>
    /// Script that animates the angular velocity representation.
    /// </summary>
    public class VelocityPoint : MonoBehaviour
    {
        private const string DisplayFormat = "F1";

        private enum Axis
        {
            X,
            Y,
            Z
        }

        [SerializeField][Min(0)] private float maxVelocity;
        [SerializeField] private float maxDistance;
        [SerializeField] private Vector2 moveLocalAxis = Vector2.zero;
        [SerializeField] private Axis representationDirection = Axis.X;
        [SerializeField] private bool isInversed;
        [SerializeField] private TMP_Text outputText;

        private void Awake()
        {
            moveLocalAxis = moveLocalAxis.normalized;
        }

        private void LateUpdate()
        {
            if (!transform.parent || maxVelocity == 0)
                return;

            //Get the value
            var value = AShipInformation.Instance.transform.InverseTransformDirection(AShipInformation.Instance.Rigidbody.velocity)[(int)representationDirection];

            //Display the value
            outputText.SetText(value.ToString(DisplayFormat));

            //Clamp the value
            value = (Mathf.Clamp(value, -maxVelocity, maxVelocity) + maxVelocity) / (maxVelocity * 2);

            //Handle inversion
            if (isInversed)
                value = 1 - value;

            //Apply the new position
            transform.localPosition = moveLocalAxis * Mathf.Lerp(-maxDistance, maxDistance, value);
        }
    }
}
