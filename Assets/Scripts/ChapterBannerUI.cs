using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A panel across the top of the screen at the start of each chapter's
// Partner Hour: where she is in her pregnancy or postpartum
// (ChapterData.timelineText), then a short story that sets the scene and
// ends on the main task (ChapterData.storyText). It stays up until the
// player presses Enter; then it fades out and the hour begins: the meters
// start draining and she can move.
//
// Builds its own overlay canvas at runtime; GameManager creates one.
public class ChapterBannerUI : MonoBehaviour
{
    static readonly Color PanelColor = new Color32(30, 24, 36, 240);
    static readonly Color BorderColor = new Color32(150, 120, 150, 255);
    static readonly Color HintColor = new Color32(190, 180, 200, 255);
    static readonly Color TimelineColor = new Color32(255, 222, 150, 255);
    static readonly Color TextColor = new Color32(240, 236, 228, 255);

    const float FadeIn = 0.35f, FadeOut = 0.45f;

    GameObject root;
    CanvasGroup group;
    RectTransform panel;
    TMP_Text chapterLine, timeline, story, hint;
    Action onDone;
    float shownAt;
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
        UseSceneFont();

        chapterLine.text = string.IsNullOrWhiteSpace(data.chapterTitle)
            ? $"Chapter {data.chapterNumber}"
            : $"Chapter {data.chapterNumber}  ·  {data.chapterTitle}";
        timeline.text = string.IsNullOrWhiteSpace(data.timelineText) ? data.chapterTitle : data.timelineText;

        // No story written yet: fall back to the obstacle and the main task.
        story.text = !string.IsNullOrWhiteSpace(data.storyText)
            ? data.storyText.Trim()
            : $"{data.obstacleText}\n\n<b>Main task:</b> {data.mainTaskText}";

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
            panel.anchoredPosition = new Vector2(0f, -28f + (1f - group.alpha) * 30f);   // drops in from the top

            // A gentle pulse on the hint once it's had time to be read.
            var c = hint.color;
            c.a = t < 1.5f ? 0.85f : 0.6f + 0.4f * (0.5f + 0.5f * Mathf.Sin((t - 1.5f) * 3f));
            hint.color = c;

            // The short pause stops the Enter from the previous screen
            // skipping the story.
            if (t > 0.6f && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            {
                closing = true;
                closeStartedAt = Time.unscaledTime;
            }
            return;
        }

        float k = (Time.unscaledTime - closeStartedAt) / FadeOut;
        group.alpha = 1f - Mathf.Clamp01(k);
        panel.anchoredPosition = new Vector2(0f, -28f + Mathf.Clamp01(k) * 30f);
        if (k >= 1f)
        {
            root.SetActive(false);
            var cb = onDone;
            onDone = null;
            cb?.Invoke();
        }
    }

    // Use the game's font (Merriweather, made at runtime by the pixel-art
    // skin), or failing that whatever the game's own text uses.
    void UseSceneFont()
    {
        TMP_FontAsset font = PixelArtSkin.CurrentFont;
        if (font == null)
        {
            foreach (var t in FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None))
            {
                if (t.font != null && !t.transform.IsChildOf(root.transform)) { font = t.font; break; }
            }
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
        panel.gameObject.AddComponent<Image>().color = BorderColor;
        var outer = panel.gameObject.AddComponent<VerticalLayoutGroup>();
        outer.padding = new RectOffset(5, 5, 5, 5);
        outer.childControlWidth = outer.childControlHeight = true;
        outer.childForceExpandWidth = true;
        outer.childForceExpandHeight = false;
        panel.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var inner = Rect("Inside", panel);
        inner.gameObject.AddComponent<Image>().color = PanelColor;
        var v = inner.gameObject.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(48, 48, 24, 24);
        v.spacing = 6;
        v.childAlignment = TextAnchor.UpperCenter;
        v.childControlWidth = v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;

        chapterLine = Text(inner, 24, HintColor, TextAlignmentOptions.Center);
        timeline = Text(inner, 44, TimelineColor, TextAlignmentOptions.Center);
        timeline.fontStyle = FontStyles.Bold;
        Spacer(inner, 10);
        story = Text(inner, 28, TextColor, TextAlignmentOptions.TopLeft);
        story.lineSpacing = 8;
        Spacer(inner, 12);
        hint = Text(inner, 24, HintColor, TextAlignmentOptions.Right);
        hint.text = "Press Enter to begin";
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
        t.richText = true;
        t.raycastTarget = false;
        return t;
    }

    static void Spacer(RectTransform parent, float height)
    {
        var le = Rect("Spacer", parent).gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = le.minHeight = height;
    }
}
