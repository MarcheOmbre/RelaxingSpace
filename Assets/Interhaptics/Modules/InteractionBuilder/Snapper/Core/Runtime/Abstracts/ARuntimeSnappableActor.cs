using Interhaptics.InteractionsEngine.Shared.Types;
using Interhaptics.Modules.Interaction_Builder.Core;
using System.Linq;
using UnityEngine;

namespace Interhaptics.ObjectSnapper.core
{
    public abstract class ARuntimeSnappableActor : ASnappableActor
    {
        #region Structures
        private struct UnitySpatialRepresentation
        {
            public Vector3 localPosition;
            public Quaternion localRotation;
        }

        private struct Velocities
        {
            public Vector3 velocity;
            public float angularVelocity;
        }
        #endregion

        #region Constants
        private const string DEBUG_TransformsCount = "The tracking reference children transforms count is different from this GameObject's children transforms count";

        private const float PLACEHOLDER_IdleSnappingDeltatime = 0f;
        private const float PLACEHOLDER_InteractionSnappingRootDeltatime = 0.01f;
        private const float PLACEHOLDER_InteractionSnappingSubDeltatime = 0.05f;
        private const bool PLACEHOLDER_SmoothDampOnIdle = false;
        #endregion

        #region Properties
        /// <summary>
        /// Returns the tracking transform followed by this ARuntimeSnappableActor.
        /// </summary>
        public Transform TrackingReference { get; private set; }
        /// <summary>
        /// Returns the tracking transform children array.
        /// </summary>
        public Transform[] TrackedTransforms { get; private set; }
        /// <summary>
        /// The SmoothDamp delta time corresponding to the root movement speed.
        /// </summary>
        public float IdleSnappingDeltatime 
        {
            get { return _idleSnappingDeltatime; }
            
            set
            {
                if (value < 0)
                    value = 0;

                _idleSnappingDeltatime = 0;
            }     
        }
        /// <summary>
        /// The SmoothDamp delta time corresponding to the root movement speed during an interaction.
        /// </summary>
        public float InteractionSnappingRootDeltatime
        {
            get { return _interactionSnappingRootDeltatime; }

            set
            {
                if (value < 0)
                    value = 0;

                _interactionSnappingRootDeltatime = 0;
            }
        }
        /// <summary>
        /// The SmoothDamp delta time corresponding to the children movement speed during an interaction.
        /// </summary>
        public float InteractionSnappingSubDeltatime
        {
            get { return _interactionSnappingSubDeltatime; }

            set
            {
                if (value < 0)
                    value = 0;

                _interactionSnappingSubDeltatime = 0;
            }
        }
        /// <summary>
        /// If true, the SmoothDamp is used even if the hand is not in interaction.
        /// </summary>
        public bool SmoothDampOnIdle { get; set; } = PLACEHOLDER_SmoothDampOnIdle;
        /// <summary>
        /// If false, the ARuntimeSnappableActor will follow the target but will not snap to the objects.
        /// </summary>
        public bool SnappingEnabled { get; set; }

        public sealed override Animator Animator => this.GetKeyAnimator();
        #endregion

        #region Variables
        /// <summary>
        /// If true, the snapping for this ARuntimeSnappableActor will be enabled on Start.
        /// </summary>
        [SerializeField] private bool snappingEnabledOnStart = true;

        protected SnappingPrimitive _currentSnappingPrimitive = null;
        protected SnappingObject _currentSnappingObject = null;

        private float _idleSnappingDeltatime =  PLACEHOLDER_IdleSnappingDeltatime;
        private float _interactionSnappingRootDeltatime = PLACEHOLDER_InteractionSnappingRootDeltatime;
        private float _interactionSnappingSubDeltatime = PLACEHOLDER_InteractionSnappingSubDeltatime;

        private UnitySpatialRepresentation[] _poseSpatialRepresentation = null;
        private Velocities[] _poseVelocities = null;

