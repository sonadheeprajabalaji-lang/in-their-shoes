using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Shown when the Partner Hour ends, before the Self Hour (replaces the plain
// HourTransitionUI message). It's the hand-over from her to you: the player
// is about to play the non-birthing partner, so this shows them what she
// went through that they didn't see:
//
//   - the chapter's line for the end of the hour (hourTransitionText)
//   - her four meters as they ended, with the one that ran out marked
//   - what she was trying to do (the main task, finished or not, and the
//     chores she got done)
//   - why the meter that ran out drained, from ChapterData's drain stories
//
// Builds its own overlay canvas at runtime, like MeterIntroUI, so nothing
// is needed in the scene. Space, Enter or a click continues (after a short
// pause, so it can't be skipped by accident).
public class PartnerHourRecapUI : MonoBehaviour
{
    // Same colours as the HUD bars (see PixelArtSkin / MeterIntroUI).
    static readonly Color BodyColor = new Color32(216, 90, 48, 255);
    static readonly Color MindColor = new Color32(239, 159, 39, 255);
    static readonly Color PersonColor = new Color32(29, 158, 117, 255);
    static readonly Color MaskColor = new Color32(127, 119, 221, 255);

    static readonly Color Backdrop = new Color(0.07f, 0.05f, 0.09f, 0.96f);
    static readonly Color PanelFallback = new Color32(43, 34, 51, 255);
    static readonly Color TitleColor = new Color32(248, 246, 240, 255);
    static readonly Color BodyText = new Color32(232, 226, 236, 255);
    static readonly Color HintText = new Color32(190, 180, 200, 255);
    static readonly Color EmptyTrack = new Color32(30, 24, 36, 255);
    static readonly Color RanOutColor = new Color32(255, 120, 110, 255);

    [Tooltip("Seconds before a key press can continue.")]
    public float minShowSeconds = 2f;

    GameObject root;
    TMP_Text kicker, title, story, hint;
    readonly List<Row> rows = new List<Row>();
    Action onDone;
    float shownAt;

    class Row
    {
        public TMP_Text name, status;
        public RectTransform fill;
        public Image fillImage;
        public Color color;
    }

    public bool IsShowing => root != null && root.activeSelf;

    void Awake()
    {
        Build();
        root.SetActive(false);
    }

    // values: body, mind, feeling like a person, mask (0..1) as the hour ended.
    // mainTask: the main task's name; mainTaskDone: held all the way through.
    // choresDone: names of the chores she finished.
    public void Show(ChapterData data, float[] values, string mainTask, bool mainTaskDone,
                     IList<string> choresDone, Action done)
    {
        onDone = done;
        ApplyFont();

        kicker.text = "Her hour";
        title.text = string.IsNullOrWhiteSpace(data.hourTransitionText)
            ? "She's become too tired to continue."
            : data.hourTransitionText;

        // Which meter ran out? If the hour ended on time instead, the lowest.
        int worst = 0;
        for (int i = 1; i < values.Length; i++) if (values[i] < values[worst]) worst = i;
        bool ranOut = values[worst] <= 0.001f;

        for (int i = 0; i < rows.Count && i < values.Length; i++)
        {
            var row = rows[i];
            float v = Mathf.Clamp01(values[i]);
            row.fill.anchorMax = new Vector2(v, 1f);
            if (values[i] <= 0.001f)
            {
                row.status.text = "Ran out";
                row.status.color = RanOutColor;
            }
            else if (v < 0.25f)
            {
                row.status.text = "Almost empty";
                row.status.color = HintText;
            }
            else
            {
                row.status.text = "";
            }
            // Dim the rows that didn't run out so the empty one stands out.
            bool focus = values[i] <= 0.001f || (!ranOut && i == worst);
            row.name.color = focus ? row.color : Color.Lerp(row.color, EmptyTrack, 0.45f);
            row.fillImage.color = focus ? row.color : Color.Lerp(row.color, EmptyTrack, 0.45f);
        }

        var sb = new System.Text.StringBuilder();
        if (!string.IsNullOrEmpty(mainTask))
        {
            sb.Append("<b>She was trying to</b> ").Append(LowerFirst(mainTask))
              .Append(mainTaskDone ? " — and got it done." : " — but couldn't finish it.")
              .Append('\n');
        }
        sb.Append("<b>She also managed:</b> ")
          .Append(choresDone != null && choresDone.Count > 0 ? JoinLower(choresDone) : "nothing else.")
          .Append("\n\n");

        for (int i = 0; i < values.Length; i++)
        {
            bool empty = values[i] <= 0.001f;
            if (!empty && !(i == worst && !ranOut)) continue;
            string meter = MeterName(i);
            string why = DrainStory(data, i);
            sb.Append("<b><color=#").Append(ColorUtility.ToHtmlStringRGB(MeterColor(i))).Append('>')
              .Append(meter).Append(empty ? " ran out." : " came closest to running out.")
              .Append("</color></b> ");
            if (!string.IsNullOrWhiteSpace(why)) sb.Append(why.Trim());
            sb.Append('\n');
        }
        story.text = sb.ToString().TrimEnd();

        hint.text = "Now you come home. Look for what she couldn't get to.   (Space to continue)";

        root.SetActive(true);
        shownAt = Time.unscaledTime;
    }

    void Update()
    {
        if (!IsShowing) return;
        if (Time.unscaledTime - shownAt < minShowSeconds) return;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0))
        {
            Finish();
        }
    }

    void Finish()
    {
        if (!IsShowing) return;
        root.SetActive(false);
        var cb = onDone;
        onDone = null;
        cb?.Invoke();
    }

    static string LowerFirst(string s) =>
        string.IsNullOrEmpty(s) || (s.Length > 1 && char.IsUpper(s[1])) ? s : char.ToLowerInvariant(s[0]) + s.Substring(1);

    static string JoinLower(IList<string> items)
    {
        var parts = new List<string>();
        foreach (var item in items) parts.Add(LowerFirst(item));
        if (parts.Count == 1) return parts[0] + ".";
        return string.Join(", ", parts.GetRange(0, parts.Count - 1)) + " and " + parts[parts.Count - 1] + ".";
    }

    static string MeterName(int i) => i switch
    {
        0 => "Body",
        1 => "Mind",
        2 => "Feeling like a person",
        _ => "The Mask",
    };

    static Color MeterColor(int i) => i switch
    {
        0 => BodyColor,
        1 => MindColor,
        2 => PersonColor,
        _ => MaskColor,
    };

    static string DrainStory(ChapterData d, int i) => i switch
    {
        0 => d.bodyDrainStory,
        1 => d.mindDrainStory,
        2 => d.personDrainStory,
        _ => d.maskDrainStory,
    };

    // The skin may load its font after this was built; pick it up on show.
    void ApplyFont()
    {
        var font = PixelArtSkin.CurrentFont;
        if (font == null) return;
        foreach (var t in root.GetComponentsInChildren<TMP_Text>(true)) t.font = font;
    }

    // ---------- UI construction ----------

    void Build()
    {
        root = new GameObject("PartnerHourRecapCanvas");
        root.transform.SetParent(transform, false);
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300;   // same layer as the hour transition
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        var backdrop = NewRect("Backdrop", root.transform);
        Stretch(backdrop);
        backdrop.gameObject.AddComponent<Image>().color = Backdrop;

        var card = NewRect("Card", root.transform);
        card.anchorMin = card.anchorMax = card.pivot = new Vector2(0.5f, 0.5f);
        card.sizeDelta = new Vector2(1400, 0);
        var cardImg = card.gameObject.AddComponent<Image>();
        var panelSprite = Resources.Load<Sprite>("PixelArt/ui_panel");
        if (panelSprite != null)
        {
            cardImg.sprite = panelSprite;
            cardImg.type = Image.Type.Sliced;
            cardImg.pixelsPerUnitMultiplier = 0.5f;
        }
        else cardImg.color = PanelFallback;

        var layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(64, 64, 48, 44);
        layout.spacing = 14;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        card.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        kicker = NewText("Kicker", card, 26, HintText);
        title = NewText("Title", card, 42, TitleColor);
        title.fontStyle = FontStyles.Bold;
        Spacer(card, 6);

        AddRow(card, "Body", BodyColor);
        AddRow(card, "Mind", MindColor);
        AddRow(card, "Feeling like a person", PersonColor);
        AddRow(card, "The Mask", MaskColor);

        Spacer(card, 8);
        story = NewText("Story", card, 29, BodyText);
        story.richText = true;
        Spacer(card, 4);
        hint = NewText("Hint", card, 24, HintText);
        hint.alignment = TextAlignmentOptions.Right;
    }

    void AddRow(RectTransform parent, string name, Color color)
    {
        // [ name ] [ bar ............ ] [ status ]
        var row = NewRect("Meter_" + name, parent);
        var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 24;
        h.childAlignment = TextAnchor.MiddleLeft;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = false;

        var label = NewText("Name", row, 30, color);
        label.text = name;
        var le = label.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = le.minWidth = 360;

        var track = NewRect("Bar", row);
        var trackLe = track.gameObject.AddComponent<LayoutElement>();
        trackLe.flexibleWidth = 1;
        trackLe.preferredHeight = trackLe.minHeight = 22;
        track.gameObject.AddComponent<Image>().color = EmptyTrack;

        var fill = NewRect("Fill", track);
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = new Vector2(1f, 1f);
        fill.offsetMin = fill.offsetMax = Vector2.zero;
        var fillImg = fill.gameObject.AddComponent<Image>();
        fillImg.color = color;

        var status = NewText("Status", row, 26, RanOutColor);
        status.fontStyle = FontStyles.Bold;
        var sle = status.gameObject.AddComponent<LayoutElement>();
        sle.preferredWidth = sle.minWidth = 220;

        rows.Add(new Row { name = label, status = status, fill = fill, fillImage = fillImg, color = color });
    }

    static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        return rt;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static TMP_Text NewText(string name, RectTransform parent, float size, Color color)
    {
        var rt = NewRect(name, parent);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.fontSize = size;
        t.color = color;
        t.enableWordWrapping = true;
        t.alignment = TextAlignmentOptions.TopLeft;
        t.lineSpacing = 6;
        t.raycastTarget = false;
        return t;
    }

    static void Spacer(RectTransform parent, float height)
    {
        var le = NewRect("Spacer", parent).gameObject.AddComponent<LayoutElement>();
        le.minHeight = le.preferredHeight = height;
    }
}
