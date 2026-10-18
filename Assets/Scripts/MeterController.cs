using UnityEngine;
using UnityEngine.UI;

// Drains the four Partner Hour meters two ways:
// 1. Continuously, every frame, at a rate set by ChapterData and shaped
//    by whatever choice was made at the main task (ApplyChoice).
// 2. In one-off steps, whenever a BAU task (dishes, stove, etc) is
//    completed (ApplyCost).
// No numbers are shown to the player, only the bars.
public class MeterController : MonoBehaviour
{
    [Header("Drag the four sliders here")]
    public Slider body;
    public Slider mind;
    public Slider feelingLikeAPerson;
    public Slider mask;

    // True once any meter has hit zero. GameManager polls this to know
    // when Partner Hour is over.
    public bool IsDepleted { get; private set; }

    [Header("Pacing (playtest: the meters ran out before players learned the chapter)")]
    [Tooltip("Scales every chapter's drain. 1 = the speeds in ChapterData; 0.6 = 40% slower.")]
    public float drainScale = 0.6f;
    [Tooltip("The drain starts at zero and builds up to full speed over this many seconds, so players can read the scene and find the main task first.")]
    public float rampUpSeconds = 15f;
    [Tooltip("After the main task's big hit, the drain eases back in from zero over this many seconds: she catches her breath, so the hit doesn't end the hour straight away.")]
    public float afterHitRampSeconds = 12f;
    [Tooltip("Scales what each chore costs (InteractableTask.cost). 1 = the costs set on each task.")]
    public float choreCostScale = 0.5f;
    float drainingFor;
    float activeRamp;

    MeterRates baseDrain;
    MeterRates currentMultiplier = new MeterRates(1f, 1f, 1f, 1f);
    bool draining = false;
    public bool IsDraining => draining;

    // Call this once, at the start of the chapter, before StartDraining.
    public void Configure(MeterRates chapterBaseDrain)
    {
        baseDrain = chapterBaseDrain;
    }

    // Call this when the Partner Hour starts
    public void StartDraining()
    {
        ResetMeters();
        currentMultiplier = new MeterRates(1f, 1f, 1f, 1f);
        collapsing = null;
        drainingFor = 0f;
        activeRamp = rampUpSeconds;
        draining = true;
    }

    // Call this when the Partner Hour ends
    public void StopDraining()
    {
        draining = false;
        collapsing = null;
    }

    public void ResetMeters()
    {
        body.value = 1f;
        mind.value = 1f;
        feelingLikeAPerson.value = 1f;
        mask.value = 1f;
        IsDepleted = false;
    }

    // Applies the chosen PartnerChoice's multipliers to the continuous
    // drain rate for the rest of Partner Hour.
    public void ApplyChoice(PartnerChoice choice)
    {
        currentMultiplier = choice.multipliers;
    }

    // ---------- Hitting her limit ----------

    // Raised when the lowest meter starts running out because she can't
    // take on any more (see CollapseLowest).
    public event System.Action<HerMeter> Collapsing;

    Slider collapsing;
    [Tooltip("Seconds the lowest meter takes to run out once she hits her limit.")]
    public float collapseSeconds = 1.2f;
    float collapseRate;

    // Runs her lowest meter down to empty over collapseSeconds, which ends
    // the Partner Hour the normal way. Used so the last chores always stay
    // undone for the Self Hour.
    public void CollapseLowest()
    {
        if (collapsing != null) return;
        HerMeter lowest = HerMeter.Body;
        foreach (HerMeter m in System.Enum.GetValues(typeof(HerMeter)))
        {
            if (Get(m) != null && Get(m).value < Get(lowest).value) lowest = m;
        }
        collapsing = Get(lowest);
        collapseRate = collapsing.value / Mathf.Max(0.1f, collapseSeconds);
        Collapsing?.Invoke(lowest);
    }

    public bool IsCollapsing => collapsing != null;

    // Raised when a big one-off hit lands (the main task), with how much
    // each meter actually dropped, so MeterEffects can make it visible.
    public event System.Action<MeterRates> HitTaken;

    public Slider Get(HerMeter m) => m switch
    {
        HerMeter.Body => body,
        HerMeter.Mind => mind,
        HerMeter.FeelingLikeAPerson => feelingLikeAPerson,
        _ => mask,
    };

    // Cuts one meter by a fraction of what's left (0.5 = halves it): the
    // main task's big hit. A fraction can never empty it.
    public void ApplyMainTaskHit(HerMeter meter, float fraction)
    {
        var s = Get(meter);
        if (s == null) return;
        float amount = s.value * Mathf.Clamp01(fraction);
        var hit = new MeterRates(0f, 0f, 0f, 0f);
        switch (meter)
        {
            case HerMeter.Body: hit.body = amount; break;
            case HerMeter.Mind: hit.mind = amount; break;
            case HerMeter.FeelingLikeAPerson: hit.feelingLikeAPerson = amount; break;
            default: hit.mask = amount; break;
        }
        ApplyHit(hit, 0f);
    }

    // A big one-off hit that never takes a meter below floor (and leaves a
    // meter already under the floor alone). Used when the main task is done.
    public void ApplyHit(MeterRates hit, float floor)
    {
        var dropped = new MeterRates(
            HitOne(body, hit.body, floor),
            HitOne(mind, hit.mind, floor),
            HitOne(feelingLikeAPerson, hit.feelingLikeAPerson, floor),
            HitOne(mask, hit.mask, floor));
        RecomputeDepleted();
        // Ease the drain back in so she has a moment after the hit.
        drainingFor = 0f;
        activeRamp = afterHitRampSeconds;
        HitTaken?.Invoke(dropped);
    }

    static float HitOne(Slider s, float amount, float floor)
    {
        if (s == null || amount <= 0f || s.value <= floor) return 0f;
        float before = s.value;
        s.value = Mathf.Max(floor, before - amount);
        return before - s.value;
    }

    // Applies a one-off reduction, e.g. from completing a BAU task.
    // Clamped at 0, same as the continuous drain.
    public void ApplyCost(MeterRates cost, float scale = 1f)
    {
        body.value = Mathf.Max(0f, body.value - cost.body * scale);
        mind.value = Mathf.Max(0f, mind.value - cost.mind * scale);
        feelingLikeAPerson.value = Mathf.Max(0f, feelingLikeAPerson.value - cost.feelingLikeAPerson * scale);
        mask.value = Mathf.Max(0f, mask.value - cost.mask * scale);
        RecomputeDepleted();
    }

    void Update()
    {
        if (!draining) return;

        if (collapsing != null)
        {
            collapsing.value = Mathf.Max(0f, collapsing.value - collapseRate * Time.deltaTime);
            RecomputeDepleted();
            return;
        }

        drainingFor += Time.deltaTime;
        float ramp = activeRamp > 0f ? Mathf.Clamp01(drainingFor / activeRamp) : 1f;
        float dt = Time.deltaTime * drainScale * ramp;
        body.value -= baseDrain.body * currentMultiplier.body * dt;
        mind.value -= baseDrain.mind * currentMultiplier.mind * dt;
        feelingLikeAPerson.value -= baseDrain.feelingLikeAPerson * currentMultiplier.feelingLikeAPerson * dt;
        mask.value -= baseDrain.mask * currentMultiplier.mask * dt;

        RecomputeDepleted();
    }

    void RecomputeDepleted()
    {
        IsDepleted = body.value <= 0f || mind.value <= 0f
                  || feelingLikeAPerson.value <= 0f || mask.value <= 0f;
    }
}