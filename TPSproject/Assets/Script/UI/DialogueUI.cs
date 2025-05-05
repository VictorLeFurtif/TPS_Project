using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Script.UI
{
    public class DialogueUI : MonoBehaviour
    {
        [TextArea]
        public string tutorialMessage;

        [SerializeField] private GameObject canvaTuto;
        
        [SerializeField] private TextMeshProUGUI tutorialText;


        private void Start()
        {
            HideTutorial();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            ShowTutorial(tutorialMessage);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player") ) return;
            HideTutorial();
        }


        private void ShowTutorial(string message)
        {
            canvaTuto.SetActive(true);
            tutorialText.text = message;
        }

        public void HideTutorial()
        {
            canvaTuto.SetActive(false);
            tutorialText.text = "";
        }
    }
}