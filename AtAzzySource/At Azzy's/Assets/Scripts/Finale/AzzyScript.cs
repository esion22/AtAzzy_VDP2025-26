using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Finale
{
    public class AzzyScript: MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject humanPrefab;
        [SerializeField] private GameObject dragonPrefab;

        [Header("Spawn Positions")]
        [SerializeField] private Vector3 humanSpawnPosition;
        [SerializeField] private Vector3 dragonSpawnPosition;
        [SerializeField] private Vector3 tavernHumanSpawnPosition;
        [SerializeField] private Vector3 tavernDragonSpawnPosition;
        
        [Header("UI")]
        [SerializeField] private Image npcPortrait;
        [SerializeField] private Sprite humanSprite;
        [SerializeField] private Sprite dragonSprite;
        
        [Header("NPC Names")]
        [SerializeField] private string humanNPCName = "AzzyHuman";
        [SerializeField] private string dragonNPCName = "AzzyDragon";
        
        [Header("Dialogue Containers")]
        [SerializeField] private DS.ScriptableObjects.DSDialogueContainerSO humanDialogueContainer;
        [SerializeField] private DS.ScriptableObjects.DSDialogueContainerSO dragonDialogueContainer;

        [Header("Finale Dialogue")]
        [SerializeField] private DS.ScriptableObjects.DSDialogueSO farmFinaleDialogue;

        [Header("Random Dialogues")]
        [SerializeField] private DS.ScriptableObjects.DSDialogueSO[] humanDialogues;
        [SerializeField] private DS.ScriptableObjects.DSDialogueSO[] dragonDialogues;

        private GameObject _currentNPC;
        private GameObject _highlight;
        private bool _isDragon = true;
        private bool _playerInRange = false;

        private const string CookingFinaleKey = "CookingFinaleCompleted";
        private const string FarmFinaleKey = "FarmFinaleCompleted";

        private void Awake()
        {
            string activeScene = SceneManager.GetActiveScene().name;

            // Check PlayerPrefs to determine the form
            _isDragon = PlayerPrefs.GetInt(CookingFinaleKey, 0) == 0;

            // Instantiate the appropriate prefab
            if (activeScene == "FarmScene")
            {
                if (_isDragon)
                {
                    _currentNPC = Instantiate(dragonPrefab, dragonSpawnPosition, Quaternion.identity, transform);
                    _currentNPC.name = dragonNPCName;
                }
                else
                {
                    _currentNPC = Instantiate(humanPrefab, humanSpawnPosition, Quaternion.identity, transform);
                    _currentNPC.name = humanNPCName;
                }
            }
            else if (activeScene == "TavernScene")
            {
                if (_isDragon)
                {
                    _currentNPC = Instantiate(dragonPrefab, tavernDragonSpawnPosition, Quaternion.identity, transform);
                    _currentNPC.name = dragonNPCName;
                }
                else
                {
                    _currentNPC = Instantiate(humanPrefab, tavernHumanSpawnPosition, Quaternion.identity, transform);
                    _currentNPC.name = humanNPCName;
                }
            }
            
            // Add and configure AzzyInteraction
            var interaction = _currentNPC.AddComponent<AzzyInteraction>();
            interaction.isDragon = _isDragon;
            interaction.humanDialogueContainer = humanDialogueContainer;
            interaction.dragonDialogueContainer = dragonDialogueContainer;
            interaction.farmFinaleDialogue = farmFinaleDialogue;
            interaction.humanDialogues = humanDialogues;
            interaction.dragonDialogues = dragonDialogues;
            
            // Find and deactivate the Highlight child object
            _highlight = _currentNPC.transform.Find("Highlight")?.gameObject;
            if (_highlight != null)
            {
                _highlight.SetActive(false);
            }
        }
        public void ConverToHuman(){
            _isDragon = false;
        }
        public bool GetIsDragon(){
            return _isDragon;
        }

        public Sprite GetCurrentSprite()
        {
            if (_isDragon)
            {
                return dragonSprite;
            }
            else
            {
                return humanSprite;
            }
        }
        
    }
}