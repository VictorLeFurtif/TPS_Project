using System.Collections;
using Script.Enemy.new_Enemy_system;
using UnityEngine;

namespace Script.Player
{
    [System.Serializable]
    public class MovementSettings
    {
        public float walkingSpeed = 5f;
        public float runningSpeed = 7f;
        public float crawlingSpeed = 2f;
        public float attackingSpeed = 0f;
        public float rotationSpeed = 1f;
        public float gravityMultiplier = 9.81f;
        public float currentSpeed;
    }

    [System.Serializable]
    public class CombatSettings
    {
        public float killDistance = 1f;
        public LayerMask enemyLayerMask;
    }

    [System.Serializable]
    public class ColliderSettings
    {
        public CapsuleCollider standCollider;
        public CapsuleCollider crouchCollider;
        public CapsuleCollider crawlCollider;
    }

    public class PlayerControl : MonoBehaviour
    {
        [SerializeField] private int vaultLayer;
        public MovementSettings movementSettings;
        [SerializeField] private CombatSettings combatSettings;
        [SerializeField] private ColliderSettings colliderSettings;
        
        [Header("REFERENCES")]
        [SerializeField] private Transform cameraReferenceTransform;
        [SerializeField] private AnimationClip animationClimb;

        [Header("STATE")]
        public PlayerStateCollider currentState = PlayerStateCollider.Normal;
        public bool isInKillZone = false;
        
        public Animator animatorComponent;
        private Rigidbody rbComponent;
        private float playerHeight;
        private float playerRadius;
        private bool isDead = false;
        
        public static PlayerControl Instance { get; private set; }

        public enum PlayerStateCollider
        {
            Normal,
            Crouching,
            Crawling,
            Attack,
            Climbing,
            Dead
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) 
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            InitializeComponents();
            InitializePhysics();
            InitializeCursor();
            InitializeSpeed();
        }

        #region INITIALIZE

        private void InitializeComponents()
        {
            animatorComponent = GetComponent<Animator>();
            if (animatorComponent == null)
                Debug.LogWarning($"No Animator detected on {gameObject.name}");

            rbComponent = GetComponent<Rigidbody>();
            if (rbComponent == null)
                Debug.LogWarning($"No Rigidbody detected on {gameObject.name}");

            playerHeight = colliderSettings.standCollider.height;
            playerRadius = colliderSettings.standCollider.radius;
        }

        private void InitializePhysics()
        {
            Physics.gravity = new Vector3(0, -movementSettings.gravityMultiplier, 0);
            currentState = PlayerStateCollider.Normal;
        }

        private void InitializeCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        private void InitializeSpeed()
        {
            movementSettings.currentSpeed = movementSettings.walkingSpeed;
        }

        #endregion
        
        
        private void Update()
        {
            if (currentState == PlayerStateCollider.Dead )
            {
                HandleDeath();
                return;
            }

            HandleInput();
        }

        private void FixedUpdate()
        {
            UpdateCollider();
            CalculateMovement();
        }

        private void HandleInput()
        {
            if (currentState == PlayerStateCollider.Climbing)
            {
                movementSettings.currentSpeed = movementSettings.attackingSpeed; // take attacking because velocity will be 0
                return;
            }
            
            HandleMovementInput();
            HandleCombatInput();
            HandleVaultInput();
        }

