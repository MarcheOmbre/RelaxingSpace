using System.Collections.Generic;
using System.Linq;
using Interhaptics.InteractionsEngine;
using Interhaptics.InteractionsEngine.Shared.Types;
using Interhaptics.Modules.Interaction_Builder.Core;
using Interhaptics.Modules.Interaction_Builder.Core.Abstract;
using Project.Scripts.Interactions.Abstracts;
using UnityEngine;

namespace Project.Scripts.Interhaptics
{
    /// <summary>
    /// Links the AShipCommand to the Interhaptics Interaction system.
    /// </summary>
    [RequireComponent(typeof(AShipCommand))]
    [RequireComponent(typeof(InteractionObject))]
    public class InterhapticsShipCommand : MonoBehaviour
    {
        private AShipCommand _shipCommand;
        private InteractionObject _interactionObject;
        private AInteractionBodyPart[] _interactionBodyParts;

        private void Awake()
        {
            _shipCommand = gameObject.GetComponent<AShipCommand>();
            _interactionObject = gameObject.GetComponent<InteractionObject>();
            _interactionBodyParts = FindObjectsOfType<AInteractionBodyPart>(true);
        }

        private void OnEnable()
        {
            _interactionObject.OnInteractionStart.AddListener(OnInteractionStart);
            _interactionObject.OnInteractionFinish.AddListener(OnInteractionFinish);
            _interactionObject.OnObjectComputed.AddListener(_shipCommand.UpdateInteraction);
        }

        private void OnDisable()
        {
            _interactionObject.OnInteractionStart.RemoveListener(OnInteractionStart);
            _interactionObject.OnInteractionFinish.RemoveListener(OnInteractionFinish);
            _interactionObject.OnObjectComputed.RemoveListener(_shipCommand.UpdateInteraction);
        }

        private void OnInteractionStart()
        {
            //Get the InteractionBodyPart
            var interactionTransforms = InteractionStrategyToTransform();

            foreach (var interactionTransform in interactionTransforms)
                _shipCommand.AddActor(interactionTransform);
        }

        private void OnInteractionFinish()
        {
            foreach (var subscribedActor in _shipCommand.SubscribedActors)
                _shipCommand.RemoveActor(subscribedActor);
        }
        
        private IEnumerable<Transform> InteractionStrategyToTransform()
        {
            var bodyParts = new List<BodyPart>();
            
            if (_interactionObject.InteractWith == BodyPartInteractionStrategy.TwoHands || _interactionObject.InteractWith == BodyPartInteractionStrategy.TwoHandsWithHead)
            {
                bodyParts.Add(BodyPart.LeftHand);
                bodyParts.Add(BodyPart.RightHand);
            }
            else
                bodyParts.Add(InteractionEngineApi.GetBodyPart(_interactionObject.InteractWith).BodyPart);

            return (from interactionBodyPart in _interactionBodyParts
                    where interactionBodyPart != null && bodyParts.Contains(interactionBodyPart.bodyPart)
                    select interactionBodyPart.transform).ToArray();
        }
    }
}
