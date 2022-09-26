using UnityEngine;

namespace Project.Scripts.Tools
{
    /// <summary>
    /// Follows the target during the specified Life Cycle method.
    /// </summary>
    public class Follower : MonoBehaviour
    {
        private enum LifeCycle
        {
            FixedUpdate,
            Update,
            LateUpdate
        }
        
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 localPosition = Vector3.zero;
        [SerializeField] private Vector3 localRotation = Vector3.zero;
        [SerializeField] private LifeCycle executionTime = LifeCycle.Update;
 
        private void FixedUpdate()
        {
            if (executionTime != LifeCycle.FixedUpdate)
                return;

            Follow();
        }

        private void Update()
        {
            if (executionTime != LifeCycle.Update)
                return;

            Follow();
        }

        private void LateUpdate()
        {
            if (executionTime != LifeCycle.LateUpdate)
                return;

            Follow();
        }
    
        private void Follow()
        {
            if (!target)
                return;

            transform.rotation = target.rotation * Quaternion.Euler(localRotation);
            transform.position = target.TransformPoint(localPosition);
        }
    }
}
