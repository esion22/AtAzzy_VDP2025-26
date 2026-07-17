using UnityEngine;
using UnityEngine.UI;
using System;

namespace Tutorial
{
    public class ShopTutorial : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image dragonPortrait;
        [SerializeField] private DSDialogueUI dialogueUI;

        [Header("Arrow")]
        [SerializeField] private ArrowIndicator arrowIndicator;


        [Header("Shop")]
        [SerializeField] private GameObject shopPanel;

        [Header("Shopkeeper")]
        [SerializeField] private Transform salesman;
        [SerializeField] private BoxCollider2D salesmanCollider;

        private const string TUTORIAL_COMPLETE_KEY = "ShopTutorialDone";

        public static ShopTutorial Instance { get; private set; }
        public bool IsActive { get; private set; }

        private int _currentStep;
        private bool _dialogueVisible;
        private int _pendingNextStep = -1;

        private Func<bool> _waitCondition;
        private int _waitNextStep = -1;


        private InventorySnapshot _inventoryBeforeBuy;

        private CanvasGroup _dialogueCanvasGroup;
        private string _lastDialogueText = "";

        /*
         * Initializes the singleton instance of the ShopTutorial.
         * Ensures global access to this tutorial controller.
         *
         * @return void
         */
        private void Awake()
        {
            Instance = this;
        }

        /*
         * Validates whether prerequisite tutorials are completed.
         * Disables this object if the shop tutorial should not run.
         *
         * @return void
         */
        private void Start()
        {
            bool cookingDone = PlayerPrefs.GetInt("CookingTutorialDone", 0) == 1;
            bool farmingDone = PlayerPrefs.GetInt("FarmTutorialDone", 0) == 1;
            bool shopDone = PlayerPrefs.GetInt(TUTORIAL_COMPLETE_KEY, 0) == 1;

            if (!cookingDone || !farmingDone || shopDone)
            {
                gameObject.SetActive(false);
                return;
            }

            StartCoroutine(StartTutorialAfterRunnerStart());
        }

        /*
         * Delays tutorial initialization by one frame to ensure
         * dialogue systems and dependencies are fully ready.
         *
         * @return IEnumerator used for coroutine execution.
         */
        private System.Collections.IEnumerator StartTutorialAfterRunnerStart()
        {
            yield return null;

            InitDialogueCanvasGroup();
            StartTutorial();
        }

