using Interhaptics.ObjectSnapper.core;
using UnityEditor;
using UnityEngine;

namespace Interhaptics.ObjectSnapper.Editor
{
    [CustomEditor(typeof(SnappingPrimitive), true)]
    [CanEditMultipleObjects]
    public class SnappingPrimitiveEditor : UnityEditor.Editor
    {
        #region Constants
        protected const string TITLE_General = "Shape";
        protected const string TITLE_Snapping = "Snapping";
        protected const string TITLE_Skin = "Skin";
        protected const string TITLE_Edition = "Edition";
        protected const string BUTTON_Save = "Save";
        protected const string BUTTON_Exit = "Exit";

        //Shape
        private const string SERIALIZEDPROPERTY_PrimitiveShape = "primitiveShape";
        private const string SERIALIZEDPROPERTY_LocalPosition = "localPosition";
        private const string SERIALIZEDPROPERTY_LocalRotation = "localRotation";
        private const string SERIALIZEDPROPERTY_PrimaryColor = "primaryColor";
        private const string SERIALIZEDPROPERTY_PrimaryRadius = "primaryRadius";
        private const string SERIALIZEDPROPERTY_Length = "length";
        private const string SERIALIZEDPROPERTY_SecondaryColor = "secondaryColor";
        private const string SERIALIZEDPROPERTY_SecondaryRadius = "secondaryRadius";
        private const string SERIALIZEDPROPERTY_DisplaySkin = "displaySkin";
        private const string SERIALIZEDPROPERTY_SkinColor = "skinColor";
        private const string SERIALIZEDPROPERTY_SkinWidth = "skinWidth";
        private const string SERIALIZEDPROPERTY_SkinLength = "skinLength";

        //Snapping
        private const string SERIALIZEDPROPERTY_Mask = "mask";
        private const string SERIALIZEDPROPERTY_PosesData = "posesData";
        private const string SERIALIZEDPROPERTY_IsFixedSnapping = "isFixedSnapping";
        private const string SERIALIZEDPROPERTY_IsDirectionLocked = "isDirectionLocked";
        private const string SERIALIZEDPROPERTY_IsPartialSnappingCE = "isPartialSnapping";
        private const string SERIALIZEDPROPERTY_MovementType = "movementType";
        private const string SERIALIZEDPROPERTY_ForwardAxis = "forwardAxis";
        private const string SERIALIZEDPROPERTY_UpwardAxis = "upwardAxis";
        private const string SERIALIZEDPROPERTY_ModelActor = "modelActor";
        private const string SERIALIZEDPROPERTY_TrackingColor = "trackingColor";
        private const string SERIALIZEDPROPERTY_TrackingDistance = "trackingDistance";
        private const string SERIALIZEDPROPERTY_TrackingRadius = "trackingRadius";
        private const string SERIALIZEDPROPERTY_TrackingAxisLength = "trackingAxisLength";
        #endregion

        #region Variables
        /*
         * 
         * Shape
         * 
         */

        protected SerializedProperty primitiveShapeCE;
        protected SerializedProperty localPositionOffsetCE;
        protected SerializedProperty localRotationOffsetCE;
        protected SerializedProperty primaryColorCE;
        protected SerializedProperty primaryRadiusCE;
        protected SerializedProperty lengthCE;
        protected SerializedProperty secondaryColorCE;
        protected SerializedProperty secondaryRadiusCE;
        protected SerializedProperty displaySkinCE;
        protected SerializedProperty skinColorCE;
        protected SerializedProperty skinWidthCE;
        protected SerializedProperty skinLengthCE;

        /*
         * 
         * Snapping
         * 
         */
        protected SerializedProperty maskCE;
        protected SerializedProperty posesDataCE;

        protected SerializedProperty isFixedSnappingCE;
        protected SerializedProperty isDirectionLockedCE;
        protected SerializedProperty isPartialSnappingCE;
        protected SerializedProperty movementTypeCE;
        protected SerializedProperty forwardAxisCE;
        protected SerializedProperty upwardAxisCE;
        protected SerializedProperty modelActorCE;
        protected SerializedProperty trackingColorCE;
        protected SerializedProperty trackingDistanceCE;
        protected SerializedProperty trackingRadiusCE;
        protected SerializedProperty trackingAxisLengthCE;
        #endregion

