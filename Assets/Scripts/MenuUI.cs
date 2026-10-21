using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Shared pieces for the screens before the chapters (Title, Warning,
// Chapters list): scene names, a full-screen canvas on the
// logo's Night background, buttons, and the logo sprites from
// Resources/Logo. Like TutorialUI, every screen builds its UI at runtime,
// so each scene only needs one component and a camera.
public static class MenuUI
{
    // Scene names, in play order. All must be in File > Build Profiles > Scenes.
    public const string TitleScene = "Title";
    public const string WarningScene = "Warning";
    public const string ChaptersScene = "ChapterSelect";
    public const string TutorialScene = "Tutorial";
    public const string InfoScene = "Info";
    public const string EndScene = "End";

    // Logo palette (see in-their-shoes-logo/index.html).
    public static readonly Color Night = new Color32(28, 25, 40, 255);
    public static readonly Color Dusk = new Color32(42, 34, 56, 255);
    public static readonly Color Rose = new Color32(231, 163, 155, 255);
    public static readonly Color Cream = new Color32(244, 232, 214, 255);
    public static readonly Color Mist = new Color32(179, 169, 184, 255);

    public static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    // A 1920x1080 overlay canvas with the Night-to-Dusk background.
    // Returns the stretched root to build on.
    public static RectTransform Canvas(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();

        var bg = UIArt.Rect("Background", go.transform);
        UIArt.Stretch(bg);
        var raw = bg.gameObject.AddComponent<RawImage>();
        raw.texture = GradientTexture();
        raw.raycastTarget = false;
        return (RectTransform)go.transform;
    }

    static Texture2D gradient;
    static Texture2D GradientTexture()
    {
        if (gradient != null) return gradient;
        gradient = new Texture2D(1, 64, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        for (int y = 0; y < 64; y++)
        {
            // y = 0 is the bottom row: Dusk at the bottom, Night at the top.
            gradient.SetPixel(0, y, Color.Lerp(Dusk, Night, y / 63f));
        }
        gradient.Apply();
        return gradient;
    }

    // ---------- logo ----------

    static readonly Dictionary<string, Sprite> logoCache = new Dictionary<string, Sprite>();

    // name: "mark-layer-partner", "mark-layer-you", "mark-layer-stitch",
    // "wordmark-dark" or "logo-stacked-dark-notag".
    public static Sprite Logo(string name)
    {
        if (logoCache.TryGetValue(name, out var s) && s != null) return s;
        var tex = Resources.Load<Texture2D>("Logo/" + name);
        if (tex == null)
        {
            Debug.LogWarning("Missing logo texture Resources/Logo/" + name);
            return null;
        }
        s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        logoCache[name] = s;
        return s;
    }

    public static Image LogoImage(Transform parent, string name, float width)
    {
        var rt = UIArt.Rect(name, parent);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = Logo(name);
        img.preserveAspect = true;
        img.raycastTarget = false;
        float aspect = img.sprite != null ? img.sprite.rect.height / img.sprite.rect.width : 1f;
        rt.sizeDelta = new Vector2(width, width * aspect);
        UIArt.Size(img, width, width * aspect);
        return img;
    }

    // ---------- controls ----------

    // The line under the wordmark: "A walk through her day" on the Title,
    // "Now walk beside her" on the End scene. Centred, below the wordmark.
    public static TMPro.TMP_Text Tagline(RectTransform parent, string text, RectTransform wordmark)
    {
        var t = UIArt.Text(parent, text, 38, Cream, TMPro.TextAlignmentOptions.Center);
        t.fontStyle = TMPro.FontStyles.Italic;
        t.enableWordWrapping = false;
        var rt = (RectTransform)t.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1000, 60);
        rt.anchoredPosition = wordmark.anchoredPosition - new Vector2(0, wordmark.sizeDelta.y / 2f + 46f);
        return t;
    }

    public static Button Button(Transform parent, string label, float width, UnityEngine.Events.UnityAction onClick)
    {
        var img = UIArt.Panel(parent, "Button_" + label, "ui_button", 1f);
        UIArt.Size(img, width, 76);
        var button = img.gameObject.AddComponent<Button>();
        button.onClick.AddListener(onClick);
        if (img.gameObject.GetComponent<ButtonSFX>() == null) img.gameObject.AddComponent<ButtonSFX>();

        // A visible focus state for keyboard players.
        var colors = button.colors;
        colors.highlightedColor = new Color(1f, 0.93f, 0.9f);
        colors.selectedColor = new Color(1f, 0.85f, 0.8f);
        button.colors = colors;

        var text = UIArt.Text(img.transform, label, 30, UIArt.KeyText);
        UIArt.Stretch((RectTransform)text.transform);
        return button;
    }

    public static void Load(string scene) => SceneManager.LoadScene(scene);

    public static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
