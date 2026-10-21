using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// The Title Scene: the In Their Shoes logo builds itself, then the
// "Begin (Enter)" button loads the Warning Scene.
//   0.4s  filled print (her) steps in
//   1.0s  outlined print (you) fills up from the heel
//   1.6s  stitching fades in
//   1.9s  wordmark rises in, then "A walk through her day" fades in
//   3.8s  Begin (Enter) button fades in
// Enter (or the button) during the animation jumps to the finished frame;
// the next press continues, so nobody skips the content note by accident.
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
        // Enter moves the game forward everywhere; the button takes clicks.
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) OnBegin();
    }

    // First press during the animation shows the finished logo; the next
    // one continues to the Warning scene.
    void OnBegin()
    {
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

        wordRt = MenuUI.Wordmark(root, 115);
        wordRt.anchorMin = wordRt.anchorMax = new Vector2(0.5f, 0.5f);
        wordHome = wordRt.anchoredPosition = new Vector2(0, -75);
        word = wordRt.gameObject.AddComponent<CanvasGroup>();

        tagline = MenuUI.Tagline(root, "A walk through her day", wordRt).gameObject.AddComponent<CanvasGroup>();

        // Same button as the End scene's Play again.
        var begin = MenuUI.Button(root, "Begin  (Enter)", 420, OnBegin);
        var brt = (RectTransform)begin.transform;
        brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0f);
        brt.sizeDelta = new Vector2(420, 76);
        brt.anchoredPosition = new Vector2(0, 170);
        prompt = begin.gameObject.AddComponent<CanvasGroup>();

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
