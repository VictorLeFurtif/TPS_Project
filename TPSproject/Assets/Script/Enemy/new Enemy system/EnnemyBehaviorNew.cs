using System;
using System.Collections;
using System.Collections.Generic;
using Script;
using UnityEngine;
using UnityEngine.AI;


public class EnnemyBehaviorNew : MonoBehaviour
{
    private NavMeshAgent agent;
    [SerializeField] private List<GameObject> listOfWaypoints;
    [SerializeField] private IaState currentIaState = IaState.Wander;
    private BoxCollider zoneOfDetection;
   [SerializeField] private bool playerInZone = false;
    [SerializeField] private bool attacking = false;

    enum IaState
    {
        Wander,
        InSearch,
        Attack,
    }
    
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        zoneOfDetection = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        IaBehavior();
    }

    private void IaBehavior()
    {
        switch (currentIaState)
        {
            case IaState.Wander : GoToWayPoints();
                break;
            case IaState.InSearch : TryToAttackPlayer();
                break;
        }
        
    }
    
    private void GoToWayPoints()
    {
        Vector3 withoutY = new Vector3(1, 0, 1);
        if (listOfWaypoints.Count == 0) return;
        Vector3 targetElement = listOfWaypoints[0].transform.position;
        if (Vector3.Distance(Vector3.Scale(targetElement,withoutY),Vector3.Scale(agent.transform.position,withoutY)) < 0.1f)
        {
            GameObject oldPosition = listOfWaypoints[0];
           listOfWaypoints.RemoveAt(0);
           listOfWaypoints.Add(oldPosition);
           
        }
        if (listOfWaypoints.Count != 0) agent.destination = targetElement;
    }

    private void TryToAttackPlayer()
    {
        if (playerInZone)
        {
            agent.destination = PlayerControl.INSTANCE.gameObject.transform.position;
        }
        else
        {
            currentIaState = IaState.Wander;
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && currentIaState == IaState.Wander && (PlayerControl.INSTANCE.currentPLayerStateCollider != 
            PlayerControl.PlayerStateCollider.Crouching || PlayerControl.INSTANCE.currentPLayerStateCollider 
            != PlayerControl.PlayerStateCollider.Crawling))
        {
            currentIaState = IaState.InSearch;
            playerInZone = true;
        }
        else
        {
            playerInZone = false;
        }
    }

    private void OnCollisionStay(Collision other)
    {
        if (attacking != false || !other.gameObject.CompareTag("Player")) return;
        attacking = true;
        StartCoroutine(AttackThePlayer(2));
    }

    IEnumerator AttackThePlayer(float time)
    {
        yield return new WaitForSeconds(time);
        Debug.Log("Attack");
        attacking = false;
    }
}
