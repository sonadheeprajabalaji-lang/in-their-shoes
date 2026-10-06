using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    [Tooltip("Optional. The card shown after each Self Hour response (impact, facts, how to help). Created automatically if left empty.")]
    public ResponseFeedbackUI responseFeedback;

    [Tooltip("Optional. The screen that explains the meters before the Partner Hour (see ChapterData.showMeterIntro). Created automatically if left empty.")]
    public MeterIntroUI meterIntro;

    [Header("End")]
    public ResponseLogger logger;
    public Button restartButton;

    [Header("Chapter transition")]
    [Tooltip("Exact scene name to load when Next Chapter is pressed, e.g. \"Chapter2\". Must be added to File > Build Settings > Scenes in Build, or LoadScene will fail. Leave blank on the last chapter -- nextChapterButton hides itself automatically when this is empty.")]
    public string nextSceneName;
    [Tooltip("Optional. Shown on the End panel. Hidden automatically if nextSceneName is blank.")]
    public Button nextChapterButton;

    int partnerChoice = -1;          // -1 = no choice made
    InteractableTask currentCue;     // the cue currently shown in the response popup, or null
    float responseShownTime;

    void Start()
    {
        // Chapter 1 is where a playthrough begins, so this is where any
        // leftover carryover from a previous playthrough gets cleared.
        // Chapter 2+ must NOT do this -- they're meant to consume what
        // the chapter before them earned.
        if (chapterData.chapterNumber == 1)
        {
            CarryoverState.Reset();
        }

        continueButton.onClick.AddListener(LeaveContentWarning);
        restartButton.onClick.AddListener(ShowContentWarning);

        if (nextChapterButton != null)
        {
            bool hasNextChapter = !string.IsNullOrEmpty(nextSceneName);
            nextChapterButton.gameObject.SetActive(hasNextChapter);
            if (hasNextChapter)
            {
                nextChapterButton.onClick.AddListener(LoadNextChapter);
            }
        }

        partnerHourController.ChoiceMade += OnChoiceMade;
        selfHourController.CueInteracted += OnCueInteracted;
        selfHourController.AllCuesResolved += OnAllCuesResolved;

        SetUpResponseButtons();

        if (meterIntro == null && chapterData.showMeterIntro)
        {
            meterIntro = new GameObject("MeterIntro").AddComponent<MeterIntroUI>();
        }

        if (responseFeedback == null)
        {
            responseFeedback = new GameObject("ResponseFeedback").AddComponent<ResponseFeedbackUI>();
        }

        if (contentWarningText != null)
        {
            contentWarningText.text = chapterData.contentWarningText;
        }

        ShowContentWarning();
    }

    void LoadNextChapter()
    {
        if (string.IsNullOrEmpty(nextSceneName)) return;
        SceneManager.LoadScene(nextSceneName);
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

    // Enter works on the content warning as well as the Continue button.
    void Update()
    {
        if (contentWarningPanel != null && contentWarningPanel.activeInHierarchy
            && (meterIntro == null || !meterIntro.IsShowing)
            && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            LeaveContentWarning();
        }
    }

    void LeaveContentWarning()
    {
        if (meterIntro != null && meterIntro.IsShowing) return;   // already on the explainer

        if (chapterData.showMeterIntro && meterIntro != null)
        {
            meterIntro.Show(chapterData.chapterNumber, chapterData.meterIntroNote, StartPartnerHour);
            return;
        }
        StartPartnerHour();
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
        var response = chapterData.responses[index];
        responsePanel.SetActive(false);

        if (responseFeedback != null && response.HasCard)
        {
            // Show what the choice means for her before moving on. The cue
            // stays "current" until the card closes, so no other cue can
            // open underneath it, and the player can't walk away.
            selfHourController.player.InputLocked = true;
            responseFeedback.Show(responseText, chapterData.learnHeading, response, () =>
            {
                int readMs = Mathf.RoundToInt(responseFeedback.LastReadSeconds * 1000f);
                logger.Log(chapterData.chapterNumber, "SelfHourCard", choiceText, responseText, readMs);

                selfHourController.player.InputLocked = false;
                FinishCue(resolvedCue);
            });
            return;
        }

        FinishCue(resolvedCue);
    }

    void FinishCue(InteractableTask resolvedCue)
    {
        currentCue = null;
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