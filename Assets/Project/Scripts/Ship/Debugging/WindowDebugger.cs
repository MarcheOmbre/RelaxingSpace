using Project.Scripts.Ship.Abstracts;
using TMPro;
using UnityEngine;

namespace Project.Scripts.Ship.Debugging
{
    public class WindowDebugger : MonoBehaviour
    {
        [SerializeField] private TMP_Text distanceFromOrigin;

        private void LateUpdate()
        {
            distanceFromOrigin.SetText(AShipInformation.Instance.transform.position.magnitude.ToString(Variables.FloatFormat));
        }
    }
}
