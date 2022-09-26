using Interhaptics.InteractionsEngine.Shared.Types;
using Interhaptics.InteractionsEngine;
using Interhaptics.Modules.Interaction_Builder.Core;
using UnityEngine;

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
#endif

namespace Interhaptics.ObjectSnapper.core
{
    [ExecuteInEditMode]
    public class SnappingPrimitive : MonoBehaviour
    {
#if UNITY_EDITOR
        #region Constants
        //General
        private const string LABEL_Separator = " ";

        //Resources
        private const string VALUE_ModelsPath = "3DModels/";
        private const string VALUE_IBCircle = "IBCircle";
        private const string VALUE_IBCylinder = "IBCylinder";
        private const string VALUE_IBTunnel = "IBTunnel";
        private const string VALUE_IBSphere = "IBSphere";
        private const string VALUE_IBCapsuleRound = "IBCapsuleRound";

        //Shape
        private const string DEBUG_ResourcesNotFound = "Some resources were not found";

        //Snapping
        private const string LABEL_Tracking = "Tracking";
        private const string LABEL_Reset = "Reset";
        private const string DEBUG_NoAnimator = "ASnappableActor's Animator is null";
        private const string DEBUG_NoAnimatorController = "No AnimatorController found on this ASnappableActor's Animator";
        private const string DEBUG_SaveFailed = "Failed to save the current pose";
        private const float VALUE_TrackingRadius = 0.05f;
        private const float VALUE_TrackingDistance = 0.05f;
        private const int VALUE_AxisLength = 2;
        #endregion

        #region Variables
        private static SnappingPrimitive s_editModSigleton = null;

        //Shape
        [SerializeField] protected Color primaryColor = Color.blue;
        [SerializeField] protected Color secondaryColor = Color.red;
        private PrimitiveShape _lastPrimitiveForm = (PrimitiveShape)(-1);

        private Mesh _primaryMesh = null, _secondaryMesh = null;
        private Vector3 _scale = Vector3.one;

        //Tracking simulation
        [SerializeField][Min(0)] protected float trackingRadius = VALUE_TrackingRadius;
        [SerializeField] protected Color trackingColor = Color.black;
        [SerializeField][Min(0)] protected float trackingDistance = VALUE_TrackingDistance;
        [SerializeField][Min(0)] protected float trackingAxisLength = VALUE_AxisLength;

        //Snapping
        [SerializeField] protected ModelSnappableActor modelActor = null;
        [SerializeField] protected bool displaySkin = false;
        [SerializeField] protected Color skinColor = Color.green;
        [SerializeField] [HideInInspector] protected SphereCollider _simulationTracking = null;
        [SerializeField] [HideInInspector] protected ModelSnappableActor _simulationActor = null;

        //DEPRECATED
        [SerializeField] protected SnappingData snappingData = null;

        private ASnappableActor _lastModelActor = null;
        #endregion

        #region Life Cycle
        //Unity
        [ContextMenu(LABEL_Reset, false, 0)]
        private void Reset()
        {
            //Shape
            primitiveShape = PrimitiveShape.Sphere;
            movementType = MovementType.Rotation;
            primaryColor = Color.blue;
            secondaryColor = Color.red;
            localPosition = Vector3.zero;
            localRotation = Vector3.zero;
            primaryRadius = 0.25f;
            length = 1f;
            secondaryRadius = 0.2f;
            _lastPrimitiveForm = (PrimitiveShape)(-1);
            _scale = Vector3.one;
            _primaryMesh = null;
            _secondaryMesh = null;

            modelActor = null;
            trackingRadius = VALUE_TrackingRadius;
            trackingColor = Color.black;
            trackingDistance = VALUE_TrackingRadius;
            trackingAxisLength = VALUE_AxisLength;
            posesData = null;

            //Cutom reset
            this.OnReset();

            this.ResetSnappingEdition();

            _lastModelActor = null;
        }

        protected virtual void OnValidate()
        {
            //Shape
            this.ShapeOnValidate();

            //Snapping
            this.SnappingOnValidate();
        }

        protected virtual void OnDrawGizmos()
        {
            if (!this.isActiveAndEnabled)
                return;

            //Shape
            this.ShapeOnDrawGizmo();

            //Snapping
            this.SnappingOnDrawGizmo();
        }

