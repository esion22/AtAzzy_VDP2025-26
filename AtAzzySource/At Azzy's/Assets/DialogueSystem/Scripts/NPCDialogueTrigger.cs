using UnityEngine;
using UnityEngine.Events;
using DS.ScriptableObjects;

public class NPCDialogueTrigger : MonoBehaviour
{
    public UnityEvent[] OnChoiceEvents;
    public DSDialogueContainerSO dialogueContainer;
    public DSDialogueSO startDialogue;

    private DSDialogueRunner runner;
    private bool playerInRange = false;
    private bool isActiveNPC = false;
    public Sprite npcPortrait; 

    /*
     * Unity Start callback.
     * Finds the dialogue runner and subscribes to the choice event.
     */
    private void Start()
    {
        runner = FindObjectOfType<DSDialogueRunner>();
        runner.OnChoiceSelectedEvent += HandleChoice;
    }

    /*
     * Handles choice events from the dialogue runner.
     * @param index Index of the selected choice.
     * Processes only if this NPC is currently active.
     */
    private void HandleChoice(int index)
    {
        if (!isActiveNPC) return;

        if (index >= 0 && index < OnChoiceEvents.Length)
            OnChoiceEvents[index]?.Invoke();
        
        isActiveNPC = false;
    }

    /*
     * Detects when the player enters the trigger.
     * @param other The collider that triggered the event.
     */
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    /*
     * Detects when the player exits the trigger.
     * @param other The collider that triggered the event.
     */
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    /*
     * Unity Update callback.
     * Checks for interaction input and starts dialogue if possible.
     */
    private void Update()
    {
        if (playerInRange && Input.GetButtonDown("DialogueInteraction"))
        {
            isActiveNPC = true;
            runner.dialogueContainer = dialogueContainer;
            runner.startDialogue = startDialogue;
            runner.SetPortrait(npcPortrait);  
            runner.StartConversation();
        }
    }

    /*
     * Cleans up event subscriptions when this object is destroyed.
     */
    private void OnDestroy()
    {
        if (runner != null)
            runner.OnChoiceSelectedEvent -= HandleChoice;
    }
}