namespace Social_Anxiety;

public enum BreathingPhase { Inhale, Hold, Exhale }

public sealed class BreathingSession(TimeProvider? clock = null)
{
    private readonly TimeProvider time = clock ?? TimeProvider.System;
    private TimeSpan accumulated;
    private long started;
    public bool SkipHold { get; set; }
    public bool IsRunning { get; private set; }
    public bool HasStarted { get; private set; }
    public TimeSpan Elapsed => accumulated + (IsRunning ? time.GetElapsedTime(started) : TimeSpan.Zero);
    private double Position => Elapsed.TotalSeconds % (SkipHold ? 10 : 12);
    public BreathingPhase Phase => Position < 4 ? BreathingPhase.Inhale
        : !SkipHold && Position < 6 ? BreathingPhase.Hold : BreathingPhase.Exhale;
    private double PhaseElapsed => Phase switch
    {
        BreathingPhase.Inhale => Position,
        BreathingPhase.Hold => Position - 4,
        _ => Position - (SkipHold ? 4 : 6)
    };
    private int Duration => Phase switch { BreathingPhase.Inhale => 4, BreathingPhase.Hold => 2, _ => 6 };
    public int SecondsRemaining => Math.Max(1, (int)Math.Ceiling(Duration - PhaseElapsed));
    public double Scale
    {
        get
        {
            var eased = (1 - Math.Cos(Math.PI * Math.Clamp(PhaseElapsed / Duration, 0, 1))) / 2;
            return Phase switch { BreathingPhase.Inhale => 0.64 + 0.36 * eased, BreathingPhase.Hold => 1, _ => 1 - 0.36 * eased };
        }
    }

    public void Start()
    {
        if (IsRunning) return;
        started = time.GetTimestamp();
        IsRunning = HasStarted = true;
    }

    public void Pause()
    {
        if (!IsRunning) return;
        accumulated = Elapsed;
        IsRunning = false;
    }

    public void Reset()
    {
        IsRunning = HasStarted = false;
        accumulated = TimeSpan.Zero;
    }
}