        //General
        protected virtual void EditorUpdate(SceneView sceneView)
        {
            this.InputsHandling();

            //Shape
            this.ShapeUpdate();

            //Dependencies
            this.SnappingUpdate();
        }

        //Shape
        protected virtual void ShapeOnValidate()
        {
            secondaryRadius = (primaryRadius > 0) ? Mathf.Clamp(secondaryRadius, 0, primaryRadius / 2) : 0;
        }

        protected virtual void ShapeOnDrawGizmo()
        {
            Vector3 gizmoPosition = this.ShapePosition;
            Quaternion gizmoRotation = this.ShapeRotation;

            if (_primaryMesh)
            {
                Gizmos.color = displaySkin ? skinColor : primaryColor;

                Vector3 snappingScale = _scale;
                if (displaySkin && primitiveShape != PrimitiveShape.Torus)
                {
                    snappingScale += Vector3.one * skinWidth * 2;

                    if (primitiveShape == PrimitiveShape.Cylinder)
                        snappingScale.y = length + skinLength;
                    else if (primitiveShape == PrimitiveShape.Capsule)
                        snappingScale.y = length;
                }

                Gizmos.DrawMesh(_primaryMesh, gizmoPosition, gizmoRotation, snappingScale);



                //Torus tube
                if (_secondaryMesh)
                {
                    if (primitiveShape == PrimitiveShape.Torus)
                    {
                        Gizmos.color = displaySkin ? skinColor : secondaryColor;

                        float snappingSecondRadius = displaySkin ? secondaryRadius + skinWidth * 2 : secondaryRadius;
                        Gizmos.DrawMesh(_secondaryMesh, gizmoPosition + (gizmoRotation * Vector3.up) * (primaryRadius), gizmoRotation * Quaternion.Euler(0, 0, 90), Vector3.one * snappingSecondRadius);
                    }
                    else if (primitiveShape == PrimitiveShape.Capsule)
                    {
                        snappingScale = Vector3.one * primaryRadius;
                        if (displaySkin)
                            snappingScale += Vector3.one * skinWidth * 2;

                        Gizmos.DrawMesh(_secondaryMesh, gizmoPosition + (gizmoRotation * Vector3.up) * length, gizmoRotation, snappingScale);
                        Gizmos.DrawMesh(_secondaryMesh, gizmoPosition + (gizmoRotation * Vector3.down) * length, gizmoRotation * Quaternion.Euler(180, 0, 0) * Quaternion.Inverse(gizmoRotation) * gizmoRotation, snappingScale);
                    }
                }
            }
        }

        protected virtual void ShapeUpdate()
        {
            if (primitiveShape != _lastPrimitiveForm)
            {
                //Load gizmos' mesh from resources
                switch (primitiveShape)
                {
                    case PrimitiveShape.Sphere:

                        if (_primaryMesh == null || _primaryMesh.name != VALUE_IBSphere)
                            _primaryMesh = this.GetResourcesMeshByName(VALUE_IBSphere);

                        break;
                    case PrimitiveShape.Cylinder:

                        if (_primaryMesh == null || _primaryMesh.name != VALUE_IBCylinder)
                            _primaryMesh = this.GetResourcesMeshByName(VALUE_IBCylinder);

                        break;
                    case PrimitiveShape.Capsule:

                        if (_primaryMesh == null || _primaryMesh.name != VALUE_IBTunnel)
                            _primaryMesh = this.GetResourcesMeshByName(VALUE_IBTunnel);

                        if (primitiveShape == PrimitiveShape.Capsule)
                        {
                            if (_secondaryMesh == null || _secondaryMesh.name != VALUE_IBCapsuleRound)
                                _secondaryMesh = this.GetResourcesMeshByName(VALUE_IBCapsuleRound);
                        }

                        break;
                    case PrimitiveShape.Torus:

                        if (_primaryMesh == null || _primaryMesh.name != VALUE_IBCircle)
                            _primaryMesh = this.GetResourcesMeshByName(VALUE_IBCircle);

                        if (_secondaryMesh == null || _secondaryMesh.name != VALUE_IBCylinder)
                            _secondaryMesh = this.GetResourcesMeshByName(VALUE_IBCylinder);

                        break;
                    default:
                        break;
                }

                _lastPrimitiveForm = primitiveShape;
            }

            switch (primitiveShape)
            {
                case PrimitiveShape.Sphere:

                    _scale = Vector3.one * primaryRadius * 2;

                    break;
                case PrimitiveShape.Cylinder:
                case PrimitiveShape.Capsule:

                    _scale = new Vector3(primaryRadius, length, primaryRadius);

                    break;

                case PrimitiveShape.Torus:

                    _scale = Vector3.one * primaryRadius * 2;

                    break;

                default:
                    break;
            }
        }

