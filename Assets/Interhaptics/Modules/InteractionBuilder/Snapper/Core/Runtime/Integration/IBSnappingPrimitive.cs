using Interhaptics.HandTracking.Tools;
using System.Collections.Generic;
using UnityEngine;

namespace Interhaptics.ObjectSnapper.core
{
    [AddComponentMenu("Interhaptics/Object Snapper/IBSnappingPrimitive")]
    public class IBSnappingPrimitive : SnappingPrimitive
    {
#if UNITY_EDITOR
        #region Constants
        //Shape
        private const float VALUE_PrimaryRadius = 0.05f;
        private const float VALUE_SecondaryRadius = 0.02f;
        private const float VALUE_Length = 0.1f;

        //Snapping
        private const float VALUE_TrackingRadius = 0.0125f;
        private const float VALUE_TrackingDistance = 0.08f;
        private const float VALUE_AxisLength = 1.5f;
        #endregion

        #region Protecteds
        protected override void OnReset()
        {
            base.OnReset();

            //Shape
            primaryColor = new Color(0.310f, 0.2f, 1f, 0.5f);
            secondaryColor = new Color(1f, 0.69f, 1f, 0.5f);
            primaryRadius = VALUE_PrimaryRadius;
            secondaryRadius = VALUE_SecondaryRadius;
            length = VALUE_Length;

            //Snapping
            skinColor = new Color(0.69f, 1f, 1f, 0.5f);

            //Tracking
            trackingColor = new Color(1f, 1f, 1f, 0.5f);
            trackingRadius = VALUE_TrackingRadius;
            trackingDistance = VALUE_TrackingDistance;
            trackingAxisLength = VALUE_AxisLength;
        }
        #endregion

        #region Publics
        public bool MirroredSave(ModelSnappableActor modelSnappableActor)
        {
            bool success = false;

            if (modelSnappableActor != null && _simulationActor != null)
            {
                //Create the mirrored object
                ModelSnappableActor mirroredModel = GameObject.Instantiate<ModelSnappableActor>(modelSnappableActor, _simulationActor.transform.parent);

                //Copy and modify the position
                Transform[] modelTransforms = _simulationActor.GetComponentsInChildren<Transform>();
                Transform[] mirroredTransforms = mirroredModel.GetComponentsInChildren<Transform>();

                //Apply modifications
                if (modelTransforms.Length == mirroredTransforms.Length)
                {
                    for (int i = 0; i < modelTransforms.Length; i++)
                    {
                        Vector3 localPosition = modelTransforms[i].localPosition;
                        Vector3 localEulerAngles = modelTransforms[i].localEulerAngles;

                        if (modelTransforms[i].transform != _simulationActor.transform)
                        {
                            localPosition.z *= -1;
                            localEulerAngles.x *= -1;
                            localEulerAngles.y *= -1;
                        }

                        mirroredTransforms[i].localPosition = localPosition;
                        mirroredTransforms[i].localEulerAngles = localEulerAngles;
                    }

                    success = this.SaveActorData(mirroredModel, posesData);
                    GameObject.DestroyImmediate(mirroredModel.gameObject);
                }
            }

            return success && this.QuickSave();
        }
        #endregion
#endif

        #region Constants
        private const string TOOLTIP_SnappingMask = "If not null, the fingers will be snapped according to the mask and the PosesData. If null, all the fingers will be snapped according to the PosesData.";

        //Hand parts
        public const string HANDPART_Thumb = "Thumb";
        public const string HANDPART_Index = "Index";
        public const string HANDPART_Middle = "Middle";
        public const string HANDPART_Ring = "Ring";
        public const string HANDPART_Pinky = "Pinky";
        #endregion

        #region Properties
        public HandMask SnappingMask { get { return snappingMask; } }
        #endregion

        #region Variable
        [Tooltip(TOOLTIP_SnappingMask)][SerializeField] private HandMask snappingMask = null;
        #endregion

        #region Life Cycle
        protected override void Awake()
        {
            base.Awake();

            //Convert the Interhaptics Hand mask into an ObjectSnapper mask
            if (snappingMask)
            {
                List<string> maskConverter = new List<string>();

                if (!snappingMask.Mask.HasFlag(HandMask.MaskPart.Thumb))
                    maskConverter.Add(HANDPART_Thumb);
                if (!snappingMask.Mask.HasFlag(HandMask.MaskPart.Index))
                    maskConverter.Add(HANDPART_Index);
                if (!snappingMask.Mask.HasFlag(HandMask.MaskPart.Middle))
                    maskConverter.Add(HANDPART_Middle);
                if (!snappingMask.Mask.HasFlag(HandMask.MaskPart.Ring))
                    maskConverter.Add(HANDPART_Ring);
                if (!snappingMask.Mask.HasFlag(HandMask.MaskPart.Pinky))
                    maskConverter.Add(HANDPART_Pinky);

                base.mask = maskConverter.ToArray();
            }
            else
                base.mask = null;
        }
        #endregion
    }
}
