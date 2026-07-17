
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

namespace Farm
{
    public class FarmingManager : MonoBehaviour
    {
        [Serializable]
        private class SeedRecord
        {
            public Vector3Int position;
            public string seedKey; // normalized name (lowercase)
            public int growthStage;
            public bool readyToGrow; // persisted readiness when scene is unloaded
        }

        private Dictionary<Vector3Int, SeedRecord> _seedRecords = new Dictionary<Vector3Int, SeedRecord>();
        private Dictionary<Vector3Int, GameObject> _instantiatedSeeds = new Dictionary<Vector3Int, GameObject>();

        private Tilemap _soilTilemap;
        [Tooltip("This list contains all the prefabs of the seeds that can be planted")]
        [SerializeField] private List<GameObject> seeds = new List<GameObject>();
        private List<string> seedNames = new List<string>();

        public static FarmingManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void Start()
        {
            GameObject soilGameObject = Utils.FindGameObjectInScene("SoilTileMap");
            if (soilGameObject != null)
            {
                _soilTilemap = soilGameObject.GetComponent<Tilemap>();
                _soilTilemap.CompressBounds();
            }

            // cache seed names (normalized to lowercase)
            seedNames.Clear();
            foreach (GameObject seed in seeds)
            {
                string seedName = seed.name.ToLower();
                seedNames.Add(seedName);
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // trova il soil tilemap nella nuova scena (se esiste)
            GameObject soilGameObject = Utils.FindGameObjectInScene("SoilTileMap");
            if (soilGameObject != null)
            {
                _soilTilemap = soilGameObject.GetComponent<Tilemap>();
                _soilTilemap.CompressBounds();
                ReinstantiateAllSeeds(soilGameObject);
            }
            else
            {
                // scena diversa dalla farm: salva lo stato di prontezza e rimuovi i GameObject dei semi istanziati
                SaveReadinessBeforeDestroy();
                DestroyInstantiatedSeeds();
                _soilTilemap = null;
            }
        }

        private void ReinstantiateAllSeeds(GameObject soilGameObject)
        {
            // distrugge eventuali semi istanziati che appartengono alla scena precedente
            DestroyInstantiatedSeeds();

            foreach (var kv in _seedRecords)
            {
                var record = kv.Value;
                GameObject prefab = GetSeedByName(record.seedKey);
                if (prefab == null) continue;

                Vector3 instantiatePos =
                    new Vector3(record.position.x + 0.5f, record.position.y + 0.5f, record.position.z);

                // parenta il seed al SoilTileMap così il seed viene gestito dalla scena corrente
                GameObject instantiated =
                    Instantiate(prefab, instantiatePos, Quaternion.identity, soilGameObject.transform);

                // applica lo stadio di crescita
                var seedComp = instantiated.GetComponent<Seed>();
                if (seedComp != null)
                {
                    for (int i = 0; i < record.growthStage; i++)
                        seedComp.IncreaseGrowthStage();

                    // se c'era un flag di readyToGrow, preservalo sul componente (se il componente ha API compatibile)
                    // se non esiste API, il flag verrà usato da EndOfFarmingDay quando il GameObject non è presente
                }

                _instantiatedSeeds[record.position] = instantiated;
            }
        }

        public bool IsSoilFree(Vector3Int pos)
        {
            return !_seedRecords.ContainsKey(pos);
        }

        private void SaveReadinessBeforeDestroy()
        {
            foreach (var kv in _instantiatedSeeds)
            {
                var pos = kv.Key;
                var go = kv.Value;
                if (go == null) continue;

                SeedRecord record;
                if (!_seedRecords.TryGetValue(pos, out record)) continue;

                var seedComp = go.GetComponent<Seed>();
                if (seedComp != null)
                {
                    // salva lo stato di prontezza nel record prima di distruggere il GameObject
                    record.readyToGrow = seedComp.IsReadyToGrow();
                }
                else
                {
                    record.readyToGrow = false;
                }
            }
        }

        private void DestroyInstantiatedSeeds()
        {
            foreach (var go in _instantiatedSeeds.Values)
            {
                if (go != null)
                    Destroy(go);
            }
            _instantiatedSeeds.Clear();
        }

        public void Plant(Vector3Int pos, GameObject seedPrefab)
        {
            if (IsSoilFree(pos))
            {
                if (DataManager.Instance != null)
                {
                    DataManager.Instance.AddSeedPlanted();
                }

                // store persistent record
                string key = seedPrefab.name.ToLower();
                var record = new SeedRecord { position = pos, seedKey = key, growthStage = 0, readyToGrow = false };
                _seedRecords[pos] = record;

                // instantiate plant in the current scene and parent to SoilTileMap if presente
                Vector3 instantiatePos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, pos.z);
                GameObject parent = Utils.FindGameObjectInScene("SoilTileMap");
                GameObject instantiatedSeed;
                if (parent != null)
                    instantiatedSeed = Instantiate(seedPrefab, instantiatePos, Quaternion.identity, parent.transform);
                else
                    instantiatedSeed = Instantiate(seedPrefab, instantiatePos, Quaternion.identity);

                _instantiatedSeeds[pos] = instantiatedSeed;
            }
#if UNITY_EDITOR
            else
            {
                Debug.LogError($"There is already a seed present at '{pos.ToString()}'");
            }
#endif
        }

