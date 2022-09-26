#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

using Interhaptics.InteractionsEngine.Shared.Types;
using Interhaptics.ObjectSnapper.core;

namespace Interhaptics.Editor.Utils.Updates
{
    /// <summary>
    /// Handles the SnappingData update into PoseData.
    /// </summary>
    public class SnappingDataUpdate : IUpdatable
    {
        #region Constants
        private const string LABEL_Title = "SnappingData update";
        private const string LABEL_UpdateName = "Snapping data";

        private const string VALUE_OldSuffix = "_Old";
        private const string VALUE_AssetExtension = ".asset";

        private const string LABEL_Finding = "Finding the SnappingData assets";
        private const string LABEL_Converting = "Converting";
        private const string LABEL_Replacing = "Replacing the assets in the scene";
        private const string LABEL_Cleaning = "Cleaning the old assets";
        private const string LABEL_End = "Converted!";

        private const string LABEL_ReplacementDialogTitle = "Data replacement";
        private const string LABEL_ReplacementDialogText = "Do you want to replace the old data with the new one in all the scenes? The prefabs have to be modified manually after the conversion. Please save your complete project before to confirm";
        private const string LABEL_CleanDialogTitle = "Clean the old data";
        private const string LABEL_CleanDialogText = "Do you want to delete the old data?";
        private const string LABEL_EmptyDialogText = "No SnappingData asset found in the project";
        private const string LABEL_Confirm = "Ok";
        private const string LABEL_Cancel = "No";

        private const string SerializeProperty_SnappingData = "snappingData";
        private const string SerializeProperty_PosesData = "posesData";
        #endregion

        #region Constructors
        public SnappingDataUpdate()
        {
            this.RefreshUpdatableSnappingData();
        }
        #endregion

        #region Properties
        public string Name => LABEL_UpdateName;
        public bool HasUpdate => snappingDataGuids != null && snappingDataGuids.Length > 0;
        #endregion

        #region Variables
        private string[] snappingDataGuids = null;
        #endregion

