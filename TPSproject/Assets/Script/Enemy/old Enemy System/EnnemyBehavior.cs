using UnityEngine;
using UnityEngine.AI;

namespace Script
{
    public class EnnemyBehavior : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private NavMeshAgent ennemy;
    
    
        void Start()
        {
            ennemy = GetComponent<NavMeshAgent>();
        }
    
        void Update()
        {
            ennemy.SetDestination(player.position);
        }
    }
}