        private Vector3 _rootStartLocalPosition;
        private Quaternion _rootStartLocalRotation;
        private float _startFowardSign;

        private bool _isInteracting;
        private Vector3 _rootVelocity;
        private float _rootAngularVelocity;
        #endregion

        #region Life Cycle
        protected override void Awake()
        {
            base.Awake();

            _poseVelocities = new Velocities[this.ChildrenTransforms.Length];

            Animator animator = this.GetKeyAnimator();
            if (animator)
                this.TrackingReference = animator.transform;

            //Get the tracking reference children
            if (this.TrackingReference)
            {
                TrackedTransforms = (from transform in this.TrackingReference.GetComponentsInChildren<Transform>(true)
                                     where transform != this.TrackingReference
                                     select transform).ToArray();

                if (TrackedTransforms.Length != this.ChildrenTransforms.Length)
                {
                    TrackedTransforms = null;
                    Debug.LogError(DEBUG_TransformsCount);
                }
            }

            //Initialization
            this.OnPrimitiveChanged(null);
        }

        protected virtual void Start()
        {
            this.SnappingEnabled = snappingEnabledOnStart;
        }

        /// <summary>
        /// Resets the snapping data when the script is disabled.
        /// </summary>
        protected virtual void OnDisable()
        {
            this.OnInteractionStateChanging(null);
        }
        #endregion

        #region Privates
        private Vector3 GetPrimitiveComparativeDirection(SnappingPrimitive snappingPrimitive, Vector3 currentPosition)
        {
            Vector3 comparativeDirection = Vector3.zero;

            if (snappingPrimitive || !this.TrackingReference)
            {
                Quaternion primitiveRotation = snappingPrimitive.ShapeRotation;

                //Get the comparative direction.
                if (snappingPrimitive.shapePrimitive == PrimitiveShape.Torus)
                    comparativeDirection = Vector3.Cross((currentPosition - snappingPrimitive.ShapePosition).normalized, primitiveRotation * Vector3.forward);
                else if (snappingPrimitive.shapePrimitive == PrimitiveShape.Cylinder || snappingPrimitive.shapePrimitive == PrimitiveShape.Capsule)
                    comparativeDirection = primitiveRotation * Vector3.up;
            }

            return comparativeDirection;
        }
        #endregion

        #region Protected
        /// <summary>
        /// The inherited ARuntimeSnappableActor must call this method at the beginning and the end of an interaction.
        /// This method subscribes to or unsubscribes from the SnappingObject.
        /// </summary>
        /// <param name="snappingObject">The SnappingObject found on the object in interaction with the ARuntimeSnappableObject</param>
        protected void OnInteractionStateChanging(SnappingObject snappingObject)
        {
            if (snappingObject == _currentSnappingObject)
                return;

            if (_currentSnappingObject)
            {
                this.OnUnsubscribeSnappingObject(_currentSnappingObject);

                _currentSnappingObject.UnsubscribeActor(this);
                _currentSnappingObject = null; ;
            }

            if (snappingObject)
            {
                snappingObject.SubscribeActor(this, OnPrimitiveChanged);
                this.OnSubscribeSnappingObject(snappingObject);
            }

            _currentSnappingObject = snappingObject;
        }