        //Snapping
        protected virtual void SnappingOnValidate()
        {
            //Check Animator
            if ((!posesData || Application.isPlaying) && modelActor)
                modelActor = null;
        }

        protected virtual void SnappingOnDrawGizmo()
        {
            if (!_simulationTracking || !_simulationActor)
                return;

            Gizmos.color = trackingColor;
            Gizmos.DrawSphere(_simulationTracking.transform.position, trackingRadius);

            Vector3 simulationTrackingPosition = _simulationTracking.transform.position;
            Quaternion simulationTrackingRotation = _simulationTracking.transform.rotation;

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(simulationTrackingPosition, simulationTrackingPosition + 
                simulationTrackingRotation * _simulationActor.RepresentativeForward * trackingRadius * trackingAxisLength);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(simulationTrackingPosition, simulationTrackingPosition + 
                simulationTrackingRotation * _simulationActor.RepresentativeUp * trackingRadius * trackingAxisLength);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(simulationTrackingPosition, simulationTrackingPosition + 
                Vector3.Cross(simulationTrackingRotation * _simulationActor.RepresentativeForward, 
                simulationTrackingRotation * _simulationActor.RepresentativeUp) * trackingRadius * trackingAxisLength);
            Gizmos.color = Color.yellow;

            Gizmos.DrawLine(simulationTrackingPosition, IbTools.Convert(this.GetSnappedSpatialRepresentation(this.GetTrackingSP(), IbTools.Convert(_simulationActor.RepresentativeForward), IbTools.Convert(_simulationActor.RepresentativeUp)).Position));
        }

        protected virtual void SnappingUpdate()
        {
            //Check if there is a new ASnappableActor
            if (modelActor != _lastModelActor)
            {
                if (modelActor && s_editModSigleton != this)
                {
                    if (s_editModSigleton)
                    {
                        s_editModSigleton.QuickSave();
                        s_editModSigleton.ResetSnappingEdition();
                    }

                    s_editModSigleton = this;

#if UNITY_2019_2_OR_NEWER
                    if (SceneVisibilityManager.instance)
                    {
                        List<GameObject> isolatedGameObject = new List<GameObject>();
                        isolatedGameObject.Add(gameObject);

                        if (_simulationActor)
                            isolatedGameObject.Add(_simulationActor.gameObject);

                        if (_simulationTracking)
                            isolatedGameObject.Add(_simulationTracking.gameObject);

                        SceneVisibilityManager.instance.Isolate(isolatedGameObject.ToArray(), true);
                    }
#endif
                }

                _lastModelActor = modelActor;
            }

            //Set tracking and the ModelSnappableActor
            if (modelActor)
            {
                if (!_simulationActor)
                {
                    _simulationActor = GameObject.Instantiate<ModelSnappableActor>(modelActor);

                    if (_simulationActor)
                    {
                        _simulationActor.gameObject.hideFlags = HideFlags.DontSave;

                        if (!_simulationActor.Animator)
                            Debug.LogWarning(DEBUG_NoAnimator);
                        else if (!_simulationActor.Animator.runtimeAnimatorController)
                            Debug.LogWarning(DEBUG_NoAnimatorController, modelActor);
                        else
                            _simulationActor.SnapTo(this);
                    }
                }

                if (_simulationActor && !_simulationTracking)
                {
                    _simulationTracking = new GameObject(LABEL_Tracking).AddComponent<SphereCollider>();
                    _simulationTracking.gameObject.hideFlags = HideFlags.DontSave;
                    _simulationTracking.radius = trackingRadius;

                    //Spatial representation
                    Camera camera = SceneView.lastActiveSceneView.camera;
                    _simulationTracking.transform.position = camera ? (camera.transform.position) : Vector3.zero;

                    _simulationTracking.transform.rotation = camera ? Quaternion.FromToRotation(camera.transform.forward, 
                        _simulationTracking.transform.rotation * _simulationActor.RepresentativeUp) * (camera.transform.rotation) : Quaternion.identity;

                    this.AlignTracking();
                }
            }
            else
                this.ResetSnappingEdition();
        }

