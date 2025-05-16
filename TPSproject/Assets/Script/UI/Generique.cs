using System;
using Script.Interact;
using UnityEngine;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Script.UI
{
    public class Generique : MonoBehaviour
    {
        [SerializeField] private float moveSpeedText;
        [SerializeField] private int menuSceneIndex;
        
        private void Update()
        {
            HandleGeneriqueSliding();
            HandleInput();
        }

        private void HandleInput()
        {
            HandleGoingBackToMenu();
        }

        private void HandleGoingBackToMenu()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene(menuSceneIndex); 
            }
        }
        
        private void HandleGeneriqueSliding()
        {
            transform.position = new Vector3(transform.position.x
                ,transform.position.y + moveSpeedText
                ,transform.position.z);
        }
    }
}
