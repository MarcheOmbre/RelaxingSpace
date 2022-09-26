using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Interhaptics.InteractionsEngine.Shared.Types;
using Interhaptics.Modules.Interaction_Builder.Core;
using UnityEngine.Events;
using System;

namespace Interhaptics.ObjectSnapper.core
{
    /// <summary>
    /// Represents the bridge between the ASnappableActor and the different SnappingPrimitive set on the GameObject or its children.
    /// </summary>
    [AddComponentMenu("Interhaptics/Object Snapper/SnappingObject")]
    [RequireComponent(typeof(InteractionObject))]
    public class SnappingObject : MonoBehaviour
    {
        #region Constants
        private const string TOOLTIP_AutomaticallyFindNearest = "If true, the SnappableActor automatically switches to the nearest SnappingPrimitive";
        #endregion

        #region Structures
        private struct InteractionData
        {
            public SnappingPrimitive snappingPrimitive;
            public Action<SnappingPrimitive> callBack;
        }
        #endregion

        #region Classes
        /// <summary>
        /// Called when the ASnappableActor switches primitive.
        /// </summary>
        public class PrimitiveChangedEvent : UnityEvent<SnappingPrimitive> { }
        #endregion

        #region Variables
        /// <summary>
        /// If true, the SnappableActor automatically switches to the nearest SnappingPrimitive
        /// </summary>
        [Tooltip(TOOLTIP_AutomaticallyFindNearest)] [SerializeField] private bool automaticallyFindNearest = false;

        private List<SnappingPrimitive> _subscribedPrimitives = new List<SnappingPrimitive>();
        private Dictionary<ARuntimeSnappableActor, InteractionData> _subscribedRuntimeActors = new Dictionary<ARuntimeSnappableActor, InteractionData>();
        #endregion

        #region Properties
        /// <summary>
        /// Returns automaticallyFindNearest.
        /// </summary>
        /// <see cref="automaticallyFindNearest"/>
        public bool AutomaticallyFindNearest { get { return automaticallyFindNearest; } set { automaticallyFindNearest = value; } }

        /// <summary>
        /// Returns the list of SnappingPrimitives that subscribed to this SnappingObject (during Awake).
        /// </summary>
        public SnappingPrimitive[] SubscribedPrimitives { get { return _subscribedPrimitives.ToArray(); } }

        /// <summary>
        /// Returns the list of ARuntimeSnappableActor that subscribed to this SnappingObject (at the beginning of the interaction).
        /// </summary>
        public ARuntimeSnappableActor[] SubscribedRuntimeActors { get { return _subscribedRuntimeActors.Keys.ToArray(); } }
        #endregion

        #region Life Cycle
        private void Update()
        {
            ARuntimeSnappableActor[] runtimeSnappableActors = _subscribedRuntimeActors.Keys.ToArray();
            for (int i = 0; i < runtimeSnappableActors.Length; i++)
                this.Refresh(runtimeSnappableActors[i]);
        }
        #endregion

        #region Privates
        private SnappingPrimitive GetNearestSnappingPrimitive(ARuntimeSnappableActor runtimeSnappableActor, SpatialRepresentation spatialRepresentation)
        {
            SnappingPrimitive snappingPrimitive = null;

            if (runtimeSnappableActor != null && _subscribedPrimitives != null && _subscribedPrimitives.Count != 0)
            {
                float? lastDistance = null;
                for (int i = _subscribedPrimitives.Count - 1; i >= 0; i--)
                {
                    if (_subscribedPrimitives[i] == null)
                    {
                        _subscribedPrimitives.RemoveAt(i);
                        i++;
                    }
                    else
                    {
                        float newDistance = System.Numerics.Vector3.DistanceSquared(spatialRepresentation.Position, _subscribedPrimitives[i].GetComputedSpatialRepresentation(spatialRepresentation, runtimeSnappableActor).Position);

                        if ((lastDistance == null || newDistance < lastDistance.Value) && _subscribedPrimitives[i] != null)
                        {
                            lastDistance = newDistance;
                            snappingPrimitive = _subscribedPrimitives[i];
                        }
                    }
                }
            }

            return snappingPrimitive;
        }

