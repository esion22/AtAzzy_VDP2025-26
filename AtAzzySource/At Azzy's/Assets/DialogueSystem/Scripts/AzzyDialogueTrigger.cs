using UnityEngine;
using UnityEngine.Events;
using DS.ScriptableObjects;
using Finale;

namespace DS
{
    public class AzzyDialogueTrigger : MonoBehaviour
    {
        public UnityEvent[] OnChoiceEvents;
        public DSDialogueContainerSO dialogueContainer;
        public DSDialogueSO startDialogue;

        private DSDialogueRunner runner;
        private bool playerInRange = false;
        private bool isActiveAzzy = false;

        private AzzyScript _azzyScript;
        private void Start()
        {
            runner = FindObjectOfType<DSDialogueRunner>();
            _azzyScript = GameObject.Find("AzzyManager").GetComponent<AzzyScript>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
            }
        }

        private void Update()
        {
            if (playerInRange && Input.GetButtonDown("DialogueInteraction"))
            {
                if (runner.IsDialogueRunning || Time.timeScale == 0.0f)
                    return;
                
                isActiveAzzy = true;
                runner.dialogueContainer = dialogueContainer;
                runner.startDialogue = startDialogue;
                runner.SetPortrait(_azzyScript.GetCurrentSprite()); 
                runner.StartConversation();
            }
        }
    }
}