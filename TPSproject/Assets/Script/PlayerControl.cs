using System.Collections;
using UnityEngine;

namespace Script
{
    public class PlayerControl : MonoBehaviour
    {
        [Header("MOVE SPEED")] [SerializeField]
        private float moveSpeed = 5;

        [SerializeField] private float moveSpeedRunning = 7;
        [SerializeField] private float moveSpeedWalking = 5;
        [SerializeField] private float moveSpeedCrawling = 2;
        [SerializeField] private float rotationSpeed = 1;
        [SerializeField] private float moveSpeedAttacking = 0;

        [Header("TRANSFORM")] [SerializeField] private Transform CameraReferenceTransform;

        [Header("COMPONENT")] public Animator animatorComponent;
        private Rigidbody rbComponent;

        [Header("CAPSULE COLLIDER")] [SerializeField]
        private CapsuleCollider standPlayer;

        [SerializeField] private CapsuleCollider crouchPlayer;
        [SerializeField] private CapsuleCollider crawlPlayer;

        [SerializeField] public bool isInKillZone = false;

        [Header("VALUE")][Tooltip("Original 9,81")] [SerializeField] private float gravityMultiplier;
    
        public static PlayerControl INSTANCE;

        public enum PlayerStateCollider
        {
            Normal,
            Crouching,
            Crawling,
            Attack
        }

        [Header("STATE PLAYER")] public PlayerStateCollider currentPLayerStateCollider = PlayerStateCollider.Normal;


        private void Awake()
        {
            if (INSTANCE != null && INSTANCE != this) Destroy(this);
            else
            {
                INSTANCE = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        void Start()
        {
            Physics.gravity = new Vector3(0,-gravityMultiplier, 0);
            moveSpeed = moveSpeedWalking;
            currentPLayerStateCollider = PlayerStateCollider.Normal;

            animatorComponent = GetComponent<Animator>();
            if (animatorComponent == null)
            {
                Debug.LogWarning("No Animator detected on " + gameObject.name);
            }

            rbComponent = GetComponent<Rigidbody>();
            if (rbComponent == null)
            {
                Debug.LogWarning("No rb detected on " + gameObject.name);
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

    
        private void Update()
        {
        
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticallInput = Input.GetAxis("Vertical");
            Vector3 camForward = CameraReferenceTransform.forward;
            Vector3 deplacement = verticallInput * camForward + horizontalInput * CameraReferenceTransform.right;
            deplacement.y = 0; // pour pas qu'il rentre dans le sol si la caméra est vu plonger
        
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                moveSpeed = moveSpeedRunning;
                animatorComponent.SetBool("IsRunning", true);
            }

            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                animatorComponent.SetBool("IsRunning", false);
                moveSpeed = moveSpeedWalking;
            }

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                moveSpeed = moveSpeedWalking;
                animatorComponent.SetBool("IsCrouching", true);
                currentPLayerStateCollider = PlayerStateCollider.Crouching;
            }

            if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                animatorComponent.SetBool("IsCrouching", false);
                currentPLayerStateCollider = PlayerStateCollider.Normal;
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                moveSpeed = moveSpeedCrawling;
                animatorComponent.SetBool("IsCrawling", true);
                currentPLayerStateCollider = PlayerStateCollider.Crawling;
            }

            if (Input.GetKeyUp(KeyCode.C))
            {
                animatorComponent.SetBool("IsCrawling", false);
                moveSpeed = moveSpeedWalking;
            }
        

            if (Input.GetKeyDown(KeyCode.Mouse0) && isInKillZone)
            {
                moveSpeed = moveSpeedAttacking;
                animatorComponent.SetTrigger("IsStabbing");
                StartCoroutine(FightStateChanger());
            }

            Vector3 deplacementFinal =
                Vector3.ClampMagnitude(deplacement, 1) *
                moveSpeed; // force pas le comme avec le normalized mais fixe le max à 1
            rbComponent.velocity =
                new Vector3(deplacementFinal.x, rbComponent.velocity.y,
                    deplacementFinal.z); // le rbComponent.velocity.y est utiliser pour permettre de sauter ou de tomber

            if (deplacement != Vector3.zero && currentPLayerStateCollider != PlayerStateCollider.Attack)
            {
                rbComponent.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(deplacement),
                    Time.deltaTime * rotationSpeed);
            }

            if (horizontalInput != 0 || verticallInput != 0) animatorComponent.SetBool("IsWalking", true);
            else animatorComponent.SetBool("IsWalking", false);
        

        }

        private void FixedUpdate()
        {
            if (currentPLayerStateCollider == PlayerStateCollider.Normal && standPlayer.enabled == false)
            {
                standPlayer.enabled = true;
                crouchPlayer.enabled = false;
                crawlPlayer.enabled = false;
            }

            if (currentPLayerStateCollider == PlayerStateCollider.Crouching && crouchPlayer.enabled == false)
            {
                crouchPlayer.enabled = true;
                standPlayer.enabled = false;
                crawlPlayer.enabled = false;
            }

            if (currentPLayerStateCollider == PlayerStateCollider.Crawling && crawlPlayer.enabled == false)
            {
                crawlPlayer.enabled = true;
                crouchPlayer.enabled = false;
                standPlayer.enabled = false;
            }
        
        }

        IEnumerator FightStateChanger()
        {
            currentPLayerStateCollider = PlayerStateCollider.Attack;
            rbComponent.constraints = RigidbodyConstraints.FreezeAll;
            yield return new WaitForSeconds(2);
            currentPLayerStateCollider = PlayerStateCollider.Normal;
            rbComponent.constraints = RigidbodyConstraints.None;
            rbComponent.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;
            
            ;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("killZone")) isInKillZone = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("killZone")) isInKillZone = false;
        }

    
    }
}