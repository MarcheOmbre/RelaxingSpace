using Project.Scripts.Ship.Components;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Scripts.Ship.Debugging
{
    /// <summary>
    /// Displays the thruster data.
    /// </summary>
    public class ThrusterGraphicDebugger : MonoBehaviour
    {
        [SerializeField] private Thruster thruster;
        [FormerlySerializedAs("pourcentUseText")] [SerializeField] private TMP_Text percentUseText = null;

        private void LateUpdate()
        {
            percentUseText.SetText($"{Mathf.CeilToInt(thruster.CurrentNormalizedForce * 100)} %");
        }
    }
}
