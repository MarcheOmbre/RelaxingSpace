#if UNITY_EDITOR
using Interhaptics.InteractionsEngine.Shared.Types;
using Interhaptics.Modules.Interaction_Builder.Core;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Interhaptics.ObjectSnapper.core
{
    /// <summary>
    /// Model used by the SnappingPrimitive. This class contains a "SnapTo" method, called by the SnappingPrimitive during the snapping edition.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(Animator))]
    public class ModelSnappableActor : ASnappableActor
    {
        #region Structures
        private struct UnitySpatialRepresentation
        {
            public Vector3 localPosition;
            public Quaternion localRotation;
        }
        #endregion

        #region Variables
        private Animator _animator = null;
        private SnappingPrimitive _currentSnappingPrimitive = null;
        private UnitySpatialRepresentation[] _originalSpatialRepresentation = null;
        #endregion

        #region Life Cycle
        protected override void Awake()
        {
            base.Awake();

            _animator = gameObject.GetComponent<Animator>();

            _originalSpatialRepresentation = (from child in this.ChildrenTransforms
                                              select new UnitySpatialRepresentation { 
                                                  localPosition = child ? child.localPosition : Vector3.zero, 
                                                  localRotation = child ? child.localRotation : Quaternion.identity
                                              }).ToArray();
        }

        private void OnEnable()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += EditorUpdate;
#else
            SceneView.onSceneGUIDelegate += HandUpdate;
#endif
        }

        protected void OnDisable()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= EditorUpdate;
#else
            SceneView.onSceneGUIDelegate -= HandUpdate;
#endif
        }
        #endregion

        #region Privates
        private void EditorUpdate(SceneView sceneView)
        {
            if (!_currentSnappingPrimitive)
                return;

            SpatialRepresentation spatialRepresentation = _currentSnappingPrimitive.GetModelSpatialRepresentation(this);
            transform.SetPositionAndRotation(IbTools.Convert(spatialRepresentation.Position), IbTools.Convert(spatialRepresentation.Rotation));
        }
        #endregion

        #region Override
        public override Animator Animator => _animator;

        public void SnapTo(SnappingPrimitive snappingPrimitive) 
        {
            if (_currentSnappingPrimitive == snappingPrimitive)
                return;

            //Check for corrupted data
            SpatialRepresentation[] poseSP = snappingPrimitive ? snappingPrimitive.GetPose(this) : null;
            if (poseSP != null && poseSP.Length != this.ChildrenTransforms.Length)
                poseSP = null;

            for (int i = 0; i < this.ChildrenTransforms.Length; i++)
            {
                if (this.ChildrenTransforms[i] == null)
                    continue;

                this.ChildrenTransforms[i].localPosition = poseSP != null ? IbTools.Convert(poseSP[i].Position) : _originalSpatialRepresentation[i].localPosition;
                this.ChildrenTransforms[i].localRotation = poseSP != null ? IbTools.Convert(poseSP[i].Rotation) : _originalSpatialRepresentation[i].localRotation;
            }

            _currentSnappingPrimitive = snappingPrimitive;
        }
        #endregion
    }
}
#endif
