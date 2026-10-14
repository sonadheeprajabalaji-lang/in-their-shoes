using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The tutorial scene, shown once before Chapter 1. Three short pages,
// mostly pictures:
//   1. How to play  - movement keys, E to use things / hold E for the main
//                     task, the prompt keys. The keycaps light up when
//                     pressed, so players can try them.
//   2. Her meters   - the four meters with an icon, a draining bar and a
//                     few words each.
//   3. Each chapter - Partner Hour (play her) then Self Hour (play her
//                     partner), with the star marker.
// Enter (or the button) moves on; Backspace goes back. The last page loads
// nextSceneName.
//
// Builds all of its UI at runtime; the scene only needs this component (and
// a camera).
public class TutorialUI : MonoBehaviour
{
    public string nextSceneName = "Chapter1";

    GameObject root;
    readonly List<GameObject> pages = new List<GameObject>();
    readonly List<Image> dots = new List<Image>();
    int page;
    Button backButton;
    TMP_Text nextLabel;
    float pageShownAt;

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
        ShowPage(0);
    }

    void Update()
    {
        if (Time.unscaledTime - pageShownAt > 0.3f)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) Next();
            else if (Input.GetKeyDown(KeyCode.Backspace)) Back();
        }

        if (page == 0) AnimateControls();
        if (page == 1) AnimateMeters();
    }

    void Next()
    {
        if (page < pages.Count - 1) ShowPage(page + 1);
        else SceneManager.LoadScene(nextSceneName);
    }

    void Back()
    {
        if (page > 0) ShowPage(page - 1);
    }

    void ShowPage(int i)
    {
        page = i;
        for (int p = 0; p < pages.Count; p++) pages[p].SetActive(p == i);
        for (int d = 0; d < dots.Count; d++) dots[d].color = d == i ? UIArt.TitleColor : new Color(1, 1, 1, 0.25f);
        backButton.gameObject.SetActive(i > 0);
        nextLabel.text = i < pages.Count - 1 ? "Next  (Enter)" : "Start Chapter 1  (Enter)";
        pageShownAt = Time.unscaledTime;
    }

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

        var card = (RectTransform)UIArt.Panel(root.transform, "Card").transform;
        card.anchorMin = card.anchorMax = card.pivot = new Vector2(0.5f, 0.5f);
        card.sizeDelta = new Vector2(1600, 900);

        pages.Add(BuildControlsPage(card));
        pages.Add(BuildMetersPage(card));
        pages.Add(BuildChapterPage(card));

        BuildFooter(card);
    }

    RectTransform NewPage(RectTransform card, string title)
    {
        var p = UIArt.Rect("Page_" + title, card);
        UIArt.Stretch(p);
        p.offsetMin = new Vector2(70, 140);   // leave room for the footer
        p.offsetMax = new Vector2(-70, -50);

        var t = UIArt.Text(p, title, 58, UIArt.TitleColor, TextAlignmentOptions.Center, true);
        var trt = (RectTransform)t.transform;
        trt.anchorMin = new Vector2(0, 1);
        trt.anchorMax = new Vector2(1, 1);
        trt.pivot = new Vector2(0.5f, 1);
        trt.sizeDelta = new Vector2(0, 80);

        var body = UIArt.Rect("Body", p);
        UIArt.Stretch(body);
        body.offsetMax = new Vector2(0, -110);
        return body;
    }

    // Page 1 ---------------------------------------------------------------

    GameObject BuildControlsPage(RectTransform card)
    {
        var body = NewPage(card, "How to play");
        var cols = UIArt.Row(body, "Columns", 50, TextAnchor.UpperCenter);
        UIArt.Stretch((RectTransform)cols.transform);

        // Move
        var move = Column(cols.transform, "Move");
        var clusters = UIArt.Row(move.transform, "Clusters", 40);
        KeyCluster(clusters.transform, arrowKeys, null,
            new[] { KeyCode.UpArrow, KeyCode.LeftArrow, KeyCode.DownArrow, KeyCode.RightArrow });
        UIArt.Text(clusters.transform, "or", 32, UIArt.HintColor);
        KeyCluster(clusters.transform, wasdKeys, new[] { "W", "A", "S", "D" },
            new[] { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D });
        Gap(move.transform, 30);
        UIArt.Icon(move.transform, "her_side_0", 150);

        // Use things
        var use = Column(cols.transform, "Use things");
        var tapRow = UIArt.Row(use.transform, "Tap", 24);
        UIArt.Key(tapRow.transform, "E", 96, -1, -1, KeyCode.E);
        UIArt.Size(UIArt.Text(tapRow.transform, "press", 32, UIArt.TextColor, TextAlignmentOptions.Left), 150, -1);

        var markerRow = UIArt.Row(use.transform, "Marker", 20);
        UIArt.Icon(markerRow.transform, "trace_marker", 56);
        UIArt.Size(UIArt.Text(markerRow.transform, "= you can use it", 30, UIArt.HintColor, TextAlignmentOptions.Left), 260, -1);

        Gap(use.transform, 20);
        var holdRow = UIArt.Row(use.transform, "Hold", 24);
        holdKey = UIArt.Key(holdRow.transform, "E", 96, -1, -1, KeyCode.E);
        var holdCol = UIArt.Column(holdRow.transform, "HoldBar", 10, TextAnchor.MiddleLeft);
        UIArt.Size(UIArt.Text(holdCol.transform, "hold for the main task", 30, UIArt.TextColor, TextAlignmentOptions.Left), 270, -1);
        holdFill = Bar(holdCol.transform, 270, 22, UIArt.TitleColor);

        // Respond to prompts
        var respond = Column(cols.transform, "Respond");
        UIArt.Size(UIArt.Text(respond.transform, "when prompts pop up", 30, UIArt.HintColor), 380, -1);
        Gap(respond.transform, 10);
        var spaceRow = UIArt.Row(respond.transform, "Space", 24);
        UIArt.Key(spaceRow.transform, "SPACE", 96, 260, -1, KeyCode.Space);
        UIArt.Size(UIArt.Text(spaceRow.transform, "hold", 32, UIArt.TextColor, TextAlignmentOptions.Left), 90, -1);
        var tapKeys = UIArt.Row(respond.transform, "QF", 24);
        UIArt.Key(tapKeys.transform, "Q", 96, -1, -1, KeyCode.Q);
        UIArt.Key(tapKeys.transform, "F", 96, -1, -1, KeyCode.F);
        UIArt.Size(UIArt.Text(tapKeys.transform, "tap", 32, UIArt.TextColor, TextAlignmentOptions.Left), 90, -1);
        Gap(respond.transform, 10);
        UIArt.Size(UIArt.Text(respond.transform, "Each chapter shows its own keys.", 26, UIArt.HintColor), 380, -1);

        Hint(body, "Try the keys!");
        return body.parent.gameObject;
    }

    void KeyCluster(Transform parent, List<KeyCapView> into, string[] letters, KeyCode[] keys)
    {
        // [ up ] over [ left down right ], stored in the order up, left, down, right
        var col = UIArt.Column(parent, "Cluster", 10);
        var top = UIArt.Row(col.transform, "Top", 10);
        var bottom = UIArt.Row(col.transform, "Bottom", 10);
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

    // Page 2 ---------------------------------------------------------------

    GameObject BuildMetersPage(RectTransform card)
    {
        var body = NewPage(card, "Her four meters");
        var list = UIArt.Column(body, "Meters", 34, TextAnchor.UpperCenter);
        UIArt.Stretch((RectTransform)list.transform);
        list.padding = new RectOffset(0, 0, 10, 0);

        string[] words = { "her energy", "her focus", "feeling like herself", "looking fine to others" };
        for (int i = 0; i < UIArt.Meters.Length; i++)
        {
            var m = UIArt.Meters[i];
            var row = UIArt.Row(list.transform, m.name, 30, TextAnchor.MiddleLeft);
            UIArt.Icon(row.transform, m.icon, 76);
            UIArt.Size(UIArt.Text(row.transform, m.name, 40, m.color, TextAlignmentOptions.Left, true), 420, -1);
            meterFills.Add(Bar(row.transform, 460, 30, m.color));
            UIArt.Size(UIArt.Text(row.transform, words[i], 32, UIArt.HintColor, TextAlignmentOptions.Left), 360, -1);
        }

        Gap(list.transform, 20);
        var end = UIArt.Row(list.transform, "End", 26);
        var track = UIArt.Rect("EmptyBar", end.transform);
        UIArt.Size(track, 220, 30);
        endBarOutline = track.gameObject.AddComponent<Image>();
        var inner = UIArt.Rect("Inside", track);
        UIArt.Stretch(inner);
        inner.offsetMin = new Vector2(4, 4);
        inner.offsetMax = new Vector2(-4, -4);
        inner.gameObject.AddComponent<Image>().color = new Color32(30, 24, 36, 255);
        UIArt.Size(UIArt.Text(end.transform, "One runs out: her hour ends.", 36, UIArt.TitleColor, TextAlignmentOptions.Left, true), 620, -1);

        return body.parent.gameObject;
    }

    // Page 3 ---------------------------------------------------------------

    GameObject BuildChapterPage(RectTransform card)
    {
        var body = NewPage(card, "Every chapter");
        var row = UIArt.Row(body, "Hours", 40, TextAnchor.MiddleCenter);
        var rrt = (RectTransform)row.transform;
        UIArt.Stretch(rrt);
        rrt.offsetMin = new Vector2(0, 80);

        HourCard(row.transform, "her_down_0", "Partner Hour", "You play her",
            k => UIArt.Key(k, "E", 64, -1, -1, KeyCode.E), "Main task first",
            k => UIArt.Icon(k, "trace_marker", 56), "Then the chores");

        var arrow = UIArt.Icon(row.transform, "icon_arrow", 90);
        arrow.color = UIArt.HintColor;
        arrow.rectTransform.localEulerAngles = new Vector3(0, 0, 270);

        HourCard(row.transform, "partner_down_0", "Self Hour", "You play her partner",
            k => UIArt.Icon(k, "trace_marker", 56), "Find what she left",
            k => UIArt.Key(k, "E", 64, -1, -1, KeyCode.E), "Choose how to help");

        var note = UIArt.Text(body, "What you choose changes her next chapter.", 34, UIArt.TitleColor, TextAlignmentOptions.Center, true);
        var nrt = (RectTransform)note.transform;
        nrt.anchorMin = new Vector2(0, 0);
        nrt.anchorMax = new Vector2(1, 0);
        nrt.pivot = new Vector2(0.5f, 0);
        nrt.sizeDelta = new Vector2(0, 60);
        nrt.anchoredPosition = new Vector2(0, 10);

        return body.parent.gameObject;
    }

    void HourCard(Transform parent, string sprite, string title, string subtitle,
                  System.Action<Transform> iconA, string textA, System.Action<Transform> iconB, string textB)
    {
        var panel = UIArt.Panel(parent, title);
        UIArt.Size(panel, 560, 500);
        var col = UIArt.Column(panel.transform, "Inside", 14);
        UIArt.Stretch((RectTransform)col.transform);
        col.padding = new RectOffset(40, 40, 30, 26);

        UIArt.Icon(col.transform, sprite, 120);
        UIArt.Text(col.transform, title, 40, UIArt.TitleColor, TextAlignmentOptions.Center, true);
        UIArt.Text(col.transform, subtitle, 30, UIArt.HintColor);
        Gap(col.transform, 10);
        IconLine(col.transform, iconA, textA);
        IconLine(col.transform, iconB, textB);
    }

    static void IconLine(Transform parent, System.Action<Transform> icon, string text)
    {
        var r = UIArt.Row(parent, text, 20, TextAnchor.MiddleLeft);
        icon(r.transform);
        UIArt.Size(UIArt.Text(r.transform, text, 32, UIArt.TextColor, TextAlignmentOptions.Left), 330, -1);
    }

    // Footer ---------------------------------------------------------------

    void BuildFooter(RectTransform card)
    {
        var footer = UIArt.Rect("Footer", card);
        footer.anchorMin = new Vector2(0, 0);
        footer.anchorMax = new Vector2(1, 0);
        footer.pivot = new Vector2(0.5f, 0);
        footer.sizeDelta = new Vector2(-140, 90);
        footer.anchoredPosition = new Vector2(0, 40);

        backButton = MakeButton(footer, "Back", 240, new Vector2(0, 0.5f), Back, out _);
        MakeButton(footer, "Next", 460, new Vector2(1, 0.5f), Next, out nextLabel);

        var dotRow = UIArt.Row(footer, "Dots", 18);
        var drt = (RectTransform)dotRow.transform;
        drt.anchorMin = drt.anchorMax = new Vector2(0.5f, 0.5f);
        drt.sizeDelta = new Vector2(200, 30);
        for (int i = 0; i < 3; i++)
        {
            var d = UIArt.Rect("Dot", dotRow.transform).gameObject.AddComponent<Image>();
            UIArt.Size(d, 18, 18);
            dots.Add(d);
        }
    }

    Button MakeButton(RectTransform parent, string label, float width, Vector2 anchor, UnityEngine.Events.UnityAction onClick, out TMP_Text text)
    {
        var img = UIArt.Panel(parent, label + "Button", "ui_button", 1f);
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.sizeDelta = new Vector2(width, 80);
        var button = img.gameObject.AddComponent<Button>();
        button.onClick.AddListener(onClick);
        text = UIArt.Text(rt, label, 32, UIArt.KeyText);
        UIArt.Stretch((RectTransform)text.transform);
        return button;
    }

    // Helpers --------------------------------------------------------------

    VerticalLayoutGroup Column(Transform parent, string heading)
    {
        var col = UIArt.Column(parent, heading, 22);
        UIArt.Size(UIArt.Text(col.transform, heading, 42, UIArt.TitleColor, TextAlignmentOptions.Center, true), 420, -1);
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

    static void Gap(Transform parent, float h) => UIArt.Size(UIArt.Rect("Gap", parent), -1, h);

    static void Hint(RectTransform body, string text)
    {
        var t = UIArt.Text(body, text, 30, UIArt.HintColor);
        var rt = (RectTransform)t.transform;
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.sizeDelta = new Vector2(0, 50);
    }
}
