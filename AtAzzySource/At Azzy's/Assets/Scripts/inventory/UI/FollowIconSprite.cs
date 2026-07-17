using UnityEngine;
using UnityEngine.UI;

public class FollowIconSprite : MonoBehaviour
{
    [SerializeField] private Image icon;
    private Image qImage;

    void Awake()
    {
        qImage = GetComponent<Image>();
    }

    void Update()
    {
        if (icon == null || qImage == null)
            return;

        bool visible =
            icon.gameObject.activeInHierarchy &&
            icon.enabled &&
            icon.sprite != null &&
            icon.color.a > 0.01f;

        qImage.enabled = visible;   
    }
}
