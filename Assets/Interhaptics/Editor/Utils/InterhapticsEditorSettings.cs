#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Interhaptics.Editor.Utils
{
    /// <summary>
    /// Stores Interhaptics editor data.
    /// </summary>
    public class InterhapticsEditorSettings : ScriptableObject
    {        
        #region Constants
        private const string VALUE_AssetPath = "Assets/Interhaptics/Editor/EditorSettings.asset";
        #endregion

        #region Properties
        public static InterhapticsEditorSettings AssetInstance
        {
            get
            {
                if (InterhapticsEditorSettings.instance == null)
                    InterhapticsEditorSettings.instance = AssetDatabase.LoadAssetAtPath<InterhapticsEditorSettings>(VALUE_AssetPath);

                return InterhapticsEditorSettings.instance;
            }
        }
        #endregion

        #region Variables
        public bool showUpdatePopup = true;

        private static InterhapticsEditorSettings instance = null;
        #endregion
    }
}
#endif