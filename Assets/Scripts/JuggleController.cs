using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Runs the juggle prompts on whichever task is currently being held (see
// InteractableTask.jugglePrompts). Each prompt is its own lane with its
// own timer, independent of the others.
//
// A lane is either a tap ("Press Q: Respond on the call") or a hold
// ("Hold SPACE 3s: Breathe through the nausea", with a progress bar);
// see JugglePrompt.holdSeconds. Letting go of a hold early starts it
// again. Miss the response window and a small, fixed meter cost applies
// immediately -- the exact same mechanism as a BAU task's cost. This only
// adds pressure and texture to the main hold; nothing here affects whether
// the main hold itself completes or cancels, so it can't become a hidden
// pass/fail layer on the choice.
public class JuggleController : MonoBehaviour
{
    [Tooltip("An empty RectTransform under your Canvas, inside the Partner Hour panel, where juggle prompt rows will be created.")]
    public RectTransform container;
    public MeterController meters;

    [Header("Look")]
    public float rowFontSize = 34f;
    public float rowWidth = 760f;
    public Color dormantColor = new Color(1f, 1f, 1f, 0.2f);
    public Color activeColor = new Color(1f, 0.85f, 0.3f);
    public Color successColor = new Color(0.45f, 0.85f, 0.6f);
    public Color missColor = new Color(0.95f, 0.45f, 0.4f);

    [Tooltip("How long \"Done\" or \"Missed\" stays on a row after a prompt ends.")]
    public float resultShowSeconds = 0.7f;

    class LaneState
    {
        public JugglePrompt prompt;
        public TMP_Text row;
        public RectTransform holdBar;      // fill of the hold progress bar (hold lanes only)
        public GameObject holdBarRoot;
        public float nextPromptTime;
        public float activeUntil = -1f;    // -1 = dormant, waiting for nextPromptTime
        public float held;                 // seconds held so far, for hold lanes
        public float clearResultAt = -1f;  // when to clear "Done" / "Missed"
    }

    InteractableTask watchedTask;
    readonly List<LaneState> lanes = new List<LaneState>();

    float RowHeight => rowFontSize * 1.5f;

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
            layout.spacing = 8f;
            layout.padding = new RectOffset(18, 18, 14, 14);
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
            var lane = new LaneState { prompt = prompt, nextPromptTime = Time.time + prompt.interval };
            CreateRow(lane);
            lanes.Add(lane);
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
                if (lane.clearResultAt > 0f && Time.time >= lane.clearResultAt)
                {
                    lane.clearResultAt = -1f;
                    lane.row.text = "";
                    lane.row.color = dormantColor;
                }
                if (Time.time >= lane.nextPromptTime) Activate(lane);
                continue;
            }

            if (lane.prompt.holdSeconds > 0f)
            {
                // Hold lane: keep the key down until the bar fills.
                if (Input.GetKey(lane.prompt.key)) lane.held += Time.deltaTime;
                else lane.held = 0f;   // let go early: start the breath again

                SetHoldBar(lane, lane.held / lane.prompt.holdSeconds);

                if (lane.held >= lane.prompt.holdSeconds)
                {
                    Finish(lane, true);
                    continue;
                }
            }
            else if (Input.GetKeyDown(lane.prompt.key))
            {
                Finish(lane, true); // caught in time, no cost
                continue;
            }

            if (Time.time > lane.activeUntil)
            {
                meters.ApplyCost(lane.prompt.missCost); // missed, small cost
                Finish(lane, false);
            }
        }
    }

    void Activate(LaneState lane)
    {
        // A hold needs at least the hold time plus a moment to react.
        float window = Mathf.Max(lane.prompt.responseWindow, lane.prompt.holdSeconds + 1.5f);
        lane.activeUntil = Time.time + window;
        lane.held = 0f;
        lane.clearResultAt = -1f;

        string key = KeyName(lane.prompt.key);
        lane.row.text = lane.prompt.holdSeconds > 0f
            ? $"[Hold {key} {lane.prompt.holdSeconds:0}s] {lane.prompt.label}"
            : $"[Press {key}] {lane.prompt.label}";
        lane.row.color = activeColor;

        if (lane.holdBarRoot != null)
        {
            lane.holdBarRoot.SetActive(lane.prompt.holdSeconds > 0f);
            SetHoldBar(lane, 0f);
        }
    }

    void Finish(LaneState lane, bool success)
    {
        lane.activeUntil = -1f;
        lane.nextPromptTime = Time.time + lane.prompt.interval;
        lane.held = 0f;
        lane.row.text = success ? $"Done: {lane.prompt.label}" : $"Missed: {lane.prompt.label}";
        lane.row.color = success ? successColor : missColor;
        lane.clearResultAt = Time.time + resultShowSeconds;
        if (lane.holdBarRoot != null) lane.holdBarRoot.SetActive(false);
    }

    static string KeyName(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.Space: return "SPACE";
            case KeyCode.Return: return "ENTER";
            case KeyCode.LeftShift:
            case KeyCode.RightShift: return "SHIFT";
            default: return key.ToString().ToUpperInvariant();
        }
    }

    static void SetHoldBar(LaneState lane, float t)
    {
        if (lane.holdBar == null) return;
        lane.holdBar.anchorMax = new Vector2(Mathf.Clamp01(t), 1f);
    }

    void CreateRow(LaneState lane)
    {
        var go = new GameObject("JuggleLane", typeof(RectTransform));
        go.transform.SetParent(container, false);

        // Explicit size so the layout group can't collapse an empty-text
        // row to zero before it ever gets a chance to show a prompt.
        var layoutElement = go.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = rowWidth;
        layoutElement.preferredHeight = RowHeight;

        var textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var textRt = (RectTransform)textGo.transform;
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(0f, 10f);   // leave room for the hold bar underneath
        textRt.offsetMax = Vector2.zero;

        var text = textGo.AddComponent<TextMeshProUGUI>();
        text.fontSize = rowFontSize;
        text.color = dormantColor;
        text.text = "";
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        lane.row = text;

        if (lane.prompt.holdSeconds > 0f)
        {
            // A thin bar under the row that fills while the key is held.
            var barGo = new GameObject("HoldBar", typeof(RectTransform));
            barGo.transform.SetParent(go.transform, false);
            var barRt = (RectTransform)barGo.transform;
            barRt.anchorMin = new Vector2(0f, 0f);
            barRt.anchorMax = new Vector2(1f, 0f);
            barRt.pivot = new Vector2(0f, 0f);
            barRt.sizeDelta = new Vector2(0f, 8f);
            barGo.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);

            var fillGo = new GameObject("Fill", typeof(RectTransform));
            fillGo.transform.SetParent(barGo.transform, false);
            var fillRt = (RectTransform)fillGo.transform;
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = new Vector2(0f, 1f);
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            fillGo.AddComponent<Image>().color = successColor;

            lane.holdBar = fillRt;
            lane.holdBarRoot = barGo;
            barGo.SetActive(false);
        }
    }

    void ClearLanes()
    {
        foreach (var lane in lanes)
        {
            if (lane.row != null) Destroy(lane.row.transform.parent.gameObject);
        }
        lanes.Clear();
    }
}
