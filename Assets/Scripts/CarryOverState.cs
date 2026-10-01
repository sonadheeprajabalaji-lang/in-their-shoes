// Holds meter eases earned from one chapter's Self Hour responses, to be
// applied at the very start of the NEXT chapter's Partner Hour, then
// cleared. Deliberately does not accumulate beyond one chapter: call
// ConsumeForNextChapter() once per chapter start, which both reads and
// clears it.
//
// This is a plain static class, not a MonoBehaviour or a ScriptableObject
// asset: it only needs to survive between GameManager.Start() calls
// within the same play session, not between separate game launches.
public static class CarryoverState
{
    static MeterRates pending = new MeterRates(1f, 1f, 1f, 1f);

    // Called by GameManager once per Self Hour response.
    // Combines multiplicatively, so several responses in one chapter
    // compound rather than overwrite each other.
    public static void AddEase(MeterRates ease)
    {
        pending = MeterRates.Multiply(pending, ease);
    }

    // Called once, at the start of a chapter's Partner Hour. Returns
    // whatever was earned last chapter, then clears it, so it never
    // reaches past the very next chapter.
    public static MeterRates ConsumeForNextChapter()
    {
        MeterRates result = pending;
        pending = new MeterRates(1f, 1f, 1f, 1f);
        return result;
    }

    // Call this when starting a fresh playthrough from Chapter 1, so a
    // previous session's leftover carryover can't leak in.
    public static void Reset()
    {
        pending = new MeterRates(1f, 1f, 1f, 1f);
    }
}