        protected virtual void OnRefreshingTracking()
        {
            if (!this.TrackingReference)
                return;

            Vector3 position;
            Quaternion rotation;

            if (this.SnappingEnabled && _currentSnappingPrimitive)
            {
                if (!_currentSnappingPrimitive.IsFixedSnapping)
                {
                    SpatialRepresentation spatialRepresentation = new SpatialRepresentation
                    {
                        Position = IbTools.Convert(this.TrackingReference.position),
                        Rotation = IbTools.Convert(this.TrackingReference.rotation)
                    };

                    spatialRepresentation = _currentSnappingPrimitive.GetComputedSpatialRepresentation(spatialRepresentation, this);
                    position = IbTools.Convert(spatialRepresentation.Position);
                    rotation = IbTools.Convert(spatialRepresentation.Rotation);

                    if (_currentSnappingPrimitive.IsDirectionLocked)
                    {
                        Vector3 normal = this.GetPrimitiveComparativeDirection(_currentSnappingPrimitive, position);
                        if (normal != Vector3.zero)
                        {
                            if (Mathf.Sign(Vector3.SignedAngle(rotation * this.RepresentativeUp, rotation * this.RepresentativeForward, normal)) != _startFowardSign)
                                rotation *= Quaternion.Euler(this.RepresentativeUp * 180f);
                        }
                    }
                }
                else
                {
                    position = _currentSnappingPrimitive.transform.TransformPoint(_rootStartLocalPosition);
                    rotation = _currentSnappingPrimitive.transform.rotation * _rootStartLocalRotation;
                }
            }
            else
            {
                position = this.TrackingReference.position;
                rotation = this.TrackingReference.rotation;
            }

            if (_isInteracting || this.SmoothDampOnIdle)
            {
                float deltaTime = _isInteracting ? this.InteractionSnappingRootDeltatime : this.IdleSnappingDeltatime;
                position = Vector3.SmoothDamp(transform.position, position, ref _rootVelocity, deltaTime);
                rotation = MathematicsExtension.SmoothDamp(transform.rotation, rotation, ref _rootAngularVelocity, deltaTime);
            }

            transform.position = position;
            transform.rotation = rotation;
        }

        protected virtual void OnRefreshingPose()
        {
            if (this.TrackedTransforms == null)
                return;

            bool snap = false;

            for (int i = 0; i < this.ChildrenTransforms.Length; i++)
            {
                if (this.ChildrenTransforms[i] == null)
                    continue;

                string maskType = this.OnExtractingBodypartMaskLayer(this.ChildrenTransforms[i]);

                //Verify the mask
                if (_poseSpatialRepresentation != null && _currentSnappingPrimitive != null)
                {
                    if (_currentSnappingPrimitive.Mask != null && _currentSnappingPrimitive.Mask.Contains(maskType))
                    {
                        if (_currentSnappingPrimitive.IsPartialSnapping)
                            snap = this.IsPartialSnappingValid(i, maskType);
                    }
                    else
                        snap = true;
                }

                Vector3 position = Vector3.zero;
                Quaternion rotation = Quaternion.identity;


                if (this.SnappingEnabled && snap)
                {
                    position = _poseSpatialRepresentation[i].localPosition;
                    rotation = _poseSpatialRepresentation[i].localRotation;
                }
                else if (TrackedTransforms[i] != null)
                {
                    position = TrackedTransforms[i].localPosition;
                    rotation = TrackedTransforms[i].localRotation;
                }

                if (_isInteracting || this.SmoothDampOnIdle)
                {
                    float deltaTime = _isInteracting ? this.InteractionSnappingSubDeltatime : this.IdleSnappingDeltatime;
                    position = Vector3.SmoothDamp(this.ChildrenTransforms[i].localPosition, position, ref _poseVelocities[i].velocity, deltaTime);
                    rotation = MathematicsExtension.SmoothDamp(this.ChildrenTransforms[i].localRotation, rotation, ref _poseVelocities[i].angularVelocity, deltaTime);
                }

                this.ChildrenTransforms[i].localPosition = position;
                this.ChildrenTransforms[i].localRotation = rotation;

            }
        }

        /// <summary>
        /// Called after the ARuntimeSnappableActor has subscribed to the SnappingObject.
        /// </summary>
        protected virtual void OnSubscribeSnappingObject(SnappingObject snappingObject) { }

        /// <summary>
        /// Called after the ARuntimeSnappableActor has unsubscribed from the SnappingObject.
        /// </summary>
        protected virtual void OnUnsubscribeSnappingObject(SnappingObject snappingObject) { }

        /// <summary>
        /// The poses are stored with an associated AnimatorController Key.
        /// </summary>
        /// <returns>The Animator containing the AnimatorController Key</returns>
        protected abstract Animator GetKeyAnimator();

