using UnityEngine;
using UnityEngine.UI;

public class PortraitAutoHide : MonoBehaviour
{
    private Image img;

    private void Awake()
    {
        img = GetComponent<Image>();
        UpdateVisibility();
    }

    private void Update()
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        // 当 sprite == null → 隐藏
        gameObject.SetActive(img.sprite != null);
    }
}
