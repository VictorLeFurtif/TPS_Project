using System;
using Script.Player;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


namespace Script.Enemy.new_Enemy_system
{
    public class EnnemyBehaviorNew : MonoBehaviour
    {
        [Header("LayerMask")]
        [SerializeField] private LayerMask whatIsPlayer;
        [SerializeField] private LayerMask whatIsGround;
    
        private NavMeshAgent agent;
    
        [Header("Ia State")]
        public IaState currentIaState = IaState.Patrol;
        private bool searching = false;
    
        [Header("Range Float")]
        [SerializeField] private float sightRange;
        [SerializeField] private float fightRange;
        private Vector3 walkPoint;
        private bool walkPointSet;
        [SerializeField] private float walkPointRange;
    
        [Header("Attack Parameters")]
        private bool alreadyAttacked = false;
        [SerializeField] private float timeBetweenAttacks;
    
        [Header("Raycast Parameters")]
        [SerializeField] private float rayLength = 20f; 
        [SerializeField] private Color rayColor = Color.red; 
        [SerializeField] private Color rayColorNoObstacle = Color.green;
        private bool isPlayerTouched = false;
    
        [Header("Animator Parameters")]
        public Animator animator;
    
        [Header("Sight Vue")]
        private BoxCollider sightVue;

        private Vector3 targetPosition = Vector3.zero;

        [Header("SPEED")] 
        [SerializeField] private float moveSpeed;

        [Header("Origin Position")] [SerializeField]
        private Vector3 originalPosition;

        public enum IaState
        {
            Patrol,
            ChasePlayer,
            Attack,
            Search,
            Dead,
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, sightRange);
        }

        private void Update()
        {
            IaBehaviour();
        }

        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            sightVue = GetComponentInChildren<BoxCollider>();
            agent.speed = moveSpeed;
            originalPosition = transform.position;
        }

        private bool CheckIfPlayerInSightEnemy()
        {
            return Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        }

        private bool CheckIfPlayerInFightZone()
        {
            return Physics.CheckSphere(transform.position, fightRange, whatIsPlayer);
        }

        private void IaBehaviour() // oublie pas quand le player est accroupi ou qu'il crawl
        {
            if (currentIaState == IaState.Dead)
            {
                return;
            }
            RayCheckObstacle();
            if (!CheckIfPlayerInSightEnemy() && !CheckIfPlayerInFightZone())
                currentIaState = IaState.Patrol;
        
            else if (CheckIfPlayerInSightEnemy() && !CheckIfPlayerInFightZone() && 
                     (PlayerControl.INSTANCE.currentPLayerStateCollider != PlayerControl.PlayerStateCollider.Crawling &&
                      PlayerControl.INSTANCE.currentPLayerStateCollider != PlayerControl.PlayerStateCollider.Crouching))
                currentIaState = IaState.ChasePlayer;
        
            else if (CheckIfPlayerInSightEnemy() && CheckIfPlayerInFightZone() && 
                     (PlayerControl.INSTANCE.currentPLayerStateCollider != PlayerControl.PlayerStateCollider.Crawling &&
                      PlayerControl.INSTANCE.currentPLayerStateCollider != PlayerControl.PlayerStateCollider.Crouching))
                currentIaState = IaState.Attack;
        
            else if (currentIaState == IaState.Search)
            {
                currentIaState = IaState.Patrol;
            }

            if (CheckIfPlayerInSightEnemy() && CheckIfPlayerInFightZone() && PlayerControl.INSTANCE.currentPLayerStateCollider == PlayerControl.PlayerStateCollider.Attack)
            {
                currentIaState = IaState.Dead;
                animator.Play("Death animation");
                return;
            }
        
            if (currentIaState == IaState.ChasePlayer)
            {
                if (!isPlayerTouched && targetPosition==Vector3.zero)
                {
                    currentIaState = IaState.Search;
                    targetPosition = PlayerControl.INSTANCE.transform.position;
                    agent.SetDestination(targetPosition);
                }
                else if (!isPlayerTouched)
                {
                    currentIaState = IaState.Search;
                }
                else
                {
                    targetPosition = Vector3.zero;
                }
            }
        
        

            switch (currentIaState)
            {
                case IaState.Patrol:
                    PatrolBehaviour();
                    animator.SetBool("Run",false);
                    break;
                case IaState.ChasePlayer:
                    ChasePlayerBehaviour();
                    animator.SetBool("Run", true);
                    break;
                case IaState.Attack:
                    AttackBehavior();
                    break;
                case IaState.Search: 
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PatrolBehaviour()
        {
            if (!walkPointSet) SearchWalkPoint();
            if (walkPointSet) agent.SetDestination(walkPoint);
    
            Vector3 distanceToWalkPoint = transform.position - walkPoint;

            if (distanceToWalkPoint.magnitude < 1f || agent.remainingDistance < 0.5f)
            {
                walkPointSet = false;
            }
        }

        private void SearchWalkPoint()
        {
            float zRandom = Random.Range(-walkPointRange, walkPointRange);
            float xRandom = Random.Range(-walkPointRange, walkPointRange);
        
            Vector3 newWalkPoint = new Vector3(transform.position.x + xRandom, transform.position.y, transform.position.z + zRandom);

            NavMeshHit hit;
            if (!NavMesh.SamplePosition(newWalkPoint, out hit, 2f, NavMesh.AllAreas)) return;
            walkPoint = hit.position;
            walkPointSet = true;
        }

        private void ChasePlayerBehaviour()
        {
            agent.SetDestination(PlayerControl.INSTANCE.transform.position);
        }
    

        private void AttackBehavior()
        {
            agent.SetDestination(transform.position); //faut lock l'ennemi
            transform.LookAt(PlayerControl.INSTANCE.transform); // Le lookAt me permet quand y'aura les anims

            if (alreadyAttacked) return;
            // futur code pour attack
            animator.Play("Zombie Punching");
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }

        private void ResetAttack()
        {
            alreadyAttacked = false;
        }
    
        private void RayCheckObstacle()
        {
            Vector3 directionToPlayer = (PlayerControl.INSTANCE.transform.position - transform.position).normalized;
            Ray ray = new Ray(transform.position, directionToPlayer);
            RaycastHit hit;
        
            if (Physics.Raycast(ray, out hit, rayLength))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    Debug.DrawRay(transform.position, directionToPlayer * rayLength, rayColorNoObstacle);
                    isPlayerTouched = true;
                }
                else
                {
                    Debug.DrawRay(transform.position, directionToPlayer * hit.distance, rayColor);
                    isPlayerTouched = false;
                }
            }
            else Debug.DrawRay(transform.position, directionToPlayer * rayLength, rayColorNoObstacle);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && CheckIfPlayerInSightEnemy())
            {
                currentIaState = IaState.ChasePlayer;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                currentIaState = IaState.Patrol;
            }
        }
    }
}
