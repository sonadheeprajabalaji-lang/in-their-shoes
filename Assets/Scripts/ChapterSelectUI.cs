using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// The Chapters list Scene: every chapter by name and stage, with
// "Start from the beginning" (tutorial, then Chapter 1) or a skip
// straight to any chapter. Number keys 1-6 pick a chapter; Enter starts
// from the beginning.
//
// Skipping in starts with no Self Hour carry-over, since the chapters
// before it weren't played.
public class ChapterSelectUI : MonoBehaviour
{
    [Tooltip("In order. Scene for chapter N is \"Chapter\" + N.")]
    public ChapterData[] chapters;

    float shownAt;

    void Start()
    {
        MenuUI.EnsureEventSystem();
        Build();
        shownAt = Time.unscaledTime;
    }

    void Update()
    {
        if (Time.unscaledTime - shownAt < 0.3f) return;
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            // Enter on a focused button is handled by the button itself.
            if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null) StartFromBeginning();
            return;
        }
        for (int i = 0; i < chapters.Length && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i)) PlayChapter(i + 1);
        }
    }

    void StartFromBeginning()
    {
        CarryoverState.Reset();
        MenuUI.Load(MenuUI.TutorialScene);
    }

    void PlayChapter(int number)
    {
        CarryoverState.Reset();
        MenuUI.Load("Chapter" + number);
    }

    void Build()
    {
        var root = MenuUI.Canvas(transform, "ChaptersCanvas");

        var page = UIArt.Column(root, "Page", 26, TextAnchor.UpperCenter);
        var prt = (RectTransform)page.transform;
        UIArt.Stretch(prt);
        page.padding = new RectOffset(0, 0, 70, 60);

        var title = UIArt.Text(page.transform, "Chapters", 56, MenuUI.Cream, TextAlignmentOptions.Center, true);
        UIArt.Size(title, 1200, -1);
        var sub = UIArt.Text(page.transform,
            "Play from the start, or go straight to a stage. Each chapter is one day: an hour as your partner, then an hour as you.",
            28, MenuUI.Mist);
        UIArt.Size(sub, 1200, -1);

        var startButton = MenuUI.Button(page.transform, "Start from the beginning  (Enter)", 640, StartFromBeginning);

        var grid = UIArt.Rect("Chapters", page.transform);
        var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(760, 150);
        layout.spacing = new Vector2(28, 22);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 2;
        layout.childAlignment = TextAnchor.UpperCenter;
        int rows = Mathf.CeilToInt((chapters?.Length ?? 0) / 2f);
        UIArt.Size(grid, 760 * 2 + 28, rows * 150 + Mathf.Max(0, rows - 1) * 22);

        if (chapters != null)
        {
            for (int i = 0; i < chapters.Length; i++) ChapterCard(grid, i + 1, chapters[i]);
        }

        var hint = UIArt.Text(page.transform, "Press 1–6 to jump to a chapter", 24, MenuUI.Mist);
        UIArt.Size(hint, 800, -1);

        EventSystem.current?.SetSelectedGameObject(startButton.gameObject);
    }

    void ChapterCard(Transform parent, int number, ChapterData data)
    {
        var img = UIArt.Panel(parent, "Chapter" + number);
        var button = img.gameObject.AddComponent<Button>();
        button.onClick.AddListener(() => PlayChapter(number));
        img.gameObject.AddComponent<ButtonSFX>();
        var colors = button.colors;
        colors.highlightedColor = new Color(1f, 0.9f, 0.88f);
        colors.selectedColor = new Color(1f, 0.82f, 0.78f);
        button.colors = colors;

        var row = img.gameObject.AddComponent<HorizontalLayoutGroup>();
        row.padding = new RectOffset(32, 32, 20, 20);
        row.spacing = 28;
        row.childAlignment = TextAnchor.MiddleLeft;
        row.childControlWidth = row.childControlHeight = true;
        row.childForceExpandWidth = row.childForceExpandHeight = false;

        var num = UIArt.Text(img.transform, number.ToString(), 64, MenuUI.Rose, TextAlignmentOptions.Center, true);
        UIArt.Size(num, 70, -1);

        var text = UIArt.Column(img.transform, "Text", 6, TextAnchor.MiddleLeft);
        string title = data != null && !string.IsNullOrWhiteSpace(data.chapterTitle) ? data.chapterTitle : "Chapter " + number;
        string stage = data != null ? data.weekContext : "";
        UIArt.Size(UIArt.Text(text.transform, title, 34, MenuUI.Cream, TextAlignmentOptions.Left, true), 590, -1);
        if (!string.IsNullOrWhiteSpace(stage))
            UIArt.Size(UIArt.Text(text.transform, stage, 26, MenuUI.Mist, TextAlignmentOptions.Left), 590, -1);
    }
}
