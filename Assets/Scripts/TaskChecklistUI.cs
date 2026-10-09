using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A simple on-screen checklist of the BAU tasks in the room (dishes,
// stove, etc), separate from the main/hold task. Ticks each one off as
// the player completes it. Rows are built entirely at runtime, so there
// is nothing to hand-build in the Editor beyond an empty RectTransform to
// hold them, assigned to Container below.
//
// The main task isn't included, since it already has its own hold-progress
// prompt above it in the world; showing it here too would be redundant.
public class TaskChecklistUI : MonoBehaviour
{
    [Tooltip("An empty RectTransform under your Canvas, inside the Partner Hour panel, where checklist rows will be created. Point this at the same RectTransform as JuggleController's Container to show both lists in one consolidated box.")]
    public RectTransform container;

    [Tooltip("Shown once, above the rows. Leave blank for no heading.")]
    public string sectionHeading = "Tasks";
    public Color headingColor = new Color(0.9f, 0.88f, 0.75f);

    public float rowFontSize = 30f;
    public float rowWidth = 460f;
    public Color pendingColor = Color.white;
    public Color doneColor = new Color(0.6f, 0.6f, 0.6f);

    readonly Dictionary<InteractableTask, TMP_Text> rows = new Dictionary<InteractableTask, TMP_Text>();
    GameObject heading;

    void Awake()
    {
        if (container.GetComponent<Image>() == null)
        {
            // A solid backing panel so the checklist stays legible no
            // matter what's rendered underneath it in the room.
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

    // Called by GameManager when Partner Hour starts.
    public void BuildList()
    {
        Clear();

        if (!string.IsNullOrEmpty(sectionHeading))
        {
            heading = CreateHeading(sectionHeading, headingColor);
        }

        foreach (var task in InteractableTask.All)
        {
            if (task.RequiresHold) continue; // the main task isn't on this checklist

            var row = CreateRow(task);
            rows[task] = row;
            task.Interacted += OnTaskDone;
        }
    }

    // Called by GameManager when Partner Hour ends. Only destroys rows
    // this script created -- the container may be shared with
    // JuggleController, so it must never sweep every child.
    public void Clear()
    {
        foreach (var kvp in rows)
        {
            kvp.Key.Interacted -= OnTaskDone;
            if (kvp.Value != null) Destroy(kvp.Value.gameObject);
        }
        rows.Clear();

        if (heading != null)
        {
            Destroy(heading);
            heading = null;
        }
    }

    GameObject CreateHeading(string text, Color color)
    {
        var go = new GameObject("Heading_" + name);
        go.transform.SetParent(container, false);

        var label = go.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = rowFontSize * 0.85f;
        label.fontStyle = FontStyles.Bold;
        label.color = color;
        label.enableWordWrapping = false;

        var layoutElement = go.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = rowWidth;
        layoutElement.preferredHeight = rowFontSize * 1.1f;

        return go;
    }

    TMP_Text CreateRow(InteractableTask task)
    {
        var go = new GameObject("Checklist_" + task.taskId);
        go.transform.SetParent(container, false);

        var text = go.AddComponent<TextMeshProUGUI>();
        text.text = Label(task, false);
        text.fontSize = rowFontSize;
        text.color = pendingColor;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.rectTransform.sizeDelta = new Vector2(rowWidth, rowFontSize * 1.4f);

        // Explicit size so the layout group can't collapse this row to
        // zero before it ever gets a chance to render.
        var layoutElement = go.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = rowWidth;
        layoutElement.preferredHeight = rowFontSize * 1.4f;

        return text;
    }

    void OnTaskDone(InteractableTask task)
    {
        if (!rows.TryGetValue(task, out var row)) return;

        row.text = Label(task, true);
        row.color = doneColor;
        row.fontStyle |= FontStyles.Strikethrough;
    }

    string Label(InteractableTask task, bool done)
    {
        string name = string.IsNullOrEmpty(task.displayName) ? task.gameObject.name : task.displayName;
        return (done ? "\u2611 " : "\u2610 ") + name;
    }
}