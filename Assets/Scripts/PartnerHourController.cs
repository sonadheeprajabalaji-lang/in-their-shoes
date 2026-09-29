using System;
using UnityEngine;

// Runs the roam part of Partner Hour: the player moves around the room
// doing BAU tasks (dishes, stove, coffee) and the one main task (the
// meeting). Each BAU task, on completion, costs a one-off chunk of the
// meters (InteractableTask.cost) and then locks itself. The main task
// instead changes the continuous drain SPEED for the rest of the hour,
// depending on whether it's held to completion (push through) or let go
// early (step away).
//
// Partner Hour is not expected to be finishable: meters keep draining
// underneath everything, so the player will run out before every task is
// done. GameManager ends the hour the moment any meter hits zero.
public class PartnerHourController : MonoBehaviour
{
    public GridPlayerController player;
    public Interactor interactor;
    public MeterController meters;

    // 0 = step away (hold cancelled), 1 = push through (hold completed)
    public event Action<int> ChoiceMade;

    ChapterData chapterData;
    InteractableTask choiceTask;
    bool choiceMade;

    public void BeginRoaming(ChapterData data)
    {
        chapterData = data;
        choiceMade = false;
        player.InputLocked = false;

        foreach (var task in InteractableTask.All)
        {
            task.locked = false;
            if (!task.RequiresHold)
            {
                task.Interacted += OnBauTaskDone;
            }
        }

        choiceTask = InteractableTask.Find(data.choiceTriggerTaskId);
        if (choiceTask != null)
        {
            choiceTask.HoldCompleted += OnHoldCompleted;
            choiceTask.HoldCancelled += OnHoldCancelled;
        }
        else
        {
            Debug.LogWarning($"PartnerHourController: no InteractableTask found with taskId '{data.choiceTriggerTaskId}'.");
        }
    }

    public void EndRoaming()
    {
        // If the player was mid-hold when the hour ended (a meter hit
        // zero, most likely), resolve that hold as a cancel rather than
        // leaving it dangling.
        interactor.ForceCancelHold();

        foreach (var task in InteractableTask.All)
        {
            if (!task.RequiresHold) task.Interacted -= OnBauTaskDone;
        }

        if (choiceTask != null)
        {
            choiceTask.HoldCompleted -= OnHoldCompleted;
            choiceTask.HoldCancelled -= OnHoldCancelled;
        }

        player.InputLocked = true;
    }

    void OnBauTaskDone(InteractableTask task)
    {
        meters.ApplyCost(task.cost);
        task.locked = true; // done is done; no repeat cost, no lingering prompt
    }

    void OnHoldCompleted(InteractableTask t)
    {
        if (choiceMade) return;
        choiceMade = true;
        choiceTask.locked = true;
        ChoiceMade?.Invoke(1);
    }

    void OnHoldCancelled(InteractableTask t)
    {
        if (choiceMade) return;
        choiceMade = true;
        choiceTask.locked = true;
        ChoiceMade?.Invoke(0);
    }
}