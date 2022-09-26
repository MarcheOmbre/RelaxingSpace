using Interhaptics.InteractionsEngine.Shared.Types;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using Interhaptics.Modules.Interaction_Builder.Core;
using UnityEditor;
#endif

namespace Interhaptics.ObjectSnapper.core
{
    [CreateAssetMenu(fileName = "Poses Data", menuName = "Interhaptics/Poses Data")]
    public class PosesData : ScriptableObject
    {
#if UNITY_EDITOR
        #region Constants
        private const string DEBUG_NullASnappableActor = "The ASnappableActor is null";
        private const string DEBUG_NullAnimator = "The ASnappableActor's Animator is null";
        private const string DEBUG_NullAnimatorController = "The Animator's AnimatorController is null!";
        private const string DEBUG_PoseCancelled = "Pose saving cancelled";
        private const string DEBUG_PoseSaved = "Pose saved";

        private const string LABEL_Title = "Override pose?";
        private const string LABEL_Content = "A pose associated to this AnimatorController already exists in this PosesData. Do you want to overwrite it?";
        private const string LABEL_Save = "Save";
        private const string LABEL_Cancel = "Cancel";
        #endregion

        #region Privates
        private void RefreshSaving()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        #endregion

        #region Publics
        /// <summary>
        /// Adds a pose entry to the Dictionary.
        /// </summary>
        /// <param name="snappableActor">Key</param>
        /// <returns>True on success</returns>
        public bool Save(ASnappableActor snappableActor)
        {
            string error = null;

            if (snappableActor == null)
                error = DEBUG_NullASnappableActor;
            else if (snappableActor.Animator == null)
                error = DEBUG_NullAnimator;
            else if (snappableActor.Animator.runtimeAnimatorController == null)
                error = DEBUG_NullAnimatorController;
            else
            {

                //Save the offset
                foreach (Transform transform in snappableActor.GetComponentsInChildren<Transform>())
                {
                    if (transform == snappableActor.transform)
                        continue;

                    SpatialRepresentation spatialRepresentation = new SpatialRepresentation
                    {
                        Position = IbTools.Convert(transform.localPosition),
                        Rotation = IbTools.Convert(transform.localRotation)
                    };

                    _serializableSpatialRepresentations.Add(new SerializableSpatialRepresentation(spatialRepresentation));
                }

                ActorData snappableActorData = new ActorData()
                {
                    transformsSP = _serializableSpatialRepresentations.ToArray()
                };

                _serializableSpatialRepresentations.Clear();

                string key = snappableActor.Animator.runtimeAnimatorController.name;
                bool containKey = posesDictionary.Contains(key);

                if (!containKey || EditorUtility.DisplayDialog(LABEL_Title, LABEL_Content, LABEL_Save, LABEL_Cancel))
                {
                    if (containKey)
                        posesDictionary.Remove(key);

                    posesDictionary.Add(key, snappableActorData);
                }
                else
                    error = DEBUG_PoseCancelled;
            }

            if (error == null)
                Debug.Log($"{ snappableActor.name}  { DEBUG_PoseSaved}");
            else if (error != DEBUG_PoseCancelled)
                Debug.LogError(error);

            bool success = (error == null);

            if (success)
                this.RefreshSaving();

            return success;
        }

        /// <summary>
        /// Removes a pose from the Dictionary.
        /// </summary>
        /// <param name="snappableActor">Key</param>
        /// <returns>True on success</returns>
        public bool Remove(ASnappableActor snappableActor)
        {
            bool success = false;

            if (snappableActor && snappableActor.Animator && snappableActor.Animator.runtimeAnimatorController)
                success = this.Remove(snappableActor.Animator.runtimeAnimatorController.name);

            return success;
        }
        /// <summary>
        /// Removes a pose from the Dictionary.
        /// </summary>
        /// <param name="key">The dictionary pose key</param>
        /// <returns>True on success</returns>
        public bool Remove(string key)
        {
            bool success = false;

            if (!string.IsNullOrWhiteSpace(key) && posesDictionary.Contains(key))
            {
                posesDictionary.Remove(key);
                success = true;
            }

            if (success)
                this.RefreshSaving();

            return success;
        }
        #endregion
#endif

        #region Variables
        [FormerlySerializedAs("snappingDataDictionary")]
        [SerializeField] [HideInInspector] public PosesDictionary posesDictionary = new PosesDictionary();

        private List<SerializableSpatialRepresentation> _serializableSpatialRepresentations = new List<SerializableSpatialRepresentation>();
        private List<SpatialRepresentation> _poseDataList = new List<SpatialRepresentation>();
        #endregion

        #region Publics
        /// <summary>
        /// Verifies if the entry exists in the Dictionary.
        /// </summary>
        /// <param name="aSnappableActor">Key</param>
        /// <returns>True on success</returns>
        public bool ContainData(ASnappableActor aSnappableActor)
        {
            return aSnappableActor && aSnappableActor.Animator && aSnappableActor.Animator.runtimeAnimatorController && posesDictionary.Contains(aSnappableActor.Animator.runtimeAnimatorController.name);
        }

        /// <summary>
        /// Tries to get the entry relative to an ASnappableActor.
        /// </summary>
        /// <param name="aSnappableActor">Key</param>
        /// <param name="poseData">The pose associated with this key</param>
        /// <returns>True on success</returns>
        public bool TryGetPose(ASnappableActor aSnappableActor, out SpatialRepresentation[] poseData)
        {
            bool success = false;

            if (aSnappableActor && aSnappableActor.Animator && aSnappableActor.Animator.runtimeAnimatorController &&
                posesDictionary.TryGetSnappableActorData(aSnappableActor.Animator.runtimeAnimatorController.name, out ActorData snappableActorData))
            {
                if (snappableActorData.transformsSP != null)
                {
                    for (int i = 0; i < snappableActorData.transformsSP.Length; i++)
                        _poseDataList.Add(snappableActorData.transformsSP[i].ToSpatialRepresentation());
                }

                success = true;
            }

            poseData = _poseDataList.ToArray();
            _poseDataList.Clear();

            return success;
        }
        #endregion
    }
}
