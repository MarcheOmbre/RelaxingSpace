using Project.Scripts.Ship.Abstracts;
using TMPro;
using UnityEngine;

namespace Project.Scripts.Ship_Lxy.UI
{
    /// <summary>
    /// Script that animates the angular velocity representation.
    /// </summary>
    public class AngularVelocityPoint : MonoBehaviour
    {
        private const string DisplayFormat = "F3";

        private enum Axis
        {
            X,
            Y,
            Z
        }

        [SerializeField][Min(0)] private float velocityMaxAngle;
        [SerializeField] private float distanceFromCenter;
        [SerializeField] private Vector2 maxRepresentationAngle;
        [SerializeField] private Axis representationDirection = Axis.X;
        [SerializeField] private bool isInversed;
        [SerializeField] private TMP_Text outputText;

        private void LateUpdate()
        {
            if (!transform.parent || velocityMaxAngle == 0)
                return;

            //Get the value
            var value = AShipInformation.Instance.transform.InverseTransformDirection(AShipInformation.Instance.Rigidbody.angularVelocity)[(int)representationDirection];

            //Display the value
            outputText.SetText(value.ToString(DisplayFormat));

            //Clamp the value
            value = (Mathf.Clamp(value, -velocityMaxAngle, velocityMaxAngle) + velocityMaxAngle) / (velocityMaxAngle * 2);

            //Handle inversion
            if (isInversed)
                value = 1 - value;

            //Apply the position relative to the parent rotation
            transform.localPosition = Quaternion.Euler(Vector3.forward * 
                Mathf.Lerp(maxRepresentationAngle.x, maxRepresentationAngle.y, value)) *  (Vector3.up * distanceFromCenter);
        }
    }
}
