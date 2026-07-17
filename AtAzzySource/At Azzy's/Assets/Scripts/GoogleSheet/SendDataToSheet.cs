using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Globalization;
using GoogleSheet;

/// <summary>
/// This class is responsible for sending player data to a Google Sheet via a Google Apps Script web app.
/// </summary>
public class SendDataToSheet : MonoBehaviour
{
    private static SendDataToSheet instance;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// It initializes the singleton instance.
    /// </summary>
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Sends player data to the Google Sheet.
    /// </summary>
    /// <param name="timestamp">Timestamp of the data submission</param>
    /// <param name="playerId">Player's unique identifier</param>
    /// <param name="numberOfSeedsPlanted">Number of seeds planted by the player</param>
    /// <param name="numberOfSeedsFailed">Number of seeds which did not grow after clicking next day button</param>
    /// <param name="numberOfRecipesDone">Number of recipes successfully output by the stations</param>
    /// <param name="numberOfRecipesFailed">Number of recipes that were not completed successfully</param>
    /// <param name="timeSpentInFarm">Time spent in the farm scene in sec</param>
    /// <param name="timeSpentInTavern">Time spent in the tavern scene in sec</param>
    public void SendData(int playerId, int numberOfSeedsPlanted, int numberOfSeedsFailed,
        int numberOfRecipesDone,
        int numberOfRecipesFailed, float timeSpentInFarm, float timeSpentInTavern)
    {
        Data data = new Data
        {
            playerId = playerId,
            numberOfSeedsPlanted = numberOfSeedsPlanted,
            numberOfSeedsFailed = numberOfSeedsFailed,
            numberOfRecipesDone = numberOfRecipesDone,
            numberOfRecipesFailed = numberOfRecipesFailed,
            timeSpentInFarm = timeSpentInFarm,
            timeSpentInTavern = timeSpentInTavern
        };

        StartCoroutine(PostData(data));
    }

    /// <summary>
    /// Posts the data to the Google Apps Script web app.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    IEnumerator PostData(Data data)
    {
        string formId = "1FAIpQLScRw2lGkcpRUjXmiAhoBSyzn8IIHtRfNPbDBv1zrwfaFnkV7w";

        string entryPlayerId = "688159983"; 
        string entrySeedsPlanted = "970805483"; 
        string entrySeedsFailed = "2063065612";
        string entryRecipesDone = "1251664802";
        string entryRecipesFailed = "1490121837";
        string entryTimeInFarm = "319620164";
        string entryTimeInTavern = "930616864";
        
        string formUrl = $"https://docs.google.com/forms/d/e/{formId}/formResponse";

        WWWForm form = new WWWForm();
        form.AddField($"entry.{entryPlayerId}", data.playerId);
        form.AddField($"entry.{entrySeedsPlanted}", data.numberOfSeedsPlanted.ToString());
        form.AddField($"entry.{entrySeedsFailed}", data.numberOfSeedsFailed.ToString());
        form.AddField($"entry.{entryRecipesDone}", data.numberOfRecipesDone.ToString());
        form.AddField($"entry.{entryRecipesFailed}", data.numberOfRecipesFailed.ToString());
        form.AddField($"entry.{entryTimeInFarm}", Math.Round(data.timeSpentInFarm, 2).ToString(CultureInfo.InvariantCulture));
        form.AddField($"entry.{entryTimeInTavern}", Math.Round(data.timeSpentInTavern, 2).ToString(CultureInfo.InvariantCulture));

        Debug.Log("Sending data to Google Forms...");

        using (UnityWebRequest www = UnityWebRequest.Post(formUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Success");
            }
            else
            {
                Debug.Log($"Error: {www.error} | Code: {www.responseCode}");
            }
        }
    }
}