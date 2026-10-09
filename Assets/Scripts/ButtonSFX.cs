using UnityEngine;
using UnityEngine.UI;

// Attach to any Button to play a sound through AudioManager when it's
// clicked. Add one per button you want sound on; leave Click Clip empty
// and it's a safe no-op until you assign a real clip.
[RequireComponent(typeof(Button))]
public class ButtonSFX : MonoBehaviour
{
    public AudioClip clickClip;
    [Range(0f, 1f)] public float volumeScale = 1f;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(PlayClick);
    }

    void PlayClick()
    {
        AudioManager.Get().PlaySFX(clickClip, volumeScale);
    }
}