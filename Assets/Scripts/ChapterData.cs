using UnityEngine;

// Holds everything that makes one chapter different from another:
// the content warning, how long Partner Hour runs, the meter drain
// rates, the two Partner Hour choices, and the Self Hour responses.
//
// Create one of these per chapter via Assets > Create > In Their Shoes > Chapter Data,
// fill it in in the Inspector, then drag it onto GameManager's Chapter Data field.
[CreateAssetMenu(fileName = "Chapter_", menuName = "In Their Shoes/Chapter Data")]
public class ChapterData : ScriptableObject
{
    [Header("Identity")]
    public int chapterNumber = 1;
    public string chapterTitle;

    [Tooltip("Shown on the context cutscene, e.g. \"Week 5, postpartum\"")]
    public string weekContext;

    [Header("Content warning")]
    [TextArea(2, 4)]
    public string contentWarningText;

    [Header("Partner Hour")]
    [Tooltip("How long Partner Hour runs before Self Hour begins")]
    public float partnerHourSeconds = 20f;

    [Tooltip("How fast each meter drains per second before any choice multiplier is applied")]
    public MeterRates baseDrain = new MeterRates(0.03f, 0.03f, 0.02f, 0.03f);

    [Tooltip("Exactly two, matching the two Partner Hour buttons in the scene")]
    public PartnerChoice[] choices;

    [Tooltip("The taskId (see InteractableTask) whose interaction triggers the Partner Hour choice popup")]
    public string choiceTriggerTaskId;

    [Header("Self Hour")]
    [Tooltip("Up to as many responses as there are response buttons in the scene. Self Hour now shows one prompt per unfinished BAU task automatically, so there is no fixed cue to set here anymore.")]
    public SelfHourResponse[] responses;
}

// The four Partner Hour meters, used both as base drain rates and as
// per-choice multipliers on those rates.
[System.Serializable]
public struct MeterRates
{
    public float body;
    public float mind;
    public float feelingLikeAPerson;
    public float mask;

    public MeterRates(float body, float mind, float feelingLikeAPerson, float mask)
    {
        this.body = body;
        this.mind = mind;
        this.feelingLikeAPerson = feelingLikeAPerson;
        this.mask = mask;
    }

    public static MeterRates Multiply(MeterRates a, MeterRates b)
    {
        return new MeterRates(
            a.body * b.body,
            a.mind * b.mind,
            a.feelingLikeAPerson * b.feelingLikeAPerson,
            a.mask * b.mask);
    }
}

[System.Serializable]
public class PartnerChoice
{
    public string label;

    [Tooltip("1 = unchanged from base drain, above 1 = drains faster, below 1 = drains slower. The drain is never fully stopped.")]
    public MeterRates multipliers = new MeterRates(1f, 1f, 1f, 1f);
}

[System.Serializable]
public class SelfHourResponse
{
    public string label;

    [Tooltip("Multiplies the drain rate for each meter at the START of the next chapter only, then resets. 1 = no effect, below 1 = eases that meter, above 1 = makes it worse. Give different responses different meters to ease, so no single response is simply \"better\".")]
    public MeterRates nextChapterEase = new MeterRates(1f, 1f, 1f, 1f);
}