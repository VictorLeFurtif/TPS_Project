using System;
using UnityEngine;

namespace Script.Interact
{
    [RequireComponent(typeof(BoxCollider))]
    public class SceneManager : MonoBehaviour
    {
        [Header("NAME OF THE SCENE TO LOAD")][Tooltip("DONT FORGET TO PUT IT INTO THE BUILD SETTINGS")]
        [SerializeField] private string sceneName;

        [Header("LAYER")] [SerializeField] private LayerMask playerLayer;
        
        
        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.layer != playerLayer)
            {
                Debug.Log("Successful Loading");
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning("The Fuck");
            }
            
        }

        public void LoadSceneByIndex(int index)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(index);
        }
    }
}