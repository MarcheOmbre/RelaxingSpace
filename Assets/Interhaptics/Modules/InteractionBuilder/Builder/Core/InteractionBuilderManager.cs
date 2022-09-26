using Interhaptics.InteractionsEngine.Shared.Types;
using Interhaptics.InteractionsEngine;

using Interhaptics.Modules.Interaction_Builder.Core.Abstract;

using UnityEngine;
using UnityEngine.Events;

namespace Interhaptics.Modules.Interaction_Builder.Core
{

    /// <summary>
    ///     The InteractionBuilderManager is here to manage all API calls
    /// </summary>
    [AddComponentMenu("Interhaptics/Interaction Builder/InteractionBuilderManager")]
    [RequireComponent(typeof(Interhaptics.HapticRenderer.Core.HapticManager))]
    public sealed class InteractionBuilderManager: MonoBehaviour
    {

        #region Log Messages
        private const string WARNING_NO_RIGHT_HAND = "<b>[InteractionBuilderManager]</b> You didn't set right hand";
        private const string WARNING_NO_LEFT_HAND = "<b>[InteractionBuilderManager]</b> You didn't set left hand";
        #endregion


        #region Consts
        private const string IB_VERSION = "20.11.02.00";
        #endregion


        #region Serialized Fields
        [Header("Body Parts")]
        [Header("Interaction Builder Version: " + IB_VERSION)]
        [SerializeField]
        [Tooltip("The right hand used")]
        private AInteractionBodyPart rightHand = null;
        [SerializeField]
        [Tooltip("The left hand used")]
        private AInteractionBodyPart leftHand = null;
        #endregion


        #region Private Fields
        private AInteractionBodyPart[] _leftFingers;
        private AInteractionBodyPart[] _rightFingers;

        private UnityAction _leftHandAction = null, _rightHandAction = null;
        #endregion


        #region Fields Getter
        /// <summary>
        ///     The right hand
        /// </summary>
        public AInteractionBodyPart RightHand => rightHand;
        
        /// <summary>
        ///     The left hand
        /// </summary>
        public AInteractionBodyPart LeftHand => leftHand;
        #endregion


        #region Life Cycles
        private void OnValidate()
        {
            if (rightHand && rightHand.bodyPart != BodyPart.RightHand)
                rightHand.bodyPart = BodyPart.RightHand;

            if (leftHand && leftHand.bodyPart != BodyPart.LeftHand)
                leftHand.bodyPart = BodyPart.LeftHand;
        }

        private void Awake()
        {
            //Actions definition
            _rightHandAction = () => UpdateInteractions(ref rightHand, ref leftHand);

            if (rightHand)
            {
                rightHand.OnInteractionStateChanged.AddListener(_rightHandAction);
                _rightFingers = rightHand.GetComponentsInChildren<AInteractionBodyPart>();
                foreach (AInteractionBodyPart aInteractionBodyPart in _rightFingers)
                {
                    if (aInteractionBodyPart == rightHand)
                        continue;

                    aInteractionBodyPart.OnInteractionStateChanged.AddListener(() =>
                    {
                        AInteractionBodyPart bodyPart = aInteractionBodyPart;
                        UpdateInteractions(ref bodyPart);
                    });
                }
            }
            else
                Debug.LogWarning(WARNING_NO_RIGHT_HAND);

            //Actions definition
            _leftHandAction = () => UpdateInteractions(ref leftHand, ref rightHand);

            if (leftHand)
            {
                leftHand.OnInteractionStateChanged.AddListener(_leftHandAction);
                _leftFingers = leftHand.GetComponentsInChildren<AInteractionBodyPart>();
                foreach (AInteractionBodyPart aInteractionBodyPart in _leftFingers)
                {
                    if (aInteractionBodyPart == leftHand)
                        continue;

                    aInteractionBodyPart.OnInteractionStateChanged.AddListener(() =>
                    {
                        AInteractionBodyPart bodyPart = aInteractionBodyPart;
                        UpdateInteractions(ref bodyPart);
                    });
                }
            }
            else
                Debug.LogWarning(WARNING_NO_LEFT_HAND);
        }

        private void LateUpdate()
        {
            if (leftHand)
            {
                UpdateHapticBodyPart(leftHand);
                if (!leftHand.IsInInteraction)
                    foreach (AInteractionBodyPart hibp in _leftFingers)
                        UpdateHapticBodyPart(hibp);
            }

            if (rightHand is null)
                return;

            UpdateHapticBodyPart(rightHand);
            if (rightHand.IsInInteraction) 
                return;
            foreach (AInteractionBodyPart hibp in _rightFingers)
                UpdateHapticBodyPart(hibp);
        }

        private void OnDestroy()
        {
            if (rightHand)
                rightHand.OnInteractionStateChanged.RemoveListener(_rightHandAction);

            if (leftHand)
                leftHand.OnInteractionStateChanged.RemoveListener(_leftHandAction);
        }
        #endregion


        #region Public Methods
        /// <summary>
        ///     Block interactions on an object
        /// </summary>
        /// <param name="obj">An haptic interaction object</param>
        /// <returns>True if the interaction was blocked, false otherwise</returns>
        public bool BlockObject(InteractionObject obj)
        {
            return obj.BlockObject();
        }

        /// <summary>
        ///     Unblock interactions on an object
        /// </summary>
        /// <param name="obj">An haptic interaction object</param>
        /// <returns>True if the interaction was unblocked, false otherwise</returns>
        public bool UnblockObject(InteractionObject obj)
        {
            return obj.UnblockObject();
        }

        /// <summary>
        ///     Try to block interactions on an object
        /// </summary>
        /// <param name="obj">An haptic interaction object</param>
        public void TryToBlockObject(InteractionObject obj)
        {
            obj.TryToBlockObject();
        }