        protected virtual void SnappingOnDisable()
        {
            this.ResetSnappingEdition();
        }
        #endregion

        #region Privates
        //General
        private Mesh GetResourcesMeshByName(string name)
        {
            Mesh foundedMesh = Resources.Load<Mesh>(VALUE_ModelsPath + name);

            if (foundedMesh == null)
                Debug.LogError(DEBUG_ResourcesNotFound + LABEL_Separator + name);

            return foundedMesh;
        }

        private SpatialRepresentation GetTrackingSP()
        {
            SpatialRepresentation spatialRepresentation = default;

            if (_simulationTracking)
            {
                spatialRepresentation = new SpatialRepresentation()
                {
                    Position = IbTools.Convert(_simulationTracking.transform.position),
                    Rotation = IbTools.Convert(_simulationTracking.transform.rotation)
                };

                if (_simulationActor)
                {
                    spatialRepresentation = this.GetSnappedSpatialRepresentation(spatialRepresentation, 
                        IbTools.Convert(_simulationActor.RepresentativeForward), IbTools.Convert(_simulationActor.RepresentativeUp));
                    spatialRepresentation.Position += System.Numerics.Vector3.Transform(IbTools.Convert(_simulationActor.RepresentativeUp) * 
                        trackingDistance, spatialRepresentation.Rotation);
                }
            }

            return spatialRepresentation;
        }

        private void AlignTracking()
        {
            if (!_simulationTracking)
                return;

            SpatialRepresentation trackingSP = this.GetTrackingSP();
            _simulationTracking.transform.SetPositionAndRotation(IbTools.Convert(trackingSP.Position), IbTools.Convert(trackingSP.Rotation));
        }
        #endregion

        #region Protecteds
        protected virtual void OnReset() { }
        #endregion

        #region Publics
        /// <summary>
        /// Returns the snapping computed SpatialRepresentation, depending on the editor tracking.
        /// </summary>
        /// <param name="modelSnappableActor">ModelSnappableActor to snap</param>
        /// <returns>Snapping corresponding to the tracking Spatial representation</returns>
        public SpatialRepresentation GetModelSpatialRepresentation(ModelSnappableActor modelSnappableActor)
        {
            return this.GetComputedSpatialRepresentation(this.GetTrackingSP(), modelSnappableActor);
        }
        /// <summary>
        /// Closes the editing mode.
        /// </summary>
        public void ResetSnappingEdition()
        {
            //Simulation
            if (_simulationTracking)
                GameObject.DestroyImmediate(_simulationTracking.gameObject);

            if (_simulationActor)
                GameObject.DestroyImmediate(_simulationActor.gameObject);

            //Variables
            if (modelActor)
            {
                modelActor = null;
                EditorUtility.SetDirty(this);
            }

            _lastModelActor = null;

            if (s_editModSigleton == this)
            {
                s_editModSigleton = null;

#if UNITY_2019_2_OR_NEWER
                if (SceneVisibilityManager.instance && SceneVisibilityManager.instance.IsCurrentStageIsolated())
                    SceneVisibilityManager.instance.ExitIsolation();
#endif
            }
        }
        /// <summary>
        /// Handles the editor Inputs.
        /// </summary>
        protected virtual void InputsHandling()
        {
            Event currentEvent = Event.current;
            if (currentEvent == null)
                return;

            if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
                this.AlignTracking();

            if (Application.isPlaying)
            {
                this.QuickSave();
                this.ResetSnappingEdition();
            }
        }
        /// <summary>
        /// Saves the PoseData.
        /// </summary>
        /// <param name="modelSnappableActor">The ModelSnappableActor to save the Pose</param>
        /// <param name="forward">Custom forward</param>
        /// <param name="upward">Custom upward</param>
        /// <returns>True on success</returns>
        public bool SaveActorData(ModelSnappableActor modelSnappableActor, PosesData poseData)
        {
            bool success = modelSnappableActor && poseData && poseData.Save(modelSnappableActor);

            if (!success)
                Debug.LogError(DEBUG_SaveFailed);

            return success;
        }
        /// <summary>
        /// Used from the SnappingPrimitiveEditor. The methods saves the PoseData using the ModelSnappableActor set in the inspector and the SnappingPrimitive tracking.
        /// </summary>
        /// <returns>True on success</returns>
        public bool QuickSave()
        {
            bool success = false;

            if (_simulationActor && posesData)
                success = this.SaveActorData(_simulationActor, posesData);

            return success;
        }
        #endregion
#endif

