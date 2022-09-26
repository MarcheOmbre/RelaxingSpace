using UnityEngine;
using UnityEngine.Events;

namespace Project.Scripts.Tools
{
    /// <summary>
    /// Shares the collisions and triggers information to the subscribed objects.
    /// </summary>
    public class CollisionSharing : MonoBehaviour
    {
        public UnityEvent<Collision> onCollisionEnter = new UnityEvent<Collision>();
        public UnityEvent<Collision> onCollisionExit = new UnityEvent<Collision>();
        public UnityEvent<Collider> onTriggerEnter = new UnityEvent<Collider>();
        public UnityEvent<Collider> onTriggerExit = new UnityEvent<Collider>();

        private void OnCollisionEnter(Collision collision) => onCollisionEnter.Invoke(collision);

        private void OnCollisionExit(Collision collision) => onCollisionExit.Invoke(collision);

        private void OnTriggerEnter(Collider other) => onTriggerEnter.Invoke(other);

        private void OnTriggerExit(Collider other) => onTriggerExit.Invoke(other);
    }
}
