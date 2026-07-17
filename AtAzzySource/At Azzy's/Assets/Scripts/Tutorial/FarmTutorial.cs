using UnityEngine;
using UnityEngine.UI;
using Farm;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using PixelAdventures.Scripts.Completed_Scripts;
using System.Collections;



namespace Tutorial
{
    public class FarmTutorial : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image dragonPortrait;
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private DSDialogueUI dialogueUI;
        [SerializeField] private DSDialogueRunner dialogueRunner;
        [Header("Highlight References")]
        [SerializeField] private HotbarController hotbarController;

      
        [SerializeField] private Vector2 cookingTutorialPanelPosition = new Vector2(184f, 0f);



        [Header("Tutorial Settings")]
        [SerializeField] private bool skipTutorial = false;
        [SerializeField] private List<Collectable> seedsInInventory;

        [Header("House Trigger")]
        [SerializeField] private BoxCollider2D houseTriggerCollider;
        [Header("Shopkeeper Control")]
        [SerializeField] private BoxCollider2D salesmanCollider;


        [Header("Farming")]
        [SerializeField] private Vector3Int tutorialPlantCell = new Vector3Int(0, 0, 0);
        [SerializeField] private string tutorialSeedName = "potato";

        [Header("Arrow")]
        [SerializeField] private Vector3 arrowOffset = new Vector3(-1.5f, -1.5f, 0);


        private const string TUTORIAL_COMPLETE_KEY = "FarmTutorialDone";

        public static FarmTutorial Instance { get; private set; }
        public bool IsActive { get; private set; }

        private int _currentStep;
        private bool _dialogueVisible;
        private int _pendingNextStep = -1;

        private Func<bool> _waitCondition;
        private int _waitNextStep = -1;

        private GameObject _currentArrow;

        private Seed _tutorialSeed;
        private GameObject _tutorialPotato;
        private Seed _seedPlanted;

        private CapsuleCollider2D _tutorialSeedCollider;


        private RectTransform _dialogueRect;
        private RectTransform _dialogueTextRect;
        private float _originalDialogueWidth;
        private float _originalTextWidth;
        private Vector2 _originalDialoguePosition;
        private float _originalFontSize;
        private bool _tutorialSeedPlanted = false;

        private Inventory inventory;

        // Spell tutorial colors (bright versions)
        private static readonly Color FireColor = new Color(1f, 0.4f, 0.2f, 1f);    
        private static readonly Color WaterColor = new Color(0.2f, 0.7f, 1f, 1f);  
        private static readonly Color GrowthColor = new Color(0.3f, 1f, 0.4f, 1f);  
        private static readonly Color CutColor = new Color(0.6f, 0.6f, 0.7f, 1f);  
        private int _highlightedSlotIndex = -1;

        




        // ─────────────────────────────
        // UNITY
        // ─────────────────────────────

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (skipTutorial || PlayerPrefs.GetInt(TUTORIAL_COMPLETE_KEY, 0) == 1)
            {
                gameObject.SetActive(false);
                return;
            }

