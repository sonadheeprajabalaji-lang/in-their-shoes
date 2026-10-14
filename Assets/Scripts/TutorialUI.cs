using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The tutorial scene, shown once before Chapter 1: one page, mostly
// pictures.
//   Top:    how to play - movement keys, E to use things / hold E for the
//           main task, the prompt keys.
//   Bottom: her four meters - icon, name and a bar.
// Nothing on it moves: it's a still picture to read before playing.
// Enter (or the button) loads nextSceneName.
//
// Builds all of its UI at runtime; the scene only needs this component (and
// a camera).
public class TutorialUI : MonoBehaviour
{
    public string nextSceneName = "Chapter1";

    GameObject root;
    float shownAt;


    void Start()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
        CarryoverState.Reset();   // a fresh playthrough starts here
        Build();
        shownAt = Time.unscaledTime;
    }

    void Update()
    {
        if (Time.unscaledTime - shownAt > 0.3f
            && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            Begin();
        }
    }

    void Begin() => SceneManager.LoadScene(nextSceneName);

    // ---------- build ----------

    void Build()
    {
        root = new GameObject("TutorialCanvas");
        root.transform.SetParent(transform, false);
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        var backdrop = UIArt.Rect("Backdrop", root.transform);
        UIArt.Stretch(backdrop);
        backdrop.gameObject.AddComponent<Image>().color = UIArt.Backdrop;

        // One big card, nearly full screen.
        var card = UIArt.Panel(root.transform, "Card").rectTransform;
        UIArt.Stretch(card);
        card.offsetMin = new Vector2(40, 30);
        card.offsetMax = new Vector2(-40, -30);

        var page = UIArt.Column(card, "Page", 18, TextAnchor.UpperCenter);
        var prt = (RectTransform)page.transform;
        UIArt.Stretch(prt);
        page.padding = new RectOffset(70, 70, 34, 30);
        page.childForceExpandWidth = true;

        BuildControls(prt);
        Divider(prt);
        BuildMeters(prt);   // ends with the Start button
    }

    void SectionTitle(Transform parent, string text) =>
        UIArt.Text(parent, text, 46, UIArt.TitleColor, TextAlignmentOptions.Center, true).enableWordWrapping = false;

    // Top: how to play -------------------------------------------------------

    void BuildControls(Transform parent)
    {
        SectionTitle(parent, "How to play");
        var cols = UIArt.Row(parent, "Columns", 80, TextAnchor.UpperCenter);

        // Move
        var move = Column(cols.transform, "Move");
        var clusters = UIArt.Row(move.transform, "Clusters", 30);
        KeyCluster(clusters.transform, null);
        UIArt.Text(clusters.transform, "or", 30, UIArt.HintColor);
        KeyCluster(clusters.transform, new[] { "W", "A", "S", "D" });

        // Use things
        var use = Column(cols.transform, "Use things");
        // "Press [E] *"  /  "Hold [E] for the main task": the word comes first.
        var tapRow = UIArt.Row(use.transform, "Tap", 20, TextAnchor.MiddleLeft);
        Word(tapRow.transform, "Press");
        UIArt.Key(tapRow.transform, "E", 76, -1, -1);
        UIArt.Icon(tapRow.transform, "trace_marker", 44);
        var holdRow = UIArt.Row(use.transform, "Hold", 20, TextAnchor.MiddleLeft);
        Word(holdRow.transform, "Hold");
        UIArt.Key(holdRow.transform, "E", 76);
        var holdCol = UIArt.Column(holdRow.transform, "HoldBar", 8, TextAnchor.MiddleLeft);
        UIArt.Size(UIArt.Text(holdCol.transform, "for the main task", 30, UIArt.TextColor, TextAlignmentOptions.Left), 260, -1);
        Bar(holdCol.transform, 260, 20, UIArt.TitleColor, 0.6f);

        // Respond to prompts
        var respond = Column(cols.transform, "Respond to prompts");
        var spaceRow = UIArt.Row(respond.transform, "Space", 20, TextAnchor.MiddleLeft);
        Word(spaceRow.transform, "Hold");
        UIArt.Key(spaceRow.transform, "SPACE", 76, 220, -1);
        var tapKeys = UIArt.Row(respond.transform, "QF", 20, TextAnchor.MiddleLeft);
        Word(tapKeys.transform, "Tap");
        UIArt.Key(tapKeys.transform, "Q", 76, -1, -1);
        UIArt.Key(tapKeys.transform, "F", 76, -1, -1);
        UIArt.Size(UIArt.Text(respond.transform, "Keys change each chapter.", 24, UIArt.HintColor, TextAlignmentOptions.Left), 360, -1);
    }

    void KeyCluster(Transform parent, string[] letters)
    {
        // [ up ] over [ left down right ], stored in the order up, left, down, right
        var col = UIArt.Column(parent, "Cluster", 8);
        var top = UIArt.Row(col.transform, "Top", 8);
        var bottom = UIArt.Row(col.transform, "Bottom", 8);
        float[] angles = { 0f, 90f, 180f, 270f };
        for (int i = 0; i < 4; i++)
        {
            var row = i == 0 ? top.transform : bottom.transform;
            if (letters == null) UIArt.Key(row, "arrow", 64, -1, angles[i]);
            else UIArt.Key(row, letters[i], 64);
        }
    }

    // Bottom: her four meters -------------------------------------------------

    void BuildMeters(Transform parent)
    {
        SectionTitle(parent, "Her four meters");

        // Two columns of two meters each.
        var grid = UIArt.Row(parent, "Meters", 90, TextAnchor.UpperCenter);
        for (int c = 0; c < 2; c++)
        {
            var col = UIArt.Column(grid.transform, "Col", 22, TextAnchor.UpperLeft);
            for (int r = 0; r < 2; r++)
            {
                int i = c * 2 + r;
                var m = UIArt.Meters[i];
                var row = UIArt.Row(col.transform, m.name, 22, TextAnchor.MiddleLeft);
                UIArt.Icon(row.transform, m.icon, 68);
                var text = UIArt.Column(row.transform, "Text", 6, TextAnchor.MiddleLeft);
                UIArt.Text(text.transform, m.name, 36, m.color, TextAlignmentOptions.Left, true).enableWordWrapping = false;
                Bar(text.transform, 600, 24, m.color, Fills[i]);
            }
        }

        // Last line: what happens when one runs out, and the Start button.
        var bottom = UIArt.Row(parent, "Bottom", 60, TextAnchor.MiddleCenter);
        var end = UIArt.Row(bottom.transform, "End", 22);
        var track = UIArt.Rect("EmptyBar", end.transform);
        UIArt.Size(track, 180, 26);
        track.gameObject.AddComponent<Image>().color = new Color32(255, 120, 110, 255);
        var inner = UIArt.Rect("Inside", track);
        UIArt.Stretch(inner);
        inner.offsetMin = new Vector2(4, 4);
        inner.offsetMax = new Vector2(-4, -4);
        inner.gameObject.AddComponent<Image>().color = new Color32(30, 24, 36, 255);
        UIArt.Text(end.transform, "One runs out: her hour ends.", 34, UIArt.TitleColor, TextAlignmentOptions.Left, true).enableWordWrapping = false;
        BuildStart(bottom.transform);
    }

    // Start button ------------------------------------------------------------

    void BuildStart(Transform parent)
    {
        var img = UIArt.Panel(parent, "StartButton", "ui_button", 1f);
        UIArt.Size(img, 480, 80);
        img.gameObject.AddComponent<Button>().onClick.AddListener(Begin);
        var label = UIArt.Text(img.transform, "Start Chapter 1  (Enter)", 32, UIArt.KeyText);
        UIArt.Stretch((RectTransform)label.transform);
    }

    // A word before a key ("Press", "Hold", "Tap"), same width in every row
    // so the keys line up.
    static void Word(Transform parent, string word) =>
        UIArt.Size(UIArt.Text(parent, word, 30, UIArt.TextColor, TextAlignmentOptions.Left), 90, -1);

    static void Divider(Transform parent)
    {
        var line = UIArt.Rect("Divider", parent);
        UIArt.Size(line, -1, 3);
        line.gameObject.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);
    }

    // Helpers --------------------------------------------------------------

    VerticalLayoutGroup Column(Transform parent, string heading)
    {
        var col = UIArt.Column(parent, heading, 18, TextAnchor.UpperLeft);
        UIArt.Text(col.transform, heading, 36, UIArt.TitleColor, TextAlignmentOptions.Left, true);
        return col;
    }

    // Example fill levels for the meter bars, so they read as bars.
    static readonly float[] Fills = { 0.8f, 0.45f, 0.65f, 0.3f };

    static RectTransform Bar(Transform parent, float width, float height, Color color, float fillAmount = 1f)
    {
        var track = UIArt.Rect("Bar", parent);
        UIArt.Size(track, width, height);
        track.gameObject.AddComponent<Image>().color = new Color32(30, 24, 36, 255);
        var fill = UIArt.Rect("Fill", track);
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = new Vector2(Mathf.Clamp01(fillAmount), 1f);
        fill.offsetMin = fill.offsetMax = Vector2.zero;
        fill.gameObject.AddComponent<Image>().color = color;
        return fill;
    }


}