        #region Constantes
        //Shape
        private const float VALUE_PrimaryRadius = 0.25f;
        private const float VALUE_SecondaryRadius = 0.2f;
        private const float VALUE_Length = 1f;
        //Snapping
        private const string TOOLTIP_MovementType = "Define whether the spatial representation of the ASnappableActor is calculated from the rotation or the position of the tracking.";
        private const string TOOLTIP_PoseData = "The poses container";
        private const string TOOLTIP_FixedSnapping = "If true, the SnappableActor keeps it's first frame local SpatialRepresentation to this SnappingPrimitive. It is fixed on the SnappingPrimitive.";
        private const string TOOLTIP_IsPartialSnapping = "If true, BodyParts ignored by snapping may still be snapped if they enter the snapping area, determined by the snapping skin";
        private const string TOOLTIP_DirectionLocked = "Except for the primitive sphere. If true, the ASnappableActor keeps its direction relative to the SnappingPrimitive. It does not change direction if not released.";
        private const string TOOLTIP_SkinWidth = "The thickness of the skin added to the snapping primitive. This skin is used to detect if any of the bodyparts ignored by the snapping should be snapped during partial snapping.";
        private const string TOOLTIP_SkinLength = "Only for the cylinder, determine the extra length of the skin.";

        private const string DEBUG_NoSnappingPrimitive = "No SnappingPrimitive found on it or one of its parents";
        #endregion

        #region Properties
        /// <summary>
        /// If true, the SnappableActor keeps it's first frame local SpatialRepresentation to this SnappingPrimitive. It is fixed on the SnappingPrimitive.
        /// </summary>
        /// <see cref="ASnappableActor"/>
        public bool IsFixedSnapping
        {
            get { return isFixedSnapping; }

            set { isFixedSnapping = false; }
        }
        /// <summary>
        /// Returns the mask set in the inspector.
        /// </summary>
        public string[] Mask { get { return mask; } }
        /// <summary>
        /// Returns true if the ignored Bodypart can be snapped if in contact with the SnappingPrimitive.
        /// </summary>
        public bool IsPartialSnapping { get { return isPartialSnapping; } }
        /// <summary>
        /// Not available for the sphere primitives. Returns true if the Bodypart keeps its direction relative to the SnappingPrimitive.
        /// </summary>
        public bool IsDirectionLocked { get { return isDirectionLocked; } }
        /// <summary>
        /// Returns the SnappingPrimitive type.
        /// </summary>
        public PrimitiveShape shapePrimitive { get { return primitiveShape; } }
        /// <summary>
        /// Returns the snapping shape's world position.
        /// </summary>
        public Vector3 ShapePosition { get { return transform.position + (transform.rotation * localPosition); } }
        /// <summary>
        /// Returns the snapping shape's world position.
        /// </summary>
        public Quaternion ShapeRotation { get { return transform.rotation * Quaternion.Euler(localRotation); } }
        #endregion

