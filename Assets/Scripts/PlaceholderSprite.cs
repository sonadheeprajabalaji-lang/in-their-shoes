using UnityEngine;

// Generates a plain white square sprite at runtime so movement and
// interaction can be built and tested before real art exists.
// Once real sprites are ready, just assign them in the Inspector on the
// SpriteRenderer instead — this is only used as a fallback when no sprite
// is already assigned.
public static class PlaceholderSprite
{
    static Sprite cached;

    public static Sprite Square()
    {
        if (cached != null) return cached;

        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        cached = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return cached;
    }
}