        /*
         * Handles tutorial input, evaluates waiting conditions,
         * and manages dialogue visibility every frame while active.
         *
         * @return void
         */
        private void Update()
        {
            if (!IsActive)
                return;

            if (_dialogueVisible &&
                (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
            {
                HideDialogue();

                if (_pendingNextStep >= 0)
                {
                    int next = _pendingNextStep;
                    _pendingNextStep = -1;
                    ShowStep(next);
                }
            }

            if (_waitCondition != null && _waitCondition())
            {
                int next = _waitNextStep;
                ClearWait();

                arrowIndicator?.ClearTarget();

                if (next >= 0)
                    ShowStep(next);
            }

            AutoUnhideForNpcDialogue();
        }

        /*
         * Begins the shop tutorial sequence.
         * Prepares UI elements, locks shopkeeper interaction,
         * and starts the tutorial flow.
         *
         * @return void
         */
        private void StartTutorial()
        {
            IsActive = true;

            SetSalesmanInteractable(false);

            ShowStep(10);
        }

        /*
         * Advances the tutorial to the specified step
         * and executes the corresponding dialogue and gameplay logic.
         *
         * @param step The tutorial step index to execute.
         * @return void
         */
        private void ShowStep(int step)
        {
            _currentStep = step;

            arrowIndicator?.ClearTarget();
            switch (step)
            {
                case 10:
                    ShowDialogue("Hope you slept well fellow magician! You proved youself a great farmer and cook!", 11);
                    break;

                case 11:
                    ShowDialogue("Unfortunately, as you have seen, not all ingredients can be found on the farm or in the wild.", 12);
                    break;

                case 12:
                    ShowDialogue("Luckily, there is a shop in town East from us where you can buy ingredients and other useful items.", 13);
                    break;

                case 13:
                    ShowDialogue("There you can buy items using the gold you earned from selling your meals.", 14);
                    break;

                case 14:
                    ShowDialogue("Maybe we will find an ingredient that can break the spell that confines me to this dragon form!", 1);
                    break;
                
                case 1:
                    ShowDialogue("Go talk to the shopkeeper.");

                    SetSalesmanInteractable(true);

                    if (arrowIndicator != null && salesman != null)
                        arrowIndicator.SetTarget(salesman);

                    WaitUntil(IsShopPanelOpen, 2);
                    break;

                case 2:
                    _inventoryBeforeBuy = new InventorySnapshot(GameManager.instance.playerInventory);
                    ShowDialogue("Buy any item you like.");
                    WaitUntil(HasBoughtItem, 4);
                    break;

                case 4:
                    ShowDialogue("Great purchase!", 5);
                    break;

                case 5:
                    ShowDialogue("This is the end of the tutorial, have fun!", 6);
                    break;

                case 6:
                    CompleteTutorial();
                    break;
            }
        }

        /*
         * Determines whether the shop panel is currently open.
         *
         * @return True if the shop UI is active; otherwise false.
         */
        private bool IsShopPanelOpen()
        {
            return shopPanel != null && shopPanel.activeInHierarchy;
        }

        /*
         * Checks whether the player has purchased an item by comparing
         * the current inventory against a previous snapshot.
         *
         * @return True if the item count increased; otherwise false.
         */
        private bool HasBoughtItem()
        {
            Inventory inventory = GameManager.instance.playerInventory;
            InventorySnapshot now = new InventorySnapshot(inventory);
            return now.totalItemCount > _inventoryBeforeBuy.totalItemCount;
        }

        /*
         * Displays tutorial dialogue and optionally schedules
         * the next tutorial step after dismissal.
         *
         * @param text The dialogue text shown to the player.
         * @param nextStepOnDismiss The step triggered after the dialogue closes.
         * @return void
         */
        private void ShowDialogue(string text, int nextStepOnDismiss = -1)
        {
            ForceStopRunnerDialogue();

            _dialogueVisible = true;
            _pendingNextStep = nextStepOnDismiss;

            SetDialogueVisible(true);

            dialogueUI.ShowLine(text);
            _lastDialogueText = text;
            dialogueUI.SetHUDVisible(true);
        }

        /*
         * Hides the tutorial dialogue while keeping the HUD visible.
         *
         * @return void
         */
        private void HideDialogue()
        {
            _dialogueVisible = false;
            SetDialogueVisible(false);
            dialogueUI.SetHUDVisible(true);
        }

        /*
         * Forces any currently running dialogue to stop
         * to prevent conflicts with tutorial dialogue.
         *
         * @return void
         */
        private void ForceStopRunnerDialogue()
        {
            DSDialogueRunner runner = FindObjectOfType<DSDialogueRunner>();
            if (runner != null && runner.IsDialogueRunning)
                runner.ForceEndDialogue();
        }

        /*
         * Registers a condition that must be satisfied before
         * advancing the tutorial to the next step.
         *
         * @param condition Function returning true when the requirement is met.
         * @param nextStep The tutorial step executed once the condition succeeds.
         * @return void
         */
        private void WaitUntil(Func<bool> condition, int nextStep)
        {
            _waitCondition = condition;
            _waitNextStep = nextStep;
        }

        /*
         * Clears the active waiting condition,
         * allowing the tutorial to resume normal flow.
         *
         * @return void
         */
        private void ClearWait()
        {
            _waitCondition = null;
            _waitNextStep = -1;
        }


        /*
         * Finalizes the tutorial by restoring UI state,
         * saving completion progress, and disabling the tutorial object.
         *
         * @return void
         */
        private void CompleteTutorial()
        {
            _dialogueVisible = false;
            _pendingNextStep = -1;

            arrowIndicator?.ClearTarget();

            if (dialogueUI != null && dialogueUI.root != null)
                dialogueUI.root.SetActive(false);


            PlayerPrefs.SetInt(TUTORIAL_COMPLETE_KEY, 1);
            PlayerPrefs.Save();
            

            IsActive = false;
            gameObject.SetActive(false);
        }

        /*
         * Ensures interaction and UI visibility are restored
         * when the tutorial object is disabled.
         *
         * @return void
         */
        private void OnDisable()
        {
            SetSalesmanInteractable(true);
            SetDialogueVisible(true);

            arrowIndicator?.ClearTarget();
        }

        /*
         * Enables or disables the shopkeeper's collider
         * to control whether the player can interact.
         *
         * @param enabled True to allow interaction; false to block it.
         * @return void
         */
        private void SetSalesmanInteractable(bool enabled)
        {
            if (salesmanCollider != null)
                salesmanCollider.enabled = enabled;
        }

        /*
         * Initializes the CanvasGroup used to control dialogue visibility
         * and player interaction with the dialogue UI.
         *
         * @return void
         */
        private void InitDialogueCanvasGroup()
        {
            if (dialogueUI == null || dialogueUI.root == null) return;

            _dialogueCanvasGroup = dialogueUI.root.GetComponent<CanvasGroup>();
            if (_dialogueCanvasGroup == null)
                _dialogueCanvasGroup = dialogueUI.root.AddComponent<CanvasGroup>();

            _dialogueCanvasGroup.alpha = 1f;
            _dialogueCanvasGroup.interactable = true;
            _dialogueCanvasGroup.blocksRaycasts = true;
        }

        /*
         * Toggles the visibility and interactivity of the dialogue UI.
         *
         * @param visible True to show the dialogue; false to hide it.
         * @return void
         */
        private void SetDialogueVisible(bool visible)
        {
            if (_dialogueCanvasGroup == null)
                InitDialogueCanvasGroup();

            if (_dialogueCanvasGroup == null)
                return;

            _dialogueCanvasGroup.alpha = visible ? 1f : 0f;
            _dialogueCanvasGroup.interactable = visible;
            _dialogueCanvasGroup.blocksRaycasts = visible;
        }

        /*
         * Automatically restores dialogue visibility if an NPC dialogue
         * appears while the tutorial dialogue is hidden.
         *
         * @return void
         */
        private void AutoUnhideForNpcDialogue()
        {
            if (dialogueUI == null || dialogueUI.dialogueText == null || _dialogueCanvasGroup == null)
                return;

            if (!_dialogueVisible && _dialogueCanvasGroup.alpha <= 0.001f)
            {
                string current = dialogueUI.dialogueText.text;
                if (!string.IsNullOrEmpty(current) && current != _lastDialogueText)
                {
                    SetDialogueVisible(true);
                }
            }
        }

        private class InventorySnapshot
        {
            public int totalItemCount;

            /*
             * Creates a lightweight snapshot of the player's inventory
             * to detect item count changes after a purchase.
             *
             * @param inventory The inventory to capture.
             * @return A snapshot containing the total item count.
             */
            public InventorySnapshot(Inventory inventory)
            {
                totalItemCount = 0;
                if (inventory == null) return;

                foreach (Slot slot in inventory.getSlots())
                {
                    if (slot.getType() != CollectableType.NONE)
                        totalItemCount += slot.getCount();
                }
            }
        }
        
#if UNITY_EDITOR
        /*
         * Editor menu item to display current tutorial key value.
         */
        [UnityEditor.MenuItem("Tools/Tutorial/Show ShopTutorial Key")]
        public static void ShowTutorialKey()
        {
            int value = PlayerPrefs.GetInt("ShopTutorialDone", 0);
            Debug.Log($"ShopTutorialDone = {value} (0=not done, 1=completed)");
        }

        /*
         * Editor menu item to reset only the tutorial PlayerPrefs key.
         */
        [UnityEditor.MenuItem("Tools/Tutorial/Reset ShopTutorial Key Only")]
        public static void ResetTutorialKeyOnly()
        {
            PlayerPrefs.DeleteKey("ShopTutorialDone");
            PlayerPrefs.Save();
            Debug.Log("ShopTutorialDone key deleted. Reload scene to replay tutorial.");
        }
#endif
    }
}
