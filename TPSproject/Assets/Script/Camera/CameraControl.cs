using Script.Player;
using UnityEngine;

namespace Script
{
    public class CameraControl : MonoBehaviour
    {
        [SerializeField] private Transform cible;
        [SerializeField] private float vitesseRotation = 2;
        [SerializeField] private float hauteur;
        [SerializeField] private float distance = 5;
        [SerializeField] private float shakeIntensity = 0.1f;  
        [SerializeField] private float shakeSpeed = 10f;      

        private float rotationX = 0;
        private float rotationY = 0;

        private Vector3 initialPosition;
        private bool isRunning = false;

        void Update()
        {
        CameraFollowTarget();
        }

        private void CameraFollowTarget()
        {
            rotationY += Input.GetAxis("Mouse X") * vitesseRotation;
            rotationX -= Input.GetAxis("Mouse Y") * vitesseRotation;
            rotationX = Mathf.Clamp(rotationX, -30, 30);
        
            Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);
            transform.rotation = rotation;

      
            Vector3 position = cible.position + rotation * new Vector3(0, 0, -distance);
        
        
            isRunning = Input.GetKey(KeyCode.LeftShift); 
        
            if (isRunning)
            {
     
                float offsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;
                float offsetY = Mathf.Cos(Time.time * shakeSpeed) * shakeIntensity;
                position += new Vector3(offsetX, offsetY, 0);
            }

            if (PlayerControl.INSTANCE.currentPLayerStateCollider == PlayerControl.PlayerStateCollider.Climbing)
            {
                transform.position = Vector3.Lerp(transform.position,cible.transform.position + new Vector3(-distance,4,0),Time.deltaTime * 4);
                return;
            }
            
            transform.position = position; 
        }
    }
}