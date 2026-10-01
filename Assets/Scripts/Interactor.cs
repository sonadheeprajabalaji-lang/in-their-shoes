using TMPro;
using UnityEngine;

// Attach this to the player, alongside GridPlayerController.
//
// Shows a prompt above whatever unlocked InteractableTask the player is
// facing. Tapping E interacts with a normal task (dishes, stove, etc).
// Holding E on a task with holdDuration > 0 starts a hold: releasing
// early counts as HoldCancelled, holding it out counts as HoldCompleted.
// Movement locks for the duration of a hold, so the player can't roam
// and hold at the same time.
public class Interactor : MonoBehaviour
{
    public GridPlayerController player;

    [Header("Prompt look")]
    public float promptFontSize = 3f;
    public float promptHeight = 0.9f;
    public Color promptColor = Color.white;
    public Color holdingColor = new Color(1f, 0.7f, 0.3f);

    TextMeshPro prompt;

    InteractableTask holding;
    float holdElapsed;

    void Awake()
    {
        CreatePrompt();
    }

    void Update()
    {
        if (holding != null)
        {
            UpdateHold();
            return;
        }

        InteractableTask target = FindFacingTask();
        UpdatePrompt(target, null);

        if (target == null) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (target.RequiresHold) StartHold(target);
            else target.Interact();
        }
    }

    void StartHold(InteractableTask task)
    {
        holding = task;
        holdElapsed = 0f;
        player.InputLocked = true;
        task.BeginHold();
    }

    void UpdateHold()
    {
        // Released early: cancelled, locked in, no retry on this attempt
        if (Input.GetKeyUp(KeyCode.E))
        {
            var cancelled = holding;
            EndHold();
            cancelled.CancelHold();
            return;
        }

        holdElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(holdElapsed / holding.holdDuration);
        UpdatePrompt(holding, t);

        if (holdElapsed >= holding.holdDuration)
        {
            var completed = holding;
            EndHold();
            completed.CompleteHold();
        }
    }

    void EndHold()
    {
        player.InputLocked = false;
        holding = null;
    }

    // Called by PartnerHourController if Partner Hour ends (a meter hit
    // zero, or the timer ran out) while the player is mid-hold. Counts
    // as letting go early, same as releasing E would.
    public void ForceCancelHold()
    {
        if (holding == null) return;
        var cancelled = holding;
        EndHold();
        cancelled.CancelHold();
    }

    // The unlocked task in the cell the player is facing, or null.
    InteractableTask FindFacingTask()
    {
        if (player.InputLocked || player.IsMoving) return null;

        Vector3 targetCell = player.FacingCellWorldPosition;
        foreach (var task in InteractableTask.All)
        {
            if (!task.locked && task.OccupiesCell(targetCell)) return task;
        }
        return null;
    }

    void CreatePrompt()
    {
        var go = new GameObject("InteractionPrompt");
        prompt = go.AddComponent<TextMeshPro>();
        prompt.alignment = TextAlignmentOptions.Center;
        prompt.fontSize = promptFontSize;
        prompt.outlineWidth = 0.25f;
        prompt.outlineColor = Color.black;
        prompt.rectTransform.sizeDelta = new Vector2(10f, 1f);
        go.GetComponent<MeshRenderer>().sortingOrder = 100;
        go.SetActive(false);
    }

    void UpdatePrompt(InteractableTask target, float? holdProgress)
    {
        if (target == null)
        {
            if (prompt.gameObject.activeSelf) prompt.gameObject.SetActive(false);
            return;
        }

        if (holdProgress.HasValue)
        {
            float remaining = target.holdDuration * (1f - holdProgress.Value);
            prompt.text = $"{target.holdingLabel} \u2014 let go to stop ({remaining:0.0}s)";
            prompt.color = holdingColor;
        }
        else
        {
            prompt.text = target.RequiresHold
                ? $"Hold E: {(string.IsNullOrEmpty(target.displayName) ? target.gameObject.name : target.displayName)}"
                : target.PromptLabel;
            prompt.color = promptColor;
        }

        prompt.transform.position = target.transform.position + Vector3.up * promptHeight;
        if (!prompt.gameObject.activeSelf) prompt.gameObject.SetActive(true);
    }

    void OnDestroy()
    {
        if (prompt != null) Destroy(prompt.gameObject);
    }
}