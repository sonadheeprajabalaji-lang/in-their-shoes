using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// The pause button in every chapter (top-right corner, or Esc), and the
// menu it opens:
//   Resume (Enter)    back to the chapter, just as it was (Esc works too)
//   Jump to chapter   the ChapterSelect scene
//   Exit game         straight to the Info scene
//
// Pausing sets Time.timeScale to 0, which stops the meters, the hour's
// timer, walking and every hold. Screens that wait for a key (the chapter
// banner, hour transition, recap, learning card) ignore input while
// IsBlocking is true, so keys pressed in the menu don't leak into the game.
//
// GameManager creates one automatically. Builds its own overlay canvas at
// runtime, above every other overlay.
[DefaultExecutionOrder(-500)]   // reads the click before other scripts see it
public class PauseMenuUI : MonoBehaviour
{
    static bool paused;
    static int resumedFrame = -10;

    public static bool IsPaused => paused;

    // True while paused, and for a frame after resuming so the Enter or
    // click that pressed Resume isn't also read by the game.
    public static bool IsBlocking => paused || Time.frameCount - resumedFrame <= 1;

    GameObject menu;
    RectTransform pauseButtonRt;
    GameObject pauseButton;
    float savedTimeScale = 1f;

    void Awake()
    {
        paused = false;
        Build();
        menu.SetActive(false);
    }

    void Update()
    {
        if (!paused)
        {
            bool clicked = Input.GetMouseButtonDown(0) && pauseButton.activeInHierarchy &&
                           RectTransformUtility.RectangleContainsScreenPoint(pauseButtonRt, Input.mousePosition, null);
            if (Input.GetKeyDown(KeyCode.Escape) || clicked) Pause();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            Resume();
    }

    void Pause()
    {
        if (paused) return;
        paused = true;
        savedTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
        Time.timeScale = 0f;
        menu.SetActive(true);
        pauseButton.SetActive(false);
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    void Resume()
    {
        if (!paused) return;
        paused = false;
        resumedFrame = Time.frameCount;
        Time.timeScale = savedTimeScale;
        menu.SetActive(false);
        pauseButton.SetActive(true);
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    void Leave(string scene)
    {
        paused = false;
        Time.timeScale = 1f;
        MenuUI.Load(scene);
    }

    void OnDestroy()
    {
        // Never leave the next scene frozen.
        if (paused) { paused = false; Time.timeScale = 1f; }
    }

    // ---------- UI ----------

    void Build()
    {
        MenuUI.EnsureEventSystem();

        var root = new GameObject("PauseCanvas");
        root.transform.SetParent(transform, false);
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900;   // above every other overlay
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        // Pause button, top-right corner. Its click is read in Update, so
        // it has no onClick of its own.
        var button = MenuUI.Button(root.transform, "Pause  (Esc)", 220, () => { });
        pauseButton = button.gameObject;
        pauseButtonRt = (RectTransform)button.transform;
        pauseButtonRt.anchorMin = pauseButtonRt.anchorMax = pauseButtonRt.pivot = new Vector2(1f, 1f);
        pauseButtonRt.sizeDelta = new Vector2(220, 60);
        pauseButtonRt.anchoredPosition = new Vector2(-24f, -20f);
        var label = button.GetComponentInChildren<TMP_Text>();
        if (label != null) label.fontSize = 24;

        // The menu: a dark cover over the game and a card in the middle.
        menu = UIArt.Rect("PauseMenu", root.transform).gameObject;
        UIArt.Stretch((RectTransform)menu.transform);
        var cover = menu.AddComponent<Image>();
        cover.color = new Color(0.07f, 0.05f, 0.09f, 0.75f);   // also blocks clicks to the game

        var card = UIArt.Panel(menu.transform, "Card");
        var crt = card.rectTransform;
        crt.anchorMin = crt.anchorMax = crt.pivot = new Vector2(0.5f, 0.5f);
        crt.sizeDelta = new Vector2(620, 0);
        var v = card.gameObject.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(70, 70, 50, 56);
        v.spacing = 22;
        v.childAlignment = TextAnchor.UpperCenter;
        v.childControlWidth = v.childControlHeight = true;
        v.childForceExpandWidth = v.childForceExpandHeight = false;
        card.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        UIArt.Text(card.transform, "Paused", 48, UIArt.TitleColor, TextAlignmentOptions.Center, true);
        UIArt.Size(UIArt.Rect("Gap", card.transform), -1, 6);
        MenuUI.Button(card.transform, "Resume  (Enter)", 420, Resume);
        MenuUI.Button(card.transform, "Jump to chapter", 420, () => Leave(MenuUI.ChaptersScene));
        MenuUI.Button(card.transform, "Exit game", 420, () => Leave(MenuUI.InfoScene));
    }
}
