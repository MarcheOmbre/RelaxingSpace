using UnityEngine;

namespace Interhaptics.ObjectSnapper.core
{
    public static class MathematicsExtension
    {
        #region Variables
        /// <summary>
        /// Smooth damp adaptation for quaternions.
        /// </summary>
        /// <param name="current">The current rotation</param>
        /// <param name="target">The target rotation</param>
        /// <param name="velocity">The angle velocity</param>
        /// <param name="deltaTime">The refresh delta time</param>
        /// <param name="maxSpeed">The maximum angle speed</param>
        /// <returns>The normalized result quaternion</returns>
        public static Quaternion SmoothDamp(this Quaternion current, Quaternion target, ref float velocity, float deltaTime, float? maxSpeed = null)
        {
            float angle = Quaternion.Angle(current, target);

            float t; 
            
            //Applies a smooth damp on the angle between the current and the target rotation.
            if(maxSpeed != null)
                t = Mathf.SmoothDampAngle(0.0f, angle, ref velocity, deltaTime, maxSpeed.Value);
            else
                t = Mathf.SmoothDampAngle(0.0f, angle, ref velocity, deltaTime);

            if (angle > 0)
            {
                //OneMinus on the angle normalized value (between 0 and the angle between the current and the target rotation)
                t /= angle;
                current = Quaternion.Slerp(current, target, t);
            }

            return current.normalized;
        }
        #endregion
    }
}
