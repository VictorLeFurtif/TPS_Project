using UnityEngine;

namespace Script
{
    public class RaycastEnnemy : MonoBehaviour
    {

        public GameObject player; 
        public float rayLength = 20f; 
        public Color rayColor = Color.red; 
        public Color rayColorNoObstacle = Color.green;
        public bool isPlayerTouched = false;
    

        private void Update()
        {
       
            Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
            Ray ray = new Ray(transform.position, directionToPlayer);
            RaycastHit hit;
        
            if (Physics.Raycast(ray, out hit, rayLength))
            {
                if (hit.collider.CompareTag("obstacles"))
                {
                    Debug.DrawRay(transform.position, directionToPlayer * hit.distance, rayColor);
                    isPlayerTouched = false;
                }
                else
                {
                    Debug.DrawRay(transform.position, directionToPlayer * rayLength, rayColorNoObstacle);
                    isPlayerTouched = true;
                }
            }
            else Debug.DrawRay(transform.position, directionToPlayer * rayLength, rayColorNoObstacle);
        }
    }
}

