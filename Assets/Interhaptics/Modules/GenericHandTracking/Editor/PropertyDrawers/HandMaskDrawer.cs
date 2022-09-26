#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Interhaptics.HandTracking.Editor.PropertyDrawers
{

    [UnityEditor.CustomPropertyDrawer(typeof(Tools.HandMask.MaskPart))]
    public class HandMaskPropertyDrawer : PropertyDrawer
    {
        #region Constantes
        private const string RESOURCES_Shader = "UI/Unlit/Detail";
        private const string RESOURCES_PalmTexture = "handmask/handmask-Palm";
        private const string RESOURCES_ThumbTexture = "handmask/handmask-Thumb";
        private const string RESOURCES_IndexTexture = "handmask/handmask-Index";
        private const string RESOURCES_MiddleTexture = "handmask/handmask-Middle";
        private const string RESOURCES_RingTexture = "handmask/handmask-Ring";
        private const string RESOURCES_PinkyTexture = "handmask/handmask-Pinky";

        private const float DISPLAY_SIZE = 250;
        #endregion

        #region Variables
        private Texture2D[] _textures = new Texture2D[6];

        private Material _unselectedMaterial = null;
        private Material _selectedMaterial = null;
        private Material _unselectableMaterial = null;
        #endregion

        #region Constructors
        public HandMaskPropertyDrawer() : base()
        {
            _textures[0] = (Texture2D)Resources.Load(RESOURCES_PalmTexture);
            _textures[1] = (Texture2D)Resources.Load(RESOURCES_ThumbTexture);
            _textures[2] = (Texture2D)Resources.Load(RESOURCES_IndexTexture);
            _textures[3] = (Texture2D)Resources.Load(RESOURCES_MiddleTexture);
            _textures[4] = (Texture2D)Resources.Load(RESOURCES_RingTexture);
            _textures[5] = (Texture2D)Resources.Load(RESOURCES_PinkyTexture);

            _selectedMaterial = new Material(Shader.Find(RESOURCES_Shader));
            _selectedMaterial.color = Color.green;
            _unselectedMaterial = new Material(Shader.Find(RESOURCES_Shader));
            _unselectedMaterial.color = Color.red;
            _unselectableMaterial = new Material(Shader.Find(RESOURCES_Shader));
            _unselectableMaterial.color = Color.gray;
        }
        #endregion

        #region Overrides
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return DISPLAY_SIZE;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            //Draw position
            float editorWidth = position.width;
            position.y += EditorGUIUtility.singleLineHeight;
            position.height = DISPLAY_SIZE;
            position.width = DISPLAY_SIZE;
            position.x += (editorWidth / 2) - position.width / 2;

            //Get mouse events
            Event mouseEvent = Event.current;
            for (int i = 1; i < _textures.Length; i++)
            {
                Tools.HandMask.MaskPart part = (Tools.HandMask.MaskPart)(1 << (i - 1));

                if (mouseEvent != null && mouseEvent.type == EventType.MouseUp)
                {
                    Vector2 mousePosition = mouseEvent.mousePosition;
                    mousePosition.x = Mathf.Clamp(mousePosition.x - position.x, 0, DISPLAY_SIZE);
                    mousePosition.y = Mathf.Clamp(mousePosition.y - position.y, 0, DISPLAY_SIZE);


                    Vector2Int pixelCoord = new Vector2Int((int)(
                        (mousePosition.x / DISPLAY_SIZE) * _textures[i].width),
                        (int)((1 - (mousePosition.y / DISPLAY_SIZE)) * _textures[i].height));

                    if (_textures[i].GetPixel(pixelCoord.x, pixelCoord.y).a > 0)
                    {            
                        property.intValue = property.intValue ^ (int)part;
                        break;
                    }
                }

                EditorGUI.DrawPreviewTexture(position, _textures[i], ((Tools.HandMask.MaskPart)property.intValue).HasFlag(part) ? _selectedMaterial : _unselectedMaterial, ScaleMode.StretchToFill);
            }

            EditorGUI.DrawPreviewTexture(position, _textures[0], _unselectableMaterial, ScaleMode.StretchToFill);

            EditorGUI.EndProperty();
        }
        #endregion
    }

}
#endif