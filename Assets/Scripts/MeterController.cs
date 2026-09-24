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

    [Header("Base drain per second (1 = full bar, 0 = empty)")]
    public float bodyDrain = 0.03f;
    public float mindDrain = 0.03f;
    public float personDrain = 0.02f;
    public float maskDrain = 0.03f;

    [Header("Choice effects (multipliers on the drain above)")]
    // Choice 0: "Mute and step away" - Mask drains slower, Mind drains faster
    public float stepAwayMask = 0.4f;
    public float stepAwayMind = 1.6f;
    // Choice 1: "Push through the call" - Mask drains faster, Mind drains slower
    public float pushThroughMask = 1.6f;
    public float pushThroughMind = 0.6f;

    float maskMultiplier = 1f;
    float mindMultiplier = 1f;
    bool draining = false;

    // Call this when the Partner Hour starts
    public void StartDraining()
    {
        ResetMeters();
        draining = true;
    }

    // Call this when the Partner Hour ends
    public void StopDraining()
    {
        draining = false;
    }

    public void ResetMeters()
    {
        maskMultiplier = 1f;
        mindMultiplier = 1f;
        body.value = 1f;
        mind.value = 1f;
        feelingLikeAPerson.value = 1f;
        mask.value = 1f;
    }

    // 0 = step away, 1 = push through
    public void ApplyChoice(int choice)
    {
        if (choice == 0)
        {
            maskMultiplier = stepAwayMask;
            mindMultiplier = stepAwayMind;
        }
        else
        {
            maskMultiplier = pushThroughMask;
            mindMultiplier = pushThroughMind;
        }
    }

    void Update()
    {
        if (!draining) return;

        float dt = Time.deltaTime;
        body.value -= bodyDrain * dt;
        mind.value -= mindDrain * mindMultiplier * dt;
        feelingLikeAPerson.value -= personDrain * dt;
        mask.value -= maskDrain * maskMultiplier * dt;
    }
}