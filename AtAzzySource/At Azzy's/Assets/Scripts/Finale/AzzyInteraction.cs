using DS;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Finale
{
    public class AzzyInteraction : MonoBehaviour
    {
        public bool isDragon { get; set; }
        public DS.ScriptableObjects.DSDialogueContainerSO humanDialogueContainer { get; set; }
        public DS.ScriptableObjects.DSDialogueContainerSO dragonDialogueContainer { get; set; }
        public DS.ScriptableObjects.DSDialogueSO farmFinaleDialogue { get; set; }
        public DS.ScriptableObjects.DSDialogueSO[] humanDialogues { get; set; }
        public DS.ScriptableObjects.DSDialogueSO[] dragonDialogues { get; set; }

        private GameObject _highlight;
        private bool _playerInRange;
        private AzzyDialogueTrigger _dialogueTrigger;
        private int _lastDialogueIndex = -1;

        private const string FarmFinaleKey = "FarmFinaleCompleted";
        private const string CookingFinaleKey = "CookingFinaleCompleted";

        private void Awake()
        {
            // Find and deactivate the Highlight child object
            _highlight = transform.Find("Highlight")?.gameObject;
            if (_highlight != null)
            {
                _highlight.SetActive(false);
            }

            // Add or find the NPCDialogueTrigger component
            _dialogueTrigger = gameObject.GetComponent<AzzyDialogueTrigger>();
            if (_dialogueTrigger == null)
            {
                _dialogueTrigger = gameObject.AddComponent<AzzyDialogueTrigger>();
            }
        }
        
        private void Start()
        {
            UpdateDialogue();
        }

        private void LateUpdate()
        {
            UpdateDialogue();
        }

        private void UpdateDialogue()
        {
            // Check the state of the finales
            bool farmFinaleCompleted = PlayerPrefs.GetInt(FarmFinaleKey, 0) == 1;
            bool cookingFinaleCompleted = PlayerPrefs.GetInt(CookingFinaleKey, 0) == 1;

            if (farmFinaleCompleted && !cookingFinaleCompleted)
            {
                // Set specific dialogue for farm finale completed but cooking finale not completed
                if ((isDragon && dragonDialogueContainer != null) || (!isDragon && humanDialogueContainer != null))
                {
                    _dialogueTrigger.dialogueContainer = isDragon ? dragonDialogueContainer : humanDialogueContainer; 
                }
                _dialogueTrigger.startDialogue = farmFinaleDialogue;
            }
            else
            {
                // Randomly select a dialogue
                var dialogues = isDragon ? dragonDialogues : humanDialogues;
                if (dialogues != null && dialogues.Length > 0)
                {
                    if ((isDragon && dragonDialogueContainer != null) || (!isDragon && humanDialogueContainer != null))
                    {
                        _dialogueTrigger.dialogueContainer = isDragon ? dragonDialogueContainer : humanDialogueContainer; 
                    }
                    int newIndex;
                    do
                    {
                        newIndex = Random.Range(0, dialogues.Length);
                    } while (dialogues.Length > 1 && newIndex == _lastDialogueIndex);

                    _lastDialogueIndex = newIndex;
                    _dialogueTrigger.startDialogue = dialogues[newIndex];
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && !other.gameObject.name.StartsWith("Customer"))
            {
                _playerInRange = true;
                if (_highlight != null)
                {
                    _highlight.SetActive(true);
                }
                
                // Update GameFinaleCookingManager's _playerInRange
                GameFinaleCookingManager.Instance?.SetPlayerInRange(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") && !other.gameObject.name.StartsWith("Customer"))
            {
                _playerInRange = false;
                if (_highlight != null)
                {
                    _highlight.SetActive(false);
                }
                
                // Update GameFinaleCookingManager's _playerInRange
                GameFinaleCookingManager.Instance?.SetPlayerInRange(false);
            }
        }
    }
}
