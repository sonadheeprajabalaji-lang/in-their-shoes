using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Put this on any object the player can interact with: the dishes, the
// stove, the laptop for the meeting, and so on.
//
// taskId must be unique within a chapter's room. ChapterData's
// choiceTriggerTaskId refers to objects by this id, so whatever you type
// here has to match exactly. Self Hour no longer needs a fixed cue id:
// it automatically shows every non-hold task still unlocked when Partner
// Hour ends (see SelfHourController).
public class InteractableTask : MonoBehaviour
{
    // Every InteractableTask currently in the scene registers itself here
    // so Interactor and the hour controllers can find them by id without
    // everyone needing a direct reference wired up in the Inspector.
    public static readonly List<InteractableTask> All = new List<InteractableTask>();

    [Tooltip("Unique within the chapter. Matched against ChapterData's choiceTriggerTaskId.")]
    public string taskId;

    [Tooltip("Name shown in the prompt, e.g. \"Dishes\". If left blank, the GameObject's name is used.")]
    public string displayName;

    [Tooltip("The first part of the prompt shown when the player faces this object.")]
    public string promptText = "Press E";

    [Tooltip("While locked, interacting does nothing. Self Hour locks every task except the seeded cue.")]
    public bool locked;

    [Header("Always-visible indicator")]
    [Tooltip("A small marker shown above this object whenever it's unlocked, so the player can tell from a distance it's interactable -- separate from the detailed \"Press E\" prompt, which only shows up close. Hides automatically once locked.")]
    public bool showIndicator = true;
    public string indicatorGlyph = "!";
    public Color indicatorColor = new Color(1f, 0.82f, 0.15f, 1f);     // high-contrast gold
    public Color indicatorOutline = new Color(0.25f, 0.05f, 0.05f, 1f); // dark red-brown, not plain black

    [Tooltip("How fast the indicator pulses (bob + scale). Higher = more urgent-looking.")]
    public float indicatorPulseSpeed = 2.4f;
    [Tooltip("How far the indicator bobs up and down, in world units.")]
    public float indicatorBobHeight = 0.08f;
    [Tooltip("How much the indicator grows and shrinks (0.15 = \u00b115% scale).")]
    public float indicatorScalePulse = 0.15f;

    TextMeshPro indicator;
    Vector3 indicatorBasePos;

    [Header("BAU task cost (dishes, stove, etc)")]
    [Tooltip("Applied once, immediately, when this task is completed. Leave at 0 for the main/hold task, which affects drain speed instead (see PartnerChoice multipliers).")]
    public MeterRates cost;

    [Header("Audio (optional)")]
    [Tooltip("Played on a normal tap-complete (BAU tasks). Leave empty for silence.")]
    public AudioClip completeClip;
    [Tooltip("Played when a hold completes (push through) or cancels (step away). Leave empty for silence.")]
    public AudioClip holdCompleteClip;
    public AudioClip holdCancelClip;

    [Header("Hold to complete (optional)")]
    [Tooltip("0 = a normal tap interaction (dishes, stove, etc). Above 0 = the player must hold Interact for this many seconds, and letting go early counts as a different outcome.")]
    public float holdDuration = 0f;

    [Tooltip("Shown while holding, e.g. \"Pushing through the call\"")]
    public string holdingLabel = "Holding";

    [Header("Juggling while holding (optional)")]
    [Tooltip("Only active while this task is being held. Each entry is its own lane (breathing, responding, composure...) firing independently on its own timer. Miss the window and its missCost applies immediately -- this adds pressure, it never affects whether the hold itself completes or cancels.")]
    public JugglePrompt[] jugglePrompts;

    public bool RequiresHold => holdDuration > 0f;

    public event Action<InteractableTask> Interacted;
    public event Action<InteractableTask> HoldStarted;      // the hold has just begun
    public event Action<InteractableTask> HoldCompleted;   // held for the full duration
    public event Action<InteractableTask> HoldCancelled;   // let go early

    // What the on-screen prompt says, e.g. "Press E: Dishes"
    public string PromptLabel
    {
        get
        {
            string label = string.IsNullOrEmpty(displayName) ? gameObject.name : displayName;
            return promptText + ": " + label;
        }
    }

