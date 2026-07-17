using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Orders;

public class EndOfDayUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject endOfDayPanel;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI dayCounterText;
    [SerializeField] private TextMeshProUGUI customersServedText;
    [SerializeField] private TextMeshProUGUI ordersCompletedText;
    [SerializeField] private TextMeshProUGUI ordersFailedText;
    [SerializeField] private TextMeshProUGUI moneyEarnedText;
    [SerializeField] private Button endDayButton;

    private void Start()
    {
        if (endOfDayPanel != null)
        {
            endOfDayPanel.SetActive(false);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (endDayButton != null)
        {
            endDayButton.onClick.AddListener(OnEndDayButtonClicked);
        }
    }

    public void ShowEndOfDayScreen(DayStats stats, float transitionDuration)
    {
        Debug.Log($"[EndOfDayUI] ShowEndOfDayScreen called. EndOfDayPanel active: {endOfDayPanel.activeSelf}");
        UpdateUI(stats);
        StartCoroutine(FadeInWithTimeSlowdown(transitionDuration));
    }

    private void UpdateUI(DayStats stats)
    {
        if (dayCounterText != null)
            dayCounterText.text = $"Day: {stats.currentDay}";

        if (customersServedText != null)
            customersServedText.text = $"{stats.customersServed}";

        if (ordersCompletedText != null)
            ordersCompletedText.text = $"{stats.ordersCompleted}";

        if (ordersFailedText != null)
            ordersFailedText.text = $"{stats.ordersFailed}";

        if (moneyEarnedText != null)
            moneyEarnedText.text = $"{stats.moneyEarned} G";
    }

    private IEnumerator FadeInWithTimeSlowdown(float duration)
    {
        endOfDayPanel.SetActive(true);

        float elapsed = 0f;
        float startTimeScale = Time.timeScale;

        while (elapsed < duration)
        {
            endOfDayPanel.SetActive(true);
            
            // Use unscaled delta time to ensure consistent timing
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / duration;

            // Fade in of the UI
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            }

            // slow down time
            Time.timeScale = Mathf.Lerp(startTimeScale, 0f, progress);

            yield return null;
        }

        // assure final values are set
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
        Time.timeScale = 0f;

        // allow interaction
        if (canvasGroup != null)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        // Ensure the EndOfDayPanel is active at the end
        endOfDayPanel.SetActive(true);
    }

    private void OnEndDayButtonClicked()
    {
        if (EndOfDayManager.Instance != null)
        {
            EndOfDayManager.Instance.EndDayAndGoToFarm();
        }
    }

    private void OnDestroy()
    {
        if (endDayButton != null)
        {
            endDayButton.onClick.RemoveListener(OnEndDayButtonClicked);
        }
    }
}