        /// <summary>
        /// Refreshes the information of the SnappingPrimitive in accordance with the ARuntimeSnappableActor current spatial representation.
        /// </summary>
        /// <param name="runtimeSnappableActor">RuntimeSnappableActor to verify</param>
        private void Refresh(ARuntimeSnappableActor runtimeSnappableActor)
        {
            if (!runtimeSnappableActor || !runtimeSnappableActor.TrackingReference || _subscribedRuntimeActors == null || _subscribedRuntimeActors.Count == 0)
                return;

            if (!_subscribedRuntimeActors.TryGetValue(runtimeSnappableActor, out InteractionData interactionData))
                return;

            SpatialRepresentation spatialRepresentation = new SpatialRepresentation()
            {
                Position = IbTools.Convert(runtimeSnappableActor.TrackingReference.position),
                Rotation = IbTools.Convert(runtimeSnappableActor.TrackingReference.rotation)
            };

            if (interactionData.snappingPrimitive == null || automaticallyFindNearest)
            {
                SnappingPrimitive snappingPrimitive = this.GetNearestSnappingPrimitive(runtimeSnappableActor, spatialRepresentation);
                if (interactionData.snappingPrimitive != snappingPrimitive)
                {
                    interactionData.snappingPrimitive = snappingPrimitive;
                    interactionData.callBack?.Invoke(snappingPrimitive);
                    _subscribedRuntimeActors[runtimeSnappableActor] = interactionData;
                }
            }
        }
        #endregion

        #region Publics
        /// <summary>
        /// Subscribes an ARuntimeSnappableActor to the SnappingObject. Called by the runtimeSnappableActor at the beginning of the interaction.
        /// </summary>
        /// <param name="runtimeSnappableActor">The runtimeSnappableActor to subscribe</param>
        public void SubscribeActor(ARuntimeSnappableActor runtimeSnappableActor, Action<SnappingPrimitive> onPrimitiveChanged)
        {
            if (!runtimeSnappableActor || _subscribedRuntimeActors.ContainsKey(runtimeSnappableActor))
                return;

            _subscribedRuntimeActors.Add(runtimeSnappableActor, new InteractionData { callBack = onPrimitiveChanged });
        }
        /// <summary>
        /// Unsubscribes an ARuntimeSnappableActor from the SnappingObject. Called by the runtimeSnappableActor at the end of the interaction.
        /// </summary>
        /// <param name="runtimeSnappableActor">The runtimeSnappableActor to unsubscribe</param>
        public void UnsubscribeActor(ARuntimeSnappableActor runtimeSnappableActor)
        {
            if (!runtimeSnappableActor || !_subscribedRuntimeActors.TryGetValue(runtimeSnappableActor, out InteractionData interactionData))
                return;

            interactionData.callBack?.Invoke(null);
            _subscribedRuntimeActors.Remove(runtimeSnappableActor);
        }
        /// <summary>
        /// Subscribes a SnappingPrimitive to the SnappingObject. The subscription allows the ARuntimeSnappableActor to snap to it.
        /// </summary>
        /// <param name="snappingPrimitive">The SnappingPrimitive to subscribe</param>
        public void SubscribePrimitive(SnappingPrimitive snappingPrimitive)
        {
            if (snappingPrimitive && !_subscribedPrimitives.Contains(snappingPrimitive))
                _subscribedPrimitives.Add(snappingPrimitive);
        }
        /// <summary>
        /// Unsubscribes a SnappingPrimitive from the SnappingObject.
        /// </summary>
        /// <param name="snappingPrimitive">The SnappingPrimitive to unsubscribe</param>
        public void UnsubscribePrimitive(SnappingPrimitive snappingPrimitive)
        {
            if (snappingPrimitive && _subscribedPrimitives.Contains(snappingPrimitive))
                _subscribedPrimitives.Remove(snappingPrimitive);
        }
        #endregion
    }
}