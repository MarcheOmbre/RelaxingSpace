using Project.Scripts.Environment;
using TMPro;
using UnityEngine;

namespace Project.Scripts.Ship.Debugging
{
    /// <summary>
    /// Displays the thruster data.
    /// </summary>
    public class GravityGraphicDebugger : MonoBehaviour
    {
        [SerializeField] private PhysicReceiver physicReceiver;
        [SerializeField] private TMP_Text gravityTypeText;
        [SerializeField] private TMP_Text gravityForceText;

        private void LateUpdate()
        {
            var physicData = physicReceiver.CurrentPhysicData;
            gravityTypeText.SetText(physicData.gravityType.ToString());
            gravityForceText.SetText(physicData.gravity.magnitude.ToString(Variables.FloatFormat));
        }
    }
}
