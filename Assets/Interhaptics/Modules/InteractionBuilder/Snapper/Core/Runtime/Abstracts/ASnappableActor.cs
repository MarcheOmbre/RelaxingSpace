using System.Linq;
using UnityEngine;

namespace Interhaptics.ObjectSnapper.core
{
    /// <summary>
    /// Base class for all the SnappableActors (Models and RuntimeSnappableActors).
    /// </summary>
    public abstract class ASnappableActor : MonoBehaviour
    {
        #region Enums
        protected enum Axis
        {
            X = 1,
            Y = 2,
            Z = 3,
            minusX = -1,
            minusY = -2,
            minusZ = -3
        }
        #endregion

        #region Constants
        private const string TOOLTIP_ForwardVector = "The ASnappableActor custom forward. For example, a hand forward should be the direction from its palm to its index distal";
        private const string TOOLTIP_UpwardVector = "The ASnappableActor custom upward. For example, a hand upward should be the direction from its palm to its back";
        #endregion

        #region Properties
        /// <summary>
        /// Get the model representative Up (from hand front to hand back),
        /// </summary>
        public Vector3 RepresentativeUp { get { return upwardVector; } }
        /// <summary>
        /// Get the model representative Forward (from the palm to the index distal),
        /// </summary>
        public Vector3 RepresentativeForward { get { return forwardVector; } }
        /// <summary>
        /// Returns the children transforms.
        /// </summary>
        protected Transform[] ChildrenTransforms { get { return _childrenTransforms; } }
        #endregion

        #region Variables
        [Tooltip(TOOLTIP_ForwardVector)] [SerializeField] protected Axis forwardAxis = Axis.Z;
        [Tooltip(TOOLTIP_UpwardVector)] [SerializeField] protected Axis upwardAxis = Axis.Y;

        private Transform[] _childrenTransforms = null;
        protected Vector3 forwardVector = Vector3.forward, upwardVector = Vector3.up;
        #endregion

        #region Life Cycle
        protected virtual void Awake()
        {
            //Set Axis
            int value = (int)forwardAxis;
            forwardVector = Vector3.zero;
            forwardVector[Mathf.Abs(value) - 1] = Mathf.Sign(value);

            value = (int)upwardAxis;
            upwardVector = Vector3.zero;
            upwardVector[Mathf.Abs(value) - 1] = Mathf.Sign(value);

            _childrenTransforms = (from child in gameObject.GetComponentsInChildren<Transform>(true) where child != this.transform select child).ToArray();
        }
        #endregion

        #region Publics
        /// <summary>
        /// Associated Animator.
        /// </summary>
        public abstract Animator Animator { get; }
        #endregion
    }
}
