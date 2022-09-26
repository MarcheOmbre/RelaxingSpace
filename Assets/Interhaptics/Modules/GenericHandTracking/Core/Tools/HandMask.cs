using UnityEngine;

namespace Interhaptics.HandTracking.Tools
{
    /// <summary>
    /// Create a hand mask that allows to choose which finger will be influenced by the HandTracking.
    /// /!\ If the tracking is based on gesture, the fingers which are not influenced will keep their idle position.
    /// </summary>
    [CreateAssetMenu(fileName = "Hand Mask", menuName = "Interhaptics/Hand Mask")]
    public class HandMask : ScriptableObject
    {
        #region Enums
        [System.Flags]
        public enum MaskPart
        {
            None = 0,

            Thumb = 1,
            Index = 2,
            Middle = 4,
            Ring = 8,
            Pinky = 16,

            All = ~0
        }
        #endregion

        #region Properties
        /// <summary>
        /// Get the mask set in the inspector.
        /// </summary>
        public MaskPart Mask { get { return handMask; } }
        #endregion

        #region Variables
        [SerializeField] private MaskPart handMask = MaskPart.All;
        #endregion
    }
}
