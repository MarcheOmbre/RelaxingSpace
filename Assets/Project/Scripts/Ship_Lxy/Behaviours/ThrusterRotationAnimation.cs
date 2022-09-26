using Project.Scripts.Ship.Components;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Scripts.Ship_Lxy.Behaviours
{
    /// <summary>
    /// Rotates the reactor depending on the force of the specified Reactor.
    /// </summary>
    public class ThrusterRotationAnimation : MonoBehaviour
    {
        [SerializeField] private Thruster thruster;
        [FormerlySerializedAs("thrusterRotativePart")] [SerializeField] private Transform thrusterRotatePart;
        [SerializeField][Min(0)] private float maximumSpeed = 500f;
        
        private void Update()
        {
            //Apply the rotation
            thrusterRotatePart.rotation *= Quaternion.Euler(Vector3.forward * maximumSpeed * thruster.CurrentNormalizedForce * Time.deltaTime);
        }
    }
}
