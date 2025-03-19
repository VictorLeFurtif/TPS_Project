using Script.Player;
using UnityEngine;

namespace Script
{
    public class ConeScript : MonoBehaviour
    {
        public bool isInCone;
        public RaycastEnnemy raycastEnnemy;

        public void Start()
        {
            raycastEnnemy = GetComponentInParent<RaycastEnnemy>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && raycastEnnemy.isPlayerTouched == true)
            {
                PlayerControl.INSTANCE.animatorComponent.SetTrigger("PlayerDead");
                PlayerControl.INSTANCE.enabled = false;
                GetComponent<ConeScript>().enabled = false;
            }
        }
    }
}