            StartCoroutine(StartWhenReady());
        }
        private void Update()
        {
            if (!IsActive)
                return;

            // Dialogue input
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

            // ⭐ Tutorial condition (independent of dialogue)
            if (_waitCondition != null && _waitCondition())
            {
                int next = _waitNextStep;
                ClearWait();

                // ⭐ FarmingManager ready → plant tutorial seed (only once)
                if (!_tutorialSeedPlanted)
                {
                    PlantTutorialSeed();
                    _tutorialSeedPlanted = true;
                }


                if (_currentStep != 100)
                {
                    ClearArrow();
                }

                if (next >= 0)
                    ShowStep(next);
            }
        }
        private IEnumerator StartWhenReady()
        {
            yield return new WaitUntil(() =>
                dialogueUI != null &&
                dialogueRunner != null &&
                GameManager.instance != null &&
                FarmingManager.Instance != null
            );

            yield return null; // 再等一帧，确保内部初始化完成

            StartTutorial();
        }


        // ─────────────────────────────
        // FLOW
        // ─────────────────────────────

        private void StartTutorial()
        {
            inventory = GameManager.instance.playerInventory;
            foreach (Collectable itemSeed in seedsInInventory)
            {
                inventory.Add(itemSeed);
            }

            IsActive = true;

            if (dialogueRunner != null)
                dialogueRunner.enabled = false;

            Time.timeScale = 1f;

            // disable triggers
            SetHouseTrigger(false);
            SetSalesmanInteractable(false);


            


            // ⭐ 等 FarmingManager ready，再执行 PlantTutorialSeed
            WaitUntil(
                () => IsFarmingManagerReady(),
                -1   // 不自动切 step，只用于初始化
            );

            ShowStep(11);
        }



        private void ShowStep(int step)
        {
            _currentStep = step;
            if (step!=100)
            {
                ClearArrow();
                
            }
            
            ClearHotbarHighlight();

            switch (step)
            {
                case 11:
                    ShowDialogue("Hello fellow wizard! Thanks for hearing my call for help.", 12);
                    break;
                
                case 12:
                    ShowDialogue("As you can see, I am not your fellow tavern keeper anymore...\nI've been transformed into a small dragon.", 13);
                    break;
                
                case 13:
                    ShowDialogue("I cannot run the tavern like this, so I need your help to manage my farm and gather ingredients.", 14);
                    break;
                
                case 14:
                    ShowDialogue("Maybe in the mean time we might also find a way to break this curse?", 15);
                    break;
                
                case 15:
                    ShowDialogue("But let's not waste time, we have a farm to run!", 2);
                    break;
                
                case 0:
                    ShowDialogue("Welcome to your farm!", 1);
                    break;

                case 1:
                    ShowDialogue("I planted a seed for you. This soil is ready.", 2);
                    break;

                case 2:
                    ShowDialogue("In order to harvest the plant, you need to use the cut spell. Press '4' to select and then click to cast it");
                    
                    SetTutorialPlantInteractable(true);

                    ShowArrowAtWorldPosition(CellCenter(tutorialPlantCell));
                    HighlightSpellSlot(3);

                    WaitUntil(
                        () =>
                        {
                            var fm = FarmingManager.Instance;
                            return fm != null && fm.IsSoilFree(tutorialPlantCell);
                        },
                        3
                    );
                    break;

                case 3:
                    ShowDialogue("Great, walk to the potato to pick it up");
                    ShowArrowAtWorldPosition(new Vector3(0.5f,0.5f,0f) );

                    WaitUntil(
                        () => IsTutorialPotatoCollected(),
                        4
                    );
                    break;

                case 4:
                    ShowDialogue("Nice, you have a potato in your inventory now. Let's plant some more seeds!",5);
                    break;

                case 5:
                    
                    ShowDialogue("Use 'Q' and 'E' to select the seed in your inventory and plant it on the soil by pressing 'F'");
                    ShowArrowAtWorldPosition(new Vector3(5f,1.44f,0f), null, Quaternion.Euler(0, 0, -90f) );
                    WaitUntil(
                        () => IsSeedPlanted(),
                        6
                    );
                    break;
                case 6:
                    ShowDialogue("Water the seed using water spell. Select it by pressing '2' and click to cast it!");
                    if(_seedPlanted!=null) {
                        Vector3 pos = _seedPlanted.transform.position;
                        ShowArrowAtWorldPosition(pos, null, Quaternion.Euler(0, 0, 90f) );
                    }
                    HighlightSpellSlot(1);
               
               
                    WaitUntil(
                        () => IsSeedWatered(),
                        7
                    );
                    break;
                case 7:
                    ShowDialogue("Now use the growth spell on the seed. This way we can collect it tomorrow! Select it by pressing '3' and click to cast it!");
                    if(_seedPlanted!=null){
                        Vector3 pos = _seedPlanted.transform.position;
                        ShowArrowAtWorldPosition(pos, null, Quaternion.Euler(0, 0, 90f) );
                    }
                    
                    HighlightSpellSlot(2);
               
                    WaitUntil(
                        () => IsSeedReadyToGrow(),
                        10
                    );
                    break;


                case 10:
                     ShowArrowAtWorldPosition(
                        new Vector3(-7.9f, 1.5f, 0f),
                        null,
                        Quaternion.Euler(0, 0, 180f)
                    );
                    SetHouseTrigger(true);
                    ShowDialogue("You are as good as a magician as you are as a farmer. Let's see if you can satisfy clients though, let's step into the tavern!", 100);
                    break;

                case 100:
                    CompleteTutorial();
                    break;
            }
        }


        // ─────────────────────────────
        // FARMING
        // ─────────────────────────────
        private bool IsSeedWatered(){
            var seeds = FindObjectsOfType<Seed>();
     
            foreach (var seed in seeds)
            {
                if (seed.IsWatered())
                {   
                    _seedPlanted = seed;
                    return true;
                }
                
            }
            return false;
        }
        private bool IsSeedReadyToGrow(){
            var seeds = FindObjectsOfType<Seed>();
     

            foreach (var seed in seeds)
            {

                if (seed.IsReadyToGrow())
                {   _seedPlanted = seed;
                    return true;
                }
                
            }
            return false;
        }

        private bool InventoryContains(CollectableType type){
            Inventory inventory = GameManager.instance.playerInventory;
            List<Slot> slots = inventory.getSlots();

            foreach (Slot slot in slots)
            {
                if (slot.getType() == type && slot.getCount() >= 1){
                    return true;
                }
                    

            }

            return false;

        }
        private bool IsSeedPlanted(){
            var seeds = FindObjectsOfType<Seed>();

            if (seeds.Length == 0)
            {
                return false;
            }
            _seedPlanted = seeds[0];


        return true;

        }

        private bool IsTutorialPotatoCollected()
        {
            foreach (var c in FindObjectsOfType<Collectable>())
            {
                if (c.getType() == CollectableType.POTATO)
                    return false; // 还有 potato，没捡
            }
            return true; // 场上已经没有 potato
        }
        

        private void PlantTutorialSeed()
        {
            var manager = FarmingManager.Instance;
            if (manager == null)
            {
                Debug.LogError("❌ FarmingManager not found");
                return;
            }

            var seedPrefab = manager.GetSeedByName(tutorialSeedName);
            if (seedPrefab == null)
            {
                Debug.LogError($"❌ Seed '{tutorialSeedName}' not found");
                return;
            }

            if (!manager.IsSoilFree(tutorialPlantCell))
            {
                Debug.LogWarning("⚠️ Tutorial soil already occupied");
                return;
            }

            manager.Plant(tutorialPlantCell, seedPrefab);
            CacheTutorialSeed();
        }

        private void CacheTutorialSeed()
        {
            foreach (var seed in FindObjectsOfType<Seed>())
            {
                if (WorldToCell(seed.transform.position) != tutorialPlantCell)
                    continue;

                seed.IncreaseGrowthStage(); // Tutorial: force mature
                _tutorialSeed = seed;

                _tutorialSeedCollider = seed.GetComponent<CapsuleCollider2D>();
                if (_tutorialSeedCollider == null)
                {
                    Debug.LogError("❌ _tutorialSeedCollider is NULL");
                }
                else
                {
                    Debug.Log("Collider Object: " + _tutorialSeedCollider.gameObject.name);

                    if (_tutorialSeedCollider.transform.parent != null)
                        Debug.Log("Parent Object: " + _tutorialSeedCollider.transform.parent.name);
                    else
                        Debug.Log("Parent Object: (no parent — root object)");
                }

                SetTutorialPlantInteractable(false);

                Debug.Log("🌱 [FarmTutorial] Tutorial seed cached");
                return;
            }

            Debug.LogWarning("⚠️ [FarmTutorial] Tutorial seed not found");
            Debug.Log($"[CacheTutorialSeed] disabling collider at frame {Time.frameCount}");

        }

        private void SetTutorialPlantInteractable(bool enabled)
        {
            if (_tutorialSeedCollider == null)
            {
                Debug.LogError("NULL collider");
                return;
            }

            _tutorialSeedCollider.enabled = enabled;

            Debug.Log($"[Tutorial] Set collider = {enabled} at frame {Time.frameCount}");
        }


        // ─────────────────────────────
        // DIALOGUE
        // ─────────────────────────────

        private void ShowDialogue(string text, int nextStepOnDismiss = -1)
        {
            _dialogueVisible = true;
            _pendingNextStep = nextStepOnDismiss;

            if (dialogueRunner != null)
                dialogueRunner.enabled = false;

            dialogueUI.ShowLine(text);
            dialogueUI.SetHUDVisible(true);
            Time.timeScale = 1f;
        }

        private void HideDialogue()
        {
            _dialogueVisible = false;
            dialogueUI.root.SetActive(false);
            dialogueUI.SetHUDVisible(true);
        }

        // ─────────────────────────────
        // WAIT SYSTEM
        // ─────────────────────────────

        private void WaitUntil(Func<bool> condition, int nextStep)
        {
            _waitCondition = condition;
            _waitNextStep = nextStep;
        }

        private void ClearWait()
        {
            _waitCondition = null;
            _waitNextStep = -1;
        }

        // ─────────────────────────────
        // ARROW
        // ─────────────────────────────

        private void ShowArrowAtWorldPosition(
            Vector3 worldPosition,
            Vector3? customOffset = null,
            Quaternion? customRotation = null
        )
        {
            if (arrowPrefab == null) return;

            Vector3 offset = customOffset ?? arrowOffset;
            Quaternion rotation = customRotation ?? Quaternion.Euler(0, 0, 90f);

            _currentArrow = Instantiate(
                arrowPrefab,
                worldPosition + offset,
                rotation
            );
        }




        private void ClearArrow()
        {
            if (_currentArrow != null)
                Destroy(_currentArrow);
        }




        // ─────────────────────────────
        // COMPLETE
        // ─────────────────────────────

        private void CompleteTutorial()
        {
            Debug.Log("✅ [FarmTutorial] CompleteTutorial() called");

            if (dialogueRunner != null)
                dialogueRunner.enabled = true;

            PlayerPrefs.SetInt(TUTORIAL_COMPLETE_KEY, 1);
            PlayerPrefs.Save();

            Debug.Log($"💾 [FarmTutorial] Saved {TUTORIAL_COMPLETE_KEY} = {PlayerPrefs.GetInt(TUTORIAL_COMPLETE_KEY)}");

            
    

            IsActive = false;
            gameObject.SetActive(false);
        }

        // ─────────────────────────────
        // UTIL
        // ─────────────────────────────

        private static Vector3Int WorldToCell(Vector3 worldPos)
        {
            return new Vector3Int(
                Mathf.FloorToInt(worldPos.x),
                Mathf.FloorToInt(worldPos.y),
                0);
        }

        private static Vector3 CellCenter(Vector3Int cell)
        {
            return new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f);
        }
        private void SetHouseTrigger(bool enabled)
        {
            if (houseTriggerCollider == null)
            {
                Debug.LogWarning("[FarmTutorial] HouseTrigger collider not assigned");
                return;
            }

            houseTriggerCollider.enabled = enabled;
            Debug.Log($"[FarmTutorial] HouseTrigger collider enabled = {enabled}");
        }
        private void SetSalesmanInteractable(bool enabled)
        {
            if (salesmanCollider == null)
            {
                Debug.LogWarning("[FarmTutorial] Salesman collider not assigned");
                return;
            }

            salesmanCollider.enabled = enabled;
            Debug.Log($"[FarmTutorial] Salesman collider enabled = {enabled}");
        }
        private bool IsFarmingManagerReady()
        {
            var manager = FarmingManager.Instance;
            if (manager == null) return false;

            return manager.GetSeedByName(tutorialSeedName) != null;
        }
        private void HighlightSpellSlot(int slotIndex)
        {
            Debug.Log("FarmTutorial>> HighlightSpellSlot>> highlighting" + slotIndex);
            Debug.Log("FarmTutorial>> HighlightSpellSlot>> hotbarController is null" + hotbarController == null);

            if (hotbarController == null) return;
            if (slotIndex < 0 || slotIndex >= hotbarController.slots.Length) return;

            ClearHotbarHighlight();

            Color color = slotIndex switch
            {
                0 => FireColor,
                1 => WaterColor,
                2 => GrowthColor,
                3 => CutColor,
                _ => Color.white
            };

            hotbarController.slots[slotIndex].SetTutorialHighlight(color);
            _highlightedSlotIndex = slotIndex;
        }
         /*
         * Clears the highlight from the currently highlighted hotbar slot.
         */
        private void ClearHotbarHighlight()
        {
            if (hotbarController == null) return;

            if (_highlightedSlotIndex >= 0 && _highlightedSlotIndex < hotbarController.slots.Length)
            {
                hotbarController.slots[_highlightedSlotIndex].SetTutorialHighlight(null);
            }
            _highlightedSlotIndex = -1;
        }

        public int getCaseNum()
        {
            return _currentStep;
        }




        #if UNITY_EDITOR
            /*
            * Editor menu item to display current tutorial key value.
            */
            [UnityEditor.MenuItem("Tools/Tutorial/Show FarmTutorial Key")]
            public static void ShowTutorialKey()
            {
                int value = PlayerPrefs.GetInt("FarmTutorialDone", 0);
                Debug.Log($"FarmTutorialDone = {value} (0=not done, 1=completed)");
            }

            /*
            * Editor menu item to reset only the tutorial PlayerPrefs key.
            */
            [UnityEditor.MenuItem("Tools/Tutorial/Reset FarmTutorial Key Only")]
            public static void ResetTutorialKeyOnly()
            {
                PlayerPrefs.DeleteKey("FarmTutorialDone");
                PlayerPrefs.Save();
                Debug.Log("FarmTutorialDone key deleted. Reload scene to replay tutorial.");
            }
        #endif



    }
}