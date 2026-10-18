using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A panel across the top of the screen at the start of each chapter's
// Partner Hour: where she is in her pregnancy or postpartum
// (ChapterData.timelineText), then the main obstacle and the main task.
// It stays up for ChapterData.bannerSeconds (Enter skips it), then fades
// out, and only then does the hour begin: the meters start draining and the
// player can move. A thin bar along its bottom shows the time left.
//
// Builds its own overlay canvas at runtime; GameManager creates one.
public class ChapterBannerUI : MonoBehaviour
{
    static readonly Color PanelColor = new Color32(30, 24, 36, 236);
    static readonly Color BorderColor = new Color32(150, 120, 150, 255);
    static readonly Color HintColor = new Color32(190, 180, 200, 255);
    static readonly Color TimelineColor = new Color32(255, 222, 150, 255);
    static readonly Color TextColor = new Color32(240, 236, 228, 255);
    static readonly Color ObstacleColor = new Color32(168, 160, 236, 255);   // matches the scene text box
    static readonly Color TaskColor = new Color32(239, 159, 39, 255);

    const float FadeIn = 0.35f, FadeOut = 0.45f;

    GameObject root;
    CanvasGroup group;
    RectTransform panel, timer;
    TMP_Text chapterLine, timeline, obstacleText, taskText;
    Action onDone;
    float shownAt, holdSeconds;
    bool closing;
    float closeStartedAt;

    public bool IsShowing => root != null && root.activeSelf;

    void Awake()
    {
        Build();
        root.SetActive(false);
    }

    public void Show(ChapterData data, Action done)
    {
        onDone = done;
        holdSeconds = Mathf.Max(1f, data.bannerSeconds);
        UseSceneFont();

        chapterLine.text = string.IsNullOrWhiteSpace(data.chapterTitle)
            ? $"Chapter {data.chapterNumber}"
            : $"Chapter {data.chapterNumber}  ·  {data.chapterTitle}";
        timeline.text = string.IsNullOrWhiteSpace(data.timelineText) ? data.chapterTitle : data.timelineText;
        obstacleText.text = data.obstacleText;
        taskText.text = data.mainTaskText;

        closing = false;
        group.alpha = 0f;
        root.SetActive(true);
        shownAt = Time.unscaledTime;
    }

    void Update()
    {
        if (!IsShowing) return;
        float t = Time.unscaledTime - shownAt;

        if (!closing)
        {
            group.alpha = Mathf.Clamp01(t / FadeIn);
            float left = 1f - Mathf.Clamp01((t - FadeIn) / holdSeconds);
            timer.anchorMax = new Vector2(left, 1f);
            panel.anchoredPosition = new Vector2(0f, -28f + (1f - group.alpha) * 30f);   // drops in from the top

            bool skip = t > 0.5f && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter));
            if (t >= FadeIn + holdSeconds || skip)
            {
                closing = true;
                closeStartedAt = Time.unscaledTime;
            }
            return;
        }

        float c = (Time.unscaledTime - closeStartedAt) / FadeOut;
        group.alpha = 1f - Mathf.Clamp01(c);
        panel.anchoredPosition = new Vector2(0f, -28f + Mathf.Clamp01(c) * 30f);
        if (c >= 1f)
        {
            root.SetActive(false);
            var cb = onDone;
            onDone = null;
            cb?.Invoke();
        }
    }

    // Use whatever font the game's own text uses (the pixel-art skin swaps
    // it at runtime), so the banner matches.
    void UseSceneFont()
    {
        TMP_FontAsset font = PixelArtSkin.CurrentFont;   // Merriweather
        if (font != null)
        {
            foreach (var t in root.GetComponentsInChildren<TMP_Text>(true)) t.font = font;
            return;
        }
        foreach (var t in FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None))
        {
            if (t.font != null && !t.transform.IsChildOf(root.transform)) { font = t.font; break; }
        }
        if (font == null) return;
        foreach (var t in root.GetComponentsInChildren<TMP_Text>(true)) t.font = font;
    }

    // ---------- UI ----------

    void Build()
    {
        root = new GameObject("ChapterBannerCanvas");
        root.transform.SetParent(transform, false);
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 400;   // over the room and HUD, under full-screen overlays
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        group = root.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;

        // Border, then the panel inside it.
        panel = Rect("Banner", root.transform);
        panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0.5f, 1f);
        panel.sizeDelta = new Vector2(980, 0);   // fits between the meters (left) and the scene text box (right)
        var border = panel.gameObject.AddComponent<Image>();
        border.color = BorderColor;
        var outer = panel.gameObject.AddComponent<VerticalLayoutGroup>();
        outer.padding = new RectOffset(5, 5, 5, 5);
        outer.childControlWidth = outer.childControlHeight = true;
        outer.childForceExpandWidth = true;
        outer.childForceExpandHeight = false;
        panel.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var inner = Rect("Inside", panel);
        inner.gameObject.AddComponent<Image>().color = PanelColor;
        var v = inner.gameObject.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(40, 40, 22, 0);
        v.spacing = 6;
        v.childAlignment = TextAnchor.UpperCenter;
        v.childControlWidth = v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;

        chapterLine = Text(inner, 26, HintColor, TextAlignmentOptions.Center);
        timeline = Text(inner, 46, TimelineColor, TextAlignmentOptions.Center);
        timeline.fontStyle = FontStyles.Bold;

        Spacer(inner, 10);

        // Main obstacle | Main task, side by side
        var cols = Rect("Columns", inner);
        var h = cols.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 40;
        h.childControlWidth = h.childControlHeight = true;
        h.childForceExpandWidth = true;
        h.childForceExpandHeight = false;
        obstacleText = Column(cols, "Main obstacle", ObstacleColor);
        taskText = Column(cols, "Main task", TaskColor);

        Spacer(inner, 22);

        // Time-left bar along the bottom edge.
        var track = Rect("Timer", inner);
        var le = track.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = le.minHeight = 6;
        track.gameObject.AddComponent<Image>().color = new Color32(60, 50, 70, 255);
        timer = Rect("Left", track);
        timer.anchorMin = Vector2.zero;
        timer.anchorMax = Vector2.one;
        timer.offsetMin = timer.offsetMax = Vector2.zero;
        timer.gameObject.AddComponent<Image>().color = TimelineColor;
    }

    TMP_Text Column(RectTransform parent, string heading, Color headingColor)
    {
        var col = Rect(heading, parent);
        var v = col.gameObject.AddComponent<VerticalLayoutGroup>();
        v.spacing = 4;
        v.childControlWidth = v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;
        var label = Text(col, 26, headingColor, TextAlignmentOptions.TopLeft);
        label.text = heading;
        label.fontStyle = FontStyles.Bold;
        return Text(col, 28, TextColor, TextAlignmentOptions.TopLeft);
    }

    static RectTransform Rect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        return rt;
    }

    static TMP_Text Text(RectTransform parent, float size, Color color, TextAlignmentOptions align)
    {
        var t = Rect("Text", parent).gameObject.AddComponent<TextMeshProUGUI>();
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.enableWordWrapping = true;
        t.raycastTarget = false;
        return t;
    }

    static void Spacer(RectTransform parent, float height)
    {
        var le = Rect("Spacer", parent).gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = le.minHeight = height;
    }
}
