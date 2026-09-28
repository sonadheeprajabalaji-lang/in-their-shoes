using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Runs a chapter's flow, driven by a ChapterData asset:
// Content warning -> Partner Hour (roam the room, then a choice popup at
// the trigger task) -> Self Hour (roam the same room, only the seeded cue
// task is unlocked, interacting with it opens the response popup) -> End.
public class GameManager : MonoBehaviour
{
    [Header("Chapter data")]
    [Tooltip("Drag the chapter's ChapterData asset here")]
    public ChapterData chapterData;

    [Header("Panels")]
    public GameObject contentWarningPanel;
    public GameObject partnerHourPanel;   // the room + meters HUD, visible for the whole Partner Hour phase
    public GameObject choicePopup;        // shown only once the choice trigger task is interacted with
    public GameObject selfHourPanel;      // the room, visible for the whole Self Hour phase
    public GameObject responsePanel;      // shown only once the seeded cue task is interacted with
    public GameObject endPanel;

    [Header("Content warning")]
    public Button continueButton;
    [Tooltip("Optional. If assigned, filled from chapterData.contentWarningText on Start.")]
    public TMP_Text contentWarningText;

    [Header("Partner Hour")]
    public MeterController meters;
    public PartnerHourController partnerHourController;
    public Button choiceAButton;
    public Button choiceBButton;

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

        partnerHourController.ChoiceTriggerReached += OnChoiceTriggerReached;
        selfHourController.CueInteracted += OnCueInteracted;

        SetUpChoiceButtons();
        SetUpResponseButtons();

        if (contentWarningText != null)
        {
            contentWarningText.text = chapterData.contentWarningText;
        }

        ShowContentWarning();
    }

    void SetUpChoiceButtons()
    {
        // Supports exactly two Partner Hour choices, matching choiceAButton/choiceBButton
        // in the scene. A chapter needing more than two would need both this method and
        // the scene's choice popup extended.
        var choices = chapterData.choices;

        choiceAButton.onClick.AddListener(() => OnPartnerChoice(0));
        if (choices != null && choices.Length > 0)
        {
            choiceAButton.GetComponentInChildren<TMP_Text>(true).text = choices[0].label;
        }

        choiceBButton.onClick.AddListener(() => OnPartnerChoice(1));
        if (choices != null && choices.Length > 1)
        {
            choiceBButton.GetComponentInChildren<TMP_Text>(true).text = choices[1].label;
        }
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
        choicePopup.SetActive(false);
        responsePanel.SetActive(false);
    }

    void StartPartnerHour()
    {
        partnerChoice = -1;
        responded = false;
        choiceAButton.interactable = true;
        choiceBButton.interactable = true;
        choicePopup.SetActive(false);

        ShowOnly(partnerHourPanel);
        meters.Configure(chapterData.baseDrain);
        meters.StartDraining();
        partnerHourController.BeginRoaming(chapterData);
        StartCoroutine(PartnerHourTimer());
    }

    IEnumerator PartnerHourTimer()
    {
        yield return new WaitForSeconds(chapterData.partnerHourSeconds);
        meters.StopDraining();
        partnerHourController.EndRoaming();
        choicePopup.SetActive(false);
        StartSelfHour();
    }

    void StartSelfHour()
    {
        ShowOnly(selfHourPanel);
        responsePanel.SetActive(false);
        selfHourController.BeginRoaming(chapterData);
    }

    // ---------- Player actions ----------

    void OnChoiceTriggerReached()
    {
        choicePopup.SetActive(true);
    }

    void OnPartnerChoice(int choice)
    {
        if (partnerChoice != -1) return;   // only one choice allowed
        if (chapterData.choices == null || choice >= chapterData.choices.Length) return;

        partnerChoice = choice;
        meters.ApplyChoice(chapterData.choices[choice]);
        choiceAButton.interactable = false;
        choiceBButton.interactable = false;
        choicePopup.SetActive(false);
        partnerHourController.ResumeAfterChoice();
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
