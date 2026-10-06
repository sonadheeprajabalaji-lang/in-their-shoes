using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The card shown in Self Hour straight after the player picks a response:
// what the choice means for her, a research-backed fact about this stage of
// pregnancy, how to help, and the sources.
//
// Builds its own UI at runtime (its own overlay canvas), so nothing needs
// to be set up in the scene. GameManager creates one automatically if none
// is assigned. Uses the pixel-art panel and button sprites from
// Resources/PixelArt when they exist, and plain colours otherwise.
//
// Close it with the Continue button, Space or Enter. (Not E: E is the
// interact key, and the same press would reach the next object.)
public class ResponseFeedbackUI : MonoBehaviour
{
    static readonly Color Dim = new Color(0.08f, 0.06f, 0.1f, 0.7f);
    static readonly Color PanelFallback = new Color32(43, 34, 51, 245);
    static readonly Color TitleColor = new Color32(248, 246, 240, 255);
    static readonly Color BodyColor = new Color32(232, 226, 236, 255);
    static readonly Color ImpactHeading = new Color32(168, 160, 236, 255);   // mask violet, lightened
    static readonly Color LearnHeading = new Color32(239, 159, 39, 255);     // mind amber
    static readonly Color HelpHeading = new Color32(93, 202, 165, 255);      // person green, lightened
    static readonly Color SourceColor = new Color32(170, 160, 176, 255);
    static readonly Color ButtonText = new Color32(43, 34, 51, 255);

    GameObject root;
    TMP_Text title;
    TMP_Text impactHead, impactBody, learnHead, learnBody, helpHead, helpBody, sources;
    Button continueButton;
    Action onClosed;
    float shownAt;

    public bool IsShowing => root != null && root.activeSelf;

    // Seconds the card was on screen the last time it closed (for logging).
    public float LastReadSeconds { get; private set; }

    void Awake()
    {
        Build();
        root.SetActive(false);
    }

    public void Show(string chosenLabel, string learnHeading, SelfHourResponse r, Action closed)
    {
        onClosed = closed;

        title.text = "You chose: " + chosenLabel;
        Fill(impactHead, impactBody, "What this means for her", r.impactText);
        Fill(learnHead, learnBody, string.IsNullOrWhiteSpace(learnHeading) ? "Did you know?" : learnHeading, r.learnText);
        Fill(helpHead, helpBody, "How you can help", r.helpText);
        sources.text = string.IsNullOrWhiteSpace(r.sourceText) ? "" : "Sources: " + r.sourceText;
        sources.gameObject.SetActive(!string.IsNullOrWhiteSpace(r.sourceText));

        root.SetActive(true);
        shownAt = Time.unscaledTime;
    }

    static void Fill(TMP_Text head, TMP_Text body, string heading, string text)
    {
        bool has = !string.IsNullOrWhiteSpace(text);
        head.gameObject.SetActive(has);
        body.gameObject.SetActive(has);
        head.text = heading;
        body.text = has ? text.Trim() : "";
    }

    void Update()
    {
        if (!IsShowing) return;
        // Ignore the first moment so the click that chose the response can't also close the card.
        if (Time.unscaledTime - shownAt < 0.4f) return;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Close();
        }
    }

    void Close()
    {
        if (!IsShowing) return;
        LastReadSeconds = Time.unscaledTime - shownAt;
        root.SetActive(false);
        var cb = onClosed;
        onClosed = null;
        cb?.Invoke();
    }

    // ---------- UI construction ----------

    void Build()
    {
        root = new GameObject("ResponseFeedbackCanvas");
        root.transform.SetParent(transform, false);
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;   // above the game's UI and the pixel-art overlays
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        // Dim the room behind the card.
        var dim = NewRect("Dim", root.transform);
        Stretch(dim);
        var dimImg = dim.gameObject.AddComponent<Image>();
        dimImg.color = Dim;

        // The card: a vertical stack that grows to fit its text.
        var card = NewRect("Card", root.transform);
        card.anchorMin = card.anchorMax = card.pivot = new Vector2(0.5f, 0.5f);
        card.sizeDelta = new Vector2(1400, 0);
        var cardImg = card.gameObject.AddComponent<Image>();
        var panelSprite = Resources.Load<Sprite>("PixelArt/ui_panel");
        if (panelSprite != null)
        {
            cardImg.sprite = panelSprite;
            cardImg.type = Image.Type.Sliced;
            cardImg.pixelsPerUnitMultiplier = 0.5f;   // a chunkier border at this size
            cardImg.color = Color.white;
        }
        else
        {
            cardImg.color = PanelFallback;
        }

        var layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(56, 56, 44, 40);
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        var fitter = card.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        title = NewText("Title", card, 42, TitleColor);
        Spacer(card, 8);
        impactHead = NewText("ImpactHeading", card, 32, ImpactHeading);
        impactBody = NewText("ImpactBody", card, 29, BodyColor);
        Spacer(card, 6);
        learnHead = NewText("LearnHeading", card, 32, LearnHeading);
        learnBody = NewText("LearnBody", card, 29, BodyColor);
        Spacer(card, 6);
        helpHead = NewText("HelpHeading", card, 32, HelpHeading);
        helpBody = NewText("HelpBody", card, 29, BodyColor);
        Spacer(card, 8);
        sources = NewText("Sources", card, 22, SourceColor);
        Spacer(card, 10);

        continueButton = NewButton(card, "Continue  (Space)");
        continueButton.onClick.AddListener(Close);
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
        var rt = NewRect("Spacer", parent);
        var le = rt.gameObject.AddComponent<LayoutElement>();
        le.minHeight = height;
        le.preferredHeight = height;
    }

    static Button NewButton(RectTransform parent, string label)
    {
        // Wrap the button so it keeps a fixed size and sits on the right
        // instead of stretching across the card.
        var row = NewRect("ButtonRow", parent);
        var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        rowLayout.childAlignment = TextAnchor.MiddleRight;
        rowLayout.childControlWidth = false;
        rowLayout.childControlHeight = false;
        rowLayout.childForceExpandWidth = false;
        var rowLe = row.gameObject.AddComponent<LayoutElement>();
        rowLe.preferredHeight = 70;

        var rt = NewRect("ContinueButton", row);
        rt.sizeDelta = new Vector2(360, 70);
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

        var text = NewText("Label", rt, 30, ButtonText);
        Stretch((RectTransform)text.transform);
        text.alignment = TextAlignmentOptions.Center;
        text.text = label;
        return button;
    }
}
