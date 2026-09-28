using System;
using UnityEngine;

// Runs the roam part of Partner Hour: the player moves around the room
// doing ordinary tasks (dishes, coffee, the online meeting) until they
// interact with whichever task ChapterData names as the choice trigger.
// At that point movement locks and GameManager shows the choice popup.
//
// Meter draining is handled separately by MeterController on a timer, so
// it keeps running underneath this regardless of where the player is.
public class PartnerHourController : MonoBehaviour
{
    public GridPlayerController player;

    // Fired once, the first time the player interacts with the task whose
    // taskId matches chapterData.choiceTriggerTaskId.
    public event Action ChoiceTriggerReached;

    ChapterData chapterData;
    bool choiceTriggered;

    public void BeginRoaming(ChapterData data)
    {
        chapterData = data;
        choiceTriggered = false;
        player.InputLocked = false;

        foreach (var task in InteractableTask.All)
        {
            task.locked = false;
            task.Interacted += OnTaskInteracted;
        }
    }

    public void EndRoaming()
    {
        foreach (var task in InteractableTask.All)
        {
            task.Interacted -= OnTaskInteracted;
        }
        player.InputLocked = true;
    }

    void OnTaskInteracted(InteractableTask task)
    {
        if (choiceTriggered) return;
        if (task.taskId != chapterData.choiceTriggerTaskId) return;

        choiceTriggered = true;
        player.InputLocked = true;
        ChoiceTriggerReached?.Invoke();
    }

    // Called by GameManager once the player has picked a choice option,
    // so roaming (and the meter drain multiplier from that choice)
    // continues for the rest of Partner Hour.
    public void ResumeAfterChoice()
    {
        player.InputLocked = false;
    }
}