        #region Movement System
        private void HandleMovementInput()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) && currentState == PlayerStateCollider.Normal)
                SetRunning(true);
            else if (Input.GetKeyUp(KeyCode.LeftShift))
                SetRunning(false);

            if (Input.GetKeyDown(KeyCode.LeftControl))
                SetCrouching(true);
            else if (Input.GetKeyUp(KeyCode.LeftControl))
                SetCrouching(false);

            if (Input.GetKeyDown(KeyCode.C))
                SetCrawling(true);
            else if (Input.GetKeyUp(KeyCode.C))
                SetCrawling(false);
        }

        private void SetRunning(bool isRunning)
        {
            movementSettings.currentSpeed = isRunning ? movementSettings.runningSpeed : movementSettings.walkingSpeed;
            animatorComponent.SetBool("IsRunning", isRunning);
        }

        private void SetCrouching(bool isCrouching)
        {
            movementSettings.currentSpeed = movementSettings.walkingSpeed;
            animatorComponent.SetBool("IsCrouching", isCrouching);
            currentState = isCrouching ? PlayerStateCollider.Crouching : PlayerStateCollider.Normal;
        }

        private void SetCrawling(bool isCrawling)
        {
            movementSettings.currentSpeed = isCrawling ? movementSettings.crawlingSpeed : movementSettings.walkingSpeed;
            animatorComponent.SetBool("IsCrawling", isCrawling);
            currentState = isCrawling ? PlayerStateCollider.Crawling : PlayerStateCollider.Normal;
        }

        private void CalculateMovement()
        {
            if (isDead) return;

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            Vector3 movement = CalculateMovementDirection(horizontal, vertical);
            ApplyMovement(movement);
            UpdateRotation(movement);
            UpdateAnimator(horizontal, vertical);
        }

        private Vector3 CalculateMovementDirection(float horizontal, float vertical)
        {
            Vector3 camForward = cameraReferenceTransform.forward;
            Vector3 movement = vertical * camForward + horizontal * cameraReferenceTransform.right;
            movement.y = 0;
            return Vector3.ClampMagnitude(movement, 1) * movementSettings.currentSpeed;
        }

        private void ApplyMovement(Vector3 movement)
        {
            rbComponent.velocity = new Vector3(movement.x, rbComponent.velocity.y, movement.z);
        }

        private void UpdateRotation(Vector3 movement)
        {
            if (movement != Vector3.zero && currentState != PlayerStateCollider.Attack)
            {
                rbComponent.rotation = Quaternion.Slerp(
                    transform.rotation, 
                    Quaternion.LookRotation(movement),
                    Time.deltaTime * movementSettings.rotationSpeed
                );
            }
        }

        private void UpdateAnimator(float horizontal, float vertical)
        {
            bool isWalking = (horizontal != 0 || vertical != 0);
            animatorComponent.SetBool("IsWalking", isWalking);
        }
        #endregion

        #region Combat System
        private void HandleCombatInput()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                TryAttack();
            }
        }

        private void TryAttack()
        {
            Vector3 attackOrigin = transform.position + new Vector3(0, playerHeight/2, 0);
            Debug.DrawRay(attackOrigin, transform.forward * combatSettings.killDistance, Color.red, 2f);
            
            if (Physics.Raycast(attackOrigin, transform.forward, out var hit, combatSettings.killDistance, combatSettings.enemyLayerMask))
            {
                StartAttack(hit);
            }
        }

        private void StartAttack(RaycastHit hit)
        {
            movementSettings.currentSpeed = movementSettings.attackingSpeed;
            animatorComponent.SetTrigger("IsStabbing");
            StartCoroutine(AttackSequence());
            
            EnemyStatic enemy = hit.collider.GetComponent<EnemyStatic>();
            enemy?.Kill();
        }

        private IEnumerator AttackSequence()
        {
            currentState = PlayerStateCollider.Attack;
            rbComponent.constraints = RigidbodyConstraints.FreezeAll;
            
            yield return new WaitForSeconds(2);
            
            currentState = PlayerStateCollider.Normal;
            rbComponent.constraints = RigidbodyConstraints.FreezeRotation;
        }
        #endregion

        #region Vault System
        private void HandleVaultInput()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TryVault();
            }
        }

        private void TryVault()
        {
            rbComponent.velocity = Vector3.zero;
            Vector3 vaultCheckOrigin = transform.position + new Vector3(0, playerHeight, 0);
            
            if (Physics.Raycast(vaultCheckOrigin, transform.forward, out var firstHit, 1f) && 
                firstHit.transform.gameObject.layer == vaultLayer)
            {
                Vector3 secondRayOrigin = firstHit.point + 
                                        (transform.forward * playerRadius) + 
                                        (Vector3.up * 0.6f * playerHeight);
                                        
                if (Physics.Raycast(secondRayOrigin, Vector3.down, out var secondHit, playerHeight))
                {
                    StartCoroutine(PerformVault(secondHit.point));
                }
            }
        }

        private IEnumerator PerformVault(Vector3 targetPosition)
        {
            currentState = PlayerStateCollider.Climbing;
            animatorComponent.Play("Climbing");
            rbComponent.useGravity = false;
            
            yield return new WaitForSeconds(animationClimb.length);
            
            animatorComponent.Play("Standing");
            transform.position = targetPosition;
            rbComponent.useGravity = true;
            currentState = PlayerStateCollider.Normal;
            movementSettings.currentSpeed = movementSettings.walkingSpeed;
        }
        #endregion

        #region Collider System
        private void UpdateCollider()
        {
            colliderSettings.standCollider.enabled =
                currentState is PlayerStateCollider.Normal or PlayerStateCollider.Dead;
            colliderSettings.crouchCollider.enabled = currentState == PlayerStateCollider.Crouching;
            colliderSettings.crawlCollider.enabled = currentState == PlayerStateCollider.Crawling;
        }
        #endregion

        #region Death System
        private void HandleDeath()
        {
            if (isDead) return;
            isDead = true;
            animatorComponent.Play("Falling Forward Death");
        }
        #endregion
    }
}