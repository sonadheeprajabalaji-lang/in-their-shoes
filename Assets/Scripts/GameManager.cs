using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Runs the Chapter 1 flow:
// Content warning -> Partner Hour -> Self Hour -> End
public class GameManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject contentWarningPanel;
    public GameObject partnerHourPanel;
    public GameObject selfHourPanel;
    public GameObject endPanel;

    [Header("Content warning")]
    public Button continueButton;

    [Header("Partner Hour")]
    public MeterController meters;
    public Button choiceAButton;
    public Button choiceBButton;
    public float partnerHourSeconds = 20f;

    [Header("Self Hour")]
    public Button cueButton;
    public GameObject responsePanel;
    public Button[] responseButtons;   // 4 buttons

    [Header("End")]
    public Button restartButton;

    // Placeholder Chapter 1 content (moves into a ChapterData file later)
    readonly string[] choiceLabels =
    {
        "Mute and step away",
        "Push through the call"
    };

    readonly string[] responseLabels =
    {
        "Make something she can eat",
        "Ask how she's really feeling",
        "Clear it away quietly",
        "Carry on as normal"
    };

    int partnerChoice = -1;      // -1 = no choice made
    float responseShownTime;
    bool responded;

    void Start()
    {
        continueButton.onClick.AddListener(StartPartnerHour);
        choiceAButton.onClick.AddListener(() => OnPartnerChoice(0));
        choiceBButton.onClick.AddListener(() => OnPartnerChoice(1));
        cueButton.onClick.AddListener(OnCueClicked);
        restartButton.onClick.AddListener(ShowContentWarning);

        for (int i = 0; i < responseButtons.Length; i++)
        {
            int index = i; // needed so each button remembers its own number
            responseButtons[i].onClick.AddListener(() => OnResponse(index));

            if (i < responseLabels.Length)
            {
                responseButtons[i].GetComponentInChildren<TMP_Text>(true).text = responseLabels[i];
            }
        }

        ShowContentWarning();
    }

    // ---------- Flow ----------

    void ShowContentWarning()
    {
        ShowOnly(contentWarningPanel);
    }

    void StartPartnerHour()
    {
        partnerChoice = -1;
        responded = false;
        choiceAButton.interactable = true;
        choiceBButton.interactable = true;

        ShowOnly(partnerHourPanel);
        meters.StartDraining();
        StartCoroutine(PartnerHourTimer());
    }

    IEnumerator PartnerHourTimer()
    {
        yield return new WaitForSeconds(partnerHourSeconds);
        meters.StopDraining();
        StartSelfHour();
    }

    void StartSelfHour()
    {
        ShowOnly(selfHourPanel);
        responsePanel.SetActive(false);
        cueButton.interactable = true;
    }

    // ---------- Player actions ----------

    void OnPartnerChoice(int choice)
    {
        if (partnerChoice != -1) return;   // only one choice allowed

        partnerChoice = choice;
        meters.ApplyChoice(choice);
        choiceAButton.interactable = false;
        choiceBButton.interactable = false;
    }

    void OnCueClicked()
    {
        cueButton.interactable = false;
        responsePanel.SetActive(true);
        responseShownTime = Time.time;
    }

    void OnResponse(int index)
    {
        if (responded) return;
        responded = true;

        int ms = Mathf.RoundToInt((Time.time - responseShownTime) * 1000f);
        string choiceText = partnerChoice >= 0 ? choiceLabels[partnerChoice] : "none";

        // Temporary log. ResponseLogger will write this to a file later.
        Debug.Log("LOG chapter=1 | choice=" + choiceText +
                  " | response=" + responseLabels[index] +
                  " | ms=" + ms);

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