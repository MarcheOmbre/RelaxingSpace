using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Project.Scripts.Interactions.Abstracts
{
    /// <summary>
    /// The base class for the interactions.
    /// </summary>
    public abstract class AShipCommand : MonoBehaviour
    {
        public enum ControlMode
        {
            Locked,
            Master,
            Slave
        }

        public ControlMode CurrentControlMode { get; set; } = ControlMode.Master;
        
        public Transform[] SubscribedActors => _subscribedActors.ToArray();
        
        public int ActorsCount => _subscribedActors.Count;


        public UnityEvent<Transform> onInteractionStart = new UnityEvent<Transform>();
        public UnityEvent<Transform> onInteractionFinish = new UnityEvent<Transform>();

        private readonly List<Transform> _subscribedActors = new List<Transform>();
        
        
        protected virtual void OnInteractionStart(Transform interactionTransform) 
        {
            onInteractionStart?.Invoke(interactionTransform);
        }
        
        protected virtual void OnInteractionFinish(Transform interactionTransform) 
        {
            onInteractionFinish?.Invoke(interactionTransform);
        }

        public virtual void AddActor(Transform actor)
        {
            if (!actor)
                return;

            if (_subscribedActors.Contains(actor)) 
                return;
            
            _subscribedActors.Add(actor);

            if (_subscribedActors.Count == 1)
                OnInteractionStart(actor);
        }
        
        public virtual void RemoveActor(Transform actor)
        {
            if (!actor)
                return;

            if (!_subscribedActors.Contains(actor)) 
                return;
            
            if (_subscribedActors.Count == 1)
                OnInteractionFinish(actor);

            _subscribedActors.Remove(actor);
        }

        public abstract void UpdateInteraction();

        public void ForceStopInteraction() 
        {
            for(var i = _subscribedActors.Count -1; i >= 0; i--)
                RemoveActor(_subscribedActors[i]);
        }
    }
}