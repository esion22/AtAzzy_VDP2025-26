using System;
using Farm;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    public void LoadTavern()
    {
        // If the farm tutorial is not completed, mark it as completed
        if (PlayerPrefs.GetInt("CookingTutorialDone", 0) == 0)
        {
            PlayerPrefs.SetInt("FarmTutorialDone", 1);
            PlayerPrefs.Save();

            // Debug.Log("FarmTutorialDone was 0, now set to 1");

            Inventory inventory = GameManager.instance.playerInventory;
            inventory.ClearInventory();
        }

        if (SceneFadeManager.Instance != null)
            SceneFadeManager.Instance.FadeToScene("TavernScene");
        else
            SceneManager.LoadScene("TavernScene", LoadSceneMode.Single);
    }

    public void LoadFarm()
    {
        if (SceneFadeManager.Instance != null)
            SceneFadeManager.Instance.FadeToScene("FarmScene");
        else
            SceneManager.LoadScene("FarmScene", LoadSceneMode.Single);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "FarmScene")
        {
            FarmingManager.Instance.EndOfFarmingDay();
            FarmingManager.Instance.StartOfFarmingDay();
        }
    }
}
