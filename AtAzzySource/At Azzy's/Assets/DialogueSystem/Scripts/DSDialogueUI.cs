using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DS.Data;
using System.Linq;
using Finale;

public class DSDialogueUI : MonoBehaviour
{
    public GameObject root;
    public TMP_Text dialogueText;
    public Transform choicesRoot;
    public Button choiceButtonPrefab;
    public GameObject dialogueSpaceImage;

    private readonly List<Button> spawned = new();
    public GameObject[] hudElements;
    public Image portraitImage; //npc portrait
    private Sprite defaultPortrait; //set by setting image in scene
    [SerializeField] private Sprite dragonDefaultPortrait;
    [SerializeField] private Sprite humanDefaultPortrait;


    private void Awake()
    {
        if (portraitImage != null)
            defaultPortrait = portraitImage.sprite;
    }

    public void SetHUDVisible(bool visible)
    {
        foreach (var element in hudElements)
        {
            element.SetActive(visible);
        }
    }

    public void ShowLine(string text)
    {
        root.SetActive(true);
        // Time.timeScale = 0f;
        UpdateDefaultPortraitFromAzzy();
        SetHUDVisible(false);

        dialogueText.text = text;
        HideChoices();
        
        // Show the dialogue space image when showing a line
        if (dialogueSpaceImage != null)
            dialogueSpaceImage.SetActive(true);
    }

    public void SetPortrait(Sprite portrait)
    {
        if (portraitImage == null) return;

        if (portrait == null)
        {
            portraitImage.sprite = defaultPortrait;
        }
        else
        {
            portraitImage.gameObject.SetActive(true);
            portraitImage.sprite = portrait;
        }
    }
    private void UpdateDefaultPortraitFromAzzy()
    {
        if (portraitImage == null) return;

        AzzyScript azzy = FindFirstObjectByType<AzzyScript>();

        portraitImage.sprite = azzy.GetCurrentSprite();
    }





    public void ShowChoices(List<DSDialogueChoiceData> choices, Action<int> onSelected)
    {
        Time.timeScale = 0f;

        SetHUDVisible(false);

        HideChoices();

        // Hide the dialogue space image when showing choices
        if (dialogueSpaceImage != null)
            dialogueSpaceImage.SetActive(false);
        for (int i = 0; i < choices.Count; i++)
        {
            var btn = Instantiate(choiceButtonPrefab, choicesRoot);
            spawned.Add(btn);

            TMP_Text t = btn.GetComponentInChildren<TMP_Text>();
            t.text = choices[i].Text;

            int idx = i;
            btn.onClick.AddListener(() => onSelected(idx));
        }
    }

    public void HideChoices()
    {
        foreach (var b in spawned)
            Destroy(b.gameObject);
        
        spawned.Clear();
    }

    public void Hide()
    {
        // Debug.Log("[DSDialogueUI] Hide() called, root->false");

        // Check if the shop UI is active; if so, do not reset time scale
        GameObject shopUI =
            GameObject.FindObjectsByType<ShopPanelUI>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            )[0].gameObject;

        if (shopUI != null && shopUI.activeInHierarchy)
        {
            //Time.timeScale = 0f;
        }
        else
        {
            //Time.timeScale = 1f;
        }

        SetHUDVisible(true);
        root.SetActive(false);
    }
}