        #region Privates
        private void RefreshUpdatableSnappingData()
        {
            snappingDataGuids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(SnappingData)));
        }

        private void RecursiveOldPoseGetter(ObjectSnapper.core.Pose root, ref List<ObjectSnapper.core.Pose> list)
        {
            if (list == null)
                return;

            list.Add(root);

            if (root.childrenPose != null)
            {
                foreach (ObjectSnapper.core.Pose oldPose in root.childrenPose)
                    this.RecursiveOldPoseGetter(oldPose, ref list);
            }
        }

        private void WorkOnScenes(Action<Scene> onSCeneLoaded, string barTitle, string barText)
        {
            if (onSCeneLoaded == null)
                return;

            string[] scenesGuids = AssetDatabase.FindAssets("t:Scene");
            List<string> activeScenes = (from scene in EditorSceneManager.GetAllScenes() select scene.path).ToList();
            for (int i = 0; i < scenesGuids.Length; i++)
            {
                EditorUtility.DisplayProgressBar(barTitle, barText, (float)i / scenesGuids.Length);

                string path = AssetDatabase.GUIDToAssetPath(scenesGuids[i]);
                bool wasLoaded = activeScenes.Contains(path);
                Scene currentScene = default;

                if (!wasLoaded)
                    currentScene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                else
                    currentScene = EditorSceneManager.GetSceneByPath(path);

                onSCeneLoaded.Invoke(currentScene);

                EditorSceneManager.SaveScene(currentScene);

                if (!wasLoaded)
                    EditorSceneManager.UnloadScene(currentScene);
            }
        }
        #endregion

        #region Publics
        public void Update()
        {
            Dictionary<SnappingData, PosesData> _oldAssets = new Dictionary<SnappingData, PosesData>();

            //Find the assets
            if (snappingDataGuids != null && snappingDataGuids.Length > 0)
            {
                for (int i = 0; i < snappingDataGuids.Length; i++)
                {
                    EditorUtility.DisplayProgressBar(LABEL_Title, LABEL_Finding, (float)i / snappingDataGuids.Length);

                    //Get the assets paths
                    string completePath = AssetDatabase.GUIDToAssetPath(snappingDataGuids[i]);
                    string directoryPath = Path.GetDirectoryName(completePath);
                    string oldName = Path.GetFileNameWithoutExtension(completePath) + VALUE_OldSuffix + VALUE_AssetExtension;

                    //Abort the data if the asset cannot be renamed.
                    if (!string.IsNullOrEmpty(AssetDatabase.RenameAsset(completePath, oldName)))
                        continue;

                    AssetDatabase.SaveAssets();

                    //Abort if the old data is null.
                    SnappingData oldData = AssetDatabase.LoadAssetAtPath<SnappingData>(Path.Combine(directoryPath, oldName));
                    if (oldData == null || oldData.snappingDataDictionary == null)
                        continue;

                    //Abort if the new data is null.
                    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<PosesData>(), completePath);
                    PosesData newData = AssetDatabase.LoadAssetAtPath<PosesData>(completePath);
                    if (newData == null)
                        continue;

                    //Extract the old data
                    SnappingDataDictionary.Datas[] posesData = oldData.snappingDataDictionary.GetOldPoses();
                    foreach (SnappingDataDictionary.Datas data in posesData)
                    {
                        EditorUtility.DisplayProgressBar(LABEL_Title, $"{LABEL_Converting} {data.name} from {oldData.name}", (float)i / snappingDataGuids.Length);

                        List<ObjectSnapper.core.Pose> oldPoses = new List<ObjectSnapper.core.Pose>();
                        this.RecursiveOldPoseGetter(data.snappableActorPosData.rootPose, ref oldPoses);

                        //Start to convert
                        List<SerializableSpatialRepresentation> serializableSpatialRepresentations = (from oldPose in oldPoses select oldPose.spatialRepresentation).ToList();
                        if (serializableSpatialRepresentations.Count >= 2)
                        {
                            SpatialRepresentation rootSpatialRepresentation = serializableSpatialRepresentations[0].ToSpatialRepresentation();
                            SpatialRepresentation subRootSpatialRepresentation = serializableSpatialRepresentations[1].ToSpatialRepresentation();

                            serializableSpatialRepresentations[1] = new SerializableSpatialRepresentation
                            (
                                new SpatialRepresentation
                                {
                                    Position = rootSpatialRepresentation.Position + subRootSpatialRepresentation.Position,
                                    Rotation = rootSpatialRepresentation.Rotation * subRootSpatialRepresentation.Rotation
                                }
                            );

                            serializableSpatialRepresentations.RemoveAt(0);
                        }

                        //Save the old data into the new data asset.
                        ActorData actorData;
                        actorData.transformsSP = serializableSpatialRepresentations.ToArray();

                        newData.posesDictionary.Add(data.name, actorData);
                    }

                    EditorUtility.SetDirty(newData);
                    _oldAssets.Add(oldData, newData);
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                //Convert assets in the scenes
                if (EditorUtility.DisplayDialog(LABEL_ReplacementDialogTitle, LABEL_ReplacementDialogText, LABEL_Confirm, LABEL_Cancel))
                {
                    this.WorkOnScenes((Scene scene) =>
                    {
                        foreach (SnappingPrimitive snappingPrimitive in from go in scene.GetRootGameObjects() from sp in go.GetComponentsInChildren<SnappingPrimitive>(true) select sp)
                        {
                            SerializedObject serializedObject = new SerializedObject(snappingPrimitive);
                            if (serializedObject == null)
                                continue;

                            SnappingData snappingData = serializedObject.FindProperty(SerializeProperty_SnappingData).objectReferenceValue as SnappingData;
                            if (snappingData && _oldAssets.TryGetValue(snappingData, out PosesData posesData))
                            {
                                SerializedProperty poseDataSP = serializedObject.FindProperty(SerializeProperty_PosesData);
                                if(poseDataSP.objectReferenceValue == null)
                                {
                                    poseDataSP.objectReferenceValue = posesData;
                                    serializedObject.ApplyModifiedProperties();
                                }

                            }
                        }

                        EditorSceneManager.SaveScene(scene);

                    }, LABEL_Title, LABEL_Replacing);

                }

                //Clean the old data
                if (EditorUtility.DisplayDialog(LABEL_CleanDialogTitle, LABEL_CleanDialogText, LABEL_Confirm, LABEL_Cancel))
                {
                    //Clean the scenes script from any old data references.
                    this.WorkOnScenes((Scene scene) =>
                    {
                        foreach (SnappingPrimitive snappingPrimitive in from go in scene.GetRootGameObjects() from sp in go.GetComponentsInChildren<SnappingPrimitive>(true) select sp)
                        {
                            SerializedObject serializedObject = new SerializedObject(snappingPrimitive);

                            if (serializedObject != null)
                            {
                                serializedObject.FindProperty(SerializeProperty_SnappingData).objectReferenceValue = null;
                                serializedObject.ApplyModifiedProperties();
                            }
                        }

                        EditorSceneManager.SaveScene(scene);

                    }, LABEL_Title, LABEL_Cleaning);


                    foreach (string guid in snappingDataGuids)
                        AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
                }


                EditorUtility.DisplayProgressBar(LABEL_Title, LABEL_End, 1);
                EditorUtility.ClearProgressBar();
            }
            else
                EditorUtility.DisplayDialog(LABEL_Title, LABEL_EmptyDialogText, LABEL_Confirm);

            this.RefreshUpdatableSnappingData();
        }
        #endregion
    }
}
#endif
