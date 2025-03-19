using Script.Player;
using UnityEngine;

namespace Script
{
    public class EnnemyScript : MonoBehaviour
    {
        [SerializeField] private GameObject fxBlood;
        [SerializeField] private Animator animatorComponentEnnemy;
        [SerializeField] private Rigidbody rbComponentEnnemy;
        [SerializeField] private GameObject vfxPlace;

        private void Start()
        {
            animatorComponentEnnemy = GetComponent<Animator>();
            if (animatorComponentEnnemy == null)
            {
                Debug.LogWarning("No Animator detected on "+gameObject.name);
            }
            rbComponentEnnemy = GetComponent<Rigidbody>();
            if (rbComponentEnnemy == null)
            {
                Debug.LogWarning("No rb detected on "+gameObject.name);
            }
        
        }

        private void Update()
        {
            if (PlayerControl.INSTANCE.isInKillZone && PlayerControl.INSTANCE.currentPLayerStateCollider == PlayerControl.PlayerStateCollider.Attack)
            {
                animatorComponentEnnemy.SetBool("isDead", true);
                Instantiate(fxBlood,vfxPlace.transform);
                //GetComponent<EnnemyBehavior>().enabled = false;
                GetComponentInChildren<ConeScript>().gameObject.SetActive(false);
                GetComponentInChildren<RaycastEnnemy>().enabled = false;
                GetComponent<EnnemyScript>().enabled = false;
            
            }
        }
    }
}
