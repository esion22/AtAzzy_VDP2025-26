namespace GoogleSheet
{
    /// <summary>
    /// Data structure to hold player statistics for Google Sheets.
    /// </summary>
    [System.Serializable]
    public class Data
    {
        public int playerId;
        public int numberOfSeedsPlanted;
        public int numberOfSeedsFailed;
        public int numberOfRecipesDone;
        public int numberOfRecipesFailed;
        public float timeSpentInFarm;
        public float timeSpentInTavern;
    }
}