using Interhaptics.HandTracking;
using Interhaptics.Modules.Interaction_Builder.Core;
using Interhaptics.Modules.Interaction_Builder.Core.Abstract;
using UnityEngine;

namespace Interhaptics.ObjectSnapper.core
{
    public class IBHandActor : ARuntimeSnappableActor
    {
        #region Constants
        private const string DEBUG_NullTrackedHand = "The TrackedHand is NULL";
        private const string DEBUG_NullInteractionObject = "The TrackedHand's GameObject doesn't contain InteractionBodyPart";

        private const int VALUE_PalmIndex = 2;
        private const int VALUE_SnapFingerIndexOffset = 2;
        #endregion

        #region Variables
        /// <summary>
        /// The TrackedHand to follow.
        /// </summary>
        [SerializeField] private TrackedHand trackedHand = null;

        private InteractionBodyPart _interactionBodyPart = null;
        private string _lastMaskType = string.Empty;
        private bool _isSnapped = false;
        #endregion

        #region Life Cycle
        protected override void Awake()
        {
            base.Awake();

            //Get the TrackedHand
            if (trackedHand)
            {
                //Get interactionBodyPart
                _interactionBodyPart = trackedHand.GetComponentInChildren<InteractionBodyPart>();
                if (!_interactionBodyPart)
                    Debug.LogError(DEBUG_NullInteractionObject);
            }
            else
                Debug.LogError(DEBUG_NullTrackedHand);

            //Initialization
            this.OnPrimitiveChanged(null);
        }

        protected virtual void OnEnable()
        {
            if (_interactionBodyPart)
            {
                _interactionBodyPart.OnInteractionStartEvent.AddListener(OnInteractionStart);
                _interactionBodyPart.OnInteractionFinishEvent.AddListener(OnInteractionFinish);
            }

            if (trackedHand)
            {
                trackedHand.AfterHandRendering.AddListener(this.OnRefreshingTracking);
                trackedHand.AfterHandRendering.AddListener(this.OnRefreshingPose);
            }
        }

        //Disable the update. The IBHandActor update depends on the InteractionObject and TrackedHand update cycle.

        protected override void OnDisable()
        {
            base.OnDisable();

            if (_interactionBodyPart)
            {
                _interactionBodyPart.OnInteractionStartEvent.RemoveListener(OnInteractionStart);
                _interactionBodyPart.OnInteractionFinishEvent.RemoveListener(OnInteractionFinish);
            }

            if (trackedHand)
            {
                trackedHand.AfterHandRendering.RemoveListener(this.OnRefreshingPose);
                trackedHand.AfterHandRendering.RemoveListener(this.OnRefreshingTracking);
            }
        }
        #endregion

        #region Protecteds
        protected override void OnRefreshingPose()
        {
            _lastMaskType = string.Empty;

            base.OnRefreshingPose();
        }

        protected override string OnExtractingBodypartMaskLayer(Transform transform)
        {
            string bodyPart = string.Empty;

            if (transform)
            {
                if (transform.name.Contains(IBSnappingPrimitive.HANDPART_Thumb))
                    bodyPart = IBSnappingPrimitive.HANDPART_Thumb;
                else if (transform.name.Contains(IBSnappingPrimitive.HANDPART_Index))
                    bodyPart = IBSnappingPrimitive.HANDPART_Index;
                else if (transform.name.Contains(IBSnappingPrimitive.HANDPART_Middle))
                    bodyPart = IBSnappingPrimitive.HANDPART_Middle;
                else if (transform.name.Contains(IBSnappingPrimitive.HANDPART_Ring))
                    bodyPart = IBSnappingPrimitive.HANDPART_Ring;
                else if (transform.name.Contains(IBSnappingPrimitive.HANDPART_Pinky))
                    bodyPart = IBSnappingPrimitive.HANDPART_Pinky;
            }

            return bodyPart;
        }

        protected override bool IsPartialSnappingValid(int trackedTransformIndex, string maskType)
        {
            if (!maskType.Equals(_lastMaskType))
            {
                _isSnapped = false;

                if(this.TrackedTransforms.Length > VALUE_PalmIndex && this.ChildrenTransforms.Length > VALUE_PalmIndex)
                {
                    Transform lastChild = null;
                    int childIndex = trackedTransformIndex + VALUE_SnapFingerIndexOffset;

                    while (!_isSnapped && TrackedTransforms.Length > childIndex && this.ChildrenTransforms.Length > childIndex && (lastChild == null || lastChild == this.ChildrenTransforms[childIndex].parent))
                    {
                         Vector3 localPosition = TrackedTransforms[VALUE_PalmIndex].InverseTransformPoint(TrackedTransforms[childIndex].position);
                        _isSnapped = _currentSnappingPrimitive.IsInPrimitive(this.ChildrenTransforms[VALUE_PalmIndex].TransformPoint(localPosition));
                        lastChild = this.ChildrenTransforms[childIndex];
                        childIndex++;
                    }

                }

                _lastMaskType = maskType;
            }

            return _isSnapped;
        }

        protected virtual void OnInteractionStart(AInteractionBodyPart interactionBodyPart, InteractionObject interactionObject)
        {
            this.OnInteractionStateChanging(interactionObject != null ? interactionObject.GetComponent<SnappingObject>() : null);

            interactionObject.OnObjectComputed.AddListener(this.OnRefreshingTracking);

            if (trackedHand)
                trackedHand.AfterHandRendering.RemoveListener(this.OnRefreshingTracking);
        }

        protected virtual void OnInteractionFinish(AInteractionBodyPart interactionBodyPart, InteractionObject interactionObject)
        {
            this.OnInteractionStateChanging(null);

            if (trackedHand)
                trackedHand.AfterHandRendering.AddListener(this.OnRefreshingTracking);

            interactionObject.OnObjectComputed.RemoveListener(this.OnRefreshingTracking);
        }
        #endregion

        #region Publics
        protected override Animator GetKeyAnimator()
        {
            return trackedHand ? trackedHand.HandAnimator : null;
        }

        /// <summary>
        /// Modifies the TrackedHand mask when the SnappingPrimitive has changed. This prevents refreshing the tracking for the snapped fingers.
        /// </summary>
        public override void OnPrimitiveChanged(SnappingPrimitive snappingPrimitive)
        {
            base.OnPrimitiveChanged(snappingPrimitive);

            IBSnappingPrimitive currentIBSnappingPrimitive = (IBSnappingPrimitive)snappingPrimitive;

            if (trackedHand)
            {
                HandTracking.Tools.HandMask.MaskPart mask = HandTracking.Tools.HandMask.MaskPart.All;

                if (currentIBSnappingPrimitive)
                {
                    if (currentIBSnappingPrimitive.SnappingMask)
                        mask ^= currentIBSnappingPrimitive.SnappingMask.Mask;

                }

                trackedHand.CurrentHandMask = mask;
            }
        }
        #endregion
    }
}
