using UnityEngine;

// The End Scene, after the Info scene: the In Their Shoes logo, shown
// whole, "Now walk beside her" under it, and one button, "Play again (Enter)", which goes back to the
// chapter list to start again from the beginning. Builds its UI at runtime
// like the other menu screens.
public class EndUI : MonoBehaviour
{
    public string nextSceneName = MenuUI.ChaptersScene;

    // Seconds before Enter works, so the Enter that left the Info scene
    // can't skip straight past this one.
    public float minShowSeconds = 1f;

    const float MarkHeight = 300f;
    float shownAt;

    void Start()
    {
        MenuUI.EnsureEventSystem();
        Build();
        shownAt = Time.unscaledTime;
    }

    void Update()
    {
        if (Time.unscaledTime - shownAt < minShowSeconds) return;
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) PlayAgain();
    }

    void PlayAgain()
    {
        CarryoverState.Reset();   // a new playthrough starts with no Self Hour carry-over
        MenuUI.Load(nextSceneName);
    }

    void Build()
    {
        var root = MenuUI.Canvas(transform, "EndCanvas");

        // The mark: the same three layers as the Title, stacked and finished.
        var mark = UIArt.Rect("Mark", root);
        mark.anchorMin = mark.anchorMax = new Vector2(0.5f, 0.5f);
        mark.sizeDelta = new Vector2(MarkHeight, MarkHeight);
        mark.anchoredPosition = new Vector2(0, 170);
        foreach (var layer in new[] { "mark-layer-partner", "mark-layer-you", "mark-layer-stitch" })
        {
            var rt = UIArt.Rect(layer, mark);
            UIArt.Stretch(rt);
            var img = rt.gameObject.AddComponent<UnityEngine.UI.Image>();
            img.sprite = MenuUI.Logo(layer);
            img.preserveAspect = true;
            img.raycastTarget = false;
        }

        var word = MenuUI.LogoImage(root, "wordmark-dark", 820).rectTransform;
        word.anchorMin = word.anchorMax = new Vector2(0.5f, 0.5f);
        word.anchoredPosition = new Vector2(0, -55);
        MenuUI.Tagline(root, "Now walk beside her", word);

        var button = MenuUI.Button(root, "Play again  (Enter)", 420, PlayAgain);
        var brt = (RectTransform)button.transform;
        brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0f);
        brt.sizeDelta = new Vector2(420, 76);
        brt.anchoredPosition = new Vector2(0, 170);
    }
}
