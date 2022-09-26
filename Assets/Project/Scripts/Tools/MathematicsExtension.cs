using UnityEngine;

namespace Project.Scripts.Tools
{
    public static class MathematicsExtension
    {
        public static Quaternion SmoothDamp(this Quaternion current, Quaternion target, ref float velocity, float deltaTime, float? maxSpeed = null)
        {
            var angle = Quaternion.Angle(current, target);

            float t; 
            
            //Applies a smooth damp on the angle between the current and the target rotation.
            t = maxSpeed != null ? 
                Mathf.SmoothDampAngle(0.0f, angle, ref velocity, deltaTime, maxSpeed.Value) : 
                Mathf.SmoothDampAngle(0.0f, angle, ref velocity, deltaTime);

            if (angle <= 0)
                return current.normalized;
            
            //OneMinus on the angle normalized value (between 0 and the angle between the current and the target rotation)
            t /= angle;
            current = Quaternion.Slerp(current, target, t);

            return current.normalized;
        }
    }
}
