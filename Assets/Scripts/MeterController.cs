using UnityEngine;
using UnityEngine.UI;

// Drains the four Partner Hour meters two ways:
// 1. Continuously, every frame, at a rate set by ChapterData and shaped
//    by whatever choice was made at the main task (ApplyChoice).
// 2. In one-off steps, whenever a BAU task (dishes, stove, etc) is
//    completed (ApplyCost).
// No numbers are shown to the player, only the bars.
public class MeterController : MonoBehaviour
{
    [Header("Drag the four sliders here")]
    public Slider body;
    public Slider mind;
    public Slider feelingLikeAPerson;
    public Slider mask;

    // True once any meter has hit zero. GameManager polls this to know
    // when Partner Hour is over.
    public bool IsDepleted { get; private set; }

    [Header("Pacing (playtest: the meters ran out before players learned the chapter)")]
    [Tooltip("Scales every chapter's drain. 1 = the speeds in ChapterData; 0.6 = 40% slower.")]
    public float drainScale = 0.6f;
    [Tooltip("The drain starts at zero and builds up to full speed over this many seconds, so players can read the scene and find the main task first.")]
    public float rampUpSeconds = 15f;
    float drainingFor;

    MeterRates baseDrain;
    MeterRates currentMultiplier = new MeterRates(1f, 1f, 1f, 1f);
    bool draining = false;

    // Call this once, at the start of the chapter, before StartDraining.
    public void Configure(MeterRates chapterBaseDrain)
    {
        baseDrain = chapterBaseDrain;
    }

    // Call this when the Partner Hour starts
    public void StartDraining()
    {
        ResetMeters();
        currentMultiplier = new MeterRates(1f, 1f, 1f, 1f);
        drainingFor = 0f;
        draining = true;
    }

    // Call this when the Partner Hour ends
    public void StopDraining()
    {
        draining = false;
    }

    public void ResetMeters()
    {
        body.value = 1f;
        mind.value = 1f;
        feelingLikeAPerson.value = 1f;
        mask.value = 1f;
        IsDepleted = false;
    }

    // Applies the chosen PartnerChoice's multipliers to the continuous
    // drain rate for the rest of Partner Hour.
    public void ApplyChoice(PartnerChoice choice)
    {
        currentMultiplier = choice.multipliers;
    }

    // Applies a one-off reduction, e.g. from completing a BAU task.
    // Clamped at 0, same as the continuous drain.
    public void ApplyCost(MeterRates cost)
    {
        body.value = Mathf.Max(0f, body.value - cost.body);
        mind.value = Mathf.Max(0f, mind.value - cost.mind);
        feelingLikeAPerson.value = Mathf.Max(0f, feelingLikeAPerson.value - cost.feelingLikeAPerson);
        mask.value = Mathf.Max(0f, mask.value - cost.mask);
        RecomputeDepleted();
    }

    void Update()
    {
        if (!draining) return;

        drainingFor += Time.deltaTime;
        float ramp = rampUpSeconds > 0f ? Mathf.Clamp01(drainingFor / rampUpSeconds) : 1f;
        float dt = Time.deltaTime * drainScale * ramp;
        body.value -= baseDrain.body * currentMultiplier.body * dt;
        mind.value -= baseDrain.mind * currentMultiplier.mind * dt;
        feelingLikeAPerson.value -= baseDrain.feelingLikeAPerson * currentMultiplier.feelingLikeAPerson * dt;
        mask.value -= baseDrain.mask * currentMultiplier.mask * dt;

        RecomputeDepleted();
    }

    void RecomputeDepleted()
    {
        IsDepleted = body.value <= 0f || mind.value <= 0f
                  || feelingLikeAPerson.value <= 0f || mask.value <= 0f;
    }
}