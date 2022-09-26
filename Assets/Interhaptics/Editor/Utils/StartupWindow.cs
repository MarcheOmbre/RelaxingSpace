#if UNITY_EDITOR
using Interhaptics.Editor.Utils.Updates;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Interhaptics.Editor.Utils
{
    /// <summary>
    /// The Interhaptics startup window.
    /// </summary>
    public class StartupWindow : EditorWindow
    {
        #region Constants
        private const string KEY_Initialized = "EditorWindow_Initialized";

        private const string LABEL_WindowTitle = "Interhaptics SDK";

        //private const string LABEL_Tips = "Tips";
        //private const string LABEL_WhatsNew = "What's new?";
        //private const string LABEL_DidYouKnow = "Did you know?";
        private const string LABEL_UpgradesTitle = "Upgrades";

        private const string LABEL_UpgradesText = "Before upgrading, please ensure that you have a copy of this project to avoid any lost data.";
        private const string LABEL_Fix = "Fix";

        private const string LABEL_DoNotShowAgain = "Do not show again";

        private const string DEBUG_LABEL_Ok = "Ok";
        #endregion

        #region Variables
        private IUpdatable[] _updatables = null;
        #endregion

        #region Life Cycle
        private void OnEnable()
        {
            //Get updates from the current Assembly
            Assembly currentAssembly = this.GetType().Assembly;
            _updatables = (from Type type in currentAssembly.GetTypes()
                           where typeof(IUpdatable).IsAssignableFrom(type) && !type.IsInterface
                           select currentAssembly.CreateInstance(type.ToString()) as IUpdatable).ToArray();
        }

        private void OnGUI()
        {
            /*
            //Tips
            EditorGUILayout.LabelField(LABEL_Tips, EditorStyles.boldLabel);
            EditorGUILayout.Space();

            //What's new
            EditorGUILayout.LabelField(LABEL_WhatsNew, EditorStyles.boldLabel);
            EditorGUILayout.Space();

            //Did you know
            EditorGUILayout.LabelField(LABEL_DidYouKnow, EditorStyles.boldLabel);
            EditorGUILayout.Space();
            */

            //Upgrades
            EditorGUILayout.LabelField(LABEL_UpgradesTitle, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(LABEL_UpgradesText, EditorStyles.helpBox);

            //Haptics materials
            foreach (IUpdatable updatable in _updatables)
            {
                if (updatable == null)
                    continue;

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(updatable.Name);

                if (updatable.HasUpdate)
                {
                    if (GUILayout.Button(LABEL_Fix))
                        updatable.Update();
                }
                else
                    EditorGUILayout.LabelField(DEBUG_LABEL_Ok);

                EditorGUILayout.EndHorizontal();
            }

            //Do not show the window again
            bool showPopup = !EditorGUILayout.Toggle(LABEL_DoNotShowAgain, !InterhapticsEditorSettings.AssetInstance.showUpdatePopup);
            if(showPopup != InterhapticsEditorSettings.AssetInstance.showUpdatePopup)
            {
                InterhapticsEditorSettings.AssetInstance.showUpdatePopup = showPopup;
                EditorUtility.SetDirty(InterhapticsEditorSettings.AssetInstance);
            }
        }
        #endregion

        #region Privates
        [InitializeOnLoadMethod]
        private static void Init()
        {
            if (!SessionState.GetBool(KEY_Initialized, false) && InterhapticsEditorSettings.AssetInstance.showUpdatePopup)
            {
                EditorApplication.delayCall += StartupWindow.ShowWindow;
                SessionState.SetBool(KEY_Initialized, true);
            }
        }

        [MenuItem("Interhaptics/Startup")]
        private static void ShowWindow()
        {
            if (InterhapticsEditorSettings.AssetInstance == null)
                return;

            EditorApplication.delayCall -= StartupWindow.ShowWindow;
            StartupWindow updateWindow = EditorWindow.GetWindow<StartupWindow>(true, LABEL_WindowTitle, true);
        }
        #endregion
    }
}
#endif
