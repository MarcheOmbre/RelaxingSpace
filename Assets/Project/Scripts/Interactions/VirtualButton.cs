using Project.Scripts.Interactions.Abstracts;
using UnityEngine;

namespace Project.Scripts.Interactions
{
    /// <summary>
    /// Interactable physical button.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class VirtualButton : AButton
    {
        private static readonly int InteractableAnimatorHash = Animator.StringToHash("Interactable");
        private static readonly int ClickingAnimatorHash = Animator.StringToHash("Clicking");
        
        public bool IsInteractable { get; set; } = true;

        private Animator _animator;

        protected virtual void Awake()
        {
            _animator = gameObject.GetComponent<Animator>();
        }

        protected virtual void Update()
        {
            //If the button is not interactable, release all the actors.
            if (!IsInteractable && ActorsCount > 0)
                ForceStopInteraction();

            //Refresh animator and fingers
            _animator.SetBool(InteractableAnimatorHash, IsInteractable);
            if (IsInteractable)
                _animator.SetBool(ClickingAnimatorHash, IsClicked);
        }
        
        protected override bool IsClicking()
        {
            return ActorsCount > 0;
        }

        public override void AddActor(Transform actor)
        {
            if (!IsInteractable)
                return;

            base.AddActor(actor);
        }

        public override void RemoveActor(Transform actor)
        {
            if (!IsInteractable)
                return;

            base.RemoveActor(actor);
        }
    }
}