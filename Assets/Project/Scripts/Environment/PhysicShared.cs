using System;
using UnityEngine;

namespace Project.Scripts.Environment
{
    public enum GravityType
    {
        None,
        Natural,
        Artificial,
        Mixed
    }

    [Serializable]
    public struct PhysicData
    {
        public GravityType gravityType;
        public float drag;
        public float angularDrag;
        public Vector3 gravity;
    }
}
