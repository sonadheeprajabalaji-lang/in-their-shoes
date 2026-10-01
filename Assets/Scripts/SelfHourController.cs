using System;
using System.Collections.Generic;
using UnityEngine;

// Runs the roam part of Self Hour: the same room as Partner Hour, but
// only the BAU tasks that were left UNFINISHED stay interactable. A
// completed BAU task was locked by PartnerHourController when it was
// done, so "still unlocked when Self Hour starts" and "left unfinished"
// mean the same thing here -- nothing extra needs tracking.
//
// The main/hold task is never a Self Hour cue, regardless of whether a
// choice was made at it.
public class SelfHourController : MonoBehaviour
{
    public GridPlayerController player;

    // Fired each time the player interacts with one of the remaining cues.
    public event Action<InteractableTask> CueInteracted;

    // Fired once every cue has a response (or there were none to begin with).
    public event Action AllCuesResolved;

    readonly List<InteractableTask> pendingCues = new List<InteractableTask>();

    public void BeginRoaming(ChapterData data)
    {
        player.InputLocked = false;
        pendingCues.Clear();

        foreach (var task in InteractableTask.All)
        {
            if (task.RequiresHold)
            {
                task.locked = true; // never a Self Hour cue
                continue;
            }

            if (task.locked) continue; // completed during Partner Hour

            pendingCues.Add(task);
            task.Interacted += OnCueInteracted;
        }

        if (pendingCues.Count == 0)
        {
            // Everything got done in Partner Hour. Self Hour has nothing
            // to show, so resolve immediately.
            AllCuesResolved?.Invoke();
        }
    }

    public void EndRoaming()
    {
        foreach (var task in pendingCues)
        {
            task.Interacted -= OnCueInteracted;
        }
        pendingCues.Clear();
        player.InputLocked = true;
    }

    void OnCueInteracted(InteractableTask task)
    {
        CueInteracted?.Invoke(task);
    }

    // Called by GameManager once the player has picked a response for
    // the cue currently shown, so it can't be responded to twice and
    // Self Hour knows when every cue has been addressed.
    public void MarkCueResolved(InteractableTask task)
    {
        task.locked = true;
        task.Interacted -= OnCueInteracted;
        pendingCues.Remove(task);

        if (pendingCues.Count == 0)
        {
            AllCuesResolved?.Invoke();
        }
    }
}