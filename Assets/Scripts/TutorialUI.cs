using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The tutorial scene, shown once before Chapter 1: one page, mostly
// pictures.
//   Top:    how to play - movement keys, E to use things / hold E for the
//           main task, the prompt keys. The keycaps light up when pressed,
//           so players can try them.
//   Bottom: her four meters - icon, a draining bar and a few words each.
// Enter (or the button) loads nextSceneName.
//
// Builds all of its UI at runtime; the scene only needs this component (and
// a camera).
public class TutorialUI : MonoBehaviour
{
    public string nextSceneName = "Chapter1";

    GameObject root;
    float shownAt;

    // page 1 animation
    readonly List<KeyCapView> arrowKeys = new List<KeyCapView>();
    readonly List<KeyCapView> wasdKeys = new List<KeyCapView>();
    KeyCapView holdKey;
    RectTransform holdFill;
    float holdProgress;
    float lastRealPress = -10f;

    // page 2 animation
    readonly List<RectTransform> meterFills = new List<RectTransform>();
    Image endBarOutline;

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

        AnimateControls();
        AnimateMeters();
    }

    void Begin() => SceneManager.LoadScene(nextSceneName);

    // ---------- animation ----------

    void AnimateControls()
    {
        // Show the keys being pressed in turn, until the player tries them.
        bool anyHeld = false;
        foreach (var k in arrowKeys) anyHeld |= k.Held;
        foreach (var k in wasdKeys) anyHeld |= k.Held;
        if (anyHeld) lastRealPress = Time.unscaledTime;
        bool demo = Time.unscaledTime - lastRealPress > 2f;
        int step = Mathf.FloorToInt(Time.unscaledTime * 2f) % 4;   // up, left, down, right
        for (int i = 0; i < arrowKeys.Count; i++)
        {
            arrowKeys[i].Demo = demo && i == step;
            wasdKeys[i].Demo = demo && i == step;
        }

        // Hold E: the bar fills while E is held (or on a loop, as a demo).
        if (holdKey.Held) holdProgress = Mathf.Min(1f, holdProgress + Time.unscaledDeltaTime / 2f);
        else holdProgress = Mathf.Repeat(Time.unscaledTime / 2.5f, 1.2f);
        holdKey.Demo = !holdKey.Held && holdProgress < 1f;
        holdFill.anchorMax = new Vector2(Mathf.Clamp01(holdProgress), 1f);
    }

    void AnimateMeters()
    {
        // Each bar drains at its own pace, then refills.
        for (int i = 0; i < meterFills.Count; i++)
        {
            float t = Mathf.Repeat(Time.unscaledTime * (0.12f + i * 0.03f) + i * 0.21f, 1f);
            meterFills[i].anchorMax = new Vector2(1f - t * 0.85f, 1f);
        }
        if (endBarOutline != null)
        {
            bool on = Mathf.Repeat(Time.unscaledTime * 2f, 1f) < 0.5f;
            endBarOutline.color = on ? new Color32(255, 120, 110, 255) : new Color32(110, 92, 116, 255);
        }
    }

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
        var head = UIArt.Row(parent, "Heading", 24, TextAnchor.LowerCenter);
        SectionTitle(head.transform, "How to play");
        UIArt.Text(head.transform, "(try the keys!)", 28, UIArt.HintColor).enableWordWrapping = false;
        var cols = UIArt.Row(parent, "Columns", 80, TextAnchor.UpperCenter);

        // Move
        var move = Column(cols.transform, "Move");
        var clusters = UIArt.Row(move.transform, "Clusters", 30);
        KeyCluster(clusters.transform, arrowKeys, null,
            new[] { KeyCode.UpArrow, KeyCode.LeftArrow, KeyCode.DownArrow, KeyCode.RightArrow });
        UIArt.Text(clusters.transform, "or", 30, UIArt.HintColor);
        KeyCluster(clusters.transform, wasdKeys, new[] { "W", "A", "S", "D" },
            new[] { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D });

        // Use things
        var use = Column(cols.transform, "Use things");
        var tapRow = UIArt.Row(use.transform, "Tap", 20, TextAnchor.MiddleLeft);
        UIArt.Key(tapRow.transform, "E", 76, -1, -1, KeyCode.E);
        UIArt.Size(UIArt.Text(tapRow.transform, "press", 30, UIArt.TextColor, TextAlignmentOptions.Left), 90, -1);
        UIArt.Icon(tapRow.transform, "trace_marker", 44);
        UIArt.Size(UIArt.Text(tapRow.transform, "= you can use it", 26, UIArt.HintColor, TextAlignmentOptions.Left), 200, -1);
        var holdRow = UIArt.Row(use.transform, "Hold", 20, TextAnchor.MiddleLeft);
        holdKey = UIArt.Key(holdRow.transform, "E", 76, -1, -1, KeyCode.E);
        var holdCol = UIArt.Column(holdRow.transform, "HoldBar", 8, TextAnchor.MiddleLeft);
        UIArt.Size(UIArt.Text(holdCol.transform, "hold for the main task", 30, UIArt.TextColor, TextAlignmentOptions.Left), 330, -1);
        holdFill = Bar(holdCol.transform, 330, 20, UIArt.TitleColor);

        // Respond to prompts
        var respond = Column(cols.transform, "Respond to prompts");
        var spaceRow = UIArt.Row(respond.transform, "Space", 20, TextAnchor.MiddleLeft);
        UIArt.Key(spaceRow.transform, "SPACE", 76, 220, -1, KeyCode.Space);
        UIArt.Size(UIArt.Text(spaceRow.transform, "hold", 30, UIArt.TextColor, TextAlignmentOptions.Left), 80, -1);
        var tapKeys = UIArt.Row(respond.transform, "QF", 20, TextAnchor.MiddleLeft);
        UIArt.Key(tapKeys.transform, "Q", 76, -1, -1, KeyCode.Q);
        UIArt.Key(tapKeys.transform, "F", 76, -1, -1, KeyCode.F);
        UIArt.Size(UIArt.Text(tapKeys.transform, "tap", 30, UIArt.TextColor, TextAlignmentOptions.Left), 80, -1);
        UIArt.Size(UIArt.Text(respond.transform, "Keys change each chapter.", 24, UIArt.HintColor, TextAlignmentOptions.Left), 360, -1);
    }

    void KeyCluster(Transform parent, List<KeyCapView> into, string[] letters, KeyCode[] keys)
    {
        // [ up ] over [ left down right ], stored in the order up, left, down, right
        var col = UIArt.Column(parent, "Cluster", 8);
        var top = UIArt.Row(col.transform, "Top", 8);
        var bottom = UIArt.Row(col.transform, "Bottom", 8);
        float[] angles = { 0f, 90f, 180f, 270f };
        for (int i = 0; i < 4; i++)
        {
            var row = i == 0 ? top.transform : bottom.transform;
            var key = letters == null
                ? UIArt.Key(row, "arrow", 64, -1, angles[i], keys[i])
                : UIArt.Key(row, letters[i], 64, -1, -1, keys[i]);
            into.Add(key);
        }
    }

    // Bottom: her four meters -------------------------------------------------

    void BuildMeters(Transform parent)
    {
        SectionTitle(parent, "Her four meters");

        // Two columns of two meters each.
        string[] words = { "her energy", "her focus", "feeling like herself", "looking fine to others" };
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
                var nameRow = UIArt.Row(text.transform, "Name", 16, TextAnchor.LowerLeft);
                UIArt.Text(nameRow.transform, m.name, 36, m.color, TextAlignmentOptions.Left, true).enableWordWrapping = false;
                UIArt.Text(nameRow.transform, words[i], 28, UIArt.HintColor, TextAlignmentOptions.Left).enableWordWrapping = false;
                meterFills.Add(Bar(text.transform, 600, 24, m.color));
            }
        }

        // Last line: what happens when one runs out, and the Start button.
        var bottom = UIArt.Row(parent, "Bottom", 60, TextAnchor.MiddleCenter);
        var end = UIArt.Row(bottom.transform, "End", 22);
        var track = UIArt.Rect("EmptyBar", end.transform);
        UIArt.Size(track, 180, 26);
        endBarOutline = track.gameObject.AddComponent<Image>();
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

    static RectTransform Bar(Transform parent, float width, float height, Color color)
    {
        var track = UIArt.Rect("Bar", parent);
        UIArt.Size(track, width, height);
        track.gameObject.AddComponent<Image>().color = new Color32(30, 24, 36, 255);
        var fill = UIArt.Rect("Fill", track);
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = Vector2.one;
        fill.offsetMin = fill.offsetMax = Vector2.zero;
        fill.gameObject.AddComponent<Image>().color = color;
        return fill;
    }


}
