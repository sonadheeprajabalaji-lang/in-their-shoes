using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// The Title Scene: the In Their Shoes logo builds itself, then
// "Press Enter to begin" (or a click) loads the Warning Scene.
//   0.4s  filled print (her) steps in
//   1.0s  outlined print (you) fills up from the heel
//   1.6s  stitching fades in
//   1.9s  wordmark rises in, then "A walk through her day" fades in
//   3.8s  prompt fades in, then pulses
// Enter (or a click) during the animation jumps to the finished frame; the
// next one continues, so nobody skips the content note by accident.
public class TitleScreenUI : MonoBehaviour
{
    public string nextSceneName = MenuUI.WarningScene;

    CanvasGroup partner, stitch, word, tagline, prompt;
    Image you;
    RectTransform partnerRt, wordRt;
    Vector2 wordHome;
    bool finished;
    float shownAt;

    const float MarkHeight = 300f;

    void Start()
    {
        MenuUI.EnsureEventSystem();
        CarryoverState.Reset();
        Build();
        shownAt = Time.unscaledTime;
        StartCoroutine(Play());
    }

    void Update()
    {
        if (Time.unscaledTime - shownAt < 0.3f) return;
        // Enter moves the game forward everywhere; a click works too.
        if (!(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0))) return;

        if (!finished) Finish();
        else MenuUI.Load(nextSceneName);
    }

    void Build()
    {
        var root = MenuUI.Canvas(transform, "TitleCanvas");

        // The mark: three same-size layers stacked on one spot.
        var mark = UIArt.Rect("Mark", root);
        mark.anchorMin = mark.anchorMax = new Vector2(0.5f, 0.5f);
        mark.sizeDelta = new Vector2(MarkHeight, MarkHeight);
        mark.anchoredPosition = new Vector2(0, 150);

        partner = Layer(mark, "mark-layer-partner", out partnerRt, out _);
        Layer(mark, "mark-layer-you", out _, out you);
        you.type = Image.Type.Filled;
        you.fillMethod = Image.FillMethod.Vertical;
        you.fillOrigin = (int)Image.OriginVertical.Bottom;
        stitch = Layer(mark, "mark-layer-stitch", out _, out _);

        var wordImg = MenuUI.LogoImage(root, "wordmark-dark", 820);
        wordRt = wordImg.rectTransform;
        wordRt.anchorMin = wordRt.anchorMax = new Vector2(0.5f, 0.5f);
        wordHome = wordRt.anchoredPosition = new Vector2(0, -75);
        word = wordImg.gameObject.AddComponent<CanvasGroup>();

        tagline = MenuUI.Tagline(root, "A walk through her day", wordRt).gameObject.AddComponent<CanvasGroup>();

        var p = UIArt.Text(root, "PRESS ENTER TO BEGIN", 26, MenuUI.Mist, TMPro.TextAlignmentOptions.Center, true);
        p.characterSpacing = 18;
        var prt = (RectTransform)p.transform;
        prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0f);
        prt.sizeDelta = new Vector2(1000, 60);
        prt.anchoredPosition = new Vector2(0, 120);
        prompt = p.gameObject.AddComponent<CanvasGroup>();

        partner.alpha = stitch.alpha = word.alpha = tagline.alpha = prompt.alpha = 0f;
        you.fillAmount = 0f;
    }

    CanvasGroup Layer(RectTransform parent, string sprite, out RectTransform rt, out Image img)
    {
        rt = UIArt.Rect(sprite, parent);
        UIArt.Stretch(rt);
        img = rt.gameObject.AddComponent<Image>();
        img.sprite = MenuUI.Logo(sprite);
        img.preserveAspect = true;
        img.raycastTarget = false;
        return rt.gameObject.AddComponent<CanvasGroup>();
    }

    IEnumerator Play()
    {
        yield return Wait(0.4f);
        yield return Tween(0.6f, t =>
        {
            float e = EaseOut(t);
            partner.alpha = e;
            partnerRt.localScale = Vector3.one * Mathf.Lerp(0.92f, 1f, e);
            partnerRt.anchoredPosition = new Vector2(0, Mathf.Lerp(-18f, 0f, e));
        });
        yield return Tween(0.8f, t => you.fillAmount = EaseInOut(t));
        yield return Tween(0.4f, t => stitch.alpha = t);
        yield return Wait(0.3f);
        yield return Tween(0.7f, t =>
        {
            float e = EaseOut(t);
            word.alpha = e;
            wordRt.anchoredPosition = wordHome + new Vector2(0, Mathf.Lerp(-20f, 0f, e));
        });
        yield return Tween(0.6f, t => tagline.alpha = EaseOut(t));
        yield return Wait(0.3f);
        yield return Tween(0.6f, t => prompt.alpha = t);
        finished = true;
        StartCoroutine(Pulse());
    }

    void Finish()
    {
        StopAllCoroutines();
        partner.alpha = stitch.alpha = word.alpha = tagline.alpha = prompt.alpha = 1f;
        partnerRt.localScale = Vector3.one;
        partnerRt.anchoredPosition = Vector2.zero;
        you.fillAmount = 1f;
        wordRt.anchoredPosition = wordHome;
        finished = true;
        StartCoroutine(Pulse());
    }

    IEnumerator Pulse()
    {
        float start = Time.unscaledTime;
        while (true)
        {
            float phase = (Time.unscaledTime - start) / 2.4f * Mathf.PI * 2f;
            prompt.alpha = Mathf.Lerp(0.45f, 1f, (Mathf.Cos(phase) + 1f) * 0.5f);
            yield return null;
        }
    }

    static IEnumerator Wait(float seconds)
    {
        float end = Time.unscaledTime + seconds;
        while (Time.unscaledTime < end) yield return null;
    }

    static IEnumerator Tween(float seconds, System.Action<float> step)
    {
        float start = Time.unscaledTime;
        float t;
        do
        {
            t = Mathf.Clamp01((Time.unscaledTime - start) / seconds);
            step(t);
            yield return null;
        } while (t < 1f);
    }

    static float EaseOut(float t) => 1f - (1f - t) * (1f - t) * (1f - t);
    static float EaseInOut(float t) => t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) * 0.5f;
}
