using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Runs a chapter's flow, driven by a ChapterData asset:
// Content warning -> Partner Hour (roam, do BAU tasks, the main task's
// hold decides the choice; ends the moment a meter hits zero) -> Self
// Hour (roam the same room; every BAU task left unfinished becomes its
// own cue, answered one at a time) -> End.
//
// Each Self Hour response also feeds CarryoverState, which eases (or
// worsens) specific meters at the start of the NEXT chapter only.
public class GameManager : MonoBehaviour
{
    [Header("Chapter data")]
    public ChapterData chapterData;

    [Header("Panels")]
    public GameObject contentWarningPanel;
    public GameObject partnerHourPanel;   // the room + meters HUD
    public GameObject selfHourPanel;      // the room
    public GameObject responsePanel;      // shown for whichever cue is currently up
    public GameObject endPanel;

    [Header("Content warning")]
    public Button continueButton;
    [Tooltip("Optional. If assigned, filled from chapterData.contentWarningText on Start.")]
    public TMP_Text contentWarningText;

    [Header("Partner Hour")]
    public MeterController meters;
    public PartnerHourController partnerHourController;
    public TaskChecklistUI checklist;
    public JuggleController juggle;

    [Header("Self Hour")]
    public SelfHourController selfHourController;
    public Button[] responseButtons;   // sized for the chapter with the most responses

    [Header("End")]
    public ResponseLogger logger;
    public Button restartButton;

    int partnerChoice = -1;          // -1 = no choice made
    InteractableTask currentCue;     // the cue currently shown in the response popup, or null
    float responseShownTime;

    void Start()
    {
        continueButton.onClick.AddListener(StartPartnerHour);
        restartButton.onClick.AddListener(ShowContentWarning);

        partnerHourController.ChoiceMade += OnChoiceMade;
        selfHourController.CueInteracted += OnCueInteracted;
        selfHourController.AllCuesResolved += OnAllCuesResolved;

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
        currentCue = null;
        ShowOnly(contentWarningPanel);
        responsePanel.SetActive(false);
    }

    void StartPartnerHour()
    {
        partnerChoice = -1;

        ShowOnly(partnerHourPanel);

        MeterRates carry = CarryoverState.ConsumeForNextChapter();
        meters.Configure(MeterRates.Multiply(chapterData.baseDrain, carry));
        meters.StartDraining();

        partnerHourController.BeginRoaming(chapterData);
        checklist.BuildList();
        juggle.BeginRoaming(chapterData);
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
        juggle.EndRoaming();
        StartSelfHour();
    }

    void StartSelfHour()
    {
        ShowOnly(selfHourPanel);
        responsePanel.SetActive(false);
        currentCue = null;
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

    void OnCueInteracted(InteractableTask cue)
    {
        if (currentCue != null) return;   // already answering one cue at a time

        currentCue = cue;
        responsePanel.SetActive(true);
        responseShownTime = Time.time;
    }

    void OnResponse(int index)
    {
        if (currentCue == null) return;   // no cue is currently awaiting a response
        if (chapterData.responses == null || index >= chapterData.responses.Length) return;

        int ms = Mathf.RoundToInt((Time.time - responseShownTime) * 1000f);
        string choiceText = partnerChoice >= 0 ? chapterData.choices[partnerChoice].label : "none";
        string responseText = chapterData.responses[index].label;

        logger.Log(chapterData.chapterNumber, "SelfHour", choiceText, responseText, ms);
        CarryoverState.AddEase(chapterData.responses[index].nextChapterEase);

        var resolvedCue = currentCue;
        currentCue = null;
        responsePanel.SetActive(false);

        selfHourController.MarkCueResolved(resolvedCue);
        // If more cues are still pending, OnCueInteracted fires again
        // when the player reaches the next one. If that was the last
        // one, OnAllCuesResolved fires instead.
    }

    void OnAllCuesResolved()
    {
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