        #region Life Cycle
        protected virtual void OnEnable()
        {
            /*
             * 
             * Shape
             * 
             */
            primitiveShapeCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_PrimitiveShape);
            localPositionOffsetCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_LocalPosition);
            localRotationOffsetCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_LocalRotation);
            primaryColorCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_PrimaryColor);
            primaryRadiusCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_PrimaryRadius);
            lengthCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_Length);
            secondaryColorCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_SecondaryColor);
            secondaryRadiusCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_SecondaryRadius);
            displaySkinCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_DisplaySkin);
            skinColorCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_SkinColor);
            skinWidthCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_SkinWidth);
            skinLengthCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_SkinLength);

            /*
             * 
             * Snapping
             * 
             */
            maskCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_Mask);
            posesDataCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_PosesData);
            isFixedSnappingCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_IsFixedSnapping);
            isDirectionLockedCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_IsDirectionLocked);
            isPartialSnappingCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_IsPartialSnappingCE);
            movementTypeCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_MovementType);
            forwardAxisCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_ForwardAxis);
            upwardAxisCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_UpwardAxis);

            modelActorCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_ModelActor);
            trackingColorCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_TrackingColor);
            trackingDistanceCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_TrackingDistance);
            trackingRadiusCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_TrackingRadius);
            trackingAxisLengthCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_TrackingAxisLength);
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
            EditorGUILayout.PropertyField(primaryColorCE);

            PrimitiveShape currentRepresentation = (PrimitiveShape)primitiveShapeCE.enumValueIndex;

            if(currentRepresentation != PrimitiveShape.Sphere)
                EditorGUILayout.PropertyField(localRotationOffsetCE);

            EditorGUILayout.PropertyField(primaryRadiusCE);

            switch (currentRepresentation)
            {
                case PrimitiveShape.Cylinder:
                case PrimitiveShape.Capsule:
                    EditorGUILayout.PropertyField(lengthCE);
                    break;
                case PrimitiveShape.Torus:
                    EditorGUILayout.PropertyField(secondaryColorCE);
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
            EditorGUILayout.PropertyField(posesDataCE);

            EditorGUILayout.PropertyField(isFixedSnappingCE);
            if (!isFixedSnappingCE.boolValue && currentRepresentation != PrimitiveShape.Sphere)
                EditorGUILayout.PropertyField(isDirectionLockedCE);
            EditorGUILayout.PropertyField(isPartialSnappingCE);

            EditorGUILayout.PropertyField(movementTypeCE);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(TITLE_Skin, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(skinColorCE);
            EditorGUILayout.PropertyField(skinWidthCE);
            if (currentRepresentation == PrimitiveShape.Cylinder)
                EditorGUILayout.PropertyField(skinLengthCE);
            EditorGUILayout.PropertyField(displaySkinCE);

            if (posesDataCE.objectReferenceValue)
            {
                EditorGUILayout.PropertyField(forwardAxisCE);
                EditorGUILayout.PropertyField(upwardAxisCE);

                SnappingPrimitive snappingPrimitive = (SnappingPrimitive)target;

                if(snappingPrimitive && snappingPrimitive.isActiveAndEnabled && !Application.isPlaying)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(TITLE_Edition, EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(maskCE);
                    EditorGUILayout.PropertyField(modelActorCE);

                    if (modelActorCE.objectReferenceValue)
                    {
                        EditorGUILayout.PropertyField(trackingColorCE);
                        EditorGUILayout.PropertyField(trackingRadiusCE);
                        EditorGUILayout.PropertyField(trackingDistanceCE);
                        EditorGUILayout.PropertyField(trackingAxisLengthCE);

                        if (GUILayout.Button($"{BUTTON_Save} & {BUTTON_Exit}"))
                        {
                            if (snappingPrimitive.QuickSave())
                                snappingPrimitive.ResetSnappingEdition();
                        }

                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(BUTTON_Save))
                            snappingPrimitive.QuickSave();
                        if (GUILayout.Button(BUTTON_Exit))
                            snappingPrimitive.ResetSnappingEdition();
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
        #endregion
    }
}
