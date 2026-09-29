using System;
using System.Collections.Generic;
using UnityEngine;

// Put this on any object the player can interact with: the dishes, the
// stove, the laptop for the meeting, and so on.
//
// taskId must be unique within a chapter's room. ChapterData's
// choiceTriggerTaskId and selfHourCueTaskId refer to objects by this id,
// so whatever you type here has to match exactly what you put in the
// ChapterData asset.
public class InteractableTask : MonoBehaviour
{
    // Every InteractableTask currently in the scene registers itself here
    // so Interactor and the hour controllers can find them by id without
    // everyone needing a direct reference wired up in the Inspector.
    public static readonly List<InteractableTask> All = new List<InteractableTask>();

    [Tooltip("Unique within the chapter. Matched against ChapterData's choiceTriggerTaskId / selfHourCueTaskId.")]
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

    public bool RequiresHold => holdDuration > 0f;

    public event Action<InteractableTask> Interacted;
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

    // Called by Interactor. Not meant to be called directly elsewhere.
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