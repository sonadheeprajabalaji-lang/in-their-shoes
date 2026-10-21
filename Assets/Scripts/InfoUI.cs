using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The Info Scene, shown after Chapter 6: the project title, a note that
// this is a research prototype and not clinical advice, a little on the
// emotional support a birthing partner may need (and encouragement for
// the partner playing), where to read more, and who to call.
//
// Links open in the browser when clicked. Next (Enter) goes to the End
// scene. Builds its UI at runtime like the other menu
// screens, so the scene only needs this component and a camera.
public class InfoUI : MonoBehaviour
{
    public string nextSceneName = MenuUI.EndScene;

    // Seconds before Enter works, so the Enter that left Chapter 6 can't
    // skip straight past this page.
    public float minReadSeconds = 1.5f;

    [TextArea(3, 8)]
    public string prototypeNote =
        "<i>In Their Shoes</i> is a student prototype. Drawing on published research, we set out to help " +
        "non-birthing partners understand each phase of pregnancy and the first year after birth from the " +
        "birthing partner's side. It is not medical advice: for your own situation, please talk to a GP, " +
        "midwife, child and family health nurse or other clinician, and don't rely on the game alone.";

    [TextArea(3, 8)]
    public string supportText =
        "Pregnancy and the first year after birth can be hard on a birthing partner's emotional health, " +
        "not just her body. Steady, everyday support helps: ask how she's really feeling and listen, take on " +
        "jobs without being asked, protect her rest, and encourage her to get help early if low mood or worry " +
        "lasts more than two weeks.";

    [TextArea(1, 3)]
    public string encouragement = "And to you, the partner: noticing is where support starts. You're doing great. Keep going!";

    TMP_Text linksText;
    float shownAt;

    void Start()
    {
        MenuUI.EnsureEventSystem();
        Build();
        shownAt = Time.unscaledTime;
    }

    void Update()
    {
        // Click a link to open it in the browser.
        if (linksText != null && Input.GetMouseButtonDown(0))
        {
            int i = TMP_TextUtilities.FindIntersectingLink(linksText, Input.mousePosition, null);
            if (i >= 0) Application.OpenURL(linksText.textInfo.linkInfo[i].GetLinkID());
        }

        if (Time.unscaledTime - shownAt < minReadSeconds) return;
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) Continue();
    }

    void Continue() => MenuUI.Load(nextSceneName);

    // ---------- UI ----------

    void Build()
    {
        var root = MenuUI.Canvas(transform, "InfoCanvas");

        var page = UIArt.Column(root, "Page", 20, TextAnchor.UpperCenter);
        var prt = (RectTransform)page.transform;
        UIArt.Stretch(prt);
        page.padding = new RectOffset(110, 110, 40, 40);
        page.childForceExpandWidth = false;

        // Title of the project.
        MenuUI.Wordmark(page.transform, 110);
        UIArt.Size(UIArt.Text(page.transform, "Thank you for playing", 28, MenuUI.Mist), 1200, -1);

        // Two columns: about the prototype and supporting her | read more and get help.
        var cols = UIArt.Row(page.transform, "Columns", 64, TextAnchor.UpperCenter);

        var left = UIArt.Column(cols.transform, "Left", 16, TextAnchor.UpperLeft);
        Heading(left.transform, "About this prototype");
        Body(left.transform, prototypeNote, 820);
        UIArt.Size(UIArt.Rect("Gap", left.transform), -1, 10);
        Heading(left.transform, "Supporting her");
        Body(left.transform, supportText, 820);
        var cheer = UIArt.Text(left.transform, encouragement, 30, MenuUI.Rose, TextAlignmentOptions.Left, true);
        UIArt.Size(cheer, 820, -1);

        var right = UIArt.Panel(cols.transform, "Help");
        var rv = right.gameObject.AddComponent<VerticalLayoutGroup>();
        rv.padding = new RectOffset(44, 44, 36, 36);
        rv.spacing = 14;
        rv.childControlWidth = rv.childControlHeight = true;
        rv.childForceExpandWidth = rv.childForceExpandHeight = false;
        UIArt.Size(right, 760, -1);

        Heading(right.transform, "Read more");
        linksText = Body(right.transform,
            "<link=\"https://www.aihw.gov.au/reports/mothers-babies/perinatal-mental-health-screening-australia/contents/what-is-perinatal-mental-health\">" +
            "<u><color=#F4E8D6>What is perinatal mental health?</color></u></link>\nAustralian Institute of Health and Welfare\n\n" +
            "<link=\"https://www.panda.org.au/\"><u><color=#F4E8D6>panda.org.au</color></u></link>\n" +
            "Perinatal Anxiety & Depression Australia",
            672);
        linksText.raycastTarget = true;   // so clicks reach the links
        UIArt.Size(UIArt.Rect("Gap", right.transform), -1, 8);

        Heading(right.transform, "Need to talk to someone?");
        Body(right.transform,
            "<b>PANDA National Helpline</b>  1300 726 306\n<size=85%>Mon–Sat, 9am–7:30pm</size>\n" +
            "<b>NSW Mental Health Line</b>  1800 011 511\n<size=85%>24 hours, 7 days</size>\n" +
            "<b>In an emergency, call 000</b>",
            672);

        MenuUI.Button(page.transform, "Next  (Enter)", 360, Continue);
    }

    static void Heading(Transform parent, string text)
    {
        var t = UIArt.Text(parent, text, 34, MenuUI.Cream, TextAlignmentOptions.Left, true);
        t.enableWordWrapping = false;
    }

    static TMP_Text Body(Transform parent, string text, float width)
    {
        var t = UIArt.Text(parent, text, 26, UIArt.TextColor, TextAlignmentOptions.Left);
        t.richText = true;
        t.lineSpacing = 10;
        UIArt.Size(t, width, -1);
        return t;
    }
}
