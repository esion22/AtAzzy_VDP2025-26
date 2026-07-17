using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Farm
{
    //This script will be need to put on the entity that will plant the seed (in our case, the player)
    //This will interact with the inventory system in order to plant the selected seed
    public class SeedPlanter : MonoBehaviour
    {
        private GameObject _plantingAvailablePopup;
        private GameObject _farmingHighlight;
        private Tilemap _soilTilemap;
        private FarmingManager _farmingManager;
        private PlayerItemController _playerInventory;
        private AudioManager _audioManager;

        private Vector3Int? _closestPlantableCell;
        
        [SerializeField] private AudioClip seedingClip;
        
        [Tooltip("This is the maximum distance at which the player will be able to plant seeds")]
        [SerializeField] private float maximumPlantingDistance = 1.5f;
        
        [SerializeField] private Camera gameCamera;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            //Get every needed game object and script reference
            _plantingAvailablePopup = Utils.FindGameObjectInScene("PlantingAvailable");
            _farmingHighlight = Utils.FindGameObjectInScene("FarmingHighlight");
            GameObject soilGameObject = Utils.FindGameObjectInScene("SoilTileMap");
            TilemapCollider2D tilemapCol = soilGameObject.GetComponent<TilemapCollider2D>();
            _soilTilemap = tilemapCol.GetComponent<Tilemap>();
            GameObject gameManager = Utils.FindGameObjectInScene("FarmingManager");
            _farmingManager = gameManager.GetComponent<FarmingManager>();
            _playerInventory = gameObject.GetComponent<PlayerItemController>();
            _audioManager = Utils.FindGameObjectInScene("AudioManager").GetComponent<AudioManager>();
            
            _plantingAvailablePopup.SetActive(false);
            _farmingHighlight.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            //checkForMouseOverSoil();
            FindClosestPlantableTile();

            Slot selection = _playerInventory.GetInventory().getSelection();
            bool canPlant = _closestPlantableCell.HasValue && selection != null &&
                            selection.getCategory() == CollectableCategory.SEED &&
                            selection.getType() != CollectableType.NONE;

            _plantingAvailablePopup.SetActive(canPlant);
            _farmingHighlight.SetActive(canPlant);

            if (canPlant)
            {
                // Update highlight position
                Vector3 tileWorldCenter = _soilTilemap.GetCellCenterWorld(_closestPlantableCell.Value);
                _farmingHighlight.transform.position = tileWorldCenter;

                if (Input.GetButtonDown("Interaction"))
                {
                    string seedName = selection.getType().ToString();
                    GameObject seedToPlant = _farmingManager.GetSeedByName(seedName);

                    if (seedToPlant)
                    {
                        //Plant the seed
                        _audioManager.PlaySFX(seedingClip);
                        _farmingManager.Plant(_closestPlantableCell.Value, seedToPlant);
                        _playerInventory.GetInventory().RemoveSelection();

                        // After planting, re-evaluate the closest tile immediately
                        FindClosestPlantableTile();
                    }
                }
            }
        }
        
        private void FindClosestPlantableTile()
        {
            float minDistance = float.MaxValue;
            Vector3Int? closestCell = null;

            // Define a search area around the player to optimize the search
            int searchRadius = Mathf.CeilToInt(maximumPlantingDistance);
            Vector3Int playerCell = _soilTilemap.WorldToCell(transform.position);

            for (int x = playerCell.x - searchRadius; x <= playerCell.x + searchRadius; x++)
            {
                for (int y = playerCell.y - searchRadius; y <= playerCell.y + searchRadius; y++)
                {
                    Vector3Int currentCell = new Vector3Int(x, y, playerCell.z);
                    // Check if there is a tile and if it's free for planting
                    if (_soilTilemap.HasTile(currentCell) && _farmingManager.IsSoilFree(currentCell))
                    {
                        Vector3 cellWorldCenter = _soilTilemap.GetCellCenterWorld(currentCell);
                        float distance = Vector2.Distance(transform.position, cellWorldCenter);

                        if (distance < minDistance && distance <= maximumPlantingDistance)
                        {
                            minDistance = distance;
                            closestCell = currentCell;
                        }
                    }
                }
            }

            _closestPlantableCell = closestCell;
        }
        
        
        
        private void checkForMouseOverSoil()
        {
            Vector2 mouseWorldPos = gameCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 0f, LayerMask.GetMask("Soil"));

            if (hit.collider)
            {
                Vector3Int cellPos = _soilTilemap.WorldToCell(mouseWorldPos);
                Vector2 cellCenter = new Vector2(cellPos.x + 0.5f, cellPos.y + 0.5f);

                // Tile must be valid to plant
                if (_farmingManager.IsSoilFree(cellPos) &&
                    Vector2.Distance(cellCenter, transform.position) < maximumPlantingDistance)
                {
                    Slot selection = _playerInventory.GetInventory().getSelection();

                    if (selection != null && selection.getCategory() == CollectableCategory.SEED &&
                        selection.getType() != CollectableType.NONE)
                    {
                        string seedName = selection.getType().ToString();
                        GameObject seedToPlant = _farmingManager.GetSeedByName(seedName);

                        if (seedToPlant)
                        {
                            Vector3 tileWorldCenter = _soilTilemap.GetCellCenterWorld(cellPos);

                            _plantingAvailablePopup.SetActive(true);
                            _farmingHighlight.transform.position = tileWorldCenter;
                            _farmingHighlight.SetActive(true);

                            if (Input.GetMouseButtonDown(0))
                            {
                                //Plant the seed
                                _audioManager.PlaySFX(seedingClip);
                                _farmingManager.Plant(cellPos, seedToPlant);

                                
                                _playerInventory.GetInventory().RemoveSelection();
                            }
                        }
                    }
                }
                else
                {
                    _plantingAvailablePopup.SetActive(false);
                    _farmingHighlight.SetActive(false);
                }
            }
            else
            {
                _plantingAvailablePopup.SetActive(false);
                _farmingHighlight.SetActive(false);
            }
        }

            
    }
}