    void OnEnable()
    {
        All.Add(this);
        if (showIndicator && indicator == null) CreateIndicator();
    }

    void OnDisable() { All.Remove(this); }

    void Update()
    {
        if (indicator == null) return;

        bool shouldShow = showIndicator && !locked;
        if (indicator.gameObject.activeSelf != shouldShow)
        {
            indicator.gameObject.SetActive(shouldShow);
        }
        if (!shouldShow) return;

        // A small bob + scale pulse, offset per-instance (via GetInstanceID)
        // so a room full of indicators doesn't pulse in perfect unison,
        // which reads as more alive and is easier to notice at a glance.
        float phase = Time.time * indicatorPulseSpeed + (GetInstanceID() % 100) * 0.1f;
        float pulse = Mathf.Sin(phase);

        indicator.transform.localPosition = indicatorBasePos + Vector3.up * (pulse * indicatorBobHeight);
        indicator.transform.localScale = Vector3.one * (1f + pulse * indicatorScalePulse);
    }

    void CreateIndicator()
    {
        var go = new GameObject("InteractIndicator");
        go.transform.SetParent(transform, false);
        // Sits above the detailed "Press E" prompt (promptHeight ~0.9 by
        // default on Interactor), so both can be visible at once up close.
        indicatorBasePos = Vector3.up * (GridPlayerController.CellSize * 1.2f);
        go.transform.localPosition = indicatorBasePos;

        indicator = go.AddComponent<TextMeshPro>();
        indicator.text = indicatorGlyph;
        indicator.fontSize = 4.2f;
        indicator.fontStyle = FontStyles.Bold;
        indicator.color = indicatorColor;
        indicator.alignment = TextAlignmentOptions.Center;
        indicator.outlineWidth = 0.35f; // thicker, higher-contrast outline than before
        indicator.outlineColor = indicatorOutline;

        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 50; // above the room, below the close-up prompt (100)
    }

    public bool OccupiesCell(Vector3 worldPos)
    {
        return Vector3.Distance(transform.position, worldPos) < GridPlayerController.CellSize * 0.5f;
    }

    public void Interact()
    {
        if (locked) return;
        AudioManager.Get().PlaySFX(completeClip);
        Interacted?.Invoke(this);
    }

    // Called by Interactor the moment a hold begins. Not meant to be
    // called directly elsewhere.
    public void BeginHold()
    {
        if (locked) return;
        HoldStarted?.Invoke(this);
    }

    public void CompleteHold()
    {
        if (locked) return;
        AudioManager.Get().PlaySFX(holdCompleteClip);
        HoldCompleted?.Invoke(this);
    }

    public void CancelHold()
    {
        if (locked) return;
        AudioManager.Get().PlaySFX(holdCancelClip);
        HoldCancelled?.Invoke(this);
    }


    public static InteractableTask Find(string id)
    {
        foreach (var t in All)
        {
            if (t.taskId == id) return t;
        }
        return null;
    }
}

// One "lane" of a juggle challenge: fires on its own timer while the
// parent task is being held, independent of the other lanes. Give each
// lane a different chapter-specific flavour (breathing, responding,
// composure) and a small missCost -- small is deliberate, since this is
// meant to add texture to the hold, not become its own fail state.
[System.Serializable]
public class JugglePrompt
{
    [Tooltip("Chapter-specific flavour text, e.g. \"Breathe through the nausea\"")]
    public string label;

    public KeyCode key = KeyCode.Space;

    [Tooltip("0 = tap the key once. Above 0 = hold the key down for this many seconds (e.g. 3 for breathing through a wave). Letting go early starts the hold again. The response window is stretched if it's shorter than the hold.")]
    public float holdSeconds = 0f;

    [Tooltip("Seconds between this lane's prompts while holding")]
    public float interval = 4f;

    [Tooltip("Seconds the player has to press the key once a prompt appears")]
    public float responseWindow = 1.5f;

    [Tooltip("Applied once if the player misses this prompt's window. Keep this small -- it's pressure, not a punishment.")]
    public MeterRates missCost;
}