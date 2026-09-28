using System;
using UnityEngine;

// Runs the roam part of Self Hour: the same room as Partner Hour, but
// every task is locked except the one seeded by what happened during
// Partner Hour (chapterData.selfHourCueTaskId). Interacting with it is
// what shows the response popup.
public class SelfHourController : MonoBehaviour
{
    public GridPlayerController player;

    public event Action CueInteracted;

    InteractableTask cueTask;

    public void BeginRoaming(ChapterData data)
    {
        player.InputLocked = false;

        foreach (var task in InteractableTask.All)
        {
            task.locked = true;
        }

        cueTask = InteractableTask.Find(data.selfHourCueTaskId);
        if (cueTask != null)
        {
            cueTask.locked = false;
            cueTask.Interacted += OnCueInteracted;
        }
        else
        {
            Debug.LogWarning($"SelfHourController: no InteractableTask found with taskId '{data.selfHourCueTaskId}'. Check it matches an object in the scene.");
        }
    }

    public void EndRoaming()
    {
        if (cueTask != null)
        {
            cueTask.Interacted -= OnCueInteracted;
        }
        player.InputLocked = true;
    }

    void OnCueInteracted(InteractableTask task)
    {
        CueInteracted?.Invoke();
    }
}
