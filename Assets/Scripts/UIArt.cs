using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Small building blocks shared by the tutorial (TutorialUI) and each
// chapter's start screen (MeterIntroUI): pixel keycaps that light up when
// their key is pressed, meter icons, and text in the game's font.
// Everything is built at runtime from sprites in Resources/PixelArt.
public static class UIArt
{
    public static readonly Color BodyColor = new Color32(216, 90, 48, 255);
    public static readonly Color MindColor = new Color32(239, 159, 39, 255);
    public static readonly Color PersonColor = new Color32(29, 158, 117, 255);
    public static readonly Color MaskColor = new Color32(127, 119, 221, 255);

    public static readonly Color Backdrop = new Color32(26, 21, 31, 255);
    public static readonly Color TitleColor = new Color32(248, 246, 240, 255);
    public static readonly Color TextColor = new Color32(232, 226, 236, 255);
    public static readonly Color HintColor = new Color32(190, 180, 200, 255);
    public static readonly Color KeyText = new Color32(43, 34, 51, 255);

    public static readonly (string name, string icon, Color color)[] Meters =
    {
        ("Body", "icon_body", BodyColor),
        ("Mind", "icon_mind", MindColor),
        ("Feeling like a person", "icon_person", PersonColor),
        ("The Mask", "icon_mask", MaskColor),
    };

    static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
    public static Sprite S(string name)
    {
        if (!cache.TryGetValue(name, out var s) || s == null)
        {
            s = Resources.Load<Sprite>("PixelArt/" + name);
            cache[name] = s;
        }
        return s;
    }

    // Merriweather, same as the rest of the game (PixelArtSkin makes it in
    // chapter scenes; the tutorial has no skin, so make it here if needed).
    static TMP_FontAsset font;
    public static TMP_FontAsset Font
    {
        get
        {
            if (PixelArtSkin.CurrentFont != null) return PixelArtSkin.CurrentFont;
            if (font != null) return font;
            var regular = Resources.Load<Font>("PixelArt/Fonts/Merriweather-Regular");
            if (regular == null) return null;
            font = TMP_FontAsset.CreateFontAsset(regular);
            var bold = Resources.Load<Font>("PixelArt/Fonts/Merriweather-Bold");
            if (font != null && bold != null && font.fontWeightTable != null && font.fontWeightTable.Length > 7)
            {
                var boldAsset = TMP_FontAsset.CreateFontAsset(bold);
                if (boldAsset != null) font.fontWeightTable[7].regularTypeface = boldAsset;
            }
            return font;
        }
    }

    // ---------- layout helpers ----------

