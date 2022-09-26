using Project.Scripts.Ship.Abstracts;
using UnityEngine;

namespace Project.Scripts.Ship.Debugging
{
    /// <summary>
    /// A script that allow to enable some testing functions.
    /// </summary>
    public class DebugFunctions : MonoBehaviour
    {
        [SerializeField][Min(0)] private float clampingDistance;

        private void Update()
        {
            if (!(clampingDistance > 0)) 
                return;
            
            var direction = Vector3.ClampMagnitude(AShipInformation.Instance.transform.position, clampingDistance);
            AShipInformation.Instance.transform.position = direction;
        }
    }
}