        public void WaterTile(Vector3 pos)
        {
            if (_soilTilemap == null)
            {
                var soilGO = Utils.FindGameObjectInScene("SoilTileMap");
                if (soilGO == null) return;
                _soilTilemap = soilGO.GetComponent<Tilemap>();
            }
            Vector3Int tilePos = _soilTilemap.WorldToCell(pos);
            _soilTilemap.SetColor(tilePos, new Color(188 / 255f, 188 / 255f, 188 / 255f, 1f));
        }

        public void StartOfFarmingDay()
        {
            Debug.Log("StartOfFarmingDay");
            // ensure tilemap reference
            GameObject soilGameObject = Utils.FindGameObjectInScene("SoilTileMap");
            if (soilGameObject != null)
            {
                _soilTilemap = soilGameObject.GetComponent<Tilemap>();
                _soilTilemap.CompressBounds();
                // reinstantiate solo se la tilemap esiste
                ReinstantiateAllSeeds(_soilTilemap.gameObject);
            }
        }

        public void EndOfFarmingDay()
        {
            Debug.Log("EndOfFarmingDay");
            GameObject soilGameObject = Utils.FindGameObjectInScene("SoilTileMap");
            if (soilGameObject != null)
            {
                _soilTilemap = soilGameObject.GetComponent<Tilemap>();
                _soilTilemap.CompressBounds();
            }

            // iterate through recorded seeds (non dipende dalla scena)
            var positions = new List<Vector3Int>(_seedRecords.Keys);
            foreach (var pos in positions)
            {
                if (_soilTilemap != null)
                    _soilTilemap.SetColor(pos, Color.white);

                SeedRecord record = _seedRecords.GetValueOrDefault(pos, null);
                GameObject seedGO = _instantiatedSeeds.GetValueOrDefault(pos, null);

                if (record == null) continue;

                if (seedGO != null)
                {
                    try { seedGO.transform.Find("GrowthBackground").gameObject.SetActive(false); } catch (Exception) { }
                    Seed seedComp = seedGO.GetComponent<Seed>();
                    if (seedComp != null)
                    {
                        if (seedComp.IsReadyToGrow())
                        {
                            seedComp.IncreaseGrowthStage();
                            record.growthStage++; // persist the growth
                        }
                        else
                        {
                            DataManager.Instance.AddSeedFailed();
                        }
                        seedComp.ResetSpellHit();
                        record.readyToGrow = false; // consumed
                    }
                }
                else
                {
                    // non siamo nella scena farm: usa il flag salvato
                    if (record.readyToGrow)
                    {
                        record.growthStage++;
                        record.readyToGrow = false;
                    }
                    else
                    {
                        DataManager.Instance.AddSeedFailed();
                    }
                }
            }
        }

        public void UpdateSeedAtPos(Vector3Int pos, GameObject seed)
        {
            // update only the instantiated mapping (used for runtime changes)
            _instantiatedSeeds[pos] = seed;

            // se il seme esiste, aggiorna anche il flag di readiness nel record (se presente)
            if (_seedRecords.TryGetValue(pos, out var record) && seed != null)
            {
                var seedComp = seed.GetComponent<Seed>();
                if (seedComp != null)
                    record.readyToGrow = seedComp.IsReadyToGrow();
            }
        }

        public void DestroySeedAtPos(Vector3Int pos)
        {
            // remove persistent record
            if (_seedRecords.ContainsKey(pos))
                _seedRecords.Remove(pos);

            // destroy instantiated object if present
            if (_instantiatedSeeds.TryGetValue(pos, out var go) && go != null)
            {
                Destroy(go);
            }
            _instantiatedSeeds.Remove(pos);

            if (_soilTilemap != null)
            {
                Vector3Int tilePos = _soilTilemap.WorldToCell((Vector3)pos);
                _soilTilemap.SetColor(tilePos, Color.white);
            }
        }

        public GameObject GetSeedByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            name = name.ToLower();

            // try direct match, otherwise try append/remove "-seed"
            string match = name;
            if (!seedNames.Contains(match))
            {
                if (!match.EndsWith("-seed")) match = match + "-seed";
                if (!seedNames.Contains(match) && match.EndsWith("-seed"))
                {
                    var alt = match.Substring(0, match.Length - 5);
                    if (seedNames.Contains(alt)) match = alt;
                }
            }

            int idx = seedNames.IndexOf(match);
            if (idx >= 0 && idx < seeds.Count) return seeds[idx];
            return null;
        }
    }
}
