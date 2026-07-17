using UnityEngine;

namespace Tutorial
{
    public class TutorialResetter: MonoBehaviour
    {
        /// <summary>
        /// Resets all tutorial and welcome dialogue PlayerPrefs keys.
        /// Call this from your settings menu.
        /// </summary>
        public static void ResetAllTutorials()
        {
            // Tutorial keys
            PlayerPrefs.DeleteKey("CookingTutorialDone");
            PlayerPrefs.DeleteKey("FarmTutorialDone");
            PlayerPrefs.DeleteKey("ShopTutorialDone");

            // Welcome dialogue keys
            PlayerPrefs.DeleteKey("WelcomePlayed_welcomefarm");
            PlayerPrefs.DeleteKey("WelcomePlayed_welcometavern");

            PlayerPrefs.DeleteKey("FarmFinaleCompleted");
            PlayerPrefs.DeleteKey("CookingFinaleCompleted");
            
            PlayerPrefs.Save();

            Debug.Log("[TutorialResetter] All tutorials and welcome dialogues reset");
        }
    }
}