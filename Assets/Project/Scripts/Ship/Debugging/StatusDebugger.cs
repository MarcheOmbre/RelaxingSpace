#if UNITY_EDITOR
using System;
using Project.Scripts.Ship.Abstracts;
using Project.Scripts.Ship.Components;
using UnityEditor;
using UnityEngine;

namespace Project.Scripts.Ship.Debugging
{
    [ExecuteAlways]
    public class StatusDebugger : MonoBehaviour
    {
        [Serializable]
        public struct ShipComponentTracker
        {
            public AShipComponent shipComponent;
            public bool expand;
        }

        private const string GeneralTitle = "General";

        private const string EnabledLabel = "Enabled";
        private const string StatusLabel = "Status";
        private const string DrivingModeLabel = "Driving Mode";
        private const string GroundedLabel = "Grounded";
        private const string DragLabel = "Drag";
        private const string AngularDragLabel = "Angular drag";
        private const string GravityLabel = "Gravity";
        private const string DistanceLabel = "Distance";
        private const string NormalLabel = "Normal";
        private const string PowerLabel = "Requested power";
        private const string RequestedAngleLabel = "Requested angle";
        private const string StrafeDirectionLabel = "Strafe direction";
        private const string ExpectedRotationLabel = "Expected rotation";
        private const string ExpectedPositionLabel = "Expected position";
        private const string StabilizationTypeLabel = "StabilizationType";
        private const string XZLabel = "XZ";
        private const string YLabel = "Y";

        [SerializeField] private AShipInformation shipInformation;
        [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.25f);
        [SerializeField] private ShipComponentTracker[] shipComponents;

        private GUIStyle _guiStyle;
        private Vector2 _scrollViewPosition = Vector2.zero;

        private void OnValidate()
        {
            Initialize();
        }

        private void Awake()
        {
            //Initializing
            Initialize();
        }
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(_guiStyle);
            _scrollViewPosition = EditorGUILayout.BeginScrollView(_scrollViewPosition);

            //General status
            GUILayout.Label(GeneralTitle, EditorStyles.boldLabel);
                GUILayout.Label($"  {StatusLabel} : {shipInformation.CurrentStatus}");
                GUILayout.Label($"  {DrivingModeLabel} : {shipInformation.CurrentDrivingMode}");

                //AShipComponents status
                for (var i = 0; i < shipComponents.Length; i++)
                    DisplayFoldout(ref shipComponents[i].expand, shipComponents[i].shipComponent);

                EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        
        private void Initialize()
        {
            var background = new Texture2D(1, 1);
            background.SetPixel(0, 0, backgroundColor);
            background.Apply();

            _guiStyle = new GUIStyle {normal = {background = background} };
        }
        /// <summary>
        /// Handles the foldout UI for each component.
        /// </summary>
        /// <param name="isExpanded">If true, the AShipComponents UI is expanded</param>
        /// <param name="shipComponent">The AShipComponents to display</param>
        private void DisplayFoldout(ref bool isExpanded, AShipComponent shipComponent)
        {
            isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(isExpanded, $"{shipComponent.gameObject.name} - {shipComponent.GetType().Name}", EditorStyles.foldoutHeader);

            if (isExpanded)
                DisplayShipComponent(shipComponent);

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        /// <summary>
        /// Display the AShipComponents information depending on its type.
        /// </summary>
        /// <param name="shipComponent">The AShipComponents from which to get the information</param>
        private void DisplayShipComponent(AShipComponent shipComponent)
        {
            GUILayout.Label($"  {shipComponent.gameObject.name}", EditorStyles.boldLabel);
            GUILayout.Label($"  {EnabledLabel} : {shipComponent.IsOn}");

            switch (shipComponent)
            {
                case LandingGear landingGear:
                    GUILayout.Label($"  {StatusLabel} : {landingGear.CurrentStatus}");
                    GUILayout.Label($"  {GroundedLabel} : {landingGear.IsGrounded}");
                    break;
                case Thruster thruster:
                    GUILayout.Label($"  {PowerLabel} : {thruster.RequestedForce.ToString(Variables.FloatFormat)}");
                    break;
                case Rotator rotator:
                    GUILayout.Label($"  {RequestedAngleLabel} : {rotator.RequestedAngleOffset.ToString(Variables.FloatFormat)}");
                    break;
                case RadarAltimeter radarAltimeter:
                {
                    var groundInfo = radarAltimeter.CurrentGroundInfo;
                    GUILayout.Label($"  {GroundedLabel} : {groundInfo != null}");
                    GUILayout.Label($"  {DragLabel} : {radarAltimeter.CurrentPhysicData.drag}");
                    GUILayout.Label($"  {AngularDragLabel} : {radarAltimeter.CurrentPhysicData.angularDrag}");
                    GUILayout.Label($"  {GravityLabel} : {radarAltimeter.CurrentPhysicData.gravity}");
                
                    if (groundInfo == null) 
                        return;
                
                    GUILayout.Label($"  {DistanceLabel} : {groundInfo.Value.Distance.ToString(Variables.FloatFormat)}");
                    GUILayout.Label($"  {NormalLabel} : {groundInfo.Value.Normal.ToString(Variables.FloatFormat)}");
                    break;
                }
                case DoubleAxisThruster doubleAxisThruster:
                    GUILayout.Label($"  {StrafeDirectionLabel} : {doubleAxisThruster.RequestedDirection}");
                    break;
                case AxisThruster axisThruster:
                    GUILayout.Label($"  {StrafeDirectionLabel} : {axisThruster.RequestedDirection}");
                    break;
                case TorquePid torquePid:
                    GUILayout.Label($"  {StabilizationTypeLabel}:");
                    GUILayout.Label($"      {XZLabel}: {torquePid.XZStabilizationType}");
                    GUILayout.Label($"      {YLabel}: {torquePid.YStabilizationType}");
                    GUILayout.Label($"  {ExpectedRotationLabel} : {torquePid.ExpectedRotation.eulerAngles}");
                    break;
                case PositionPid positionPid:
                    GUILayout.Label($"  {StabilizationTypeLabel} : {positionPid.PositionStabilizationType}");
                    GUILayout.Label($"  {ExpectedPositionLabel} : {positionPid.ExpectedPosition}");
                    break;
                default:
                    DisplayShipComponentExtended(shipComponent);
                    break;
            }
        }

        protected virtual void DisplayShipComponentExtended(AShipComponent shipComponent) { }
    }
}
#endif
