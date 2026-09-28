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

    [Tooltip("Shown as a prompt when the player faces this object, if you wire up a prompt UI later.")]
    public string promptText = "Press E";

    [Tooltip("While locked, interacting does nothing. Self Hour locks every task except the seeded cue.")]
    public bool locked;

    public event Action<InteractableTask> Interacted;

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

    public static InteractableTask Find(string id)
    {
        foreach (var t in All)
        {
            if (t.taskId == id) return t;
        }
        return null;
    }
}
