using UnityEngine;
using UnityEngine.SceneManagement;

public class GMChecker : MonoBehaviour
{
    /*
     * Unity Start callback.
     * Automatically triggers a scan for GameManager instances.
     */
    private void Start()
    {
        CheckGameManagers();
    }

    /*
     * Scans the scene for all GameManager instances and logs their information.
     */
    void CheckGameManagers()
    {
        GameManager[] managers = FindObjectsOfType<GameManager>(true);

        // Debug.Log($"<color=yellow>[GMChecker] Found {managers.Length} GameManager(s)</color>");

        int index = 0;
        foreach (var gm in managers)
        {
            // Debug.Log($"<color=cyan>GameManager[{index}] name = {gm.gameObject.name}, scene = {gm.gameObject.scene.name}</color>");
            index++;
        }
    }
}
