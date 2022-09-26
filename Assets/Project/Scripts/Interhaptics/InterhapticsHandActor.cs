using Interhaptics.Modules.Interaction_Builder.Core;
using Interhaptics.Modules.Interaction_Builder.Core.Abstract;
using Interhaptics.ObjectSnapper.core;
using UnityEngine;

namespace Project.Scripts.Interhaptics
{
    public class InterhapticsHandActor : IBHandActor
    {
        [SerializeField] private Collider[] contactColliders;

        protected override void OnInteractionStart(AInteractionBodyPart interactionBodyPart, InteractionObject interactionObject)
        {
            base.OnInteractionStart(interactionBodyPart, interactionObject);

            EnableColliders(false);
        }

        protected override void OnInteractionFinish(AInteractionBodyPart interactionBodyPart, InteractionObject interactionObject)
        {
            base.OnInteractionFinish(interactionBodyPart, interactionObject);

            EnableColliders(true);
        }

        private void EnableColliders(bool enable)
        {
            if (contactColliders == null) 
                return;
            
            foreach (var contactCollider in contactColliders)
            {
                if (contactCollider)
                    contactCollider.enabled = enable;
            }
        }
    }
}
