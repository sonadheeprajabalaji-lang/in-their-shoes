using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Runs the juggle prompts on whichever task is currently being held (see
// InteractableTask.jugglePrompts). Each prompt is its own lane with its
// own timer, independent of the others. Miss the response window and a
// small, fixed meter cost applies immediately -- the exact same
// mechanism as a BAU task's cost. This only adds pressure and texture
// to the hold; nothing here affects whether the hold itself completes
// or cancels, so it can't become a hidden pass/fail layer on the choice.
public class JuggleController : MonoBehaviour
{
    [Tooltip("An empty RectTransform under your Canvas, inside the Partner Hour panel, where juggle prompt rows will be created.")]
    public RectTransform container;
    public MeterController meters;

    public Color dormantColor = new Color(1f, 1f, 1f, 0.2f);
    public Color activeColor = new Color(1f, 0.85f, 0.3f);

    class LaneState
    {
        public JugglePrompt prompt;
        public TMP_Text row;
        public float nextPromptTime;
        public float activeUntil = -1f; // -1 = dormant, waiting for nextPromptTime
    }

    InteractableTask watchedTask;
    readonly List<LaneState> lanes = new List<LaneState>();

    void Awake()
    {
        if (container.GetComponent<Image>() == null)
        {
            // A solid backing panel so the prompts stay legible no matter
            // what's rendered underneath them in the room.
            var bg = container.gameObject.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.65f);
        }

        if (container.GetComponent<VerticalLayoutGroup>() == null)
        {
            var layout = container.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.spacing = 4f;
            layout.padding = new RectOffset(8, 8, 8, 8);
        }
        if (container.GetComponent<ContentSizeFitter>() == null)
        {
            var fitter = container.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }

    // Called by GameManager when Partner Hour starts. Watches whichever
    // task is this chapter's main/choice task for hold events.
    public void BeginRoaming(ChapterData data)
    {
        watchedTask = InteractableTask.Find(data.choiceTriggerTaskId);
        if (watchedTask == null) return;

        watchedTask.HoldStarted += OnHoldStarted;
        watchedTask.HoldCompleted += OnHoldEnded;
        watchedTask.HoldCancelled += OnHoldEnded;
    }

    // Called by GameManager when Partner Hour ends.
    public void EndRoaming()
    {
        if (watchedTask != null)
        {
            watchedTask.HoldStarted -= OnHoldStarted;
            watchedTask.HoldCompleted -= OnHoldEnded;
            watchedTask.HoldCancelled -= OnHoldEnded;
        }
        watchedTask = null;
        ClearLanes();
    }

    void OnHoldStarted(InteractableTask task)
    {
        ClearLanes();

        if (task.jugglePrompts == null) return;

        foreach (var prompt in task.jugglePrompts)
        {
            lanes.Add(new LaneState
            {
                prompt = prompt,
                row = CreateRow(),
                nextPromptTime = Time.time + prompt.interval
            });
        }
    }

    void OnHoldEnded(InteractableTask task)
    {
        ClearLanes();
    }

    void Update()
    {
        foreach (var lane in lanes)
        {
            if (lane.activeUntil < 0f)
            {
                if (Time.time >= lane.nextPromptTime) Activate(lane);
                continue;
            }

            if (Input.GetKeyDown(lane.prompt.key))
            {
                Deactivate(lane); // caught in time, no cost
                continue;
            }

            if (Time.time > lane.activeUntil)
            {
                meters.ApplyCost(lane.prompt.missCost); // missed, small cost
                Deactivate(lane);
            }
        }
    }

    void Activate(LaneState lane)
    {
        lane.activeUntil = Time.time + lane.prompt.responseWindow;
        lane.row.text = $"[{lane.prompt.key}] {lane.prompt.label}";
        lane.row.color = activeColor;
    }

    void Deactivate(LaneState lane)
    {
        lane.activeUntil = -1f;
        lane.nextPromptTime = Time.time + lane.prompt.interval;
        lane.row.text = "";
        lane.row.color = dormantColor;
    }

    TMP_Text CreateRow()
    {
        var go = new GameObject("JuggleLane");
        go.transform.SetParent(container, false);

        var text = go.AddComponent<TextMeshProUGUI>();
        text.fontSize = 20f;
        text.color = dormantColor;
        text.text = "";
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.rectTransform.sizeDelta = new Vector2(420f, 28f);

        // Explicit size so the layout group can't collapse an empty-text
        // row to zero before it ever gets a chance to show a prompt.
        var layoutElement = go.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = 420f;
        layoutElement.preferredHeight = 28f;

        return text;
    }

    void ClearLanes()
    {
        foreach (var lane in lanes)
        {
            if (lane.row != null) Destroy(lane.row.gameObject);
        }
        lanes.Clear();
    }
}