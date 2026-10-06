using System;
using System.Collections.Generic;
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

    [Header("BAU task cost (dishes, stove, etc)")]
    [Tooltip("Applied once, immediately, when this task is completed. Leave at 0 for the main/hold task, which affects drain speed instead (see PartnerChoice multipliers).")]
    public MeterRates cost;

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

    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    public bool OccupiesCell(Vector3 worldPos)
    {
        return Vector3.Distance(transform.position, worldPos) < GridPlayerController.CellSize * 0.5f;
    }

    public void Interact()
    {
        if (locked) return;
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
        HoldCompleted?.Invoke(this);
    }

    public void CancelHold()
    {
        if (locked) return;
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