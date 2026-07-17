using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DS.ScriptableObjects;
using Finale;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DSDialogueRunner : MonoBehaviour
{
    public event Action<int> OnChoiceSelectedEvent;

    [Header("Dialogue Data")]
    public DSDialogueContainerSO dialogueContainer;

    [Tooltip("First time welcome dialogue")]
    public DSDialogueSO startDialogue;

    [Tooltip("Repeated welcome dialogue (after first time)")]
    public DSDialogueSO repeatDialogue;

    [Header("UI")]
    public DSDialogueUI ui;

    [Header("Events")]
    public UnityEvent OnDialogueFinished;

    public bool IsDialogueRunning { get; private set; }

    private DSDialogueSO _currentDialogue;
    private readonly List<DSDialogueSO> _all = new();

    private bool waitingForContinue;
    private bool isWelcome;
    private Sprite _currentPortrait;

    // ─────────────────────────────
    // UNITY
    // ─────────────────────────────

    private void Awake()
    {
        CollectAll();
    }

    private void Start()
    {
        AzzyScript azzy = FindFirstObjectByType<AzzyScript>();

        _currentPortrait = azzy.GetCurrentSprite();
        
        // Hide UI by default
        ui.Hide();
        Time.timeScale = 1f;

        if (dialogueContainer == null)
            return;

        string rawName = dialogueContainer.name;
        string name = rawName.ToLower();

        isWelcome = name.Contains("welcome");

        // Non-welcome dialogues do not auto-play
        if (!isWelcome)
            return;

        // Welcome dialogue must wait until all tutorials are completed
        if (!AreAllTutorialsDone())
            return;

        // PlayerPrefs key
        string playedKey = $"WelcomePlayed_{name}";
        bool hasPlayedWelcome = PlayerPrefs.GetInt(playedKey, 0) == 1;

        if (!hasPlayedWelcome)
        {
            // ⭐ First time → play welcome dialogue
            if (startDialogue == null)
                startDialogue = _all.Find(d => d.IsStartingDialogue);

            PlayerPrefs.SetInt(playedKey, 1);
            PlayerPrefs.Save();
        }
        else
        {
            // 🔁 After first time → always play repeat dialogue
            if (repeatDialogue == null)
                return;

            startDialogue = repeatDialogue;
        }

        StartConversation();
    }
    public void SetPortrait(Sprite portrait)
    {
        _currentPortrait = portrait;

        if (ui != null)
            ui.SetPortrait(portrait);
    }


    // ─────────────────────────────
    // CONVERSATION FLOW
    // ─────────────────────────────

    public void StartConversation()
    {
        IsDialogueRunning = true;

        CollectAll();

        if (startDialogue == null)
        {
            EndConversation();
            return;
        }

        ShowDialogue(startDialogue);
    }

    private void ShowDialogue(DSDialogueSO dialogue)
    {
        if (dialogue == null)
            return;

        Canvas c = GetComponentInParent<Canvas>();
        if (c != null)
            c.sortingOrder = 999;

        _currentDialogue = dialogue;

        ui.ShowLine(dialogue.Text);
        ui.SetPortrait(_currentPortrait);   // ⭐ NPC头像

        int count = dialogue.Choices?.Count ?? 0;

        if (count <= 1)
        {
            waitingForContinue = true;
            ui.HideChoices();
        }
        else
        {
            waitingForContinue = false;
            ui.ShowChoices(dialogue.Choices, OnChoiceSelected);
        }
    }


    private void Update()
    {
        bool isFading = SceneFadeManager.Instance.sceneFadeOnLoadPublic.isFading;
        if (!waitingForContinue || isFading)
            return;

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetMouseButtonDown(0) ||
            Input.GetKeyDown(KeyCode.Return))
        {
            waitingForContinue = false;
            ContinueWithoutChoice();
        }
    }

    private void ContinueWithoutChoice()
    {
        var choices = _currentDialogue.Choices;

        if (choices == null || choices.Count == 0)
        {
            EndConversation();
            return;
        }

        var next = choices[0].NextDialogue;

        if (next == null)
        {
            EndConversation();
            return;
        }

        ShowDialogue(next);
    }

    private void OnChoiceSelected(int index)
    {
        OnChoiceSelectedEvent?.Invoke(index);

        var choices = _currentDialogue?.Choices;
        if (choices == null || index < 0 || index >= choices.Count)
        {
            EndConversation();
            return;
        }

        var next = choices[index].NextDialogue;

        if (next == null)
        {
            EndConversation();
            return;
        }

        ShowDialogue(next);
    }

    // ─────────────────────────────
    // END / FORCE END
    // ─────────────────────────────

    private void EndConversation()
    {
        ui.Hide();
        IsDialogueRunning = false;

        _currentDialogue = null;
        waitingForContinue = false;

        // Only trigger finish event for welcome dialogues
        if (isWelcome)
            OnDialogueFinished?.Invoke();
    }

    public void ForceEndDialogue()
    {
        EndConversation();
    }

    public bool CanStartDialogue()
    {
        return !IsDialogueRunning;
    }

    // ─────────────────────────────
    // DATA
    // ─────────────────────────────

    private void CollectAll()
    {
        _all.Clear();

        if (dialogueContainer == null)
            return;

        foreach (var d in dialogueContainer.UngroupedDialogues)
            if (d != null)
                _all.Add(d);

        foreach (var kv in dialogueContainer.DialogueGroups)
            foreach (var d in kv.Value)
                if (d != null)
                    _all.Add(d);
    }

    private bool AreAllTutorialsDone()
    {
        return PlayerPrefs.GetInt("FarmTutorialDone", 0) == 1
            && PlayerPrefs.GetInt("ShopTutorialDone", 0) == 1
            && PlayerPrefs.GetInt("CookingTutorialDone", 0) == 1;
    }

#if UNITY_EDITOR
    [MenuItem("Tools/Dialogue/Reset Welcome Dialogues")]
    public static void ResetWelcomeDialogues()
    {
        PlayerPrefs.DeleteKey("WelcomePlayed_welcomefarm");
        PlayerPrefs.DeleteKey("WelcomePlayed_welcometavern");
        PlayerPrefs.Save();

        Debug.Log("[Dialogue] Welcome dialogue PlayerPrefs cleared");
    }

    [MenuItem("Tools/Dialogue/Show Welcome Dialogue Keys")]
    public static void ShowWelcomeDialogueKey()
    {
        Debug.Log($"WelcomePlayed_welcomefarm = {PlayerPrefs.GetInt("WelcomePlayed_welcomefarm", 0)} (0=not done, 1=completed)");
        Debug.Log($"WelcomePlayed_welcometavern = {PlayerPrefs.GetInt("WelcomePlayed_welcometavern", 0)} (0=not done, 1=completed)");
    }
#endif
}
