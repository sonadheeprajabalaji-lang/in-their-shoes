using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Runs a chapter's flow, driven by a ChapterData asset:
// Content warning -> Partner Hour (roam the room; holding the choice
// trigger task to completion or letting go early decides the choice) ->
// Self Hour (roam the same room, only the seeded cue task is unlocked) ->
// End.
public class GameManager : MonoBehaviour
{
    [Header("Chapter data")]
    public ChapterData chapterData;

    [Header("Panels")]
    public GameObject contentWarningPanel;
    public GameObject partnerHourPanel;   // the room + meters HUD
    public GameObject selfHourPanel;      // the room
    public GameObject responsePanel;      // shown once the seeded cue task is interacted with
    public GameObject endPanel;

    [Header("Content warning")]
    public Button continueButton;
    [Tooltip("Optional. If assigned, filled from chapterData.contentWarningText on Start.")]
    public TMP_Text contentWarningText;

    [Header("Partner Hour")]
    public MeterController meters;
    public PartnerHourController partnerHourController;
    public TaskChecklistUI checklist;

    [Header("Self Hour")]
    public SelfHourController selfHourController;
    public Button[] responseButtons;   // sized for the chapter with the most responses

    [Header("End")]
    public ResponseLogger logger;
    public Button restartButton;

    int partnerChoice = -1;      // -1 = no choice made
    float responseShownTime;
    bool responded;

    void Start()
    {
        continueButton.onClick.AddListener(StartPartnerHour);
        restartButton.onClick.AddListener(ShowContentWarning);

        partnerHourController.ChoiceMade += OnChoiceMade;
        selfHourController.CueInteracted += OnCueInteracted;

        SetUpResponseButtons();

        if (contentWarningText != null)
        {
            contentWarningText.text = chapterData.contentWarningText;
        }

        ShowContentWarning();
    }

    void SetUpResponseButtons()
    {
        var responses = chapterData.responses;

        for (int i = 0; i < responseButtons.Length; i++)
        {
            int index = i; // needed so each button remembers its own number
            responseButtons[i].onClick.AddListener(() => OnResponse(index));

            bool hasResponse = responses != null && i < responses.Length;
            responseButtons[i].gameObject.SetActive(hasResponse);
            if (hasResponse)
            {
                responseButtons[i].GetComponentInChildren<TMP_Text>(true).text = responses[i].label;
            }
        }
    }

    // ---------- Flow ----------

    void ShowContentWarning()
    {
        ShowOnly(contentWarningPanel);
        responsePanel.SetActive(false);
    }

    void StartPartnerHour()
    {
        partnerChoice = -1;
        responded = false;

        ShowOnly(partnerHourPanel);
        meters.Configure(chapterData.baseDrain);
        meters.StartDraining();
        partnerHourController.BeginRoaming(chapterData);
        checklist.BuildList();
        StartCoroutine(PartnerHourTimer());
    }

    IEnumerator PartnerHourTimer()
    {
        // Partner Hour normally ends because a meter hits zero, not
        // because time runs out. partnerHourSeconds is a generous safety
        // cap, not the usual way the hour ends.
        float elapsed = 0f;
        while (elapsed < chapterData.partnerHourSeconds && !meters.IsDepleted)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        meters.StopDraining();
        partnerHourController.EndRoaming();
        checklist.Clear();
        StartSelfHour();
    }

    void StartSelfHour()
    {
        ShowOnly(selfHourPanel);
        responsePanel.SetActive(false);
        selfHourController.BeginRoaming(chapterData);
    }

    // ---------- Player actions ----------

    void OnChoiceMade(int choice)
    {
        if (partnerChoice != -1) return;   // first outcome is final
        if (chapterData.choices == null || choice >= chapterData.choices.Length) return;

        partnerChoice = choice;
        meters.ApplyChoice(chapterData.choices[choice]);
    }

    void OnCueInteracted()
    {
        responsePanel.SetActive(true);
        responseShownTime = Time.time;
    }

    void OnResponse(int index)
    {
        if (responded) return;
        responded = true;

        int ms = Mathf.RoundToInt((Time.time - responseShownTime) * 1000f);
        string choiceText = partnerChoice >= 0 ? chapterData.choices[partnerChoice].label : "none";
        string responseText = chapterData.responses[index].label;

        logger.Log(chapterData.chapterNumber, "SelfHour", choiceText, responseText, ms);

        selfHourController.EndRoaming();
        ShowOnly(endPanel);
    }

    // ---------- Helper ----------

    void ShowOnly(GameObject panel)
    {
        contentWarningPanel.SetActive(panel == contentWarningPanel);
        partnerHourPanel.SetActive(panel == partnerHourPanel);
        selfHourPanel.SetActive(panel == selfHourPanel);
        endPanel.SetActive(panel == endPanel);
    }
}