using System;
using UnityEngine;


namespace Project.Scripts.Ship.Debugging
{
    [Serializable]
    public struct SingleAxis
    {
        public float DeadZone => deadValue;
        
        public float Value
        {
            get
            {
                var axisX = Input.GetKey(minus);
                var axisY = Input.GetKey(plus);

                if (axisX)
                    _value -= speed * Time.deltaTime;
                if (axisY)
                    _value += speed * Time.deltaTime;

                //Auto back
                if (autoBack && !axisX && !axisY)
                    _value = Mathf.MoveTowards(_value, deadValue, speed * Time.deltaTime);

                //Clamp
                if (clamped != Vector2.zero)
                    _value = Mathf.Clamp(_value, clamped.x, clamped.y);

                return _value;
            }
        }
        
        [SerializeField] private KeyCode plus;
        [SerializeField] private KeyCode minus;
        [SerializeField] [Min(0)] private float speed;
        [SerializeField] private Vector2 clamped;
        [SerializeField] private bool autoBack;
        [SerializeField] private float deadValue;

        private float _value;
        
        public void Reset() { _value = deadValue; }
    }
    
    [Serializable]
    public struct DoubleAxis
    {
        public Vector2 DeadZone => new Vector2(horizontalAxis.DeadZone, verticalAxis.DeadZone);
        
        public Vector2 Value => new Vector2(horizontalAxis.Value, verticalAxis.Value);
        
        [SerializeField] private SingleAxis horizontalAxis;
        [SerializeField] private SingleAxis verticalAxis;

        public void Reset()
        {
            horizontalAxis.Reset();
            verticalAxis.Reset();
        }
    }

    public static class Variables
    {
        public const string FloatFormat = "F2";
    }
}
