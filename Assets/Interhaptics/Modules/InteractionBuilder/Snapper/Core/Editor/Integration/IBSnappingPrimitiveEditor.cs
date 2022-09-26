using Interhaptics.ObjectSnapper.core;
using UnityEditor;
using UnityEngine;

namespace Interhaptics.ObjectSnapper.Editor
{
    [CustomEditor(typeof(IBSnappingPrimitive))]
    [CanEditMultipleObjects]
    public class IBSnappingPrimitiveEditor : SnappingPrimitiveEditor
    {
        #region Constants
        private const string PATH_LeftHand = "Prefabs/CustomLeft";
        private const string PATH_RightHand = "Prefabs/CustomRight";

        private const string BUTTON_LeftHand = "Left Hand";
        private const string BUTTON_RightHand = "Right Hand";
        private const string Label_Mirroring = "Mirror";

        private const string SERIALIZEDPROPERTY_SnappingMask = "snappingMask";
        #endregion

        #region Variables
        /*
         * 
         * Snapping
         * 
         */
        private SerializedProperty snappingMaskCE;

        //Privates
        private ModelSnappableActor _leftHandResource = null;
        private ModelSnappableActor _rightHandResource = null;
        private ModelSnappableActor _mirorredResource = null;
        #endregion

        #region Life Cycle
        private void Awake()
        {
            _leftHandResource = Resources.Load<ModelSnappableActor>(PATH_LeftHand);
            _rightHandResource = Resources.Load<ModelSnappableActor>(PATH_RightHand);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            snappingMaskCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_SnappingMask);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            /*
             * 
             * Shape
             * 
             */
            EditorGUILayout.LabelField(TITLE_General, EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(primitiveShapeCE);
            EditorGUILayout.PropertyField(localPositionOffsetCE);

            PrimitiveShape currentRepresentation = (PrimitiveShape)primitiveShapeCE.enumValueIndex;

            if (currentRepresentation != PrimitiveShape.Sphere)
                EditorGUILayout.PropertyField(localRotationOffsetCE);

            EditorGUILayout.PropertyField(primaryRadiusCE);

            switch (currentRepresentation)
            {
                case PrimitiveShape.Cylinder:
                case PrimitiveShape.Capsule:
                    EditorGUILayout.PropertyField(lengthCE);
                    break;
                case PrimitiveShape.Torus:
                    EditorGUILayout.PropertyField(secondaryRadiusCE);
                    break;
            }

            /*
             * 
             * Snapping
             * 
             */
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(TITLE_Snapping, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(snappingMaskCE);
            EditorGUILayout.PropertyField(posesDataCE);

            EditorGUILayout.PropertyField(isFixedSnappingCE);
            if (!isFixedSnappingCE.boolValue && currentRepresentation != PrimitiveShape.Sphere)
                EditorGUILayout.PropertyField(isDirectionLockedCE);
            EditorGUILayout.PropertyField(isPartialSnappingCE);

            EditorGUILayout.PropertyField(movementTypeCE);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(TITLE_Skin, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(skinWidthCE);
            if (currentRepresentation == PrimitiveShape.Cylinder)
                EditorGUILayout.PropertyField(skinLengthCE);
            EditorGUILayout.PropertyField(displaySkinCE);

            if (posesDataCE.objectReferenceValue)
            {
                EditorGUILayout.Space();

                IBSnappingPrimitive ibSnappingPrimitive = (IBSnappingPrimitive)target;
                if (ibSnappingPrimitive && ibSnappingPrimitive.isActiveAndEnabled && !Application.isPlaying)
                {

                    EditorGUILayout.LabelField(TITLE_Edition, EditorStyles.boldLabel);

                    if (!modelActorCE.objectReferenceValue)
                    {
                        EditorGUILayout.BeginHorizontal();

                        if (GUILayout.Button(BUTTON_LeftHand))
                            modelActorCE.objectReferenceValue = _leftHandResource;
                        else if (GUILayout.Button(BUTTON_RightHand))
                            modelActorCE.objectReferenceValue = _rightHandResource;

                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        bool mirrored = EditorGUILayout.Toggle(Label_Mirroring, _mirorredResource != null);
                        if (mirrored && _mirorredResource == null)
                            _mirorredResource = (modelActorCE.objectReferenceValue == _leftHandResource) ? _rightHandResource : _leftHandResource;
                        else if (!mirrored && _mirorredResource != null)
                            _mirorredResource = null;

                        if (GUILayout.Button($"{BUTTON_Save} & {BUTTON_Exit}"))
                        {
                            if (!_mirorredResource && ibSnappingPrimitive.QuickSave() ||
                                _mirorredResource && ibSnappingPrimitive.MirroredSave(_mirorredResource))
                                ibSnappingPrimitive.ResetSnappingEdition();
                        }

                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(BUTTON_Save))
                        {
                            if (_mirorredResource)
                                ibSnappingPrimitive.MirroredSave(_mirorredResource);
                            else
                                ibSnappingPrimitive.QuickSave();
                        }
                        if (GUILayout.Button(BUTTON_Exit))
                            ibSnappingPrimitive.ResetSnappingEdition();
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
        #endregion
    }
}