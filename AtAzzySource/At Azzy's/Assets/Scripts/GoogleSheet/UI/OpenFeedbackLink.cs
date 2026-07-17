using System.Collections;
using UnityEditor;
using UnityEngine;

/// <summary>
/// This script is responsible for opening the feedback Google Form link when the button is clicked and
/// sending data to Google Sheets.
/// </summary>
public class FeedbackButton : MonoBehaviour
{
    public string googleFormUrl =
        "https://docs.google.com/forms/d/e/1FAIpQLScnpo84CoJkTNeHy1m2WS36ImuXiryiUFmGzZX16LJ6pI4oZA/viewform?usp=pp_url&entry.615266980={0}";

    /// <summary>
    /// Opens the Google Form URL in the default web browser and sends data to Google Sheets.
    /// </summary>
    public void OpenForm()
    {
        int playerId = DataManager.Instance.playerId;

        SendDataToSheet sender = FindFirstObjectByType<SendDataToSheet>();
        sender.SendData(
            playerId,
            DataManager.Instance.numberOfSeedsPlanted,
            DataManager.Instance.numberOfSeedsFailed,
            DataManager.Instance.numberOfRecipesDone,
            DataManager.Instance.numberOfRecipesFailed,
            DataManager.Instance.timeSpentInFarm,
            DataManager.Instance.timeSpentInTavern);

        string finalUrl = string.Format(googleFormUrl, playerId);
        Application.OpenURL(finalUrl);
        Debug.Log("Opening URL: " + finalUrl);
        StartCoroutine(QuitAfterDelay(3f));
    }


    /// <summary>
    /// Quits the application after a specified delay.
    /// </summary>
    /// <param name="delay">Delay in seconds before quitting the application.</param>
    /// <returns> IEnumerator for coroutine.</returns>
    IEnumerator QuitAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Debug.Log("Quit game");

        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}