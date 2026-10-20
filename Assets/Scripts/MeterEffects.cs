using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Shows her state in the room itself, so the player doesn't have to watch
// the bars (playtest: "keeping up with the sliders is hard when I'm doing
// multiple tasks"). Only during Partner Hour; everything resets in Self Hour.
//
//   Body low                  -> she walks more slowly
//   Mind low                  -> double vision (a faint, drifting second copy
//                                of the room, the tasks and her), and the
//                                prompts flicker and wobble
//   Feeling like a person low -> the colour drains out of the room
//
// Each effect starts when its meter falls below startAt and is strongest at
// fullAt. The first time an effect starts in an hour, a short line appears
// above her ("Her body is slowing her down") in that meter's colour, so the
// player learns which meter it belongs to.
//
// The meter bars themselves also call for attention:
//   - for the first few seconds of each Partner Hour, all four bars blink
//     (they're already draining while they blink), and
//   - a bar that's nearly empty flashes, faster the closer it is to zero.
//
// PixelArtSkin adds and sets this up; nothing is needed in the scene.
public class MeterEffects : MonoBehaviour
{
    [Header("When effects start and peak (meter value 0..1)")]
    public float startAt = 0.75f;
    public float fullAt = 0.15f;

    [Header("Body: walking speed")]
    [Tooltip("How many times slower each step is at full effect.")]
    public float maxSlowdown = 2.5f;

    [Header("Mind: double vision and flicker")]
    public float maxGhostAlpha = 0.35f;
    [Tooltip("How far the second image drifts, in pixels (32 per grid cell).")]
    public float maxGhostDriftPixels = 3f;
    public float maxPromptFlicker = 0.6f;
    public float maxPromptJitterPixels = 6f;

    [Header("Feeling like a person: colour drains")]
    public float maxGreyAlpha = 0.55f;
    public Color greyColor = new Color(0.52f, 0.52f, 0.55f, 1f);

    [Header("First-time captions")]
    public string bodyCaption = "Her body is slowing her down.";
    public string mindCaption = "Her thoughts are blurring.";
    public string personCaption = "The colour is going out of her day.";
    public float captionSeconds = 2.8f;

    [Header("Meter bars")]
    [Tooltip("Seconds the bars blink at the start of each Partner Hour.")]
    public float introBlinkSeconds = 3f;
    [Tooltip("Blinks per second at the start of the hour.")]
    public float introBlinkRate = 2.5f;
    [Tooltip("A bar starts flashing below this (0..1).")]
    public float lowFlashAt = 0.25f;
    [Tooltip("Flashes per second when a bar has just gone low, and when it's almost empty.")]
    public float lowFlashSlow = 1.5f, lowFlashFast = 4.5f;
    [Tooltip("How long a bar flashes red and shakes after a big hit (the main task).")]
    public float hitFlashSeconds = 1.2f;
    public string hitCaption = "That took a lot out of her.";
    public string limitCaption = "She has nothing left for the rest.";

    static readonly Color BodyColor = new Color32(240, 140, 100, 255);
    static readonly Color MindColor = new Color32(245, 185, 80, 255);
    static readonly Color PersonColor = new Color32(93, 202, 165, 255);

    GameManager gm;
    GridPlayerController player;
    Func<bool> isPartnerHour;
    TextMeshPro worldPrompt;
    RectTransform juggleBox;
    CanvasGroup juggleGroup;
    Vector2 juggleBasePos;
    Image grey;
    float baseMoveDuration;

    class Ghost { public SpriteRenderer source; public SpriteRenderer copy; }
    readonly List<Ghost> ghosts = new List<Ghost>();

    class Bar { public Slider slider; public Image fill; public Color baseColor; public CanvasGroup group; public float hitUntil; public Vector2 basePos; public bool shaking; }
    static readonly Color HitColor = new Color32(255, 70, 60, 255);
    readonly List<Bar> bars = new List<Bar>();
    bool barColoursKnown;
    float partnerStartedAt = -100f;   // no blink until the meters first start draining

