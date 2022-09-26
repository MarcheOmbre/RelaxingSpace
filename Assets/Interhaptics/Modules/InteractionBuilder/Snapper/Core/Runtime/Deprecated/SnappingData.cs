#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;
using static Interhaptics.ObjectSnapper.core.SnappingDataDictionary;

namespace Interhaptics.ObjectSnapper.core
{
    #region Structures
    //DEPRECATED
    [Obsolete("Will be removed in the next updates", false)]
    [Serializable]
    public struct Pose
    {
        public string transformName;
        public SerializableSpatialRepresentation spatialRepresentation;
        public List<Pose> childrenPose;
    }

    [Obsolete("Will be removed in the next updates", false)]
    [Serializable]
    public struct SnappableActorData
    {
        public SerializableVector forward;
        public SerializableVector upward;
        public Pose rootPose;
    }
    #endregion

    #region Class
    [Obsolete("Will be removed in the next updates", false)]
    [Serializable]
    public class SnappingDataDictionary
    {
        #region Structures
        [Serializable]
        public struct Datas
        {
            public string name;
            public SnappableActorData snappableActorPosData;
        }
        #endregion

        #region Variables
        [SerializeField] [HideInInspector] private string[] serializedData;

        #endregion

        #region Publics
        public Datas[] GetOldPoses()
        {
            List<Datas> oldSnappableActorData = new List<Datas>();

            if (serializedData != null)
            {
                try
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    MemoryStream memoryStream;

                    for (int i = 0; i < serializedData.Length; i++)
                    {
                        using (memoryStream = new MemoryStream(Convert.FromBase64String(serializedData[i])))
                        {
                            memoryStream.Position = 0;
                            Datas data = (Datas)binaryFormatter.Deserialize(memoryStream);
                            oldSnappableActorData.Add(data);
                        }
                    }
                }
                catch (SerializationException se) { Debug.Log(se); }
            }

            return oldSnappableActorData.ToArray();
        }
        #endregion
    }
    #endregion

#if UNITY_EDITOR
    [Obsolete("Will be removed in the next updates", false)]
    [CustomEditor(typeof(SnappingData))]
    public class SnappingDataEditor : Editor
    {
        #region Constants
        private const string LABEL_NoData = "No data found in this SnappingData. This class is deprecated, you can create data by editing a PosesData through the SnappingPrimitive script";
        private const string LABEL_ObjectID = "ObjectID";
        #endregion

        #region Variables
        private GUIStyle _titleEditorStyle = null;
        #endregion

        #region Life Cycle
        private void Awake()
        {
            _titleEditorStyle = new GUIStyle(EditorStyles.boldLabel);
            _titleEditorStyle.alignment = TextAnchor.MiddleCenter;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            Datas[] dataKeys = ((SnappingData)target).snappingDataDictionary.GetOldPoses();

            if (dataKeys == null || dataKeys.Length == 0)
                GUILayout.Label(LABEL_NoData, EditorStyles.wordWrappedLabel);
            else
            {
                GUILayout.Label(LABEL_ObjectID, EditorStyles.boldLabel);
                EditorGUILayout.Space();
                foreach (Datas OldData in dataKeys)
                    GUILayout.Label(OldData.name, EditorStyles.label);
            }
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
        #endregion
    }
#endif

    /// <summary>
    /// Deprecated class, used to convert the old data to the new architecture.
    /// </summary>
    [Obsolete("Will be removed in the next updates", false)]
    public class SnappingData : ScriptableObject
    {
        #region Variables
        [SerializeField] [HideInInspector] public SnappingDataDictionary snappingDataDictionary = new SnappingDataDictionary();
        #endregion
    }
}
#endif