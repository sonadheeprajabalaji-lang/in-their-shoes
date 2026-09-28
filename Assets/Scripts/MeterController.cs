using UnityEngine;
using UnityEngine.UI;

// Drains the four Partner Hour meters over time.
// No numbers are shown to the player, only the bars.
// The player's choice changes HOW the meters drain, never WHETHER they drain.
public class MeterController : MonoBehaviour
{
    [Header("Drag the four sliders here")]
    public Slider body;
    public Slider mind;
    public Slider feelingLikeAPerson;
    public Slider mask;

    MeterRates baseDrain;
    MeterRates currentMultiplier = new MeterRates(1f, 1f, 1f, 1f);
    bool draining = false;

    // Call this once, at the start of the chapter, before StartDraining.
    // Pulls the base drain rates from the chapter's data instead of
    // having them hardcoded per-script.
    public void Configure(MeterRates chapterBaseDrain)
    {
        baseDrain = chapterBaseDrain;
    }

    // Call this when the Partner Hour starts
    public void StartDraining()
    {
        ResetMeters();
        currentMultiplier = new MeterRates(1f, 1f, 1f, 1f);
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
    }

    // Applies the chosen PartnerChoice's multipliers to all four meters.
    public void ApplyChoice(PartnerChoice choice)
    {
        currentMultiplier = choice.multipliers;
    }

    void Update()
    {
        if (!draining) return;

        float dt = Time.deltaTime;
        body.value -= baseDrain.body * currentMultiplier.body * dt;
        mind.value -= baseDrain.mind * currentMultiplier.mind * dt;
        feelingLikeAPerson.value -= baseDrain.feelingLikeAPerson * currentMultiplier.feelingLikeAPerson * dt;
        mask.value -= baseDrain.mask * currentMultiplier.mask * dt;
    }
}