        #region Variables
        //Shape
        /// <summary>
        /// A string list used to determine if the bodypart is ignored. 
        /// In ARuntimeSnappableActor, if the string returned by the OnExtractingBodypartMaskLayer method is found in this mask, then that bodypart is considered as ignored.
        /// </summary>
        /// <see cref="ARuntimeSnappableActor.OnExtractingBodypartMaskLayer(Transform)"/>
        [SerializeField] protected string[] mask = null;
        /// <summary>
        /// The shape of the primitive that drives the movement.
        /// </summary>
        [SerializeField] protected PrimitiveShape primitiveShape = PrimitiveShape.Sphere;
        /// <summary>
        /// Defines whether the spatial representation of the ASnappableActor is calculated from the rotation or the position of the tracking.
        /// </summary>
        [SerializeField] [Tooltip(TOOLTIP_MovementType)] protected MovementType movementType = MovementType.Rotation;
        /// <summary>
        /// The radius of the shape.
        /// </summary>
        [SerializeField][Min(0)] protected float primaryRadius = VALUE_PrimaryRadius;
        /// <summary>
        /// The length of the shape (Reserved for the cylinder and Capsule shape).
        /// </summary>
        [SerializeField][Min(0)] protected float length = VALUE_Length;
        /// <summary>
        /// The second radius of the shape (reserved to the toruse tube's radius).
        /// </summary>
        [SerializeField][Min(0)] protected float secondaryRadius = VALUE_SecondaryRadius;
        /// <summary>
        /// The local position relative to the GameObject on which the script is attached.
        /// </summary>
        [SerializeField] protected Vector3 localPosition = Vector3.zero;
        /// <summary>
        /// The local rotation relative to the GameObject on which the script is attached.
        /// </summary>
        [SerializeField] protected Vector3 localRotation = Vector3.zero;
        /// <summary>
        /// The thickness of the skin added to the snapping primitive. 
        /// This skin is used to detect if any of the bodyparts ignored by the snapping should be snapped during partial snapping.
        /// </summary>
        [Tooltip(TOOLTIP_SkinWidth)][SerializeField][Min(0)] protected float skinWidth = 0;
        /// <summary>
        /// Only for the cylinder, determine the extra length of the skin.
        /// </summary>
        [Tooltip(TOOLTIP_SkinLength)] [SerializeField][Min(0)] protected float skinLength = 0;

        //Snapping
        /// <summary>
        /// The poses container. To create one: Right-click -> Create -> Interhaptics -> PosesData.
        /// </summary>
        [Tooltip(TOOLTIP_PoseData)] [SerializeField] protected PosesData posesData = null;
        /// <summary>
        /// If true, the SnappableActor keeps it's first frame local SpatialRepresentation to this SnappingPrimitive. It is fixed on the SnappingPrimitive.
        /// </summary>
        [Tooltip(TOOLTIP_FixedSnapping)] [SerializeField] protected bool isFixedSnapping = false;
        /// <summary>
        /// Except for the primitive sphere. If true, the Bodypart keeps its direction relative to the SnappingPrimitive. It does not change direction if not released.
        /// </summary>
        [Tooltip(TOOLTIP_DirectionLocked)] [SerializeField] protected bool isDirectionLocked = false;
        /// <summary>
        /// If true, Bodyparts ignored by snapping may still be snapped if they enter the snapping area, determined by the snapping skin.
        /// </summary>
        [Tooltip(TOOLTIP_IsPartialSnapping)] [SerializeField] private bool isPartialSnapping = true;

        private SnappingObject _snappingObject = null;
        #endregion

        #region Life Cycle
        protected virtual void Awake()
        {
            //Dependencies
            _snappingObject = gameObject.GetComponentInParent<SnappingObject>();
            if (!_snappingObject)
                Debug.LogWarning(DEBUG_NoSnappingPrimitive, this);
        }

        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += EditorUpdate;
#else
            SceneView.onSceneGUIDelegate += Update;
#endif
#endif
            //Snapping
            if (_snappingObject)
                _snappingObject.SubscribePrimitive(this);
        }

        protected virtual void OnDisable()
        {
#if UNITY_EDITOR
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= EditorUpdate;
#else
            SceneView.onSceneGUIDelegate -= Update;
#endif            
            //Snapping
            this.SnappingOnDisable();
#endif

            if (_snappingObject)
                _snappingObject.UnsubscribePrimitive(this);
        }
        #endregion

        #region Privates
        private SpatialRepresentation GetSnappedSpatialRepresentation(SpatialRepresentation trackingSR, System.Numerics.Vector3 forward, System.Numerics.Vector3 upward)
        {
            if (forward != System.Numerics.Vector3.Zero && upward != System.Numerics.Vector3.Zero)
            {
                SpatialRepresentation snappingPrimitiveSR = new SpatialRepresentation()
                {
                    Position = IbTools.Convert(transform.position + (transform.rotation * localPosition)),
                    Rotation = IbTools.Convert(transform.rotation * Quaternion.Euler(localRotation))
                };

                switch (primitiveShape)
                {
                    case PrimitiveShape.Sphere:
                        trackingSR = InteractionEngineApi.SphereComputing(forward, upward, trackingSR, snappingPrimitiveSR, primaryRadius);
                        break;
                    case PrimitiveShape.Cylinder:
                    case PrimitiveShape.Capsule:
                        trackingSR = InteractionEngineApi.CylinderComputing(forward, upward, trackingSR, snappingPrimitiveSR, length, primaryRadius, movementType, primitiveShape == PrimitiveShape.Cylinder);
                        break;
                    case PrimitiveShape.Torus:
                        trackingSR = InteractionEngineApi.TorusComputing(forward, upward, trackingSR, snappingPrimitiveSR, primaryRadius, secondaryRadius, movementType);
                        break;
                    default:
                        break;
                }
            }
            return trackingSR;
        }

