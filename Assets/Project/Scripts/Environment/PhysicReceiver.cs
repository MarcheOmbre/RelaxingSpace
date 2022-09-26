using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Environment
{
    /// <summary>
    /// Script that receives the different gravity modifications (to be able to track it).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicReceiver : MonoBehaviour
    {
        public Rigidbody Rigidbody { get; private set; }

        public PhysicData CurrentPhysicData { get; private set; }


        [SerializeField] private PhysicData defaultPhysicData;

        private readonly List<PhysicData> _addedPhysicData = new List<PhysicData>();
        
        private void Awake()
        {
            Rigidbody = gameObject.GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (Rigidbody.useGravity)
                Rigidbody.AddForce(CurrentPhysicData.gravity, ForceMode.Acceleration);
        }

        private void Update()
        {
            //Add physic no PhysicManager is calling this script.
            if (_addedPhysicData.Count == 0)
                _addedPhysicData.Add(defaultPhysicData);

            //Computes the new physicData.
            PhysicData newPhysicData = default;

            for (var i = 0; i < _addedPhysicData.Count; i++)
            {
                //Gravity type
                if (newPhysicData.gravityType == GravityType.None)
                    newPhysicData.gravityType = _addedPhysicData[i].gravityType;
                else if (newPhysicData.gravityType != GravityType.Mixed && newPhysicData.gravityType != _addedPhysicData[i].gravityType)
                    newPhysicData.gravityType = GravityType.Mixed;

                newPhysicData.drag += _addedPhysicData[i].drag;
                newPhysicData.angularDrag += _addedPhysicData[i].angularDrag;
                newPhysicData.gravity += _addedPhysicData[i].gravity;
            }

            //Last setup
            newPhysicData.drag /= _addedPhysicData.Count;
            newPhysicData.angularDrag /= _addedPhysicData.Count;

            CurrentPhysicData = newPhysicData;

            //Cleans the variables
            _addedPhysicData.Clear();

            //Applies
            Rigidbody.drag = CurrentPhysicData.drag;
            Rigidbody.angularDrag = CurrentPhysicData.angularDrag;
        }

        public void RequestPhysicData(PhysicData physicData)
        {
            _addedPhysicData.Add(physicData);
        }
    }
}