        /// <summary>
        /// This function is designed to extract a mask layer from each child of this ARuntimeSnappableActor.
        /// If this Bodypart mask layer doesn't exist in the SnappingPrimitive mask, then the bodypart is snapped. 
        /// Otherwise, if the SnappinPrimitive "IsPartialSnapping" is set to true, then the Bodypart is snapped according to the "IsPartialSnappingValid" output.
        /// </summary>
        /// <param name="transform">The child</param>
        /// <returns>Empty if the current snapping primitive Mask does not contain the current child mask layer</returns>
        /// <example><see cref="IBHandActor"/></example>
        /// <see cref="SnappingPrimitive.Mask"/>
        /// <see cref="SnappingPrimitive.isPartialSnapping"/>
        protected abstract string OnExtractingBodypartMaskLayer(Transform transform);

        /// <summary>
        /// Called if the current SnappingPrimitive Mask contains the current child layer and if the "IsPartialSnapping" method returns. 
        /// </summary>
        /// <param name="trackedTransformIndex">The index of the child to check the partial snapping</param>
        /// <param name="maskType">The mask type of the index</param>
        /// <returns>True if the current child can be snapped to the snapping primitive</returns>
        /// <example><see cref="IBHandActor"/></example>
        /// <see cref="SnappingPrimitive.Mask"/>
        /// <see cref="SnappingPrimitive.isPartialSnapping"/>
        protected abstract bool IsPartialSnappingValid(int trackedTransformIndex, string maskType);
        #endregion

        #region Publics
        /// <summary>
        /// Called by the subscribed SnappingObject when the ARuntimeSnappableActor is switching for a new SnappingPrimitive.
        /// </summary>
        /// <param name="snappingPrimitive">The new SnappingPrimitive</param>
        public virtual void OnPrimitiveChanged(SnappingPrimitive snappingPrimitive)
        {
            if (this.TrackingReference && snappingPrimitive)
            {
                SpatialRepresentation spatialRepresentation = snappingPrimitive.GetComputedSpatialRepresentation(new SpatialRepresentation
                {
                    Position = IbTools.Convert(this.TrackingReference.position),
                    Rotation = IbTools.Convert(this.TrackingReference.rotation),
                }, this);

                Quaternion rotation = IbTools.Convert(spatialRepresentation.Rotation);
                _rootStartLocalPosition = snappingPrimitive.transform.InverseTransformPoint(IbTools.Convert(spatialRepresentation.Position));
                _rootStartLocalRotation = Quaternion.Inverse(snappingPrimitive.transform.rotation) * rotation;

                //Get pose custom up direction
                _startFowardSign = Mathf.Sign(Vector3.SignedAngle(rotation * this.RepresentativeUp, rotation * 
                    this.RepresentativeForward, this.GetPrimitiveComparativeDirection(snappingPrimitive, IbTools.Convert(spatialRepresentation.Position))));
            }

            //Snapping pose
            SpatialRepresentation[] poseSP = snappingPrimitive ? snappingPrimitive.GetPose(this) : null;
            _poseSpatialRepresentation = null;
            _isInteracting = poseSP != null && poseSP.Length == this.ChildrenTransforms.Length;
            if (_isInteracting)
            {
                _poseSpatialRepresentation = new UnitySpatialRepresentation[poseSP.Length];

                for (int i = 0; i < _poseSpatialRepresentation.Length; i++)
                {
                    _poseSpatialRepresentation[i].localPosition = IbTools.Convert(poseSP[i].Position);
                    _poseSpatialRepresentation[i].localRotation = IbTools.Convert(poseSP[i].Rotation);
                    _poseVelocities[i].velocity = Vector3.zero;
                    _poseVelocities[i].angularVelocity = 0;
                }
            }

            _currentSnappingPrimitive = snappingPrimitive;
        }
        #endregion
    }
}