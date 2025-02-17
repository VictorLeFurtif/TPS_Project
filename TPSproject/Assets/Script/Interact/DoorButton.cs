using UnityEngine;

public class DoorButton : MonoBehaviour
{
    [SerializeField] private GameObject door;
    private bool doorIsOpen = false;
    private Vector3 startingPosition;
    private Vector3 openPosition;
    private readonly float animationSpeed = 2f;  
    private float lerpTime = 0f;        
    private bool isAnimating = false;   

    private void Start()
    {
        startingPosition = door.transform.position;
        openPosition = startingPosition + new Vector3(0, 4, 0);
    }

    private void Update()
    {
        ToggleDoor();
    }

    private void ToggleDoor()
    {
        if (!isAnimating) return;
        lerpTime += Time.deltaTime * animationSpeed;
        if (lerpTime > 1f)
        {
            lerpTime = 1f;
            isAnimating = false;
        }
        
        door.transform.position = Vector3.Lerp(
            doorIsOpen ? startingPosition : openPosition, 
            doorIsOpen ? openPosition : startingPosition, 
            lerpTime
        );
    }

    private void OnTriggerStay(Collider other)
    {
        if (!Input.GetKeyDown(KeyCode.F) || !other.CompareTag("Player")) return;
        doorIsOpen = !doorIsOpen;
        lerpTime = 0f;     
        isAnimating = true;
    }
}