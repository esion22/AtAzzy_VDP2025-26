using UnityEngine;
using UnityEngine.SceneManagement;

public class Test : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void TestMethod()
    {
        Debug.Log("testMethod called");
    }

    public void ChangeToFarmScene()
    {
        SceneManager.LoadScene("Scenes/FarmScene", LoadSceneMode.Single);
        
        GameObject audioManagerObject = GameObject.Find("AudioManager");
        if (audioManagerObject != null)
        {
            AudioManager audioManager = audioManagerObject.GetComponent<AudioManager>();
            if (audioManager != null)
            {
                audioManager.PlayBackgroundFarmMusic();
            }
        }
    }
    
    public void ChangeToTavernScene()
    {
        SceneManager.LoadScene("Scenes/TavernScene", LoadSceneMode.Single);
        
        GameObject audioManagerObject = GameObject.Find("AudioManager");
        if (audioManagerObject != null)
        {
            AudioManager audioManager = audioManagerObject.GetComponent<AudioManager>();
            if (audioManager != null)
            {
                audioManager.PlayBackgroundTavernMusic();
            }
        }
    }
}
