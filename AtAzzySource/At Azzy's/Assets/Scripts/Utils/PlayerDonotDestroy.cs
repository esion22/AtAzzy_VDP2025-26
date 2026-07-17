using UnityEngine;

public class PlayerDonotDestroy : MonoBehaviour
{
    private static PlayerDonotDestroy instance;

    /*
     * Unity Awake callback.
     * Ensures only one instance of this object persists across scenes.
     */
    void Awake()
    {
        // 更安全的单例检测 —— 不依赖 PlayerItemController
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