        /// <summary>
        /// Returns the ASnappableActor pose.
        /// </summary>
        /// <param name="aSnappableActor">The actor key</param>
        /// <returns>Null if not existing</returns>
        public SpatialRepresentation[] GetPose(ASnappableActor aSnappableActor)
        {
            SpatialRepresentation[] spatialRepresentations = null;
            if (posesData)
                posesData.TryGetPose(aSnappableActor, out spatialRepresentations);

            return spatialRepresentations;
        }
        #endregion

        #region Publics

        /// <summary>
        /// Verifies if the position is located in the primitive (bounded by the skin).
        /// </summary>
        /// <param name="position">The position to verify</param>
        public bool IsInPrimitive(Vector3 position)
        {
            Vector3 shapePosition = this.ShapePosition;
            Quaternion shapeRotation = this.ShapeRotation;
            Vector3 direction = position - shapePosition;

            bool IsIn = false;

            if (primitiveShape == PrimitiveShape.Sphere)
            {
                IsIn = direction.magnitude <= primaryRadius + skinWidth;
            }
            else if (primitiveShape == PrimitiveShape.Cylinder || primitiveShape == PrimitiveShape.Capsule)
            {
                Vector3 rotationAxis = shapeRotation * Vector3.up;
                Vector3 verticalDirection = Vector3.Project(direction, rotationAxis);

                float halfLength = primitiveShape == PrimitiveShape.Cylinder ? length + skinLength : length;
                float verticalMagnitude = verticalDirection.magnitude;
                bool verticalIn = verticalMagnitude <= halfLength;
                bool horizontalIn = Vector3.ProjectOnPlane(direction, rotationAxis).magnitude <= primaryRadius + skinWidth;

                if (primitiveShape == PrimitiveShape.Capsule && horizontalIn && !verticalIn)
                {
                    if ((position - (transform.TransformPoint(localPosition) + verticalDirection.normalized * halfLength)).magnitude <= primaryRadius + skinWidth)
                        verticalIn = true;
                }
                
                IsIn = verticalIn && horizontalIn;
            }
            else if(primitiveShape == PrimitiveShape.Torus)
            {
                Vector3 rotationAxis = shapeRotation * Vector3.forward;

                float verticaMagnitude = Vector3.ProjectOnPlane(direction, rotationAxis).magnitude;
                bool verticalIn = verticaMagnitude > primaryRadius - secondaryRadius - skinWidth && verticaMagnitude < verticaMagnitude + secondaryRadius + skinWidth;
                float horizontalMagnitude = Vector3.Project(direction, rotationAxis).magnitude;
                bool horizontalIn = horizontalMagnitude < secondaryRadius + skinWidth;

                IsIn = verticalIn && horizontalIn;
            }

            return IsIn;
        }

        /// <summary>
        /// Returns the snapping SpatialRepresentation of an ASnappableActor (depending on its PosesData).
        /// </summary>
        /// <param name="trackingSP">The tracking SpatialRepresentation</param>
        /// <param name="snappableActor">The actor key</param>
        /// <returns>The tracking spatial representation if its PosesData has not been found</returns>
        public SpatialRepresentation GetComputedSpatialRepresentation(SpatialRepresentation trackingSP, ASnappableActor snappableActor)
        {
            System.Numerics.Vector3 forward = System.Numerics.Vector3.UnitZ;
            System.Numerics.Vector3 up = System.Numerics.Vector3.UnitY;

            if (snappableActor)
            {
                forward = IbTools.Convert(snappableActor.RepresentativeForward);
                up = IbTools.Convert(snappableActor.RepresentativeUp);
            }

            return this.GetSnappedSpatialRepresentation(trackingSP, forward, up);
        }
    }

    #endregion
}