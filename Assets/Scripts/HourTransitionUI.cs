using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A brief full-screen message shown between Partner Hour ending and Self
// Hour starting (see ChapterData.hourTransitionText). Builds its own
// overlay canvas at runtime, same as MeterIntroUI and ResponseFeedbackUI,
// so GameManager can create one automatically with no scene setup.
//
// Stays up for at least minShowSeconds (so it can't blink past unread),
// then continues either on its own after autoDismissAfter, or sooner if
// the player presses a key / clicks.
public class HourTransitionUI : MonoBehaviour
{
    static readonly Color Backdrop = new Color(0.07f, 0.05f, 0.09f, 0.95f);
    static readonly Color TitleColor = new Color32(240, 236, 228, 255);
    static readonly Color HintColor = new Color32(176, 166, 188, 255);

    GameObject root;
    TMP_Text title;
    TMP_Text hint;

    Action onDone;
    float shownAt;
    float minShowSeconds;
    bool dismissed;

    public bool IsShowing => root != null && root.activeSelf;

    void Awake()
    {
        Build();
        root.SetActive(false);
    }

    public void Show(string message, float minShowSeconds, float autoDismissAfter, Action done)
    {
        onDone = done;
        this.minShowSeconds = minShowSeconds;
        dismissed = false;

        title.text = string.IsNullOrWhiteSpace(message)
            ? "She's become too tired to continue."
            : message;

        root.SetActive(true);
        shownAt = Time.unscaledTime;

        CancelInvoke();
        if (autoDismissAfter > 0f) Invoke(nameof(Finish), autoDismissAfter);
    }

    void Update()
    {
        if (!IsShowing || dismissed) return;
        if (Time.unscaledTime - shownAt < minShowSeconds) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0))
        {
            Finish();
        }
    }

    void Finish()
    {
        if (dismissed) return;
        dismissed = true;

        CancelInvoke();
        root.SetActive(false);

        var callback = onDone;
        onDone = null;
        callback?.Invoke();
    }

    // ---------- build (runtime, no scene setup needed) ----------

    void Build()
    {
        var canvasGO = new GameObject("HourTransitionCanvas");
        canvasGO.transform.SetParent(transform, false);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300; // above everything else, including other overlays
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        root = new GameObject("Root", typeof(RectTransform));
        root.transform.SetParent(canvasGO.transform, false);
        var rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        var backdrop = root.AddComponent<Image>();
        backdrop.color = Backdrop;

        title = CreateLabel(root.transform, "Title", 34f, TitleColor, FontStyles.Bold);
        var titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.sizeDelta = new Vector2(900f, 220f);
        titleRect.anchoredPosition = new Vector2(0f, 20f);
        title.alignment = TextAlignmentOptions.Center;
        title.enableWordWrapping = true;

        hint = CreateLabel(root.transform, "Hint", 18f, HintColor, FontStyles.Normal);
        var hintRect = hint.rectTransform;
        hintRect.anchorMin = new Vector2(0.5f, 0.5f);
        hintRect.anchorMax = new Vector2(0.5f, 0.5f);
        hintRect.sizeDelta = new Vector2(600f, 60f);
        hintRect.anchoredPosition = new Vector2(0f, -120f);
        hint.alignment = TextAlignmentOptions.Center;
        hint.text = "Press Space to continue";
    }

    TMP_Text CreateLabel(Transform parent, string name, float fontSize, Color color, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.color = color;
        text.fontStyle = style;
        return text;
    }
}