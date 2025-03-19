using System.Collections;
using UnityEngine;

namespace Script.Player
{
    public class PlayerControl : MonoBehaviour
    {
        [SerializeField] private int vaultLayer;
        
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
        [SerializeField] private AnimationClip animationClimb;

        [Header("CAPSULE COLLIDER")] [SerializeField]
        private CapsuleCollider standPlayer;

        private float playerHeight;
        private float playerRadius;

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
            Attack,
            Climbing
        }

        [Header("STATE PLAYER")] 
        public PlayerStateCollider currentPLayerStateCollider = PlayerStateCollider.Normal;


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

            playerHeight = standPlayer.height;
            playerRadius = standPlayer.radius;
            
            //lock Cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

    
        private void Update()
        {
        PlayerMovement();
        Vault();
        Vector3 playerHead = gameObject.transform.position + new Vector3(0, playerHeight, 0);
        Debug.DrawRay(playerHead, transform.forward, Color.green, vaultLayer);
        
        }

        private void PlayerMovement()
        {
            
            
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
            
        }

        private void PlayerMovementWithoutUpdate()
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticallInput = Input.GetAxis("Vertical");
            Vector3 camForward = CameraReferenceTransform.forward;
            Vector3 deplacement = verticallInput * camForward + horizontalInput * CameraReferenceTransform.right;
            deplacement.y = 0; // pour pas qu'il rentre dans le sol si la caméra est vu plonger
            
            Vector3 deplacementFinal =
                Vector3.ClampMagnitude(deplacement, 1) *
                moveSpeed; // force pas le comme avec le normalized mais fixe le max à 1
            rbComponent.velocity =
                new Vector3(deplacementFinal.x, rbComponent.velocity.y,
                    deplacementFinal.z); // le rbComponent.velocity.y est utiliser pour permettre de sauter ou de tomber

            if (deplacement != Vector3.zero && currentPLayerStateCollider != PlayerStateCollider.Attack)
            {
                rbComponent.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(deplacement),
                    Time.deltaTime * rotationSpeed); // Slerp apprament mieux que Lerp dans ce cas là car c'est une interpolation sphérique
            }

            if (horizontalInput != 0 || verticallInput != 0) animatorComponent.SetBool("IsWalking", true);
            else animatorComponent.SetBool("IsWalking", false);
        }
         
        private void FixedUpdate()
        {
            PlayerCollision();
            PlayerMovementWithoutUpdate();
        }

        private void PlayerCollision()
        {
            switch (currentPLayerStateCollider)
            {
                case PlayerStateCollider.Normal when standPlayer.enabled == false:
                    standPlayer.enabled = true;
                    crouchPlayer.enabled = false;
                    crawlPlayer.enabled = false;
                    break;
                case PlayerStateCollider.Crouching when crouchPlayer.enabled == false:
                    crouchPlayer.enabled = true;
                    standPlayer.enabled = false;
                    crawlPlayer.enabled = false;
                    break;
                case PlayerStateCollider.Crawling when crawlPlayer.enabled == false:
                    crawlPlayer.enabled = true;
                    crouchPlayer.enabled = false;
                    standPlayer.enabled = false;
                    break;
                case PlayerStateCollider.Attack:
                case PlayerStateCollider.Climbing:
                    break;
            }
        }
        
        
        IEnumerator FightStateChanger()
        {
            currentPLayerStateCollider = PlayerStateCollider.Attack;
            rbComponent.constraints = RigidbodyConstraints.FreezeAll;
            yield return new WaitForSeconds(2);
            currentPLayerStateCollider = PlayerStateCollider.Normal;
            rbComponent.constraints = RigidbodyConstraints.None;
            rbComponent.constraints = RigidbodyConstraints.FreezeRotationX |
                                      RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("killZone")) isInKillZone = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("killZone")) isInKillZone = false;
        }

        private void Vault()
        {
            if (!Input.GetKeyDown(KeyCode.Space)) return;
            Vector3 playerHead = gameObject.transform.position + new Vector3(0, playerHeight, 0);
            if (!Physics.Raycast(playerHead, transform.forward, out var firstHit, 1f, vaultLayer)) return;
            Debug.Log("Here is a wall");
            if (Physics.Raycast(firstHit.point + (transform.forward * playerRadius) +
                                (Vector3.up * 0.6f * playerHeight), Vector3.down,
                    out var secondHit,playerHeight))
            {
                print("Found place to land");
                StartCoroutine(VaultAnimation(secondHit.point, 2));
            }
        }

        private IEnumerator VaultAnimation(Vector3 targetPosition, float duration)
        {
            currentPLayerStateCollider = PlayerStateCollider.Climbing;
            animatorComponent.Play("Climbing");
            rbComponent.useGravity = false;

           /* while (time < duration)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
                time += Time.deltaTime;
                yield return null;
            }*/
           yield return new WaitForSeconds(animationClimb.length);
           animatorComponent.Play("Standing");
            transform.position = targetPosition;
            rbComponent.useGravity = true;
            currentPLayerStateCollider = PlayerStateCollider.Normal;
            
            
        }
        
        
    }
}