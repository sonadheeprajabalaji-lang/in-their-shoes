using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A short screen between the content warning and the Partner Hour that
// explains her four meters, each next to a bar in its own colour, plus the
// controls. Enter, Space or the Start button begins the chapter.
//
// Builds its own UI at runtime (its own overlay canvas), so nothing needs
// to be set up in the scene. GameManager creates one automatically and
// shows it when the chapter's ChapterData.showMeterIntro is ticked.
public class MeterIntroUI : MonoBehaviour
{
    // Meter colours match the HUD (see PixelArtSkin).
    static readonly Color BodyColor = new Color32(216, 90, 48, 255);
    static readonly Color MindColor = new Color32(239, 159, 39, 255);
    static readonly Color PersonColor = new Color32(29, 158, 117, 255);
    static readonly Color MaskColor = new Color32(127, 119, 221, 255);

    static readonly Color Backdrop = new Color32(26, 21, 31, 255);
    static readonly Color PanelFallback = new Color32(43, 34, 51, 255);
    static readonly Color TitleColor = new Color32(248, 246, 240, 255);
    static readonly Color BodyText = new Color32(232, 226, 236, 255);
    static readonly Color HintText = new Color32(190, 180, 200, 255);
    static readonly Color ButtonText = new Color32(43, 34, 51, 255);

    static readonly (string name, Color color, string meaning)[] Meters =
    {
        ("Body", BodyColor, "Her physical energy. Nausea, tiredness and chores wear it down."),
        ("Mind", MindColor, "Her focus. Juggling the call, the chores and the worry drains it."),
        ("Feeling like a person", PersonColor, "Whether she still feels like herself. It drops when she can't eat, rest or do anything for herself."),
        ("The Mask", MaskColor, "How well she can keep looking fine to everyone else. Every time she hides how she feels, it slips."),
    };

    GameObject root;
    TMP_Text title;
    TMP_Text note;
    TMP_Text buttonLabel;
    Action onStart;
    float shownAt;

    public bool IsShowing => root != null && root.activeSelf;

    void Awake()
    {
        Build();
        root.SetActive(false);
    }

    // chapterNote: an optional chapter-specific line (controls, the main task).
    public void Show(int chapterNumber, string chapterNote, Action start)
    {
        onStart = start;
        title.text = "Before you start: her four meters";
        note.text = string.IsNullOrWhiteSpace(chapterNote) ? "" : chapterNote.Trim();
        note.gameObject.SetActive(!string.IsNullOrWhiteSpace(chapterNote));
        buttonLabel.text = $"Start Chapter {chapterNumber}  (Enter)";
        root.SetActive(true);
        shownAt = Time.unscaledTime;
    }

    void Update()
    {
        if (!IsShowing) return;
        // Ignore the first moment so the Enter or click that left the
        // content warning can't also skip this screen.
        if (Time.unscaledTime - shownAt < 0.4f) return;
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
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

    // ---------- UI construction ----------

    void Build()
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
        else
        {
            cardImg.color = PanelFallback;
        }

        var layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(64, 64, 52, 48);
        layout.spacing = 14;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        card.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        title = NewText("Title", card, 46, TitleColor);
        var intro = NewText("Intro", card, 30, BodyText);
        intro.text = "In the Partner Hour you play as her. These meters show how she's coping. " +
                     "You can't keep them all full: when any one runs out, the hour ends.";
        Spacer(card, 8);

        foreach (var m in Meters) MeterRow(card, m.name, m.color, m.meaning);

        Spacer(card, 6);
        note = NewText("ChapterNote", card, 28, HintText);
        Spacer(card, 6);

        var button = NewButton(card, out buttonLabel);
        button.onClick.AddListener(Begin);
    }

    void MeterRow(RectTransform parent, string name, Color color, string meaning)
    {
        // [ name over a coloured bar ] [ what it means ]
        var row = NewRect("Meter_" + name, parent);
        var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 32;
        h.childAlignment = TextAnchor.MiddleLeft;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = false;

        var left = NewRect("Label", row);
        var leftLe = left.gameObject.AddComponent<LayoutElement>();
        leftLe.preferredWidth = 380;
        leftLe.minWidth = 380;
        var v = left.gameObject.AddComponent<VerticalLayoutGroup>();
        v.spacing = 6;
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;

        var label = NewText("Name", left, 32, color);
        label.text = name;

        var bar = NewRect("Bar", left);
        var barLe = bar.gameObject.AddComponent<LayoutElement>();
        barLe.preferredHeight = 14;
        barLe.minHeight = 14;
        bar.gameObject.AddComponent<Image>().color = color;

        var desc = NewText("Meaning", row, 29, BodyText);
        desc.text = meaning;
        var descLe = desc.gameObject.AddComponent<LayoutElement>();
        descLe.flexibleWidth = 1;
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
        le.minHeight = height;
        le.preferredHeight = height;
    }

    static Button NewButton(RectTransform parent, out TMP_Text label)
    {
        var row = NewRect("ButtonRow", parent);
        var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        rowLayout.childAlignment = TextAnchor.MiddleRight;
        rowLayout.childControlWidth = false;
        rowLayout.childControlHeight = false;
        rowLayout.childForceExpandWidth = false;
        row.gameObject.AddComponent<LayoutElement>().preferredHeight = 76;

        var rt = NewRect("StartButton", row);
        rt.sizeDelta = new Vector2(440, 76);
        var img = rt.gameObject.AddComponent<Image>();
        var buttonSprite = Resources.Load<Sprite>("PixelArt/ui_button");
        if (buttonSprite != null)
        {
            img.sprite = buttonSprite;
            img.type = Image.Type.Sliced;
        }
        else
        {
            img.color = new Color32(240, 228, 206, 255);
        }
        var button = rt.gameObject.AddComponent<Button>();

        label = NewText("Label", rt, 32, ButtonText);
        Stretch((RectTransform)label.transform);
        label.alignment = TextAlignmentOptions.Center;
        return button;
    }
}
