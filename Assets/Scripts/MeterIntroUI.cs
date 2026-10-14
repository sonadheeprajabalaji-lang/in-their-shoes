using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A short screen before each chapter's Partner Hour. The tutorial scene
// teaches the keys and meters in full, so this is just a picture of what
// this chapter needs:
//   - the chapter's number and title
//   - her four meters as icons (a reminder, no explanations)
//   - the main task (hold E) and the prompt keys for this chapter, read
//     straight from the main task's jugglePrompts, so it always matches
// Enter or the Start button begins the chapter.
//
// Builds its own UI at runtime (its own overlay canvas). GameManager
// creates one automatically and shows it when ChapterData.showMeterIntro
// is ticked. (ChapterData.meterIntroNote is no longer shown.)
public class MeterIntroUI : MonoBehaviour
{
    GameObject root;
    RectTransform content;
    Action onStart;
    float shownAt;

    public bool IsShowing => root != null && root.activeSelf;

    void Awake()
    {
        BuildFrame();
        root.SetActive(false);
    }

    public void Show(ChapterData data, Action start)
    {
        onStart = start;
        Fill(data);
        root.SetActive(true);
        shownAt = Time.unscaledTime;
    }

    void Update()
    {
        if (!IsShowing) return;
        // Ignore the first moment so the Enter or click that left the
        // content warning can't also skip this screen.
        if (Time.unscaledTime - shownAt < 0.4f) return;
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Begin();
        }
    }

    void Begin()
    {
        if (!IsShowing) return;
        root.SetActive(false);
        var cb = onStart;
        onStart = null;
        cb?.Invoke();
    }

    // ---------- UI ----------

    void BuildFrame()
    {
        root = new GameObject("MeterIntroCanvas");
        root.transform.SetParent(transform, false);
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 450;   // above the game's UI, below the learning card
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        var backdrop = UIArt.Rect("Backdrop", root.transform);
        UIArt.Stretch(backdrop);
        backdrop.gameObject.AddComponent<Image>().color = UIArt.Backdrop;

        var card = UIArt.Panel(root.transform, "Card");
        var crt = card.rectTransform;
        crt.anchorMin = crt.anchorMax = crt.pivot = new Vector2(0.5f, 0.5f);
        crt.sizeDelta = new Vector2(1300, 0);
        var layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(70, 70, 50, 50);
        layout.spacing = 26;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        card.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        content = crt;
    }

    void Fill(ChapterData data)
    {
        for (int i = content.childCount - 1; i >= 0; i--) Destroy(content.GetChild(i).gameObject);

        // Chapter N / Title
        UIArt.Text(content, $"Chapter {data.chapterNumber}", 30, UIArt.HintColor);
        UIArt.Text(content, string.IsNullOrWhiteSpace(data.chapterTitle) ? "" : data.chapterTitle, 54, UIArt.TitleColor,
                   TextAlignmentOptions.Center, true);

        // Her four meters, as icons
        var meters = UIArt.Row(content, "Meters", 44);
        foreach (var m in UIArt.Meters)
        {
            var chip = UIArt.Row(meters.transform, m.name, 12);
            UIArt.Icon(chip.transform, m.icon, 48);
            UIArt.Text(chip.transform, m.name, 28, m.color, TextAlignmentOptions.Left, true).enableWordWrapping = false;
        }

        Divider();

        // Main task
        var main = InteractableTask.Find(data.choiceTriggerTaskId);
        string mainName = main == null ? data.mainTaskText
            : (string.IsNullOrEmpty(main.displayName) ? main.gameObject.name : main.displayName);
        var mainRow = UIArt.Row(content, "MainTask", 24);
        UIArt.Key(mainRow.transform, "E", 84, -1, -1, KeyCode.E);
        var mainText = UIArt.Column(mainRow.transform, "Text", 4, TextAnchor.MiddleLeft);
        UIArt.Size(UIArt.Text(mainText.transform, "Hold first", 26, UIArt.HintColor, TextAlignmentOptions.Left), 560, -1);
        UIArt.Size(UIArt.Text(mainText.transform, mainName, 38, UIArt.TitleColor, TextAlignmentOptions.Left, true), 560, -1);

        // This chapter's prompt keys, from the main task itself
        if (main != null && main.jugglePrompts != null && main.jugglePrompts.Length > 0)
        {
            UIArt.Text(content, "While you hold E, keep up with:", 28, UIArt.HintColor);
            var lanes = UIArt.Column(content, "Prompts", 14, TextAnchor.UpperCenter);
            foreach (var p in main.jugglePrompts)
            {
                var row = UIArt.Row(lanes.transform, p.label, 22);
                string key = UIArt.KeyLabel(p.key);
                UIArt.Key(row.transform, key, 64, key.Length > 1 ? 180 : -1, -1, p.key);
                UIArt.Size(UIArt.Text(row.transform, p.holdSeconds > 0f ? "hold" : "tap", 28, UIArt.HintColor,
                                      TextAlignmentOptions.Left), 80, -1);
                UIArt.Size(UIArt.Text(row.transform, p.label.Trim('"'), 32, UIArt.TextColor, TextAlignmentOptions.Left), 560, -1);
            }
        }

        // Start button
        var buttonRow = UIArt.Row(content, "ButtonRow", 0, TextAnchor.MiddleRight);
        UIArt.Size(buttonRow, -1, 84);
        var img = UIArt.Panel(buttonRow.transform, "StartButton", "ui_button", 1f);
        UIArt.Size(img, 460, 80);
        img.gameObject.AddComponent<Button>().onClick.AddListener(Begin);
        var label = UIArt.Text(img.transform, $"Start Chapter {data.chapterNumber}  (Enter)", 32, UIArt.KeyText);
        UIArt.Stretch((RectTransform)label.transform);
    }

    void Divider()
    {
        var line = UIArt.Rect("Divider", content);
        UIArt.Size(line, -1, 3);
        line.gameObject.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);
    }
}
