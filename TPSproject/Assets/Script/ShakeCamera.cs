using UnityEngine;

namespace Script
{
    public class ShakeCamera : MonoBehaviour
    {
        public Transform player;  
        public float shakeIntensity = 0.1f;  
        public float shakeSpeed = 10f;  
        public bool isRunning = false; 
    
        private Vector3 initialPosition;

        void Start()
        {
            initialPosition = transform.localPosition;
        }

   
        void Update()
        {
            isRunning = Input.GetKey(KeyCode.LeftShift);
        
            if (isRunning)
            {
           
                float offsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;
                float offsetY = Mathf.Cos(Time.time * shakeSpeed) * shakeIntensity;
                transform.localPosition = initialPosition + new Vector3(offsetX, offsetY, 0);
            }
            else
            {
           
                transform.localPosition = initialPosition;
            }
        }
    }
}
