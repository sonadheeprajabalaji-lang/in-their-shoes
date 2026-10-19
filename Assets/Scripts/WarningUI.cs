using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The Warning Scene: who the game is for, what it shows (and doesn't),
// and where to get help. Shown once, after the Title Scene.
// Continue (Enter) goes to the Chapters list; Stop here quits.
public class WarningUI : MonoBehaviour
{
    public string nextSceneName = MenuUI.ChaptersScene;

    // Seconds before Enter works, so the page can't be skipped by a key
    // still held from the Title Scene.
    public float minReadSeconds = 1.5f;

    [TextArea(3, 10)]
    public string body =
        "<i>In Their Shoes</i> is designed for non-birthing partners, dads, same-sex partners, and anyone supporting " +
        "someone through pregnancy or the first year after birth. You'll spend part of this day as your partner, " +
        "and part of it as yourself.\n\n" +
        "It includes exhaustion, physical pain, isolation and low mood. Nothing is graphic. It does not depict " +
        "pregnancy loss, stillbirth, or harm to an infant.\n\n" +
        "There's no score and no correct path. This isn't a test of whether you're a good partner.";

    [TextArea(2, 6)]
    public string help =
        "You can stop any time. If something here is already happening in your house:\n" +
        "<b>PANDA</b>  1300 726 306, Mon–Sat 9am–7:30pm\n" +
        "<b>Lifeline</b>  13 11 14, any time";

    float shownAt;

    void Start()
    {
        MenuUI.EnsureEventSystem();
        Build();
        shownAt = Time.unscaledTime;
    }

    void Update()
    {
        if (Time.unscaledTime - shownAt < minReadSeconds) return;
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) Continue();
    }

    void Continue() => MenuUI.Load(nextSceneName);

    void Build()
    {
        var root = MenuUI.Canvas(transform, "WarningCanvas");

        var col = UIArt.Column(root, "Page", 34, TextAnchor.MiddleCenter);
        var crt = (RectTransform)col.transform;
        crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f);
        crt.sizeDelta = new Vector2(1240, 980);

        var heading = UIArt.Text(col.transform, "Before you start", 54, MenuUI.Cream, TextAlignmentOptions.Center, true);
        UIArt.Size(heading, 1240, -1);

        var text = UIArt.Text(col.transform, body, 32, UIArt.TextColor, TextAlignmentOptions.Left);
        text.richText = true;
        text.lineSpacing = 18;
        UIArt.Size(text, 1180, -1);

        // Help lines in their own panel so they stand out.
        var panel = UIArt.Panel(col.transform, "Help");
        var prow = panel.gameObject.AddComponent<VerticalLayoutGroup>();
        prow.padding = new RectOffset(40, 40, 28, 28);
        prow.childControlWidth = prow.childControlHeight = true;
        prow.childForceExpandWidth = prow.childForceExpandHeight = false;
        var helpText = UIArt.Text(panel.transform, help, 30, MenuUI.Cream, TextAlignmentOptions.Left);
        helpText.richText = true;
        helpText.lineSpacing = 14;
        UIArt.Size(helpText, 1100, -1);

        var buttons = UIArt.Row(col.transform, "Buttons", 30);
        MenuUI.Button(buttons.transform, "Stop here", 300, MenuUI.Quit);
        // Not pre-selected: Enter is handled in Update, after minReadSeconds.
        MenuUI.Button(buttons.transform, "Continue  (Enter)", 420, Continue);
    }
}
