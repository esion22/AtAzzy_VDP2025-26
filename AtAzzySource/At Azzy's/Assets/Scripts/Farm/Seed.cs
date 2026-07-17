using System;
using System.Collections.Generic;
using System.Linq;
using PixelAdventures.Scripts.Completed_Scripts;
using UnityEngine;
using Tutorial;
namespace Farm
{
    public class Seed : MonoBehaviour, ISpellInteractable
    {
        [Tooltip("Amount of days that the seed needs to grow before being ready for harvest")]
        [SerializeField] private int growthDays = 1;
        [Tooltip("List of sprites containing the different growth stages of a seed NOTE: length of list must be growthDays+1")]
        [SerializeField] private List<Sprite> growthSprites = new List<Sprite>();
        [Tooltip("Game Object that will be dropped when the plant is cut down")]
        [SerializeField] private GameObject dropObject;
        [Tooltip("List of spells that the seed needs to receive daily in order to grow")]
        [SerializeField] private List<SpellEnum> spellsNeeded = new List<SpellEnum>();
        private List<SpellEnum> _spellsHit = new List<SpellEnum>();
        
        private int _growthStage = 0;
        private Collider2D _collider;
        private GameObject _growthEffect;

        void Start()
        {
            _collider = GetComponent<Collider2D>();
            _growthEffect = transform.Find("GrowthBackground").gameObject;
            _growthEffect.SetActive(false);
        }

        public void OnSpellHit(SpellEnum spellType)
        {
            Vector3Int pos = new Vector3Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), 0);
            if (spellsNeeded.Contains(spellType))
            {
                if (!_spellsHit.Contains(spellType))
                {
                    _spellsHit.Add(spellType);
                }

                if (_spellsHit.Contains(SpellEnum.WATER))
                {
                    _collider.excludeLayers = LayerMask.GetMask("Farming Magic Interactable WATER");
                }
                if (_spellsHit.Contains(SpellEnum.GROWTH))
                {
                    _collider.excludeLayers = LayerMask.GetMask("Farming Magic Interactable GROWTH");
                }
                
                if (IsReadyToGrow() && _growthStage < growthDays)
                {
                    _collider.excludeLayers = LayerMask.GetMask("Magic Interactable", "Farming Magic Interactable WATER", "Farming Magic Interactable GROWTH");
                }
                
                FarmingManager.Instance.UpdateSeedAtPos(pos, gameObject);
                //Add visual feedback that for example a plant got watered (make the soil darker?) and got growth (make the plant greener?)
                switch(spellType){
                    case SpellEnum.WATER:
                        FarmingManager.Instance.WaterTile(transform.position);
                        break;
                    case SpellEnum.GROWTH:
                        _growthEffect.SetActive(true);
                        break;
                    case SpellEnum.FIRE:
                        Debug.Log("Hit by fire spell and not destroyed");
                        break;
                    default: 
                        Debug.Log("New spell or i need to cut this plant to make it grow");
                        break;
                }
            }
            else
            {
                Debug.Log("Seed>>spellType " +SpellEnum.FIRE + ", FarmTutorial.Instance.IsActive "+ FarmTutorial.Instance.IsActive + "gameObject.name " + gameObject.name + ", FarmTutorial.Instance.getCaseNum()" +FarmTutorial.Instance.getCaseNum());
                if (spellType == SpellEnum.FIRE && FarmTutorial.Instance != null && FarmTutorial.Instance.IsActive && gameObject.name == "Potato-Seed(Clone)" && FarmTutorial.Instance.getCaseNum() <= 7)
                {
                    
                    return;
                }
                if (spellType == SpellEnum.CUT && growthDays == _growthStage)
                {
                    Drop();
                }
                FarmingManager.Instance.DestroySeedAtPos(pos);
                Destroy(gameObject);
            }
            
        }

        public void IncreaseGrowthStage()
        {
            if (_growthStage < growthDays)
            {
                _growthStage++;
                GetComponent<SpriteRenderer>().sprite = growthSprites[_growthStage];
            }
        }
        public bool IsWatered(){
            return _spellsHit.Contains(SpellEnum.WATER);
        }
        
        public bool IsReadyToGrow()
        {
            return spellsNeeded.All(spell => _spellsHit.Contains(spell));
        }

        public void ResetSpellHit()
        {
            _spellsHit.Clear();

            if (_collider)
            {
                _collider.excludeLayers = 0;
            }
        }
        
        public Sprite GetGrowthSprite(int idx)
        {
            if (idx <= growthDays + 1)
            {
                return growthSprites[idx];
            }
            #if UNITY_EDITOR
                Debug.LogError("No growth sprite found for " + idx);
            #endif
            return null;
        }

        private void Drop()
        {
            Instantiate(dropObject, transform.position, Quaternion.identity);
        }
    }
}
