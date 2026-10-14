using System;
using UnityEngine;

// Runs the roam part of Partner Hour: the player moves around the room
// doing BAU tasks (dishes, stove, coffee) and the one main task (the
// meeting). Each BAU task, on completion, costs a one-off chunk of the
// meters (InteractableTask.cost) and then locks itself. The main task
// instead changes the continuous drain SPEED for the rest of the hour:
// finishing it is "push through" (choice 1). It only counts as "step away"
// (choice 0) if the hour ends before she finishes it.
//
// The main task always comes first and has to be held all the way through:
// letting go early just resets it, so she can try again. Until it's done,
// every chore is waiting (InteractableTask.waitingForMainTask), shows no
// marker, and says "First: <main task>" when the player faces it.
//
// Partner Hour is not expected to be finishable: meters keep draining
// underneath everything, so the player will run out before every task is
// done. GameManager ends the hour the moment any meter hits zero.
public class PartnerHourController : MonoBehaviour
{
    public GridPlayerController player;
    public Interactor interactor;
    public MeterController meters;

    [Tooltip("Chores that always stay undone, so the Self Hour has traces to find. Finishing the chore that would leave fewer than this ends her hour: she's hit her limit.")]
    public int minChoresLeft = 1;

    // 0 = step away (hold cancelled), 1 = push through (hold completed)
    public event Action<int> ChoiceMade;

    ChapterData chapterData;
    InteractableTask choiceTask;
    bool choiceMade;
    bool ending;   // true while EndRoaming runs: a let-go then is final

    public void BeginRoaming(ChapterData data)
    {
        chapterData = data;
        choiceMade = false;
        ending = false;
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

            // The main task always comes first: every chore waits until it
            // has been held all the way through.
            string main = string.IsNullOrEmpty(choiceTask.displayName) ? choiceTask.gameObject.name : choiceTask.displayName;
            SetChoresWaiting(true, $"First: {main}");
        }
        else
        {
            Debug.LogWarning($"PartnerHourController: no InteractableTask found with taskId '{data.choiceTriggerTaskId}'.");
        }
    }

    public void EndRoaming()
    {
        ending = true;
        // If the player was mid-hold when the hour ended (a meter hit
        // zero, most likely), resolve that hold as a cancel rather than
        // leaving it dangling.
        interactor.ForceCancelHold();

        // Chores never reached stay unfinished for Self Hour (not locked).
        SetChoresWaiting(false, null);

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
        meters.ApplyCost(task.cost, meters.choreCostScale);
        task.locked = true; // done is done; no repeat cost, no lingering prompt

        // Never let every chore get done: the Self Hour needs something
        // left behind. When only the last few remain, she's hit her limit.
        int left = 0;
        foreach (var t in InteractableTask.All)
        {
            if (!t.RequiresHold && !t.locked) left++;
        }
        if (left <= minChoresLeft)
        {
            player.InputLocked = true;
            meters.CollapseLowest();
        }
    }

    void OnHoldCompleted(InteractableTask t)
    {
        if (choiceMade) return;
        choiceMade = true;
        choiceTask.locked = true;
        // Getting through it costs her: the chapter's main meter is cut in
        // half (ChapterData.mainTaskHitMeter / mainTaskHitFraction), which
        // can't end the hour by itself, and the drain
        // eases back in afterwards (MeterController.afterHitRampSeconds).
        meters.ApplyMainTaskHit(chapterData.mainTaskHitMeter, chapterData.mainTaskHitFraction);
        SetChoresWaiting(false, null);
        ChoiceMade?.Invoke(1);
    }

    void OnHoldCancelled(InteractableTask t)
    {
        if (choiceMade) return;
        // Let go early mid-hour: nothing is decided. The main task stays
        // open (no reset of the meters, no chores unlocked), so she has to
        // come back and finish it. Interactor tells the player why.
        if (!ending) return;
        choiceMade = true;
        choiceTask.locked = true;
        SetChoresWaiting(false, null);
        ChoiceMade?.Invoke(0);
    }

    // True while the main task hasn't been dealt with yet.
    public bool ChoresWaiting { get; private set; }

    void SetChoresWaiting(bool waiting, string message)
    {
        ChoresWaiting = waiting;
        foreach (var task in InteractableTask.All)
        {
            if (task.RequiresHold) continue;
            task.waitingForMainTask = waiting;
            task.waitingMessage = message;
        }
    }
}