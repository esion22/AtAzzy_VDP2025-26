using UnityEngine;
using UnityEngine.SceneManagement;
using Farm;   

public class BootsceneController : MonoBehaviour
{
    private void Awake()
    {
        // Global listener for all scene loads
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void StartGame()
    {
        Debug.Log(PlayerPrefs.GetInt("FarmTutorialDone", 0));
        if(PlayerPrefs.GetInt("FarmTutorialDone", 0) == 1){
            if (SceneFadeManager.Instance != null)
                SceneFadeManager.Instance.FadeToScene("FarmScene");
            else
                SceneManager.LoadScene("FarmScene");
        } else {
            if (SceneFadeManager.Instance != null) 
                SceneFadeManager.Instance.FadeToScene("CutScene");
            else
                SceneManager.LoadScene("CutScene");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
            return;

        // Farm logic → disable PlayerCook
        if (scene.name == "FarmScene")
        {
            var cook = player.GetComponent<PlayerCook>();
            if (cook != null)
                cook.enabled = false;
        }

        // Tavern logic → disable SeedPlanter
        if (scene.name == "TavernScene")
        {
            var planter = player.GetComponent<SeedPlanter>();
            if (planter != null)
                planter.enabled = false;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