    TextMeshPro caption;
    float captionUntil;
    bool shownBody, shownMind, shownPerson;
    bool wasPartner;
    bool wasDraining;

    public void Init(GameManager gm, GridPlayerController player, Func<bool> isPartnerHour,
                     Transform overlayCanvas, IEnumerable<SpriteRenderer> doubled)
    {
        this.gm = gm;
        this.player = player;
        this.isPartnerHour = isPartnerHour;
        if (player != null) baseMoveDuration = player.moveDuration;

        // A flat grey wash on the overlay canvas (under the game's UI, over
        // the room): mixing toward grey takes the colour out of the room.
        var go = new GameObject("ColourDrain", typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(overlayCanvas, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        grey = go.AddComponent<Image>();
        grey.raycastTarget = false;
        SetAlpha(grey, greyColor, 0f);

        foreach (var sr in doubled) AddGhost(sr);

        caption = MakeCaption();

        if (gm != null && gm.meters != null)
        {
            AddBar(gm.meters.body);
            AddBar(gm.meters.mind);
            AddBar(gm.meters.feelingLikeAPerson);
            AddBar(gm.meters.mask);
            gm.meters.HitTaken += OnHit;
            gm.meters.Collapsing += OnCollapsing;
        }
    }

    void AddBar(Slider slider)
    {
        if (slider == null) return;
        var bar = new Bar { slider = slider };
        if (slider.fillRect != null) bar.fill = slider.fillRect.GetComponent<Image>();
        bar.basePos = ((RectTransform)slider.transform).anchoredPosition;
        bar.group = slider.GetComponent<CanvasGroup>();
        if (bar.group == null) bar.group = slider.gameObject.AddComponent<CanvasGroup>();
        bars.Add(bar);
    }

    // The interaction prompt and the juggle box are found lazily, because
    // they're created by other scripts.
    public void SetWorldPrompt(TextMeshPro prompt) { worldPrompt = prompt; }

    public void SetJuggleBox(RectTransform box)
    {
        juggleBox = box;
        if (box == null) return;
        juggleGroup = box.GetComponent<CanvasGroup>();
        if (juggleGroup == null) juggleGroup = box.gameObject.AddComponent<CanvasGroup>();
        juggleBasePos = box.anchoredPosition;
    }

    public void AddGhost(SpriteRenderer source)
    {
        if (source == null) return;
        var go = new GameObject("DoubleVision");
        go.transform.SetParent(source.transform, false);
        var copy = go.AddComponent<SpriteRenderer>();
        copy.sortingLayerID = source.sortingLayerID;
        copy.sortingOrder = source.sortingOrder + 1;
        copy.enabled = false;
        ghosts.Add(new Ghost { source = source, copy = copy });
    }

    float Strength(float meter) => Mathf.Clamp01((startAt - meter) / Mathf.Max(0.01f, startAt - fullAt));

    void LateUpdate()
    {
        bool partner = isPartnerHour != null && isPartnerHour();
        if (partner && !wasPartner)
        {
            shownBody = shownMind = shownPerson = false;   // new hour, new captions
        }
        wasPartner = partner;

        // The bars blink when the meters actually start draining, which is
        // after the chapter banner has gone (see ChapterBannerUI).
        bool draining = gm != null && gm.meters != null && gm.meters.IsDraining;
        if (draining && !wasDraining) partnerStartedAt = Time.time;
        wasDraining = draining;

        float body = 1f, mind = 1f, person = 1f;
        if (partner && gm != null && gm.meters != null)
        {
            if (gm.meters.body != null) body = gm.meters.body.value;
            if (gm.meters.mind != null) mind = gm.meters.mind.value;
            if (gm.meters.feelingLikeAPerson != null) person = gm.meters.feelingLikeAPerson.value;
        }

        float b = partner ? Strength(body) : 0f;
        float m = partner ? Strength(mind) : 0f;
        float p = partner ? Strength(person) : 0f;

        ApplyBody(b);
        ApplyMind(m);
        ApplyPerson(p);

        if (partner)
        {
            if (!shownBody && b > 0.3f) { shownBody = true; ShowCaption(bodyCaption, BodyColor); }
            else if (!shownMind && m > 0.3f) { shownMind = true; ShowCaption(mindCaption, MindColor); }
            else if (!shownPerson && p > 0.3f) { shownPerson = true; ShowCaption(personCaption, PersonColor); }
        }
        UpdateCaption(partner);
        UpdateBars(partner);
    }

    // ---------- Meter bars: blink at the start, flash when nearly empty ----------

    void OnHit(MeterRates dropped)
    {
        float[] d = { dropped.body, dropped.mind, dropped.feelingLikeAPerson, dropped.mask };
        bool any = false;
        for (int i = 0; i < bars.Count && i < d.Length; i++)
        {
            if (d[i] <= 0.001f) continue;
            bars[i].hitUntil = Time.time + hitFlashSeconds;
            any = true;
        }
        if (any) ShowCaption(hitCaption, new Color32(255, 150, 140, 255));
    }

    void OnCollapsing(HerMeter meter)
    {
        int i = (int)meter;
        if (i < bars.Count) bars[i].hitUntil = Time.time + hitFlashSeconds + 0.5f;
        ShowCaption(limitCaption, new Color32(255, 150, 140, 255));
    }

    void OnDestroy()
    {
        if (gm != null && gm.meters != null)
        {
            gm.meters.HitTaken -= OnHit;
            gm.meters.Collapsing -= OnCollapsing;
        }
    }

    void UpdateBars(bool partner)
    {
        // The skin colours the bars before this runs; remember those colours
        // so flashing can always return to them.
        if (!barColoursKnown && partner)
        {
            foreach (var bar in bars) if (bar.fill != null) bar.baseColor = bar.fill.color;
            barColoursKnown = true;
        }

        float since = Time.time - partnerStartedAt;
        bool introBlink = partner && since < introBlinkSeconds;
        // Square on/off blink so it reads as an alert, not a fade.
        float introAlpha = introBlink && Mathf.Repeat(since * introBlinkRate, 1f) >= 0.5f ? 0.2f : 1f;

        foreach (var bar in bars)
        {
            if (bar.group != null) bar.group.alpha = introAlpha;
            if (bar.fill == null || !barColoursKnown) continue;

            // Just hit: flash red and shake for a moment.
            var rt = (RectTransform)bar.slider.transform;
            float hitLeft = bar.hitUntil - Time.time;
            if (partner && hitLeft > 0f)
            {
                float k = hitLeft / hitFlashSeconds;                              // 1 -> 0
                bool on = Mathf.Repeat(Time.time * 8f, 1f) < 0.5f;
                bar.fill.color = on ? HitColor : Color.Lerp(bar.baseColor, HitColor, 0.4f);
                rt.anchoredPosition = bar.basePos + new Vector2(Mathf.Sin(Time.time * 70f) * 6f * k, 0f);
                bar.shaking = true;
                continue;
            }
            if (bar.shaking) { rt.anchoredPosition = bar.basePos; bar.shaking = false; }

            float v = bar.slider.value;
            if (partner && v > 0f && v < lowFlashAt)
            {
                float urgency = 1f - v / lowFlashAt;                       // 0 just gone low, 1 almost empty
                float rate = Mathf.Lerp(lowFlashSlow, lowFlashFast, urgency);
                // Time.time * rate keeps a steady rhythm; the per-bar speed
                // still rises as the bar drains.
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * rate * Mathf.PI * 2f);
                bar.fill.color = Color.Lerp(bar.baseColor, Color.white, pulse * 0.85f);
            }
            else
            {
                bar.fill.color = bar.baseColor;
            }
        }
    }

    // ---------- Body: slower steps ----------

    void ApplyBody(float t)
    {
        if (player == null) return;
        player.moveDuration = baseMoveDuration * Mathf.Lerp(1f, maxSlowdown, t);
    }

    // ---------- Mind: double vision, flicker ----------

    void ApplyMind(float t)
    {
        float px = GridPlayerController.CellSize / 32f;   // one art pixel in world units
        // Two slow sine waves so the drift wanders rather than ticking.
        float dx = (Mathf.Sin(Time.time * 1.3f) + 0.5f * Mathf.Sin(Time.time * 2.9f)) * maxGhostDriftPixels * t;
        float dy = (Mathf.Cos(Time.time * 0.9f)) * maxGhostDriftPixels * 0.5f * t;
        var offset = new Vector3(Mathf.Round(dx) * px, Mathf.Round(dy) * px, 0f);
        float alpha = maxGhostAlpha * t;

        foreach (var g in ghosts)
        {
            if (g.source == null || g.copy == null) continue;
            bool show = alpha > 0.01f && g.source.enabled && g.source.sprite != null && g.source.gameObject.activeInHierarchy;
            g.copy.enabled = show;
            if (!show) continue;
            g.copy.sprite = g.source.sprite;
            g.copy.flipX = g.source.flipX;
            g.copy.drawMode = g.source.drawMode;
            if (g.source.drawMode != SpriteDrawMode.Simple) g.copy.size = g.source.size;
            var c = g.source.color;
            c.a *= alpha;
            g.copy.color = c;
            g.copy.transform.localPosition = new Vector3(offset.x / Mathf.Max(0.0001f, g.source.transform.lossyScale.x),
                                                         offset.y / Mathf.Max(0.0001f, g.source.transform.lossyScale.y), 0f);
        }

        // Prompts flicker and wobble: hard to keep hold of what you were doing.
        float noise = Mathf.PerlinNoise(Time.time * 6f, 0.37f);
        float flicker = 1f - maxPromptFlicker * t * noise;
        if (worldPrompt != null)
        {
            var c = worldPrompt.color;
            c.a = flicker;
            worldPrompt.color = c;
        }
        if (juggleGroup != null)
        {
            juggleGroup.alpha = flicker;
            float jx = (Mathf.PerlinNoise(Time.time * 4f, 1.7f) - 0.5f) * 2f * maxPromptJitterPixels * t;
            float jy = (Mathf.PerlinNoise(2.3f, Time.time * 4f) - 0.5f) * 2f * maxPromptJitterPixels * t;
            juggleBox.anchoredPosition = juggleBasePos + new Vector2(jx, jy);
        }
    }

    // ---------- Feeling like a person: colour drains ----------

    void ApplyPerson(float t)
    {
        if (grey != null) SetAlpha(grey, greyColor, maxGreyAlpha * t);
    }

    // ---------- Captions ----------

    TextMeshPro MakeCaption()
    {
        var go = new GameObject("MeterCaption");
        go.transform.SetParent(transform, false);
        var t = go.AddComponent<TextMeshPro>();
        t.alignment = TextAlignmentOptions.Center;
        t.fontSize = 4.2f;
        t.enableWordWrapping = false;
        t.rectTransform.sizeDelta = new Vector2(20f, 1.5f);
        t.outlineWidth = 0.2f;
        t.outlineColor = new Color32(30, 24, 36, 255);
        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 101;   // above the Press E prompt
        go.SetActive(false);
        return t;
    }

    void ShowCaption(string text, Color color)
    {
        if (caption == null || string.IsNullOrEmpty(text)) return;
        caption.text = text;
        caption.color = color;
        captionUntil = Time.time + captionSeconds;
        caption.gameObject.SetActive(true);
    }

    void UpdateCaption(bool partner)
    {
        if (caption == null || !caption.gameObject.activeSelf) return;
        float left = captionUntil - Time.time;
        if (!partner || left <= 0f || player == null)
        {
            caption.gameObject.SetActive(false);
            return;
        }
        // Rises gently above her and fades out over the last second.
        float rise = (captionSeconds - left) * 0.15f;
        caption.transform.position = player.transform.position + new Vector3(0f, 1.6f + rise, 0f);
        var c = caption.color;
        c.a = Mathf.Clamp01(left);
        caption.color = c;
    }

    static void SetAlpha(Image img, Color baseColor, float a)
    {
        baseColor.a = a;
        img.color = baseColor;
    }
}
