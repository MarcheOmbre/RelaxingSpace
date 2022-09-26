using UnityEngine;

namespace Project.Scripts.Ship.Components
{
    /// <summary>
    /// PID classes. By instantiating this class, you can keep the track of the desired value errors. 
    /// Thanks to https://sudonull.com/post/63631-TAU-for-the-smallest-an-example-of-the-implementation-of-the-PID-controller-in-Unity3D
    /// </summary>
    public class PidController
    {
        private const float MaxIntegral = 9999f;
   
        public float Proportional { get; private set; }
        public float Integral { get; private set; }
        public float Derivative { get; private set; }

        /// <summary>
        /// The proportional influence.
        /// x = kP
        /// y = kI
        /// z = kD
        /// </summary>
        public Vector3 Parameters;

        private float _lastError;
  
        /// <summary>
        /// Updates the PID value and returns the new computed value.
        /// </summary>
        /// <param name="pv">The current value</param>
        /// <param name="sp">The expected value</param>
        /// <param name="deltaTime">The delta time used for the computation</param>
        /// <returns>The computed value</returns>
        public float RefreshPid(float pv, float sp, float deltaTime)
        {
            Proportional = sp - pv;
            Integral = Mathf.Clamp(Integral + Proportional * deltaTime, -MaxIntegral, MaxIntegral);     
            Derivative = (Proportional - _lastError) / deltaTime;

            _lastError = Proportional;

            return Proportional * Parameters.x + Integral * Parameters.y + Derivative * Parameters.z;
        }
   
        public void ResetPid()
        {
            Proportional = 0;
            Integral = 0;
            Derivative = 0;
            _lastError = 0;
        }
    }
}