        /// <summary>
        ///     Try to unblock interactions on an object
        /// </summary>
        /// <param name="obj">An haptic interaction object</param>
        public void TryToUnblockObject(InteractionObject obj)
        {
            obj.TryToUnblockObject();
        }

        /// <summary>
        ///     Force to finish interactions
        /// </summary>
        /// <param name="interactionObject">An haptic interaction object</param>
        /// <param name="bodyPartInteractionStrategy">The body part strategy which interact with</param>
        public void ForceFinishInteraction(InteractionObject interactionObject,
            BodyPartInteractionStrategy bodyPartInteractionStrategy)
        {
            InteractionEngineApi.ChangeObjectBlockingState(interactionObject.ObjectId, false);
            InteractionEngineApi.ChangeBodyPartBlockingState(bodyPartInteractionStrategy, false);
            interactionObject.FinishInteraction(bodyPartInteractionStrategy);
        }

        /// <summary>
        ///     Force to start interactions
        /// </summary>
        /// <param name="interactionObject">An haptic interaction object</param>
        /// <param name="bodyPartInteractionStrategy">The body part strategy which interact with</param>
        public void ForceStartInteraction(InteractionObject interactionObject, BodyPartInteractionStrategy bodyPartInteractionStrategy)
        {
            InteractionEngineApi.ChangeObjectBlockingState(interactionObject.ObjectId, false);
            InteractionEngineApi.ChangeBodyPartBlockingState(bodyPartInteractionStrategy, false);
            interactionObject.StartInteraction(bodyPartInteractionStrategy);
        }
        #endregion


        #region Private Methods
        private void UpdateInteractions(ref AInteractionBodyPart bodyPartToUpdate, ref AInteractionBodyPart theOther)
        {
            if (bodyPartToUpdate.InteractionObject == null || bodyPartToUpdate.InteractionObject.InteractionPrimitive == null)
                return;

            bool objectIsTwoHandInteraction = (bodyPartToUpdate.InteractionObject.InteractionPrimitive.bodyPart == BodyPartInteractionStrategy.TwoHands ||
             bodyPartToUpdate.InteractionObject.InteractionPrimitive.bodyPart == BodyPartInteractionStrategy.TwoHandsWithHead);

            if (!bodyPartToUpdate.IsInInteraction && bodyPartToUpdate.Interaction != InteractionTrigger.None &&
                bodyPartToUpdate.Interaction == bodyPartToUpdate.InteractionObject.InteractionPrimitive.interactionTrigger)
            {
                //If the interaction is two hands based and a hand is already grabbing the current object, then we stop the one hand interaction to switch on the two hands one.
                if (objectIsTwoHandInteraction && theOther.Interaction != InteractionTrigger.None && theOther.InteractionObject &&
                 theOther.InteractionObject.GetInstanceID() == bodyPartToUpdate.InteractionObject.GetInstanceID())
                {
                    theOther.InteractionObject.FinishInteraction(theOther.bodyPart, false);
                    bodyPartToUpdate.InteractionObject.StartInteraction(bodyPartToUpdate.InteractionObject.InteractionPrimitive.bodyPart);
                }
                else if(!objectIsTwoHandInteraction || bodyPartToUpdate.InteractionObject.InteractionPrimitive.oneHandSwitch)                //Otherwise, we simply start a one hand interaction.
                    bodyPartToUpdate.InteractionObject.StartInteraction(bodyPartToUpdate.bodyPart);
            }
            else if (bodyPartToUpdate.IsInInteraction && bodyPartToUpdate.Interaction == InteractionTrigger.None)
            {
                //If the interaction is two hands based and the two hands are grabbing the current object, then we stop the two hands interaction to switch on the one hand one.
                if (objectIsTwoHandInteraction && theOther.IsInInteraction && theOther.Interaction != InteractionTrigger.None &&
                    theOther.InteractionObject.GetInstanceID() == bodyPartToUpdate.InteractionObject.GetInstanceID())
                {
                    bool isOneHandSwitch = bodyPartToUpdate.InteractionObject.InteractionPrimitive.oneHandSwitch;

                    bodyPartToUpdate.InteractionObject.FinishInteraction(bodyPartToUpdate.InteractionObject.InteractionPrimitive.bodyPart, !isOneHandSwitch);

                    if(isOneHandSwitch)
                        bodyPartToUpdate.InteractionObject.StartInteraction(theOther.bodyPart);
                }
                else                 //Otherwise, we simply stop the one hand interaction.
                    bodyPartToUpdate.InteractionObject.FinishInteraction(bodyPartToUpdate.bodyPart); 
            }            
        }

        private static void UpdateInteractions(ref AInteractionBodyPart bodyPartToUpdate)
        {
            if (bodyPartToUpdate.InteractionObject == null)
                return;

            if (!bodyPartToUpdate.IsInInteraction && bodyPartToUpdate.Interaction != InteractionTrigger.None &&
                bodyPartToUpdate.Interaction ==
                bodyPartToUpdate.InteractionObject.InteractionPrimitive.interactionTrigger)
                bodyPartToUpdate.InteractionObject.StartInteraction(bodyPartToUpdate.bodyPart);
            else if (bodyPartToUpdate.IsInInteraction && bodyPartToUpdate.Interaction == InteractionTrigger.None)
                bodyPartToUpdate.InteractionObject.FinishInteraction(bodyPartToUpdate.bodyPart);
        }

        private static void UpdateHapticBodyPart(AInteractionBodyPart interactionBodyPart)
        {
            if (interactionBodyPart && interactionBodyPart.InteractionObject)
                interactionBodyPart.InteractionObject.EvaluateHapticAmplitude();
        }
        #endregion

    }

}