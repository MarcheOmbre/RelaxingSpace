using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Scripts.Environment
{
    /// <summary>
    /// Manage the environment gravity. It applies a directional force (from outside to its center if gravity direction is not set) to each Rigidbody inside the influence radius.
    /// </summary>
    public class PhysicManager : MonoBehaviour
    {
#if UNITY_EDITOR
        private readonly Color _gravityInfluenceSphere = new Color(0, 0, 1, 0.5f);

        private void OnDrawGizmos()
        {
            if (UnityEditor.Selection.activeGameObject != gameObject) 
                return;
            
            Gizmos.color = _gravityInfluenceSphere;
            Gizmos.DrawSphere(transform.position, this.Radius);
        }
#endif
        private const float PlaceholderRadius = 100f;
        
        private float Radius
        {
            get => radius;

            set
            {
                if (value < 0)
                    radius = 0;
            }
        }
 
        public float GravityForce
        { 
            get => gravityForce;

            set
            {
                if (value < 0)
                    value = 0;

                gravityForce = value;
            }
        }

        public PhysicData PhysicData
        {
            get => physicData;

            set => physicData = value;
        }
        
        [SerializeField] [Min(0)] private float radius = PlaceholderRadius;
        [FormerlySerializedAs("gravityforce")] [SerializeField] [Min(0)] private float gravityForce;
        [SerializeField] private PhysicData physicData;

        private void Update()
        {
            //Get all the eligible Rigidbodies.
            var proximityGravityReceivers = 
                (from Collider collider in Physics.OverlapSphere(transform.position, radius)
                    let gravityReceiver = collider.attachedRigidbody ? collider.attachedRigidbody.GetComponent<PhysicReceiver>() : null
                    where  gravityReceiver != null select gravityReceiver).Distinct().ToArray();

            //Apply gravity
            foreach (var gravityReceiver in proximityGravityReceivers)
            {
                var currentPhysicData = PhysicData;
                
                currentPhysicData.gravity = (physicData.gravityType == GravityType.Artificial ? transform.TransformDirection(PhysicData.gravity).normalized :
                    (transform.position - gravityReceiver.Rigidbody.position).normalized) * GravityForce;

                gravityReceiver.RequestPhysicData(currentPhysicData);
            }
        }
    }
}