    public static RectTransform Rect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        return rt;
    }

    public static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public static LayoutElement Size(Component c, float w, float h)
    {
        var le = c.GetComponent<LayoutElement>();
        if (le == null) le = c.gameObject.AddComponent<LayoutElement>();
        if (w >= 0) le.preferredWidth = le.minWidth = w;
        if (h >= 0) le.preferredHeight = le.minHeight = h;
        return le;
    }

    public static HorizontalLayoutGroup Row(Transform parent, string name, float spacing, TextAnchor align = TextAnchor.MiddleCenter)
    {
        var rt = Rect(name, parent);
        var h = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = spacing;
        h.childAlignment = align;
        h.childControlWidth = h.childControlHeight = true;
        h.childForceExpandWidth = h.childForceExpandHeight = false;
        return h;
    }

    public static VerticalLayoutGroup Column(Transform parent, string name, float spacing, TextAnchor align = TextAnchor.UpperCenter)
    {
        var rt = Rect(name, parent);
        var v = rt.gameObject.AddComponent<VerticalLayoutGroup>();
        v.spacing = spacing;
        v.childAlignment = align;
        v.childControlWidth = v.childControlHeight = true;
        v.childForceExpandWidth = v.childForceExpandHeight = false;
        return v;
    }

    public static TMP_Text Text(Transform parent, string text, float size, Color color,
                                TextAlignmentOptions align = TextAlignmentOptions.Center, bool bold = false)
    {
        var rt = Rect("Text", parent);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        if (Font != null) t.font = Font;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.enableWordWrapping = true;
        t.raycastTarget = false;
        if (bold) t.fontStyle = FontStyles.Bold;
        return t;
    }

    public static Image Icon(Transform parent, string sprite, float size)
    {
        var rt = Rect(sprite, parent);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = S(sprite);
        img.preserveAspect = true;
        img.raycastTarget = false;
        Size(img, size, size);
        return img;
    }

    public static Image Panel(Transform parent, string name, string sprite = "ui_panel", float ppuMultiplier = 0.5f)
    {
        var rt = Rect(name, parent);
        var img = rt.gameObject.AddComponent<Image>();
        var s = S(sprite);
        if (s != null)
        {
            img.sprite = s;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = ppuMultiplier;
        }
        else img.color = new Color32(43, 34, 51, 255);
        return img;
    }

    // A pixel keycap. label is the letter ("E", "SPACE"); arrowAngle >= 0
    // draws an arrow instead (0 = up, 90 = left, 180 = down, 270 = right).
    // It lights up while any of keys is held down, so players can try keys
    // on the spot.
    public static KeyCapView Key(Transform parent, string label, float size, float width = -1f,
                                 float arrowAngle = -1f, params KeyCode[] keys)
    {
        var rt = Rect("Key_" + label, parent);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = S("ui_key");
        img.type = Image.Type.Sliced;
        // The 24px sprite has a 6px border; scale it so the border stays a
        // quarter of the key's height at any size (canvas 100 units per
        // unit / sprite 32 PPU = 3.125).
        img.pixelsPerUnitMultiplier = 6f * 3.125f / (size * 0.25f);
        img.raycastTarget = false;
        Size(img, width > 0 ? width : size, size);

        var face = Rect("Face", rt);
        Stretch(face);
        face.offsetMin = new Vector2(0, size * 0.18f);   // sit on the top face, above the side
        if (arrowAngle >= 0f)
        {
            var a = Rect("Arrow", face);
            a.anchorMin = a.anchorMax = new Vector2(0.5f, 0.5f);
            a.sizeDelta = new Vector2(size * 0.5f, size * 0.5f);
            a.localEulerAngles = new Vector3(0, 0, arrowAngle);
            var ai = a.gameObject.AddComponent<Image>();
            ai.sprite = S("icon_arrow");
            ai.raycastTarget = false;
        }
        else
        {
            var t = Text(face, label, label.Length > 1 ? size * 0.32f : size * 0.5f, KeyText, TextAlignmentOptions.Center, true);
            Stretch((RectTransform)t.transform);
        }

        var view = rt.gameObject.AddComponent<KeyCapView>();
        view.Init(img, face, keys);
        return view;
    }

    public static string KeyLabel(KeyCode k) => k switch
    {
        KeyCode.Space => "SPACE",
        KeyCode.Return => "ENTER",
        KeyCode.LeftShift or KeyCode.RightShift => "SHIFT",
        _ => k.ToString().ToUpperInvariant(),
    };
}

// Lights a keycap up while its key is held, or while Demo is set.
public class KeyCapView : MonoBehaviour
{
    Image image;
    RectTransform face;
    KeyCode[] keys;
    Vector2 faceMin;
    public bool Demo;   // set by a script to show the key being pressed

    public void Init(Image image, RectTransform face, KeyCode[] keys)
    {
        this.image = image;
        this.face = face;
        this.keys = keys ?? new KeyCode[0];
        faceMin = face.offsetMin;
    }

    public bool Held
    {
        get
        {
            foreach (var k in keys) if (Input.GetKey(k)) return true;
            return false;
        }
    }

    void Update()
    {
        bool down = Demo || Held;
        image.sprite = UIArt.S(down ? "ui_key_down" : "ui_key");
        // The pressed sprite's top face sits a little lower.
        face.offsetMin = faceMin;
        face.offsetMax = new Vector2(0, down ? -((RectTransform)transform).rect.height * 0.08f : 0f);
    }
}
