using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorButton : MonoBehaviour
{
    [SerializeField] private GameObject door;
    private BoxCollider colliderInteract;
    private bool doorIsOpen = false;
    private Vector3 startingPosition;
    private Vector3 openPosition;
    

    private void Start()
    {
        colliderInteract = GetComponent<BoxCollider>();
        startingPosition = door.transform.position;
        openPosition = startingPosition + new Vector3(0,4, 0);
    }

    private void Update()
    {
        ToggleDoor();
    }

    private void ToggleDoor()
    {
        if ( doorIsOpen)
        {
            door.transform.position = Vector3.Lerp(startingPosition, openPosition, Time.deltaTime * 4);
        }
        else
        {
            door.transform.position=Vector3.Lerp(openPosition,startingPosition , Time.deltaTime*4);
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (Input.GetKeyDown(KeyCode.F) && other.CompareTag("Player"))
        {
            doorIsOpen = !doorIsOpen;
        }
        
    }
}
